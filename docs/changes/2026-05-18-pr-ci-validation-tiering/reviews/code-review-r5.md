# Code Review R5: PR CI Validation Tiering M3

## Review Status

changes-requested

## Reviewed Milestone

M3. Release-Evidence Workflow

## Review Inputs

- Review surface: commit `3886bf9` (`M3: add release evidence workflow`)
- Tracked governing branch state: committed on `main`; working tree was clean before review recording
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M3
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Implementation files:
  - `.github/workflows/release-evidence.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`
  - M3 lifecycle records and change notes

## Diff Summary

M3 adds `.github/workflows/release-evidence.yml` with a `ci-release-evidence` job on `windows-latest`, job-level `pwsh`, `actions/setup-dotnet@v4`, `dotnet --info`, restore, build, explicit Corpus `TestCategory=ReleaseEvidence` validation, runtime summary generation, and TRX artifact upload. The workflow is runnable by `workflow_dispatch`, a non-top-of-hour nightly schedule, release branch/tag push patterns, and `merge_group`, and it does not add a `pull_request` trigger.

The test changes extend the workflow model to parse push tags and scheduled cron entries, add reusable hosted-lane validation, add release-evidence workflow contract tests, and add runtime-summary coverage for release-evidence category/status output. Lifecycle records were updated for M3 implementation evidence.

## Findings

### PRCI-CR3: Release-evidence contract tests do not prove failure and summary-after-failure semantics

- Severity: major
- Location:
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs:148`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs:30`
  - `.github/workflows/release-evidence.yml:52`
  - `.github/workflows/release-evidence.yml:56`
- Evidence:
  - The spec requires `ci-release-evidence` to fail when a release-evidence command fails (`specs/pr-ci-validation-tiering.md:198`) and requires hosted lane summaries to run after validation has started even when validation fails (`specs/pr-ci-validation-tiering.md:220`).
  - The test spec maps those to PRCI-T020 and PRCI-T023. PRCI-T020 expects release-evidence command failures to fail the job (`specs/pr-ci-validation-tiering.test.md:293`), and PRCI-T023 expects workflow inspection for `if: always()` or equivalent summary behavior (`specs/pr-ci-validation-tiering.test.md:322`).
  - The committed workflow currently satisfies the intended shape: the release-evidence test step has no `continue-on-error`, and the summary step uses `if: always()`. However, the M3 workflow contract test only checks command selection, TRX output, summary arguments, `steps.test_release_evidence.outcome`, and artifact upload. It does not assert that the release-evidence test step cannot use `continue-on-error`, and the workflow model does not parse step `if`, so the test cannot fail if the summary step stops using `always()`.
  - `CiWorkflowStep` already records `ContinueOnError`, but the release-evidence test never asserts it for the validation step.
- Required outcome: Workflow contract coverage must fail if the `ci-release-evidence` release-evidence validation step uses `continue-on-error`, or if the release-evidence summary helper step is not configured to run with `if: always()` or an explicitly accepted equivalent.
- Safe resolution path:
  - Extend the test-owned workflow model to expose step `if` values.
  - Update the release-evidence workflow contract test to locate the `test_release_evidence` step and assert `ContinueOnError` is false.
  - Update the release-evidence workflow contract test to locate the runtime summary step and assert the effective condition is `always()`.
  - Keep release-evidence command selection, trigger policy, and workflow failure semantics unchanged.
  - Rerun the focused M3 validation: `CiWorkflowContract`, `CiRuntimeSummary`, and `git diff --check`.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | The workflow shape aligns with R4-R8, R11, R28-R34, R40-R48, and R65-R69, but PRCI-CR3 leaves R32/R40 under-proved by contract tests. |
| Test coverage | concern | Release-evidence trigger, environment, command-order, category-status, TRX, and inventory coverage exists, but failure semantics and summary `if: always()` coverage are missing. |
| Edge cases | concern | A future `continue-on-error: true` or missing summary `always()` condition could hide release-evidence failures while current tests still pass. |
| Error handling | concern | Failure-context summary wiring references `steps.test_release_evidence.outcome`, but the test does not prove the validation step still fails the job. |
| Architecture boundaries | pass | The workflow is separate from ordinary PR CI and does not change production App/Core/Windows/Corpus behavior or `scripts/ci.ps1`. |
| Compatibility | pass | The new workflow uses the accepted Windows runner, `pwsh`, SDK setup, explicit release-evidence command, and TRX artifact path. |
| Security/privacy | pass | The release-evidence workflow uses `contents: read`, no secrets, no cache keys, and summary values flow through the existing redaction helper. |
| Derived artifact currency | pass | Change metadata, plan index, active plan, and explain-change were updated for M3 implementation evidence. |
| Unrelated changes | pass | The diff is scoped to release-evidence workflow infrastructure, workflow/runtime tests, parser support, and lifecycle records. |
| Validation evidence | pass | Implementation validation and reviewer reruns passed for the focused M3 workflow/runtime contract suites, but passing tests do not close the missing PRCI-T020/PRCI-T023 assertions. |

## Validation Evidence Reviewed

- Implementation evidence records `CiWorkflowContract` failing before `.github/workflows/release-evidence.yml` existed, then passing with 9 tests after implementation.
- Implementation evidence records `CiRuntimeSummary` passing with 6 tests after release-evidence summary coverage.
- Implementation evidence records `dotnet restore VeloFile.sln` passing.
- Implementation evidence records `dotnet build VeloFile.sln -c Debug --no-restore` passing with 0 warnings and 0 errors.
- Implementation evidence records `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=ReleaseEvidence"` passing with 10 tests in 5 m 12 s.
- Implementation evidence records `git diff --check` passing with Git LF-to-CRLF working-copy warnings only.
- Reviewer rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed with 9 tests.
- Reviewer rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed with 6 tests.
- Review artifact check: `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

## Recommended Next Stage

Enter review-resolution for PRCI-CR3. Do not proceed to M4 implementation until the release-evidence workflow contract tests prove the failure and summary-after-failure semantics and code-review is rerun.
