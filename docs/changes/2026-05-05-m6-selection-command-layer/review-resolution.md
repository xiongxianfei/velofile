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

## Validation

- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 8 App shell contract tests.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 126 tests across 4 test assemblies.
