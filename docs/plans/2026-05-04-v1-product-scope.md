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
| Drag/drop and Windows compatibility corpus | R46, R79-R82, AC9 | M10 |
| Context menu, commands, shortcuts, clipboard | R47-R53, AC8 | M6 |
| Terminal integration | R54-R59, S2, AC12 | M14 |
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
  - `pwsh scripts/ci.ps1` or the final M1 CI command recorded in `AGENTS.md`
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
  - `pwsh scripts/generate-corpus.ps1 -Profile smoke -Root <scratch-root>`
  - `pwsh scripts/run-compat-corpus.ps1 -Scope smoke -Root <scratch-root>`
  - `pwsh scripts/run-preview-corpus.ps1 -Root <scratch-root>`
  - `pwsh scripts/run-benchmarks.ps1 -NonGating -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/ci.ps1`
- Expected observable result: Users can select files with mouse/keyboard, open the built-in context menu, and invoke safe command-layer routes.
- Commit message: `M6: add selection and built-in command layer`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
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
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/run-compat-corpus.ps1 -Scope safe-delete -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/run-compat-corpus.ps1 -Scope operations -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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

- Goal: Implement Windows-correct drag/drop modifiers, resolved drop-action indicators, cross-app drag/drop boundaries, and expand the compatibility corpus for long paths, junctions, symlinks, reparse points, file associations, and drag/drop.
- Requirements: R46, R79-R82, I8, EC24-EC25, AC9, ADR 0002, ADR 0007.
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
  - `pwsh scripts/run-compat-corpus.ps1 -Scope dragdrop -Root <scratch-root>`
  - `pwsh scripts/run-compat-corpus.ps1 -Scope paths -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/run-preview-corpus.ps1 -Scope contract -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/run-preview-corpus.ps1 -Scope providers -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/run-preview-corpus.ps1 -Scope thumbnails -Root <scratch-root>`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/ci.ps1`
- Expected observable result: Users can explicitly open a configured terminal or open files through Windows associations without command injection or state corruption.
- Commit message: `M14: implement safe terminal and file association integration`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Argument handling differs by terminal target.
- Rollback/recovery: Failed launch leaves browsing state unchanged and records only redacted diagnostics.

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
  - `pwsh scripts/generate-corpus.ps1`
  - `pwsh scripts/run-benchmarks.ps1`
  - `pwsh scripts/run-compat-corpus.ps1`
  - `dotnet test VeloFile.sln -c Debug --filter Accessibility`
  - `pwsh scripts/ci.ps1`
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
  - `pwsh scripts/ci.ps1`
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
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- `scripts/ci.sh` wraps `scripts/ci.ps1` when the Bash environment can invoke PowerShell.

Expected post-M1 core commands:

- `dotnet restore VeloFile.sln`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug`
- `pwsh scripts/ci.ps1`

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
- [ ] M2 complete.
- [ ] M3 complete.
- [ ] M4 complete.
- [ ] M5 complete.
- [ ] M6 complete.
- [ ] M7 complete.
- [ ] M8 complete.
- [ ] M9 complete.
- [ ] M10 complete.
- [ ] M11 complete.
- [ ] M12 complete.
- [ ] M13 complete.
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
| 2026-05-04 | Use Windows PowerShell as the local CI invocation and `pwsh` in GitHub Actions. | The local workstation has Windows PowerShell available but not PowerShell 7 on PATH; GitHub Windows runners support `pwsh`. |

## Surprises and Discoveries

- `docs/plan.md` and `docs/plans/` were absent when planning started; this plan restores the required plan index and creates the first real plan body.
- `scripts/ci.sh` and `scripts/release-verify.sh` are template placeholders, so M1 and M16 must replace them with product-specific commands.
- No `CONSTITUTION.md` or `docs/project-map.md` exists yet. The V1 test spec has been created and marked active.
- First-pass `plan-review` found sequencing and milestone-size issues; this revision adds validation tooling early and splits file-operation and preview work.
- M1 was initially blocked because the installed `dotnet` host had runtimes but no SDK. After the environment was updated, `dotnet --info` reports SDKs `9.0.313` and `10.0.203`.
- `dotnet new list winui` does not list WinUI templates, but Visual Studio has WinUI C# templates installed and equivalent checked-in project files build successfully with `Microsoft.WindowsAppSDK` `1.7.250401001`.
- The local workstation does not have `pwsh` on PATH; the M1 CI script is compatible with Windows PowerShell and the GitHub workflow still uses `pwsh`.
- `bash scripts/ci.sh` was smoke-tested from WSL Bash and cannot run there because that Bash environment cannot invoke Windows PowerShell. This is not the M1 gating command; local validation uses Windows PowerShell directly.

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
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and test all green.
  - Launch smoke passed: `src\VeloFile.App\bin\x86\Debug\net8.0-windows10.0.19041.0\VeloFile.App.exe` started and stayed alive for 2 seconds before being stopped.
  - `scripts/select-validation.py` is not present in M1; selector-based validation is therefore not available yet.

## Outcome and Retrospective

M1 complete. The repository now has a buildable WinUI app shell, core and Windows boundary projects, smoke tests, Windows CI entry point, and contributor-facing build/test commands. Later V1 product behavior remains assigned to M2-M16.

## Readiness

M1 is ready for `code-review`. Do not start M2 until M1 review findings are resolved or explicitly deferred.

Implementation resumes after M1 review with:

- M2 establishing validation tooling and minimal corpus foundations.
