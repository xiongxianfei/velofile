# Code Review R1: PR CI Validation Tiering M1

## Review Status

changes-requested

## Reviewed Milestone

M1. Runtime Summary Helper And Broad-CI Reporting Foundation

## Review Inputs

- Commit reviewed: `006ac19` (`M1: add CI runtime summary foundation`)
- Plan: `docs/plans/2026-05-18-pr-ci-validation-tiering.md`
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Implementation files:
  - `.github/workflows/ci.yml`
  - `scripts/Write-CiRuntimeSummary.ps1`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`

## Material Findings

### PRCI-CR1: Broad CI summary hook does not report the failed command when CI fails before TRX output exists

- Severity: material
- Location: `.github/workflows/ci.yml` lines 30-44; `scripts/Write-CiRuntimeSummary.ps1` lines 170-172; `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs` lines 51-76.
- Evidence: The approved spec says that if a test command fails before TRX output is produced, the job summary reports the failed command and missing structured output, and EC7 expects a build failure summary plus unavailable slow-test details. The helper supports `-FailedCommand` and the focused helper test proves that behavior only when the parameter is supplied. The hosted `ci` workflow summary step always invokes `Write-CiRuntimeSummary.ps1` without `-FailedCommand`, without a repository CI step id, and without passing the prior step outcome. If `./scripts/ci.ps1` fails before producing structured test output, the summary can report missing structured output but cannot identify the failed broad CI command.
- Required outcome: The broad CI summary hook must provide enough failure context for the summary to report the failed command or command outcome when `./scripts/ci.ps1` fails before TRX or equivalent structured output exists, while preserving the original CI failure semantics.
- Safe resolution path: Give the repository CI step a stable id, pass failure context to the summary helper only when that step fails, and add or update workflow contract coverage proving the summary hook wires the failed broad CI command into the helper. Keep `scripts/ci.ps1` command selection unchanged and rerun the focused M1 validation.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Spec alignment | concern | R40/R46/AC12 summary behavior is partially implemented, but the workflow does not pass failed-command context for the broad CI lane. |
| Test-spec alignment | concern | PRCI-T024 expects failed command or missing structured output coverage; the helper test covers the helper parameter but the workflow hook test does not prove hosted wiring. |
| Scope discipline | pass | Production App/Core/Windows/Corpus behavior is unchanged, and `scripts/ci.ps1` remains broad. |
| Security and privacy | pass | The helper redacts common secret/token/credential/password/certificate patterns and private Windows profile paths; the workflow does not add secrets. |
| Workflow behavior | concern | The summary step runs with `if: always()`, but it lacks prior-step failure context. |
| Maintainability | pass | The shared PowerShell helper is a reasonable reusable foundation for later lanes. |

## Validation Evidence Reviewed

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` failed before implementation because the helper and workflow hook were missing, then passed after implementation with 4 tests.
- `git diff --check -- scripts\Write-CiRuntimeSummary.ps1 .github\workflows\ci.yml tests\VeloFile.Corpus.Tests` passed with Git LF-to-CRLF working-copy warnings only.
- `Select-String -Path scripts\Write-CiRuntimeSummary.ps1,.github\workflows\ci.yml,tests\VeloFile.Corpus.Tests\TestRuntime\CiRuntimeSummaryTests.cs -Pattern '[ \t]+$'` produced no matches.

## Decision

M1 needs review-resolution before it can close. Do not proceed to M2 implementation until PRCI-CR1 is resolved and code-review is rerun.

## Immediate Next Stage

`review-resolution` for PRCI-CR1.
