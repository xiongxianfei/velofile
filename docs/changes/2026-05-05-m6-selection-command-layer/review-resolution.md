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

## Fourth-Pass Findings

### Blocker: Production file list has no real data feed

Disposition: fixed.

Resolution:

- Added production composition from `AppCompositionRoot` to `FolderListingCoordinator` and `WindowsFolderEntrySource`.
- Updated `AppShellViewModel` to start active-tab listing on startup and refresh listing state on accepted navigation, Back/Forward, parent navigation, refresh, tab create/duplicate/close/reopen/switch, Start Fresh, and visibility-setting changes.
- Added `ShellStateChanged` so the WinUI shell refreshes after asynchronous listing completion without putting enumeration logic in code-behind.
- Added app tests proving startup listing data reaches `FileItems`, navigation reloads visible rows, active-tab switching swaps visible rows, and selected visible rows can be copied through the view-model command route.

### Major: Multi-selection order must follow current view order

Disposition: fixed.

Resolution:

- Changed `FileListSelectionMapper` to accept the current visible file-item order.
- Normalized selected UI rows to file paths, then returned selected rows by walking `ViewModel.FileItems` in order.
- Updated `AppShellViewModel.SetSelectedFileItems` to keep selected items in current visible order.
- Added tests for out-of-order selected enumeration, sorted/filtered visible order, stale selected rows, wrapper/container mapping, and clipboard output order.

## Validation

- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` first failed for missing app-shell input bridge types, then passed after third-pass fixes: 16 App shell contract/route tests.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests and 3 App command-route tests.
- `dotnet test VeloFile.sln -c Debug --filter Selection` passed: 8 Core selection/navigation-category tests and 2 App selection tests.
- `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter Slow_hint_completion_updates_only_matching_generation` first exposed that the existing stale-generation test used a concurrency cap that conflicted with the live-underlying-read cap; the test setup was corrected and the focused test passed.
- `dotnet test VeloFile.sln -c Debug` passed: 134 tests across 4 test assemblies.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 134 tests across 4 test assemblies.
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed after fourth-pass fixes: 23 App shell contract/route tests.
- `dotnet build VeloFile.sln -c Debug` passed after fourth-pass fixes with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter Commands` passed after fourth-pass fixes: 14 Core command tests and 3 App command-route tests.
- `dotnet test VeloFile.sln -c Debug --filter Selection` passed after fourth-pass fixes: 8 Core selection/navigation-category tests and 6 App selection tests.
- `dotnet test VeloFile.sln -c Debug --filter Listing` passed after fourth-pass fixes: 19 Core listing tests, 4 Windows listing tests, and 1 App listing-route test.
- `dotnet test VeloFile.sln -c Debug` passed after fourth-pass fixes: 141 tests across 4 test assemblies.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed after fourth-pass fixes: restore, build with 0 warnings and 0 errors, and 141 tests across 4 test assemblies.
