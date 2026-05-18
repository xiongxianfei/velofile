# Review Resolution

## Status

closed by code-review-r2; ready for M2 implementation

## Findings

### PRCI-CR1: Broad CI summary hook does not report the failed command when CI fails before TRX output exists

- Source review: [code-review-r1](reviews/code-review-r1.md)
- Disposition: accepted
- Status: closed by code-review-r2
- Severity: material
- Required outcome: The broad CI summary hook must provide enough failure context for the runtime summary to report the failed command or command outcome when `./scripts/ci.ps1` fails before TRX or equivalent structured output exists, while preserving the original CI failure semantics.
- Safe resolution path:
  - Give the repository CI step a stable id.
  - Pass failure context to `scripts/Write-CiRuntimeSummary.ps1` only when the repository CI step fails.
  - Add or update workflow contract coverage proving the summary hook wires the failed broad CI command into the helper.
  - Keep `scripts/ci.ps1` command selection unchanged.
  - Rerun focused M1 validation and rerun code-review.
- Resolution:
  - Added stable `id: repository_ci` to the broad CI script step in `.github/workflows/ci.yml`.
  - Kept the broad CI command selection as `./scripts/ci.ps1` under `shell: pwsh`; no `continue-on-error` was added.
  - Kept the runtime summary step as `if: always()`.
  - Updated the runtime summary step to read `${{ steps.repository_ci.outcome }}` and pass `-FailedCommand "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1"` plus `-FailedOutcome` when the broad CI outcome is not `success`.
  - Added `-FailedOutcome` support to `scripts/Write-CiRuntimeSummary.ps1` so the summary can report the command outcome alongside the failed command.
  - Strengthened `CiRuntimeSummaryTests` to prove the helper reports failed command, command outcome, missing structured output, and unavailable slow-test details, and to prove the committed workflow wires the prior-step outcome into the summary helper.
- Validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` failed first after test updates because `-FailedOutcome` and workflow failure-context wiring were missing.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed after the resolution with 4 tests.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~WorkflowContract"` completed with no matching tests in this project.
- Closeout:
  - [code-review-r2](reviews/code-review-r2.md) approved M1 with no findings.
  - PRCI-CR1 is closed.
  - M1 is closed.

### PRCI-SR1: Hosted runner and PowerShell shell contract is missing

- Source review: [spec-review-r1](reviews/spec-review-r1.md)
- Status: resolved
- Severity: major
- Required outcome: Add a hosted execution-environment contract so the CI tiering spec requires Windows-hosted validation and PowerShell 7 for PowerShell/script steps, or explicitly records a reviewed exception with equivalent validation evidence.
- Safe resolution path:
  - Add requirements that `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.
  - Add requirements that PowerShell/script steps use `pwsh` unless a reviewed reason records another shell.
  - Add requirements that hosted lanes install or select the repository-approved .NET SDK before validation commands.
  - Add acceptance criteria for workflow contract tests covering runner OS, shell, and SDK setup.
  - Rerun `spec-review` after the amendment.
- Resolution:
  - Added `Hosted execution environment` requirements R65-R69 to `specs/pr-ci-validation-tiering.md`.
  - R65 requires all hosted lanes introduced or changed by the spec to run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.
  - R66 requires PowerShell and repository script steps to use `pwsh` unless a reviewed shell exception is recorded.
  - R67 requires each hosted lane to install or select the repository-approved .NET SDK before restore, build, test, UI contract, release-evidence, or closeout commands.
  - R68 and R69 define narrow reviewed exception evidence for non-`windows-latest` Windows runners and non-`pwsh` PowerShell/script steps.
  - Added examples E7 and E8 for a valid hosted lane environment and failing runner, shell, and SDK-order cases.
  - Added error/boundary behavior for non-Windows runners, missing `pwsh`, and validation before SDK setup.
  - Added compatibility/security notes that runner and shell exceptions are reviewed infrastructure constraints, not convenience, and Linux/macOS hosted validation remains out of scope until a later accepted cross-platform validation design exists.
  - Added AC17-AC20 for workflow contract validation of Windows runner usage, `pwsh`, SDK setup ordering, and actionable diagnostics.
- Validation:
  - `git diff --check`
  - Normative scan: `Select-String -Path .\specs\pr-ci-validation-tiering.md -Pattern 'MUST|SHOULD|MAY|must|should|may' | Where-Object { $_.Line -notmatch '^R[0-9]+\.' }`
  - Section scan for added examples, requirements, and acceptance criteria.
- Closeout:
  - PRCI-SR1 was resolved by the spec amendment.
  - [spec-review-r2](reviews/spec-review-r2.md) approved the amended spec with no findings.
  - The next lifecycle stage is `architecture`.
  - Plan, test-spec, workflow implementation, and branch-protection changes remain blocked until their downstream stages are completed.

### PRCI-PLR1: Fast-lane plan omits the required `dotnet --info` command from M2 detail

- Source review: [plan-review-r1](reviews/plan-review-r1.md)
- Disposition: accepted
- Status: resolved
- Severity: material
- Required outcome: Update the execution plan so M2 explicitly requires `ci-fast-required` to run `dotnet --info` and requires workflow contract or milestone validation proof for that command.
- Safe resolution path:
  - Amend M2 tests to add workflow contract validation for the `dotnet --info` step.
  - Amend M2 implementation steps so `ci-fast-required` runs `dotnet --info` before restore/build and validation commands.
  - Amend M2 validation commands or workflow-contract validation notes so the required command is directly proved.
  - Tighten the M3 release branch/tag pattern dependency so plan-review is not listed as the stage selecting implementation values.
  - Rerun `plan-review` after the plan amendment.
- Chosen action: Amend M2 so `ci-fast-required` explicitly runs `dotnet --info` before restore/build. Add workflow-contract test coverage and validation evidence proving the command is present in the fast-required lane and ordered before restore/build.
- Rationale: The approved spec requires `dotnet --info`. M2 claims fast-lane requirement coverage, so the plan must make the command observable and testable rather than implicit.
- Resolution:
  - Added an M2 workflow-contract test bullet requiring `ci-fast-required` to run `dotnet --info` before restore/build validation.
  - Updated the M2 implementation steps so the fast lane command order is `dotnet --info`, then restore/build, then validation/test commands.
  - Added M2 validation evidence requiring workflow-contract proof that `dotnet --info` appears in `ci-fast-required` before `dotnet restore` and `dotnet build`, plus direct `dotnet --info` command evidence.
  - Tightened M3 dependency wording so implementation must record selected release branch/tag patterns in change evidence before the workflow can be closed.
- Validation: [plan-review-r2](reviews/plan-review-r2.md) verified that M2 now covers `dotnet --info` in workflow-contract tests, implementation ordering, and validation evidence, and that M3 no longer leaves release branch/tag-pattern selection to plan-review.
- Closeout:
  - [plan-review-r2](reviews/plan-review-r2.md) approved the amended plan with no findings.
  - The next lifecycle stage is `test-spec`.
  - Workflow implementation remains blocked until the matching test spec is created and reviewed.
