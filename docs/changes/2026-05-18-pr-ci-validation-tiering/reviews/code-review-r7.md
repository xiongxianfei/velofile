# Code Review R7: PR CI Validation Tiering M4

## Review Status

clean-with-notes

## Reviewed Milestone

M4. Full Closeout Workflow

## Review Inputs

- Review surface: M4 implementation through commit `43d11b7` (`M4: add full closeout workflow`)
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M4
- Feature spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Implementation files:
  - `.github/workflows/closeout.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`
  - M4 lifecycle records and change notes

## Diff Summary

M4 adds `.github/workflows/closeout.yml` as the separate manual `ci-full-closeout` lane. The lane uses `workflow_dispatch`, `contents: read`, `windows-latest`, job-level `pwsh`, `actions/setup-dotnet@v4`, and invokes the unchanged `./scripts/ci.ps1` broad closeout script. It keeps closeout failure semantics intact by not using `continue-on-error`, runs the summary step with `if: always()`, passes `steps.full_closeout.outcome` as failed-command context when needed, and uploads TRX artifacts when structured output exists.

The workflow contract tests now load the closeout workflow, assert the manual trigger and hosted execution environment, prove `scripts/ci.ps1` remains broad and unfiltered, verify summary and artifact wiring, and include an invalid fixture for closeout `continue-on-error` and missing summary `always()` diagnostics. The test-owned workflow validator now has a `ValidateFullCloseoutLane` path for hosted environment, command selection, and failure-semantics checks.

## Findings

No blocking or required-change findings.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | The workflow satisfies R9-R10, R35-R39, R40-R48, R65-R69, AC10-AC12, and AC17-AC20: stable `ci-full-closeout`, manual dispatch, broad `scripts/ci.ps1`, failure-on-script-failure, summary hook, Windows runner, `pwsh`, and SDK setup before closeout. |
| Test coverage | pass | `CiWorkflowContractTests` cover closeout identity, no ordinary PR/push trigger, Windows/pwsh/SDK setup, broad script invocation, no duplicated fast/release filters, summary fields, artifact upload path, failure diagnostics, and broad `scripts/ci.ps1` contents. |
| Edge cases | pass | Invalid fixture coverage proves diagnostics when `full_closeout` is marked `continue-on-error` or the closeout summary omits `if: always()`. Missing structured output remains an honest runtime-summary limitation because `scripts/ci.ps1` is unchanged. |
| Error handling | pass | `./scripts/ci.ps1` remains a normal failing step; summary generation uses `always()` and reports `-FailedCommand`/`-FailedOutcome` from the step outcome without changing job failure semantics. |
| Architecture boundaries | pass | The implementation follows ADR 0012 separate-workflow topology and keeps closeout behavior in `scripts/ci.ps1` instead of duplicating command selection in YAML. |
| Compatibility | pass | `scripts/ci.ps1` remains locally runnable and broad; no production App/Core/Windows/Corpus behavior, test category taxonomy, fast-lane policy, or release-evidence policy changed. |
| Security/privacy | pass | The workflow uses scoped `contents: read`, does not reference secrets, and passes only stable lane/status/command metadata to the shared summary helper. |
| Derived artifact currency | pass | Plan index, active plan, change metadata, and change notes were updated for M4 implementation and review handoff. |
| Unrelated changes | pass | The diff is scoped to the full closeout workflow, workflow contract tests/model, and lifecycle records. |
| Validation evidence | pass | Implementation validation recorded the required combined workflow/documentation selector, local broad `scripts/ci.ps1`, and diff check. Reviewer reran `CiWorkflowContract` and `git diff --check`. |

## Validation Evidence Reviewed

- Implementation evidence: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` failed before workflow implementation because `.github/workflows/closeout.yml` was missing, then passed with 14 tests.
- Implementation evidence: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation"` passed with 16 tests.
- Implementation evidence: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed with Core 168, App 168, Windows 52, and Corpus 110 tests.
- Implementation evidence: `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- Reviewer rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed with 14 tests.
- Reviewer rerun: `git diff --check` passed with no output.

## No-Finding Rationale

The M4 diff adds exactly the approved manual closeout lane and the direct workflow-contract proof needed for that lane. It preserves `scripts/ci.ps1` unchanged as the broad closeout authority, keeps ordinary PR and release-evidence command selection untouched, and records the known limitation that closeout TRX details only appear when structured output exists. No material review finding is required before M5.

## Residual Risks

- The closeout summary can only report slow tests or per-project durations when structured output exists. This is acceptable for M4 because the spec requires honest limitation reporting and does not require changing `scripts/ci.ps1` to emit TRX in this slice.

## Recommended Next Stage

Close M4 and proceed to `implement` M5. Do not start final closeout; M5, final explain-change, verify, and PR handoff remain open.
