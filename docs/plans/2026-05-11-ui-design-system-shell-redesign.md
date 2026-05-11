# UI Design-System and Shell Redesign Execution Plan

## Status

active

This plan passed `plan-review` and has an active test spec. Readiness is not Done: implementation starts at M1 only after the current handoff summary says M1 is ready.

## Purpose / Big Picture

This plan turns the approved UI design-system spec and architecture update into small implementation slices. The first slice establishes VeloFile-owned design contracts, static validation, WinUI resources, file-list row presentation, deterministic fixture mode, and reviewed visual evidence without changing V1 navigation, listing, selection, search, preview, drag/drop, file-operation, persistence, diagnostics, terminal, or Windows integration behavior.

The main implementation risk is mixing visual redesign with behavior rewrites. This plan sequences contract and validation foundations before XAML resource consumption, then adds fixture and visual evidence only after the file-list resources are stable.

## Source Artifacts

| Artifact | Path | Status |
|---|---|---|
| Proposal | [2026-05-11-ui-design-system-shell-redesign.md](../proposals/2026-05-11-ui-design-system-shell-redesign.md) | accepted |
| Feature spec | [specs/ui-design-system-shell-redesign.md](../../specs/ui-design-system-shell-redesign.md) | approved |
| Architecture package | [docs/architecture/system/architecture.md](../architecture/system/architecture.md) | approved |
| C4 container | [docs/architecture/system/diagrams/container.mmd](../architecture/system/diagrams/container.mmd) | current |
| C4 component | [docs/architecture/system/diagrams/desktop-app-components.mmd](../architecture/system/diagrams/desktop-app-components.mmd) | current |
| ADR 0009 | [0009-ui-design-contracts-static-validation-and-visual-fixtures.md](../adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md) | accepted |
| Test spec | [specs/ui-design-system-shell-redesign.test.md](../../specs/ui-design-system-shell-redesign.test.md) | active proof surface |

Spec review approved the spec on 2026-05-11 with no material findings. Architecture review approved the architecture update and ADR 0009 on 2026-05-11 with no material findings. Plan review approved this plan on 2026-05-11 with no material findings.

## Context and Orientation

The existing WinUI shell lives mostly in [MainWindow.xaml](../../src/VeloFile.App/MainWindow.xaml) and [MainWindow.xaml.cs](../../src/VeloFile.App/MainWindow.xaml.cs). The file list currently defines row layout inline inside `MainWindow.xaml`, including row height, padding, thumbnail placeholder dimensions, border values, and text styling. The implementation should factor first-slice row presentation into app resources instead of broad rewrites.

The app resource root is [App.xaml](../../src/VeloFile.App/App.xaml). The new resource dictionaries should live under `src/VeloFile.App/Resources/Tokens/` and `src/VeloFile.App/Resources/Components/`, then be merged from the app resource tree.

The app state and row data already flow through [AppShellViewModel.cs](../../src/VeloFile.App/ViewModels/AppShellViewModel.cs) and [FileListRowViewModel.cs](../../src/VeloFile.App/ViewModels/FileListRowViewModel.cs). The redesign must consume existing row/view-model state rather than adding a new selection system, row behavior model, or virtualization behavior.

The existing test layout is MSTest under `tests/`. App tests currently use linked shell seams and static contract checks rather than full UI automation. UI contract validation should therefore start as static .NET tooling under `tools/VeloFile.UiContracts`, with screenshot evidence added as reviewed artifacts rather than an immediate hard pixel-diff release gate.

## Non-Goals

- Pixel parity with `hifi-design/`.
- Importing or generating production resources from `hifi-design` tokens.
- Porting JSX or web component structure.
- Runtime theme switching, persisted theme/density settings, compact/spacious density exposure, or a broad theme engine.
- New file-list selection behavior, virtualization behavior, behavior model, or custom file-list row control.
- New file-operation, preview, search, drag/drop, terminal, persistence, or diagnostics behavior.
- Hard-gated screenshot pixel diff in the first validation slice.
- Appium/WinAppDriver or `DebugUiTest` in the first slice.
- Generated disk-backed visual fixtures for the first file-list baseline.

## Requirements Covered

| Requirement area | IDs | Primary milestone(s) |
|---|---|---|
| Authority, scope, fixed defaults, V1 behavior preservation | R1-R8, I1-I5, C1-C6 | M1-M4 |
| UI principles and file-list visual stability | R9-R14, A11Y1-A11Y7, P1-P4 | M2, M3 |
| Token contract and first-slice token values | R15-R25, AC1 | M1 |
| WinUI resources and static conformance | R26-R47, AC2, AC4-AC7 | M1, M2 |
| File-list row resources and states | R48-R61, AC14 | M2 |
| Fixture mode and visual evidence | R62-R79, AC8-AC13 | M3, M4 |
| Design deviations | R80-R84, AC3 | M1-M4 |
| Security, privacy, observability, compatibility | S1-S5, O1-O5, EC1-EC20 | M1-M4 |

## Milestones

### M1. UI Contract Artifacts and Static Validator

- Milestone state: closed
- Goal: Add the repo-owned token/scope/deviation artifacts and the lightweight static validation tool before any production XAML consumes the new resources.
- Requirements: R1-R8, R15-R25, R36-R47, R80-R84, O1, S5, AC1-AC3, AC5-AC6, ADR 0009.
- Files/components likely touched: `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`, `tools/VeloFile.UiContracts/`, `VeloFile.sln`, `tests/VeloFile.Corpus.Tests/` or a focused tool test project, `scripts/ci.ps1` only if validation is added to CI in this milestone.
- Dependencies: approved spec and architecture; plan-review and test spec.
- Tests to add/update: contract tests for token JSON shape, required keys, XAML duplicate-key detection, resource type checks, color-to-brush relationships, strict resource literal checks, targeted first-slice scope checks, and nonzero/actionable failure output using controlled valid/invalid fixture dictionaries.
- Implementation steps:
  - Create `docs/ui/tokens.v1.json` with all first-slice token IDs, XAML keys, types, values, categories, and `requiredInFirstSlice`.
  - Create `docs/ui/ui-contract-scopes.v1.json` with the first active file-list scope and required resource references.
  - Create `docs/ui/design-deviations.md` with status values and the required deviation template.
  - Add `tools/VeloFile.UiContracts` as a .NET console project included in `VeloFile.sln`.
  - Implement `validate-tokens` and first-slice scope validation over static JSON/XAML inputs without launching the app or depending on WinUI runtime.
  - Add focused tests plus controlled valid/invalid fixture dictionaries for missing keys, duplicate keys, wrong values, wrong types, and forbidden literals.
- Validation commands:
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug`
  - `dotnet run --project tools/VeloFile.UiContracts -- validate-tokens --contract docs/ui/tokens.v1.json --xaml-root tests/fixtures/ui-contracts/valid`
  - `dotnet test VeloFile.sln -c Debug --filter UiContracts`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: The repo has a machine-checked UI contract surface and a solution-included validator that fails fast for first-slice contract drift against controlled fixtures; production resource validation begins in M2.
- Commit message: `M1: add UI contract artifacts and static validator`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: The validator can become too broad and fail legacy XAML outside first-slice scope.
- Rollback/recovery: Revert the tool and docs artifacts; no production UI behavior or durable user data changes are introduced in M1.

### M2. WinUI Token Resources and File-List Row Redesign

- Milestone state: closed
- Goal: Add first-slice WinUI resource dictionaries and consume named file-list row resources from `MainWindow.xaml` without changing file-list behavior.
- Requirements: R9-R14, R26-R35, R48-R61, A11Y1-A11Y7, P1-P4, AC4, AC7, AC14.
- Files/components likely touched: `src/VeloFile.App/App.xaml`, `src/VeloFile.App/Resources/Tokens/*.xaml`, `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/ViewModels/FileListRowViewModel.cs`, `tests/VeloFile.App.Tests/`.
- Dependencies: M1 validator and contracts.
- Tests to add/update: static XAML tests for merged dictionaries, `VfFileListRowTemplate`, `VfFileListItemContainerStyle`, row text styles, row height/icon resources, and absence of first-slice direct row literals in governed scope; app-shell tests proving selection/focus/hidden/protected row state inputs remain available.
- Implementation steps:
  - Add token resource dictionaries for colors, typography, spacing, sizing, radius, focus, density, state, and motion.
  - Add `VeloFile.FileList.xaml` with named `ItemContainerStyle`, row template, name/metadata text styles, row height, row padding, icon/thumbnail fallback sizing, and state resources.
  - Merge the dictionaries from `App.xaml`.
  - Replace the inline file-list row template in `MainWindow.xaml` with `ItemTemplate="{StaticResource VfFileListRowTemplate}"` and `ItemContainerStyle="{StaticResource VfFileListItemContainerStyle}"`.
  - Preserve existing bindings to row/view-model state and avoid new custom controls, selection systems, behavior models, or virtualization changes.
  - Run the M1 validator against the new resources and update `docs/ui/design-deviations.md` only for meaningful intentional deviations.
- Validation commands:
  - `dotnet run --project tools/VeloFile.UiContracts -- validate-tokens --contract docs/ui/tokens.v1.json --xaml-root src/VeloFile.App/Resources`
  - `dotnet build VeloFile.sln -c Debug`
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: The file list renders through repo-owned resources with stable row height, distinct selected/focused/hidden/protected states, and no first-slice behavior rewrite.
- Commit message: `M2: apply WinUI file-list design resources`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: XAML resource changes can accidentally break binding paths, selection visuals, or virtualization.
- Rollback/recovery: Revert resource dictionaries and restore the prior inline template; Core and Windows adapters remain untouched.

### M3. Guarded Test Fixture Mode and Deterministic File-List States

- Milestone state: closed
- Goal: Add the non-production fixture launch path and deterministic first-slice file-list fixtures needed for visual evidence.
- Requirements: R62-R69, R67-R68, S1-S4, O2, AC11-AC13, ADR 0009.
- Files/components likely touched: `src/VeloFile.App/App.xaml.cs`, `src/VeloFile.App/AppCompositionRoot.cs`, `src/VeloFile.App/ViewModels/`, `tests/VeloFile.App.Tests/`, possibly a small app-shell fixture registry under `src/VeloFile.App/Testing/` guarded by build symbols.
- Dependencies: M2 file-list resources.
- Tests to add/update: process/startup parsing tests or app composition tests for Release rejection, Debug/test rejection without `VELOFILE_ENABLE_TEST_UI_FIXTURES=1`, unknown fixture rejection, allowlisted `file-list-v1` acceptance, and no arbitrary fixture data path support.
- Implementation steps:
  - Add a fixture option parser for `--test-ui-fixture`, `--theme`, `--density`, and `--viewport` values required by the spec.
  - Add a hardcoded fixture registry with `file-list-v1` and an empty-folder fixture state.
  - Gate fixture mode behind Debug/test availability plus `VELOFILE_ENABLE_TEST_UI_FIXTURES=1`.
  - Reject production/Release fixture launches, missing environment guard, and unknown names with nonzero exit before rendering normal or fixture UI.
  - Populate deterministic rows for normal, selected, focused, selected-and-focused, multi-selected, hidden/protected, thumbnail fallback, long-name, metadata-heavy, and empty-folder states.
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter "Fixture|UiContracts"`
  - `dotnet build src/VeloFile.App/VeloFile.App.csproj -c Release`
  - Manual or scripted app-launch checks for invalid fixture flag rejection if process-level app launch is available locally.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Fixture mode is auditable, deterministic, unavailable by accident, and ready for screenshot capture without touching real filesystem fixtures.
- Commit message: `M3: add guarded UI fixture mode`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Fixture routing can accidentally become a hidden production mode or silently fall back to normal UI.
- Rollback/recovery: Remove fixture parser/registry; normal production app launch remains unchanged.

### M4. Visual Baseline Evidence and Baseline Update Workflow

- Milestone state: planned
- Goal: Add the reviewed visual baseline storage, generated-output ignore rules, metadata sidecars, and review-gated baseline update workflow for the first file-list profile.
- Requirements: R69-R79, O3-O4, AC8-AC10, ADR 0009.
- Files/components likely touched: `tests/visual/baselines/winui/dark-comfortable-1440x900-100/`, `tests/visual/current/`, `tests/visual/diffs/`, `.gitignore`, `scripts/update-ui-baselines.ps1`, `tests/VeloFile.Corpus.Tests/` or script validation tests, docs under `docs/ui/` if needed.
- Dependencies: M3 fixture mode.
- Tests to add/update: script tests proving baseline update requires `-ReviewId`, refuses absent current screenshots, writes/updates sidecars, preserves generated current/diff as ignored outputs, and never runs as normal CI mutation.
- Implementation steps:
  - Add `.gitignore` entries for `tests/visual/current/` and `tests/visual/diffs/`.
  - Add `scripts/update-ui-baselines.ps1` with `-Suite winui`, `-Profile`, `-AllReviewed`, and required `-ReviewId`.
  - Add JSON sidecar schema checks for theme, density, viewport, scale, screen, fixture, dynamic regions, and review ID.
  - Capture or add reviewed first-slice baselines for the nine required screens under `dark-comfortable-1440x900-100/`.
  - Keep screenshot comparison soft: generated diffs are review evidence, not hard release gates in this slice.
- Validation commands:
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/update-ui-baselines.ps1 -Suite winui -Profile dark-comfortable-1440x900-100 -ReviewId <review-id>`
  - Negative tests for missing `-ReviewId` and missing current screenshots.
  - `dotnet test VeloFile.sln -c Debug --filter "Visual|UiContracts"`
  - `git status --short -- tests/visual/current tests/visual/diffs`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: The repo contains the required first-slice baseline screenshots and sidecars, while generated current/diff outputs stay uncommitted and baseline mutation remains review-gated.
- Commit message: `M4: add first UI visual baselines`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Screenshot capture can be noisy or unavailable on a local machine/CI runner.
- Rollback/recovery: Remove baseline files and script changes; production behavior remains governed by M1-M3 static validation and resource tests.

### M5. Lifecycle Closeout and Regression Verification

- Milestone state: lifecycle-closeout
- Goal: Confirm the first redesign slice remains coherent across spec, architecture, test spec, implementation evidence, review resolution, final verification, and PR handoff.
- Requirements: AC15 plus all first-slice acceptance criteria after M1-M4.
- Files/components likely touched: active plan progress, validation notes, change records, PR notes; no new product behavior.
- Dependencies: M1-M4 closed, code-review/review-resolution complete for each implementation milestone.
- Tests to add/update: none unless review findings require remediation.
- Implementation steps:
  - Ensure every in-scope implementation milestone is `closed` or removed by reviewed plan revision.
  - Run focused final validation plus the broad local CI command.
  - Use `explain-change`, `verify`, and `pr` stages as required by workflow.
  - Update this plan's progress, validation notes, outcome, and retrospective after final verification.
- Validation commands:
  - `dotnet --info`
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug`
  - `dotnet test VeloFile.sln -c Debug`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: The branch has a coherent first-slice UI design-system implementation ready for PR review, with validation evidence and no overclaimed release proof.
- Commit message: `M5: close UI redesign first-slice lifecycle evidence`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks: Final validation may expose behavior regressions outside the visual slice.
- Rollback/recovery: Identify the milestone that introduced the regression, fix within that milestone's review-resolution loop, and rerun targeted plus broad validation.

## Validation Plan

Validation is layered:

- Static contract validation starts in M1 and runs in every later milestone.
- XAML/resource build validation starts in M2.
- Fixture rejection and deterministic fixture state validation starts in M3.
- Visual baseline script and sidecar validation starts in M4.
- Broad V1 behavior regression validation runs after every implementation milestone where production app code changes.

Minimum recurring commands after M2:

```powershell
dotnet run --project tools/VeloFile.UiContracts -- validate-tokens --contract docs/ui/tokens.v1.json --xaml-root src/VeloFile.App/Resources
dotnet build VeloFile.sln -c Debug
dotnet test VeloFile.sln -c Debug
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1
```

Manual/review evidence:

- Review the nine `dark-comfortable-1440x900-100` baseline screens.
- Confirm generated `tests/visual/current/` and `tests/visual/diffs/` outputs remain uncommitted.
- Record intentional reference deviations in `docs/ui/design-deviations.md`.

## Risks and Recovery

- Risk: resource refactor breaks existing file-list behavior. Recovery: revert M2 XAML/resource changes while keeping M1 contracts, then reapply resources in smaller patches.
- Risk: validator overreaches into legacy XAML. Recovery: narrow `ui-contract-scopes.v1.json` and keep literal bans scoped to first-slice file-list references.
- Risk: fixture mode leaks into production. Recovery: fail closed by default; require Release/prod rejection tests before M3 closeout.
- Risk: screenshots become noisy or hard to reproduce. Recovery: keep screenshots soft review evidence and rely on static/resource tests until a stable visual comparison helper is approved.
- Risk: design deviations become unreviewed drift. Recovery: require `docs/ui/design-deviations.md` entries for meaningful deviations and review them with the implementation slice that introduced the deviation.

No data migration is expected. Rollback of the first slice removes first-slice resource, fixture, validation, and visual evidence changes without changing Core or Windows adapter behavior.

## Dependencies

- `plan-review` approved this plan on 2026-05-11.
- A matching test spec must be reviewed before implementation.
- M1 must precede all production XAML resource consumption.
- M2 must precede deterministic screenshot capture.
- M3 must precede committed visual baselines.
- M4 depends on local/CI ability to launch or otherwise capture the app fixture profile.
- Future theme/density persistence, generated token pipelines, hard-gated pixel diffs, `DebugUiTest`, or broad UI automation require separate accepted specs/architecture updates.

## Progress

- [x] M1. UI Contract Artifacts and Static Validator - closed
- [x] M2. WinUI Token Resources and File-List Row Redesign - closed
- [x] M3. Guarded Test Fixture Mode and Deterministic File-List States - closed
- [ ] M4. Visual Baseline Evidence and Baseline Update Workflow - planned
- [ ] M5. Lifecycle Closeout and Regression Verification - lifecycle-closeout

## Current Handoff Summary

Current milestone: M4. Visual Baseline Evidence and Baseline Update Workflow
Current milestone state: planned
Last reviewed milestone: M3
Review status: M3 rerun code-review clean-with-notes; no required-change findings remain for M3
Remaining in-scope implementation milestones: M4
Next stage: `implement` M4
Final closeout readiness: not ready
Reason final closeout is or is not ready: M4 remains planned.

## Decision Log

| Date | Decision | Reason |
|---|---|---|
| 2026-05-11 | Split implementation into contracts/tooling, resources, fixtures, visual evidence, and lifecycle closeout. | Keeps behavior-preserving visual work reviewable and prevents screenshot/fixture work from preceding stable resources. |
| 2026-05-11 | Keep M1 static validation independent from the running app. | Matches ADR 0009 and enables fast CI/local validation before UI automation is stable. |
| 2026-05-11 | Treat M5 as lifecycle closeout, not an implementation milestone. | Prevents final closeout from hiding unfinished implementation work. |
| 2026-05-11 | Use the matching test spec as the active proof surface before implementation. | Keeps implementation test-driven and traceable to spec requirements. |
| 2026-05-11 | Expose first-slice scope validation through `validate-tokens --scopes` rather than a separate command. | Keeps M1 validation entry points small while still supporting token and targeted scope checks. |
| 2026-05-11 | Add explicit scope-region markers for targeted literal scanning when present. | Prevents M1 validation from blocking unrelated legacy XAML literals outside the first file-list scope. |
| 2026-05-11 | Keep the first file-list row extraction to `DataTemplate`, `ItemContainerStyle`, text styles, and component padding resources. | Meets M2's named-resource contract without introducing a custom row control, selection model, or virtualization behavior. |

## Surprises and Discoveries

- The first UI contract test run failed as expected because `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`, and `tools/VeloFile.UiContracts` did not exist yet.
- Initial scope validation was too broad: it scanned all of `MainWindow.xaml` and required every resource reference in every scoped file. The validator now aggregates required references across scoped files and scans explicit `ui-contract-scope:<id>` marker regions when they exist.
- Existing app-shell contract tests expected file-row thumbnail and opacity bindings directly in `MainWindow.xaml`; M2 updated those tests to follow the extracted `VeloFile.FileList.xaml` component resource instead.
- WinUI accepted `FocusVisualPrimary*` and `FocusVisualSecondary*` setters on `ListViewItem`, but rejected `Style.Resources` in the M2 component style. The resolution scopes `ListViewItemBackground*` resource overrides inside `FileListSurface.Resources` instead.

## Validation Notes

Planning validation:

- `git diff --check -- specs/ui-design-system-shell-redesign.md docs/architecture/system/architecture.md docs/architecture/system/diagrams/container.mmd docs/architecture/system/diagrams/desktop-app-components.mmd docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md docs/plan.md docs/plans/2026-05-11-ui-design-system-shell-redesign.md`
- `git diff --check -- specs/ui-design-system-shell-redesign.test.md docs/plans/2026-05-11-ui-design-system-shell-redesign.md docs/plan.md`

Implementation validation will be recorded per milestone.

M1 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter UiContracts` initially failed for the expected missing M1 contract/tool artifacts.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid` passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 9 tests passed.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 351 tests passed.

M1 review-resolution validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter UiContracts` passed: 14 tests passed.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes docs\ui\ui-contract-scopes.v1.json --scope-root tests\fixtures\ui-contracts\valid` passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 tests passed.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 356 tests passed.

M1 rerun code-review validation:

- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 tests passed.

M2 validation:

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~FileListResourceContractTests.App_resources_merge"` failed before implementation for the expected missing first-slice resource dictionaries.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~FileListResourceContractTests"` passed: 6 tests passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources` passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"` passed: App tests 19 passed, Corpus UI contract tests 14 passed.
- First `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` attempt timed out at the 5-minute tool limit while the no-build test step was still running; the leftover test process was allowed to finish.
- Rerun `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded with 0 warnings and 0 errors, UI contract validation passed, and 362 tests passed.

## Outcome and Retrospective

M1 added the repo-owned UI token contract, first active UI contract scope, design-deviation policy document, static validation tool, controlled valid/invalid XAML fixtures, and focused contract/tool tests. No production UI behavior was changed.

Code review R1 requested changes for M1:

- CR-M1-001: extra first-slice resources are not validated. Resolved by default governed-resource key validation for `Resources/Tokens` and `Resources/Components`.
- CR-M1-002: strict tokenized-literal rules are not enforced for new token/component resource dictionaries. Resolved by default governed component literal checks and expanded invalid fixtures.

Rerun code review closed M1 with status `clean-with-notes`; no required-change findings remain for M1.

M2 added checked-in WinUI token dictionaries, the first file-list component resource dictionary, and scoped `MainWindow.xaml` references to `VfFileListRowTemplate` and `VfFileListItemContainerStyle`. Existing file-list command, selection, drag/drop, context-menu, row opacity, thumbnail, and metadata bindings remain routed through the same app state; M2 did not add fixture mode, screenshot baselines, a custom row control, or new behavior/persistence.

M2 code review R2 requested changes:

- CR-M2-001: file-list selected/focused states are not governed by first-slice resources. Resolved by named row state resources, focus visual setters, scoped file-list selected/hover background resources, and focused static tests.

M2 review-resolution validation:

- `dotnet test VeloFile.sln -c Debug --filter FileListResourceContractTests` passed: 8 app tests passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 corpus UI contract tests passed.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 364 tests passed.

M2 CR-M2-002 review-resolution validation:

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter FileListResourceContractTests` failed before implementation for the expected missing opacity selector.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter FileListResourceContractTests` passed: 11 app tests passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed after adding `VfFileListRowOpacityConverter` to the file-list scope.
- `dotnet test VeloFile.sln -c Debug --filter FileListResourceContractTests` passed: 11 app tests passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 corpus UI contract tests passed.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests` passed: 18 app tests passed.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 367 tests passed.

M2 final rerun code-review validation:

- `dotnet test VeloFile.sln -c Debug --filter FileListResourceContractTests` passed: 11 app tests passed.
- `git diff --check` passed.

M3 validation:

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Fixture|UiContracts"` failed before implementation for the expected missing fixture launch and registry files.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Fixture|UiContracts"` passed after implementation: 12 app tests passed.
- `dotnet test VeloFile.sln -c Debug --filter "Fixture|UiContracts"` passed: App fixture tests 12 passed, Corpus UI contract tests 14 passed, and Core/Windows projects had no matching tests.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Release` passed with 0 warnings and 0 errors.
- `rg "fixture-data|C:\\Users|xiongxianfei|20260428-velofile|--test-ui-fixture|VELOFILE_ENABLE_TEST_UI_FIXTURES" src\VeloFile.App tests\VeloFile.App.Tests docs -n` found only expected source, test, and documentation references; deterministic fixture rows do not use local user/workspace paths.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded with 0 warnings and 0 errors, UI contract validation passed, and 379 tests passed.
- Process-level GUI launch checks were not run in this slice; fixture rejection/acceptance was covered by static startup wiring tests and launch gate unit tests.

M3 added a guarded non-production UI fixture launch path and deterministic first-slice file-list fixtures. Fixture mode is accepted only in Debug/test builds with `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` and a hardcoded allowlisted fixture name. Release/production, missing guard, unsupported options, arbitrary fixture-data paths, positional data paths, and unknown fixtures reject before normal or fixture UI creation. The fixture registry provides `file-list-v1` and `file-list-empty-folder` using synthetic rows under `C:\VeloFileFixture`, preserving the normal app shell/listing/view-model route without reading real disk fixtures.

M3 code-review requested changes:

- CR-M3-001: deterministic selected/focused/multi-selected fixture states are labels only and are not consumed by the rendered app/startup path.

M3 review-resolution validation:

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Fixture|UiContracts"` failed before the resolution for missing presentation-state code paths, then passed after resolution with 15 app tests.
- `dotnet test VeloFile.sln -c Debug --filter "Fixture|UiContracts"` passed: App fixture tests 15 passed, Corpus UI contract tests 14 passed, and Core/Windows projects had no matching tests.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Release` passed with 0 warnings and 0 errors.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded with 0 warnings and 0 errors, UI contract validation passed, and 382 tests passed.

M3 review-resolution resolved CR-M3-001 by adding explicit fixture presentation state with stable row IDs, preserving that state through fixture shell creation, and applying accepted fixture selected/focused rows through `FileListSurface` in `MainWindow`.

M3 rerun code-review result:

- Status: clean-with-notes.
- No required-change findings remain for M3.
- M3 is closed and the next implementation milestone is M4.

## Readiness

See Current Handoff Summary. This plan is active and ready for M4 implementation. Final closeout is not ready.
