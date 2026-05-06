# V1 Product Scope Execution Plan

## Status

active

This plan has passed `plan-review` and has an active test spec. Implementation starts at M1.

## Purpose / Big Picture

This plan turns the accepted V1 product direction, approved V1 product scope, and approved architecture package into reviewable implementation slices. The repository is still template-stage: there is no application source, test project, package manifest, benchmark harness, or product CI. The first implementation work must therefore create the Windows-native app foundation and verification surfaces before adding user-facing file-manager behavior.

V1 ships only when the core daily workflows work together: open folders, navigate with tabs and sidebar, render large folders with virtualization, filter/search, preview, perform safe file operations, launch terminals, restore sessions, record local diagnostics, and distribute as signed MSIX without replacing Explorer.

## Source Artifacts

| Artifact | Path | Status |
|---|---|---|
| Product direction | [docs/proposals/2026-05-04-v1-product-direction.md](../proposals/2026-05-04-v1-product-direction.md) | accepted |
| Feature spec | [specs/v1-product-scope.md](../../specs/v1-product-scope.md) | approved |
| Architecture package | [docs/architecture/system/architecture.md](../architecture/system/architecture.md) | approved |
| C4 context | [docs/architecture/system/diagrams/context.mmd](../architecture/system/diagrams/context.mmd) | current |
| C4 container | [docs/architecture/system/diagrams/container.mmd](../architecture/system/diagrams/container.mmd) | current |
| C4 component | [docs/architecture/system/diagrams/desktop-app-components.mmd](../architecture/system/diagrams/desktop-app-components.mmd) | current |
| ADRs | [docs/adr/](../adr/) | 0001-0008 accepted |
| Test spec | [specs/v1-product-scope.test.md](../../specs/v1-product-scope.test.md) | active proof surface |

Architecture review approved the package on 2026-05-04 with one administrative condition: normalize architecture status to `approved`. That normalization is done before this plan relies on the architecture.

## Context and Orientation

Current repository state:

- No product source tree exists yet.
- Existing CI is template-only: [.github/workflows/ci.yml](../../.github/workflows/ci.yml) runs [scripts/ci.sh](../../scripts/ci.sh), which reports that no real build system is configured.
- Existing release workflow is template-only: [.github/workflows/release.yml](../../.github/workflows/release.yml) runs [scripts/release-verify.sh](../../scripts/release-verify.sh), which is a checklist placeholder.
- Existing public docs still include template text in [README.md](../../README.md), though the vision block now describes VeloFile.
- There is no `CONSTITUTION.md` or `docs/project-map.md`. The V1 test spec now exists as the active proof surface for implementation.

Planned source layout, subject to M1 validation:

```text
VeloFile.sln
src/
  VeloFile.App/              # WinUI 3 desktop app
  VeloFile.Core/             # app services, state, commands, testable models
  VeloFile.Windows/          # Shell/Win32/WinRT/OLE interop adapters
tests/
  VeloFile.Core.Tests/
  VeloFile.Windows.Tests/
  VeloFile.App.Tests/
tools/
  VeloFile.Corpus/
  VeloFile.Benchmarks/
packaging/
  msix/
docs/
  release/
  user/
```

Architectural boundaries to preserve:

- UI shell owns WinUI surfaces and accessibility-visible states, not raw Shell COM or file mutations.
- Command layer passes structured intents and paths as data.
- Windows integration lives behind Shell/Win32/WinRT/OLE adapters.
- Persistence writes versioned documents with same-directory temp files, Windows-safe replacement, and last-known-good recovery.
- Diagnostics are local-only by default, redacted, bounded by retention rules, and never upload without a separate approved opt-in proposal.
- OS shell menu integration and third-party Shell extension hosting are out of V1.

## Non-Goals

- Cross-platform runtime.
- Global Explorer replacement.
- OS shell menu integration or third-party Shell extension hosting.
- Cloud sync, P2P sync, FTP/SFTP, archive-as-filesystem browsing, folder synchronization, duplicate finder, AI classification, or plugin marketplace.
- Custom global file indexer, content search, or indexed search.
- Office/media/RAW/archive preview.
- Fully remappable shortcuts or embedded terminal pane.
- Dual-pane view, batch rename, tagging, customizable toolbar, color labels, or theme engine beyond light/dark.
- Durable cross-session undo or durable resumable file operations.
- Telemetry upload or crash-report upload without a separate user-consented specification.

## Requirements Covered

| Requirement area | IDs | Primary milestone(s) |
|---|---|---|
| Product boundary, Windows target, MSIX, no Explorer replacement | R1-R4, C1-C2, C9-C10, AC1, AC16 | M1, M16 |
| Validation tooling and corpora | R46, R67-R68, R83, O5, P1-P16, AC9-AC10, AC15 | M2, M10, M13, M15 |
| Navigation and virtualized file list | R5-R13, AC2-AC3 | M4, M5 |
| Sidebar, favorites, recents, drives | R14-R17 | M5 |
| Tabs and session restore | R18-R27, C6-C8, AC13 | M3, M5 |
| Filtering and recursive search | R28-R35, AC4-AC5 | M7 |
| File operation contracts, safe delete, rename | R36-R40, R43, R45, I1-I2, S5, AC6 | M8 |
| Copy/move, conflicts, progress, cancellation, undo eligibility | R36-R46, R41-R45, AC7 | M9 |
| Drag/drop and Windows compatibility corpus | R46, R79-R80, AC9 | M10 |
| Context menu, commands, shortcuts, clipboard | R47-R53, AC8 | M6 |
| Terminal and file association integration | R54-R59, R81-R82, I8, S2, AC12 | M14 |
| Preview contract, metadata fallback, provider implementations, thumbnails/details | R60-R72, I3, S7, AC10-AC11 | M11, M12, M13 |
| Visibility and Explorer parity | R73-R83, I5, I7-I8, S6, S8, AC9, AC14 | M5, M6, M10, M16 |
| Diagnostics and preview readiness | R84-R89, O1-O7, S3, AC17 | M3, M15 |
| Packaging, docs, release notes | R90-R93, AC18 | M16 |
| Accessibility and UX | A11Y1-A11Y7, AC19 | M5, M6, M8, M9, M11, M13, M16 |
| Performance expectations and quality scenarios | P1-P16, QS-* | M2, M4, M7, M11, M13, M15, M16 |

## Milestones

### M1. Product Foundation, Solution Layout, and Real CI

- Goal: Create the WinUI 3 / Windows App SDK solution skeleton, test project structure, Windows CI entrypoint, and baseline app launch surface.
- Requirements: R1-R4, C1-C2, ADR 0001, architecture sections 2, 5, and 7.
- Files/components likely touched: `VeloFile.sln`, `src/VeloFile.App/`, `src/VeloFile.Core/`, `src/VeloFile.Windows/`, `tests/*`, `scripts/ci.*`, `.github/workflows/ci.yml`, `.github/workflows/release.yml`, `README.md`, `AGENTS.md`.
- Dependencies: Windows dev environment, .NET SDK, Windows App SDK, WinUI 3 project templates or equivalent checked-in project files.
- Tests to add/update: smoke test for app composition root or bootstrappable services; CI smoke command proving solution restore/build/test.
- Implementation steps:
  - Create solution and project layout.
  - Add an empty but launchable VeloFile WinUI app shell.
  - Add test projects with initial passing smoke tests.
  - Replace template CI with Windows-specific build/test commands.
  - Update `AGENTS.md` verification commands from placeholder guidance to real commands.
- Validation commands:
  - `dotnet --info`
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug`
  - `dotnet test VeloFile.sln -c Debug`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` or the final M1 CI command recorded in `AGENTS.md`
- Expected observable result: The repository has a real Windows app solution, a testable service layer, and CI no longer reports template-only validation.
- Commit message: `M1: scaffold WinUI app foundation and CI`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: WinUI project setup can be brittle across SDK versions.
- Rollback/recovery: Revert the scaffold commit; no durable user data exists yet.

### M2. Validation Tooling and Minimal Corpus Foundations

- Goal: Create the validation scripts, scratch-root safety rules, and minimal generated corpus profiles that later milestones can use before they depend on those commands.
- Requirements: R46, R67-R68, R83, O5, P1-P16, AC9-AC10, AC15, ADR 0003, ADR 0004.
- Files/components likely touched: `tools/VeloFile.Corpus/`, `scripts/generate-corpus.ps1`, `scripts/run-compat-corpus.ps1`, `scripts/run-preview-corpus.ps1`, `scripts/run-benchmarks.ps1`, `tests/VeloFile.Corpus.Tests/`, `docs/release/benchmark-baseline.md`.
- Dependencies: M1.
- Tests to add/update: corpus profile smoke tests, script argument/scope validation tests, scratch-root refusal tests, empty non-gating benchmark report stub test.
- Implementation steps:
  - Create `tools/VeloFile.Corpus/` with deterministic minimal profiles: smoke, operations, preview, search, and large-folder placeholders.
  - Add `scripts/generate-corpus.ps1` with required scratch root argument and refusal to run outside an explicit generated-corpus workspace.
  - Add `scripts/run-compat-corpus.ps1` with scoped runners and a safe no-op/fail-fast mode for scopes not implemented yet.
  - Add `scripts/run-preview-corpus.ps1` with smoke corpus generation and provider-test handoff hooks.
  - Add `scripts/run-benchmarks.ps1` as a documented non-gating stub that emits environment/report shape but does not assert performance until M15.
  - Document the milestone dependency rule: no milestone may require a validation command unless the script, corpus profile, and fixtures exist from a prior milestone or are created in that same milestone before use.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Corpus`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope smoke -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Later milestones can call corpus/compat/preview/benchmark scripts without depending on nonexistent validation assets, and all generated files stay under an explicit scratch root.
- Commit message: `M2: add validation tooling and minimal corpus foundations`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Validation scripts can accidentally mutate user folders if scratch-root checks are weak.
- Rollback/recovery: Scripts must refuse ambiguous roots; generated corpora are disposable and can be deleted safely.

### M3. Durable State, Diagnostics, and Local Data Foundations

- Goal: Implement versioned persistence, partial-write-safe local documents, local diagnostics, crash markers, and last-action markers before workflows depend on them.
- Requirements: R21-R27, R84-R89, C6-C8, I6, I10, O1-O4, O6, S3-S4, ADR 0006, ADR 0008, QS-SESSION-RECOVERY-01, QS-DIAG-PRIV-01.
- Files/components likely touched: `src/VeloFile.Core/Persistence/`, `src/VeloFile.Core/Diagnostics/`, `src/VeloFile.Windows/Storage/`, `tests/VeloFile.Core.Tests/`, `tests/VeloFile.Windows.Tests/`.
- Dependencies: M1, M2 for script/scratch-root conventions where fault fixtures are generated.
- Tests to add/update: durable document header tests, unknown-field tests, per-field fallback tests, same-directory temp/backup recovery tests, redaction allowlist/prohibited field tests, retention/rotation tests.
- Implementation steps:
  - Define durable document envelopes for session, settings, favorites, and recent locations.
  - Implement atomic write protocol with temp files and last-known-good recovery.
  - Implement diagnostic event schema, redaction rules, local retention, and marker storage.
  - Implement crash marker and repeated-failure detection primitives without UI recovery flow yet.
  - Add test seams for persistence fault injection.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Persistence`
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: State and diagnostics can be safely read/written locally with redaction and recovery behavior independent of the UI.
- Commit message: `M3: add safe persistence and local diagnostics foundations`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Atomic replacement behavior differs by file system and packaging context.
- Rollback/recovery: Keep persisted file formats behind versioned readers; tests must prove fallback to safe defaults.

### M4. Folder Enumeration, File Models, and Virtualized Listing Feed

- Goal: Build the file listing service and Windows adapters needed to enumerate folders asynchronously, isolate slow locations, and provide a virtualization-ready first viewport feed.
- Requirements: R5, R7-R13, R17, R73-R78, C5, EC1-EC5, EC8, EC23, QS-RESP-01, QS-SLOW-TAB-01.
- Files/components likely touched: `src/VeloFile.Core/Listing/`, `src/VeloFile.Core/Visibility/`, `src/VeloFile.Windows/FileSystem/`, `src/VeloFile.Windows/Shell/`, `tests/*`.
- Dependencies: M1, M2 validation tooling, M3 diagnostics for failure logging.
- Tests to add/update: enumeration success/failure, access denied, invalid path, unavailable path, long path surface, hidden/protected/extension visibility projection, cancellation/ignore on stale work, slow-location isolation unit tests.
- Implementation steps:
  - Define file item models and folder state models.
  - Add Windows folder enumeration adapter with cancellation/ignore semantics.
  - Add visibility setting projection for hidden, protected system, and extension display.
  - Add first-viewport feed abstraction for the UI virtualized list.
  - Add user-visible state models for empty, unavailable, access-denied, and pending folders.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Listing`
  - `dotnet test VeloFile.sln -c Debug --filter Visibility`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Core services can enumerate representative folders and expose stable state for a virtualized UI without blocking unrelated tabs.
- Commit message: `M4: add asynchronous listing and visibility services`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Shell metadata/icon calls can creep into the hot path.
- Rollback/recovery: Keep metadata and thumbnail enrichment behind separate adapters so listing can fall back to names/types only.

### M5. UI Shell, Tabs, Sidebar, Breadcrumb, and Session Restore

- Goal: Connect the app shell to navigation, tabs, sidebar, breadcrumb/path bar, visibility settings, and session restore.
- Requirements: R5-R6, R8-R16, R18-R27, R73-R78, A11Y1-A11Y2, A11Y4, A11Y6-A11Y7, AC2, AC13-AC14.
- Files/components likely touched: `src/VeloFile.App/Views/`, `src/VeloFile.App/ViewModels/`, `src/VeloFile.Core/Navigation/`, `src/VeloFile.Core/Session/`, `tests/VeloFile.App.Tests/`, `tests/VeloFile.Core.Tests/`.
- Dependencies: M3, M4.
- Tests to add/update: tab lifecycle, history, active tab, missing-path restore, monitor fallback, scroll-anchor restore by first visible item name, sidebar recents capped at 20, favorites add/remove, breadcrumb raw path editing, visibility settings persistence.
- Implementation steps:
  - Implement tab, history, and navigation state services.
  - Implement app shell views for tabs, sidebar, breadcrumb/path bar, and file list modes.
  - Wire session restore and missing-location tab UI.
  - Wire favorites, recent locations, drives, and persistent visibility toggles.
  - Add keyboard paths for navigation, tabs, preview toggle placeholder, and state surfaces.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Navigation`
  - `dotnet test VeloFile.sln -c Debug --filter Session`
  - `dotnet test VeloFile.sln -c Debug --filter Sidebar`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: A user can launch VeloFile, open folders, navigate paths, use tabs/sidebar, and restore a session with safe missing-path behavior.
- Commit message: `M5: implement navigation shell and session restore`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Restored state can create confusing UI or crash loops.
- Rollback/recovery: Start-fresh recovery remains available; corrupt session fields fall back per M3.
- Review-resolution notes:
  - First review resolution wired the WinUI shell to `AppShellCommandSurface`, moved app launch through durable session/settings/favorites/recent-location restore, and enforced the always-at-least-one-tab workspace invariant.
  - Second review resolution keeps typed/pasted invalid paths out of the active tab path/history/recents, while preserving restore-specific missing-location tabs.
  - Visibility toggles now write through a retained durable settings writer and survive restarted bootstrap.
  - Restored window placement is resolved against real production monitor layout data and applied through a WinUI window placement applier; production composition no longer uses the pass-through monitor resolver.
  - Window-placement safety review resolution adds a shared placement policy, rejects positive-but-below-minimum saved rectangles, returns do-not-apply when monitor enumeration is empty or fails, and passes an auditable `WindowPlacementResolution` to the app applier.
  - DPI unit review resolution makes persisted and resolved window bounds physical-pixel based, converts the XAML effective-pixel shell minimum through the selected target monitor scale before validation, and rejects persisted placement when scale is unavailable.
  - Focused validation for the DPI-aware window-placement review resolution passed with `dotnet test VeloFile.sln -c Debug --filter Session` (36 Core tests) and `dotnet test VeloFile.sln -c Debug --filter "Navigation|Session|Visibility"` (51 Core tests and 3 App shell contract tests); final CI passed with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` (115 tests).

### M6. Selection, Command Layer, Built-In Context Menu, and Clipboard

- Goal: Implement Explorer-compatible selection, keyboard commands, built-in context menu core verbs, copy path/name, and command availability.
- Requirements: R47-R53, R50-R51, R73-R78, A11Y1-A11Y3, AC8, ADR 0007.
- Files/components likely touched: `src/VeloFile.Core/Commands/`, `src/VeloFile.Core/Selection/`, `src/VeloFile.App/Views/ContextMenu/`, `src/VeloFile.Windows/Clipboard/`, `tests/*`.
- Dependencies: M4, M5.
- Tests to add/update: keyboard selection, range/toggle selection, `Ctrl+A`, `Escape`, Enter, F2, F5, Backspace, Delete command routing, copy path/name clipboard formatting, context menu command availability, absence of OS Shell extension entries.
- Implementation steps:
  - Implement selection model and keyboard routing.
  - Implement built-in context menu command definitions.
  - Implement clipboard adapter for copy path and copy name.
  - Add Properties command placeholder or implementation path consistent with architecture.
  - Ensure context menu never enumerates OS Shell extensions.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Commands`
  - `dotnet test VeloFile.sln -c Debug --filter Selection`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Users can select files with mouse/keyboard, open the built-in context menu, and invoke safe command-layer routes.
- Commit message: `M6: add selection and built-in command layer`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated
  - validation notes updated
  - milestone committed
- Implementation notes:
  - Added a Core `FileSelectionController` for Explorer-style single, Ctrl toggle, Shift range, Ctrl+A, Escape, and arrow focus behavior over `ListedFileItem`.
  - Added a VeloFile-owned built-in command registry and keyboard router for R47-R53. The registry reports that it never enumerates Shell extensions.
  - Added copy path/name clipboard formatting through a Core `IClipboardTextWriter` seam and a Windows Unicode clipboard writer.
  - Wired WinUI context-menu items and file-command keyboard accelerators through `AppShellViewModel`; file-operation verbs route as command ids/placeholders until M8-M10 implement destructive behavior.
  - Review resolution now refreshes the visible WinUI context menu from Core command availability before opening and before command execution.
  - WinUI XAML uses `VirtualKey.Back` for the Backspace accelerator; this still maps to the V1 Backspace parent-folder command route.
- Risks: Command shortcuts can conflict with text input focus.
- Rollback/recovery: Command registry should allow disabling commands without destabilizing navigation.

### M7. Current-Folder Filter and Recursive Search

- Goal: Implement non-recursive current-folder filtering and explicit recursive filesystem search with streaming, cancellation, skipped-location reporting, and 10,000-result cap.
- Requirements: R28-R35, I4, EC6-EC7, EC26, AC4-AC5, QS-RESP-01.
- Files/components likely touched: `src/VeloFile.Core/Search/`, `src/VeloFile.Core/Filtering/`, `src/VeloFile.Windows/FileSystem/`, `src/VeloFile.App/Views/Search/`, `tests/*`.
- Dependencies: M4, M5, M6 for UI command wiring.
- Tests to add/update: substring filter, clearing filter, no recursive/indexed dependency for filter, explicit search start, streaming first results, cancellation before/after cap, result-limit state, permission failures, reparse loop avoidance.
- Implementation steps:
  - Add current-folder filter service over active listing.
  - Add recursive search service that walks from current folder without Windows Search.
  - Add streaming results and result cap behavior.
  - Add skipped-location/error reporting models.
  - Wire UI states and cancellation.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Filtering`
  - `dotnet test VeloFile.sln -c Debug --filter Search`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Users can instantly narrow current-folder items and run bounded cancellable recursive searches with clear cap/skipped states.
- Commit message: `M7: implement filter and recursive search`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Recursive search can accidentally follow loops or monopolize resources.
- Rollback/recovery: Search service remains cancellable and isolated from listing/navigation state.

### M8. File Operation Contracts, Safe Delete, and Rename

- Goal: Implement file-operation contracts, command routing, Shell-owned rename and Recycle Bin delete paths, permanent-delete confirmation flow, progress/failure state models, and undo/no-undo eligibility rules for this narrow operation set.
- Requirements: R36-R40, R43-R45, I1-I2, I9, S1, S5, EC12, AC6, QS-SAFE-DELETE-01, ADR 0002, ADR 0007.
- Files/components likely touched: `src/VeloFile.Core/Operations/`, `src/VeloFile.Windows/Shell/`, `src/VeloFile.App/Views/Operations/`, `tests/VeloFile.Core.Tests/`, `tests/VeloFile.Windows.Tests/`.
- Dependencies: M2 validation tooling, M3 diagnostics, M4 listing, M6 command layer.
- Tests to add/update: operation command validation, rename success/failure, Recycle Bin default, unsupported Recycle Bin fallback requiring permanent-delete confirmation, permanent-delete explicit gesture/confirmation, no undo for permanent delete, basic progress/completion/failure state tests.
- Implementation steps:
  - Define file-operation request/result/progress contracts and Shell operation adapter interface.
  - Implement rename and normal delete through the Shell-owned operation adapter.
  - Implement permanent-delete confirmation state and no-undo eligibility.
  - Add safe scratch-root operation fixtures to the compatibility corpus where needed before validation uses them.
  - Wire command-layer routes for rename, normal delete, and permanent delete.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Operations`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Rename and delete commands use the operation boundary, normal delete routes to Recycle Bin where supported, and permanent delete cannot occur without explicit confirmation.
- Commit message: `M8: implement file operation contracts and safe delete`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Real file operations can damage test data or user data.
- Rollback/recovery: Keep operation tests under generated scratch roots; disable destructive commands when target support is unknown.

### M9. Copy/Move, Conflicts, Progress, Cancellation, and Undo Eligibility

- Goal: Add Shell-owned copy/move behavior, conflict choices, batch conflict handling, progress/cancellation states, and session-scoped undo eligibility for move, rename, and Recycle Bin delete where supported.
- Requirements: R36-R46, R41-R45, R37, EC13-EC14, AC7, QS-SAFE-DELETE-01, ADR 0002.
- Files/components likely touched: `src/VeloFile.Core/Operations/`, `src/VeloFile.Windows/Shell/`, `src/VeloFile.App/Views/Operations/`, `tests/*`.
- Dependencies: M8.
- Tests to add/update: copy/move success, same-name collisions, skip/replace/keep-both choices, apply-to-batch behavior if implemented, cancellation after partial completion, progress/completion/failure states, undo eligibility for move/rename/Recycle Bin delete, no undo for permanent delete.
- Implementation steps:
  - Extend operation service for copy and move.
  - Add conflict resolution models and UI states.
  - Add progress and cancellation propagation from Shell adapter.
  - Add session-scoped undo eligibility tracking for supported recent operations.
  - Expand operations corpus with collisions and partial-completion fixtures before validation uses them.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Operations`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope operations -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Copy/move operations expose Windows-correct conflict, progress, cancellation, and undo-eligibility states without silently aborting unaffected work.
- Commit message: `M9: implement copy move conflicts and undo eligibility`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Batch operations can leave partially completed scratch data.
- Rollback/recovery: Corpus runners must clean scratch roots; operation state must preserve completed/failed item reporting.

### M10. Drag/Drop and Compatibility Corpus Expansion

- Goal: Implement Windows-correct drag/drop modifiers, resolved drop-action indicators, cross-app drag/drop boundaries, and expand the compatibility corpus for long paths, junctions, symlinks, reparse points, and drag/drop. File-association Open/Open With implementation remains in M14.
- Requirements: R46, R79-R80, EC24-EC25, AC9, ADR 0002, ADR 0007.
- Files/components likely touched: `src/VeloFile.Core/DragDrop/`, `src/VeloFile.Windows/DragDrop/`, `src/VeloFile.Windows/Shell/`, `tests/VeloFile.Compatibility.Tests/`, `tools/VeloFile.Corpus/`, `scripts/run-compat-corpus.ps1`.
- Dependencies: M2 validation tooling, M6 command layer, M8-M9 operation services.
- Tests to add/update: same-volume/cross-volume default action resolution, Ctrl/Shift/Ctrl+Shift modifiers, right-drag or menu path if supported, drop-action indicator, Explorer/browser/IDE/Office data-object boundaries where automatable, long path/junction/symlink/reparse behavior corpus.
- Implementation steps:
  - Add drag/drop domain models and OLE adapter boundary.
  - Wire resolved drop-action indicator into UI.
  - Integrate drop actions with file-operation service.
  - Expand compatibility corpus profiles for drag/drop and path edge cases.
  - Add manual compatibility checklist for external apps where automation is not stable.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter DragDrop`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Drag/drop follows Windows copy/move/shortcut conventions and compatibility corpus covers file-operation/path behaviors required before release.
- Commit message: `M10: implement drag drop and compatibility corpus`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Cross-process drag/drop can be hard to automate reliably.
- Rollback/recovery: Keep manual checklist separate from automated corpus and keep unsupported drop targets visibly rejected.

### M11. Preview Provider Contract, Metadata Fallback, and Timeout Harness

- Goal: Implement the preview provider contract, terminal states, metadata fallback/details data model, timeout harness, previous-preview clearing behavior, loading-state timing, cancellation/ignore semantics, diagnostics hooks, and non-mutation test harness before adding content providers.
- Requirements: R60, R67-R72, I3, S7, EC9, EC27, AC10-AC11, QS-PREVIEW-TIMEOUT-01, ADR 0004.
- Files/components likely touched: `src/VeloFile.Core/Preview/`, `src/VeloFile.Core/Metadata/`, `src/VeloFile.App/Views/Preview/`, `tests/VeloFile.Core.Tests/`, `scripts/run-preview-corpus.ps1`.
- Dependencies: M2 validation tooling, M3 diagnostics, M4 listing, M5 UI shell.
- Tests to add/update: provider interface states, unsupported/failed/loading/success transitions with fake providers, metadata fallback, previous preview cleared immediately, loading after 200 ms, cancellation visible within 50 ms, timeout harness, no source mutation test seam.
- Implementation steps:
  - Define preview request/result/provider interfaces and terminal states.
  - Implement metadata fallback and details data model without rich content providers.
  - Add timeout and cancellation/ignore orchestration.
  - Wire preview pane shell to contract states using fake/metadata providers.
  - Add diagnostics hooks for preview failures without raw paths/content.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter PreviewContract`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope contract -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Preview UI and services handle loading, unsupported, failed, metadata fallback, timeouts, and stale selection changes before content decoders exist.
- Commit message: `M11: add preview contract and metadata fallback`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Preview orchestration can accidentally show stale content.
- Rollback/recovery: Clear preview state on every selection change and fall back to metadata on provider failure.

### M12. Image, Text, and PDF Preview Providers

- Goal: Implement bounded image, text/code, and PDF first-page preview providers using the M11 contract.
- Requirements: R61-R67, R69-R72, I3, S7, EC10-EC11, AC10-AC11, ADR 0004.
- Files/components likely touched: `src/VeloFile.Windows/Preview/`, `src/VeloFile.Core/Preview/`, `tests/*`, `tools/VeloFile.Corpus/`.
- Dependencies: M11.
- Tests to add/update: image success, image >100 MB metadata-only, decoded dimensions >8192 by 8192 metadata-only, text first 1 MB with truncation, text >100 MB metadata-only, PDF first page, later-page user navigation, PDF >500 MB metadata-only, corrupt/access-denied/timeout failures, no source mutation.
- Implementation steps:
  - Implement image provider with size/dimension caps.
  - Implement bounded text/code provider and binary/refusal detection.
  - Implement PDF first-page provider with later-page navigation hook.
  - Expand preview corpus for image/text/PDF success and boundary cases before validation uses them.
  - Ensure provider diagnostics use allowed fields only.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter PreviewProviders`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope providers -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Supported images, bounded text/code, and PDFs render through bounded providers; oversized/corrupt/unsupported inputs fall back safely.
- Commit message: `M12: implement image text and PDF preview providers`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Native decode/render APIs can exceed expected time or memory budgets.
- Rollback/recovery: Provider timeout and metadata fallback from M11 remain the safe default.

### M13. Thumbnail, Icon, Details Pane, and Preview UI Concurrency

- Goal: Implement thumbnail/icon loading, details pane UI, preview UI polish, and thumbnail concurrency limits after content providers are stable.
- Requirements: R60, R67-R72, R78, A11Y1-A11Y6, AC10-AC11, AC19, QS-PREVIEW-TIMEOUT-01.
- Files/components likely touched: `src/VeloFile.Windows/Thumbnails/`, `src/VeloFile.Windows/Shell/`, `src/VeloFile.App/Views/Preview/`, `src/VeloFile.App/Views/FileList/`, `tests/*`.
- Dependencies: M4 listing, M5 UI shell, M11-M12 preview providers.
- Tests to add/update: thumbnail timeout <=500 ms per item, no more than 4 concurrent thumbnail operations, generic icon fallback, extension/icon cache behavior, details pane fields, accessibility states for loading/unsupported/failed/empty, hidden/protected visual distinction where applicable.
- Implementation steps:
  - Implement Shell icon and thumbnail adapters behind Windows boundary.
  - Add thumbnail work queue with concurrency limit and stale-work ignore.
  - Wire details pane and preview UI states.
  - Add accessibility-visible labels/states for preview/details outcomes.
  - Expand preview corpus for thumbnail/icon cases before validation uses them.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Thumbnails`
  - `dotnet test VeloFile.sln -c Debug --filter PreviewUi`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope thumbnails -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: File list thumbnails/icons and preview/details UI enrich navigation without blocking or violating concurrency/timeout caps.
- Commit message: `M13: implement thumbnails icons and preview UI concurrency`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Thumbnail work can reintroduce navigation stalls.
- Rollback/recovery: Disable thumbnail enrichment independently while keeping metadata/details fallback available.
- Progress notes (2026-05-05):
  - Implemented Core thumbnail state, provider result contracts, and a thumbnail controller with four-operation concurrency, per-item R67 timeout, stale-generation cancellation, and generic icon fallback.
  - Implemented the Windows thumbnail provider using Windows Storage thumbnail APIs with cached generic icon fallback by directory/extension class.
  - Bound the production file list to shell row view models that preserve `ListedFileItem` selection identity while exposing thumbnail state and hidden/protected dimming.
  - Kept row view models stable across thumbnail state updates so completion events do not replace visible rows and risk clearing selection.
  - Added preview/details UI state for accessibility names and complete metadata-field exposure.
  - Added `preview --scope thumbnails` corpus evidence for concurrency, timeout, fallback, and stale-result behavior.
  - Focused validation passed for Core/Windows/App/Corpus thumbnail and preview UI tests plus the standalone thumbnail corpus script.
  - Solution-level `Thumbnails` and `PreviewUi` filters passed. Full `scripts\ci.ps1` first caught a stale App contract assertion for preview metadata binding; after updating it to `DetailsMetadataFields`, CI passed with 0 build warnings/errors and 291 tests.
  - Review resolution fixed the non-cooperative provider timeout gap by racing visible thumbnail requests against the thumbnail budget while keeping timed-out provider tasks counted against live concurrency until actual completion.
  - Review resolution added an app-layer shell dispatcher so thumbnail completion events post to the UI dispatcher before row view-model mutation or binding notification; production app launch now passes the WinUI dispatcher into composition before thumbnail event subscription.
  - Review-resolution validation passed focused Core/App/Windows/Corpus thumbnail tests, direct thumbnail corpus execution, solution `Thumbnails` and `PreviewUi` filters, `dotnet build`, `dotnet test --no-build`, and `scripts\ci.ps1`.
  - Second-pass review found the live thumbnail provider gate was still generation-local. The controller now owns one long-lived provider gate, so timed-out non-cooperative work from old generations remains counted until actual provider completion.

### M14. Terminal Launch and File Association Open

- Goal: Implement Open, Open With, Open terminal here, terminal discovery, safe launch, and user-visible launch failures.
- Requirements: R54-R59, R81-R82, S1-S2, EC18-EC19, AC12, ADR 0005.
- Files/components likely touched: `src/VeloFile.Core/Terminal/`, `src/VeloFile.Core/FileAssociations/`, `src/VeloFile.Windows/Terminal/`, `src/VeloFile.Windows/ShellExecute/`, `src/VeloFile.App/Views/Settings/`, `tests/*`.
- Dependencies: M3 diagnostics, M6 command layer.
- Tests to add/update: terminal discovery ordering, Git Bash/WSL selectable behavior, discovery does not block launch, shell-metacharacter paths, unavailable terminal errors, inaccessible working directory errors, file association open respects defaults, Open With does not modify associations.
- Implementation steps:
  - Add terminal target discovery and settings persistence.
  - Add safe process-launch adapter using working directory/structured arguments.
  - Add file association and Open With adapter.
  - Wire command layer and settings UI.
  - Add diagnostic events that record selected terminal identity but no raw command text.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter Terminal`
  - `dotnet test VeloFile.sln -c Debug --filter FileAssociations`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Users can explicitly open a configured terminal or open files through Windows associations without command injection or state corruption.
- Commit message: `M14: implement safe terminal and file association integration`
- Milestone closeout:
  - validation passed: `Terminal`, `FileAssociations`, build, full solution tests, and `scripts\ci.ps1`
  - progress updated: Core, App, Windows, test, persistence, and change-local docs are updated
  - decision log updated: selected terminal target is persisted in the existing settings payload; discovery is lazy and not invoked by app shell construction
  - validation notes updated: see M14 implementation notes below
  - milestone committed: `M14: implement safe terminal and file association integration`
- Risks: Argument handling differs by terminal target.
- Rollback/recovery: Failed launch leaves browsing state unchanged and records only redacted diagnostics.
- M14 implementation notes:
  - Added Core terminal discovery/launch services with default ordering for Windows Terminal, PowerShell 7, Windows PowerShell, and Command Prompt; Git Bash and WSL distributions remain selectable when available.
  - Added Windows terminal discovery/probing and process launch adapters that pass folder paths as working-directory/argument data, not concatenated shell command text.
  - Added Open/Open With Core and Windows ShellExecute boundaries. Open With uses the `openas` verb and association launch requests never modify system associations.
  - Wired `Open`, `OpenWith`, and `OpenTerminalHere` through `AppShellViewModel.ExecuteBuiltInCommandAsync`; double-click routes to Open; launch failures update visible shell status while preserving browsing state.
  - Added a lazy terminal target selector in the shell and persisted the selected terminal target through `SettingsStatePayload`.
  - Added terminal launch diagnostics that record terminal identity and result state without recording active paths or command text.
- M14 validation notes:
  - `dotnet test VeloFile.sln -c Debug --filter Terminal` passed 9 Core, 7 App, and 3 Windows terminal tests.
  - `dotnet test VeloFile.sln -c Debug --filter "Terminal|Diagnostics"` passed 17 Core, 7 App, and 3 Windows tests after adding the terminal launch diagnostic field allowlist.
  - `dotnet test VeloFile.sln -c Debug --filter FileAssociations` passed 3 Core, 7 App, and 3 Windows file-association tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --no-build` passed 321 tests across App, Core, Windows, and Corpus test assemblies.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed `dotnet --info`, restore, build with 0 warnings and 0 errors, and 321 tests across 4 test assemblies.
  - Review-resolution validation for terminal diagnostic reason-code allowlist drift passed `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter "Terminal|Diagnostics"` with 19 Core tests and `dotnet test VeloFile.sln -c Debug --filter "Terminal|Diagnostics"` with 19 Core, 7 App, and 3 Windows tests.

### M15. Benchmark Harness, Accessibility Checks, and Release Triage

- Goal: Promote the M2 validation foundations into app-level benchmarks, full benchmark reports, accessibility checks, diagnostics conformance gates, and preview-release triage threshold support.
- Requirements: P1-P16, O5-O7, R46, R83-R89, AC9, AC15, AC17, AC19, ADR 0003, ADR 0008, QS-RESP-01, QS-SLOW-TAB-01, QS-DIAG-PRIV-01.
- Files/components likely touched: `tools/VeloFile.Corpus/`, `tools/VeloFile.Benchmarks/`, `tests/VeloFile.Compatibility.Tests/`, `tests/VeloFile.Accessibility.Tests/`, `scripts/run-benchmarks.*`, `scripts/run-compat-corpus.*`, `docs/release/preview-triage.md`.
- Dependencies: M2 validation tooling, M4-M14 enough app behavior to measure and triage.
- Tests to add/update: generated small/medium/large/deep/preview/pathological corpora, performance measurement reporting median/p95/p99/environment, compatibility corpus, diagnostics conformance, accessibility keyboard/focus/state checks.
- Implementation steps:
  - Extend deterministic corpus generator to full small/medium/large/deep/pathological profiles.
  - Replace non-gating benchmark stub with app-level benchmark harness that drives app process and writes reports.
  - Finalize compatibility corpus aggregation for file operations, drag/drop, long paths, junctions, reparse points, associations, DPI.
  - Add diagnostics conformance and retention/export checks.
  - Define preview-release triage threshold document owned by release policy.
- Validation commands:
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -ScratchRoot <scratch-root>`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope smoke -ScratchRoot <scratch-root>`
  - `dotnet test VeloFile.sln -c Debug --filter Accessibility`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Release readiness can be judged using generated corpus, benchmark reports, diagnostics thresholds, and compatibility checks.
- Commit message: `M15: add benchmarks accessibility and release triage gates`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Benchmarks may be noisy or unsuitable for hosted CI.
- Rollback/recovery: Keep benchmark gates separate from unit CI until corpus and baseline machine class are documented; preserve non-gating mode for contributor machines.

### M16. MSIX Packaging, Docs, Release Notes, and V1 Hardening

- Goal: Finish signed MSIX packaging path, stable update channel docs, rollback/uninstall instructions, user docs, release notes, and final V1 acceptance pass.
- Requirements: R3, R90-R93, C2, C9, S6, AC1, AC16, AC18, AC19, QS-MSIX-ROLLBACK-01.
- Files/components likely touched: `packaging/msix/`, `src/VeloFile.App/Package.appxmanifest`, `.github/workflows/release.yml`, `scripts/release-verify.*`, `docs/release/`, `docs/user/`, `README.md`, `SECURITY.md`.
- Dependencies: M1-M15.
- Tests to add/update: package build smoke, install/update/uninstall manual checklist, release verification script, docs link checks if available, extension-display release note tests/checks.
- Implementation steps:
  - Add MSIX manifest/package configuration.
  - Define signing identity and stable update channel documentation.
  - Replace template release verification with product checks.
  - Write "Differences from File Explorer" docs and extension-display release note.
  - Run full acceptance matrix against spec and architecture quality scenarios.
- Validation commands:
  - `dotnet publish src/VeloFile.App/VeloFile.App.csproj -c Release`
  - `pwsh scripts/package-msix.ps1`
  - `pwsh scripts/release-verify.ps1`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
  - Manual: install, launch, update/rollback where available, uninstall, confirm Explorer and file associations remain usable.
- Expected observable result: V1 can be packaged, documented, verified, and rolled back without taking ownership of Explorer or system associations.
- Commit message: `M16: package and document VeloFile V1 release path`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Signing and MSIX update infrastructure may not be available on every contributor machine.
- Rollback/recovery: Keep unsigned local packaging separate from signed release packaging; uninstall remains the product rollback path.

## Validation Plan

Before implementation starts:

- `plan-review` must approve this plan or produce revisions.
- `specs/v1-product-scope.test.md` must be reviewed.
- The first implementation milestone must define the real CI command in `AGENTS.md`.
- No milestone may require a validation command unless the script, corpus profile, and fixtures exist from a prior milestone or are created in that same milestone before use.

Current available commands:

- `dotnet restore VeloFile.sln`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` as a local fallback only when `pwsh` is unavailable
- `scripts/ci.sh` wraps `scripts/ci.ps1` when the Bash environment can invoke PowerShell.

Expected post-M1 core commands:

- `dotnet restore VeloFile.sln`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` by default; GitHub Actions also invokes `scripts/ci.ps1` under `pwsh`. Use `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` only as a local fallback when `pwsh` is unavailable.

Expected later commands:

- `pwsh scripts/generate-corpus.ps1` created in M2
- `pwsh scripts/run-compat-corpus.ps1` created in M2 and expanded in M10/M15
- `pwsh scripts/run-preview-corpus.ps1` created in M2 and expanded in M11-M13
- `pwsh scripts/run-benchmarks.ps1` created as a non-gating stub in M2 and promoted in M15
- `pwsh scripts/package-msix.ps1`
- `pwsh scripts/release-verify.ps1`

Each milestone must update validation notes with the commands actually run and any manual checks performed. Do not report a milestone as done if validation is skipped without a recorded reason.

## Risks and Recovery

- Empty source baseline: M1 must keep source layout conservative and testable so later milestones do not pile behavior into the UI project.
- Validation asset ordering risk: M2 must establish scripts, scratch-root safety, and minimal profiles before later milestones rely on corpus commands.
- Shell behavior risk: all Shell/Win32/WinRT/OLE access stays behind `VeloFile.Windows`; unsafe direct interop outside that boundary requires architecture review.
- Data-loss risk: file operations must use generated scratch corpora during testing and keep permanent delete behind explicit confirmation.
- Persistence corruption risk: M3 fault-injection tests must pass before session restore or settings depend on persistence.
- Diagnostics privacy risk: diagnostics allowlist/prohibited-field tests must exist before workflows produce diagnostic logs.
- Performance risk: no public performance claims until M15 corpus and benchmark reports exist.
- MSIX risk: unsigned local packaging and signed release packaging should remain separate so contributors can validate without signing credentials.
- Rollback: every milestone is one coherent commit and can be reverted independently before dependent milestones land.

## Dependencies

- M1 blocks all implementation milestones.
- M2 blocks any milestone that invokes generated corpus, compatibility, preview corpus, or benchmark scripts.
- M3 blocks session restore, diagnostics, benchmark triage, and settings-dependent UI.
- M4 blocks navigation UI, filtering/search, preview item selection, and most performance work.
- M5 and M6 block realistic end-to-end workflows.
- M8-M10 split file-operation safety, copy/move behavior, and drag/drop compatibility; M10 must land before final Explorer parity validation.
- M11-M13 split preview contracts, content providers, and thumbnail/details UI; M11 must land before provider implementation.
- M15 depends on enough app behavior to benchmark and enough diagnostics to triage.
- M16 depends on all V1 behaviors and release-readiness scripts.
- `plan-review` and `test-spec` are required before implementation.

## Progress

- [x] Proposal accepted.
- [x] Spec approved.
- [x] Architecture approved.
- [x] Execution plan drafted.
- [x] Plan reviewed.
- [x] Test spec created.
- [x] Test spec active.
- [x] M1 unblocked in local toolchain.
- [x] M1 complete.
- [x] M2 complete.
- [x] M3 complete.
- [x] M4 complete.
- [x] M5 complete.
- [x] M6 complete.
- [x] M7 complete.
- [x] M8 complete.
- [x] M9 complete.
- [x] M10 complete.
- [x] M11 complete.
- [x] M12 complete.
- [x] M13 complete.
- [ ] M14 complete.
- [ ] M15 complete.
- [ ] M16 complete.
- [ ] V1 verified.

## Decision Log

| Date | Decision | Reason |
|---|---|---|
| 2026-05-04 | Use a single V1 execution plan with sixteen milestones. | The approved V1 scope spans app foundation, validation tooling, core services, Shell interop, persistence, diagnostics, benchmarks, and packaging; milestone slices keep each reviewable. |
| 2026-05-04 | Treat M1 as source/CI foundation, not feature behavior. | The repository has no app source or real CI yet, so feature implementation needs a verified base first. |
| 2026-05-04 | Normalize architecture status to `approved` before plan creation. | Architecture review approved the package and required status normalization before planning relies on it. |
| 2026-05-04 | Mark this plan active once the test spec exists. | The repository has no separate test-spec review stage; implementation relies on the active test spec as the proof surface. |
| 2026-05-04 | Add M2 for validation tooling and minimal corpus foundations. | Plan review found later milestones invoked corpus scripts before they existed. |
| 2026-05-04 | Split file-operation work into M8-M10. | Safe delete, copy/move/conflicts, and drag/drop compatibility are safety-sensitive and need separate review loops. |
| 2026-05-04 | Split preview work into M11-M13. | Preview contracts, content providers, and thumbnail/details UI have different risks and validation needs. |
| 2026-05-04 | Keep the checked-in solution as `VeloFile.sln`. | The .NET 10 SDK created `.slnx` by default, but the approved plan and validation commands name `VeloFile.sln`. |
| 2026-05-04 | Use PowerShell 7 (`pwsh`) as the default CI script host, with Windows PowerShell as a local fallback. | `pwsh` matches GitHub Actions and current PowerShell best practice; the fallback keeps M1 runnable on contributors' Windows machines before PowerShell 7 is installed. |
| 2026-05-04 | Put corpus generation and runner dispatch in `tools/VeloFile.Corpus`. | Keeping scratch-root validation and deterministic corpus writes in one testable console tool prevents script-only logic from diverging across runners. |
| 2026-05-05 | Keep file-association Open/Open With implementation in M14 while M10 covers drag/drop and path compatibility. | The plan text previously mentioned R81-R82/I8 near M10, but the test spec maps those requirements to T026 and M14 explicitly owns ShellExecute/file-association adapters; merging them into M10 would collapse two planned review slices. |
| 2026-05-04 | Make M2 benchmark output non-gating with null timing values. | ADR 0003 and P16 prohibit public performance claims before the benchmark harness and reference corpus exist; M2 only establishes report shape. |
| 2026-05-04 | Split M3 persistence decisions between Core schema/recovery and Windows file replacement. | Core owns durable document contracts and recovery policy; Windows owns same-directory temp, flush, and atomic replacement behavior. |
| 2026-05-04 | Split M4 listing between Core state/projection and Windows file-system adapters. | Core owns virtualization-ready listing state, visibility projection, and stale request gating; Windows owns `DirectoryInfo`, `FileSystemInfo`, and `DriveInfo` access. |
| 2026-05-04 | Split M5 navigation/session behavior into pure Core state services plus a compiled WinUI shell surface. | Core can prove tabs, sidebar, breadcrumb, restore, visibility, and missing-path behavior without a UI automation harness; the WinUI app still exposes the required shell regions for later command wiring. |
| 2026-05-04 | Resolve M5 review by adding a Core shell command surface and an app launch composition root. | The review found static shell controls and hardcoded launch state; navigation UI routes now converge on Core command methods, and launch restore reads durable state before creating the shell view model. |
| 2026-05-05 | Implement M6 command behavior as Core selection/command services plus thin WinUI routes. | Core tests can prove Explorer-style selection, command availability, keyboard routing, and clipboard formatting without a UI automation harness; WinUI exposes the built-in menu and accelerators without Shell extension enumeration. |

## Surprises and Discoveries

- `docs/plan.md` and `docs/plans/` were absent when planning started; this plan restores the required plan index and creates the first real plan body.
- `scripts/ci.sh` and `scripts/release-verify.sh` are template placeholders, so M1 and M16 must replace them with product-specific commands.
- No `CONSTITUTION.md` or `docs/project-map.md` exists yet. The V1 test spec has been created and marked active.
- First-pass `plan-review` found sequencing and milestone-size issues; this revision adds validation tooling early and splits file-operation and preview work.
- M1 was initially blocked because the installed `dotnet` host had runtimes but no SDK. After the environment was updated, `dotnet --info` reports SDKs `9.0.313` and `10.0.203`.
- `dotnet new list winui` does not list WinUI templates, but Visual Studio has WinUI C# templates installed and equivalent checked-in project files build successfully with `Microsoft.WindowsAppSDK` `1.7.250401001`.
- PowerShell 7 (`pwsh`) is now installed on PATH and validated with the M1 CI script. The M1 CI script remains compatible with Windows PowerShell as a fallback.
- `bash scripts/ci.sh` was smoke-tested from WSL Bash before `pwsh` was installed and could not run there because that Bash environment could not invoke Windows PowerShell. This is not the M1 gating command; local validation now uses `pwsh` directly.
- M2 corpus tests initially ran in parallel and contended on the generated corpus tool build output. `tests/VeloFile.Corpus.Tests` disables parallelism so script smoke tests execute like normal command-line validation.
- M2 scratch-root safety uses a marker file plus path-leaf requirements: the root must be absolute, dedicated to VeloFile corpus work, and either empty/new or already marked with `.velofile-corpus-root`.
- M2 scratch-owned `DOTNET_CLI_HOME` caused current .NET SDK first-run behavior to append per-run scratch `.dotnet\tools` paths to the persistent User PATH. `Invoke-CorpusTool.ps1` now sets `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` for corpus tool invocations.
- M3 implementation found that UI session restore and numeric preview-release thresholds are later milestone concerns. M3 supplies durable schema/recovery and local diagnostic primitives that M5 and M15 consume.
- M4 implementation found that `scripts/select-validation.py` is still absent, so M4 used the plan-specified `dotnet test` filters and `scripts/ci.ps1` directly rather than selector-selected checks.
- M4 keeps WinUI folder-open entry points, breadcrumb/sidebar rendering, view-mode controls, protected-system-file confirmation UI, thumbnails, icons, and nonessential metadata enrichment out of the listing hot path for later milestones.
- M4 review found two correctness gaps: request-token tests did not prove slow-tab isolation, and drive free-space hints were loaded synchronously. M4 now has a tab listing coordinator with per-tab cancellation/stale-result protection and a drive hint enrichment service that keeps entries separate from timeout-bounded hints.
- M5 found that WinUI `Window` itself does not expose `DataContext`; the app shell view model is attached to the root XAML grid instead.
- M5 app-shell tests are file-based contract checks until a later UI automation harness exists. They verify shell regions, command/event routes, keyboard accelerator invoked handlers, and app composition wiring without launching WinUI inside MSTest.
- M5 review resolution found that build/test commands share `obj` outputs; running filtered `dotnet test` commands in parallel can trip file locks, so validation commands that build the solution should run sequentially.
- M6 confirmed the same build-output lock behavior when two filtered `dotnet test` commands were started in parallel. M6 validation used sequential commands after the expected red compile failures were captured.
- WinUI `KeyboardAccelerator.Key` uses `Windows.System.VirtualKey`; Backspace must be represented as `Key="Back"` in XAML while still routing to the V1 Backspace parent-folder behavior.

## Validation Notes

- Planning-time reads: `AGENTS.md`, accepted proposal, approved spec, approved architecture, ADRs 0001-0008, README, CONTRIBUTING, CI/release scripts, GitHub workflows, repository file list.
- No product validation commands were run because no product source or test project exists yet.
- Current template command available but not sufficient for V1: `bash scripts/ci.sh`.
- Applied first-pass `plan-review` revisions: added M2, split original file-operation milestone into M8-M10, and split original preview milestone into M11-M13.
- Second-pass `plan-review` approved the plan. Created `specs/v1-product-scope.test.md` as the active V1 proof surface.
- Corrected the lifecycle wording to remove the non-existent test-spec review gate.
- M1 initial blocker check:
  - `dotnet --info` reports no .NET SDKs installed.
  - `dotnet new list winui` fails because no .NET SDK is installed.
  - Visual Studio MSBuild 17.14 is present at `C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe`.
  - `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Sdks\Microsoft.NET.Sdk` is missing.
  - No product scaffold was created because M1 cannot meet its test-first and validation requirements without the SDK/tooling dependency.
- M1 test-first implementation evidence:
  - Added smoke tests before production foundation files.
  - `dotnet test VeloFile.sln -c Debug --no-restore` failed as expected for missing `VeloFile.Core.Foundation`, missing `VeloFile.Windows.Foundation`, and missing WinUI app project/XAML files.
- M1 final validation:
  - `dotnet --info` passed with SDK `10.0.203`.
  - `dotnet restore VeloFile.sln` passed.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug` passed: 5 tests across 3 test assemblies.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and test all green.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` also passed as a fallback route before `pwsh` was installed.
  - Launch smoke passed: `src\VeloFile.App\bin\x86\Debug\net8.0-windows10.0.19041.0\VeloFile.App.exe` started and stayed alive for 2 seconds before being stopped.
  - `scripts/select-validation.py` is not present in M1; selector-based validation is therefore not available yet.
- M2 test-first implementation evidence:
  - Added `tests/VeloFile.Corpus.Tests` before the corpus scripts and tool existed.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug` failed as expected because `scripts/generate-corpus.ps1` and `scripts/run-benchmarks.ps1` were missing.
- M2 final validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug` passed: 4 tests.
  - `dotnet test VeloFile.sln -c Debug --filter Corpus` passed: 4 corpus tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>` passed.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope smoke -ScratchRoot <scratch-root>` passed.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -ScratchRoot <scratch-root>` passed.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -ScratchRoot <scratch-root>` passed and wrote `benchmarks\benchmark-smoke-report.json`.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: 9 tests across 4 test assemblies.
  - `scripts/select-validation.py` is still not present; M2 used the plan-specified validation commands directly.
- M2 corpus tooling review-resolution evidence:
  - Updated T004 tests first; `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug` failed as expected because wrappers did not yet support `-ScratchRoot`.
  - Added scratch-owned tool invocation so script builds, MSBuild intermediates, NuGet caches, `DOTNET_CLI_HOME`, `TEMP`, and `TMP` stay under `<scratch-root>/.velofile-tools`.
  - Added deterministic placeholder generation for `smoke`, `operations`, `preview`, `search`, and `large-folder` under `<scratch-root>/corpora/<profile>`.
  - Added `tests/validation/CorpusScriptsIsolation.Tests.ps1` to prove script execution does not create, modify, or delete repository-side files.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File tests/validation/CorpusScriptsIsolation.Tests.ps1 -ScratchRoot <scratch-root>` passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>` passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile operations -ScratchRoot <scratch-root>` passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile preview -ScratchRoot <scratch-root>` passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile search -ScratchRoot <scratch-root>` passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile large-folder -ScratchRoot <scratch-root>` passed.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug` passed: 4 tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: 9 tests across 4 test assemblies.
- M2 corpus PATH pollution bugfix evidence:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Corpus_tool_wrapper_disables_dotnet_global_tools_path_mutation` failed before the wrapper set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`, then passed after the fix.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Corpus_scripts_do_not_add_scratch_dotnet_tools_to_user_path` passed and proved `generate-corpus.ps1` does not add scratch `.dotnet\tools` paths to persistent User PATH.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug` passed: 6 tests.
- M3 test-first implementation evidence:
  - Added persistence, diagnostics, and Windows storage tests before production namespaces existed.
  - `dotnet test VeloFile.sln -c Debug --filter "Persistence|Diagnostics"` failed as expected because `VeloFile.Core.Persistence`, `VeloFile.Core.Diagnostics`, and `VeloFile.Windows.Storage` were missing.
- M3 final validation:
  - `dotnet test VeloFile.sln -c Debug --filter Persistence` passed: 7 tests across Core and Windows test assemblies.
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` passed: 4 Core diagnostics tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and 20 tests across 4 test assemblies.
- M3 review-resolution evidence:
  - Added regression tests first for recoverable persistence read failures, durable codecs for settings/favorites/recent locations, session window-placement round trip and malformed fallback, best-effort diagnostic storage failures, and diagnostic string sanitization.
  - `dotnet test VeloFile.sln -c Debug --filter Persistence` initially failed as expected because `DurableDocumentStorageReadResult`, `ReadText`, and local-state codecs were not implemented.
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` initially failed behind the same missing persistence compile surface before diagnostics production fixes existed.
  - `dotnet test VeloFile.sln -c Debug --filter Persistence` passed: 10 tests across Core and Windows test assemblies.
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` passed: 6 Core diagnostics tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and 25 tests across 4 test assemblies.
  - Second review-resolution pass added regression tests for deny-by-default diagnostic field policies, generated-id validation, dangerous values across every serialized diagnostic string field, repository-level per-field fallback diagnostics, and diagnostic-write failure during successful persistence reads.
  - `dotnet test VeloFile.sln -c Debug --filter Persistence` passed: 14 tests across Core and Windows test assemblies.
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` passed: 7 Core diagnostics tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and 30 tests across 4 test assemblies.
  - Third review-resolution pass added regression proof that denied diagnostic strings do not use predictable unsalted SHA-based redaction tokens; denied arbitrary strings now serialize as the non-correlating constant token `redacted-string`.
  - `dotnet test VeloFile.sln -c Debug --filter Persistence` passed: 14 tests across Core and Windows test assemblies.
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` passed: 7 Core diagnostics tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and 30 tests across 4 test assemblies.
- M4 test-first implementation evidence:
  - Added listing, visibility, and Windows file-system tests before production namespaces existed.
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Listing|TestCategory=Visibility"` failed as expected because `VeloFile.Core.Listing`, `VeloFile.Core.Visibility`, and `VeloFile.Windows.FileSystem` were missing.
- M4 final validation:
  - `dotnet test VeloFile.sln -c Debug --filter Listing` passed: 7 Core listing tests and 4 Windows listing/file-system tests.
  - `dotnet test VeloFile.sln -c Debug --filter Visibility` passed: 4 Core visibility tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 45 tests across 4 test assemblies.
- M4 review-resolution evidence:
  - Added regression tests first for direct concurrent slow-vs-healthy tab isolation, stale slow completion after same-tab navigation, tab close cancellation, slow/hanging drive hints, stale drive hint generations, hint failures, and hint cancellation.
  - `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter "Slow_tab_listing|Drive_entries_are_returned"` failed first because `FolderListingCoordinator`, `DriveEntryService`, `IDriveHintSource`, `DriveHint`, and `DriveHintStatus` did not exist.
  - `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter "Slow_tab_listing|Stale_slow_listing|Closing_tab_cancels|Drive_entries_are_returned|Slow_hint_completion|Hint_failure|Cancelling_hint"` passed: 8 tests.
  - Second review-resolution pass added direct proof for live underlying drive hint read caps and cancellation-ignoring stale listing completion/failure.
  - `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter "Timed_out_non_cancelling_hints|Cancellation_ignoring_old_listing"` initially failed because timed-out hint reads released capacity before underlying reads completed, then passed after live-read slot tracking.
  - `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter Listing` passed: 4 tests.
  - `dotnet test VeloFile.sln -c Debug --filter Listing` passed: 18 Core listing tests and 4 Windows listing/file-system tests.
  - `dotnet test VeloFile.sln -c Debug --filter Visibility` passed: 4 Core visibility tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 58 tests across 4 test assemblies.
- M5 test-first implementation evidence:
  - Added navigation, sidebar, session restore, visibility-settings, and app shell contract tests before production M5 namespaces and shell regions existed.
  - `dotnet test VeloFile.sln -c Debug --filter "Navigation|Session|Sidebar"` failed as expected because `VeloFile.Core.Navigation`, `VeloFile.Core.Sidebar`, `VeloFile.Core.Session`, and the app shell contract regions were missing.
- M5 final validation:
  - `dotnet test VeloFile.sln -c Debug --filter Navigation` passed: 6 Core navigation tests and 2 App shell contract tests.
  - `dotnet test VeloFile.sln -c Debug --filter Session` passed: 8 Core session/persistence restore tests.
  - `dotnet test VeloFile.sln -c Debug --filter Sidebar` passed: 3 Core sidebar tests and 1 App shell contract test.
  - `dotnet test VeloFile.sln -c Debug --filter Visibility` passed: 7 Core visibility tests and 1 App shell contract test.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 73 tests across 4 test assemblies.
- M5 review-resolution evidence:
  - Added regression tests first for shell command routes, startup restore, crash recovery start-fresh, missing-path restore reaching the shell, empty session restore, and close-last-tab behavior.
  - `dotnet test VeloFile.sln -c Debug --filter "Navigation|Session"` failed first as expected because `VeloFile.Core.Shell`, `IDefaultLaunchPathProvider`, and WinUI command routes were missing.
  - `dotnet test VeloFile.sln -c Debug --filter "Navigation|Session"` passed: 19 Core tests and 3 App shell contract tests.
  - `dotnet test VeloFile.sln -c Debug --filter Navigation` passed: 9 Core navigation tests and 3 App shell contract tests.
  - `dotnet test VeloFile.sln -c Debug --filter Session` passed: 11 Core session/startup restore tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` initially failed because the App composition contract test expected explicit typed repositories while the composition root hid them behind a generic helper; the composition root was made explicit.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 85 tests across 4 test assemblies.
- M6 test-first implementation evidence:
  - Added selection, command registry, keyboard routing, clipboard, Windows clipboard boundary, and App shell contract tests before production M6 namespaces existed.
  - `dotnet test VeloFile.sln -c Debug --filter Selection` and `dotnet test VeloFile.sln -c Debug --filter Commands` first failed as expected because `VeloFile.Core.Selection`, `VeloFile.Core.Commands`, and `VeloFile.Windows.Clipboard` were missing. The first attempt also hit shared-output file locks because the two build-producing test commands ran concurrently.
- M6 final validation:
  - `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests.
  - `dotnet test VeloFile.sln -c Debug --filter Selection` passed: 8 Core selection/navigation-category tests.
  - `dotnet test VeloFile.sln -c Debug --filter Clipboard` passed: 4 Core clipboard tests and 1 Windows clipboard boundary test.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 8 App shell contract tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 126 tests across 4 test assemblies.
- M6 review-resolution evidence:
  - First-pass `code-review` found that the visible WinUI context menu bypassed command availability even though Core availability was correct.
  - Added a shell contract assertion first for the menu opening route and `ViewModel.IsBuiltInCommandAvailable`.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 8 App shell contract tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 126 tests across 4 test assemblies.
  - Second-pass `code-review` found no blocking or required-change findings and recommended `verify`.
  - Third-pass `code-review` found two M6 app-shell blockers: production file-list rows were static placeholders that did not populate `SelectedFileItems`, and WinUI file-command accelerators did not pass text-input focus context into the Core keyboard router.
  - Added app-shell tests first for selected `ListedFileItem` copy path/name behavior, row/container selection mapping, and accelerator suppression when text input has focus.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` first failed for missing app-shell input bridge types, then passed: 16 App shell contract/route tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests and 3 App command-route tests.
  - `dotnet test VeloFile.sln -c Debug --filter Selection` passed: 8 Core selection/navigation-category tests and 2 App selection tests.
  - `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter Slow_hint_completion_updates_only_matching_generation` first exposed an existing test setup conflict with the live-underlying-read cap, then passed after correcting the stale-generation test's hint concurrency setup.
  - `dotnet test VeloFile.sln -c Debug` passed: 134 tests across 4 test assemblies.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 134 tests across 4 test assemblies.
  - Fourth-pass `code-review` found that production `FileItems` still had no listing feed and multi-selection ordering followed selected-item enumeration instead of current view order.
  - Wired `AppCompositionRoot` to create a `FolderListingCoordinator` over `WindowsFolderEntrySource`, and updated `AppShellViewModel` to refresh active-tab listing on startup, accepted navigation, refresh, tab lifecycle, tab switching, Start Fresh, and visibility-setting changes.
  - Updated selection mapping to use current visible `FileItems` order and ignore stale selected rows.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 23 App shell contract/route tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter Commands` passed: 14 Core command tests and 3 App command-route tests.
  - `dotnet test VeloFile.sln -c Debug --filter Selection` passed: 8 Core selection/navigation-category tests and 6 App selection tests.
  - `dotnet test VeloFile.sln -c Debug --filter Listing` passed: 19 Core listing tests, 4 Windows listing tests, and 1 App listing-route test.
  - `dotnet test VeloFile.sln -c Debug` passed: 141 tests across 4 test assemblies.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 141 tests across 4 test assemblies.
- M6 follow-up review evidence:
  - Follow-up `code-review` for commit `9931839` returned `clean-with-notes`; no blocking or required-change findings remained for the listing-feed and view-ordered selection fixes.
- M7 test-first implementation evidence:
  - Added Core filtering/search tests and App shell route tests before production M7 namespaces and methods existed.
  - `dotnet test VeloFile.sln -c Debug --filter "Filtering|Search"` first failed because `VeloFile.Core.Filtering`, `VeloFile.Core.Search`, recursive search updates, and app view-model filter/search methods were missing.
  - The first implementation pass then exposed an app cancellation test race; the test seam was corrected to keep recursive search live while cancellation is asserted.
  - Added `CurrentFolderFilterService`, recursive search models/service, app-shell filter/search controls, app view-model filter/search state, and production composition through `RecursiveSearchService`.
- M7 validation:
  - `dotnet test VeloFile.sln -c Debug --filter "Filtering|Search"` passed: 7 Core filtering/search tests and 5 App filtering/search tests.
  - `dotnet test VeloFile.sln -c Debug --filter Filtering` passed: 3 Core filtering tests and 2 App filtering tests.
  - `dotnet test VeloFile.sln -c Debug --filter Search` passed: 6 Core search tests and 10 App search tests after default-cap review-resolution fixes.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 34 App shell contract/route tests after default-cap review-resolution fixes.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 160 tests across 4 test assemblies.
  - A parallel `Filtering`/`Search` validation attempt produced a transient file-lock on the `Filtering` build output; the same `Filtering` selector passed when rerun sequentially.
- M7 review-resolution evidence:
  - Added `VisibleItems` display mode so the file list switches from folder rows to recursive search rows while search is active, completed, cancelled, or capped.
  - Added skipped-location count/details and clear-search shell wiring.
  - Added direct App tests for visible streamed search rows, skipped-location details, cap-to-new-query replacement, stale old-run update suppression, and clear search returning to current-folder rows.
  - Added direct Core and App route tests proving the V1 default recursive search result cap is 10,000.
- M7 follow-up review evidence:
  - Follow-up `code-review` for M7 returned `clean-with-notes`; no blocking or required-change findings remained for current-folder filtering and recursive search.
- M8 test-first implementation evidence:
  - Added Core operation service tests, Windows shell-operation mapper tests, App command-route/visible-state tests, and safe-delete corpus runner proof before production operation contracts existed.
  - `dotnet test VeloFile.sln -c Debug --filter Operations` first failed because `VeloFile.Core.Operations` and `VeloFile.Windows.Shell` did not exist.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>` first failed while the new corpus scope was being wired, then passed after the safe-delete fixture checks matched the generated operations profile.
  - Added `FileOperationService`, operation request/progress/result/confirmation/undo-eligibility models, Windows shell-operation adapter and request mapper, app command routing for rename/delete/permanent-delete, visible operation status, permanent-delete confirmation UI, and safe-delete corpus scope support.
- M8 validation:
  - `dotnet test VeloFile.sln -c Debug --filter Operations` passed: 7 Core operation tests, 3 Windows shell-operation tests, and 5 App operation shell/route tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>` passed.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug` passed: 6 corpus tooling tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 39 App shell contract/route tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 175 tests across 4 test assemblies.
- M8 review-resolution:
  - `dotnet test VeloFile.sln -c Debug --filter Operations` first failed after adding regressions for missing production Recycle Bin classification, missing rename commit/cancel shell route, and missing in-flight operation cancellation; final run passed: 9 Core operation tests, 6 Windows shell-operation tests, and 10 App operation shell/route tests.
  - Added a production Windows recycle-capability seam. Known unsupported Recycle Bin targets, including UNC paths, now return `RecycleBinUnavailable` before any delete executor call; ambiguous failures remain normal failed operations and do not enter destructive fallback.
  - Added visible rename commit/cancel shell state and invalid-name recovery. F2/context Rename starts rename mode; Enter/button commit calls the operation boundary; Escape/button cancel avoids adapter calls.
  - Added cancellable operation state for adapters that support it, plus a shell Cancel operation route to the retained in-flight cancellation token.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 44 App shell contract/route tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>` first failed because the scratch-root leaf did not contain both required safety tokens; rerun with a compliant `<scratch-root>` passed.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug` passed: 6 corpus tooling tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 185 tests across 4 test assemblies.
- M8 post-review refresh resolution:
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter Operations` first failed after adding regressions because successful rename/delete/permanent-delete operations did not refresh visible rows; final run passed: 19 App operation shell/route tests.
  - Completed rename, Recycle Bin delete, and confirmed permanent delete now refresh the operation origin tab/path through `FolderListingCoordinator`.
  - Failed, cancelled, and confirmation-waiting operations preserve visible rows and keep recoverable operation state visible.
  - Added direct app-route proof for failed delete preservation, completed mutation with failed refresh warning, and delayed origin-tab refresh suppression after switching to another tab.
  - A late post-mutation refresh cannot overwrite newer navigation because the refresh result must still match the coordinator's active tab request/version and the view model's active tab/path before visible rows are applied.
  - `dotnet test VeloFile.sln -c Debug --filter Operations` passed: 9 Core operation tests, 6 Windows shell-operation tests, and 19 App operation shell/route tests.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug` passed: 53 App shell contract/route tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug` passed: 6 corpus tooling tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 194 tests across 4 test assemblies.
- M9 test-first implementation evidence:
  - Added Core operation tests, Windows shell-operation tests, App shell-route tests, App contract tests, and operations corpus tests before the copy/move/conflict production APIs existed.
  - `dotnet test VeloFile.sln -c Debug --filter Operations` first failed for missing copy/move operation APIs, conflict models, Windows collision probe, App paste/conflict properties, and conflict-resolution shell routes.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` first failed because `run-compat-corpus.ps1 -Scope operations` still returned the M2 "not implemented" result.
  - Added Core copy/move requests and conflict state, Windows copy/move mapping and collision classification, App Copy/Cut/Paste staging, visible conflict resolution UI, original-paste-target refresh after conflict resolution, and operations corpus fixture validation.
  - Added same-slice regressions after review of the first pass: Skip now skips only colliding copy/move targets while unaffected batch items continue, and conflict resolution refreshes the original paste target if the user navigates elsewhere before choosing a conflict option.
- M9 validation:
  - `dotnet test VeloFile.sln -c Debug --filter Operations` passed: 13 Core operation tests, 23 App operation shell/route tests, and 12 Windows shell-operation tests.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug` passed: 6 corpus tooling tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope operations -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 208 tests across 4 test assemblies.
- M9 code-review resolution:
  - Added direct Core and App route proof for cancellation after one completed move item, retained `1 of 3` progress, cancelled final state, and no undo eligibility.
  - Added Core/App route proof for Skip and Keep Both conflict choices, including visible target-list refresh after the selected resolution completes.
  - Added a Windows executor scratch test proving Keep Both preserves the existing destination file and writes the incoming file to a distinct non-colliding destination.
  - `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter Operations` passed: 15 Core operation tests.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter Operations` first failed because operation status text did not include retained partial progress; after adding progress count formatting, it passed: 26 App operation shell/route tests.
  - `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter Operations` passed: 13 Windows shell-operation tests.
  - `dotnet test VeloFile.sln -c Debug --filter Operations` passed: 15 Core operation tests, 26 App operation shell/route tests, and 13 Windows shell-operation tests; Corpus tests had no matching `Operations` category.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 214 tests across 4 test assemblies.
- M10 test-first implementation evidence:
  - Added Core drag/drop resolver tests before the `VeloFile.Core.DragDrop` namespace existed; the first targeted run failed for the missing namespace and models.
  - Added App shell drop-indicator/drop-commit tests before `AppShellViewModel` exposed drag/drop APIs; the first targeted run failed for missing `UpdateDropAction`, `CommitDropAsync`, and indicator properties.
  - Added Windows OLE file-drop adapter tests before the `VeloFile.Windows.DragDrop` namespace existed; the first targeted run failed for the missing namespace.
  - Added corpus tests for `dragdrop` and `paths` compatibility scopes before the corpus tool supported them; the first runner test failed for unimplemented `dragdrop`.
  - Added Core drop action resolution for same-volume move, cross-volume copy, Ctrl copy, Shift move, and Ctrl+Shift/Alt shortcut intent.
  - Added App drop-action indicator state and copy/move drop commit through `FileOperationService` with post-mutation listing refresh.
  - Added Windows file-drop path projection to Core `DropItem` values behind the Windows boundary.
  - Added deterministic `dragdrop` and `pathological` corpus profiles, `dragdrop` and `paths` compatibility runner scopes, and a manual cross-app checklist.
- M10 validation:
  - `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter DragDrop` passed: 3 Core drag/drop tests.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop` passed: 3 App drag/drop tests.
  - `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter DragDrop` passed: 2 Windows OLE drag/drop tests.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` passed.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Generate_placeholder_profiles_are_deterministic` passed.
  - `dotnet test VeloFile.sln -c Debug --filter DragDrop` passed: 3 Core, 3 App, and 2 Windows drag/drop tests; Corpus tests had no matching `DragDrop` filter.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests` passed: 11 App shell contract tests.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 222 tests across 4 test assemblies.
- M10 review-resolution evidence:
  - `code-review` found that the production WinUI drag/drop route was missing, shortcut drops were advertised but rejected at commit, and placeholder path compatibility cases were reported as passing evidence.
  - Added `AppDragDropRoute`, wired `FileListSurface` drag-over/drag-leave/drop handlers, and mapped accepted operations to copy/move/link/no-drop.
  - Added `FileOperationKind.CreateShortcut`, `FileOperationService.CreateShortcutsAsync`, and Windows Shell `.lnk` creation with non-colliding shortcut names.
  - Replaced path compatibility placeholder pass output with per-case `verified`, `skipped`, `unavailable`, or `failed` results using scratch-relative fixture references.
  - Second-pass `code-review` found that drag/drop extraction failures could still escape the WinUI route and that fixture creation alone could still count as verified path behavior.
  - Added drag/drop exception boundaries in `AppDragDropRoute`, WinUI drag/drop handlers, and Windows path projection. Malformed or mixed external payloads now resolve to no-drop without starting a partial operation.
  - Follow-up `code-review` found that WinUI `StorageItem` projection still filtered blank paths before adapter projection; storage-item projection is now all-or-nothing, so blank or virtual payload items reject the entire drop instead of committing a partial operation.
  - Split path corpus evidence into fixture and behavior fields. Verified cases require behavior-verifier invocation; long/unicode/unusual path cases use bounded Core listing evidence, and junction/reparse-loop cases use bounded Core recursive-search loop-detection evidence.
  - Updated `scripts/Invoke-CorpusTool.ps1` to copy `VeloFile.Core` into the scratch-owned tool source so the corpus runner can use Core listing/search behavior without repo-side build output.
- M10 review-resolution validation:
  - `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter "DragDrop|Create_shortcuts"` first failed for missing shortcut/drop APIs; final run passed 6 tests.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop` first failed for missing app drag/drop route types; final run passed 7 tests.
  - `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter "Create_shortcut|DragDrop"` first failed for missing shortcut operation mapping and Shell link support; final run passed 5 tests.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` first failed against the old placeholder paths result schema; final run passed.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter DragDrop` passed: 5 Core, 7 App, and 2 Windows drag/drop tests; Corpus tests had no matching `DragDrop` filter.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>` passed with a compliant scratch root and per-case path outcomes.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests` passed: 11 App shell contract tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 232 tests across 4 test assemblies.
  - Second-pass review-resolution tests were added first. `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter OleDragDrop` first failed with `ArgumentException` from `Path.GetFullPath` on a malformed payload path; the final run passed 3 tests.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` first failed because verified path cases lacked behavior-evidence fields; the final run passed.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop` passed: 9 App drag/drop route tests, including throwing extractor no-drop/recoverable failure cases.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors after second-pass fixes.
  - `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter "DragDrop|Create_shortcuts"` passed: 6 Core drag/drop and shortcut tests.
  - `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter "Create_shortcut|DragDrop|OleDragDrop"` passed: 6 Windows drag/drop and shortcut tests.
  - `dotnet test VeloFile.sln -c Debug --filter DragDrop` passed: 5 Core, 9 App, and 3 Windows drag/drop tests; Corpus tests had no matching `DragDrop` filter.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>` passed with a compliant scratch root after second-pass fixes.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>` passed with behavior-verifier evidence in path case results after second-pass fixes.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 235 tests across 4 test assemblies after second-pass fixes.
  - Follow-up all-or-nothing projection test was added first. `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop_winui_extractor_uses_all_or_nothing_storage_item_projection` first failed because `MainWindow.xaml.cs` filtered blank `StorageItem.Path` values before adapter projection.
  - `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop` passed: 13 App drag/drop route/contract tests, including mixed valid plus blank storage-item path rejection and all-valid storage-item commit.
  - `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter OleDragDrop` passed: 4 Windows OLE drag/drop tests, including blank and mixed blank path rejection.
  - `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` passed: 1 corpus tooling test.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter DragDrop` passed: 5 Core, 13 App, and 4 Windows drag/drop tests; Corpus tests had no matching DragDrop filter.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 240 tests across 4 test assemblies.
- M11 test-first implementation evidence:
  - Added Core preview contract tests before the `VeloFile.Core.Preview` namespace and models existed; the first targeted `dotnet test VeloFile.sln -c Debug --filter PreviewContract` failed for missing preview types.
  - Added App preview shell tests and a shell contract test for the Ctrl+P/preview-pane route before the view model and XAML exposed preview state.
  - Added a preview corpus contract test before `run-preview-corpus.ps1 -Scope contract` had behavior-verifier output.
  - Added `PreviewController`, preview request/result/content/metadata models, metadata fallback provider, delayed loading, timeout, cancellation/ignore gating, terminal state mapping, and redacted failure diagnostics.
  - Wired production app composition to a metadata-only preview provider and preview controller, with a retained local diagnostics redaction salt.
  - Wired the shell preview pane, metadata list, Ctrl+P toggle, single-selection preview start, non-single selection clear, pane close clear, tab/listing/search clear paths, and zero-width collapsed preview column.
  - Expanded the preview corpus `contract` scope to invoke an in-process preview behavior verifier for loading delay, timeout, metadata fallback, and stale selection before writing verified evidence.
- M11 validation:
  - `dotnet test VeloFile.sln -c Debug --filter PreviewContract` passed: 5 Core, 4 App, and 1 Corpus preview-contract tests; Windows tests had no matching filter.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope contract -ScratchRoot <scratch-root>` first failed when the scratch-root leaf omitted the required `corpus` safety token; rerun with a compliant scratch root passed.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build with 0 warnings and 0 errors, and 250 tests across 4 test assemblies.
- M11 review-resolution:
  - First-pass `code-review` requested provider-specific preview timeout policy and direct scratch-file proof for non-mutation plus complete metadata fallback.
  - Added `PreviewOperation`, `PreviewProviderContext`, and `PreviewTimeoutPolicy.Default` with the R67 image/text/PDF/thumbnail budgets and thumbnail concurrency limit.
  - The preview controller now enforces timeout using the selected provider operation budget rather than a single global preview timeout.
  - Expanded listed-item and preview metadata to carry created, modified, and accessed timestamps when available, and updated Windows listing projection to populate them.
  - Added scratch-file preview tests that compare content hash, length, creation time, last-write time, and attributes after image/text/PDF-operation fake providers run through the controller path.
  - Added unsupported metadata fallback and unavailable-metadata tests, plus App shell proof that the expanded metadata fields are exposed.
  - `dotnet test VeloFile.sln -c Debug --filter PreviewContract` passed: 17 Core, 4 App, and 1 Corpus preview-contract tests; Windows tests had no matching filter.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope contract -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter "Listing|Visibility"` passed: 29 Core, 3 App, and 4 Windows tests; Corpus tests had no matching filter.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` first failed because the assertion cleanup used the wrong `Assert.IsGreaterThanOrEqualTo` parameter order; final run passed.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` first failed for the same corpus assertion issue; final run passed restore, build with 0 warnings and 0 errors, and 262 tests across 4 test assemblies.
- M12 test-first implementation evidence:
  - Added Windows provider tests before `VeloFile.Windows.Preview` existed; the first `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter PreviewProviders` failed for the missing namespace/provider types.
  - Added an App composition contract test before composition changed; it failed because production still instantiated `[new MetadataOnlyPreviewProvider()]`.
  - Added a preview provider corpus test before `run-preview-corpus.ps1 -Scope providers` was implemented; it first failed with the unimplemented provider scope, then failed until `scripts/Invoke-CorpusTool.ps1` copied `src/VeloFile.Windows` into its isolated scratch build.
  - Added `WindowsImagePreviewProvider`, `WindowsTextPreviewProvider`, `WindowsPdfPreviewProvider`, and `WindowsPreviewProviderFactory`.
  - Review-resolution tests for the M12 render blockers failed first for missing render-artifact, decoder, renderer, and shell PDF navigation contracts.
  - Image preview now supports common image extensions through `WindowsImagePreviewDecoder`, decodes image bodies with Windows imaging APIs, returns PNG render artifacts, enforces the 100 MB input cap, rejects decoded dimensions over 8192 by 8192, and fails corrupt bodies instead of accepting plausible headers.
  - Text/code preview now reads at most 1 MB, reports truncation, rejects binary-looking inputs, and uses the 100 MB metadata-only cap.
  - PDF preview now renders pages through `WindowsPdfPageRenderer`, returns encoded page artifacts, renders page 1 initially, exposes `IPagedPreviewProvider`/view-model navigation for later pages, and uses the 500 MB metadata-only cap.
  - The app preview pane now exposes content text and an image surface that loads image/PDF render artifact bytes.
  - Production app composition now injects the Windows provider chain before metadata fallback.
  - The preview corpus `providers` scope now writes behavior-verifier evidence for image artifact success, text truncation, PDF page artifact success, over-size fallback, and source non-mutation using decodable fixtures.
  - Validation exposed that the corpus wrapper's shared intermediate-output overrides corrupt project-reference `.deps.json` generation once the corpus tool references both Core and Windows projects. The wrapper now lets each copied scratch project use its own default scratch-local `bin/obj` folders and still publishes to the existing scratch-local publish path.
  - Review-resolution for the real-boundary byte-cap blocker added sparse-file tests where listing metadata is null or stale. `WindowsImagePreviewDecoder` and `WindowsPdfPageRenderer` now check the opened stream length before BitmapDecoder/PdfDocument work; over-limit inputs return metadata-only unsupported reasons and unavailable stream length fails closed.
  - Follow-up byte-cap review resolution added direct PDF exact-boundary proof: exactly 500 MB is allowed past the renderer boundary guard, while 500 MB plus one byte is rejected before render invocation.
  - Review-resolution for the shell PDF navigation blocker added Previous/Next preview-pane controls and view-model command properties so later-page rendering is reachable through the production shell route.
  - Follow-up PDF shell navigation review resolution added durable PDF navigation context that remains visible while a later page is loading or fails, disables previous/next during in-flight rendering, no-ops at page bounds without calling the renderer, and preserves the last successfully rendered page on recoverable page-render failure.
  - Validation exposed that `CorpusToolingSmokeTests.RunScript` could deadlock while reading redirected stdout/stderr sequentially from corpus subprocesses. The harness now drains both streams concurrently before collecting the result, which unblocks solution-level filtered validation.
- M12 validation:
  - `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter PreviewProviders` first failed for the real-boundary byte-cap regression: oversized sparse image/PDF files with null listing length returned failed/corrupt states instead of size-limit unsupported states. The final follow-up run passed 14 Windows provider tests, including exact-PDF-cap and cap-plus-one boundary proof.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter PreviewProviders` passed: 1 App composition test.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "PreviewContract_pdf_page_navigation|PreviewContract_main_window_preview_pane"` first failed for missing durable PDF navigation context properties; the final follow-up run passed 5 App shell/view-model/main-window PDF navigation tests.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter PreviewContract` passed: 8 App preview-contract tests.
  - `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter PreviewContract` passed: 18 Core preview-contract tests.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter PreviewProviders` passed: 1 Corpus provider evidence test.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreviewContract_scope_records_contract_behavior_evidence"` passed: 1 Corpus preview-contract evidence test.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope providers -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope contract -ScratchRoot <scratch-root>` passed with a compliant scratch root.
  - `dotnet test VeloFile.sln -c Debug --filter PreviewProviders` first timed out because the Corpus test harness deadlocked on redirected output; after fixing the harness and adding the final PDF cap proof, it passed 1 App, 14 Windows, and 1 Corpus provider tests; Core tests had no matching filter.
  - `dotnet test VeloFile.sln -c Debug --filter PreviewContract` passed: 18 Core, 8 App, and 1 Corpus preview-contract tests; Windows tests had no matching filter.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --no-build` passed 283 tests across App, Core, Windows, and Corpus assemblies.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug` first failed because the corpus wrapper used one shared intermediate-output directory for multiple copied project references; final run passed 8 Corpus tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` review-resolution run passed `dotnet --info`, restore, build with 0 warnings and 0 errors, and 283 tests across 4 test assemblies.
- M13 test-first implementation evidence:
  - Added Core thumbnail controller tests before the thumbnail controller existed; the first focused run failed for missing thumbnail controller APIs.
  - Added Windows thumbnail provider tests before the production Windows provider existed; the first focused run failed for missing provider types.
  - Added App preview UI tests before file-list rows exposed thumbnail state or preview accessibility names; the first focused run failed for missing row/presenter surface.
  - Added Corpus thumbnail scope tests before `preview --scope thumbnails` existed; the first corpus run failed for missing scope support.
  - Implemented Core thumbnail state, four-operation concurrency, per-item timeout fallback, generation cancellation, and stale-result ignore.
  - Implemented the Windows thumbnail provider with Windows Storage thumbnail APIs and cached generic fallback artifacts.
  - Bound the file list to stable row view models that expose thumbnail state, dimming, selection identity, and preview/details accessibility state.
  - Expanded the preview corpus thumbnail scope with behavior-verifier evidence for concurrency, timeout, fallback, and stale-result behavior.
- M13 review-resolution:
  - First-pass `code-review` requested proof and implementation for non-cooperative provider timeout behavior and UI-dispatcher-bound thumbnail row updates.
  - Added non-cooperative provider tests proving visible timeout fallback, live-slot retention after timeout, queued-row deadline fallback, and late success ignore.
  - Updated `ThumbnailController` to race provider work against the visible thumbnail deadline while keeping semaphore-held provider work alive until actual completion.
  - Added `IShellDispatcher`, passed a WinUI `DispatcherQueue` implementation through production app composition before thumbnail event subscription, and routed thumbnail state-change row refreshes through the dispatcher before row mutation.
  - Added App tests proving no row mutation or `PropertyChanged` occurs before dispatcher execution, and stale thumbnail completion does not update a recycled row.
  - Second-pass review found the live provider semaphore was generation-local. Added a failing cross-generation regression, moved the gate to controller scope, and added slot-reuse proof for future explicit generations after old provider completion.
  - `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 6 Core thumbnail tests.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Thumbnails|PreviewUi" --no-restore` passed: 5 App thumbnail/preview UI tests.
  - `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 2 Windows thumbnail tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\run-preview-corpus.ps1 -Scope thumbnails -ScratchRoot <scratch-root>` passed.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 1 Corpus thumbnail test.
  - `dotnet test VeloFile.sln -c Debug --filter Thumbnails` passed: 4 Core, 2 Windows, and 1 Corpus thumbnail tests.
  - `dotnet test VeloFile.sln -c Debug --filter PreviewUi` passed: 5 App preview UI tests.
  - An intermediate parallel App focused test plus solution build hit the known App.Tests `obj` file-lock behavior; `dotnet build-server shutdown` followed by sequential validation passed.
  - The first post-composition full-suite run failed because an older app-launch contract assertion still expected the no-dispatcher composition call. The final test now asserts `CreateShellViewModel(shellDispatcher)`.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - Second-pass validation reran the focused M13 gates after the controller-wide throttle fix.
  - `dotnet test VeloFile.sln -c Debug --no-build` passed 297 tests across App, Core, Windows, and Corpus assemblies.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed `dotnet --info`, restore, build with 0 warnings and 0 errors, and 297 tests across 4 test assemblies.

## Outcome and Retrospective

M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, and M14 complete. The repository now has a buildable WinUI app shell, core and Windows boundary projects, smoke tests, Windows CI entry point, generated corpus tooling, safe scratch-root checks, smoke corpus runners, a non-gating benchmark report stub, durable local state contracts, Windows safe-write storage, local redacted diagnostics foundations, non-UI folder listing/visibility services with direct slow-tab isolation and bounded drive-hint enrichment proofs, core navigation/sidebar/session restore state, a Core shell navigation command surface, app launch restore composition, a compiled shell surface wired to those commands, Explorer-style selection state, a built-in command registry, command keyboard routing, clipboard copy path/name boundaries, current-folder filtering, explicit bounded recursive search, reviewed file-operation safety contracts for rename, Recycle Bin delete, permanent-delete confirmation, visible operation state, post-mutation visible-list refresh, in-flight cancellation, production unsupported-Recycle-Bin classification, safe-delete corpus validation, copy/move/conflict behavior with operations corpus validation, Core drag/drop action resolution, production WinUI drag/drop routing with extraction failure and all-or-nothing storage-item projection boundaries, App drop-action indicators, Windows file-drop projection, shortcut drop creation, drag/drop/path compatibility corpus scopes with behavior-verifier evidence for verified path cases, a preview contract with metadata fallback, timeout/cancellation orchestration, redacted diagnostics, shell preview toggle, preview corpus contract evidence, bounded Windows image/text/PDF preview providers with render artifacts, PDF page navigation, provider corpus evidence, thumbnail/icon/details UI with bounded non-blocking thumbnail execution and UI-dispatched row updates, safe explicit terminal launch with lazy target discovery, terminal selection persistence, and Windows file association Open/Open With integration. Later V1 benchmark, accessibility, release-triage, packaging, update, and release-readiness behavior remains assigned to M15-M16.

## Readiness

M14 terminal launch and file association integration is implemented, validation is passing, and the slice is ready for `code-review`.
