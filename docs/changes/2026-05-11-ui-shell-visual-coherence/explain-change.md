# Explain Change: UI Shell Visual Coherence

## Summary

This change extends the first UI design-system slice into the whole WinUI shell. It adds governed shell surface, file-list icon, command-band, sidebar, status/operation, and preview/details resources; wires those resources into `MainWindow.xaml`; extends static UI contract validation; and records the proposal/spec/architecture/plan/review trail that keeps the visual work scoped.

The goal was not to port `hifi-design/` or introduce new product behavior. The goal was to make the existing V1 shell read as one coherent Windows-native product surface while preserving navigation, listing, search, preview, file operations, drag/drop, terminal, diagnostics, persistence, and accessibility routes.

Current workflow state: M1-M7 are closed, optional M8 is not triggered, and the next lifecycle stage is `verify`. Final verification and PR readiness have not yet been claimed.

## Problem

The shell had the right behavioral surface, but the UI still looked like separate raw controls placed around a file-list redesign. The proposal identified these recurring problems:

- app root, chrome, sidebar, command band, file list, status, and preview did not share one surface model;
- raw/default WinUI controls were still visible inside governed shell regions;
- fixture file-list icons looked like placeholder text chips;
- ordinary focus, selected state, warning, danger, disabled, and status states were not consistently separated;
- sidebar hierarchy, command-band anatomy, operation/status safety surfaces, and preview/details states needed region-specific polish;
- static checks and behavior tests had to remain the hard proof because screenshot automation was not yet stable enough to be a required gate.

## Decision Trail

| Decision layer | Decision | Source |
|---|---|---|
| Exploration / proposal | Use a follow-on shell coherence spec instead of patching controls one by one or directly porting `hifi-design/`. | `docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md` |
| Authority model | `hifi-design/` is a benchmark/reference only; production authority remains VeloFile specs, tokens, scopes, XAML resources, tests, and deviation records. | Spec R1-R4, architecture UI design-system section, ADR 0010 |
| Scope | Preserve V1 behavior and do not add persisted theme/density, broad theme engine, plugin UI, dual-pane browsing, or other V1 expansion. | Spec R5-R8, R27, C1-C7 |
| Contract strategy | Extend `docs/ui/tokens.v1.json` and `docs/ui/ui-contract-scopes.v1.json` additively; do not create a token major version for this follow-on. | Spec R9-R15, ADR 0010 |
| Proof model | Static resource validation, behavior-preservation tests, accessibility checks, and deviation records are hard proof; screenshots/manual notes are optional supporting artifacts. | Spec R22, R66-R77, AC9-AC13, AC21 |
| Architecture boundary | UI resources and contract tooling may change; Core services and Windows adapters should not be redesigned for visual styling. | `docs/architecture/system/architecture.md`, ADR 0010 |
| Plan | Implement in region milestones: M1 contracts, M2 shell foundation, M3 file list/icons, M4 command band, M5 sidebar, M6 status/operations, M7 preview/details, optional M8 artifacts, M9 lifecycle closeout. | `docs/plans/2026-05-11-ui-shell-visual-coherence.md` |

## Diff Rationale By Area

| Files / area | Change | Reason | Source artifact | Test / evidence |
|---|---|---|---|---|
| `docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md` | Added accepted proposal for shell-wide visual coherence. | Captures why the UI issue is systemic and why a follow-on spec is needed. | Proposal option D | Proposal accepted |
| `specs/ui-shell-visual-coherence.md` | Added the shell visual-coherence contract and later amended it to make visual artifacts optional. | Defines requirements R1-R82, including behavior preservation, optional screenshot guardrails, and non-goals. | Spec R1-R82, AC1-AC21 | Spec reviews r1/r2 |
| `specs/ui-shell-visual-coherence.test.md` | Added traceable test spec, behavior-preservation matrix, and TSC001-TSC021. | Maps each requirement area to concrete static, app, corpus, behavior, accessibility, and optional artifact checks. | Test spec mapping table | Test-spec reviews r1/r2 |
| `docs/architecture/system/architecture.md`, `docs/adr/0010-shell-visual-coherence-contracts.md` | Added shell visual-coherence architecture rules and optional-artifact amendment. | Keeps the UI authority chain additive and separates visual fixture evidence from real behavior evidence. | ADR 0010, QS-UI-SHELL-01/02 | Architecture review r1 |
| `docs/plans/2026-05-11-ui-shell-visual-coherence.md`, `docs/plan.md` | Added and updated the living execution plan, progress, validation notes, and handoff state. | Keeps milestone sequencing, validation, review outcomes, and next stage explicit. | Plan milestones M1-M9 | Plan reviews r1-r4; M7 closeout |
| `docs/changes/2026-05-11-ui-shell-visual-coherence/*` | Added change record, review log, review-resolution record, code/spec/plan/test/architecture review records, visual evidence notes, and this explanation. | Gives reviewers durable evidence for why changes happened and how findings were resolved. | Workflow contract | Review log through code-review-r18 |
| `docs/ui/ui-contract-scopes.v1.json` | Added additive governed shell scopes, active region metadata, resource key requirements, fixture-icon invariants, behavior matrix metadata, and optional artifact guardrails. | Lets static validation enforce region-specific contracts without globally banning legacy XAML outside active scopes. | Spec R9-R15, R35-R43, R66-R82 | UI contract validator; `ShellVisualCoherenceContractTests` |
| `tools/VeloFile.UiContracts/Program.cs` | Extended validation for shell scopes, governed fixture icon invariants, raw icon color literals, optional sidecars, and scoped visual literals. | Static checks catch defect classes such as raw/default visuals, glyph icons, text chips, and unsafe optional artifacts before runtime review. | TSC003, TSC004, TSC009, TSC014, TSC019 | Corpus UI contract tests and validator runs |
| `src/VeloFile.App/App.xaml` | Merged new component dictionaries: Shell, CommandBand, Sidebar, Status, Operations, Preview, and FixtureIcons. | Makes governed resources available to the production shell through the app resource chain. | Spec R12, R23-R65 | Resource contract tests |
| `src/VeloFile.App/Resources/Components/VeloFile.Shell.xaml` | Added shell-wide surface, text, control, focus, accent, titlebar, contrast, toggle, and foundational resources. | Provides the shared dark comfortable shell surface model required before region polish. | R16-R27, CR-003 | `ShellSurfaceResourceContractTests` |
| `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` | Added deterministic fixture icon resources and file-list details/header/icon polish. | Replaces placeholder-like chips with allowlisted vector icons and keeps row rhythm stable. | R28-R43, AC6-AC8, AC16 | `FileListResourceContractTests`, fixture tests |
| `src/VeloFile.App/Ui/FileListIconKind.cs`, `FileListIconGeometryConverter.cs`, `ViewModels/FileListRowViewModel.cs`, `Testing/UiFixtureRegistry.cs` | Added allowlisted fixture icon kind plumbing and deterministic fixture mappings. | Prevents arbitrary resource-key fixture input and makes governed fixture rows deterministic. | R39-R40, S1, P3 | `UiFixtureRegistryTests`, icon contract tests |
| `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml`, `MainWindow.xaml` command band region | Added command-band styles, scoped markers, resource consumption, and state styling for buttons/TextBoxes. | Makes navigation/path/filter/search read as one region while preserving existing route handlers. | R44-R49, CR-008 | `CommandBandResourceContractTests`, command route filters |
| `src/VeloFile.App/Resources/Components/VeloFile.Sidebar.xaml`, `MainWindow.xaml` sidebar region | Added sidebar styles, navigation-first grouping, compact visibility toggles, accessible labels, and terminal control styling. | Moves sidebar toward navigation-first component anatomy while preserving discoverability and keyboard/access routes. | R50-R56, AC14 | `SidebarResourceContractTests` |
| `src/VeloFile.App/Resources/Components/VeloFile.Status.xaml`, `VeloFile.Operations.xaml`, `MainWindow.xaml`, `MainWindow.xaml.cs` operation/status paths | Added governed status/operation/destructive styles and status-to-style mapping. | Makes active operation, conflict, failed, cancelled, and destructive states visually distinct without changing operation services. | R57-R61 | `StatusOperationResourceContractTests` |
| `src/VeloFile.App/Resources/Components/VeloFile.Preview.xaml`, `MainWindow.xaml`, `MainWindow.xaml.cs` preview paths | Added preview pane/status/content/PDF/image/metadata styles and preview-status style mapping. | Integrates preview/details into the shell surface while preserving preview controller/provider and PDF navigation behavior. | R62-R65 | `PreviewResourceContractTests` |
| `tests/fixtures/ui-contracts/**` | Added/updated valid and invalid fixture resources for active scopes, command/status/sidebar/preview dictionaries, and icon literal failures. | Keeps the static validator representative and proves both allowed and rejected patterns. | TSC003, TSC009, TSC014 | Corpus UI contract validation |
| `tests/VeloFile.App.Tests/UiDesign/**` | Added focused App UI resource contract tests for shell surface, file list, command band, sidebar, status/operations, and preview. | Tests the actual XAML/resource wiring that reviewers cannot infer from key existence alone. | TSC006-TSC012, TSC015-TSC016 | M2-M7 focused app validation |
| `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs` | Added and updated corpus contract tests for shell scopes, icon invariant rules, optional sidecars, and active-region progression. | Keeps UI contract policy executable through the corpus/tooling layer. | TSC001-TSC005, TSC008-TSC014 | Corpus tests and CI |

Note: the current branch also contains separate test-runtime optimization changes with their own proposal/spec/plan/change record. This explanation covers only `2026-05-11-ui-shell-visual-coherence`.

## Tests Added Or Changed

| Test / area | What it proves | Why this level is appropriate |
|---|---|---|
| `ShellVisualCoherenceContractTests` | Shell scopes, fixture icon invariants, optional sidecars, and active scope metadata obey the contract. | Corpus/static tests are the right level for contract policy that does not require launching the app. |
| `ShellSurfaceResourceContractTests` | Shell dictionary merge, shell surface resource keys, titlebar resource bridge, compact toggles, contrast pairs, route preservation, and governed resource consumption. | App-level static XAML tests prove production shell resources are wired without changing runtime behavior. |
| `FileListResourceContractTests` | File-list header/icon resources, deterministic vector icon treatment, row-state stability, no placeholder chips, and focus/selection separation. | File-list polish is mostly XAML/resource contract plus fixture/view-model determinism. |
| `UiFixtureRegistryTests` | Fixture rows expose allowlisted icon kinds and wait for deterministic rendered fallback state. | Fixture tests prove the deterministic visual fixture input path without depending on real Shell icon extraction. |
| `CommandBandResourceContractTests` | Command-band resources exist and are consumed by governed buttons/TextBoxes, including hover/pressed/disabled/focused state resources. | CR-008 showed key existence was insufficient; these tests prove actual style consumption. |
| `SidebarResourceContractTests` | Sidebar dictionary merge, active scope, navigation-first order, accessible names, access keys, and existing handlers. | Sidebar reordering is observable behavior, so static order/accessibility checks and route preservation are required. |
| `StatusOperationResourceContractTests` | Status/operation/destructive resources, separate danger/focus treatment, operation status style mapping, and existing route preservation. | Safety surfaces need focused checks because destructive treatment must not share ordinary focus/accent meaning. |
| `PreviewResourceContractTests` | Preview resources, scoped preview XAML usage, preview status style mapping, PDF navigation wiring, active scope metadata, and unchanged preview width behavior. | Preview changes are UI styling only, so tests prove resource wiring and route preservation rather than provider behavior. |
| Existing filtered behavior tests | Navigation, search, app shell, file list, operations, preview, terminal, accessibility, and UI contract routes remain covered. | Spec R78-R82 require behavior-preservation evidence; screenshots/fixtures cannot replace these routes. |

## Validation Evidence Available Before Final Verify

This is milestone validation evidence, not final `verify` evidence.

- M1:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"`: passed after expected initial failures.
  - `dotnet test VeloFile.sln -c Debug --filter UiContracts`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
- M2:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurfaceResourceContractTests|AppShellContract|UiDesign"`: passed.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
- M3:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FileListResourceContractTests|UiFixtureRegistryTests"`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|FileList|AppShellContract|DragDrop|Preview"`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
- M4:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~CommandBandResourceContractTests"`: passed after CR-008 resolution.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "AppShellCommandRouteTests|CommandBand|Accessibility"`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
- M5:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~SidebarResourceContractTests"`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Terminal|Accessibility"`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
- M6:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~StatusOperationResourceContractTests"`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "Operation|FileOperation|Status|Accessibility"`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
- M7:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~PreviewResourceContractTests"`: passed.
  - `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed after tokenizing the preview border thickness.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Preview|PreviewSurface|Accessibility"`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|Preview|FileListResourceContractTests"`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed.
  - `git diff --check`: passed with line-ending normalization warnings only.
- Code-review-r18 reviewer reruns:
  - `PreviewResourceContractTests`: passed, 4 tests.
  - production UI contract validator: passed.
  - `Preview|PreviewSurface|Accessibility`: passed, 19 tests.
  - `git diff --check`: passed.

Final verification still needs to run the repository's final closeout commands in the `verify` stage.

## Review Resolution Summary

Detailed review findings and dispositions are in `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`.

Summary by disposition:

- Resolved by implementation: CR-001, CR-002, CR-003, CR-004, CR-006, CR-007, CR-008, PR-002, TSR-001.
- Resolved or superseded by approved visual-evidence amendment: CR-005.
- Superseded by approved visual-evidence amendment: PR-001.
- No-material code reviews after resolution: code-review-r3, r9, r10, r15, r16, r17, r18.

The final implementation review is `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r18.md`, which closed M7 with no material findings. M1-M7 are closed and optional M8 is not triggered because no optional current screenshots, diffs, sidecars, or baselines were added after the visual-evidence gate amendment.

## Alternatives Rejected

- Directly porting `hifi-design/`: rejected because it would make the reference artifact an implicit production authority and risk importing non-WinUI assumptions.
- Creating `tokens.v2.json`: rejected because the work adds regions and resources but does not change token semantics incompatibly.
- Hard-gating screenshots or pixel diffs: rejected because screenshot automation, dynamic regions, scaling, and rasterization are not stable enough to act as release proof in this slice.
- Treating optional screenshots or fixture visuals as behavior proof: rejected because behavior must remain proven through App/Core/Windows routes.
- Using real Windows Shell icons for first deterministic fixture icon work: deferred because real Shell icons add OS/theme/cache variance.
- Rewriting Core services, Windows adapters, preview providers, file operations, or persistence for visual polish: rejected as outside the approved shell visual-coherence scope.

## Scope Control

The change deliberately stays inside these boundaries:

- Production behavior routes are preserved unless a region explicitly needed source-order or visual grouping changes approved by the spec.
- Core domain services and Windows adapters are not redesigned for UI styling.
- The first-slice UI design-system spec remains authoritative for its original file-list/token scope.
- `hifi-design/` remains reference input only.
- No persisted theme/density preference, broad theme engine, tweak panel, plugin UI, dual-pane browsing, or other V1 feature expansion was added.
- Generated `tests/visual/current/` and `tests/visual/diffs/` artifacts remain uncommitted.
- The separate test-runtime optimization work on this branch is governed by its own change record and is not part of this explanation.

## Risks And Follow-Ups

- Final `verify` has not run yet. The next stage must run final validation before any PR readiness claim.
- Runtime screenshot/manual visual review is optional under the amended contract; this explanation does not claim post-M7 screenshot acceptance.
- The UI is governed region by region, but high-fidelity product polish can still improve after V1 if new proposals/specs accept that work.
- Real Windows Shell icons remain future integration evidence; fixture icons are deterministic visual-contract assets, not OS icon parity proof.
- Optional M8 remains available if maintainers later record optional screenshots, sidecars, diffs, baselines, or manual notes that need consolidation.
