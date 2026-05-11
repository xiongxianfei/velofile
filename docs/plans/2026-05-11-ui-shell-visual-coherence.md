# UI Shell Visual Coherence Execution Plan

## Status

draft

Readiness is not Done. Plan review is approved and the matching test spec is active; implementation may start at M1.

## Purpose / Big Picture

This plan turns the approved UI shell visual-coherence spec into reviewable implementation slices. The work extends the accepted first UI design-system slice from file-list resources into the whole WinUI shell: app root, chrome, sidebar, command band, file list, status/operation surfaces, preview/details, deterministic fixture icons, and full-shell visual evidence.

The main risk is mixing visual redesign with behavior changes. This plan keeps the contract/tooling work first, applies the shell surface foundation before region polish, preserves App/Core/Windows behavior routes, and treats screenshots as soft review evidence rather than proof of real platform behavior.

## Source Artifacts

| Artifact | Path | Status |
|---|---|---|
| Proposal | [2026-05-11-shell-visual-coherence-follow-up.md](../proposals/2026-05-11-shell-visual-coherence-follow-up.md) | accepted |
| Feature spec | [specs/ui-shell-visual-coherence.md](../../specs/ui-shell-visual-coherence.md) | approved |
| First-slice UI spec | [specs/ui-design-system-shell-redesign.md](../../specs/ui-design-system-shell-redesign.md) | approved; remains authoritative for first-slice scope |
| Architecture package | [docs/architecture/system/architecture.md](../architecture/system/architecture.md) | approved; amended for shell visual coherence |
| ADR 0010 | [0010-shell-visual-coherence-contracts.md](../adr/0010-shell-visual-coherence-contracts.md) | accepted |
| Existing UI plan | [2026-05-11-ui-design-system-shell-redesign.md](2026-05-11-ui-design-system-shell-redesign.md) | active; branch-ready for PR handoff |
| Test spec | [specs/ui-shell-visual-coherence.test.md](../../specs/ui-shell-visual-coherence.test.md) | active |

Proposal review, spec review, and architecture review completed on 2026-05-11 with no material findings. This plan is the next lifecycle artifact before plan review and test-spec authoring.

## Context and Orientation

The current UI design-system slice already added `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`, `tools/VeloFile.UiContracts`, token resource dictionaries under `src/VeloFile.App/Resources/Tokens/`, file-list component resources under `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, guarded fixture launch support under `src/VeloFile.App/Testing/`, and first file-list visual baselines under `tests/visual/baselines/winui/dark-comfortable-1440x900-100/`.

The visible shell still routes through [MainWindow.xaml](../../src/VeloFile.App/MainWindow.xaml), [MainWindow.xaml.cs](../../src/VeloFile.App/MainWindow.xaml.cs), [AppShellViewModel.cs](../../src/VeloFile.App/ViewModels/AppShellViewModel.cs), and existing Core/App/Windows services. The follow-on must consume existing shell state and command routes rather than introducing new navigation, listing, search, preview, file-operation, drag/drop, terminal, diagnostics, persistence, or Windows adapter behavior.

The UI contract tool currently validates first-slice token and scope rules without launching the app. This plan expands that same validation path additively instead of creating a second token authority. Screenshot evidence remains review evidence; generated `tests/visual/current/` and `tests/visual/diffs/` stay uncommitted.

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
| Full-shell visual evidence and sidecars | R66-R77, AC9-AC13 | M8 |
| Behavior-preservation matrix | R78-R82, AC17-AC18, AC20 | M1-M8 through the matching test spec |
| Security, privacy, accessibility, performance | S1-S6, A11Y1-A11Y9, P1-P7, O1-O5 | M1-M8 |

## Milestones

## Region-Slice Visual Evidence Rule

Each shell visual-coherence region milestone must produce full-shell visual evidence before closeout. The evidence may be an automated current screenshot, a recorded manual screenshot review, or a documented visual-review note when automation is not yet stable.

The evidence must show the whole shell, not only the touched component, so reviewers can identify new mismatches between redesigned and non-redesigned regions. If a mismatch is accepted temporarily, it must be recorded in `docs/ui/design-deviations.md`.

M8 remains the consolidation and baseline-inventory milestone. M8 is not the first milestone where region screenshots are produced.

### M1. Shell Contract and Validator Extension

- Milestone state: closed
- Goal: Extend the existing V1 UI contract chain for shell-wide scopes, fixture icon invariants, screenshot sidecar metadata, and behavior-preservation matrix hooks before changing shell visuals.
- Requirements: R1-R15, R66-R82, O1-O5, S1-S6, AC2-AC5, AC7, AC12, AC17-AC20, ADR 0010.
- Files/components likely touched: `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`, `tools/VeloFile.UiContracts/`, `tests/fixtures/ui-contracts/`, `tests/VeloFile.Corpus.Tests/UiContracts/`, `tests/VeloFile.App.Tests/`.
- Dependencies: approved plan review; matching test spec must define traceable tests before implementation.
- Tests to add/update: failing tests for additive shell tokens/scopes, forbidden raw/default governed visuals, forbidden icon controls/text chips, required fixture icon resource keys, sidecar required fields/privacy, and behavior-matrix inventory shape.
- Implementation steps:
  - Add additive shell-wide token entries or document why existing tokens cover the governed shell region.
  - Add additive governed scopes for shell foundation, command band, sidebar, status/operation, preview/details, and fixture icon resources as those scopes become active.
  - Extend `tools/VeloFile.UiContracts` so it can validate shell scopes, icon-resource invariants, and full-shell screenshot sidecars without launching the app.
  - Add controlled valid/invalid fixtures for `<SymbolIcon`, `<PathIcon`, private-use glyph usage, ellipsized icon chips, unapproved icon sizes, missing sidecar fields, and sidecar privacy violations.
  - Add or update `docs/ui/design-deviations.md` templates for temporary shell-region mismatch records.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "UiContracts|Visual"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet build VeloFile.sln -c Debug`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Shell-wide static validation fails fast for governed contract, icon, and sidecar violations while legacy out-of-scope XAML remains outside enforcement.
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

- Milestone state: review-requested
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
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurface|UiDesign|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`
  - Produce current full-shell `shell-default` evidence for `shell-standard-1440x900-100`, or record a manual screenshot review note if automation is not stable.
  - Produce `shell-default` evidence for `shell-min-900x560-100` when the slice changes layout, surface sizes, titlebar/sidebar width, command-band height, content bounds, or minimum-window behavior.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: The default shell no longer mixes raw light/default chrome or sidebar surfaces with the dark file list, and existing shell routes remain present.
- Commit message: `M2: apply shell surface foundation`
- Milestone closeout:
  - validation passed
  - current full-shell `shell-default` screenshot or manual screenshot review recorded at `shell-standard-1440x900-100`
  - `shell-min-900x560-100` evidence recorded if the slice changes layout, surface sizes, or minimum-window behavior
  - no new light/dark split or surface mismatch introduced without a deviation record
  - full-shell visual evidence recorded for the touched region/state/profile, or an explicit manual visual-review note recorded with reason
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Surface changes can expose contrast, focus, or clipped-control issues at small sizes.
- Rollback/recovery: Revert the shell resource dictionary and `MainWindow.xaml` resource substitutions; Core and Windows adapters remain untouched.

### M3. File-List Polish and Deterministic Fixture Icons

- Milestone state: planned
- Goal: Complete the file-list follow-on polish and replace placeholder-like fixture icon chips with governed deterministic XAML vector resources.
- Requirements: R28-R43, R66, A11Y1-A11Y3, A11Y8, P1-P4, AC6-AC8, AC16.
- Files/components likely touched: `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`, `src/VeloFile.App/Testing/UiFixtureRegistry.cs`, `src/VeloFile.App/ViewModels/FileListRowViewModel.cs`, `src/VeloFile.App/MainWindow.xaml`, `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `tests/VeloFile.App.Tests/UiFixtures/`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M1 validator support and M2 shell foundation.
- Tests to add/update: icon invariant tests rejecting `SymbolIcon`, `PathIcon`, private-use glyphs, arbitrary icon resource keys, text chips, missing required `VfIconGeometry*` resources, missing icon container/path styles, and row-height changes across fixture icon kinds.
- Implementation steps:
  - Add `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` with required geometry resources and `VfFileListIconContainerStyle`, `VfFileListIconPathStyle`, and `VfFileListFixtureIconTemplate`.
  - Extend fixture row data to expose allowlisted icon kinds: `FileGeneric`, `Folder`, `Pdf`, `Image`, `Text`, `Spreadsheet`, `Executable`, `Markdown`, and `ThumbnailFallback`.
  - Hardcode fixture icon-kind mapping and reject arbitrary icon resource keys.
  - Refine file-list normal, hover, selected, focused, selected-focused, multi-selected, hidden, protected/system, long filename, folder, thumbnail fallback, and empty-folder visual states.
  - Preserve file-list virtualization, row height, existing selection mapping, context menu routes, drag/drop target behavior, and thumbnail/preview state flow.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FileListResourceContractTests|UiFixture"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|FileList|AppShellContract|DragDrop|Preview"`
  - Produce current full-shell `shell-file-list-selected-focused` evidence for `shell-standard-1440x900-100`, or record a manual screenshot review note if automation is not stable.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Fixture file-list rows use deterministic vector icons and polished row states without changing V1 file-list behavior.
- Commit message: `M3: add deterministic file-list fixture icons`
- Milestone closeout:
  - validation passed
  - full-shell `shell-file-list-selected-focused` evidence recorded
  - deterministic fixture icons visible in shell evidence
  - no placeholder text chips such as `P...`, `D...`, or `T...` remain in governed visual fixtures
  - focus and selection are visibly distinct and not error-like
  - full-shell visual evidence recorded for the touched region/state/profile, or an explicit manual visual-review note recorded with reason
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Fixture icon mapping could become an arbitrary resource lookup or row polish could destabilize virtualization.
- Rollback/recovery: Revert icon dictionary, fixture mapping, and file-list resource updates; restore previous first-slice file-list resources.

### M4. Command Band Visual Coherence

- Milestone state: planned
- Goal: Redesign navigation/path/filter/search controls as one intentional command band while preserving existing commands and accessibility routes.
- Requirements: R16-R22, R44-R49, R66-R77, R78-R82, A11Y1-A11Y6, AC9-AC13, AC15, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/MainWindow.xaml.cs`, `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml`, `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `tests/VeloFile.App.Tests/AppShellCommandRouteTests.cs`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M2 shell foundation; M1 validator support.
- Tests to add/update: static command-band resource tests, accessible-name/tooltip checks for icon or ambiguous controls, route-preservation tests for back, forward, up, refresh, path/breadcrumb entry, filter, search, cancel, and clear.
- Implementation steps:
  - Add command-band resources for button/input height, radius, padding, typography, hover, focus, pressed, disabled, and input border/background states.
  - Apply resources to existing navigation/path/filter/search controls.
  - Reorganize the command band only as far as required for visual coherence and minimum-size reachability; keep command handlers and view-model routes stable.
  - Record touched behavior-preservation matrix rows in the plan once the matching test spec exists.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "AppShellCommandRouteTests|CommandBand|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`
  - Produce current full-shell `shell-filter-active` and `shell-search-active` evidence for `shell-standard-1440x900-100`, or record manual screenshot review notes if automation is not stable.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: The top shell controls read as one navigation/search band and all existing command routes remain available.
- Commit message: `M4: style the shell command band`
- Milestone closeout:
  - validation passed
  - full-shell `shell-filter-active` evidence recorded
  - full-shell `shell-search-active` evidence recorded
  - path, filter, search, cancel, and clear controls remain reachable and visually integrated
  - disabled states are readable and not confused with unavailable app state
  - full-shell visual evidence recorded for the touched region/state/profile, or an explicit manual visual-review note recorded with reason
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Layout changes could clip controls at `900 x 560` or accidentally change path/search behavior.
- Rollback/recovery: Revert command-band resources and XAML layout edits; preserve M1-M3 foundation and icon work.

### M5. Navigation-First Sidebar

- Milestone state: planned
- Goal: Reorder and style the sidebar as navigation-first while preserving access, keyboard order, accessible names, and discoverability for existing sidebar functions.
- Requirements: R16-R22, R50-R56, R66-R77, R78-R82, A11Y1-A11Y6, AC9-AC14, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/MainWindow.xaml.cs`, `src/VeloFile.App/ViewModels/AppShellViewModel.cs` only if existing view state needs presentation grouping, `src/VeloFile.App/Resources/Components/VeloFile.Sidebar.xaml`, `tests/VeloFile.App.Tests/AppShellContractTests.cs`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M2 shell foundation; M1 validator support.
- Tests to add/update: sidebar visual grouping/static resource tests, keyboard traversal order checks where feasible through XAML/static route tests, accessible names for locations/favorites/recents/drives/visibility toggles/terminal controls, route tests proving existing controls remain reachable.
- Implementation steps:
  - Add sidebar resources for group headings, navigation items, selected/hover/focus/disabled states, compact secondary sections, and terminal/visibility controls.
  - Present locations, favorites, recent entries, and drives before visibility toggles and terminal controls.
  - Preserve existing sidebar commands and settings routes; do not add new sidebar product features.
  - Validate or record keyboard order and accessible names for the new grouping.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Sidebar|AppShellContract|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Terminal|Accessibility"`
  - Produce current full-shell `shell-sidebar-focused` or `shell-default` evidence for `shell-standard-1440x900-100`, or record a manual screenshot review note if automation is not stable.
  - Record keyboard order and accessible-name evidence for locations, favorites, recents, drives, visibility toggles, and terminal controls.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Sidebar hierarchy is navigation-first, visually integrated with the shell, and all previous sidebar controls remain discoverable and keyboard reachable.
- Commit message: `M5: make the sidebar navigation first`
- Milestone closeout:
  - validation passed
  - full-shell sidebar-focused or default-shell evidence recorded
  - keyboard order and accessible names for locations, favorites, recents, drives, visibility toggles, and terminal controls recorded or tested
  - navigation remains primary and settings-like controls remain discoverable but secondary
  - no existing sidebar route is removed without spec approval
  - full-shell visual evidence recorded for the touched region/state/profile, or an explicit manual visual-review note recorded with reason
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Reordering can be a behavioral regression if keyboard order or discoverability is not preserved.
- Rollback/recovery: Revert sidebar resources/layout to the previous grouped order; existing state and persistence are unchanged.

### M6. Status, Operation, and Destructive Surfaces

- Milestone state: planned
- Goal: Bring operation progress, conflict/failure, cancellation, and destructive confirmation visuals into the shell surface system without changing operation behavior.
- Requirements: R16-R22, R57-R61, R66-R77, R78-R82, A11Y1-A11Y3, A11Y6, A11Y9, AC9-AC13, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.Status.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.Operations.xaml`, `tests/VeloFile.App.Tests/AppShellCommandRouteTests.cs`, operation route tests in `tests/VeloFile.Core.Tests/` or `tests/VeloFile.App.Tests/` as needed.
- Dependencies: M2 shell foundation; M4/M5 if screenshots need coherent command/sidebar context.
- Tests to add/update: static resource tests for status/operation/danger treatment, operation-route preservation tests, destructive confirmation route tests, no danger styling for ordinary focus.
- Implementation steps:
  - Add tokenized status/operation resources for running, cancelled, failed, conflict, progress, cancellation, and destructive confirmation surfaces.
  - Apply resources to governed operation/status surfaces.
  - Preserve existing file-operation service and command routes.
  - Ensure active operation and destructive confirmation states do not obscure primary navigation in required profiles.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter "Operation|FileOperation|Status|Accessibility"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - Produce current full-shell `shell-operation-running` and `shell-destructive-confirmation` evidence for `shell-standard-1440x900-100`, or record manual screenshot review notes if automation is not stable.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Operation and destructive states are visually distinct, tokenized, and behavior-preserving.
- Commit message: `M6: style status and operation surfaces`
- Milestone closeout:
  - validation passed
  - full-shell `shell-operation-running` evidence recorded
  - full-shell `shell-destructive-confirmation` evidence recorded
  - cancel, progress, failure, and destructive states remain visible without unexpectedly covering primary navigation
  - permanent-delete confirmation hierarchy is readable and visually distinct from ordinary actions
  - full-shell visual evidence recorded for the touched region/state/profile, or an explicit manual visual-review note recorded with reason
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Destructive styling could be confused with focus/accent state or operation surfaces could cover navigation.
- Rollback/recovery: Revert status/operation resource and XAML changes; operation services remain unchanged.

### M7. Preview and Details Surface Treatment

- Milestone state: planned
- Goal: Integrate preview/details loading, success, unsupported, failed, metadata, image, text, and PDF states into the shell surface system without destabilizing file-list layout.
- Requirements: R16-R22, R62-R65, R66-R77, R78-R82, A11Y1-A11Y6, P1-P2, AC9-AC13, AC18, AC20.
- Files/components likely touched: `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.Preview.xaml`, `tests/VeloFile.App.Tests/Preview/`, `tests/VeloFile.App.Tests/UiDesign/`.
- Dependencies: M2 shell foundation; existing preview provider architecture remains unchanged.
- Tests to add/update: static preview resource tests, preview route tests for loading/success/unsupported/failed/PDF navigation state, checks that preview visual changes do not change file-list row height or hide selected/focused rows.
- Implementation steps:
  - Add preview/details resources for container, header, metadata, loading, unsupported, failed, image/text/PDF states, and separators.
  - Apply resources to preview/details panes while preserving existing preview controller and provider routes.
  - Verify row selection/focus remains visible with preview open.
  - Keep preview fixture evidence synthetic and non-sensitive.
- Validation commands:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Preview|PreviewSurface|Accessibility"`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|Preview|FileListResourceContractTests"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - Produce current full-shell `shell-preview-open` evidence for `shell-standard-1440x900-100`, or record a manual screenshot review note if automation is not stable.
  - Produce `shell-preview-open` evidence for `shell-min-900x560-100` when preview layout affects minimum layout or content bounds.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Preview/details surfaces visually belong to the shell and existing preview behavior remains intact.
- Commit message: `M7: style preview and details surfaces`
- Milestone closeout:
  - validation passed
  - full-shell `shell-preview-open` evidence recorded
  - preview open state does not obscure file-list usability or primary navigation
  - loading, failed, unsupported, and metadata states remain visually distinct where touched
  - any accepted preview/layout mismatch is recorded as a deviation
  - full-shell visual evidence recorded for the touched region/state/profile, or an explicit manual visual-review note recorded with reason
  - any accepted mismatch or reference deviation recorded in `docs/ui/design-deviations.md`
  - existing behavior-preservation tests for touched routes still pass
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Preview layout can hide file-list selection/focus or introduce clipped text at required profiles.
- Rollback/recovery: Revert preview resource/layout edits; preview providers and Core behavior are unchanged.

### M8. Full-Shell Evidence Consolidation and Baseline Inventory

- Milestone state: planned
- Goal: Consolidate required full-shell screenshot states, sidecars, evidence inventory validation, and profile classification after M2-M7 have already produced region-slice visual evidence.
- Requirements: R66-R82, O3-O4, S3-S6, A11Y6-A11Y7, AC9-AC13, AC17-AC20.
- Files/components likely touched: `src/VeloFile.App/Testing/UiFixtureRegistry.cs`, screenshot capture helpers if present, `scripts/update-ui-baselines.ps1`, `tests/visual/baselines/winui/shell-min-900x560-100/`, `tests/visual/baselines/winui/shell-standard-1440x900-100/`, `tests/visual/baselines/winui/shell-standard-1440x900-200/` or manual evidence records, `tests/VeloFile.Corpus.Tests/UiContracts/`.
- Dependencies: M2-M7 visual regions; M1 sidecar validation.
- Tests to add/update: visual inventory tests for seven required states, sidecar metadata/privacy tests, generated-output ignore tests, `200%` automated-or-manual classification tests, and behavior-preservation matrix citation checks from the matching test spec.
- Implementation steps:
  - Add or extend deterministic full-shell fixture states for any required state not already covered by M2-M7: `shell-default`, `shell-file-list-selected-focused`, `shell-filter-active`, `shell-search-active`, `shell-preview-open`, `shell-operation-running`, and `shell-destructive-confirmation`.
  - Validate that all required current screenshots from M2-M7 exist or have recorded manual evidence.
  - Capture/review any missing `shell-min-900x560-100` and `shell-standard-1440x900-100` evidence.
  - Capture `shell-standard-1440x900-200` if automation is stable; otherwise record it as manual/release review evidence with sidecar expectations.
  - Validate sidecars for profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, review ID, and privacy exclusions.
  - Validate that deviation records exist for accepted visual mismatches.
  - Confirm generated current/diff outputs remain uncommitted.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "Visual|UiContracts"`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\update-ui-baselines.ps1 -Suite winui -Profile shell-standard-1440x900-100 -ReviewId <review-id>`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\update-ui-baselines.ps1 -Suite winui -Profile shell-min-900x560-100 -ReviewId <review-id>`
  - `git status --short -- tests\visual\current tests\visual\diffs`
  - `dotnet test VeloFile.sln -c Debug --filter "Visual|UiContracts|Accessibility"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Expected observable result: Required full-shell visual evidence from M2-M7 is inventoried, sidecars are safe and complete, baseline review is traceable, and behavior-preservation citations remain separate.
- Commit message: `M8: consolidate full-shell visual evidence`
- Milestone closeout:
  - validation passed
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
- Dependencies: M1-M8 closed, code-review/review-resolution complete for each implementation milestone.
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
- Full-shell screenshot and sidecar inventory validation runs in M8.
- Broad V1 regression validation runs after production shell changes.

Minimum recurring commands after M2:

```powershell
dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .
dotnet build VeloFile.sln -c Debug
dotnet test VeloFile.sln -c Debug
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
```

Manual/review evidence:

- Review full-shell screenshots for required states and profiles.
- Record `shell-standard-1440x900-200` as manual/release evidence if automation cannot reliably set 200% scale.
- Confirm generated `tests/visual/current/` and `tests/visual/diffs/` outputs remain uncommitted.
- Record meaningful reference or temporary-region deviations in `docs/ui/design-deviations.md`.

## Risks and Recovery

- Risk: visual work changes V1 behavior. Recovery: require touched behavior-preservation rows and existing App/Core/Windows tests before accepting each slice.
- Risk: shell scope validation blocks legacy XAML. Recovery: keep `docs/ui/ui-contract-scopes.v1.json` additive and region-scoped; activate only implemented regions.
- Risk: icons repeat the prior glyph defect class. Recovery: enforce raw vector fixture icons, reject `SymbolIcon`/`PathIcon`/private-use glyphs in governed icon scopes, and require screenshot proof.
- Risk: screenshots are noisy or subjective. Recovery: keep them soft evidence and rely on static contracts, sidecars, and behavior tests as hard proof.
- Risk: high-DPI capture is unstable. Recovery: classify `shell-standard-1440x900-200` as manual/release evidence until automation is stable; do not silently drop the profile.
- Risk: sidebar reordering harms keyboard flow or discoverability. Recovery: treat sidebar grouping as observable behavior, preserve accessible names and keyboard order, and revert the sidebar slice if access regresses.
- Risk: region slices leave the shell mismatched. Recovery: record temporary deviations and prioritize the next dependent region, or revert the mismatching slice if it cannot be justified.

No data migration is expected. Rollback is per governed region by reverting that region's resources, scoped XAML usage, tests, and visual evidence without changing Core or Windows adapters.

## Dependencies

- `plan-review` must approve this plan before implementation.
- `specs/ui-shell-visual-coherence.test.md` must exist and be reviewed before implementation starts.
- The first UI design-system slice should remain authoritative for first-slice token/file-list row contracts.
- The existing UI design-system branch-ready plan may need to land or be rebased before this follow-on implementation to avoid resource and visual-baseline conflicts.
- Windows/.NET local validation is required. Full-shell screenshots require a local environment capable of launching the WinUI app.
- `shell-standard-1440x900-200` automation may depend on reliable high-DPI environment control; manual/release evidence is acceptable until that stabilizes.

## Progress

- [x] Proposal accepted.
- [x] Spec approved.
- [x] Architecture update and ADR 0010 approved by architecture review.
- [x] Draft execution plan created.
- [x] Plan review completed.
- [x] Matching test spec created.
- [x] Matching test spec accepted.
- [x] M1 closed.
- [ ] M2 closed.
- [ ] M3 closed.
- [ ] M4 closed.
- [ ] M5 closed.
- [ ] M6 closed.
- [ ] M7 closed.
- [ ] M8 closed.
- [ ] Lifecycle closeout completed.

## Current Handoff Summary

Current milestone: M2. Shell Surface Foundation
Current milestone state: review-requested
Last reviewed milestone: M1
Review status: M1 code-review clean-with-notes after CR-001 resolution
Remaining in-scope implementation milestones: M2-M8
Next stage: `code-review` M2
Final closeout readiness: not ready
Reason final closeout is or is not ready: M1 is closed and M2 is ready for code review, but M2-M8 are not all closed.

## Decision Log

| Date | Decision | Reason |
|---|---|---|
| 2026-05-11 | Keep this follow-on as a new draft plan instead of replacing the first-slice UI plan. | The first-slice plan is already active and branch-ready; this work extends it with a separate approved spec. |
| 2026-05-11 | Put shell contract/tooling extension before visual resource edits. | The spec and ADR require additive token/scope validation, icon invariants, sidecar checks, and behavior matrix proof before screenshots are trusted. |
| 2026-05-11 | Keep shell surface foundation before file-list polish and icon treatment. | The spec requires shell foundation as the first follow-on implementation region and file-list/icon work as the second. |
| 2026-05-11 | Split command band, sidebar, status/operation, and preview/details into separate implementation milestones. | Each region touches different behavior-preservation rows and should get its own review loop. |
| 2026-05-11 | Treat M9 as lifecycle closeout, not implementation. | Final closeout must not hide unfinished region work. |
| 2026-05-11 | Reuse existing first-slice dark comfortable tokens for M2 shell foundation instead of adding new token IDs. | Existing V1 tokens already cover the M2 surface, text, focus, selection, hover, disabled, danger, warning, success, spacing, radius, sizing, and focus-thickness needs. |

## Surprises and Discoveries

- M2 did not require new token values; it added shell component resources that alias and consume existing token resources.
- `scripts/ci.ps1` exposed that the valid UI-contract fixture also needed the newly active shell surface scope and fixture shell dictionary. The fixture was updated so active M2 validation remains representative.
- An attempted build run overlapped a filtered test run and failed because `testhost` temporarily locked `VeloFile.Corpus.Tests.dll`; rerunning `dotnet build VeloFile.sln -c Debug` after the testhost exited passed with 0 warnings and 0 errors.

## Validation Notes

Planning validation:

- `git diff --check -- docs/plans/2026-05-11-ui-shell-visual-coherence.md docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`

PR-001 review-resolution:

- Added the region-slice visual evidence rule.
- Updated M2-M7 with full-shell visual evidence validation and closeout requirements.
- Reframed M8 as consolidation and baseline inventory.

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

## Outcome and Retrospective

Not started.

## Readiness

See Current Handoff Summary. M1 is ready for code review; this plan is not ready for final closeout, verification, PR handoff, or Done.
