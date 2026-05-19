# Code Review R2: PR CI Validation Tiering M1

## Review Status

clean-with-notes

## Review Inputs

- Review surface: M1 implementation through commit `3e8abd2` (`M1: resolve CI runtime summary review finding`)
- Prior review: [code-review-r1](code-review-r1.md)
- Review resolution: [review-resolution.md](../review-resolution.md)
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M1
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Validation evidence: plan validation notes, change metadata, commit message, and rerun validation during this review.

## Diff Summary

M1 adds `scripts/Write-CiRuntimeSummary.ps1`, wires the existing broad `.github/workflows/ci.yml` job to write a GitHub Actions summary after `./scripts/ci.ps1`, and adds focused `CiRuntimeSummaryTests`. The PRCI-CR1 resolution gives the broad CI step a stable `repository_ci` id, keeps the existing `./scripts/ci.ps1` command under `shell: pwsh`, keeps the summary step as `if: always()`, passes failed-command and command-outcome context when the broad CI step outcome is not `success`, and extends the helper/test coverage for that failure context.

## Findings

No blocking or required-change findings.

## PRCI-CR1 Re-Review

PRCI-CR1 is resolved.

- `.github/workflows/ci.yml` gives the broad CI step `id: repository_ci`.
- The broad CI step still runs `./scripts/ci.ps1` with `shell: pwsh` and does not use `continue-on-error`.
- The summary step still uses `if: always()`.
- The summary step reads `${{ steps.repository_ci.outcome }}` and only passes `-FailedCommand` and `-FailedOutcome` when the outcome is not `success`.
- `Write-CiRuntimeSummary.ps1` reports `Failed command` and `Command outcome` only when explicit workflow-provided values are supplied.
- `CiRuntimeSummaryTests` proves failed command plus missing structured output, command outcome reporting, redaction of outcome values, and committed workflow wiring from `steps.repository_ci.outcome`.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | R40-R46 and AC12 are covered by the helper output and broad-CI summary hook. The EC7 failed-before-TRX path now has workflow-supplied failed-command context. |
| Test coverage | pass | `CiRuntimeSummaryTests` cover lane/tier fields, slow TRX parsing, missing structured output, failed command/outcome, redaction, and broad workflow summary wiring. |
| Edge cases | pass | Missing TRX does not fabricate slow-test rows, and failed command/outcome are only emitted from explicit inputs rather than inferred from missing output. |
| Error handling | pass | The summary step runs with `if: always()` while the broad CI step remains a normal failing step, preserving job failure semantics. |
| Architecture boundaries | pass | Runtime reporting stays in the shared PowerShell helper and GitHub Actions workflow layer; no production App/Core/Windows/Corpus behavior changed. |
| Compatibility | pass | `scripts/ci.ps1` command selection remains unchanged, and no fast-lane, release-evidence, branch-protection, or category policy changed in M1. |
| Security/privacy | pass | The helper redacts secret/token/credential/password/certificate-like values and private Windows profile paths; the workflow does not reference `secrets.`. |
| Derived artifact currency | pass | Review log, review-resolution, change metadata, plan, and change notes were updated to reflect PRCI-CR1 resolution and M1 review state. |
| Unrelated changes | pass | The reviewed diff is limited to M1 runtime-summary workflow/helper/tests and lifecycle records. |
| Validation evidence | pass | `FullyQualifiedName~CiRuntimeSummary` passed with 4 tests during review; prior `WorkflowContract` filter evidence is recorded as no matching tests. |

## No-Finding Rationale

No required-change findings remain because the implementation now provides the failure context that PRCI-CR1 required, keeps the broad CI command semantics intact, avoids `continue-on-error`, and has direct regression coverage for both helper output and committed workflow wiring.

## Residual Risks

- The `FullyQualifiedName~WorkflowContract` filter has no matching tests in this project. This is non-blocking for M1 because the committed workflow wiring is covered by `CiRuntimeSummaryTests`, and fuller workflow contract test coverage is planned for later milestones.

## Recommended Next Stage

Close M1 and proceed to `implement` M2.
