# Code Review R3: PR CI Validation Tiering M2

## Review Status

changes-requested

## Reviewed Milestone

M2. Fast PR Shadow Lane And Workflow Contract Tests

## Review Inputs

- Review surface: M2 implementation through commit `d274ab4` (`M2: add fast PR CI shadow lane`)
- Plan: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M2
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Prior review/resolution context: [code-review-r1](code-review-r1.md), [code-review-r2](code-review-r2.md), [review-resolution.md](../review-resolution.md)
- Implementation files:
  - `.github/workflows/ci.yml`
  - `scripts/Write-CiRuntimeSummary.ps1`
  - `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`
  - M2 lifecycle records and explanation notes

## Diff Summary

M2 adds a shadow `ci-fast-required` job to `.github/workflows/ci.yml` while preserving the broad `ci` job. The fast lane runs on `pull_request` and `push` to `main`, uses `windows-latest`, defaults run steps to `pwsh`, sets up .NET 10, runs `dotnet --info`, restore, build, production UI contract validation, direct Core/App/Windows tests, Corpus `Fast|Contract`, and Corpus `CorpusScript&Smoke`, then writes a summary and uploads TRX artifacts. The milestone also adds test-owned workflow YAML parsing and contract tests backed by a test-only `YamlDotNet` dependency.

## Material Findings

### PRCI-CR2: Fast-lane summary omits per-test-project duration even though TRX output is available

- Severity: major
- Location: `.github/workflows/ci.yml` lines 50-84; `scripts/Write-CiRuntimeSummary.ps1` lines 186-192; `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs` lines 75-89.
- Evidence: The spec requires each runtime summary to report per test project duration when structured test output or command timing data is available (R44), and the observability invariant says per test project duration and slowest tests come from structured output when available. The fast lane now writes TRX for each test project and passes discovered TRX files to the summary helper through `-TrxPath`, so structured output is available. However, the workflow never passes `-TestProjectDuration`, and the helper only renders per-project rows from `-TestProjectDuration`; with no values it emits `No per test project duration data available.` The workflow contract test only asserts `-TrxPath` and does not prove any per-project duration source.
- Required outcome: The fast-lane summary must report per-test-project duration when the lane produces structured test output or command timing data, and the workflow/helper tests must prove that hosted wiring rather than only helper behavior.
- Safe resolution path: Add a scoped per-test-project duration source for `ci-fast-required` without changing fast-lane command selection or failure semantics. Acceptable approaches include timing each test step and passing stable `Project=duration` values to `Write-CiRuntimeSummary.ps1`, or extending the helper to derive project durations from TRX/structured result metadata. Update workflow contract/helper coverage to fail if the fast-lane summary helper call has TRX output but no per-project duration source, then rerun the focused M2 validation.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | Fast-lane command selection, runner, shell, SDK setup, category filters, no closeout, and no ReleaseEvidence align with R1-R27 and R65-R69. R44 is not satisfied for `ci-fast-required` because structured TRX output is available but no per-test-project duration is reported. |
| Test coverage | concern | Workflow contract tests cover the fast-lane commands, product-test filter separation, Corpus category filters, TRX output, summary tier labels, artifact upload, and diagnostics. They do not cover the R44 hosted wiring gap for per-test-project duration. |
| Edge cases | concern | EC1, EC2, EC3, and invalid workflow diagnostics are covered. Runtime regression visibility is incomplete because the summary can show slow tests but not project-level runtime attribution. |
| Error handling | pass | The fast-lane summary step uses `if: always()`, required validation steps are normal failing steps, and failed-step context is passed when a required step outcome is not `success` or `skipped`. |
| Architecture boundaries | pass | Workflow parsing remains test-owned, the new YAML parser is test-scoped, and production App/Core/Windows/Corpus behavior is unchanged. |
| Compatibility | pass | The broad `ci` job remains during shadow rollout and `scripts/ci.ps1` is not narrowed or called from `ci-fast-required`. |
| Security/privacy | pass | The workflow uses `contents: read`, no new secrets, no cache keys, and TRX artifact upload only for generated test results. |
| Derived artifact currency | concern | Lifecycle records were updated for M2 implementation, but review state must now move to review-resolution for PRCI-CR2. |
| Unrelated changes | pass | The diff is scoped to fast-lane workflow infrastructure, workflow contract tests, a test-only parser dependency, and lifecycle records. |
| Validation evidence | concern | Focused M2 validation evidence is recorded and relevant, but it did not catch the per-test-project duration wiring gap. |

## Validation Evidence Reviewed

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` failed before workflow implementation because `ci-fast-required` was missing, then passed after implementation with 6 tests.
- `dotnet --info` passed with SDK 10.0.203 on Windows.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug --no-restore` passed with 0 warnings and 0 errors.
- Production UI contract validation passed.
- Direct Core, App, and Windows test project commands passed.
- Corpus `Fast|Contract` and `CorpusScript&Smoke` filtered commands passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed with 4 tests.

## Decision

M2 needs review-resolution before it can close. Do not proceed to M3 implementation until PRCI-CR2 is resolved and code-review is rerun.

## Immediate Next Stage

`review-resolution` for PRCI-CR2.
