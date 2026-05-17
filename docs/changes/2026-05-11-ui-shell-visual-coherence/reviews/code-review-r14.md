# Code Review R14: M4 Command Band Under Amended Proof Model

## Review Status

changes-requested

## Reviewed Milestone

M4. Command Band Visual Coherence

## Review Inputs

- Implementation diff: `9699b23` (`M4: Style command band pending visual evidence`)
- Current amendment state: visual-evidence gate removal approved by `spec-review-r2`, `architecture-review-r1`, `plan-review-r4`, and `test-spec-review-r2`
- Feature spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0010-shell-visual-coherence-contracts.md`
- Scope metadata: `docs/ui/ui-contract-scopes.v1.json`
- Validation evidence: M4 commands recorded in `change.yaml` and plan; reviewer reran focused command-band tests and the broader M4 route/accessibility filter.

## Diff Summary

M4 adds `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml`, merges it through `App.xaml`, activates the `shell-command-band` scope, applies command-band styles to navigation, breadcrumb/path, filter/search, skipped-location, and status controls in `MainWindow.xaml`, and adds resource contract coverage in `CommandBandResourceContractTests`.

The 2026-05-17 amendment removes mandatory screenshot/manual full-shell evidence gates, so this review does not require `shell-filter-active` or `shell-search-active` visual evidence to close CR-005.

## Findings

### CR-008: Command-band state tokens are declared but not consumed by governed controls

Severity: major

Evidence:

- Spec R45 requires command-band controls in governed scope to share tokenized hover, focus, pressed, disabled, and input border/background states: `specs/ui-shell-visual-coherence.md`.
- Spec R48 requires disabled command-band controls to be visibly disabled and distinguishable without relying on color alone.
- `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml` declares state resources such as `VfCommandBandButtonHoverBackgroundBrush`, `VfCommandBandButtonPressedBackgroundBrush`, `VfCommandBandButtonDisabledBackgroundBrush`, `VfCommandBandButtonDisabledForegroundBrush`, `VfCommandBandInputFocusBorderBrush`, and `VfCommandBandDisabledOpacity`.
- A focused scan found those state resources are only declared, listed in scopes/fixtures, or checked for key existence. They are not consumed by the production command-band button/textbox styles except for `VfCommandBandInputPlaceholderBrush`.
- `tests/VeloFile.App.Tests/UiDesign/CommandBandResourceContractTests.cs` verifies that the keys exist, but it does not prove hover, pressed, disabled, or focused-border state resources are actually wired into the governed command controls.

Required outcome:

- Governed command-band controls must actually consume tokenized hover, pressed, disabled, focus, and input-border state resources, or the governing spec must be revised to narrow that requirement.
- Tests must fail if the command-band dictionary only declares state resources without wiring them to the relevant control state visuals.

Safe resolution path:

- Keep the fix scoped to M4 command-band resource/XAML tests and styles.
- Wire command-band `Button` and `TextBox` state visuals through WinUI-recognized scoped resources or an equivalent accepted style mechanism, using VeloFile resource aliases rather than local literals.
- Add focused tests proving `VfCommandBandButtonHoverBackgroundBrush`, `VfCommandBandButtonPressedBackgroundBrush`, disabled background/foreground or opacity, and `VfCommandBandInputFocusBorderBrush` are consumed by governed command-band controls.
- Rerun the focused M4 validation commands and return M4 to code review.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | M4 satisfies command route preservation, but R45/R48 state styling is not fully implemented. |
| Test coverage | concern | Tests cover resource existence and route preservation, but not state-resource consumption. |
| Edge cases | concern | Disabled and focused command-band visual states are named edge cases in R45/R48 and lack direct proof. |
| Error handling | pass | No new error-path behavior is introduced by the reviewed diff. |
| Architecture boundaries | pass | Changes stay in App XAML/resources, UI contract scopes, fixtures, and tests; no Core/Windows boundary changes observed. |
| Compatibility | pass | Existing command handlers and view-model route calls are preserved. |
| Security/privacy | pass | No new local path, fixture input, telemetry, or screenshot artifact exposure observed. |
| Derived artifact currency | pass | Scope metadata and valid fixture metadata include `shell-command-band`. |
| Unrelated changes | pass | The implementation diff is scoped to command-band resources, XAML usage, UI contract scope fixtures, tests, and change records. |
| Validation evidence | concern | Focused tests and UI contract validation pass, but passing evidence does not prove the missing state-resource wiring. |

## Reviewer Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~CommandBandResourceContractTests"`: passed, 4 tests.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
- `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`: passed; App 64, Core 6, Corpus 20 matching tests passed; Windows had no matching tests for the filter.
- `git diff --check`: passed with CRLF normalization warnings only.

## Milestone-Aware Handoff

- Reviewed milestone: M4. Command Band Visual Coherence
- Review status: changes-requested
- Milestone state after review: resolution-needed
- Required review-resolution: CR-008
- Remaining in-scope implementation milestones: M4-M7; M8 optional only if visual artifacts are recorded
- Next stage: review-resolution for CR-008
- Final closeout readiness: not ready; M4 remains open and M5-M7 are still planned
