# M4 Listing and Visibility Change Explanation

## Scope

M4 adds the non-UI foundations for folder browsing:

- Core file-system entry and listed-item models.
- A folder listing service that exposes pending, ready, empty, access-denied, unavailable, invalid-path, cancelled, and failed state shapes.
- A folder listing coordinator that owns per-tab listing state, per-tab cancellation, in-flight request versions, and stale completion rejection.
- A virtualization-ready first-viewport feed that stops after the visible working set instead of requiring full-folder UI allocation.
- A per-tab request gate that lets stale slow-tab results be ignored without invalidating work in other tabs.
- Visibility projection for hidden files, protected operating-system files, extension display, and dimming flags for hidden/protected items that are intentionally visible.
- Windows folder and drive adapters for local enumeration, extended local paths, cancellation, file attributes, fast drive entry discovery, and optional drive free-space hints through timeout-bounded enrichment.

This milestone does not implement WinUI navigation surfaces. Folder open entry points, breadcrumb/path bar, sidebar rendering, view-mode controls, keyboard navigation, and accessibility-visible UI state remain assigned to M5/M6.

## Test-First Evidence

M4 tests were added before production listing and visibility namespaces existed:

- `tests/VeloFile.Core.Tests/Listing/ListingTests.cs`
- `tests/VeloFile.Core.Tests/Listing/FolderListingCoordinatorTests.cs`
- `tests/VeloFile.Core.Tests/Listing/DriveEntryServiceTests.cs`
- `tests/VeloFile.Core.Tests/Visibility/VisibilityTests.cs`
- `tests/VeloFile.Windows.Tests/FileSystem/WindowsFolderEntrySourceTests.cs`

The first targeted run failed for the expected reason: `VeloFile.Core.Listing`, `VeloFile.Core.Visibility`, and `VeloFile.Windows.FileSystem` did not exist yet.

## Design Choices

`VeloFile.Core.Listing` owns folder state, file item models, the first-viewport feed, stale request gating, and a tab listing coordinator. The coordinator starts per-tab requests independently, cancels superseded tab work, keeps cancellation outside the coordinator lock to avoid reentrant stale applies, and only publishes results when the tab id, request version, and active path still match.

`VeloFile.Core.Visibility` owns the projection rules for hidden/protected/system files and display names. Extensions are shown by default. Known extensions can be hidden per VeloFile settings, but unknown extensions are left visible to avoid making suspicious filenames look safer than they are.

`VeloFile.Core.Listing` also owns drive entry refresh orchestration. Drive entries are produced from fast discovery first with null hints, while free-space and label hints are read through `IDriveHintSource` with timeout, cancellation, bounded concurrency, and refresh-generation checks.

`VeloFile.Windows.FileSystem` owns direct `DirectoryInfo`, `FileSystemInfo`, and `DriveInfo` access. It surfaces attributes, fast drive entries, and an async drive hint source, but does not perform thumbnails, icons, Shell metadata, preview work, or synchronous drive free-space reads on the listing hot path.

Navigation failures can emit redacted diagnostics through the existing M3 diagnostics boundary. The service preserves the previous valid listing state on recoverable failures when a caller supplies it.

## Validation

M4 focused validation passed with:

- `dotnet test VeloFile.sln -c Debug --filter Listing`
- `dotnet test VeloFile.sln -c Debug --filter Visibility`

M4 review-resolution focused validation also passed with:

- `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter "Slow_tab_listing|Stale_slow_listing|Closing_tab_cancels|Drive_entries_are_returned|Slow_hint_completion|Hint_failure|Cancelling_hint"`
- `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter "Timed_out_non_cancelling_hints|Cancellation_ignoring_old_listing"`
- `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter Listing`
- `dotnet test VeloFile.sln -c Debug --filter Listing`

Final closeout CI is recorded in the active plan and `change.yaml`.

Final milestone closeout also passed:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`

The review-resolution closeout run built with 0 warnings and 0 errors and passed 58 tests across 4 test assemblies.

## Deferred By Plan

- WinUI folder open entry points, breadcrumb/path bar, sidebar, tab history integration, and view-mode controls: M5.
- Built-in context menu, keyboard commands, and selection behavior: M6.
- Current-folder filtering and recursive search over active listings: M7.
- Thumbnail/icon/detail enrichment and preview selection behavior: M11-M13.
- App-level performance harness gates for p95 folder switch and slow-tab scenarios: M15.
