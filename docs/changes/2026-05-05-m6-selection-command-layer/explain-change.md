# M6 Selection And Command Layer Change Explanation

## Scope

M6 adds the V1 command and selection foundation:

- Explorer-style selection state for single selection, Ctrl toggle, Shift range, Ctrl+A, Escape, and arrow focus movement.
- A Core built-in command registry for the V1 context-menu verbs.
- Keyboard routing for Enter, F2, Delete, Shift+Delete, F5, Backspace, Ctrl+Shift+C, and Ctrl+Shift+N.
- Clipboard formatting for Copy path and Copy name through a Core `IClipboardTextWriter` boundary.
- A Windows clipboard writer that uses Unicode text on the native clipboard.
- WinUI shell context-menu items and keyboard accelerators that route into `AppShellViewModel`.
- A bound WinUI file list whose selected visible rows map to canonical `ListedFileItem` values.
- An app-shell focus-context bridge so file-command accelerators are suppressed while text input owns focus.
- A production listing feed from the active tab path into `AppShellViewModel.FileItems`.

The implementation deliberately does not add OS Shell extension context-menu hosting. That remains out of V1 scope after the earlier accepted product decision.

## Test-First Evidence

M6 tests were added before the production command and selection namespaces existed:

- `tests/VeloFile.Core.Tests/Selection/FileSelectionControllerTests.cs`
- `tests/VeloFile.Core.Tests/Commands/BuiltInCommandRegistryTests.cs`
- `tests/VeloFile.Core.Tests/Commands/KeyboardCommandRouterTests.cs`
- `tests/VeloFile.Core.Tests/Commands/ClipboardCommandServiceTests.cs`
- `tests/VeloFile.Windows.Tests/Clipboard/WindowsClipboardTextWriterTests.cs`
- `tests/VeloFile.App.Tests/AppShellContractTests.cs`
- `tests/VeloFile.App.Tests/AppShellCommandRouteTests.cs`

The first focused runs failed for the expected reason: `VeloFile.Core.Selection`, `VeloFile.Core.Commands`, and `VeloFile.Windows.Clipboard` did not exist. The first attempt also exposed that parallel `dotnet test` invocations contend on shared `obj` outputs, matching the M5 validation note; validation was run sequentially after that.

## Design Choices

`FileSelectionController` is independent of WinUI. It owns focused index, range anchor, and selected indices over `ListedFileItem` values so later virtualized file-list work can reuse the same behavior outside UI tests.

`BuiltInCommandRegistry` is explicit and VeloFile-owned. It returns only built-in command definitions and reports that it does not enumerate Shell extensions, satisfying R49 while leaving destructive operation implementation to later milestones.

`KeyboardCommandRouter` treats text-input focus as a separate context. File commands such as Delete and Ctrl+Shift+C are suppressed when text input owns focus so shortcuts do not accidentally become file operations while the user edits a path.

`ClipboardCommandService` formats paths and names as data, one selected item per line. The Windows adapter owns native clipboard interop; Core tests use a collecting writer so validation does not depend on the real desktop clipboard.

`MainWindow` keeps code-behind thin. XAML buttons, menu items, and keyboard accelerators translate into `AppShellViewModel` calls; the command registry and clipboard formatting live outside the UI layer.

`FileListSurface` is bound to `AppShellViewModel.FileItems`; static placeholder rows are not part of the production file list. The shell selection route uses `FileListSelectionMapper` to turn selected rows, wrappers, or item containers back into `ListedFileItem` values before commands run.

`AppShellViewModel` starts folder listing through `FolderListingCoordinator` for the active tab on startup, accepted navigation, refresh, tab switching, and visibility-setting changes. The WinUI shell only binds and forwards selection; it does not enumerate folders.

Selection mapping is ordered by the current visible `FileItems` collection. This keeps Copy path/name output aligned with the current sorted or filtered file-list order and ignores stale selected rows that are no longer visible.

`AppFileCommandAcceleratorRouter` asks `IKeyboardFocusContextProvider` for the current WinUI focus context before routing file commands. Text-input focus is passed into the Core `KeyboardCommandRouter`, and suppressed routes do not mark the accelerator handled as a file command.

WinUI's XAML key name for Backspace is `Back` because `KeyboardAccelerator.Key` uses `Windows.System.VirtualKey`. The app handler still routes it to the V1 Backspace parent-folder command.

## Validation

Focused validation passed with:

- `dotnet test VeloFile.sln -c Debug --filter Selection`
- `dotnet test VeloFile.sln -c Debug --filter Commands`
- `dotnet test VeloFile.sln -c Debug --filter Clipboard`
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug`

Build validation passed with:

- `dotnet build VeloFile.sln -c Debug`

Final milestone closeout passed with:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`

The latest CI run built with 0 warnings and 0 errors and passed 141 tests across 4 test assemblies. During validation, an existing drive-hint stale-generation test was corrected to avoid using a concurrency cap that contradicted the live-underlying-read cap; the focused test and full CI both pass after that setup fix.

## Deferred By Plan

- Actual copy/move/delete/rename file-operation execution and permanent-delete confirmation: M8-M10.
- Open terminal target discovery and launch execution: M14.
- Search/filter and preview selection consumers: M7 and M11-M13.
