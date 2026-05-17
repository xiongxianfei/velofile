# UI Shell Visual Coherence Execution Plan

## Status

done

This plan is closed. M1-M7 closed, optional M8 was not triggered, the visual-evidence gate amendment was approved, final verification passed, and PR #3 merged after hosted CI passed.

## Purpose / Big Picture

This plan turns the UI shell visual-coherence spec into reviewable implementation slices. The work extends the accepted first UI design-system slice from file-list resources into the whole WinUI shell: app root, chrome, sidebar, command band, file list, status/operation surfaces, preview/details, deterministic fixture icons, and optional visual-review artifact guardrails.

The main risk is mixing visual redesign with behavior changes. This plan keeps the contract/tooling work first, applies the shell surface foundation before region polish, preserves App/Core/Windows behavior routes, and treats screenshots as soft review evidence rather than proof of real platform behavior.

## Source Artifacts

| Artifact | Path | Status |
|---|---|---|
| Proposal | [2026-05-11-shell-visual-coherence-follow-up.md](../proposals/2026-05-11-shell-visual-coherence-follow-up.md) | accepted |
| Feature spec | [specs/ui-shell-visual-coherence.md](../../specs/ui-shell-visual-coherence.md) | visual-evidence amendment approved by spec-review-r2 |
| First-slice UI spec | [specs/ui-design-system-shell-redesign.md](../../specs/ui-design-system-shell-redesign.md) | approved; remains authoritative for first-slice scope |
| Architecture package | [docs/architecture/system/architecture.md](../architecture/system/architecture.md) | visual-evidence amendment approved by architecture-review-r1 |
| ADR 0010 | [0010-shell-visual-coherence-contracts.md](../adr/0010-shell-visual-coherence-contracts.md) | accepted; visual-evidence amendment approved by architecture-review-r1 |
| Existing UI plan | [2026-05-11-ui-design-system-shell-redesign.md](2026-05-11-ui-design-system-shell-redesign.md) | done |
| Test spec | [specs/ui-shell-visual-coherence.test.md](../../specs/ui-shell-visual-coherence.test.md) | visual-evidence amendment approved by test-spec-review-r2 |

Proposal review, spec review, architecture review, plan review, and test-spec authoring were complete for the prior contract. The 2026-05-17 amendment removes mandatory visual-evidence gates; spec-review-r2 approved the feature-spec amendment, architecture-review-r1 approved the architecture/ADR amendment, plan-review-r4 approved the plan amendment, and test-spec-review-r2 approved the test-spec amendment. M4 code review can rely on the amended contract.

## Context and Orientation

The current UI design-system slice already added `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`, `tools/VeloFile.UiContracts`, token resource dictionaries under `src/VeloFile.App/Resources/Tokens/`, file-list component resources under `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, guarded fixture launch support under `src/VeloFile.App/Testing/`, and first file-list visual baselines under `tests/visual/baselines/winui/dark-comfortable-1440x900-100/`.

The visible shell still routes through [MainWindow.xaml](../../src/VeloFile.App/MainWindow.xaml), [MainWindow.xaml.cs](../../src/VeloFile.App/MainWindow.xaml.cs), [AppShellViewModel.cs](../../src/VeloFile.App/ViewModels/AppShellViewModel.cs), and existing Core/App/Windows services. The follow-on must consume existing shell state and command routes rather than introducing new navigation, listing, search, preview, file-operation, drag/drop, terminal, diagnostics, persistence, or Windows adapter behavior.

The UI contract tool currently validates first-slice token and scope rules without launching the app. This plan expands that same validation path additively instead of creating a second token authority. Screenshot evidence remains review evidence; generated `tests/visual/current/` and `tests/visual/diffs/` stay uncommitted.

## Reference Gap Handling

`hifi-design/` is a visual-quality and component-anatomy reference only. It is not the production source of truth, not a token authority, not an implementation contract, and not a pixel-parity target.

Each region milestone should compare the production shell against the reference at the level of user-visible quality: hierarchy, spacing rhythm, state clarity, icon polish, density, accessibility, and whole-shell coherence. A milestone does not need pixel parity with the reference. It must either close the relevant quality gap for that governed region or record an accepted temporary deviation in `docs/ui/design-deviations.md`.

Reference comparison must not override approved V1 behavior, architecture, accessibility, performance, security, privacy, or Windows-native constraints. The goal is to meet or exceed the reference quality using VeloFile-owned WinUI resources, fixtures, validation, and evidence. A VeloFile region may intentionally differ when the production design is more readable, more accessible, more Windows-native, more performant, easier to maintain, or better aligned with approved V1 behavior.

Optional visual-review artifacts must be privacy-safe. Diagnostic screenshots that expose private local paths, real user profile names, or unrelated foreground windows may be useful debugging evidence, but they are not accepted public artifacts. Optional evidence should use guarded fixture states, sanitized screenshots, or a recorded manual note.

## Non-Goals

- Replacing or rewriting the approved first-slice UI design-system spec or plan.
- Pixel parity with `hifi-design/` or importing production resources from `hifi-design/`.
- Persisted theme or density preferences.
- Broad theme engine, tweak panel, per-component customization, color-label system, plugin UI, dual-pane browsing, or other V1 scope expansion.
- New navigation, listing, selection, search, preview, drag/drop, file-operation, terminal, diagnostics, persistence, or Windows adapter behavior.
- New file-list selection system, row behavior model, virtualization behavior, or custom row control.
- Real Windows Shell icons for deterministic fixture baselines.
- Hard-gated screenshot pixel comparison.
- New `DebugUiTest` configuration unless a later architecture decision approves it.
- Lowering the supported minimum shell size to `720 x 500`.

## Requirements Covered

| Requirement area | IDs | Primary milestone(s) |
|---|---|---|
| Authority, scope, V1 behavior preservation, non-goals | R1-R8, I1-I6, C1-C7 | M1-M8 |
| Additive token and scope contracts, deviation records | R9-R15, AC2-AC5, AC19 | M1-M2, M4-M7 |
| Visual-coherence rubric | R16-R22 | M2-M8 |
| Shell surface foundation | R23-R27, AC4 | M2 |
| File-list polish and deterministic fixture icons | R28-R43, AC6-AC8, AC16 | M3 |
| Command band | R44-R49, AC15 | M4 |
| Sidebar behavior and visual hierarchy | R50-R56, AC14 | M5 |
| Status, operation, and destructive surfaces | R57-R61 | M6 |
| Preview/details pane | R62-R65 | M7 |
| Optional visual artifacts and sidecar guardrails | R66-R77, AC9-AC13 | Optional M8 when artifacts are used |
| Behavior-preservation matrix | R78-R82, AC17-AC18, AC20 | M1-M8 through the matching test spec |
| Security, privacy, accessibility, performance | S1-S6, A11Y1-A11Y9, P1-P7, O1-O5 | M1-M8 |

## Milestones

## Optional Visual Artifact Rule

Shell visual-coherence region milestones do not require screenshots or manual full-shell visual-review notes before closeout. Region slices close through static resource validation, behavior-preservation tests, accessibility checks, and required deviation records.

Screenshots, sidecars, and manual visual-review notes may still be recorded as optional supporting review artifacts. When present, they must be privacy-safe, labeled as supporting context, and must not be treated as behavior or release proof.

The prior M3-only visual-evidence deferral is superseded by this rule. Optional M8 visual-artifact consolidation is needed only if this initiative records optional screenshots, sidecars, or manual visual notes that need inventory or baseline guardrail review.

### M1. Shell Contract and Validator Extension

- Milestone state: closed
- Goal: Extend the existing V1 UI contract chain for shell-wide scopes, fixture icon invariants, optional screenshot sidecar metadata, and behavior-preservation matrix hooks before changing shell visuals.
- Requirements: R1-R15, R66-R82, O1-O5, S1-S6, AC2-AC5, AC7, AC12, AC17-AC20, ADR 0010.
- Files/components likely touched: `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`, `tools/VeloFile.UiContracts/`, `tests/fixtures/ui-contracts/`, `tests/VeloFile.Corpus.Tests/UiContracts/`, `tests/VeloFile.App.Tests/`.
- Dependencies: approved plan review; matching test spec must define traceable tests before implementation.
- Tests to add/update: failing tests for additive shell tokens/scopes, forbidden raw/default governed visuals, forbidden icon controls/text chips, required fixture icon resource keys, optional sidecar required fields/privacy when sidecars are used, and behavior-matrix inventory shape.
- Implementation steps:
  - Add additive shell-wide token entries or document why existing tokens cover the governed shell region.
  - Add additive governed scopes for shell foundation, command band, sidebar, status/operation, preview/details, and fixture icon resources as those scopes become active.
  - Extend `tools/VeloFile.UiContracts` so it can validate shell scopes, icon-resource invariants, and optional full-shell screenshot sidecars without launching the app.
  - Add controlled valid/invalid fixtures for `<SymbolIcon`, `<PathIcon`, private-use glyph usage, ellipsized icon chips, unapproved icon sizes, missing sidecar fields, and sidecar privacy violations.
  - Add or update `docs/ui/design-deviations.md` templates for temporary shell-region mismatch records.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "UiContracts|Visual"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet build VeloFile.sln -c Debug`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Shell-wide static validation fails fast for governed contract, icon, and optional sidecar violations while legacy out-of-scope XAML remains outside enforcement.
- Commit message: `M1: extend shell UI contract validation`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: The validator could overreach into legacy XAML and block unrelated surfaces.
- Rollback/recovery: Revert contract/tool/test changes; no production shell visuals or behavior routes are changed in M1.

### M2. Shell Surface Foundation

- Milestone state: closed
- Goal: Apply one dark comfortable shell surface model to app root, chrome, sidebar, content, command band, status area, and preview/details containers before isolated region polish.
- Requirements: R16-R27, R57, R62, A11Y1-A11Y3, A11Y6, P1-P2, AC4-AC5.
- Files/components likely touched: `src/VeloFile.App/App.xaml`, `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/Resources/Tokens/*.xaml`, new or extended `src/VeloFile.App/Resources/Components/VeloFile.Shell.xaml`, `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M1 contract/tooling; first-slice resource dictionaries remain authoritative for file-list row resources.
- Tests to add/update: static XAML tests for merged shell dictionaries, app root/chrome/sidebar/content/status/preview resource references, no raw light/default governed surfaces, focus/accent/danger separation, and no removal of existing route controls.
- Implementation steps:
  - Add shell surface component resources for app root, chrome, sidebar, content, command band shell container, status area, preview/details container, separators, and text hierarchy.
  - Merge new resources through `App.xaml`.
  - Replace governed shell container literals in `MainWindow.xaml` with named VeloFile resources.
  - Keep layout structure and command/event bindings intact unless a spec requirement explicitly allows region-level reorganization later.
  - Record any temporary mismatch between redesigned and non-redesigned regions in `docs/ui/design-deviations.md`.
  - Keep M2 focused on usable shell foundation only: app root, chrome, sidebar/content backgrounds, command-band/status/preview containers, text hierarchy, control foreground/background/borders, focus/accent/danger separation, and readable default shell evidence.
  - Assign deeper reference-quality gaps for file-list details rhythm, command-band anatomy, sidebar item anatomy, operation surfaces, and preview/details states to M3-M7 instead of expanding M2.
  - Add a milestone reference-comparison note that identifies reference patterns considered, production decisions, improvements over the previous shell, intentional differences, deviation status, and behavior routes preserved.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurface|UiDesign|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: The default shell no longer mixes raw light/default chrome or sidebar surfaces with the dark file list, and existing shell routes remain present.
- Commit message: `M2: apply shell surface foundation`
- Milestone closeout:
  - validation passed
  - shell-default resource/static checks show the app is readable and usable
  - no garbled or localized toggle state text is visible
  - no raw light-theme control surface dominates the dark shell
  - any remaining reference-quality gap is assigned to M3-M7 or recorded as a deviation
  - no new light/dark split or surface mismatch introduced without a deviation record
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Surface changes can expose contrast, focus, or clipped-control issues at small sizes.
- Rollback/recovery: Revert the shell resource dictionary and `MainWindow.xaml` resource substitutions; Core and Windows adapters remain untouched.

### M3. File-List Polish and Deterministic Fixture Icons

- Milestone state: closed
- Goal: Complete the file-list follow-on polish and replace placeholder-like fixture icon chips with governed deterministic XAML vector resources.
- Requirements: R28-R43, A11Y1-A11Y3, A11Y8, P1-P4, AC6-AC8, AC16, AC21.
- Files/components likely touched: `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`, `src/VeloFile.App/Testing/UiFixtureRegistry.cs`, `src/VeloFile.App/ViewModels/FileListRowViewModel.cs`, `src/VeloFile.App/MainWindow.xaml`, `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `tests/VeloFile.App.Tests/UiFixtures/`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M1 validator support and M2 shell foundation.
- Tests to add/update: icon invariant tests rejecting `SymbolIcon`, `PathIcon`, private-use glyphs, arbitrary icon resource keys, text chips, missing required `VfIconGeometry*` resources, missing icon container/path styles, and row-height changes across fixture icon kinds.
- Implementation steps:
  - Add `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` with required geometry resources and `VfFileListIconContainerStyle`, `VfFileListIconPathStyle`, and `VfFileListFixtureIconTemplate`.
  - Extend fixture row data to expose allowlisted icon kinds: `FileGeneric`, `Folder`, `Pdf`, `Image`, `Text`, `Spreadsheet`, `Executable`, `Markdown`, and `ThumbnailFallback`.
  - Hardcode fixture icon-kind mapping and reject arbitrary icon resource keys.
  - Refine file-list normal, hover, selected, focused, selected-focused, multi-selected, hidden, protected/system, long filename, folder, thumbnail fallback, and empty-folder visual states.
  - Add reference-quality details-view targets: header, stable column rhythm, metadata typography, selected-row accent rail or equivalent, calmer separators, hidden/protected treatment, deterministic vector icons, long-name behavior, and empty-folder state.
  - Add a milestone reference-comparison note that explains how the production file list closes or intentionally differs from the reference details-view anatomy.
  - Preserve file-list virtualization, row height, existing selection mapping, context menu routes, drag/drop target behavior, and thumbnail/preview state flow.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FileListResourceContractTests|UiFixture"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|FileList|AppShellContract|DragDrop|Preview"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Fixture file-list rows use deterministic vector icons and polished row states without changing V1 file-list behavior.
- Commit message: `M3: add deterministic file-list fixture icons`
- Milestone closeout:
  - validation passed
  - details-view header, column rhythm, metadata treatment, selection/focus, hidden/protected treatment, long names, and empty-folder state are covered by tests or evidence where touched
  - deterministic fixture icons covered by static resource/fixture tests; whole-shell visual confirmation is optional
  - no placeholder text chips such as `P...`, `D...`, or `T...` remain in governed visual fixtures
  - focus and selection are visibly distinct and not error-like
  - optional visual artifacts, if any, are labeled as supporting context only
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Fixture icon mapping could become an arbitrary resource lookup or row polish could destabilize virtualization.
- Rollback/recovery: Revert icon dictionary, fixture mapping, and file-list resource updates; restore previous first-slice file-list resources.

### M4. Command Band Visual Coherence

- Milestone state: closed
- Goal: Redesign navigation/path/filter/search controls as one intentional command band while preserving existing commands and accessibility routes.
- Requirements: R16-R22, R44-R49, R66-R77, R78-R82, A11Y1-A11Y6, AC9-AC13, AC15, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/MainWindow.xaml.cs`, `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml`, `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `tests/VeloFile.App.Tests/AppShellCommandRouteTests.cs`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M2 shell foundation; M1 validator support.
- Tests to add/update: static command-band resource tests, accessible-name/tooltip checks for icon or ambiguous controls, route-preservation tests for back, forward, up, refresh, path/breadcrumb entry, filter, search, cancel, and clear.
- Implementation steps:
  - Add command-band resources for button/input height, radius, padding, typography, hover, focus, pressed, disabled, and input border/background states.
  - Apply resources to existing navigation/path/filter/search controls.
  - Reorganize the command band only as far as required for visual coherence and minimum-size reachability; keep command handlers and view-model routes stable.
  - Add reference-quality command-band targets: compact navigation buttons, breadcrumb segment style, path input style, filter input style, search input cluster, disabled Search/Cancel/Clear states, shared height/radius/spacing, and clear row hierarchy.
  - Add a milestone reference-comparison note that explains how the production command band closes or intentionally differs from the reference toolbar/breadcrumb/search anatomy.
  - Record touched behavior-preservation matrix rows in the plan once the matching test spec exists.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "AppShellCommandRouteTests|CommandBand|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: The top shell controls read as one navigation/search band and all existing command routes remain available.
- Commit message: `M4: style the shell command band`
- Milestone closeout:
  - validation passed
  - path, filter, search, cancel, and clear controls remain reachable and visually integrated
  - navigation buttons, breadcrumb/path, filter, search, and action controls share a coherent height, radius, spacing, and state model
  - disabled states are readable and not confused with unavailable app state
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Layout changes could clip controls at `900 x 560` or accidentally change path/search behavior.
- Rollback/recovery: Revert command-band resources and XAML layout edits; preserve M1-M3 foundation and icon work.

### M5. Navigation-First Sidebar

- Milestone state: closed
- Goal: Reorder and style the sidebar as navigation-first while preserving access, keyboard order, accessible names, and discoverability for existing sidebar functions.
- Requirements: R16-R22, R50-R56, R66-R77, R78-R82, A11Y1-A11Y6, AC9-AC14, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/MainWindow.xaml.cs`, `src/VeloFile.App/ViewModels/AppShellViewModel.cs` only if existing view state needs presentation grouping, `src/VeloFile.App/Resources/Components/VeloFile.Sidebar.xaml`, `tests/VeloFile.App.Tests/AppShellContractTests.cs`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M2 shell foundation; M1 validator support.
- Tests to add/update: sidebar visual grouping/static resource tests, keyboard traversal order checks where feasible through XAML/static route tests, accessible names for locations/favorites/recents/drives/visibility toggles/terminal controls, route tests proving existing controls remain reachable.
- Implementation steps:
  - Add sidebar resources for group headings, navigation items, selected/hover/focus/disabled states, compact secondary sections, and terminal/visibility controls.
  - Present locations, favorites, recent entries, and drives before visibility toggles and terminal controls.
  - Add reference-quality sidebar targets: section headers, sidebar item anatomy, selected item rail or equivalent, hover/focus state, drive/favorite/recent item styles, secondary visibility controls, terminal selector styling, keyboard order, and accessible names.
  - Add a milestone reference-comparison note that explains how the production sidebar closes or intentionally differs from the reference sidebar anatomy.
  - Preserve existing sidebar commands and settings routes; do not add new sidebar product features.
  - Validate or record keyboard order and accessible names for the new grouping.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Sidebar|AppShellContract|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Terminal|Accessibility"`
  - Record keyboard order and accessible-name evidence for locations, favorites, recents, drives, visibility toggles, and terminal controls.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Sidebar hierarchy is navigation-first, visually integrated with the shell, and all previous sidebar controls remain discoverable and keyboard reachable.
- Commit message: `M5: make the sidebar navigation first`
- Milestone closeout:
  - validation passed
  - keyboard order and accessible names for locations, favorites, recents, drives, visibility toggles, and terminal controls recorded or tested
  - navigation remains primary and settings-like controls remain discoverable but secondary
  - section headers, selected/hover/focus states, and secondary controls are visually integrated with the shell surface model
  - no existing sidebar route is removed without spec approval
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Reordering can be a behavioral regression if keyboard order or discoverability is not preserved.
- Rollback/recovery: Revert sidebar resources/layout to the previous grouped order; existing state and persistence are unchanged.

### M6. Status, Operation, and Destructive Surfaces

- Milestone state: closed
- Goal: Bring operation progress, conflict/failure, cancellation, and destructive confirmation visuals into the shell surface system without changing operation behavior.
- Requirements: R16-R22, R57-R61, R66-R77, R78-R82, A11Y1-A11Y3, A11Y6, A11Y9, AC9-AC13, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.Status.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.Operations.xaml`, `tests/VeloFile.App.Tests/AppShellCommandRouteTests.cs`, operation route tests in `tests/VeloFile.Core.Tests/` or `tests/VeloFile.App.Tests/` as needed.
- Dependencies: M2 shell foundation; M4/M5 if optional screenshots need coherent command/sidebar context.
- Tests to add/update: static resource tests for status/operation/danger treatment, operation-route preservation tests, destructive confirmation route tests, no danger styling for ordinary focus.
- Implementation steps:
  - Add tokenized status/operation resources for running, cancelled, failed, conflict, progress, cancellation, and destructive confirmation surfaces.
  - Apply resources to governed operation/status surfaces.
  - Add reference-quality operation/status targets: operation running state, cancel state, failed state, conflict state, destructive confirmation hierarchy, danger styling distinct from focus/accent, and status area integration with the shell surface.
  - Add a milestone reference-comparison note that explains how the production operation/status surfaces close or intentionally differ from the reference safety-surface anatomy.
  - Preserve existing file-operation service and command routes.
  - Ensure active operation and destructive confirmation states do not obscure primary navigation in required profiles.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter "Operation|FileOperation|Status|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Operation and destructive states are visually distinct, tokenized, and behavior-preserving.
- Commit message: `M6: style status and operation surfaces`
- Milestone closeout:
  - validation passed
  - cancel, progress, failure, and destructive states remain visible without unexpectedly covering primary navigation
  - danger, warning, progress, and ordinary focus/accent states remain visually distinct
  - permanent-delete confirmation hierarchy is readable and visually distinct from ordinary actions
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Destructive styling could be confused with focus/accent state or operation surfaces could cover navigation.
- Rollback/recovery: Revert status/operation resource and XAML changes; operation services remain unchanged.

### M7. Preview and Details Surface Treatment

- Milestone state: closed
- Goal: Integrate preview/details loading, success, unsupported, failed, metadata, image, text, and PDF states into the shell surface system without destabilizing file-list layout.
- Requirements: R16-R22, R62-R65, R66-R77, R78-R82, A11Y1-A11Y6, P1-P2, AC9-AC13, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.Preview.xaml`, `tests/VeloFile.App.Tests/Preview/`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M2 shell foundation; existing preview provider architecture remains unchanged.
- Tests to add/update: static preview resource tests, preview route tests for loading/success/unsupported/failed/PDF navigation state, checks that preview visual changes do not change file-list row height or hide selected/focused rows.
- Implementation steps:
  - Add preview/details resources for container, header, metadata, loading, unsupported, failed, image/text/PDF states, and separators.
  - Apply resources to preview/details panes while preserving existing preview controller and provider routes.
  - Add reference-quality preview/details targets: empty state, loading state, unsupported state, failed state, metadata rows, image/text/PDF layout, page navigation treatment, actions, and separators.
  - Add a milestone reference-comparison note that explains how the production preview/details pane closes or intentionally differs from the reference preview anatomy.
  - Verify row selection/focus remains visible with preview open.
  - Keep preview fixture evidence synthetic and non-sensitive.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Preview|PreviewSurface|Accessibility"`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|Preview|FileListResourceContractTests"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Preview/details surfaces visually belong to the shell and existing preview behavior remains intact.
- Commit message: `M7: style preview and details surfaces`
- Milestone closeout:
  - validation passed
  - preview open state does not obscure file-list usability or primary navigation
  - preview empty, loading, unsupported, failed, metadata, image/text/PDF, and page navigation states are covered by tests or evidence where touched
  - loading, failed, unsupported, and metadata states remain visually distinct where touched
  - any accepted preview/layout mismatch is recorded as a deviation
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Preview layout can hide file-list selection/focus or introduce clipped text at required profiles.
- Rollback/recovery: Revert preview resource/layout edits; preview providers and Core behavior are unchanged.

### M8. Optional Visual Artifact Guardrails

- Milestone state: optional; not triggered
- Goal: Consolidate optional full-shell screenshot states, sidecars, evidence inventory validation, and profile classification only if optional visual artifacts are recorded during M2-M7.
- Requirements: R66-R82, O3-O4, S3-S6, A11Y6-A11Y7, AC9-AC13, AC17-AC20.
- Files/components likely touched if optional artifacts are used: `src/VeloFile.App/Testing/UiFixtureRegistry.cs`, screenshot capture helpers if present, `scripts/update-ui-baselines.ps1`, `tests/visual/baselines/winui/` or manual evidence records, `tests/VeloFile.Corpus.Tests/UiContracts/`.
- Dependencies: optional visual artifacts from M2-M7; M1 sidecar validation.
- Tests to add/update: optional visual inventory tests, sidecar metadata/privacy tests, generated-output ignore tests, optional profile classification tests, and behavior-preservation matrix citation checks from the matching test spec.
- Implementation steps:
  - Add or extend deterministic full-shell fixture states only when optional screenshots are useful for review.
  - Validate optional current screenshots or manual evidence records only when they exist.
  - Record optional `shell-min-900x560-100`, `shell-standard-1440x900-100`, or `shell-standard-1440x900-200` artifacts only when they are useful for a review question.
  - Validate sidecars for profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, review ID, and privacy exclusions.
  - Validate that deviation records exist for accepted visual mismatches.
  - Validate that each M2-M7 region has a reference-comparison note or an explicit no-reference-comparison rationale.
  - Consolidate reference-comparison notes into an evidence inventory that records reference patterns considered, production decisions, intentional differences, deviation links, and behavior routes preserved.
  - Confirm generated current/diff outputs remain uncommitted.
- Validation commands when optional visual artifacts exist:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "Visual|UiContracts"`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\update-ui-baselines.ps1 -Suite winui -Profile shell-standard-1440x900-100 -ReviewId <review-id>` only when reviewed current screenshots exist
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\update-ui-baselines.ps1 -Suite winui -Profile shell-min-900x560-100 -ReviewId <review-id>` only when reviewed current screenshots exist
  - `git status --short -- tests\visual\current tests\visual\diffs`
  - `dotnet test VeloFile.sln -c Debug --filter "Visual|UiContracts|Accessibility"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Optional full-shell visual artifacts, if any, are inventoried, sidecars are safe and complete, baseline review is traceable, and behavior-preservation citations remain separate.
- Commit message: `M8: consolidate optional visual artifacts`
- Milestone closeout:
  - validation passed
  - optional screenshot or manual visual artifact inventory is complete if artifacts exist
  - sidecars are complete and privacy-safe
  - deviation records are complete for accepted mismatches
  - region reference-comparison notes are complete
  - behavior-preservation matrix is complete
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Screenshot capture can be noisy, and `200%` scale may not be automatable.
- Rollback/recovery: Remove or regenerate the affected baseline/profile evidence; keep static resource validation and behavior tests as the primary enforcement path.

### M9. Lifecycle Closeout and Regression Verification

- Milestone state: lifecycle-closeout
- Goal: Close the shell visual-coherence initiative only after all implementation milestones are closed or explicitly removed by reviewed plan revision, review findings are resolved, durable rationale exists, and final verification passes.
- Requirements: all acceptance criteria after M1-M8.
- Files/components likely touched: this plan, change records, explain-change artifact, PR notes; no new product behavior.
- Dependencies: M1-M7 closed, optional M8 closed only if triggered, and code-review/review-resolution complete for each implementation milestone.
- Tests to add/update: none unless review findings require remediation.
- Implementation steps:
  - Ensure each in-scope implementation milestone is `closed`.
  - Run final validation and update validation notes.
  - Use `explain-change`, `verify`, and `pr` stages as required by the repository workflow.
  - Fill outcome and retrospective after final verification.
- Validation commands:
  - `dotnet --info`
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug`
  - `dotnet test VeloFile.sln -c Debug`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
  - `git diff --check`
- Expected observable result: The branch is ready for PR handoff with traceable shell visual-coherence implementation, review, and validation evidence.
- Commit message: `M9: close shell visual-coherence lifecycle evidence`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Final validation may expose regressions outside the latest milestone.
- Rollback/recovery: Identify the responsible milestone, reopen it through review-resolution, and rerun targeted plus broad validation.

## Validation Plan

Validation is layered:

- Static UI contract validation starts in M1 and runs in every implementation milestone.
- XAML/resource build and App shell route validation start in M2.
- Fixture icon invariant validation starts in M3.
- Region-specific behavior preservation runs in M4-M7.
- Optional full-shell screenshot and sidecar inventory validation runs in M8 only when optional visual artifacts are recorded.
- Broad V1 regression validation runs after production shell changes.

Minimum recurring commands after M2:

```powershell
dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .
dotnet build VeloFile.sln -c Debug
dotnet test VeloFile.sln -c Debug
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
```

Manual/review evidence:

- Review chosen optional full-shell screenshots for their recorded states and profiles when visual artifacts are recorded.
- Use static/resource tests, app tests, or explicit manual behavior notes for touched high-DPI risk when automation cannot reliably set 200% scale.
- Confirm generated `tests/visual/current/` and `tests/visual/diffs/` outputs remain uncommitted.
- Record meaningful reference or temporary-region deviations in `docs/ui/design-deviations.md`.

## Risks and Recovery

- Risk: visual work changes V1 behavior. Recovery: require touched behavior-preservation rows and existing App/Core/Windows tests before accepting each slice.
- Risk: shell scope validation blocks legacy XAML. Recovery: keep `docs/ui/ui-contract-scopes.v1.json` additive and region-scoped; activate only implemented regions.
- Risk: icons repeat the prior glyph defect class. Recovery: enforce raw vector fixture icons and reject `SymbolIcon`/`PathIcon`/private-use glyphs in governed icon scopes through static invariant tests.
- Risk: screenshots are noisy or subjective. Recovery: keep them optional soft-review artifacts and rely on static contracts, sidecars when present, and behavior tests as hard proof.
- Risk: high-DPI capture is unstable. Recovery: rely on static resource/layout tests or explicit manual behavior notes for touched high-DPI risk; optional screenshots may support review when available.
- Risk: sidebar reordering harms keyboard flow or discoverability. Recovery: treat sidebar grouping as observable behavior, preserve accessible names and keyboard order, and revert the sidebar slice if access regresses.
- Risk: region slices leave the shell mismatched. Recovery: record temporary deviations and prioritize the next dependent region, or revert the mismatching slice if it cannot be justified.

No data migration is expected. Rollback is per governed region by reverting that region's resources, scoped XAML usage, tests, and visual evidence without changing Core or Windows adapters.

## Dependencies

- `plan-review` must approve this plan before implementation.
- `specs/ui-shell-visual-coherence.test.md` must exist and be reviewed before implementation starts.
- The first UI design-system slice should remain authoritative for first-slice token/file-list row contracts.
- The existing UI design-system branch-ready plan may need to land or be rebased before this follow-on implementation to avoid resource and visual-baseline conflicts.
- Windows/.NET local validation is required. Optional full-shell screenshots require a local environment capable of launching the WinUI app when maintainers choose to record them.
- `shell-standard-1440x900-200` automation may depend on reliable high-DPI environment control; optional visual artifacts may support review when that stabilizes.

## Progress

- [x] Proposal accepted.
- [x] Spec approved.
- [x] Architecture update and ADR 0010 approved by architecture review.
- [x] Draft execution plan created.
- [x] Plan review completed.
- [x] Matching test spec created.
- [x] Matching test spec accepted.
- [x] M1 closed.
- [x] M2 closed.
- [x] M3 closed.
- [x] M4 closed.
- [x] M5 closed.
- [x] M6 closed.
- [x] M7 closed.
- [x] M8 not triggered under the amended optional visual-artifact rule.
- [x] Final verification completed.
- [x] PR handoff completed.

## Current Handoff Summary

Current milestone: post-merge closeout
Current milestone state: done
Last reviewed milestone: M7 (clean-with-notes)
Review status: code-review-r18 closed M7 with no material findings; M7 requires no review-resolution; PR #3 merged with hosted CI success
Remaining in-scope implementation milestones: none; M8 is optional and not triggered
Next stage: none
Final closeout readiness: done
Reason final closeout is or is not ready: M1-M7 are closed, optional M8 is not triggered, explain-change is recorded, final local verification passed, and PR #3 merged after hosted CI passed.

## Decision Log

| Date | Decision | Reason |
|---|---|---|
| 2026-05-11 | Keep this follow-on as a new draft plan instead of replacing the first-slice UI plan. | The first-slice plan is already active and branch-ready; this work extends it with a separate approved spec. |
| 2026-05-11 | Put shell contract/tooling extension before visual resource edits. | The spec and ADR require additive token/scope validation, icon invariants, sidecar checks, and behavior matrix proof before screenshots are trusted. |
| 2026-05-11 | Keep shell surface foundation before file-list polish and icon treatment. | The spec requires shell foundation as the first follow-on implementation region and file-list/icon work as the second. |
| 2026-05-11 | Split command band, sidebar, status/operation, and preview/details into separate implementation milestones. | Each region touches different behavior-preservation rows and should get its own review loop. |
| 2026-05-11 | Treat M9 as lifecycle closeout, not implementation. | Final closeout must not hide unfinished region work. |
| 2026-05-11 | Reuse existing first-slice dark comfortable tokens for M2 shell foundation instead of adding new token IDs. | Existing V1 tokens already cover the M2 surface, text, focus, selection, hover, disabled, danger, warning, success, spacing, radius, sizing, and focus-thickness needs. |
| 2026-05-16 | Use allowlisted `FileListIconKind` values and `Resources/Icons/VeloFile.FixtureIcons.xaml` geometry resources for M3 file-list icons. | This closes the placeholder text-chip class without introducing arbitrary resource-key fixture input or glyph-font/private-use icon rendering. |
| 2026-05-17 | Defer M3 `shell-file-list-selected-focused` full-shell visual evidence to M8. | Maintainer requested skipping the M3 evidence gate so code review can assess deterministic icon/resource and behavior-preservation work now. This decision is superseded by the approved visual-evidence-gate removal amendment. |
| 2026-05-17 | Draft removal of mandatory visual-evidence gates. | Maintainer requested removing the visual evidence requirement entirely; screenshots/manual visual notes become optional supporting artifacts and static/behavior/accessibility validation remains the hard closeout proof. |
| 2026-05-17 | Resolve CR-008 by wiring command-band state resources instead of narrowing the spec. | The M4 contract requires tokenized hover, pressed, disabled, focused, and input-border states; key existence alone was insufficient proof. |

## Surprises and Discoveries

- M2 did not require new token values; it added shell component resources that alias and consume existing token resources.
- `scripts/ci.ps1` exposed that the valid UI-contract fixture also needed the newly active shell surface scope and fixture shell dictionary. The fixture was updated so active M2 validation remains representative.
- An attempted build run overlapped a filtered test run and failed because `testhost` temporarily locked `VeloFile.Corpus.Tests.dll`; rerunning `dotnet build VeloFile.sln -c Debug` after the testhost exited passed with 0 warnings and 0 errors.
- M3 did not require new token IDs; the file-list icon container, path style, and details header reuse existing spacing, sizing, radius, typography, and text/surface resources.

## Validation Notes

Planning validation:

- `git diff --check -- docs/plans/2026-05-11-ui-shell-visual-coherence.md docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`

PR-001 review-resolution:

- Added the region-slice visual evidence rule.
- Updated M2-M7 with full-shell visual evidence validation and closeout requirements under the prior contract.
- Reframed M8 as consolidation and baseline inventory.
- 2026-05-17 amendment supersedes the region-slice visual evidence rule and makes M8 optional only when visual artifacts are recorded.

Test-spec authoring:

- Created `specs/ui-shell-visual-coherence.test.md` as the draft proof plan for shell visual-coherence implementation.
- Maintainer accepted `specs/ui-shell-visual-coherence.test.md` as the active proof plan.

M1 implementation validation:

- Added tests first in `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`.
- Confirmed the new tests failed before implementation with missing follow-on scopes, missing forbidden icon detection, and missing `--visual-root` support:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"` failed with 3 expected failures.
- Implemented additive shell scope/behavior-matrix contract entries and UI contract validator support for governed fixture icon scans and visual sidecar validation.
- Targeted validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"` passed: 3 passed.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "UiContracts|Visual"` passed: 21 passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
  - `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed after rerunning with a longer timeout; the first 5-minute attempt timed out before completion.

CR-001 review-resolution validation:

- Added direct local icon color literal coverage for governed fixture icons.
- Validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"`: 6 passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid\Resources`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\invalid\fixture-icon-local-color\Resources`: failed as expected and included `forbidden-fixture-icon-color`, `Fill`, `#FFFFFF`, and the governed icon file path.
  - `dotnet test VeloFile.sln -c Debug --filter ShellVisualCoherenceContractTests`: 6 passed.
  - `dotnet test VeloFile.sln -c Debug --filter UiContracts`: 20 passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.

M2 implementation validation:

- M2 implementation started; scope is limited to shell surface foundation resources, static XAML contract tests, scoped `MainWindow.xaml` resource consumption, manual visual evidence note, and plan/change-record state sync.
- Added tests first in `tests/VeloFile.App.Tests/UiDesign/ShellSurfaceResourceContractTests.cs`.
- Confirmed the new M2 tests failed before implementation with missing `VeloFile.Shell.xaml`, missing `App.xaml` merge, and missing `shell-surface-foundation` scope markers:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter ShellSurface` failed with 3 expected failures.
- Implemented `src/VeloFile.App/Resources/Components/VeloFile.Shell.xaml`, merged it through `App.xaml`, activated the `shell-surface-foundation` UI contract scope, and applied shell surface styles to the root shell, tab/chrome, command band container, sidebar, content, status container, and preview pane.
- Updated the valid UI-contract fixture and M1 follow-on scope test so `shell-surface-foundation` may be active while later follow-on scopes remain planned.
- Recorded soft full-shell visual evidence note at `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`. Screenshot automation was not stable/implemented for M2, and this slice did not change layout dimensions, surface sizes, titlebar/sidebar width, command-band height, content bounds, or minimum-window behavior.
- Targeted and broad validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter ShellSurface`: 4 passed after implementation.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurface|UiDesign|Accessibility"`: 18 passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes docs\ui\ui-contract-scopes.v1.json --scope-root tests\fixtures\ui-contracts\valid`: passed after fixture update.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`: App 20 passed, Corpus 20 passed; Core/Windows had no matching tests for the filter.
  - `dotnet build VeloFile.sln -c Debug`: passed with 0 warnings and 0 errors after rerunning without the parallel testhost lock.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 142/Windows 52/Corpus 37 tests passed.

CR-002 review-resolution attempt:

- Code review found that the M2 visual evidence note did not record an observed full-shell visual review.
- Checked for an existing screenshot/capture path:
  - `rg -n "capture-ui|screenshot|update-ui-baselines|visual/current|shell-default|UiFixture|test-ui-fixture" scripts src tests docs -g "!*bin*" -g "!*obj*"`
  - `Get-ChildItem scripts`
  - `Get-ChildItem src\VeloFile.App\bin -Recurse -Filter VeloFile.App.exe`
- Discovery: the repo has `scripts/update-ui-baselines.ps1` for copying reviewed current screenshots, but no checked-in script that launches the WinUI app and captures `shell-default`; this tool session cannot directly observe the local WinUI desktop.
- Updated `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md` to explicitly record visual evidence as unavailable.
- Validation after updating the evidence record:
  - `dotnet test VeloFile.sln -c Debug --filter ShellSurfaceResourceContractTests`: passed after rerun; the first parallel attempt hit a transient file lock.
  - `dotnet test VeloFile.sln -c Debug --filter ShellVisualCoherenceContractTests`: passed after rerun; the first parallel attempt hit a transient file lock.
  - `dotnet test VeloFile.sln -c Debug --filter UiContracts`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 142/Windows 52/Corpus 37 tests passed.
- Later implement-stage attempt to unblock CR-002:
  - Launched `src\VeloFile.App\bin\Debug\net8.0-windows10.0.19041.0\VeloFile.App.exe` and confirmed the app exposes a visible `VeloFile` window.
  - Attempted a local desktop screenshot capture, but the desktop reported `150%` scale rather than the required `100%` profile, and the first capture targeted the foreground browser instead of VeloFile.
  - Discarded the invalid generated current screenshot/sidecar outputs.
- Failed diagnostic screenshot resolution:
  - Updated `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md` to record the diagnostic shell screenshot as failed visual evidence, not an accepted deviation.
  - Added App UI design checks for shell readability token pairs, governed shell control foreground/background resources, compact sidebar toggles with no visible On/Off content, removal of raw light-theme foreground brushes, and non-text file-list icon rendering.
  - Applied M2 resource fixes in `VeloFile.Shell.xaml`, `MainWindow.xaml`, and `VeloFile.FileList.xaml` so shell controls share dark-surface foreground/background/border/focus resources, sidebar toggles suppress visible state text, and file-list rows render a deterministic vector icon placeholder instead of text chips.
  - Updated `docs/ui/ui-contract-scopes.v1.json` so the expanded M2 shell/file-list resources remain governed by the existing additive V1 scope contract.
  - Validation after the static/code fix:
    - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter ShellSurfaceResourceContractTests`: failed first with the expected missing-resource/default-control/icon-chip assertions, then passed after implementation.
    - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter UiDesign`: passed, 19 tests.
    - `dotnet test VeloFile.sln -c Debug --filter "ShellSurfaceResourceContractTests|FileListResourceContractTests|ShellVisualCoherenceContractTests"`: passed, App 19 and Corpus 6 matching tests.
    - `dotnet test VeloFile.sln -c Debug --filter ShellSurfaceResourceContractTests`: passed, App 8 matching tests.
    - `dotnet test VeloFile.sln -c Debug --filter ShellVisualCoherenceContractTests`: passed, Corpus 6 matching tests.
    - `dotnet test VeloFile.sln -c Debug --filter UiContracts`: passed, Corpus 20 matching tests.
    - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`: passed, App 20 and Corpus 20 matching tests.
    - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter AppShellContract`: passed, 19 tests.
    - `dotnet build VeloFile.sln -c Debug`: passed with 0 warnings and 0 errors.
    - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
    - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes docs\ui\ui-contract-scopes.v1.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
    - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 146/Windows 52/Corpus 37 tests passed.
- M2 remains `resolution-needed`; it cannot return to code review until automated screenshot evidence or an observed manual full-shell visual-review note exists for `shell-default` at `shell-standard-1440x900-100`.

Reference gap sharpening:

- Added `Reference Gap Handling` to make `hifi-design/` a benchmark and component-anatomy reference only, not a production source of truth, token authority, implementation contract, or pixel-parity target.
- Added privacy/evidence language so diagnostic screenshots with private local paths, unrelated foreground windows, or wrong profiles cannot become accepted milestone evidence.
- Added M2 closeout language that distinguishes usable shell foundation from full hi-fi-level region polish.
- Added M3-M7 reference-quality targets for file-list details rhythm, command/path/search band anatomy, sidebar component system, operation/status safety surfaces, and preview/details pane states.
- Added M8 consolidation requirements for reference-comparison notes, deviation links, privacy-safe evidence, and behavior-preservation matrix completion.

CR-003 and CR-004 review-resolution:

- CR-003 resolved by routing native titlebar colors through governed `VfTitleBar*Color` resources declared in the active `shell-surface-foundation` scope, with `MainWindow.xaml.cs` reading those resources through a narrow titlebar resource bridge instead of raw RGB literals.
- CR-004 resolved by deferring sidebar navigation-first reordering back to M5 while keeping M2 readability fixes for compact sidebar toggles, hidden visible On/Off content, tokenized sidebar labels, terminal selector styling, and dark shell integration.
- Validation passed:
  - `dotnet build VeloFile.sln -c Debug`
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurfaceResourceContractTests|AppShellContract|UiDesign"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
  - `git diff --check`
- CR-002 was resolved by maintainer-provided manual full-shell visual review recorded in `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`; M2 returned to code review.
- M2 code review R9 completed with status `clean-with-notes`; M2 is closed and the next stage is M3 implementation.

M3 implementation validation:

- Added tests first in `tests/VeloFile.App.Tests/UiDesign/FileListResourceContractTests.cs`, `tests/VeloFile.App.Tests/UiFixtures/UiFixtureRegistryTests.cs`, and `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`.
- Confirmed the new M3 tests failed before implementation with missing `VeloFile.App.Ui` icon-kind contract and missing fixture icon resources:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FileListResourceContractTests|UiFixtureRegistryTests"` failed with missing `VeloFile.App.Ui`/`FileListIconKind` compile errors.
- Implemented `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` with named `VfIconGeometry*` resources, `VfFileListIconContainerStyle`, `VfFileListIconPathStyle`, and `VfFileListFixtureIconTemplate`.
- Added `FileListIconKind`/resolver and a resource-backed geometry converter, exposed allowlisted fixture icon kinds from `UiFixtureRegistry`, and moved the row template from an inline generic icon path to `IconKind` + `VfFileListFixtureIconTemplate`.
- Added a tokenized file-list details header in the governed file-list scope to improve column rhythm without changing selection, context-menu, drag/drop, thumbnail, or preview routes.
- Activated the `fixture-icons` UI contract scope and added the M3 icon/template/header resource keys to `docs/ui/ui-contract-scopes.v1.json`.
- Updated the valid UI-contract fixture and targeted scope fixture so active M3 header/icon resources are represented in `scripts/ci.ps1`.
- Recorded deferred M3 visual evidence at `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m3-shell-file-list-selected-focused.md`; M3 whole-shell visual acceptance is not claimed, and the deferred state was required for M8 only under the prior contract.
- 2026-05-17 amendment supersedes the M3 visual-evidence deferral as a closeout requirement.
- Validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FileListResourceContractTests|UiFixtureRegistryTests"`: passed, 21 tests.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet test VeloFile.sln -c Debug --filter "FileListResourceContractTests|UiFixture|UiContracts"`: passed; App 29 matching tests and Corpus 20 matching tests.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|FileList|AppShellContract|DragDrop|Preview"`: passed; Core 29, App 54, Windows 20, and Corpus 24 matching tests.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 149/Windows 52/Corpus 37 tests passed.
  - `git diff --check`: passed with line-ending warnings only.

M3 evidence deferral amendment:

- `shell-file-list-selected-focused` full-shell visual evidence was deferred to M8 by maintainer request under the prior contract.
- M3 code review may proceed, but M3 must not claim whole-shell visual acceptance.
- M8 remained blocked until the deferred state had captured or manual-review evidence under the prior contract.
- The approved 2026-05-17 amendment supersedes this M8 blocker.

M3 code review:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r10.md`: clean-with-notes, no findings.
- M3 is closed under the approved visual-evidence deferral. No M3 whole-shell visual acceptance is claimed; the M8 blocker is superseded by the approved visual-evidence-gate removal amendment.
- The approved 2026-05-17 amendment supersedes this M8 blocker.

M4 implementation validation:

- Added `tests/VeloFile.App.Tests/UiDesign/CommandBandResourceContractTests.cs` first. The focused M4 test failed before implementation with missing `Resources/Components/VeloFile.CommandBand.xaml`, missing `shell-command-band` scope markers, inactive command-band scope metadata, and missing command-band resource references.
- Added `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml` with governed command-band surface, button, icon button, breadcrumb, input, path, radio, status, and skipped-location styles.
- Merged the command-band dictionary from `src/VeloFile.App/App.xaml`.
- Scoped existing navigation/path/filter/search XAML in `src/VeloFile.App/MainWindow.xaml` with `shell-command-band` markers and applied command-band styles without changing existing click, key, filter, search, cancel, clear, breadcrumb, or path handlers.
- Activated the `shell-command-band` scope in `docs/ui/ui-contract-scopes.v1.json` and updated the valid UI-contract fixtures to keep `scripts/ci.ps1` aligned with the active production scope.
- Updated `ShellVisualCoherenceContractTests` so `shell-command-band` is expected to be active for M4.
- Tightened `AppShellContractTests.Main_window_icon_buttons_use_raw_vectors` so it rejects actual `SymbolIcon`, `PathIcon`, glyph, and icon-font usage instead of failing on the command-band style name.
- Validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "CommandBandResourceContractTests"`: passed, 4 tests after initial expected failures.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "AppShellCommandRouteTests|CommandBand|Accessibility"`: passed, 67 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`: passed; App 64, Core 6, and Corpus 20 matching tests passed; Windows had no matching tests for this filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core 168, App 153, Windows 52, and Corpus 90 tests passed.
- M4 code/static validation is complete. Under the approved visual-evidence-gate removal amendment, M4 no longer needs `shell-filter-active` and `shell-search-active` visual evidence before code review.

M4 code review:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r11.md`: changes-requested.
- CR-005: M4 lacked required `shell-filter-active` and `shell-search-active` full-shell visual evidence under the prior contract; it is resolved/superseded by the approved visual-evidence-gate removal amendment.
- CR-006: resolved by moving older M3 validation under `history.prior_milestone_validation.M3` and keeping one current M4 `implementation.validation` key.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r12.md`: changes-requested; CR-005 remains open and CR-006 resolution is confirmed.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r13.md`: blocked; CR-007 requires upstream amendment review before M4 code review can rely on the visual-evidence gate removal.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/spec-review-r2.md`: approved; feature-spec amendment has no material findings.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/architecture-review-r1.md`: approved; canonical architecture and ADR amendment have no material findings.
- PR-002: resolved by making active validation wording conditional on optional visual artifacts and normalizing the stale M4 milestone state.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r4.md`: approved; plan amendment has no material findings.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/test-spec-review-r1.md`: revise; TSR-001 required stale test-spec readiness wording to be updated before test-spec review can approve the amendment.
- TSR-001: resolved by replacing stale M1 readiness wording with amendment-specific test-spec review readiness and recording upstream approvals in the test spec follow-on artifacts.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/test-spec-review-r2.md`: approved; test-spec amendment has no material findings.
- CR-005: resolved/superseded by the approved visual-evidence gate removal amendment.
- CR-007: resolved by the approved spec, architecture, plan, and test-spec review chain.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r14.md`: changes-requested; CR-008 requires command-band state tokens to be consumed by governed controls or the spec to narrow the requirement.
- CR-008: resolved by wiring `VfCommandBandButtonStyle` to a governed button template that consumes hover, pressed, disabled foreground/background, and disabled opacity resources; by adding scoped WinUI `TextControl*` resource aliases around governed command-band TextBoxes for normal, hover, focused, disabled, placeholder, and border states; and by adding focused tests that fail when state resources are only declared.
- CR-008 validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~CommandBandResourceContractTests"`: passed, 7 tests.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "AppShellCommandRouteTests|CommandBand|Accessibility"`: passed, 70 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`: passed; Core 6, App 64, and Corpus 20 matching tests passed; Windows had no matching tests for this filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core 168, App 156, Windows 52, and Corpus 90 tests passed.
  - `git diff --check`: passed with CRLF normalization warnings only.
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r15.md`: clean-with-notes; M4 closed with no material findings.

M5 implementation validation:

- Added `tests/VeloFile.App.Tests/UiDesign/SidebarResourceContractTests.cs` first. The focused M5 tests failed before implementation with missing `Resources/Components/VeloFile.Sidebar.xaml`, inactive `shell-sidebar` scope metadata, missing sidebar scope markers, and missing sidebar resource references.
- Added `src/VeloFile.App/Resources/Components/VeloFile.Sidebar.xaml` with governed sidebar surface, section header, navigation list, item text/container, selected/hover/focus/disabled, secondary control row, compact toggle, and terminal picker resources.
- Merged the sidebar dictionary from `src/VeloFile.App/App.xaml`.
- Scoped the sidebar in `src/VeloFile.App/MainWindow.xaml` with `shell-sidebar` markers, moved `SidebarLocationsList` before `VisibilityControls` and `TerminalControls`, and applied sidebar styles without changing existing item-click, visibility-toggle, or terminal-target handlers.
- Activated the `shell-sidebar` scope in `docs/ui/ui-contract-scopes.v1.json` and updated the valid UI-contract fixtures to keep `scripts/ci.ps1` aligned with the active production scope.
- Updated `ShellSurfaceResourceContractTests` so M2 compact toggle coverage accepts `VfSidebarToggleStyle` as the M5 sidebar-specific style based on `VfShellToggleSwitchStyle`.
- Updated `ShellVisualCoherenceContractTests` so `shell-sidebar` is expected to be active for M5.
- Keyboard/accessibility evidence recorded through static XAML checks:
  - visual and XAML focus order is navigation first: `SidebarLocationsList`, then `VisibilityControls`, then `TerminalControls`;
  - accessible names are present for `Navigation locations`, `Visibility controls`, `Show hidden files`, `Show protected operating system files`, `Show file extensions`, `Terminal controls`, and `Terminal target`;
  - access keys remain present for visibility toggles: `H`, `S`, and `E`;
  - existing sidebar routes remain wired through `SidebarLocationsList_ItemClick`, `ShowHiddenFilesToggle_Toggled`, `ShowSystemFilesToggle_Toggled`, `ShowExtensionsToggle_Toggled`, `TerminalTargetComboBox_DropDownOpened`, and `TerminalTargetComboBox_SelectionChanged`.
- Full CI initially exposed a fixture-view-model race where `File_list_fixture_view_model_renders_fixture_rows_without_disk_backing` asserted `ThumbnailFallback` before thumbnail generation updated the row view models. The test now waits for the rendered fallback icon state before asserting it.
- Validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~SidebarResourceContractTests"`: passed, 4 tests after initial expected failures.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Sidebar|AppShellContract|Accessibility"`: passed, 25 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Terminal|Accessibility"`: passed; Core 11, App 27, Windows 3, and Corpus 20 matching tests passed.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~File_list_fixture_view_model_renders_fixture_rows_without_disk_backing"`: passed, 1 test.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core 168, App 160, Windows 52, and Corpus 90 tests passed.
- M5 static/resource validation is complete. Under the approved visual-evidence-gate removal amendment, M5 does not need sidebar-focused visual evidence before code review.

M5 code review:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r16.md`: clean-with-notes; M5 closed with no material findings.

M6 implementation validation:

- Added `tests/VeloFile.App.Tests/UiDesign/StatusOperationResourceContractTests.cs` first. The focused M6 tests failed before implementation with missing `Resources/Components/VeloFile.Status.xaml`, missing `Resources/Components/VeloFile.Operations.xaml`, inactive `shell-status-operations` scope metadata, and missing `shell-status-operations` scope markers.
- Added `src/VeloFile.App/Resources/Components/VeloFile.Status.xaml` with governed status surface, muted status, normal status, failure status, focus, spacing, and border resources.
- Added `src/VeloFile.App/Resources/Components/VeloFile.Operations.xaml` with governed operation surface, progress, completed, cancelled, failed, conflict, destructive confirmation, destructive action, cancel button, secondary button, danger, and focus resources.
- Merged the status and operations dictionaries from `src/VeloFile.App/App.xaml`.
- Scoped operation/status XAML in `src/VeloFile.App/MainWindow.xaml` with `shell-status-operations` markers and applied status/operation/destructive styles to rename, permanent-delete confirmation, file-operation conflict, status text, launch status, drop indicator, and cancel-operation controls without changing existing click handlers or operation services.
- Added `ApplyFileOperationStatusStyle` in `src/VeloFile.App/MainWindow.xaml.cs` so `FileOperationStatus.Running/Cancelling`, `Completed`, `Cancelled`, `Failed`, `WaitingForConflict`, and `WaitingForConfirmation` map to distinct VeloFile style resources.
- Activated the `shell-status-operations` scope in `docs/ui/ui-contract-scopes.v1.json` and updated the valid UI-contract fixtures to keep `scripts/ci.ps1` aligned with the active production scope.
- Updated `ShellVisualCoherenceContractTests` so `shell-status-operations` is expected to be active for M6.
- Behavior preservation remains covered by existing operation route tests in the M6 filtered solution run; M6 did not change Core operation services, Windows adapters, or command route methods.
- Validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~StatusOperationResourceContractTests"`: failed before implementation for the expected missing M6 resources/scope, then passed, 4 tests.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "Operation|FileOperation|Status|Accessibility"`: passed; App 37, Core 22, Windows 16, and Corpus 1 matching tests passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core 168, App 164, Windows 52, and Corpus 90 tests passed.
  - `git diff --check`: passed with CRLF normalization warnings only.
- M6 static/resource and behavior-preservation validation is complete. Under the approved visual-evidence-gate removal amendment, M6 does not need operation-running or destructive-confirmation visual evidence before code review.

M6 code review:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r17.md`: clean-with-notes; M6 closed with no material findings.

Visual-evidence gate amendment:

- Revised `specs/ui-shell-visual-coherence.md`, `specs/ui-shell-visual-coherence.test.md`, `docs/architecture/system/architecture.md`, and `docs/adr/0010-shell-visual-coherence-contracts.md` so screenshots/manual visual notes are optional supporting artifacts rather than closeout gates.
- Updated this plan so M4-M7 no longer require visual evidence before closeout and M8 is optional only if visual artifacts are recorded.

M7 implementation validation:

- Added `tests/VeloFile.App.Tests/UiDesign/PreviewResourceContractTests.cs` first. The focused M7 tests failed before implementation with missing `Resources/Components/VeloFile.Preview.xaml`, missing `shell-preview-details` scope markers, inactive `shell-preview-details` scope metadata, and missing `App.xaml` resource merge.
- Added `src/VeloFile.App/Resources/Components/VeloFile.Preview.xaml` with governed preview pane, layout, header, status, loading, ready, unsupported, failed, content, image, PDF navigation, metadata list, metadata row, label, and value resources.
- Merged the preview dictionary from `src/VeloFile.App/App.xaml`.
- Scoped the preview pane in `src/VeloFile.App/MainWindow.xaml` with `shell-preview-details` markers and applied preview styles to the pane, status text, content text, PDF navigation/buttons/page indicator, rendered image, and metadata list/rows without changing preview provider/controller behavior.
- Added `ApplyPreviewStatusStyle` in `src/VeloFile.App/MainWindow.xaml.cs` so `PreviewStatus.Loading`, `Success`, `Unsupported`, and `Failed` map to distinct VeloFile style resources.
- Activated the `shell-preview-details` scope in `docs/ui/ui-contract-scopes.v1.json` and updated the valid UI-contract fixtures to keep `scripts/ci.ps1` aligned with the active production scope.
- Updated `ShellVisualCoherenceContractTests` so `shell-preview-details` is expected to be active for M7.
- Reference comparison note: the production decision uses a Windows-native pane with tokenized status hierarchy, metadata rhythm, compact PDF navigation, and existing shell button/text styles rather than trying to reproduce `hifi-design/` pixel parity. No accepted preview/layout deviation is recorded for M7.
- Behavior preservation remains covered by existing preview route tests in the M7 filtered app and solution runs. M7 did not change Core preview providers, the preview controller, Windows adapters, file-list row templates, or preview command route methods.
- Validation passed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~PreviewResourceContractTests"`: failed before implementation for the expected missing M7 resources/scope, then passed, 4 tests.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: initially failed on inline `BorderThickness="0"` in `VeloFile.Preview.xaml`; fixed by adding `VfPreviewNoBorderThickness`; then passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Preview|PreviewSurface|Accessibility"`: passed, 19 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|Preview|FileListResourceContractTests"`: passed; App 30, Core 24, Windows 16, and Corpus 26 matching tests passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core 168, App 168, Windows 52, and Corpus 90 tests passed.
  - `git diff --check`: passed with CRLF normalization warnings only.
- M7 static/resource and behavior-preservation validation is complete. Under the approved visual-evidence-gate removal amendment, M7 does not need `shell-preview-open` visual evidence before code review.

M7 code review:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r18.md`: clean-with-notes; M7 closed with no material findings.
- No M7 review-resolution is required.
- Optional M8 is not triggered by M7 because no optional current screenshots, sidecars, diffs, or baseline artifacts were added. Prior M2/M3 visual-evidence records remain historical context under AC21 and do not block final closeout.

Explain-change:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/explain-change.md`: updated as the durable rationale linking the actual diff, requirements, architecture, plan milestones, tests, review resolutions, validation evidence, alternatives, scope boundaries, and remaining risks.
- No final verification was claimed by explain-change; final verification is recorded separately below.

Verify:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/verify-report.md`: branch-ready for PR handoff.
- Final local validation passed: `dotnet --info`, `dotnet restore VeloFile.sln`, `dotnet build VeloFile.sln -c Debug`, `dotnet test VeloFile.sln -c Debug`, `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`, `git diff --check`, generated visual output checks, and active artifact drift scans.
- Hosted GitHub CI has not been observed for the local verification-report commit; it should run after push/PR.

## Outcome and Retrospective

Not started.

## Readiness

Done. PR #3 merged on 2026-05-18 local date after hosted `ci` passed.

## Post-Merge Closeout

- PR: https://github.com/xiongxianfei/velofile/pull/3
- Merge commit: `e2170982f3bdfdb8255b69cc795ba40c6498544e`
- Hosted CI: `ci` completed successfully before merge.
- Plan index: moved to Done in `docs/plan.md`.
