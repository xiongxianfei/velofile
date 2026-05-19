# PR CI Post-Merge Handoff Execution Plan

## Status

active

This plan was approved by `plan-review-r2`. M1 has recorded current branch-protection evidence and closed with `code-review-r1` as clean-with-notes. M2 removed the temporary broad `ci` default workflow path and closed with `code-review-r2` as clean-with-notes. M3 is blocked until hosted PR evidence exists for the M2 workflow change.

## Purpose / Big Picture

PR #4 merged the hosted CI tiering work and proved `ci-fast-required` can pass in a hosted PR cycle while the broad `ci` job remains available as a shadow/rollback path. The remaining rollout work is external-policy handoff plus cleanup: make the ordinary PR gate point at `ci-fast-required`, record that handoff without overclaiming, then stop running the broad closeout job on ordinary pull requests.

This plan keeps the final rollout small. It does not redesign CI lanes, test categories, or release-evidence policy. It sequences the maintainer-operated branch-protection check before changing the ordinary PR workflow shape.

## Source Artifacts

| Artifact | Path | Status |
|---|---|---|
| Proposal | [2026-05-18-pr-ci-validation-tiering.md](../proposals/2026-05-18-pr-ci-validation-tiering.md) | accepted |
| Spec | [pr-ci-validation-tiering.md](../../specs/pr-ci-validation-tiering.md) | approved |
| Test spec | [pr-ci-validation-tiering.test.md](../../specs/pr-ci-validation-tiering.test.md) | approved |
| Architecture | [system/architecture.md](../architecture/system/architecture.md) | current project architecture |
| ADR | [0012-hosted-pr-ci-validation-tiers.md](../adr/0012-hosted-pr-ci-validation-tiers.md) | accepted |
| Completed implementation plan | [2026-05-18-pr-ci-validation-tiering.md](2026-05-18-pr-ci-validation-tiering.md) | done; PR #4 merged |
| Current project map | [project-map.md](../project-map.md) | records post-handoff fast default CI and explicit broad closeout paths |

No new spec or architecture decision is expected for this plan. The work implements the post-merge handoff already required by the approved PR CI tiering spec.

## Context and Orientation

Current merged workflow state:

- `.github/workflows/ci.yml` runs `ci-fast-required` on `pull_request` and `push` to `main`.
- The same workflow still runs the broad `ci` job on `pull_request` and `push` to `main`; that job invokes `scripts/ci.ps1`.
- `.github/workflows/release-evidence.yml` preserves explicit release-evidence validation through manual, scheduled, release branch/tag, and merge-queue triggers.
- `.github/workflows/closeout.yml` preserves manual full closeout validation through `scripts/ci.ps1`.
- `scripts/Write-CiRuntimeSummary.ps1` and workflow contract tests already cover hosted lane summaries, TRX artifacts, failure context, and fast-lane project durations.

Current external policy state:

- `gh api repos/xiongxianfei/velofile/rulesets/16578519 --jq '{id, name, target, enforcement, conditions, rules}'` reports active repository ruleset `protect` on the default branch with `ci-fast-required` as a required status check.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` still returns `Branch not protected` with HTTP 404, so the observed required-check handoff is ruleset-based rather than classic branch protection.
- The spec treats branch-protection settings as maintainer-operated external configuration, not something workflow tests should mutate or assume.

Key files likely touched by this plan:

- `.github/workflows/ci.yml`
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`
- `docs/changes/2026-05-19-pr-ci-post-merge-handoff/`
- `docs/project-map.md`
- `CONTRIBUTING.md` and `README.md`, only if contributor-facing CI guidance changes
- this plan and `docs/plan.md`

## Non-goals

- Do not change fast-lane command selection.
- Do not change test categories, release-evidence filters, or public corpus wrapper smoke policy.
- Do not remove `scripts/ci.ps1`.
- Do not remove the manual full closeout workflow.
- Do not make release evidence required for every ordinary PR.
- Do not automate branch-protection mutation from tests.
- Do not claim branch protection changed unless fresh maintainer evidence proves it.
- Do not treat the fast PR lane as release readiness.

## Requirements Covered

| Requirements | Plan coverage |
|---|---|
| R1-R3 | M2 removes broad closeout cost from ordinary PRs after handoff while keeping `scripts/ci.ps1` available through closeout. |
| R11-R13 | M1 records maintainer-operated required-check state; M2 keeps ordinary PR CI centered on `ci-fast-required`. |
| R49-R51 | M1 confirms the shadow run and required-check handoff before M2 changes the broad PR shadow behavior. |
| R52 | M2 and M3 keep release readiness tied to release-evidence, full closeout, or accepted release gates. |
| R53 | M2 records rollback to broad closeout-required behavior if the fast required check fails in production use. |
| AC13 | M1 and M3 record branch-protection state without overclaiming external settings. |
| PRCI-T027 | M1 preserves the existing hosted shadow-run evidence and records the post-merge handoff decision. |
| PRCI-T028 | M1 updates handoff evidence so tests/docs distinguish intended required checks from actual GitHub branch protection. |
| PRCI-T029 / PRCI-M003 | M2-M3 preserve release readiness and rollback documentation after ordinary PR cleanup. |

## Milestones

### M1. Branch-Protection Handoff Evidence

- Milestone state: closed
- Goal: record the post-merge required-check state and maintainer handoff decision before workflow cleanup.
- Requirements: R13, R49-R51, AC13, PRCI-T027, PRCI-T028
- Files/components likely touched:
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/change.yaml`
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/branch-protection-handoff.md`
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/shadow-run.md`, if the accepted evidence is copied forward instead of linked
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs`, if existing evidence tests need a post-merge handoff fixture
  - this plan and `docs/plan.md`
- Dependencies:
  - PR #4 remains merged on `main`.
  - Maintainers either configure branch protection or explicitly record that branch protection remains absent.
  - Fresh GitHub API evidence is available.
- Tests to add/update:
  - evidence test proves the handoff artifact names intended required check `ci-fast-required`
  - evidence test proves the artifact does not claim branch protection changed when GitHub reports 404
  - evidence test proves rollback path still names broad closeout validation
- Implementation steps:
  - capture current `main` branch-protection state with `gh api`
  - record exact required status checks if protection exists
  - record that `ci-fast-required` is the intended ordinary PR check
  - record whether broad `ci` remains only as shadow/rollback or is still externally required
  - stop before M2 if branch protection is missing or does not yet require `ci-fast-required`
- Validation commands:
  - `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRolloutEvidence"`
  - `git diff --check`
- Expected observable result:
  - branch-protection evidence is fresh and explicit
  - the plan either advances to M2 or records why M2 is blocked
- Commit message: `M1: Record PR CI post-merge handoff evidence`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated with branch-protection API result
  - milestone committed
- Risks:
  - branch protection may remain absent, which blocks workflow cleanup
  - required check names may differ from workflow job names
- Rollback/recovery:
  - leave broad `ci` unchanged and move this plan to Blocked until maintainers record the handoff

### M2. Remove Broad Closeout From Ordinary PRs

- Milestone state: closed
- Goal: stop running the broad `ci` closeout job for ordinary pull requests after M1 proves `ci-fast-required` is the intended required PR gate.
- Requirements: R1-R3, R11-R13, R49-R53, PRCI-T028, PRCI-T029
- Files/components likely touched:
  - `.github/workflows/ci.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`
  - `docs/project-map.md`
  - `CONTRIBUTING.md` and `README.md`, only if ordinary PR guidance still says broad `ci` shadows PRs
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/`
- Dependencies:
  - M1 closed with accepted branch-protection handoff evidence.
  - `ci-fast-required` is named as the ordinary PR check in the handoff record.
- Tests to add/update:
  - workflow contract test proves `ci-fast-required` still runs on ordinary PRs
  - workflow contract test proves broad `ci` no longer runs from `.github/workflows/ci.yml` on `pull_request` or `push` after handoff
  - workflow contract test proves broad closeout remains available through `.github/workflows/closeout.yml`
  - workflow contract test proves release evidence remains outside ordinary PR triggers
  - rollout evidence test proves docs no longer describe broad `ci` as an ordinary PR shadow after cleanup
- Implementation steps:
  - remove or fully disable the temporary broad `ci` job from `.github/workflows/ci.yml`
  - keep `ci-fast-required` unchanged
  - update workflow model/tests to fail if broad `ci` still runs on `pull_request` or `push`
  - update guidance and project map to remove stale shadow-rollout language
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"`
  - `git diff --check`
- Expected observable result:
  - ordinary PR workflow contract is fast-only by default
  - default push validation does not run the former broad `ci` shadow job
  - full closeout and release evidence remain explicit, runnable lanes
- Commit message: `M2: Remove broad PR CI shadow after handoff`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if hosted or maintainer evidence changes the accepted handoff behavior
  - validation notes updated
  - milestone committed
- Risks:
  - removing broad `ci` too early could leave maintainers without a fallback required check
  - tests might assert workflow implementation details instead of event semantics
- Rollback/recovery:
  - restore the broad `ci` job for pull requests or make broad closeout required again and leave `ci-fast-required` optional

### M3. Hosted Confirmation and Lifecycle Closeout

- Milestone state: planned
- Goal: prove the post-handoff workflow behavior in a hosted PR cycle and complete the normal rationale, verification, and PR handoff path.
- Requirements: R49-R53, AC13, PRCI-M003
- Files/components likely touched:
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/explain-change.md`
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/verify-report.md`
  - `docs/changes/2026-05-19-pr-ci-post-merge-handoff/shadow-run.md` or hosted confirmation evidence
  - this plan and `docs/plan.md`
- Dependencies:
  - M1 and M2 closed.
  - A hosted PR run exists for the M2 workflow change.
- Tests to add/update:
  - no new product tests expected
  - update evidence tests only if hosted confirmation artifact shape changes
- Implementation steps:
  - collect hosted PR run evidence for `ci-fast-required`
  - confirm broad `ci` does not run on ordinary PR according to the M2 contract
  - confirm broad `ci` does not run on `push` to `main` unless an upstream spec or ADR amendment explicitly accepts a default broad push lane
  - confirm release-evidence and closeout workflows remain manually or explicitly triggered
  - write `explain-change`
  - run final `verify`
  - prepare PR handoff only after validation and hosted evidence are recorded
- Validation commands:
  - `gh run view <run-id> --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"`
  - `git diff --check`
  - additional `verify`-stage commands selected by the verify skill
- Expected observable result:
  - hosted PR evidence shows the ordinary PR lane uses `ci-fast-required` without broad closeout cost
  - hosted evidence does not show a default broad `ci` job on PR or push
  - release-readiness and rollback caveats remain documented
- Commit message: `M3: Close PR CI post-merge handoff`
- Milestone closeout:
  - hosted evidence recorded
  - explain-change completed
  - verify completed
  - progress updated
  - plan index updated
  - milestone committed
- Risks:
  - hosted evidence can lag local commits
  - GitHub branch protection may change between M1 and M3
- Rollback/recovery:
  - if hosted PR evidence contradicts the expected behavior, return to M2 or reinstate the broad PR job before requesting review

## Validation Plan

Focused local validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"`
- `git diff --check`

External/manual validation:

- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
- `gh run view <run-id> --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt`

Escalation rule:

- If branch protection is still absent or does not name `ci-fast-required`, stop before M2 and record the blocker instead of changing workflow behavior.

## Risks and Recovery

- Risk: branch protection cannot be observed or remains absent. Recovery: leave broad `ci` unchanged, keep this plan Blocked, and record the missing external gate.
- Risk: broad `ci` is still required by maintainers under a hidden or renamed check. Recovery: do not remove the PR shadow job until the exact check names are reconciled.
- Risk: workflow cleanup accidentally changes fast-lane commands. Recovery: workflow contract tests must prove the fast-lane command sequence remains unchanged.
- Risk: release evidence appears to be removed. Recovery: keep release-evidence and closeout workflow contract tests in the M2 validation scope.
- Risk: hosted run behavior differs from static workflow parsing. Recovery: treat hosted evidence as authoritative for M3 and revise M2 if needed.

## Dependencies

- Maintainer-operated GitHub branch protection configuration.
- GitHub Actions availability for a hosted PR confirmation run.
- Existing approved PR CI tiering spec, test spec, and ADR.
- Existing workflow model and contract tests in `tests/VeloFile.Corpus.Tests/TestRuntime/`.

## Progress

- [x] PR #4 merged to `main`.
- [x] Local `main` synced to merge commit `37a17cb1ced1f8d213aad258d8b4514434454b3d`.
- [x] Current branch-protection preflight recorded: GitHub API returned HTTP 404, `Branch not protected`.
- [x] Plan review r1 completed with PRCI-PMHR1.
- [x] PRCI-PMHR1 plan revision completed.
- [x] Plan review rerun approved by `plan-review-r2`.
- [x] M1 branch-protection handoff evidence implemented.
- [x] M1 code-review completed by `code-review-r1` with clean-with-notes; no material findings.
- [x] Maintainer ruleset handoff recorded: active default-branch ruleset `protect` requires `ci-fast-required`.
- [x] M2 broad PR shadow cleanup implemented.
- [x] M2 code-review completed by `code-review-r2` with clean-with-notes; no material findings.
- [x] M3 hosted confirmation and lifecycle closeout reviewed clean-with-notes.

## Current Handoff Summary

- Current stage: explain-change
- Plan status: implementation milestones closed; final closeout ready to start
- Current milestone: M3 hosted confirmation and lifecycle closeout
- Current milestone state: closed
- Last reviewed stage: code-review-r3 reviewed M3 as clean-with-notes with no material findings
- Next stage: explain-change
- Implementation readiness: all in-scope implementation milestones are closed
- Final closeout readiness: ready to start with explain-change; verify and PR handoff not yet complete
- Remaining completion gates: explain-change, verify, PR handoff

## Decision Log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-19 | Treat post-merge branch protection as a separate follow-up plan. | PR #4 merged the workflow tiering, but the branch-protection API still reports `main` as unprotected. | Claim the PR #4 plan changed external required checks. |
| 2026-05-19 | Do not remove broad `ci` from ordinary PRs until handoff evidence exists. | The approved spec requires shadow evidence before required-check changes and forbids overclaiming maintainer-operated settings. | Remove broad PR CI immediately after merge. |
| 2026-05-19 | Remove broad `ci` from default CI after branch-protection handoff. | Broad closeout validation remains accessible via `ci-full-closeout` and local `scripts/ci.ps1`; a continuing push-to-main broad lane would need a spec or ADR amendment. | Keep broad `ci` on `push` to `main` as a default hosted lane. |
| 2026-05-19 | Keep release evidence and closeout workflows unchanged in this plan. | The remaining issue is rollout cleanup, not lane semantics. | Reopen command selection, categories, or release triggers. |
| 2026-05-19 | Treat the active repository ruleset as the external required-check handoff. | GitHub classic branch protection still returns HTTP 404, but ruleset `protect` applies to the default branch and requires `ci-fast-required`. | Wait for classic branch protection when the repository uses rulesets. |

## Surprises and Discoveries

- Post-merge `main` classic branch protection still returns HTTP 404, but active repository ruleset `protect` now requires `ci-fast-required` on the default branch. Handoff evidence must describe this as ruleset-based protection.
- The merged `ci.yml` still ran broad `ci` on ordinary PRs as the intended shadow/rollback state from PR #4 until M2 removed that default workflow path.
- First hosted PR #5 run after M2 failed in `ci-fast-required` because fast/contract Corpus tests still contained stale broad-`ci` preservation assertions against `.github/workflows/ci.yml`. The workflow behavior was correct; the stale tests needed to preserve broad closeout through `.github/workflows/closeout.yml`.

## Validation Notes

Planning preflight:

- `git fetch origin`, `git checkout main`, and `git pull --ff-only` synced local `main` to merge commit `37a17cb1ced1f8d213aad258d8b4514434454b3d`.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` returned `Branch not protected` with HTTP 404.
- `.github/workflows/ci.yml` inspection confirmed `ci-fast-required` and broad `ci` both still run under the top-level `pull_request` trigger.

Plan artifact validation:

- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- `rg -n "post-merge handoff|Branch-Protection Handoff|Remove Broad Closeout|HTTP 404|plan-review" docs\plan.md docs\plans\2026-05-19-pr-ci-post-merge-handoff.md docs\plans\2026-05-18-pr-ci-validation-tiering.md` confirmed the new draft plan, index row, stale-state settlement, and handoff blocker references.

Review-resolution notes:

- PRCI-PMHR1 was accepted. M2 now plans to remove or fully disable the temporary broad `ci` job from `.github/workflows/ci.yml` after branch-protection handoff, not keep it as a default `push` to `main` lane.
- Workflow contract expectations now fail if broad `ci` still runs on `pull_request` or `push` after handoff, while preserving `ci-full-closeout` and local `scripts/ci.ps1` as broad closeout paths.

M1 implementation:

- Added `Post_merge_handoff_records_current_branch_protection_blocker` to `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs`.
- The new test failed before implementation because `docs/changes/2026-05-19-pr-ci-post-merge-handoff/branch-protection-handoff.md` did not exist.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` returned `Branch not protected` (HTTP 404).
- `gh pr view 4 --json url,state,mergedAt,mergeCommit,baseRefName,headRefOid,title` confirmed PR #4 is merged into `main` at merge commit `37a17cb1ced1f8d213aad258d8b4514434454b3d`.
- `gh run view 26065439926 --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt` confirmed the final observed PR run passed `ci-fast-required` and broad `ci` on PR head `85fbb0bc5e6bee98c9055c4ad284579474f8a8b0`.
- Added `docs/changes/2026-05-19-pr-ci-post-merge-handoff/branch-protection-handoff.md` recording the 404 branch-protection result, intended required check, no maintainer handoff claim, and M2 blocker.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRolloutEvidence"` passed after evidence implementation with 4 tests.
- Final M1 rerun of `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRolloutEvidence"` passed with 4 tests.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

M1 code review:

- `code-review-r1` reviewed commit `e0b4ea8 M1: Record PR CI post-merge handoff evidence` against the approved proposal, spec, test spec, ADR, active plan, handoff artifact, focused rollout evidence test, and recorded validation.
- Review status: clean-with-notes.
- Material findings: none.
- Result at review time: M1 closed while M2 stayed blocked pending maintainer handoff evidence.

Maintainer ruleset handoff:

- `gh api repos/xiongxianfei/velofile/rulesets/16578519 --jq '{id, name, target, enforcement, conditions, rules}'` reported active default-branch ruleset `protect` with `required_status_checks` containing `ci-fast-required` and integration id `15368`.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` still returned `Branch not protected` (HTTP 404), so evidence records the required check as ruleset-based rather than classic branch protection.
- Updated `docs/changes/2026-05-19-pr-ci-post-merge-handoff/branch-protection-handoff.md` and `Post_merge_handoff_records_ruleset_required_check` to prove the handoff evidence names the active ruleset, required `ci-fast-required` check, classic-API distinction, and M2 unblocked state.

M2 implementation:

- Updated workflow contract tests first so `.github/workflows/ci.yml` fails if broad `ci` still exists, invokes `scripts/ci.ps1`, or reports full closeout as run in the default PR/push workflow.
- Updated rollout guidance tests first so README, CONTRIBUTING, and project map fail if they still describe broad `ci` as an ordinary PR shadow after handoff.
- Initial focused M2 validation failed as expected because `.github/workflows/ci.yml` still contained broad `ci` and docs still contained shadow-rollout wording.
- Removed the broad `ci` job from `.github/workflows/ci.yml`; `ci-fast-required` remains unchanged.
- Updated README, CONTRIBUTING, and project map to describe the ruleset-required fast PR check and preserve broad closeout through `ci-full-closeout` and `scripts/ci.ps1`.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` passed after implementation with 22 tests.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

M2 code review:

- `code-review-r2` reviewed commit `0bf7298 M2: Remove broad PR CI shadow after handoff` against the approved spec, test spec, ADR, active plan, workflow diff, test changes, handoff evidence, contributor guidance, and recorded validation.
- Independent validation rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` passed with 22 tests.
- `git diff --check HEAD~1..HEAD` passed.
- Review status: clean-with-notes.
- Material findings: none.
- Result: M2 closed. M3 remains blocked until hosted PR evidence exists for the M2 workflow change.

M3 implementation:

- Opened draft PR #5 (`https://github.com/xiongxianfei/velofile/pull/5`) from `pr-ci-post-merge-handoff` to trigger hosted confirmation for the M2 workflow cleanup.
- `gh pr checks 5 --watch --interval 10` observed `ci-fast-required` fail in run `26085553757` after 4m25s.
- `gh run view 26085553757 --log` showed `Test Corpus fast and contract` failed because `ReleaseEvidenceTierTests.Broad_closeout_ci_remains_unsplit_and_unfiltered` and `CiRuntimeSummaryTests.Broad_ci_workflow_writes_runtime_summary_after_repository_ci_step` still expected broad `scripts/ci.ps1` wiring in `.github/workflows/ci.yml`.
- Updated the stale tests to preserve broad closeout through `.github/workflows/closeout.yml` and `ci-full-closeout`, without changing fast-lane command selection or workflow failure semantics.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed with 97 tests after rebuilding the test assembly.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRuntimeSummary|FullyQualifiedName~ReleaseEvidenceTier"` passed with 33 tests.
- Pushed commit `b29fd249df61c370dcd069edde664a4c7281cec6`; hosted run `26086191007` passed `ci-fast-required` on PR #5 in 5m22s.
- `gh run view 26086191007 --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt` recorded one hosted job: `ci-fast-required`, conclusion `success`, event `pull_request`, head SHA `b29fd249df61c370dcd069edde664a4c7281cec6`.
- `gh pr checks 5` reported `ci-fast-required` passed in 5m22s.
- `gh pr view 5 --json url,state,isDraft,headRefOid,statusCheckRollup` reported draft PR #5 open at head `b29fd249df61c370dcd069edde664a4c7281cec6` with only `ci-fast-required` in the status-check rollup.
- Added `docs/changes/2026-05-19-pr-ci-post-merge-handoff/shadow-run.md` recording the accepted post-handoff hosted cycle, step durations, selected categories, broad-closeout non-execution on ordinary PR, ruleset-required check status, and the earlier failed run.
- Added `Post_merge_hosted_confirmation_records_fast_only_pr_cycle`; it failed before evidence creation because `shadow-run.md` was missing and passed after the artifact was added.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` passed with 23 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed with 98 tests.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

M3 code review:

- `code-review-r3` reviewed `6d9f276..HEAD`, including commits `b29fd24` and `c2f4abe`, against the approved spec, test spec, ADR, active plan, M3 evidence artifact, workflow contract tests, and hosted PR evidence.
- Independent hosted evidence check: `gh pr view 5 --json headRefOid,statusCheckRollup,url,state,isDraft` reported current draft PR #5 head `c2f4abe067612fec3dbadda4c5048e918d613c70` with only `ci-fast-required` in the status-check rollup and conclusion `SUCCESS`.
- Independent hosted run check: `gh run view 26086704434 --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt` reported current-head run success for `ci-fast-required` in 5m20s.
- Independent validation rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` passed with 23 tests.
- `git diff --check 6d9f276..HEAD` passed with no output.
- Review status: clean-with-notes.
- Material findings: none.
- Result: M3 closed. All implementation milestones are closed; next stage is `explain-change`.

## Outcome and Retrospective

Not started. Fill this section only after the post-merge handoff, workflow cleanup, hosted confirmation, and final lifecycle closeout complete.
