# Code Review R6: PR CI Validation Tiering M3 Re-Review

## Review Status

clean-with-notes

## Reviewed Milestone

M3. Release-Evidence Workflow

## Review Inputs

- Review surface: PRCI-CR3 resolution through commit `1c0b553` (`M3: resolve release evidence workflow review finding`)
- Prior review: [code-review-r5](code-review-r5.md)
- Review resolution: [review-resolution.md](../review-resolution.md)
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M3
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Implementation files:
  - `.github/workflows/release-evidence.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - M3 lifecycle records and change notes

## Diff Summary

The PRCI-CR3 resolution keeps `.github/workflows/release-evidence.yml` unchanged and strengthens only the test-owned workflow contract surface. `CiWorkflowStep` now exposes step-level `if` values, and `ValidateReleaseEvidenceLane` adds release-evidence-specific diagnostics for failure semantics. `CiWorkflowContractTests` now assert that the committed `test_release_evidence` step does not use `continue-on-error`, that the release-evidence summary step uses `if: always()`, and that an invalid workflow fixture reports actionable diagnostics for both violations.

Lifecycle records were updated to mark PRCI-CR3 resolved and to request this M3 code-review rerun.

## Findings

No blocking or required-change findings.

## PRCI-CR3 Re-Review

PRCI-CR3 is resolved.

- The workflow model parses step-level `if` into `StepIfCondition`.
- `ValidateReleaseEvidenceLane` combines hosted-lane checks with release-evidence failure-semantics diagnostics.
- `Release_evidence_lane_runs_release_evidence_validation_and_reports_expensive_categories` proves the committed release-evidence validation step has `ContinueOnError == false` and the summary step has `if: always()`.
- `Release_evidence_lane_diagnostics_name_failure_semantics_violations` proves invalid fixtures fail with diagnostics for `ContinueOnError=true` and a missing summary `always()` condition.
- The release-evidence workflow triggers, commands, failure behavior, artifact paths, and summary helper behavior were not changed by the resolution.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | R32 and R40 are now directly protected by workflow contract tests; the existing release-evidence lane still aligns with R4-R8, R11, R28-R34, R40-R48, and R65-R69. |
| Test coverage | pass | `CiWorkflowContractTests` now cover release-evidence triggers, environment, command order, category/status reporting, TRX artifacts, inventory drift, validation-step failure semantics, and summary `if: always()` behavior. |
| Edge cases | pass | The invalid fixture proves tests fail if `continue-on-error: true` is added or if the summary step omits `always()`. |
| Error handling | pass | Failure context still reads `steps.test_release_evidence.outcome`, and the validation step remains a normal failing step rather than a tolerated failure. |
| Architecture boundaries | pass | The resolution stays in test-owned workflow parsing and lifecycle records; no production App/Core/Windows/Corpus behavior changed. |
| Compatibility | pass | Existing workflow syntax remains accepted, and `IsAlwaysCondition` also accepts the expression-wrapped `${{ always() }}` form. |
| Security/privacy | pass | No new secrets, artifact contents, cache keys, or summary data sources were introduced. |
| Derived artifact currency | pass | Review-resolution, change metadata, plan index, active plan, and explain-change were updated for PRCI-CR3. |
| Unrelated changes | pass | The diff is scoped to the PRCI-CR3 test gap and lifecycle records; `.github/workflows/release-evidence.yml` is unchanged. |
| Validation evidence | pass | Focused `CiWorkflowContract`, `CiRuntimeSummary`, and `git diff --check` reruns passed during implementation and review. |

## Validation Evidence Reviewed

- Implementation resolution evidence: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed with 10 tests.
- Implementation resolution evidence: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed with 6 tests.
- Implementation resolution evidence: `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- Reviewer rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed with 10 tests.
- Reviewer rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed with 6 tests.
- Reviewer rerun: `git diff --check` passed with no output.

## No-Finding Rationale

The specific under-proved failure mode from PRCI-CR3 now has direct committed-workflow assertions and an invalid fixture that produces actionable diagnostics. The resolution does not change release-evidence workflow behavior, command selection, triggers, artifacts, runtime summary helper behavior, `scripts/ci.ps1`, test categories, caching, or production code.

## Residual Risks

- The release branch and tag patterns remain the M3 selected policy (`release/**`, `v*`, `v*-rc*`). Future release naming changes still need the documented M5/handoff review path.

## Recommended Next Stage

Close M3 and proceed to `implement` M4. Do not start final closeout; M4, M5, final explain-change, verify, and PR handoff remain open.
