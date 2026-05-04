# V1 Product Scope Test Spec

## Status

active

This test spec is the active proof surface for the approved V1 product scope. It is not executable yet because the repository has no product source tree, test projects, corpus generator, benchmark harness, package manifest, or product CI. M1 and M2 in the execution plan create those foundations.

## Related Spec and Plan

- Feature spec: [v1-product-scope.md](v1-product-scope.md)
- Execution plan: [2026-05-04-v1-product-scope.md](../docs/plans/2026-05-04-v1-product-scope.md)
- Architecture: [architecture.md](../docs/architecture/system/architecture.md)
- ADRs: [docs/adr/](../docs/adr/)

## Testing Strategy

The V1 proof strategy is layered:

- Unit tests prove deterministic domain behavior in `VeloFile.Core`: navigation state, selection, filtering, search orchestration, session models, persistence migration, command routing, operation state models, preview state transitions, diagnostics redaction, and settings.
- Windows adapter contract tests prove `VeloFile.Windows` boundaries without leaking Shell/Win32 details into the UI or Core: file-system enumeration, Shell-owned file operations, OLE drag/drop, terminal launch, ShellExecute, thumbnails/icons, WIC/PDF/text preview, long paths, and persistence replacement primitives.
- App/component tests prove WinUI-visible behavior where automation is stable: tabs, sidebar, breadcrumb/path bar, virtualized list state, built-in context menu, preview/details pane, dialogs, progress surfaces, missing-location tabs, and accessibility-visible states.
- Corpus integration tests use generated scratch data for file operations, search, preview, long paths, reparse points, drag/drop action resolution, and compatibility behavior.
- Benchmark tests use the generated reference corpus and app-level harness after M15. Benchmark output is release evidence, not a substitute for unit or integration tests.
- Manual QA covers Windows behaviors that are expensive or brittle to automate: MSIX install/update/uninstall/rollback, mixed-DPI monitor checks, cross-app drag/drop with Explorer/browser/IDE/Office, selected terminal targets on real machines, file associations, and signed release-channel evidence.

No milestone may rely on a validation command before the script, corpus profile, and fixture set exist in a previous milestone or are created in the same milestone before validation runs.

## Requirement Coverage Map

| Requirement | Tests / verification |
|---|---|
| R1 | T001, T002, T037, T040 |
| R2 | T002, T037 |
| R3 | T037 |
| R4 | T003, T021, T037 |
| R5 | T005, T040 |
| R6 | T006, T040 |
| R7 | T007, T039 |
| R8 | T005, T007 |
| R9 | T007, T039 |
| R10 | T008, T027, T032 |
| R11 | T008, T039 |
| R12 | T008, T036 |
| R13 | T008, T034 |
| R14 | T010 |
| R15 | T010 |
| R16 | T010 |
| R17 | T010, T008 |
| R18 | T011 |
| R19 | T011 |
| R20 | T011, T036 |
| R21 | T012, T013 |
| R22 | T013 |
| R23 | T013 |
| R24 | T013 |
| R25 | T012 |
| R26 | T012 |
| R27 | T013, T034 |
| R28 | T014 |
| R29 | T014 |
| R30 | T014 |
| R31 | T014 |
| R32 | T015 |
| R33 | T015 |
| R34 | T016 |
| R35 | T016, T038 |
| R36 | T017, T019 |
| R37 | T017, T019, T039 |
| R38 | T017 |
| R39 | T018 |
| R40 | T018 |
| R41 | T019 |
| R42 | T019 |
| R43 | T017, T019, T036 |
| R44 | T019 |
| R45 | T018, T019 |
| R46 | T020, T033, T035 |
| R47 | T021 |
| R48 | T021 |
| R49 | T003, T021, T034 |
| R50 | T022 |
| R51 | T022 |
| R52 | T023 |
| R53 | T023 |
| R54 | T024, T025 |
| R55 | T024 |
| R56 | T024, T039 |
| R57 | T025 |
| R58 | T025 |
| R59 | T025, T034 |
| R60 | T027, T036 |
| R61 | T028, T029, T030 |
| R62 | T028 |
| R63 | T029 |
| R64 | T029 |
| R65 | T030 |
| R66 | T030 |
| R67 | T027, T028, T029, T030, T032 |
| R68 | T027 |
| R69 | T027 |
| R70 | T027 |
| R71 | T031 |
| R72 | T027, T031, T032 |
| R73 | T009 |
| R74 | T009, T038 |
| R75 | T009 |
| R76 | T009 |
| R77 | T009, T036 |
| R78 | T009, T036 |
| R79 | T033 |
| R80 | T033 |
| R81 | T026 |
| R82 | T026 |
| R83 | T036, T039 |
| R84 | T034 |
| R85 | T034 |
| R86 | T034 |
| R87 | T034 |
| R88 | T035 |
| R89 | T003, T034, T035 |
| R90 | T038 |
| R91 | T038 |
| R92 | T038 |
| R93 | T037 |
| I1 | T017 |
| I2 | T018 |
| I3 | T031 |
| I4 | T014, T015 |
| I5 | T003, T021 |
| I6 | T013 |
| I7 | T009, T012 |
| I8 | T026 |
| I9 | T008, T016, T025, T027, T034 |
| I10 | T034 |
| C1 | T001, T037 |
| C2 | T002, T037 |
| C3 | T026 |
| C4 | T026, T037 |
| C5 | T008 |
| C6 | T012 |
| C7 | T012 |
| C8 | T012 |
| C9 | T037 |
| C10 | T003, T021, T034 |
| O1 | T034 |
| O2 | T034, T035 |
| O3 | T034 |
| O4 | T034 |
| O5 | T035, T039 |
| O6 | T035 |
| O7 | T008, T016, T017, T019, T027, T034 |
| S1 | T014, T017, T025, T026, T031, T034 |
| S2 | T025 |
| S3 | T034 |
| S4 | T012, T013 |
| S5 | T018 |
| S6 | T038 |
| S7 | T027, T028, T029, T030, T031 |
| S8 | T003, T021 |
| A11Y1 | T011, T022, T027, T036 |
| A11Y2 | T036 |
| A11Y3 | T017, T019, T022, T036 |
| A11Y4 | T036 |
| A11Y5 | T018, T036 |
| A11Y6 | T008, T027, T036 |
| A11Y7 | T013, T036 |
| P1 | T035, T039 |
| P2 | T039 |
| P3 | T039 |
| P4 | T039 |
| P5 | T039 |
| P6 | T039 |
| P7 | T039 |
| P8 | T039 |
| P9 | T039 |
| P10 | T039 |
| P11 | T039 |
| P12 | T039 |
| P13 | T039 |
| P14 | T035 |
| P15 | T035 |
| P16 | T035, T038 |

## Acceptance Criteria Coverage Map

| Acceptance criterion | Tests / verification |
|---|---|
| AC1 | T002, T037 |
| AC2 | T005, T006, T011, T040 |
| AC3 | T007, T039 |
| AC4 | T014 |
| AC5 | T015, T016 |
| AC6 | T017, T018 |
| AC7 | T019 |
| AC8 | T021 |
| AC9 | T020, T026, T033, T035 |
| AC10 | T027, T028, T029, T030, T032 |
| AC11 | T031 |
| AC12 | T024, T025 |
| AC13 | T012, T013 |
| AC14 | T009 |
| AC15 | T035, T039 |
| AC16 | T037 |
| AC17 | T034, T035 |
| AC18 | T038 |
| AC19 | T036 |

## Example Coverage Map

| Example | Tests / verification |
|---|---|
| E1 | T005, T006, T040 |
| E2 | T014 |
| E3 | T015 |
| E4 | T017 |
| E5 | T018 |
| E6 | T012, T013 |
| E7 | T027, T031 |
| E8 | T021 |
| E9 | T015 |
| E10 | T027, T028, T029, T030, T032 |
| E11 | T024 |
| E12 | T013 |

## Edge Case Coverage

| Edge case | Tests / verification |
|---|---|
| EC1 | T008 |
| EC2 | T008 |
| EC3 | T007, T039 |
| EC4 | T008, T014 |
| EC5 | T020 |
| EC6 | T016 |
| EC7 | T016 |
| EC8 | T008, T032 |
| EC9 | T027 |
| EC10 | T027, T028, T029, T030 |
| EC11 | T028, T029, T030 |
| EC12 | T017, T018 |
| EC13 | T019 |
| EC14 | T019 |
| EC15 | T012 |
| EC16 | T013 |
| EC17 | T013 |
| EC18 | T025 |
| EC19 | T026 |
| EC20 | T021, T038 |
| EC21 | T009, T036 |
| EC22 | T012, T037 |
| EC23 | T008, T039 |
| EC24 | T033, manual QA |
| EC25 | T033 |
| EC26 | T015 |
| EC27 | T027 |

## Architecture and ADR Coverage

| Architecture / ADR item | Tests / verification |
|---|---|
| UI shell boundary | T005, T006, T007, T011, T021, T027, T036, T040 |
| Command layer | T021, T022, T023, T025 |
| Navigation/session service | T011, T012, T013 |
| Enumeration/metadata providers | T007, T008, T032 |
| Search/filter services | T014, T015, T016 |
| Preview/thumbnail providers | T027, T028, T029, T030, T031, T032 |
| File-operation service | T017, T018, T019, T020, T033 |
| Shell/Win32 interop boundary | T020, T024, T025, T026, T028, T029, T030, T032, T033 |
| Persistence service | T012, T013 |
| Diagnostics service | T034, T035 |
| Benchmark harness and corpus | T004, T035, T039 |
| QS-RESP-01 | T007, T039 |
| QS-SAFE-DELETE-01 | T017, T018, T019 |
| QS-SLOW-TAB-01 | T008, T039 |
| QS-PREVIEW-TIMEOUT-01 | T027, T028, T029, T030, T032 |
| QS-SESSION-RECOVERY-01 | T012, T013 |
| QS-DIAG-PRIV-01 | T034 |
| QS-MSIX-ROLLBACK-01 | T037 |
| ADR 0001 | T001, T037 |
| ADR 0002 | T017, T020, T021, T033 |
| ADR 0003 | T004, T035, T039 |
| ADR 0004 | T027, T028, T029, T030, T031, T032 |
| ADR 0005 | T024, T025 |
| ADR 0006 | T012, T013 |
| ADR 0007 | T009, T021, T022, T033 |
| ADR 0008 | T034, T035 |

## Test Cases

### Foundation and Product Boundary

T001. Windows app foundation and CI smoke
- Covers: R1, C1, M1
- Level: smoke
- Fixture/setup: M1 solution skeleton with `VeloFile.sln`, app project, test projects, and Windows CI entrypoint.
- Steps: Run `dotnet restore`, `dotnet build`, `dotnet test`, and the shared CI entrypoint `scripts/ci.ps1`; GitHub Actions invokes it with `pwsh`, local Windows validation may use `pwsh scripts/ci.ps1` when PowerShell 7 is installed, and local M1 acceptance may use `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` when `pwsh` is unavailable. Launch the app shell on supported Windows.
- Expected result: The app restores, builds, tests, and reaches a visible launchable shell without template-only validation.
- Failure proves: The repository is not ready to host V1 behavior.
- Automation location: `.github/workflows/ci.yml`, `scripts/ci.ps1`, `tests/VeloFile.App.Tests/`.

T002. Side-by-side Explorer boundary
- Covers: R1, R2, C2, AC1
- Level: smoke | manual
- Fixture/setup: Local Windows 10/11 machine with Windows File Explorer available.
- Steps: Install or launch VeloFile; open Explorer independently; inspect packaging/app registration for Explorer replacement hooks.
- Expected result: VeloFile runs as its own app and does not replace or hijack Explorer.
- Failure proves: V1 violates its product boundary.
- Automation location: `tests/VeloFile.App.Tests/`, `scripts/release-verify.ps1`, manual release checklist.

T003. V1 non-goal guardrails
- Covers: R4, R49, R89, I5, C10, S8
- Level: contract
- Fixture/setup: Built app command registry, context menu definitions, package manifest, and documentation tree.
- Steps: Assert no command, setting, manifest entry, or enabled preview flag exposes cross-platform runtime, global indexer, OS shell menu integration, third-party Shell extension hosting, telemetry upload, or other V1 non-goals.
- Expected result: Non-goal surfaces are absent; any post-V1 shell-menu marker category is diagnostics-only and not user-enabled.
- Failure proves: Scope has expanded beyond the approved V1 contract.
- Automation location: `tests/VeloFile.Core.Tests/Scope/`, docs checks in `scripts/release-verify.ps1`.

T004. Validation tooling and scratch-root safety
- Covers: M2 validation dependency rule
- Level: integration
- Fixture/setup: Temporary scratch roots under explicit generated-corpus workspace plus unsafe paths such as repository root and user home.
- Steps: Run corpus, preview, compatibility, and benchmark scripts with valid and invalid roots.
- Expected result: Scripts require explicit scratch roots, refuse ambiguous or unsafe roots, generate deterministic smoke profiles, and leave no files outside the scratch root.
- Failure proves: Later validation could mutate user data or depend on missing assets.
- Automation location: `tests/VeloFile.Corpus.Tests/`, `scripts/generate-corpus.ps1`, `scripts/run-compat-corpus.ps1`, `scripts/run-preview-corpus.ps1`, `scripts/run-benchmarks.ps1`.

### Navigation, Listing, Sidebar, and Tabs

T005. Folder open entry points and view modes
- Covers: R5, R8, E1, AC2
- Level: integration | e2e
- Fixture/setup: Smoke corpus with local folder, favorites, recent location, and drive entry.
- Steps: Open the same folder from launch, typed path, pasted path, breadcrumb, sidebar, recent, favorite, and drive entry; switch details, list, and large-icons modes.
- Expected result: The active tab shows the requested folder, first viewport, current view mode, and accepts navigation input.
- Failure proves: Core browsing entry points or view-mode contract is incomplete.
- Automation location: `tests/VeloFile.App.Tests/Navigation/`, `tests/VeloFile.Core.Tests/Navigation/`.

T006. Breadcrumb and raw path editing
- Covers: R6, E1, AC2
- Level: integration | e2e
- Fixture/setup: Nested generated folders with at least three path segments.
- Steps: Click breadcrumb segments, switch to raw path edit, paste a valid path, paste an invalid path.
- Expected result: Valid segment and raw-path navigation updates the active tab; invalid path shows recoverable failure and preserves prior valid tab state when possible.
- Failure proves: Breadcrumb/path bar is not a dependable navigation surface.
- Automation location: `tests/VeloFile.App.Tests/Breadcrumb/`.

T007. Virtualized large-folder first viewport
- Covers: R7, R9, EC3, AC3, QS-RESP-01
- Level: integration | performance
- Fixture/setup: Generated large folder profile with 100,000 items.
- Steps: Open the folder and inspect rendered UI elements and listing state before thumbnail/nonessential metadata completion.
- Expected result: Only visible working-set UI elements are realized, first viewport appears before enrichment completes, and no full-folder UI element allocation occurs.
- Failure proves: The file list can scale poorly and violates the responsiveness architecture.
- Automation location: `tests/VeloFile.App.Tests/FileList/`, `tools/VeloFile.Benchmarks/`.

T008. Folder state boundaries and slow-tab isolation
- Covers: R10-R13, R17, C5, I9, O7, EC1, EC2, EC8, EC23, QS-SLOW-TAB-01
- Level: integration | performance
- Fixture/setup: Empty folder, access-denied folder, disappearing path, unavailable/removable simulation, slow-location adapter, and a healthy local tab.
- Steps: Navigate to each failure/slow state, switch to the healthy tab during pending work, cancel or retry where available.
- Expected result: Each affected tab shows pending, empty, unavailable, access-denied, or failure state; other tabs remain interactive; stale thumbnail/metadata work is cancelled or ignored.
- Failure proves: Slow or failed locations can corrupt or freeze unrelated browsing state.
- Automation location: `tests/VeloFile.Core.Tests/Listing/`, `tests/VeloFile.Windows.Tests/FileSystem/`, benchmark slow-tab scenario.

T009. Visibility settings and Explorer safety divergence
- Covers: R73-R78, I7, AC14, EC21
- Level: unit | integration | e2e
- Fixture/setup: Folder with normal, hidden, protected system, extension-known, and extension-ambiguous files.
- Steps: Verify defaults; toggle hidden, protected operating-system files, and extensions; restart the app; enable protected files for the first time.
- Expected result: Extensions are shown by default; known extensions can be hidden within VeloFile only; protected files are hidden by default and first-use confirmation is shown when enabling; shown hidden/protected items are visually distinguishable.
- Failure proves: Visibility policy violates safety, persistence, or Explorer parity requirements.
- Automation location: `tests/VeloFile.Core.Tests/Visibility/`, `tests/VeloFile.App.Tests/Settings/`.

T010. Sidebar favorites, recent locations, and drives
- Covers: R14-R17
- Level: integration
- Fixture/setup: Generated folders, more than 20 navigated locations, available drives with and without free-space data.
- Steps: Add/remove favorites, dismiss individual recents, navigate through more than 20 locations, render drives.
- Expected result: Favorites are mutable, recents cap at 20 and are dismissible, drives render, and free-space hints appear only when available without delaying navigation.
- Failure proves: Sidebar state does not meet V1 daily-navigation contract.
- Automation location: `tests/VeloFile.Core.Tests/Sidebar/`, `tests/VeloFile.App.Tests/Sidebar/`.

T011. Tab lifecycle, history, and keyboard switching
- Covers: R18-R20, A11Y1, E6, AC2
- Level: unit | integration | e2e
- Fixture/setup: Multiple generated folders and keyboard automation harness.
- Steps: Open, close, reorder, duplicate, reopen closed tab, navigate back/forward per tab, and switch tabs by keyboard.
- Expected result: Each tab owns its history; tab commands update active tab predictably; keyboard path works with visible focus.
- Failure proves: Tabs are not first-class or keyboard-accessible.
- Automation location: `tests/VeloFile.Core.Tests/Tabs/`, `tests/VeloFile.App.Tests/Tabs/`.

### Persistence, Session, and Diagnostics Foundations

T012. Versioned durable documents and partial-write recovery
- Covers: R21, R25, R26, C6-C8, S4, EC15, EC22, ADR 0006
- Level: unit | integration | migration
- Fixture/setup: Canonical, temp, and last-known-good files for session, settings, favorites, and recent locations; corrupt fields; unknown fields; newer schema versions; write-fault injection seams.
- Steps: Read/write documents; inject crashes before temp creation, after temp creation, during replace; load corrupt canonical with valid/invalid backup; migrate known fields from newer schema.
- Expected result: Old-valid, new-valid, last-known-good, or safe-default state is recovered; unknown fields are ignored; malformed optional fields fall back per field; failures log redacted diagnostics and do not block launch.
- Failure proves: Persistence can corrupt launch state or violate ADR 0006.
- Automation location: `tests/VeloFile.Core.Tests/Persistence/`, `tests/VeloFile.Windows.Tests/Storage/`.

T013. Session restore behavior and crash recovery UI
- Covers: R21-R24, R27, I6, A11Y7, E6, E12, AC13, EC16, EC17
- Level: integration | e2e
- Fixture/setup: Saved sessions with ten tabs, active tab, sort/view state, history, sidebar state, scroll anchor, missing path, removed monitor, stale selection/filter/search/clipboard/operation data, and crash marker.
- Steps: Restore each session and inspect restored state, missing-location tab, monitor fallback, scroll anchoring, excluded fields, and start-fresh option after repeated failure.
- Expected result: Included fields restore; scroll anchors by first visible item name; missing paths remain visible with path and close action; excluded fields are not restored; crash recovery offers start fresh.
- Failure proves: Session restore can surprise users, lose tabs, or create crash loops.
- Automation location: `tests/VeloFile.Core.Tests/Session/`, `tests/VeloFile.App.Tests/SessionRestore/`.

### Filtering and Search

T014. Current-folder filter contract
- Covers: R28-R31, I4, S1, E2, AC4, EC4
- Level: unit | integration
- Fixture/setup: Active listing with mixed-case names, extension variants, folders, and a fake recursive-search/indexer probe that fails if called.
- Steps: Apply substring filters, clear filters, use names containing shell/search metacharacters, and verify no recursive search or Windows Search path is invoked.
- Expected result: Only current-folder visible items are narrowed by name substring; clearing restores the list; recursive/indexed search remains idle.
- Failure proves: Filtering is coupled to search or unsafe input handling.
- Automation location: `tests/VeloFile.Core.Tests/Filtering/`, `tests/VeloFile.App.Tests/Search/`.

T015. Recursive search streaming, cap, and cancellation
- Covers: R32-R33, E3, E9, AC5, EC26
- Level: unit | integration
- Fixture/setup: Generated deep-tree search corpus with more than 10,000 matching items and controllable traversal delay.
- Steps: Start search explicitly, observe first streamed results, reach the 10,000-result cap, cancel before and after cap, refine/start new query.
- Expected result: Results stream progressively, stop adding after 10,000, preserve displayed results, show "result limit reached", and cancellation remains available before and after cap.
- Failure proves: Search can become unbounded, non-cancellable, or ambiguous at the cap.
- Automation location: `tests/VeloFile.Core.Tests/Search/`, `scripts/run-compat-corpus.ps1 -Scope search`.

T016. Recursive search skipped locations and loops
- Covers: R34, R35, I9, O7, EC6, EC7
- Level: integration
- Fixture/setup: Search corpus with access-denied branches, reparse-point loops, and optional glob-enabled configuration.
- Steps: Search through corpus; inspect skipped-location reporting; if glob syntax is implemented, run documented supported patterns and rejected patterns.
- Expected result: Access-denied branches and loops are reported without aborting the whole search when continuing is possible; glob behavior is either absent or documented and tested.
- Failure proves: Search can recurse unboundedly, hide failures, or ship undocumented optional behavior.
- Automation location: `tests/VeloFile.Core.Tests/Search/`, `tools/VeloFile.Corpus/`.

### File Operations and Compatibility

T017. Operation contracts, Recycle Bin default, and visible status
- Covers: R36-R38, R43, I1, O7, E4, AC6
- Level: unit | integration
- Fixture/setup: Generated operations scratch corpus with normal files/folders and Shell operation adapter fake plus Windows adapter contract.
- Steps: Invoke rename and normal delete through command layer; verify operation request/result/progress states and adapter calls.
- Expected result: Rename/delete use the file-operation service boundary; normal delete uses Recycle Bin behavior where supported; progress/completion/cancellation/failure states are visible.
- Failure proves: Destructive behavior bypasses the safety boundary or UI status contract.
- Automation location: `tests/VeloFile.Core.Tests/Operations/`, `tests/VeloFile.Windows.Tests/ShellOperations/`, `scripts/run-compat-corpus.ps1 -Scope safe-delete`.

T018. Permanent delete confirmation and no-undo rule
- Covers: R39, R40, R45, I2, S5, E5, AC6, EC12
- Level: unit | integration | e2e
- Fixture/setup: Operations scratch corpus and UI confirmation harness.
- Steps: Invoke `Shift+Delete`; cancel confirmation; confirm permanent delete; attempt delete when Recycle Bin is unavailable; inspect undo command availability.
- Expected result: Permanent delete requires distinct gesture and explicit confirmation; cancel does not delete; destructive fallback from unsupported Recycle Bin still requires confirmation; undo is not offered for permanent delete.
- Failure proves: VeloFile can permanently delete data without the required user intent.
- Automation location: `tests/VeloFile.Core.Tests/Operations/`, `tests/VeloFile.App.Tests/Dialogs/`.

T019. Copy/move conflicts, progress, cancellation, and undo eligibility
- Covers: R36, R37, R41-R45, R43, R44, A11Y3, AC7, EC13, EC14
- Level: unit | integration
- Fixture/setup: Operations corpus with same-name collisions, partially cancellable batches, and completed move/rename/Recycle Bin delete records.
- Steps: Copy/move items into conflicting destinations; choose skip, replace, keep both, apply-to-batch; cancel after partial completion; inspect progress and undo eligibility.
- Expected result: Conflicts queue or pause for resolution without silently aborting unrelated work; progress/failure/completion are visible; supported recent move/rename/Recycle Bin delete are undo-eligible; permanent delete is not.
- Failure proves: Batch operations are unsafe, opaque, or non-recoverable.
- Automation location: `tests/VeloFile.Core.Tests/Operations/`, `scripts/run-compat-corpus.ps1 -Scope operations`.

T020. Long paths, junctions, symlinks, and reparse corpus
- Covers: R46, C5, EC5, AC9
- Level: integration | manual
- Fixture/setup: Compatibility corpus with long paths, junctions, symlinks, reparse points, and access constraints.
- Steps: Enumerate, search, copy, move, rename, delete, and open items in the corpus according to the documented compatibility behavior.
- Expected result: Behavior is defined before release; successes and clear Windows-denied failures match the corpus expectations.
- Failure proves: V1 compatibility behavior is undefined for risky Windows path shapes.
- Automation location: `tests/VeloFile.Compatibility.Tests/`, `scripts/run-compat-corpus.ps1 -Scope paths`.

### Commands, Context Menu, Clipboard, Terminal, and File Associations

T021. Built-in context menu only
- Covers: R47-R49, I5, S8, E8, AC8, EC20
- Level: unit | integration | e2e
- Fixture/setup: Selected file/folder, built-in command registry, installed system with known Shell extension menu entries.
- Steps: Open context menu by mouse and keyboard; inspect menu verbs and backing command providers.
- Expected result: Menu contains V1 core verbs and VeloFile-specific commands when applicable; OS Shell extension menu entries are absent and no third-party context menu handlers are enumerated.
- Failure proves: V1 shipped the excluded OS shell menu path or missed core context verbs.
- Automation location: `tests/VeloFile.Core.Tests/Commands/`, `tests/VeloFile.App.Tests/ContextMenu/`.

T022. Keyboard selection and command routing
- Covers: R50, R51, A11Y1-A11Y3
- Level: unit | integration | e2e
- Fixture/setup: File list with multiple items and keyboard automation.
- Steps: Exercise single selection, multi-selection, `Ctrl+A`, `Escape`, arrow focus, `Shift` range, `Ctrl` toggle, Enter, `F2`, `Delete`, `Shift+Delete`, `F5`, Backspace, `Ctrl+Shift+C`, and `Ctrl+Shift+N`.
- Expected result: Selection/focus/command states match the spec and do not fire file operations from text-input focus accidentally.
- Failure proves: Explorer muscle-memory behavior or keyboard accessibility is broken.
- Automation location: `tests/VeloFile.Core.Tests/Selection/`, `tests/VeloFile.App.Tests/Keyboard/`.

T023. Copy path and copy name clipboard output
- Covers: R52, R53
- Level: unit | integration
- Fixture/setup: Selected single and multiple items with absolute Windows paths and names containing spaces/metacharacters.
- Steps: Invoke copy path and copy name from keyboard and menu; read clipboard through adapter.
- Expected result: Copy path writes absolute Windows paths for selected items; copy name writes selected item names only; no test assumes a separator format that the feature spec does not define.
- Failure proves: Clipboard commands expose wrong data or unsafe formatting.
- Automation location: `tests/VeloFile.Core.Tests/Commands/`, `tests/VeloFile.Windows.Tests/Clipboard/`.

T024. Terminal discovery and default ordering
- Covers: R54-R56, R55, E11, AC12
- Level: unit | integration
- Fixture/setup: Fake probe results for Windows Terminal, PowerShell 7, Windows PowerShell, Command Prompt, Git Bash, and WSL distributions; timeout-controlled probes.
- Steps: Run discovery with all, some, and no optional targets; simulate slow probes.
- Expected result: Default order is Windows Terminal, PowerShell 7, Windows PowerShell, Command Prompt; Git Bash and WSL are selectable when available but do not outrank defaults unless user-selected; discovery does not block launch.
- Failure proves: Terminal integration violates user-choice or launch responsiveness requirements.
- Automation location: `tests/VeloFile.Core.Tests/Terminal/`, `tests/VeloFile.Windows.Tests/TerminalDiscovery/`.

T025. Safe terminal launch and failure states
- Covers: R57-R59, S1-S2, I9, E11, EC18, AC12
- Level: unit | integration
- Fixture/setup: Folder paths with shell metacharacters, inaccessible working directory, missing terminal target, and process-launch spy.
- Steps: Invoke Open terminal here explicitly; inspect process launch arguments/working directory; attempt launch with missing terminal and inaccessible folder.
- Expected result: Paths are passed as data, not concatenated command text; no automatic launch occurs on navigation; failures are user-visible and leave browsing state unchanged.
- Failure proves: Terminal launch is injection-prone or state-corrupting.
- Automation location: `tests/VeloFile.Core.Tests/Terminal/`, `tests/VeloFile.Windows.Tests/ProcessLaunch/`.

T026. File association Open and Open With
- Covers: R81, R82, I8, C3-C4, EC19, AC9
- Level: integration | manual
- Fixture/setup: Test files with known associations, missing/broken default app simulation where possible, and association-change monitor.
- Steps: Invoke Open and Open With; inspect ShellExecute adapter call; verify no association changes.
- Expected result: User-configured Windows defaults are respected; Open With is available; global file associations are not modified; broken defaults show user-visible failure.
- Failure proves: VeloFile bypasses Windows association behavior or mutates system state.
- Automation location: `tests/VeloFile.Windows.Tests/ShellExecute/`, manual compatibility checklist.

### Preview, Details, Thumbnails, and Icons

T027. Preview contract states, clearing, loading, cancellation, and reasons
- Covers: R60, R67-R70, R72, S7, O7, E7, E10, EC9, EC27, AC10
- Level: unit | integration | e2e
- Fixture/setup: Fake preview providers for success, unsupported, failure with known reason, delayed loading, timeout, and stale selection.
- Steps: Select files while preview pane is open; switch selection during work; inspect previous-preview clearing, loading after 200 ms, terminal states, known failure reason, and metadata fallback.
- Expected result: Previous preview clears immediately; loading appears only after 200 ms when pending; stale work is cancelled or ignored visibly within 50 ms; terminal states are loading/success/unsupported/failed.
- Failure proves: Preview can show stale data, hide failures, or block selection.
- Automation location: `tests/VeloFile.Core.Tests/Preview/`, `tests/VeloFile.App.Tests/Preview/`.

T028. Image preview provider boundaries
- Covers: R61, R62, R67, R71, S7, EC10, EC11, AC10
- Level: integration
- Fixture/setup: Preview corpus with common image formats, >100 MB image, image exceeding 8192 by 8192 decoded dimensions, corrupt image, access-denied image, and slow decode fake.
- Steps: Preview each image case; inspect timeout, metadata-only fallback, and diagnostics.
- Expected result: Supported images render; over-size/over-dimension images skip content preview and show metadata only; image decode times out at 2 s; corrupt/access-denied cases fail closed.
- Failure proves: Image preview can exceed V1 bounds or fail unsafely.
- Automation location: `tests/VeloFile.Windows.Tests/Preview/Image/`, `scripts/run-preview-corpus.ps1 -Scope providers`.

T029. Text/code preview provider boundaries
- Covers: R61, R63, R64, R67, R71, S7, EC10, EC11, AC10
- Level: integration
- Fixture/setup: Text/code files below 1 MB, above 1 MB, above 100 MB, binary file, invalid encoding, access-denied file, and slow read fake.
- Steps: Preview each file; inspect rendered prefix, truncation indicator, metadata-only fallback, timeout, and binary refusal.
- Expected result: At most first 1 MB is read; truncation indicator appears when content continues; files over 100 MB show metadata only; text read/encoding detection times out at 1 s.
- Failure proves: Text preview is unbounded, misleading, or unsafe for binary/large files.
- Automation location: `tests/VeloFile.Windows.Tests/Preview/Text/`, `scripts/run-preview-corpus.ps1 -Scope providers`.

T030. PDF preview provider boundaries
- Covers: R61, R65, R66, R67, R71, S7, EC10, EC11, AC10
- Level: integration
- Fixture/setup: PDF corpus with valid PDF, multi-page PDF, >500 MB PDF placeholder/fake, corrupt PDF, access-denied PDF, and slow render fake.
- Steps: Preview PDF first page, navigate to a later page, preview over-limit and corrupt cases.
- Expected result: First page renders initially; later pages render only after user navigation; PDFs over 500 MB show metadata only; first-page render times out at 3 s.
- Failure proves: PDF preview is unbounded or richer than V1 promises.
- Automation location: `tests/VeloFile.Windows.Tests/Preview/Pdf/`, `scripts/run-preview-corpus.ps1 -Scope providers`.

T031. Preview non-mutation and metadata fallback
- Covers: R71, R72, I3, S1, S7, AC11
- Level: integration
- Fixture/setup: Preview corpus with known timestamps, attributes, hashes on demand where supported, unsupported files, and provider failures.
- Steps: Preview supported, unsupported, failed, over-limit, and access-denied items; compare source file timestamps/content markers before and after.
- Expected result: Preview generation does not modify source files; metadata fallback shows size, timestamps, attributes, and type when available.
- Failure proves: Preview providers are unsafe for user files or metadata fallback is incomplete.
- Automation location: `tests/VeloFile.Core.Tests/Preview/`, `tests/VeloFile.Windows.Tests/Preview/`.

T032. Thumbnails, icons, details pane, and concurrency
- Covers: R10, R67, R72, R78, EC8, AC10
- Level: integration | performance
- Fixture/setup: File list with thumbnailable images/PDFs, non-thumbnailable files, hidden/protected files, slow thumbnail fake, and concurrency probe.
- Steps: Load list thumbnails/icons; trigger slow thumbnail work; switch tabs/selections; inspect details pane.
- Expected result: Thumbnail generation times out at 500 ms per item, no more than 4 run concurrently, generic icon fallback appears, details pane shows metadata, hidden/protected visual distinction remains.
- Failure proves: Thumbnail enrichment can block navigation or violate preview concurrency bounds.
- Automation location: `tests/VeloFile.Windows.Tests/Thumbnails/`, `tests/VeloFile.App.Tests/PreviewUi/`.

### Drag/Drop, Accessibility, Benchmarks, Packaging, and Release

T033. Windows drag/drop semantics and action indicator
- Covers: R79, R80, EC24, EC25, AC9
- Level: integration | manual
- Fixture/setup: Same-volume and cross-volume generated folders, OLE data-object fakes, and manual external-app checklist.
- Steps: Drag/drop with no modifier, Ctrl, Shift, Ctrl+Shift, and right-drag or menu path where supported; inspect resolved action indicator before drop.
- Expected result: Same-volume default move, cross-volume default copy, Ctrl copy, Shift move, Ctrl+Shift shortcut, and indicator reflects resolved action.
- Failure proves: VeloFile violates Windows drag/drop muscle memory.
- Automation location: `tests/VeloFile.Core.Tests/DragDrop/`, `tests/VeloFile.Windows.Tests/OleDragDrop/`, manual compatibility checklist.

T034. Diagnostics schema, redaction, locality, retention, and markers
- Covers: R84-R87, I10, O1-O4, S3, ADR 0008, AC17
- Level: unit | integration | security
- Fixture/setup: Failures from navigation, preview, file operation, search, terminal launch, and session restore using paths containing username/sensitive filename/document title; local diagnostics directory.
- Steps: Record events, crash markers, last-action markers; rotate logs; export redacted diagnostics; scan logs and exports.
- Expected result: Only allowed fields are present; no raw paths, filenames, usernames, search queries, clipboard contents, credentials, file contents, preview text, or raw command text appear; logs remain local, rotate at 5 MB, retain at most 30 days or 50 MB, retain latest 10 crash markers, and keep latest last-action marker per category.
- Failure proves: Diagnostics violate the privacy contract or cannot support release triage.
- Automation location: `tests/VeloFile.Core.Tests/Diagnostics/`, `tests/VeloFile.Windows.Tests/Diagnostics/`.

T035. Benchmark report shape and preview-release triage gates
- Covers: R46, R88, R89, O2, O5-O6, P1, P14-P16, AC15, AC17
- Level: integration | performance | release
- Fixture/setup: M15 benchmark harness, generated reference corpus, diagnostic sample inputs, and preview-release policy document.
- Steps: Run benchmark harness and compatibility aggregation; inspect report metadata/distribution fields; simulate crash/hang marker counts around triage threshold; inspect release decision output.
- Expected result: Reports include environment, run count, median, p95, p99, release-gating status; promotion threshold is documented; exceeded thresholds block promotion until triage/mitigation/exception; no public performance claims exist before harness/corpus evidence.
- Failure proves: Release readiness cannot be judged repeatably or diagnostics thresholds are undefined.
- Automation location: `tools/VeloFile.Benchmarks/`, `scripts/run-benchmarks.ps1`, `docs/release/preview-triage.md`.

T036. Accessibility and UX state checks
- Covers: R20, R60, R77, R78, R83, A11Y1-A11Y7, AC19
- Level: integration | manual
- Fixture/setup: Keyboard automation, accessibility inspection tools, mixed-DPI manual environment, dialogs/states for navigation, operations, preview, search, and settings.
- Steps: Navigate every V1 command surface by keyboard; inspect focus visibility, destructive confirmation text, readable mixed-DPI text/icons, and distinct loading/unsupported/failed/empty states.
- Expected result: Every required workflow has a keyboard path; focus remains visible; confirmations state destructive consequences; state surfaces are distinct and accessible.
- Failure proves: V1 is not keyboard-first or accessible enough for the approved UX contract.
- Automation location: `tests/VeloFile.Accessibility.Tests/`, manual accessibility checklist.

T037. MSIX package, stable update channel, uninstall, and rollback
- Covers: R1-R3, R93, C1-C2, C9, AC1, AC16, QS-MSIX-ROLLBACK-01
- Level: smoke | manual | release
- Fixture/setup: Release package build, signed MSIX or documented unsigned local package, stable update channel docs, rollback/uninstall instructions.
- Steps: Publish, package, install, launch, update or roll back where available, uninstall, then open Explorer and known file associations.
- Expected result: VeloFile installs side by side, has published release source/signing identity/versioning/update cadence/rollback docs, uninstalls cleanly, and leaves Explorer and associations usable.
- Failure proves: Distribution or rollback breaks the trust boundary.
- Automation location: `packaging/msix/`, `scripts/package-msix.ps1`, `scripts/release-verify.ps1`, manual release checklist.

T038. Release notes and user docs
- Covers: R35, R90-R92, S6, AC18, EC20
- Level: contract | manual
- Fixture/setup: Release notes, README/user docs, "Differences from File Explorer" section, settings help text.
- Steps: Inspect docs for extension-display default, `invoice.pdf.exe` safety case, per-application setting scope, absence of OS shell menu integration, and optional glob documentation only if glob syntax ships.
- Expected result: Divergences and optional behavior are documented without claiming unsupported safety or feature behavior.
- Failure proves: Users will encounter deliberate V1 differences without explanation.
- Automation location: docs link/content checks in `scripts/release-verify.ps1`, manual docs review.

T039. App-level benchmark scenarios
- Covers: R7, R9, R11, R37, R56, R83, O5, P1-P13, QS-RESP-01, QS-SLOW-TAB-01
- Level: performance
- Fixture/setup: M15 generated small, medium, large, deep, preview, and slow-location corpus on documented baseline machine class.
- Steps: Measure cold/warm launch, small/medium/large folder switch, current-folder filter latency, recursive search first result and 1,000-result milestone, built-in context menu open, tab switch, session restore, sustained scroll, slow-tab isolation, and terminal discovery launch impact.
- Expected result: Harness records p95 against configured targets plus p99/median/environment; no public claims are made unless reports exist.
- Failure proves: Responsiveness goals are not measurable or are regressing without release evidence.
- Automation location: `tools/VeloFile.Benchmarks/`, `scripts/run-benchmarks.ps1`.

T040. End-to-end V1 acceptance smoke
- Covers: R1, R5-R6, AC2
- Level: e2e | smoke
- Fixture/setup: Built app with smoke corpus and safe scratch roots after M16.
- Steps: Launch VeloFile, open a folder, navigate breadcrumb/sidebar/tab history, filter, search, preview, copy path, perform safe operation in scratch root, open terminal, restore session, and exit/relaunch.
- Expected result: Core workflows compose without corrupting browsing state, user files, diagnostics privacy, or session restore.
- Failure proves: Individually tested features do not compose into a usable V1 file manager.
- Automation location: `tests/VeloFile.App.Tests/EndToEnd/`, `scripts/release-verify.ps1`.

## Fixtures and Data

M2 creates the minimal deterministic corpus foundation:

- `smoke`: small folder tree for launch, navigation, sidebar, and basic file-list tests.
- `operations`: scratch-only files/folders for rename, delete, copy/move, collisions, partial cancellation, and undo eligibility.
- `preview`: common images, bounded text/code, PDF, unsupported, corrupt, access-denied, over-limit, and timeout-provider cases.
- `search`: deep tree, >10,000 result cases, access-denied branches, and reparse-loop cases.
- `large-folder`: placeholder/full profile for 100,000-item enumeration and virtualization checks.
- `pathological`: long paths, junctions, symlinks, reparse points, and unavailable/removable/mapped-location simulations.

Fixture rules:

- All destructive tests run under an explicit scratch root created for that test run.
- Corpus scripts refuse ambiguous roots and never write outside the scratch root.
- Tests that require Windows-specific behavior are skipped or marked manual on unsupported OS/permission environments, but release validation must run them on supported Windows.
- Slow and timeout cases prefer fake adapters or controllable providers for deterministic unit/integration tests; app-level benchmark timing uses the generated corpus and real app process.

## Mocking/Stubbing Policy

- `VeloFile.Core` unit tests use fakes for file-system, clock, dispatcher, diagnostics, process launch, Shell operation, thumbnail, preview, and persistence adapters.
- `VeloFile.Windows` tests must include adapter contract tests against real Windows APIs where feasible, especially file operations, ShellExecute, drag/drop formats, thumbnails/icons, terminal launch, and atomic replacement behavior.
- UI tests may use fake providers to force loading, unsupported, failed, timeout, cancellation, and missing-location states.
- Shell-owned file operation behavior may be represented by adapter fakes in unit tests, but compatibility corpus tests must exercise the real adapter before release.
- Terminal tests must inspect structured process arguments or working-directory usage; tests must not assert behavior by parsing concatenated shell command strings.
- Diagnostics tests use redaction scans over serialized event/log/export payloads rather than trusting event constructors.

## Migration or Compatibility Tests

Migration coverage is primarily T012, T013, and T037:

- Versioned session/settings/favorites/recent-location documents include durable headers from first release.
- Unknown fields are ignored.
- Malformed optional fields fall back per field.
- Malformed structural fields recover from last-known-good or safe defaults.
- Newer schema versions degrade to known fields where safe.
- Missing restored paths remain visible with close action.
- Removed monitor/window placement falls back onto an available monitor.
- MSIX update/uninstall/rollback does not break Explorer, file associations, or launch with older/newer local state.

Compatibility coverage is primarily T020, T026, T033, T035, T037, and manual QA:

- Windows 10 and 11 support.
- Long paths, junctions, symlinks, and reparse points.
- File associations and Open With without association mutation.
- OLE drag/drop semantics and cross-app manual checks.
- Per-monitor DPI manual checks.
- Stable update channel and rollback instructions.

## Observability Verification

Diagnostics and benchmark observability are covered by T034 and T035.

Required assertions:

- Local crash markers are recorded for preview builds.
- Last-action markers identify high-level categories only.
- Diagnostic logs include enough redacted context for navigation, operation, preview, search, terminal, and session fallback triage.
- No diagnostics upload happens by default.
- Logs rotate at or before 5 MB and retain at most 30 days or 50 MB total.
- Crash markers retain at most the latest 10 markers.
- Last-action markers retain only the latest marker per category.
- Benchmark reports include OS build, hardware class, CPU, RAM, storage type, Windows Search state, antivirus state when observable, DPI configuration, run count, median, p95, and p99.
- Preview-release promotion threshold documentation exists and is used by release checks.

## Security/Privacy Verification

Security and privacy checks are covered by T003, T017, T018, T025, T031, T034, T037, and T038.

Required assertions:

- File and folder names are treated as untrusted input in search, terminal launch, diagnostics, preview, and operations.
- Terminal launch does not concatenate folder paths into shell command text.
- Permanent delete requires a distinct gesture and explicit confirmation, including keyboard invocation.
- Preview providers fail closed to metadata-only fallback and do not modify source files.
- Diagnostic logs and exports contain no file contents, raw paths, raw file names, usernames, search queries, clipboard contents, credentials, secrets, preview text, or raw terminal command lines.
- V1 does not host third-party Shell extension menu handlers.
- Signed release documentation and rollback instructions preserve user trust without Explorer replacement.

## Performance Checks

Performance checks become meaningful only after M15 accepts the benchmark harness and reference corpus. Until then, M2's benchmark script is non-gating and validates report shape only.

Benchmark scenarios:

- Cold launch to first interactive frame.
- Warm launch to first interactive frame.
- Small folder switch, warm cache.
- Medium folder switch, warm cache.
- Large folder first viewport, warm cache.
- Current-folder filter latency on medium corpus.
- Recursive search first result on deep corpus.
- Recursive search 1,000-result milestone on deep corpus.
- Built-in context menu open.
- Tab switch to rendered viewport.
- Session restore for 10-tab session.
- Sustained large-folder scroll frame timing.
- Slow-tab isolation while another tab enumerates unavailable or slow storage.

Benchmark release policy:

- Report median, p95, p99, run count, and environment.
- Use p95 as the release target basis.
- Regressions over 10% require explicit ADR or release-note acknowledgement.
- Regressions over 25% block release unless a later accepted proposal or ADR changes the policy.
- No public performance claim is allowed until harness and corpus evidence exists.

## Manual QA Checklist

Manual checks required before V1 release:

- Install, launch, update/rollback where available, and uninstall MSIX on Windows 10 and Windows 11.
- Confirm Explorer remains available and file associations remain usable after install and uninstall.
- Verify mixed-DPI monitor behavior with sharp text/icons and no off-screen restored window.
- Verify cross-app drag/drop with Explorer, a browser, an IDE, and Office or a representative Office app where available.
- Verify Open and Open With with real user-default associations.
- Verify at least Windows Terminal, Windows PowerShell, and Command Prompt launch behavior on a real machine; verify PowerShell 7, Git Bash, and WSL when installed.
- Review release notes and "Differences from File Explorer" docs for extension display and absence of OS shell menu integration.
- Review accessibility by keyboard-only use and accessibility inspection tools.
- Review signed release-channel metadata: published release source, signing identity, versioning policy, update cadence expectation, and rollback/uninstall instructions.

## What Not To Test

- Cross-platform behavior, because V1 targets Windows only.
- Global Explorer replacement behavior, because V1 must not provide it.
- OS shell menu integration and third-party Shell extension hosting, except negative tests that prove they are absent.
- Cloud sync, P2P sync, FTP/SFTP, archive-as-filesystem browsing, folder synchronization, duplicate finding, AI classification, plugin marketplace, full scripting, dual-pane, batch rename, tagging, customizable toolbar, color labels, or theme engine behavior.
- Content search, indexed search, or Windows Search integration for V1 search.
- Office document preview, media playback, RAW image preview, archive preview, audio/video scrubbing, or rich syntax highlighting.
- Durable cross-session undo or resumable in-flight file operations.
- Telemetry upload or crash-report upload without a separate future opt-in spec.

## Uncovered Gaps

None requiring a spec or architecture change before implementation planning continues.

Implementation will still need to choose concrete test frameworks, UI automation approach, and exact benchmark baseline machine documentation during M1/M15. Those are execution details already owned by the approved plan and ADR 0003.

## Next Artifacts

- M1 implementation under [2026-05-04-v1-product-scope.md](../docs/plans/2026-05-04-v1-product-scope.md).
- M1 creates the real solution, test projects, and CI commands that make this test spec executable over time.

## Follow-on Artifacts

None yet.

## Readiness

Active and ready for `implement` at M1. M1 is responsible for creating the real solution, test projects, and CI commands that make this test spec executable over time.
