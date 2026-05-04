# M5 Review Resolution

## Findings Resolved

### WinUI shell command wiring

Resolution:

- Added `AppShellCommandSurface` in Core as the shared command path for typed paths, breadcrumb/sidebar activation, back/forward/up/refresh, tab lifecycle, tab switching, visibility toggles, missing-location close, and crash recovery start-fresh.
- Updated `MainWindow.xaml` and `MainWindow.xaml.cs` so buttons, sidebar activation, breadcrumb activation, path-box Enter, tab selection, visibility toggles, and keyboard accelerators call the view model instead of remaining static.
- Kept code-behind as a thin UI bridge; navigation and restore policy remain in Core services.

Tests:

- `AppShellCommandSurfaceTests` verifies path submit, sidebar favorites/recents/drives, history, parent navigation, refresh, tab lifecycle, missing-path state, and start-fresh.
- `AppShellContractTests` verifies command routes and accelerator invoked handlers exist in the WinUI shell.

### Launch restore and persisted state

Resolution:

- Added `AppShellStartupService` as the testable startup composition boundary.
- Added `AppCompositionRoot` in the app project to read durable session, settings, favorites, and recent-location documents with `DurableDocumentRepository`.
- App launch now constructs diagnostics, safe persistence storage, restore probes, crash recovery signal, drive entries, `SessionRestoreService`, and the shell view model through the composition root.

Tests:

- `AppShellStartupServiceTests` verifies normal restore, safe-default restore, crash-marker start-fresh state, and missing-path restore reaching the shell.
- `AppShellContractTests` verifies the app uses the composition root instead of constructing hardcoded shell state in `MainWindow`.

### Empty workspace invariant

Resolution:

- `NavigationWorkspace` now enforces an always-at-least-one-tab invariant.
- Empty restored tabs create a default tab.
- Closing the last tab replaces it with a default tab instead of leaving `ActiveTab` unsafe.
- `SessionRestoreService` receives an explicit `IDefaultLaunchPathProvider` and normalizes empty session payloads through the workspace invariant.

Tests:

- `NavigationWorkspaceTests` covers empty restored tabs and close-last-tab behavior.
- `SessionRestoreServiceTests` covers `SessionStatePayload.Empty` restoring to a safe default active tab.

## Validation

Validation commands and final results are recorded in the active plan and `change.yaml`.
