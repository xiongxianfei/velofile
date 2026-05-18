# Code Review R4: PR CI Validation Tiering M2

## Review Status

clean-with-notes

## Reviewed Milestone

M2. Fast PR Shadow Lane And Workflow Contract Tests

## Review Inputs

- Review surface: PRCI-CR2 resolution through commit `804bf46` (`M2: resolve fast lane duration summary review finding`)
- Prior review: [code-review-r3](code-review-r3.md)
- Review resolution: [review-resolution.md](../review-resolution.md)
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M2
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Implementation files:
  - `scripts/Write-CiRuntimeSummary.ps1`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - M2 lifecycle records and change notes

## Diff Summary

The PRCI-CR2 resolution extends `scripts/Write-CiRuntimeSummary.ps1` so runtime summaries derive per-test-project duration rows from TRX when explicit `-TestProjectDuration` values are absent. The helper keeps explicit duration inputs as the first source, then uses TRX test method `codeBase` metadata for the test assembly name and falls back to the TRX file stem when needed. The resolution adds a runtime-summary regression test proving TRX-derived project duration output and strengthens workflow contract coverage so the fast-lane summary has an accepted project-duration source. `ci-fast-required` command selection and normal failure semantics are unchanged.

## Findings

No blocking or required-change findings.

## PRCI-CR2 Re-Review

PRCI-CR2 is resolved.

- `Write-CiRuntimeSummary.ps1` now builds project-duration rows from `-TrxPath` when explicit `-TestProjectDuration` values are absent.
- `Runtime_summary_derives_test_project_durations_from_trx_when_explicit_durations_are_absent` proves the helper emits a `VeloFile.Core.Tests` duration row from TRX alone.
- `Fast_required_lane_reports_fast_confidence_summary_and_artifacts` still proves the fast lane passes `-TrxPath`, and now records the duration-source contract for the fast summary hook.
- `.github/workflows/ci.yml` was not changed by this resolution; fast-lane validation commands, filters, and failure behavior remain as reviewed in M2.
- `review-resolution.md` records the accepted disposition, resolution, and validation evidence for PRCI-CR2.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | R44 is now satisfied for the fast lane because the helper can report per-test-project duration from structured TRX output. R40-R48 summary behavior remains aligned, and no release-evidence or closeout policy changed. |
| Test coverage | pass | `CiRuntimeSummaryTests` prove explicit durations, missing output, redaction, broad-CI failure context, and TRX-derived project durations. `CiWorkflowContractTests` prove fast-lane summary wiring and artifact behavior. |
| Edge cases | pass | Missing TRX still reports limitations without fabricated slow-test rows, while present TRX can supply both slow tests and project durations. |
| Error handling | pass | Parse failures in the TRX duration fallback do not fail or fabricate duration rows; slow-test parsing remains the source of structured-output limitation reporting. |
| Architecture boundaries | pass | Runtime reporting remains in the shared PowerShell helper and workflow/test infrastructure; no production App/Core/Windows/Corpus behavior changed. |
| Compatibility | pass | Explicit `-TestProjectDuration` inputs still take precedence, and existing `-TrxPath` workflow usage remains compatible. |
| Security/privacy | pass | Project names are derived from assembly file names or TRX file stems and still pass through summary redaction before rendering. |
| Derived artifact currency | pass | Review-resolution, change metadata, plan index, active plan, and change notes were updated for PRCI-CR2 and M2 re-review handoff. |
| Unrelated changes | pass | The resolution is scoped to runtime summary helper behavior, focused tests, and lifecycle records. |
| Validation evidence | pass | Focused rerun evidence passed for `CiWorkflowContract` and `CiRuntimeSummary`; the initial parallel rerun hit a local build-output lock and passed when rerun sequentially. |

## Validation Evidence Reviewed

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` failed first during implementation after adding the regression test because TRX-derived project durations were missing, then passed after the helper update with 5 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed during implementation with 6 tests.
- During this review, parallel `CiRuntimeSummary` and `CiWorkflowContract` test reruns caused a local build-output file lock in the `CiRuntimeSummary` run while `CiWorkflowContract` passed with 6 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed when rerun sequentially with 5 tests.
- `git diff --check` passed after the PRCI-CR2 implementation with Git LF-to-CRLF working-copy warnings only.

## No-Finding Rationale

No required-change findings remain because the specific R44 gap from PRCI-CR2 now has direct helper output coverage, workflow contract coverage, and recorded validation. The resolution keeps the fast-lane command selection unchanged, preserves the broad `ci` job, and does not touch release-evidence policy, test categories, caching, or production behavior.

## Residual Risks

- The TRX fallback reports duration from structured test result durations rather than externally timed wall-clock command duration. This is acceptable for PRCI-CR2 because the spec allows structured output as the source and explicit command timing inputs still override it when provided by later hosted lanes.

## Recommended Next Stage

Close M2 and proceed to `implement` M3.
