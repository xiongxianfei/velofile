# M5 Review Resolution

## Findings Resolved

### Invalid typed path navigation

Resolution:

- Split interactive path submission from restore-time missing-location behavior.
- `NavigationEntryPointService` now parses and probes typed/sidebar/breadcrumb targets before committing the active tab path.
- Rejected typed paths leave the active tab path, history, location state, and recents unchanged, and surface a recoverable path-entry error instead.
- Restore-time missing paths still remain visible as missing-location tabs with a close action.

Tests:

- `AppShellCommandSurfaceTests` verifies typed missing paths preserve the current valid tab, history, and recents.
- `AppShellCommandSurfaceTests` verifies empty/invalid typed input preserves active state and surfaces a path-entry error.
- `AppShellStartupServiceTests` continues to verify missing restored paths reach the shell as visible recoverable tabs.
- `AppShellContractTests` verifies the WinUI shell exposes a path-entry failure state.

### Durable visibility setting writes

Resolution:

- Added `ISettingsStateWriter` and `DurableSettingsStateWriter` so visibility toggles write through the durable settings repository instead of only mutating memory.
- `AppShellStartupService` retains the settings writer and passes it to the command surface.
- `AppCompositionRoot` wires the production settings writer to the partial-write-safe settings repository.
- Toggle write failures are best-effort: the current in-memory setting remains applied, diagnostics are attempted, and the UI command does not crash.

Tests:

- `AppShellCommandSurfaceTests` verifies hidden/system/extension toggles write updated settings without resetting unrelated flags.
- `AppShellCommandSurfaceTests` verifies settings write failure does not crash or revert the current session state.
- `AppShellStartupServiceTests` verifies a visibility toggle survives durable write plus restarted bootstrap.

### Window placement application and removed-monitor fallback

Resolution:

- Added `MonitorWindowPlacementResolver` with a testable monitor-layout source.
- Added `WindowsMonitorLayoutSource` for production monitor/work-area enumeration.
- Replaced the production pass-through resolver in `AppCompositionRoot` with the real resolver.
- Added `IWindowPlacementApplier` / `WinUiWindowPlacementApplier` and wired `MainWindow` launch to apply resolved placement through WinUI app-window APIs.
- Added `WindowPlacementPolicy` so app shell minimum size and resolver fallback behavior use one policy source.
- Replaced raw nullable placement handoff with `WindowPlacementResolution`, including status and `ShouldApply`, so the app applier receives a resolved safe placement or an explicit do-not-apply result.
- Treat positive-but-below-minimum persisted sizes as invalid and fall back to visible default placement.
- Treat empty or failed monitor enumeration as unable to prove placement safety; stale persisted placement is not applied unchanged.
- Made the resolver boundary explicitly physical-pixel based for persisted bounds, monitor work areas, resolved placements, and `AppWindow.MoveAndResize` input.
- Kept the WinUI shell minimum in XAML effective pixels and converted it to physical pixels with the selected target monitor's scale before size validation.
- Treat unknown monitor scale as unable to prove placement safety; stale persisted placement is not applied when scale is unavailable.

Tests:

- `WindowPlacementResolverTests` verifies valid placement preservation, removed-monitor fallback, offscreen clamping, and invalid-dimension fallback.
- `WindowPlacementResolverTests` verifies `1x1`, `100x100`, `899x560`, `900x559`, partially offscreen tiny placement, missing-monitor tiny placement, empty monitor enumeration, throwing monitor enumeration, oversized clamping, constrained monitor fallback, and minimum/above-minimum valid sizes.
- `WindowPlacementResolverTests` verifies 200% scale rejects `900x560` physical pixels, accepts `1800x1120`, uses 150% and mixed-DPI target-monitor scale, uses fallback-monitor scale for removed monitors, and does not apply placement when scale is unknown.
- `AppShellStartupServiceTests` verifies startup exposes a safe window-placement resolution for the app applier.
- `AppShellContractTests` verifies production composition uses the real monitor resolver and applies restored placement.

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
