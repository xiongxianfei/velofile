# Code Review R9: M6 Runtime Reporting and Optimization Evidence

## Review status

clean-with-notes

## Review inputs

- Diff range: `4b5a065..5d30595`
- Review surface: tracked commit `5d30595 M6: Record test runtime optimization evidence`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/architecture/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`, especially R48-R60 and AC10
- Test spec: `specs/test-runtime-optimization.test.md`, especially TTO-T032 through TTO-T035
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M6
- Architecture / ADR: `docs/architecture/system/architecture.md`; `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M6 validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`
- Reviewer spot checks:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~RuntimeReportTests"` passed: 4 tests
  - `git diff --check` passed

## Diff summary

M6 adds reviewable runtime evidence and a contract test for that evidence:

- `RuntimeReportTests` now validates that the M6 report records baseline runtime, optimized timings, full CI status, top slow-test evidence, metadata/privacy wording, and target misses without deleting coverage.
- `m6-optimized-runtime.md` records the accepted `5 m 49 s` Corpus baseline, optimized fast/default and contract timings, script-smoke timing, release-evidence timing, full CI status, TRX-derived top 10 slow tests, target outcomes, and coverage preservation.
- The active plan, plan index, change metadata, and change rationale were updated to hand M6 to code review with validation evidence.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | The report records baseline runtime, optimized fast/contract runtime, script-smoke runtime, top 10 slow tests, full CI status, command/configuration/filter/date/environment assumptions, non-universal timing wording, and missed SHOULD targets with rationale as required by R48-R60. |
| Test coverage | pass | `RuntimeReportTests` covers TTO-T032 through TTO-T035 by checking the report sections, metadata/privacy wording, TRX slow-test source, exactly 10 slow-test rows, target outcome IDs, and coverage preservation. |
| Edge cases | pass | The spec edge cases for unavailable structured output and full CI limitation are not triggered because the report records TRX as the source and full CI as passed. Missed SHOULD targets are explicitly recorded without weakening coverage. |
| Error handling | pass | No runtime code path is changed. The evidence handles target misses by recording measured results and rationale rather than treating misses as failures hidden from reviewers. |
| Architecture boundaries | pass | The diff is limited to test-runtime evidence, tests, and workflow bookkeeping. It does not change public scripts, prepared-tool behavior, CI routing, or production App/Core/Windows behavior. |
| Compatibility | pass | Existing full `scripts\ci.ps1` closeout and explicit `ReleaseEvidence` command remain available and are recorded as passing. |
| Security/privacy | pass | The committed report uses repository-relative commands, states that raw TRX is not committed because it may contain machine-local absolute paths, and the contract test rejects private profile path and token/secret markers. |
| Derived artifact currency | pass | `change.yaml`, `explain-change.md`, `docs/plan.md`, and the active plan all reference the M6 runtime evidence and current handoff state. |
| Unrelated changes | pass | The commit only touches M6 runtime report tests, M6 evidence, and test-runtime lifecycle bookkeeping. |
| Validation evidence | pass | The active plan and change record list the required M6 commands, including build-producing and `--no-build` fast/contract runs, contract run, script-smoke run, release-evidence run, full CI, TRX slow-test source run, and `git diff --check`. Reviewer spot checks passed. |

## No-finding rationale

No blocking findings were found because the diff matches the approved M6 scope, the report includes every required runtime evidence class, the new contract test directly guards the report shape and privacy/overclaiming constraints, and target misses are recorded without deleting release, wrapper, prepared-tool, or full-CI coverage.

## Residual risks

- The fast/contract and full-CI timing targets remain missed or not directly comparable on this machine. This is recorded in the report and should guide future optimization work, but it does not block M6 because R60 requires recorded evidence and coverage preservation rather than deletion of slow tests.

## Recommended next stage

Close M6 and proceed to M7 lifecycle closeout planning/implementation. Do not start final verification until M7 closes.
