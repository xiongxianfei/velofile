# M6 Review Resolution

## First-Pass Finding

### Major: WinUI context menu bypasses command availability

Disposition: fixed.

Resolution:

- Added `Opening="BuiltInFileContextMenu_Opening"` to the WinUI built-in menu.
- Named each menu item so code-behind can refresh enabled state.
- Added `AppShellViewModel.IsBuiltInCommandAvailable`, backed by the Core `BuiltInCommandRegistry`.
- Updated menu click handlers to execute only when the command is available in the current context.
- Added App shell contract assertions that the menu opening route and `ViewModel.IsBuiltInCommandAvailable` are wired.

The production shell currently reports paste availability as false because file paste execution is not implemented until the file-operation milestones. The Core registry still supports Paste when a future clipboard/file-operation boundary can prove applicability.

## Third-Pass Findings

### Blocker: Visible file list selection does not expose real file models

Disposition: fixed.

Resolution:

- Removed the production static `ListViewItem` placeholder rows from `FileListSurface`.
- Bound `FileListSurface.ItemsSource` to `AppShellViewModel.FileItems`.
- Added a real item template over `ListedFileItem` display fields.
- Added `FileListSelectionMapper` so direct `ListedFileItem` rows, row wrappers, and selected containers with `DataContext = ListedFileItem` all map to canonical file models.
- Updated the WinUI selection handler to update `AppShellViewModel.SelectedFileItems` from mapped file models.
- Added app tests for copy path/name from selected `ListedFileItem` values and selection mapping across direct, wrapped, and container-backed rows.

### Blocker: WinUI file-command accelerators ignore text-input focus

Disposition: fixed.

Resolution:

- Added an app-layer `IKeyboardFocusContextProvider` and WinUI implementation that detects focused text-input controls.
- Added `AppFileCommandAcceleratorRouter` so file-command accelerators pass the current focus context into `AppShellViewModel.HandleFileListShortcut`.
- Updated file-list accelerators to mark `args.Handled` only when the Core router returns a routed command or selection action.
- Kept text-input suppression observable through the Core `KeyboardCommandRouter`; suppressed file commands no longer copy, rename, delete, clear selection, or consume the accelerator route.
- Added app tests proving copy path is suppressed when text input has focus and still works when file-list focus is active.

## Validation

- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` first failed for missing app-shell input bridge types, then passed: 16 App shell contract/route tests.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests and 3 App command-route tests.
- `dotnet test VeloFile.sln -c Debug --filter Selection` passed: 8 Core selection/navigation-category tests and 2 App selection tests.
- `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter Slow_hint_completion_updates_only_matching_generation` first exposed that the existing stale-generation test used a concurrency cap that conflicted with the live-underlying-read cap; the test setup was corrected and the focused test passed.
- `dotnet test VeloFile.sln -c Debug` passed: 134 tests across 4 test assemblies.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 134 tests across 4 test assemblies.
