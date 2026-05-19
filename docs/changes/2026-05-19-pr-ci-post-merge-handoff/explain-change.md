# PR CI Post-Merge Handoff Explain Change

## Status

explain-change complete; verify and PR handoff still pending

## Summary

This change completes the post-merge handoff after the hosted PR CI tiering work. It records that the repository ruleset now requires `ci-fast-required`, removes the temporary broad `ci` job from the default PR/push workflow, keeps broad closeout available through `ci-full-closeout` and local `scripts/ci.ps1`, and records hosted PR evidence that the post-handoff ordinary PR path runs only `ci-fast-required`.

The work is intentionally about rollout policy and evidence, not test selection redesign. Fast-lane commands, release-evidence commands, test categories, and production App/Core/Windows behavior are unchanged.

## Problem

PR #4 merged the CI tiering implementation with a deliberate shadow/rollback shape: ordinary PRs still had the broad `ci` job while maintainers reviewed shadow evidence and changed external required-check policy. After merge, the remaining problem was to complete the handoff safely:

- record maintainer-operated required-check state without overclaiming classic branch protection;
- remove the default broad `ci` PR/push job after `ci-fast-required` became the intended ordinary gate;
- preserve release readiness and broad closeout outside ordinary PR validation;
- prove the post-handoff shape in a hosted PR cycle.

## Decision Trail

| Source | Decision |
|---|---|
| Proposal | Choose hosted CI validation tiers, preserve release evidence, and make fast PR validation the ordinary required default only after reviewed rollout evidence. |
| Spec | R11-R13 require ordinary PRs not to run release/full-closeout lanes by default and require repository artifacts to avoid claiming external branch protection changed unless maintainers record it. R23-R24 keep `ci-fast-required` from calling `scripts/ci.ps1` or running `ReleaseEvidence` by default. R35-R36 preserve `scripts/ci.ps1` through `ci-full-closeout`. R49-R53 govern shadow evidence, handoff, release readiness, and rollback. R61 requires old broad-PR preservation tests to move to the new policy. |
| Test spec | PRCI-T027 and PRCI-M001 require shadow/hosted evidence. PRCI-T028 requires maintainer evidence for required-check claims. PRCI-T029 and PRCI-M003 preserve release-readiness and rollback wording. The test spec also says broad closeout claims should preserve `scripts/ci.ps1` rather than mocking it. |
| Architecture / ADR 0012 | Ordinary PR confidence belongs in `ci-fast-required`; `ci-full-closeout` invokes `scripts/ci.ps1`; branch-protection changes are external maintainer-operated state; workflow contract tests replace the old broad-PR preservation expectation. |
| Plan | M1 records branch-protection/ruleset handoff evidence, M2 removes broad closeout from ordinary PRs after handoff, and M3 records hosted confirmation and closes the implementation milestones. |
| Reviews | Plan-review finding PRCI-PMHR1 required removal/disablement of the temporary broad `ci` default job after handoff. Code reviews R1, R2, and R3 all completed clean-with-notes for M1-M3. |

## Diff Rationale By Area

| Area | Files | Why changed | Source/Test evidence |
|---|---|---|---|
| Default PR workflow | `.github/workflows/ci.yml` | Removed the broad `ci` job so ordinary PR/push validation no longer invokes `scripts/ci.ps1` by default after the ruleset handoff. `ci-fast-required` remains the default workflow lane. | R11-R12, R23-R24, R49-R53, PRCI-PMHR1; `Default_ci_workflow_no_longer_runs_broad_closeout_job`; hosted PR #5 checks. |
| Branch/ruleset evidence | `docs/changes/2026-05-19-pr-ci-post-merge-handoff/branch-protection-handoff.md` | Records the active repository ruleset `protect`, required status check `ci-fast-required`, and the classic branch-protection API 404 distinction. | R13, PRCI-T028; `Post_merge_handoff_records_ruleset_required_check`. |
| Hosted confirmation evidence | `docs/changes/2026-05-19-pr-ci-post-merge-handoff/shadow-run.md` | Records accepted post-handoff hosted PR #5 run `26086191007`, step durations, selected categories, no broad `ci` job, and the earlier failed stale-test attempt. | R49-R51, R50, PRCI-M001; `Post_merge_hosted_confirmation_records_fast_only_pr_cycle`. |
| Workflow contract tests | `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs` | Adds a regression guard that fails if broad `ci`, `scripts/ci.ps1`, or `FullCloseoutStatus = "run"` reappears in `.github/workflows/ci.yml`. | R11-R12, R23, R61; focused workflow contract validation. |
| Rollout evidence tests | `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs` | Adds tests for the ruleset handoff and the post-handoff hosted confirmation artifact; extends guidance checks to include `docs/project-map.md` and reject stale broad-shadow wording. | PRCI-T027, PRCI-T028, PRCI-T029, PRCI-M001, PRCI-M003. |
| Runtime summary / closeout tests | `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`, `ReleaseEvidenceTierTests.cs` | Redirects broad closeout preservation checks from removed `.github/workflows/ci.yml` wiring to `.github/workflows/closeout.yml` and `ci-full-closeout`. | R35-R36, R39-R40, R61; hosted run `26085553757` exposed the stale assertion. |
| Contributor guidance | `README.md`, `CONTRIBUTING.md`, `docs/project-map.md` | Updates guidance so contributors see `ci-fast-required` as the ordinary PR check while release readiness remains tied to release evidence, closeout, or an accepted release gate. Removes stale broad-shadow language. | R52-R53, PRCI-T029, PRCI-M003; `Rollout_guidance_keeps_release_readiness_and_rollback_explicit`. |
| Plan and lifecycle records | `docs/plan.md`, `docs/plans/2026-05-18-pr-ci-validation-tiering.md`, `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`, `change.yaml`, review records, `review-log.md`, `review-resolution.md` | Creates the post-merge handoff plan, records PRCI-PMHR1 resolution, milestone evidence, review outcomes, and current handoff state. | Repository workflow requirements; code-review-r1/r2/r3. |

## Tests Added Or Changed

- `Default_ci_workflow_no_longer_runs_broad_closeout_job`: proves `.github/workflows/ci.yml` no longer contains a broad `ci` job, `scripts/ci.ps1`, or full-closeout status after handoff.
- `Post_merge_handoff_records_ruleset_required_check`: proves the handoff artifact records the active ruleset, `ci-fast-required`, classic branch-protection 404, and broad-closeout preservation.
- `Post_merge_hosted_confirmation_records_fast_only_pr_cycle`: proves hosted PR #5 evidence records a passed fast-only cycle, selected categories, no broad closeout, and no classic branch-protection overclaim.
- `Rollout_guidance_keeps_release_readiness_and_rollback_explicit`: now covers `docs/project-map.md` and rejects stale wording that would imply broad `ci` still shadows ordinary PRs after handoff.
- `Full_closeout_workflow_writes_runtime_summary_after_closeout_step`: now checks the manual closeout workflow, `full_closeout` step, failure context, and summary behavior instead of the removed broad default workflow job.
- `Broad_closeout_ci_remains_unsplit_and_unfiltered`: now preserves broad closeout through `closeout.yml` and verifies `scripts/ci.ps1` remains unfiltered.

These are static/evidence contract tests because the change is about committed workflow shape, hosted evidence, and documentation promises. They are the right level for preventing drift without changing production behavior.

## Validation Evidence Available Before Final Verify

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRolloutEvidence"` passed with 4 tests after M1 evidence implementation.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` passed with 22 tests after M2 and 23 tests after M3.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRuntimeSummary|FullyQualifiedName~ReleaseEvidenceTier"` passed with 33 tests during M3 stale-test correction.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed with 98 tests after adding M3 hosted evidence coverage.
- `gh api repos/xiongxianfei/velofile/rulesets/16578519 --jq '{id, name, target, enforcement, conditions, rules}'` reported active default-branch ruleset `protect` with `ci-fast-required` as a required status check.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` returned `Branch not protected` with HTTP 404, so artifacts correctly describe the handoff as ruleset-based rather than classic branch protection.
- Hosted PR #5 run `26086191007` passed `ci-fast-required` in 5m22s at commit `b29fd249df61c370dcd069edde664a4c7281cec6`; the accepted evidence artifact records this run.
- Hosted PR #5 current-head run `26087170115` passed `ci-fast-required` in 6m32s at commit `cc1b100cf90402fd3bb68634500ec93b5af81a50` after the M3 review receipt was pushed.
- `git diff --check` and `git diff --check 6d9f276..HEAD` passed; the latter produced no output during code-review-r3.

This is pre-verify evidence. It does not claim final `verify`, PR readiness, or release readiness.

## Review Resolution Summary

- Plan-review finding PRCI-PMHR1 was accepted and closed by `plan-review-r2`. The resolution changed M2 from keeping broad `ci` as a push-to-main default to removing or disabling broad `ci` from default CI after handoff while preserving `ci-full-closeout` and `scripts/ci.ps1`.
- Code-review-r1, code-review-r2, and code-review-r3 all recorded `clean-with-notes` with no material findings.
- No material code-review finding is open. No `review-resolution` loop is required before `verify`.

## Alternatives Rejected

- Keep broad `ci` on `push` to `main` after handoff: rejected by PRCI-PMHR1 because the approved policy keeps broad closeout explicit, not a default hosted lane.
- Remove or narrow `scripts/ci.ps1`: rejected by the spec and ADR because it remains the broad closeout command.
- Treat `ci-fast-required` success as release readiness: rejected because release readiness still requires release evidence, full closeout, local `scripts/ci.ps1`, or another accepted release gate.
- Claim classic GitHub branch protection is configured: rejected because the classic branch-protection API still returns HTTP 404; the observed required-check handoff is repository-ruleset based.
- Change fast-lane command selection, release-evidence triggers, or test categories during this handoff: rejected as outside this post-merge rollout scope.

## Scope Control

The change is limited to CI workflow cleanup, tests that guard workflow/evidence contracts, contributor guidance, and lifecycle artifacts. It does not change production App/Core/Windows/Corpus behavior, public prepared-tool script options, release-evidence taxonomy, manual closeout semantics, or runner/shell/SDK contracts.

## Risks And Follow-Ups

- Hosted evidence is a snapshot, not a timing guarantee. Current observed `ci-fast-required` runs were 5m22s and 6m32s, but hosted runner performance can vary.
- GitHub Actions emitted warnings about Node.js 20 action deprecation and upcoming `windows-latest` redirection. Those warnings are outside this handoff scope and should be tracked separately if they become actionable.
- The PR remains a draft implementation vehicle until `verify` and `pr` complete.
- Release readiness remains explicit and separate from `ci-fast-required`.

## Current Handoff

All implementation milestones are closed and code-review-r3 found no material issues. The next lifecycle stage is `verify`; final verification, PR readiness, and release readiness have not been claimed by this artifact.
