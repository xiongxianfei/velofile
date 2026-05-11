# UI Design-System and Shell Redesign Change Explanation

## Summary

This change implements the first VeloFile UI design-system slice from M1 through M4. It defines repo-owned UI contracts, validates those contracts statically, moves the first file-list row presentation into WinUI resources, adds guarded deterministic UI fixtures, and records reviewed visual baseline evidence.

The work intentionally does not port `hifi-design/` into production. The reference package remains benchmark input only. Production authority now flows from accepted VeloFile specs and UI contract artifacts into checked-in WinUI resources and tests.

## Problem

The app had working V1 file-manager behavior, but the visible shell did not yet have a repo-owned UI standard. The original concern was that the current UI was far away from the high-fidelity reference. The accepted direction reframed that problem: VeloFile should not mechanically align to the prototype; it should define its own WinUI-native visual system that can meet or exceed the reference while preserving V1 behavior, accessibility, performance, and Windows-native expectations.

The implementation also had to avoid a common failure mode: hiding design decisions inside broad `MainWindow.xaml` edits without a token contract, fixture discipline, or reviewable visual evidence.

## Decision Trail

| Decision source | Decision used in implementation |
|---|---|
| Proposal | Choose a repo-owned VeloFile design system and phased shell redesign; treat `hifi-design/` as reference input only. |
| Spec | R1-R8 define authority and fixed first-slice defaults; R15-R25 define `docs/ui/tokens.v1.json`; R26-R47 define WinUI resource and static validation rules; R48-R61 define file-list row resources; R62-R79 define fixture and visual baseline evidence; R80-R84 define design-deviation handling. |
| Test spec | Map token contracts, resource dictionaries, fixture gates, file-list row states, visual baselines, and V1 behavior preservation to concrete tests. |
| Architecture / ADR 0009 | UI authority flows from accepted repo artifacts to WinUI resources and validation. Static validation lives in `tools/VeloFile.UiContracts`; visual baseline updates are review-gated PowerShell; fixture mode is Debug/test-only plus environment guard plus allowlist. |
| Plan | M1 contracts and validator, M2 WinUI resources and file-list rows, M3 guarded fixtures, M4 visual baselines, then M5 lifecycle closeout. |

## Diff Rationale By Area

| Area / files | Change | Reason | Source artifact | Test/evidence |
|---|---|---|---|---|
| `docs/ui/tokens.v1.json` | Added first-slice dark/comfortable token contract. | Makes VeloFile-owned tokens the machine-readable source of truth instead of `hifi-design/`. | R15-R25, AC1 | `UiContractTests`, validator runs. |
| `docs/ui/ui-contract-scopes.v1.json` | Added first active file-list scope, required resource references, and allowed component resources. | Lets validation govern new resources and targeted legacy XAML without banning all old literals. | R36-R47, AC2 | `UiContractTests`, M1 review-resolution tests. |
| `docs/ui/design-deviations.md` | Added deviation policy and template. | Creates a durable place to record intentional differences from reference material. | R80-R84, AC3 | Artifact existence and review workflow. |
| `tools/VeloFile.UiContracts/` and `VeloFile.sln` | Added solution-included static validator. | Validates token JSON and XAML resources without launching WinUI. | ADR 0009, M1 | `dotnet run --project tools\VeloFile.UiContracts ...`, `UiContractTests`. |
| `tests/fixtures/ui-contracts/` | Added valid and invalid token/resource fixtures. | Proves missing keys, duplicate keys, wrong values/types, extra resources, and forbidden literals fail with actionable diagnostics. | M1 test spec, CR-M1-001, CR-M1-002 | `dotnet test ... --filter UiContracts`. |
| `scripts/ci.ps1` | Added UI contract validation to normal CI path. | Keeps first-slice token/resource drift visible during ordinary validation. | AC5-AC6, ADR 0009 | `scripts/ci.ps1` validation notes. |
| `src/VeloFile.App/Resources/Tokens/*.xaml` | Added checked-in WinUI resource dictionaries for colors, typography, spacing, sizing, radius, focus, density, state, and motion. | Bridges accepted tokens into WinUI resources without a generated token pipeline. | R26-R35, ADR 0009 | UI contract validator and app build. |
| `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml` | Added named file-list row template, item-container style, row text styles, selection/focus resources, opacity resources, and converter resource. | Makes file-list presentation reviewable and token-governed. | R48-R61, AC14, CR-M2-001, CR-M2-002 | `FileListResourceContractTests`. |
| `src/VeloFile.App/App.xaml` | Merged first-slice token and component dictionaries. | Makes app resources available through the normal WinUI resource tree. | R26, M2 | App build and resource merge tests. |
| `src/VeloFile.App/MainWindow.xaml` | Replaced inline file-list row template references with `VfFileListRowTemplate` and `VfFileListItemContainerStyle`; scoped file-list selected/hover resource overrides. | Moves row presentation out of broad shell XAML while preserving existing list behavior. | R48-R61, non-goals | `FileListResourceContractTests`, app-shell tests. |
| `src/VeloFile.App/ViewModels/FileListRowViewModel.cs`, `FileListRowOpacityResourceSelector.cs`, `Ui/FileListRowOpacityConverter.cs` | Replaced rendered `RowOpacity` authority with semantic visibility state and resource-backed opacity selection. | Hidden/protected row visual state must be governed by VeloFile resources, not hardcoded VM opacity. | CR-M2-002 | `FileListResourceContractTests`, `AppShellContractTests`. |
| `src/VeloFile.App/Testing/UiFixtureLaunch.cs` | Added guarded fixture argument parsing and launch acceptance rules. | Fixture mode must be non-production, explicit, and allowlisted. | R62-R68, AC11-AC13, ADR 0009 | `UiFixtureLaunchTests`, Release build. |
| `src/VeloFile.App/Testing/UiFixtureRegistry.cs` | Added deterministic `file-list-v1` and `file-list-empty-folder` fixtures plus presentation state. | M4 screenshots need stable rows and selected/focused/multi-selected targets without disk fixtures. | R62-R69, CR-M3-001 | `UiFixtureRegistryTests`. |
| `src/VeloFile.App/App.xaml.cs`, `AppCompositionRoot.cs`, `MainWindow.xaml.cs` | Wired accepted fixture launches into shell creation and applied fixture presentation state through `FileListSurface`. | Screenshots must render actual selected/focused UI state, not unused metadata labels. | CR-M3-001 | Fixture tests and local screenshot capture. |
| `src/VeloFile.App/ViewModels/AppShellViewModel.cs` | Rechecked active listing result inside the dispatcher callback before applying listing state. | Prevents stale queued listing completions from replacing visible rows after newer navigation wins. | CR-M4-001 | `QueuedListingCompletion_DoesNotApplyAfterNewerNavigationWins`. |
| `.gitignore` | Ignored `tests/visual/current/` and `tests/visual/diffs/`. | Generated visual outputs must not be committed. | R74-R75, AC9 | `VisualBaselineInventoryTests`. |
| `scripts/update-ui-baselines.ps1` | Added review-gated baseline update command requiring `-ReviewId`. | Baseline mutation must be deliberate maintainer action, not normal CI side effect. | R76-R79, AC10, ADR 0009 | Script tests in `VisualBaselineInventoryTests`. |
| `tests/visual/baselines/winui/dark-comfortable-1440x900-100/` | Added nine PNG baselines and JSON sidecars. | Provides first-slice visual evidence for required file-list states. | R69-R73, AC8 | Inventory tests and local capture notes. |
| `tests/VeloFile.App.Tests/` | Added/updated file-list resource, fixture, command-route, and app-shell tests. | Proves production/resource paths rather than only artifact existence. | M2-M4 test spec, review findings | Focused MSTest commands and CI script. |
| `tests/VeloFile.Corpus.Tests/` | Added UI contract and visual baseline inventory tests. | Keeps static contracts and visual evidence auditable without launching the full app in corpus tests. | M1, M4 | `UiContracts`, `Visual` filters. |
| `docs/changes/...`, `docs/plan.md`, `docs/plans/...` | Recorded validation, review findings, resolutions, and milestone closeout. | Maintains workflow traceability and prevents silent handoff over unresolved findings. | AGENTS.md, constitution, plan | Code-review closeout and current handoff summary. |

## Tests Added Or Changed

| Test area | What it proves | Why this level is appropriate |
|---|---|---|
| `UiContractTests` | Token JSON shape, required XAML keys, value/type checks, duplicate keys, extra governed resources, strict resource literals, and scoped legacy XAML checks. | Static validation is the contract boundary for M1 and does not require a running app. |
| `FileListResourceContractTests` | Resource dictionaries merge, row template/style exist, selected/focused state resources are consumed, hidden/protected opacity resolves from tokens, and no custom row control was introduced. | M2 is presentation-resource work; static XAML/resource assertions directly guard drift. |
| `AppShellContractTests` | Existing app-shell row state inputs remain available after file-list resource extraction. | Protects behavior routes while the visual template moves out of `MainWindow.xaml`. |
| `UiFixtureLaunchTests` | Fixture launch rejects Release/production, missing env guard, unknown fixtures, unsupported options, and arbitrary fixture data paths; allowlisted Debug/test fixture launches pass. | Fixture mode is a startup safety boundary, so parser/gate tests are the right first proof. |
| `UiFixtureRegistryTests` | Deterministic fixture rows and presentation state are explicit, stable-ID based, preserved into shell state, and not inferred from filenames. | M3 fixture data is deterministic app-shell setup, not filesystem integration. |
| `AppShellCommandRouteTests.QueuedListingCompletion_DoesNotApplyAfterNewerNavigationWins` | A queued old listing completion cannot overwrite visible rows after newer navigation completes. | Directly reproduces the dispatcher race from CR-M4-001. |
| `VisualBaselineInventoryTests` | Required screenshots and sidecars exist, metadata is safe, generated outputs are ignored, baseline update requires review ID/current files, and normal CI does not mutate baselines. | M4 visual evidence is committed artifact validation plus script guardrails, not hard pixel diff. |

## Validation Evidence Available Before Final Verify

Validation is recorded in `docs/changes/2026-05-11-ui-design-system-shell-redesign/change.yaml` and the active plan. The key evidence includes:

- M1: `dotnet restore VeloFile.sln`, `dotnet build VeloFile.sln -c Debug`, validator run against valid fixtures, `dotnet test VeloFile.sln -c Debug --filter UiContracts`, and `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed.
- M1 review-resolution: focused UI contract tests, scoped validator command, build, and CI script passed.
- M2: file-list resource tests failed before implementation for missing resources, then passed; production resource validation, Debug build, filtered app/corpus tests, and CI script passed.
- M2 review-resolution: focused file-list contract tests, scoped validator command, build, app-shell tests, UI contract tests, and CI script passed.
- M3: fixture tests failed before implementation for missing fixture code, then passed; filtered solution fixture/UI contract tests, Release app build, path/privacy `rg` check, and CI script passed.
- M3 review-resolution: fixture tests failed before presentation-state resolution, then passed; filtered solution tests, Release app build, and CI script passed.
- M4: visual inventory tests failed before implementation for missing baselines/script/ignore rules, then passed; fixture launch/registry tests, Debug app build, baseline update script, filtered visual/UI contract tests, generated-output status check, and CI script passed.
- M4 review-resolution: stale listing regression failed before the fix and passed after; `AppShellCommandRouteTests`, filtered solution tests, and CI script passed.
- M4 rerun code-review: clean-with-notes; no required-change findings remain.

This is not final `verify` evidence. Final verification still needs to run in the `verify` stage.

## Review Resolution Summary

All material findings recorded through M4 are resolved in `docs/changes/2026-05-11-ui-design-system-shell-redesign/review-resolution.md`.

| Finding | Disposition |
|---|---|
| CR-M1-001 | Resolved: validator rejects extra governed first-slice resources. |
| CR-M1-002 | Resolved: strict literal validation runs for governed token/component dictionaries. |
| CR-M2-001 | Resolved: file-list selection/focus visuals consume named first-slice resources. |
| CR-M2-002 | Resolved: hidden/protected opacity is resource-governed rather than VM-hardcoded. |
| CR-M3-001 | Resolved: fixture selected/focused/multi-selected presentation state is carried and applied through the UI boundary. |
| CR-M4-001 | Resolved: queued listing completion rechecks active result at dispatcher mutation time. |

No finding is marked `needs-decision`, and the M4 rerun code review closed the final implementation milestone with no remaining required-change findings.

## Alternatives Rejected

These alternatives were considered in the approved proposal or ADR and were not implemented:

- Directly port the prototype into `MainWindow.xaml`: rejected because it would mix visual decisions and behavior risk in one broad shell diff.
- Treat `hifi-design/` or its token file as the production source of truth: rejected because VeloFile owns its production UI contract.
- Keep tokens and row styling inline in `MainWindow.xaml`: rejected because it makes drift hard to validate.
- Generate WinUI resources from a token pipeline in the first slice: rejected as premature for the small fixed dark/comfortable baseline.
- Use MSBuild item metadata as the first validation source: rejected because static JSON/XAML parsing better validates keys, values, types, and literals.
- Silently ignore `--test-ui-fixture` outside allowed contexts: rejected because screenshot jobs could capture the wrong UI.
- Add `DebugUiTest` immediately: rejected because the first fixture family does not justify extra build/CI/packaging surface.
- Add hard-gated pixel comparison in M4: rejected/deferred because first-slice screenshots are soft review evidence until visual automation is stable.

## Scope Control

The implementation preserves the approved non-goals:

- No pixel parity requirement with `hifi-design/`.
- No JSX/web prototype port.
- No runtime theme or density switcher.
- No persisted theme/density settings.
- No generated token pipeline.
- No custom file-list row control.
- No new file-list selection or virtualization behavior.
- No disk-backed visual fixtures in the first visual baseline.
- No Appium/WinAppDriver or `DebugUiTest`.
- No hard-gated screenshot pixel diff.
- No file-operation, preview, search, drag/drop, terminal, persistence, diagnostics, or Windows adapter rewrite.

## Risks And Follow-Ups

- Final verification has not run yet; the next lifecycle stage is `verify`.
- The visual baselines are review evidence, not a release-quality pixel-diff gate.
- Process-level GUI launch checks remain limited; fixture safety is currently covered by unit/static startup tests plus local guarded screenshot capture.
- Future persisted theme/density behavior needs a separate spec and architecture update.
- Future hard visual-regression gates should add a dedicated visual diff helper and stable CI capture profile before becoming release blockers.

## Current Readiness

All implementation milestones M1-M4 are closed. `explain-change` was completed before final verify.

## Post-Verify Bugfix Addendum

After local final verification, shell icon buttons were reported as rendering garbled icon text. The bug was caused by shell toolbar buttons using `SymbolIcon` glyphs, which depend on icon-font/private-use glyph resolution. If that font path fails, the UI can show garbled text instead of icons.

The fix is intentionally scoped to `MainWindow.xaml` icon buttons: all shell `SymbolIcon`/icon-control content was replaced with raw vector `Path` shapes inside `Viewbox` elements. This avoids both icon font glyph lookup and icon-control rendering. Click handlers, tooltips, automation names, keyboard accelerators, and command routing are unchanged.

Regression coverage was tightened in `AppShellContractTests.Main_window_icon_buttons_use_raw_vectors`. The test fails if any `SymbolIcon` or `PathIcon` returns to `MainWindow.xaml` and checks that all shell icon buttons use raw vector content.

Validation for the addendum:

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter Main_window_icon_buttons_use_raw_vectors` passed.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests` passed with 19 tests.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `rg -n "<SymbolIcon|<PathIcon" src\VeloFile.App\MainWindow.xaml` returned no matches.

This post-verify code change superseded the prior branch-ready state until code review and final verification were rerun. The shell icon bugfix is now reviewed and renewed final verification has passed.
