# V1 Product Direction

## Status

accepted

## Problem

Windows users still spend daily work time in a file manager that can feel slow, inconsistent, and hard to trust during common browsing, finding, tab, terminal, and file-operation workflows. Existing alternatives often trade one pain for another: paid or closed-source tools, dated power-user interfaces, broad feature surfaces, heavy runtimes, or weak Windows integration.

VeloFile needs a concrete V1 direction that names the product problem, keeps scope disciplined, and gives future specs and architecture decisions a stable target.

## Goals

- Define VeloFile V1 as a fast, Windows-native, open-source file explorer for Windows 10 and 11.
- Focus V1 on daily file-management workflows: opening folders, tabs, sidebar navigation, breadcrumb navigation, virtualized file listing, preview, current-folder filtering, recursive search, common file operations, terminal integration, session restore, and MSIX distribution.
- Prioritize responsiveness, safety, Windows compatibility, maintainability, and later extensibility in that order.
- Keep V1 narrow enough that contributors can review behavior, architecture, tests, and release readiness against a clear contract.
- Establish the pain-point mapping that justifies V1 scope and rejects speculative feature growth.

## Non-goals

- Replacing Windows Explorer globally.
- Building a cross-platform runtime or mobile app.
- Shipping cloud sync, P2P sync, FTP/SFTP, archive-as-filesystem support, folder synchronization, duplicate finding, AI file classification, or a plugin marketplace in V1.
- Providing OS shell menu integration or hosting third-party Shell extensions in V1.
- Creating a custom global file indexer.
- Designing a full scripting language, full theme engine, or fully remappable shortcut system for V1.
- Committing to dual-pane view, batch rename, file tagging, embedded terminal panes, customizable toolbars, or color labels before the core workflows are proven.

## Vision fit

fits the current vision

The initial `VISION.md` frames VeloFile as a narrow, Windows-native file explorer that wins on responsiveness, safe file operations, Windows compatibility, open-source maintainability, and carefully bounded extensibility. This proposal now keeps OS shell menu integration and third-party Shell extension hosting out of V1, so it fits the current vision without an exception.

## Context

The repository is still close to its template state: the README contains template guidance, specs only include templates, and no product architecture or feature specs exist yet. The supplied V1 material provides the first substantive product direction and identifies concrete pain points P1 through P20 across performance, workflow, safety, power-user friction, and distribution trust.

The source input includes priority labels for candidate V1 features. This proposal treats those labels as prioritization evidence, not as final requirements; the detailed contract belongs in later specs.

Follow-up answers on 2026-05-04 resolve the first V1 open questions at recommendation level. They select WinUI 3 with C#, Shell-owned file operations and interoperability, generated benchmark corpus and app-level benchmark harness, conservative built-in preview providers, safe terminal launch semantics, explicit session persistence rules, and an Explorer parity bar based on muscle memory, compatibility, and safety.

A second follow-up on 2026-05-04 resolves the remaining proposal-level questions. It drops OS shell menu integration from V1, keeps p95 benchmark targets after the harness exists, sets concrete preview limits and terminal states, defines a cheap-and-reliable session restore field set, requires crash diagnostics and preview-release triage thresholds, and records release-note wording for the deliberate choice to show file extensions by default.

## Options considered

### Option 0: Do nothing / keep repository without V1 product direction

VeloFile could remain a template-stage repository without a committed V1 product direction.

This would avoid premature product commitment, but it would leave contributors without a scope boundary, prevent meaningful specs or architecture work, and fail to convert the named Windows file-management pain points into a reviewable project direction.

### Option 1: Explorer parity first

VeloFile could aim to recreate Windows File Explorer behavior as completely as possible before adding differentiated workflows.

This would reduce compatibility risk, but it would also spend V1 effort on broad parity instead of the pain points that justify the project. It risks inheriting Explorer's slow paths, Shell-extension latency, and unclear safety behavior.

### Option 2: Broad power-user file manager

VeloFile could launch with dual panes, batch rename, tags, plugin APIs, archive browsing, embedded terminal panes, theme customization, and advanced shortcut remapping.

This would appeal to power users quickly, but it widens the attack surface and review surface before the core browsing and file-operation contract is proven. It also conflicts with the "useful, not bloated" direction.

### Option 3: Cross-platform file manager

VeloFile could choose a cross-platform runtime and target Windows, macOS, and Linux from the start.

This would increase audience size, but it would weaken the Windows-native promise. Shell behavior, Recycle Bin semantics, drag/drop, DPI, file associations, thumbnails, long paths, and MSIX packaging are central to the product value.

### Option 4: Focused Windows-native V1

VeloFile can focus V1 on the smallest daily workflow set that directly addresses the named pain points: fast navigation and filtering, dependable tabs and session restore, bounded recursive search, safe common file operations, preview and details inspection, terminal integration, Windows-correct platform behavior, and trusted MSIX distribution.

This is the recommended option because it preserves the strongest differentiator while leaving room for future power-user features behind service and provider boundaries.

## Recommended direction

Adopt the focused Windows-native V1 direction.

V1 should behave like a fast, safe daily file manager rather than a full Explorer replacement or a power-user suite. The hot path is browsing and acting on files in the current context: launch, navigate, list, filter, preview, search, open, copy, move, rename, delete, drag/drop, and restore tabs. Windows integration should be deep where users already have expectations, while third-party Shell extension hosting and global replacement behavior stay outside the product.

This direction gives VeloFile a reviewable center: every V1 feature should either improve responsiveness, protect data, preserve Windows compatibility, or support daily developer and power-user workflows without broadening into unrelated file-management domains.

Use WinUI 3 via the Windows App SDK with C# as the recommended V1 runtime stack. This best matches the proposal's priorities together: modern Windows-native UI, mixed-DPI behavior, MSIX alignment, access to Windows and Shell APIs, and a contributor-friendly language. Record in architecture that WinUI 3 may still require selective P/Invoke or CsWin32-generated Shell COM interop.

Prefer high-level Shell APIs for Windows-correct behavior, dropping to lower-level APIs only where the managed or high-level surface cannot meet V1 needs. The recommended ownership model is:

- `IFileOperation` for copy, move, delete, Recycle Bin delete, permanent delete confirmation flow, progress, cancellation, conflicts, long paths, junctions, and elevation prompts.
- OLE drag/drop (`IDropTarget`, `IDropSource`, `IDataObject`) with `CF_HDROP` and Shell ID list formats for cross-process interoperability.
- `ShellExecuteEx` for file association open and Open With behavior.
- `IShellItemImageFactory` for thumbnails, with cache-first paths, background execution, cancellation, timeout, and generic-icon fallback.
- `SHGetFileInfo` and the system image list for icons, with extension-level caching where safe.
- `GetFinalPathNameByHandle` and consistent `\\?\` conventions for canonical paths, long paths, and reparse-point-sensitive behavior.

Do not ship OS shell menu integration in V1. V1 should provide a fast built-in context menu with core verbs and VeloFile-specific commands only. Shell extension menu hosting should require a later proposal or ADR, local crash markers, shell-menu last-action markers, diagnostic logs, and a preview-release triage threshold before it appears in any preview build.

Keep previews conservative: WIC for common images, bounded text/code preview without syntax highlighting, `Windows.Data.Pdf` for PDF page rendering, and metadata-only fallback for other file types. Treat preview providers as internal cancellable, bounded services so later preview formats can be added without changing the UI shell.

Treat terminal launch as explicit user intent and pass folder paths as data, not command fragments. Discovery should be lazy and cached; safe launch should use process working-directory support or structured arguments rather than shelling through `cmd /c` or concatenating commands.

Restore session state for tabs, history, sort/view state, scroll position, active tab, window placement, sidebar state, recent locations, and visibility toggles. Do not restore in-flight file operations, clipboard contents, recursive search results, or authentication state. Persist session state with debounced background writes plus graceful-shutdown writes using temp-file and atomic-rename behavior.

Define Explorer parity by user muscle memory and safety impact. Keyboard selection and drag/drop modifiers need high parity; fast context menu contents need core-verb parity but not exact ordering; hidden and protected operating-system files need Explorer-compatible distinction; extension display should intentionally diverge by showing extensions by default.

Treat the following p95 benchmark values as the recommended contractual targets once the generated corpus and harness exist on the documented baseline machine class:

| Area | Target |
|---|---|
| Cold launch to first interactive frame | 1500 ms |
| Warm launch to first interactive frame | 600 ms |
| Small folder switch, warm cache | 50 ms |
| Medium folder switch, warm cache | 150 ms |
| Large folder switch, warm cache | 400 ms to first viewport |
| Current-folder filter latency, medium folder | 30 ms |
| Recursive search to first result | 200 ms |
| Recursive search to 1,000 results | 2 s |
| Built-in context menu open | 50 ms |
| Tab switch to rendered viewport | 80 ms |
| Session restore, 10-tab session | 1500 ms to first interactive frame |
| Sustained large-folder scroll | 60 fps under continuous input |

Regressions over 10% at p95 should require an ADR or release-note acknowledgement. Regressions over 25% should block release. Memory footprint, startup CPU, and battery targets should wait until V1.1 unless a measurement harness exists earlier.

Set preview limits and fallbacks explicitly. Images should decode through WIC up to 8192 by 8192 decoded pixels and 100 MB input files, downsampling during decode where needed. Text/code preview should read the first 1 MB, show a truncation indicator above that, and skip preview entirely above 100 MB. PDFs should render the first page initially with user-driven page navigation, a suggested 4096 by 4096 per-page rendered memory cap, and metadata-only fallback above 500 MB. Suggested timeouts are 2 s for image decode, 1 s for text read and encoding detection, 3 s for PDF first-page render, and 500 ms per thumbnail with about four concurrent thumbnail operations. Preview work should cancel visibly within 50 ms on selection change, never modify the file, and end in one of four states: loading, success, unsupported, or failed.

Use the cheap-and-reliable session restore set for V1. Include tab paths, active tab index, per-tab sort and view state, per-tab history, window placement with monitor fallback, sidebar state, and per-tab scroll anchored by first visible item name. Keep hidden/system/extensions toggles in settings rather than session state. Exclude selection state, per-tab filter text, search query/results, clipboard contents, and in-flight file operations. Session schema reads should ignore unknown fields and fall back per-field rather than discarding the whole session.

Document the extension-display divergence in V1 release notes, the "Differences from File Explorer" docs, and inline setting help. The release note should say that VeloFile shows extensions by default, explain the `invoice.pdf.exe` safety case without claiming it makes users safe, and state that users can hide known extensions in VeloFile settings without changing File Explorer.

## Expected behavior changes

- Users can launch VeloFile side by side with Explorer and browse local, mapped, removable, and slow locations without one stalled tab freezing the whole app.
- Folder listings render through a virtualized list and keep thumbnail and metadata work off the navigation path.
- Tabs become a first-class workflow with reorder, duplicate, reopen closed tab, per-tab history, keyboard switching, and session restore.
- Current-folder filtering is separate from recursive search so quick narrowing does not depend on Windows Search indexing.
- Recursive search walks the file system directly, streams bounded results, and remains cancellable.
- Common file operations run off the UI thread, default to Recycle Bin for deletion, expose visible progress, queue conflicts, and allow session-scoped undo for the safest recent operations.
- The built-in context menu covers the V1 command set; OS shell extension menus are not exposed in V1.
- Developer workflows expose open-terminal-here, copy path, copy name, hidden/system/extensions toggles, and keyboard-first navigation.
- File extensions are shown by default, with an explicit setting to hide known extensions inside VeloFile.
- Distribution uses a signed MSIX package and does not attempt full Explorer replacement mode.

## Architecture impact

The proposal points toward clear product boundaries rather than a monolithic file manager:

- UI shell for tabs, sidebar, breadcrumb/path bar, toolbar toggles, preview/details panes, and virtualized file views.
- WinUI 3 / Windows App SDK runtime boundary with C# application code and targeted Shell interop through P/Invoke or generated COM bindings where required.
- Navigation and session services for tab state, history, scroll position, sort state, and launch restore.
- File-system enumeration and metadata providers that can cancel work on tab switch and isolate slow locations.
- Thumbnail and preview providers that are asynchronous, timeout-aware, cancellable, and bounded by supported V1 formats.
- File-operation service wrapping Shell-owned copy, move, rename, delete, Recycle Bin, conflict, progress, cancellation, long-path, junction, elevation, and undo behavior.
- Search and filter services that keep current-folder filtering distinct from recursive file-system search.
- Command layer for context menu actions, keyboard shortcuts, file association open, Open With, copy path/name, and safe terminal launch.
- OLE drag/drop boundary for cross-process drag/drop with Explorer, browsers, IDEs, and Office.
- Settings and persistence for favorites, recent locations, visibility toggles, terminal choice, versioned local session restore, and per-field migration fallback.
- Diagnostics boundary for local crash markers, last-action markers, diagnostic logs, and preview-release triage thresholds; future shell menu experiments require shell-menu last-action markers before enablement.
- Packaging and release boundary for MSIX signing and side-by-side installation.

Future architecture work should preserve room for dual-pane, batch rename, tagging, plugin APIs, embedded terminal panes, and post-V1 OS shell menu integration without pre-building those features in V1.

## Testing and verification strategy

- Unit coverage for command routing, path handling, filter matching, glob matching if included, session serialization, settings persistence, and conflict policy decisions.
- Component tests for tab state, breadcrumb editing, sidebar recent/favorite behavior, toolbar toggles, context menu command availability, preview selection behavior, and operation progress state.
- Integration tests over a fixed file-operation corpus covering copy, move, rename, Recycle Bin delete, permanent-delete confirmation, long paths, junctions, collisions, drag/drop action resolution, and file association open behavior.
- Search tests over synthetic directory trees for streaming results, bounded result counts, cancellation, permission failures, and slow path handling.
- Performance benchmarks using a deterministic generated corpus and an app-level harness for cold and warm launch, folder switch, built-in context menu open, current-folder filter latency, recursive-search first result and result milestones, tab switch, tab restore, and large-folder scroll frame timing.
- Compatibility and manual verification for mixed-DPI monitors, Explorer/browser/IDE/Office drag/drop, MSIX install/update/uninstall, common terminal targets, and preview formats.

Benchmark reports should record median, p95, and p99 values across fixed run counts, separate cold-cache and warm-cache variants where relevant, and include OS build, hardware class, storage type, Windows Search state, antivirus state, and DPI configuration. Initial performance numbers remain starting targets until the harness and corpus exist.

After the harness exists, V1 release gating should use p95 targets rather than medians. Regressions over 10% should be explicitly justified, and regressions over 25% should block release unless the proposal or a later ADR changes the benchmark policy.

Preview tests should cover size caps, timeout paths, unsupported states, failed states, file-access failures, corrupt inputs, cancellation on selection change, thumbnail concurrency limits, and the guarantee that preview generation does not modify source files.

Session restore tests should cover schema migration, unknown fields, per-field corruption fallback, missing paths, removed monitors, first-visible-item scroll anchoring, excluded selection state, excluded filter text, and crash recovery from local crash markers.

Preview-release diagnostics should verify local crash markers, diagnostic logs, last-action markers, and the defined preview-release triage threshold. Shell-menu last-action markers are required before any post-V1 preview build enables OS shell menu integration.

## Rollout and rollback

V1 should roll out as a side-by-side MSIX application rather than an Explorer replacement. Early releases can be preview-labeled while benchmarks, Shell parity tests, and packaging confidence mature.

Rollback is straightforward at the product level: users can uninstall the MSIX and continue using Explorer because VeloFile does not own global Explorer replacement behavior or system file associations. Session and settings data should be versioned so future builds can ignore or migrate older state without blocking launch.

If a V1 feature misses quality targets, it should degrade by disabling the affected surface or falling back to an explicit Windows shell path, not by blocking navigation or file operations globally.

The V1 release that establishes extension display should include the release note and matching documentation. Because showing extensions is a deliberate divergence from Explorer's default, it should be documented; matching Explorer's protected-system-file default does not need special release-note treatment.

Preview releases should define how local crash markers and diagnostic logs are stored, how last-action markers are recorded, and what triage threshold blocks promotion. Any future shell menu preview must add shell-menu last-action markers before it can be enabled.

## Risks and mitigations

- Performance targets may be unrealistic until benchmarks exist. Mitigate by treating the supplied numbers as starting targets and creating repeatable benchmark fixtures before making release claims.
- Windows Shell behavior is deep and inconsistent across drives, junctions, network paths, and long paths. Mitigate with a fixed Explorer parity corpus and explicit compatibility specs.
- File operations can destroy user data. Mitigate with Recycle Bin default, distinct permanent-delete gesture, confirmation, operation progress, cancellation, conflict queuing, and focused undo.
- Built-in context menus may omit commands users expect from Shell extensions. Mitigate by making V1's context menu scope explicit, covering core verbs well, and treating OS shell menu integration as post-V1 work.
- Future OS shell menu integration can destabilize the app through third-party handlers. Mitigate by requiring local crash markers, shell-menu last-action markers, diagnostic logs, and a preview-release triage threshold before any shell-menu preview integration is enabled.
- Preview and thumbnail work can reintroduce UI stalls. Mitigate with async providers, cancellation, bounded queues, and tab-switch cancellation.
- Terminal launch can introduce command-injection bugs if paths are composed into shell commands. Mitigate by treating paths as structured process arguments or working directories and avoiding `cmd /c` launch chains.
- Session restore can create crash loops if a bad tab path repeatedly reopens. Mitigate with crash recovery indication and a one-click start-fresh option.
- Slow removable or network drives can still consume resources. Mitigate with per-tab cancellation, timeouts where appropriate, and progress states scoped to the affected tab or operation.
- MSIX signing and update infrastructure can slow releases. Mitigate by separating packaging readiness from core feature development and testing unsigned local builds separately from signed releases.
- Open-source maintainability can suffer if V1 becomes a catch-all backlog. Mitigate by using this proposal, later specs, and ADRs as contribution boundaries.

## Open questions

The first- and second-round V1 product-direction questions now have recommendation-level answers in this proposal. No proposal-level product questions remain open.

## Decision log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-04 | Draft focused Windows-native V1 product direction. | It best matches the supplied pain points, quality goals, and initial `VISION.md` while keeping V1 reviewable. | Explorer parity first; broad power-user file manager; cross-platform file manager. |
| 2026-05-04 | Record recommendation-level answers to V1 open questions. | The answers align architecture and spec follow-up with the proposal priorities without turning the proposal into an implementation plan. | Leaving runtime, Shell ownership, benchmark, preview, terminal, session, and parity choices unresolved. |
| 2026-05-04 | Record follow-on defaults for context menu tradeoff, benchmark gates, preview limits, session restore, and extension-display release notes. | These defaults resolved the remaining product-direction questions and made the context-menu scope tradeoff explicit. | Deferring all follow-on questions to spec. |
| 2026-05-04 | Drop OS shell menu integration from V1. | This resolves the vision-fit conflict and keeps V1 focused on the built-in fast context menu. | V1 in-process Shell menu exception; V1 out-of-process Shell menu host. |

## Next artifacts

- `proposal-review` for this draft proposal.
- `specs/v1-product-scope.md` to turn accepted product direction into externally observable behavior.
- `specs/v1-product-scope.test.md` to map V1 contract points to verification.
- Architecture artifact covering UI/runtime choice, Shell integration, file-operation boundaries, data flow, persistence, packaging, and performance strategy.
- ADRs for UI/runtime choice, Shell API ownership, benchmark corpus and harness, preview provider boundaries, terminal launch safety, session restore model, Explorer parity policy, and diagnostic triage policy.
- Execution plan under `docs/plans/` after proposal, spec, test spec, and architecture are stable enough to sequence.

## Follow-on artifacts

- [V1 Product Scope](../../specs/v1-product-scope.md)

## Readiness

Accepted and ready for downstream spec, architecture, test-spec, and planning work. The prior vision-fit conflict is resolved by dropping OS shell menu integration from V1.
