# M5 Navigation Shell Change Explanation

## Scope

M5 connects the non-UI listing and persistence foundations to core browsing state and a compiled app shell surface:

- Tab lifecycle state: open, close, reorder, duplicate, reopen closed tab, active tab, and keyboard-style next/previous switching.
- Per-tab navigation history with independent back/forward stacks.
- Breadcrumb segment parsing and raw path navigation inputs.
- View-mode, sort, and first-visible-item scroll-anchor state on tabs.
- Sidebar state for pinned favorites, recent locations capped at 20, dismissible recents, and drive entries with nullable hints.
- Session restore orchestration for tab paths, active tab index, history, sort, view mode, scroll anchor by item name, window placement, missing-path tabs, sidebar state, visibility settings, and repeated-crash start-fresh prompts.
- Visibility settings service for hidden files, protected operating-system files, and file extensions.
- A Core shell command surface for typed path navigation, sidebar targets, breadcrumb navigation, back/forward/up/refresh, tab lifecycle, keyboard-style tab switching, visibility toggles, missing-location close, and crash-recovery start-fresh.
- A launch composition path that reads durable session, settings, favorites, and recent-location documents before creating the WinUI shell view model.
- WinUI main-window shell regions and event routes for tabs, sidebar, breadcrumb/raw path, navigation buttons, visibility controls, keyboard accelerators, file list states, crash recovery, and missing-location state.

M5 does not implement file selection, file operations, context-menu verbs, filtering/search, preview providers, drag/drop, or benchmarked UI automation. Those remain assigned to later milestones. M5 now includes navigation command routing because it is required for the shell to satisfy AC2.

## Test-First Evidence

M5 tests were added before production navigation, sidebar, and session namespaces existed:

- `tests/VeloFile.Core.Tests/Navigation/NavigationWorkspaceTests.cs`
- `tests/VeloFile.Core.Tests/Sidebar/SidebarStateTests.cs`
- `tests/VeloFile.Core.Tests/Session/SessionRestoreServiceTests.cs`
- `tests/VeloFile.Core.Tests/Shell/AppShellCommandSurfaceTests.cs`
- `tests/VeloFile.Core.Tests/Shell/AppShellStartupServiceTests.cs`
- `tests/VeloFile.Core.Tests/Visibility/VisibilitySettingsServiceTests.cs`
- `tests/VeloFile.App.Tests/AppShellContractTests.cs`

The first focused run failed for the expected reason: `VeloFile.Core.Navigation`, `VeloFile.Core.Sidebar`, `VeloFile.Core.Session`, and the app shell regions did not exist yet.

Review-resolution tests were also added before production fixes for the three review findings. The first focused review-resolution run failed for the expected reason: the Core shell command/startup surfaces and app shell command routes did not exist.

## Design Choices

`VeloFile.Core.Navigation` owns tab state and path navigation without touching the filesystem. That keeps slow path validation and enumeration behind the M4 listing boundary while still letting the shell update active paths, history, view mode, sort state, and scroll anchors.

`VeloFile.Core.Sidebar` owns favorites, recents, and drive entries as local state. Recents are capped through the existing durable payload rule, and drive hints remain nullable because M4 already established that hints must not block navigation.

`VeloFile.Core.Session` converts durable session/settings/favorites/recent payloads into runtime state. Path existence, monitor availability, scroll-anchor resolution, and repeated-crash detection are explicit interfaces so restore behavior is testable and does not couple Core to Windows APIs.

`VisibilitySettingsService` wraps the M4 visibility projection settings with persistence conversion and first-use confirmation behavior for protected operating-system files.

`AppShellCommandSurface` is the single command path for user-visible navigation surfaces. Buttons, path submission, sidebar target activation, breadcrumb activation, tab selection, and keyboard accelerators route through `AppShellViewModel` into that command surface; code-behind only translates WinUI events into typed command calls.

`AppCompositionRoot` is the launch composition boundary. It constructs local diagnostics, Windows durable storage, typed durable repositories for session/settings/favorites/recent locations, restore probes, drive entries, `SessionRestoreService`, and `AppShellStartupService` before creating `AppShellViewModel`. `MainWindow` receives the view model and no longer decides restore policy.

`NavigationWorkspace` now enforces the V1 invariant that a usable shell always has at least one tab. Empty restored sessions and close-last-tab behavior normalize to a default active tab rather than leaving `ActiveTab` unsafe.

The WinUI shell is intentionally a concrete first screen, not a landing page. It exposes the required regions, command routes, and keyboard accelerator invoked handlers, while later milestones will connect file selection, operations, search, and preview surfaces.

## Validation

Focused M5 validation passed with:

- `dotnet test VeloFile.sln -c Debug --filter Navigation`
- `dotnet test VeloFile.sln -c Debug --filter Session`
- `dotnet test VeloFile.sln -c Debug --filter Sidebar`
- `dotnet test VeloFile.sln -c Debug --filter Visibility`

Build validation passed with:

- `dotnet build VeloFile.sln -c Debug`

Final milestone closeout also passed:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`

The review-resolution closeout run built with 0 warnings and 0 errors and passed 85 tests across 4 test assemblies.

## Deferred By Plan

- Command layer, file selection, built-in context menu, and clipboard commands: M6.
- Current-folder filter and recursive search: M7.
- File operations and drag/drop: M8-M10.
- Preview, thumbnails, icons, and details pane: M11-M13.
- Release packaging and benchmark gates: M15-M16.
