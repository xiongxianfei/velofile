# Code Review R1: M1 UI Contract Artifacts and Static Validator

## Review status

changes-requested

## Review inputs

- Diff range: `3a9e559^..3a9e559`
- Review surface: M1 commit `3a9e559` plus current tracked governing artifacts
- Spec: `specs/ui-design-system-shell-redesign.md`
- Test spec: `specs/ui-design-system-shell-redesign.test.md`
- Plan: `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md`
- Validation evidence: M1 validation recorded in `docs/changes/2026-05-11-ui-design-system-shell-redesign/change.yaml`

## Diff summary

M1 adds repo-owned UI contract artifacts, a static .NET validator under `tools/VeloFile.UiContracts`, controlled valid/invalid XAML fixtures, focused `UiContracts` MSTest coverage, and CI execution of the valid-fixture validator command. It also records the approved proposal, spec, test spec, architecture/ADR updates, active plan, and change-local explanation.

## Findings

### CR-M1-001: Extra first-slice resources are not validated

- Severity: blocker
- Evidence: `specs/ui-design-system-shell-redesign.md` R41 requires the checker to validate "missing or extra first-slice resources." In `tools/VeloFile.UiContracts/Program.cs`, `ValidateTokens` only iterates required contract tokens and their declared keys (`Program.cs` lines 127-160). `XamlResourceSet` keeps parsed resources private and exposes only `TryGet`/`Get` by key (`Program.cs` lines 525-537), so there is no check that reports extra governed first-slice resource keys. The tests cover missing, duplicate, wrong type, wrong value, and wrong brush fixtures, but no extra-resource fixture exists in `tests/VeloFile.Corpus.Tests/UiContracts/UiContractTests.cs` lines 105-124.
- Required outcome: The validator must fail with an actionable message when governed first-slice resource dictionaries contain unapproved extra first-slice resources.
- Safe resolution path: Expose parsed resource keys to validation, define the governed extra-resource rule narrowly enough to avoid blocking legacy XAML, add an invalid `extra-resource` fixture, and extend the `UiContracts` tests plus validator diagnostics.

### CR-M1-002: Strict tokenized-literal rules are not enforced for new token/component resource dictionaries

- Severity: blocker
- Evidence: `specs/ui-design-system-shell-redesign.md` R42 requires strict tokenized-literal rules in new token and component resource dictionaries. The validator only runs literal detection inside `ValidateScopes` when `--scopes` is passed (`Program.cs` lines 39-41 and 276-324). The M1 CI command in `scripts/ci.ps1` runs only `validate-tokens --contract ... --xaml-root tests/fixtures/ui-contracts/valid`, so no strict resource-dictionary literal rule runs in CI. The literal rules are scoped text regex checks (`Program.cs` lines 357-368) and are not applied to `Resources/Tokens` or `Resources/Components` as strict dictionary policy.
- Required outcome: New token/component resource dictionaries must be checked for forbidden unapproved literals as part of the static validator path, without imposing a global ban on legacy XAML outside first-slice scope.
- Safe resolution path: Add a validator mode or default rule that applies strict literal checks to governed resource dictionary roots, add invalid fixtures for component/token resource literals, and update CI/tests to exercise that path.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | R41 and R42 are not fully implemented. |
| Test coverage | concern | T003/T004 cover several invalid fixtures, but not extra resources or strict resource-dictionary literal policy. |
| Edge cases | concern | EC5 and EC6 have partial proof; extra-resource drift is unproved. |
| Error handling | pass | Existing validator failures return nonzero and actionable messages for covered cases. |
| Architecture boundaries | pass | Tool is in `tools/`, solution-included, and has no WinUI runtime dependency. |
| Compatibility | pass | No production app behavior changed in M1. |
| Security/privacy | pass | Tool consumes local paths and no upload/telemetry path was introduced. |
| Derived artifact currency | pass | No generated artifacts are introduced in M1. |
| Unrelated changes | pass | Diff is broad because it includes approved lifecycle artifacts, but it matches the workflow surface for this initiative. |
| Validation evidence | pass | Recorded commands include restore, build, direct validator run, filtered tests, and full `scripts/ci.ps1`; the evidence does not cover the two findings. |

## Required resolution

Enter `review-resolution` for M1. Do not start M2 until CR-M1-001 and CR-M1-002 are resolved and M1 returns to `code-review`.
