# M4 Listing and Visibility Change Explanation

## Scope

M4 adds the non-UI foundations for folder browsing:

- Core file-system entry and listed-item models.
- A folder listing service that exposes pending, ready, empty, access-denied, unavailable, invalid-path, cancelled, and failed state shapes.
- A virtualization-ready first-viewport feed that stops after the visible working set instead of requiring full-folder UI allocation.
- A per-tab request gate that lets stale slow-tab results be ignored without invalidating work in other tabs.
- Visibility projection for hidden files, protected operating-system files, extension display, and dimming flags for hidden/protected items that are intentionally visible.
- Windows folder and drive adapters for local enumeration, extended local paths, cancellation, file attributes, and optional drive free-space hints.

This milestone does not implement WinUI navigation surfaces. Folder open entry points, breadcrumb/path bar, sidebar rendering, view-mode controls, keyboard navigation, and accessibility-visible UI state remain assigned to M5/M6.

## Test-First Evidence

M4 tests were added before production listing and visibility namespaces existed:

- `tests/VeloFile.Core.Tests/Listing/ListingTests.cs`
- `tests/VeloFile.Core.Tests/Visibility/VisibilityTests.cs`
- `tests/VeloFile.Windows.Tests/FileSystem/WindowsFolderEntrySourceTests.cs`

The first targeted run failed for the expected reason: `VeloFile.Core.Listing`, `VeloFile.Core.Visibility`, and `VeloFile.Windows.FileSystem` did not exist yet.

## Design Choices

`VeloFile.Core.Listing` owns folder state, file item models, the first-viewport feed, and stale request gating. It depends on an `IFolderEntrySource` interface so slow or platform-specific enumeration remains outside Core.

`VeloFile.Core.Visibility` owns the projection rules for hidden/protected/system files and display names. Extensions are shown by default. Known extensions can be hidden per VeloFile settings, but unknown extensions are left visible to avoid making suspicious filenames look safer than they are.

`VeloFile.Windows.FileSystem` owns direct `DirectoryInfo`, `FileSystemInfo`, and `DriveInfo` access. It surfaces attributes and optional drive space hints but does not perform thumbnails, icons, Shell metadata, or preview work on the listing hot path.

Navigation failures can emit redacted diagnostics through the existing M3 diagnostics boundary. The service preserves the previous valid listing state on recoverable failures when a caller supplies it.

## Validation

M4 focused validation passed with:

- `dotnet test VeloFile.sln -c Debug --filter Listing`
- `dotnet test VeloFile.sln -c Debug --filter Visibility`

Final closeout CI is recorded in the active plan and `change.yaml`.

Final milestone closeout also passed:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`

The closeout run built with 0 warnings and 0 errors and passed 45 tests across 4 test assemblies.

## Deferred By Plan

- WinUI folder open entry points, breadcrumb/path bar, sidebar, tab history integration, and view-mode controls: M5.
- Built-in context menu, keyboard commands, and selection behavior: M6.
- Current-folder filtering and recursive search over active listings: M7.
- Thumbnail/icon/detail enrichment and preview selection behavior: M11-M13.
- App-level performance harness gates for p95 folder switch and slow-tab scenarios: M15.
