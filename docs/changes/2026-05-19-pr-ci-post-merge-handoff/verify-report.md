# PR CI Post-Merge Handoff Verify Report

## Verdict

branch-ready for PR handoff, with a local validation concern recorded

This verifies the post-merge PR CI handoff change pack after M1-M3 implementation, code review, plan-review resolution, and explain-change. It does not claim PR body readiness or release readiness.

## Scope

- Change ID: `2026-05-19-pr-ci-post-merge-handoff`
- Branch: `pr-ci-post-merge-handoff`
- Verified implementation HEAD before verify-report recording: `00f006a17c736439595dcc0b99c6953fe0fe6aac`
- Draft PR: https://github.com/xiongxianfei/velofile/pull/5
- Plan: `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`
- Spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Explain-change: `docs/changes/2026-05-19-pr-ci-post-merge-handoff/explain-change.md`
- Final code review: `docs/changes/2026-05-19-pr-ci-post-merge-handoff/reviews/code-review-r3.md`

## Traceability

| Requirement area | Test IDs / proof | Files changed | Verification evidence | Status |
|---|---|---|---|---|
| Ordinary PRs do not run release evidence or full closeout by default | R11-R12, R23-R24, AC14, `Default_ci_workflow_no_longer_runs_broad_closeout_job` | `.github/workflows/ci.yml`, workflow contract tests | Focused workflow tests passed; hosted PR #5 check rollup shows only `ci-fast-required` | pass |
| Required-check handoff is maintainer-recorded and not overclaimed | R13, AC13, PRCI-T028, `Post_merge_handoff_records_ruleset_required_check` | `branch-protection-handoff.md`, rollout tests, plan/change metadata | Ruleset API reports active `ci-fast-required`; classic branch-protection API returns HTTP 404 and is documented | pass |
| Full closeout remains broad and explicit | R35-R36, R39-R40, R52-R53, PRCI-T029, PRCI-M003 | `.github/workflows/closeout.yml`, runtime-summary/closeout tests, guidance docs | Contract tests passed; `scripts/ci.ps1` remains unfiltered and closeout workflow still invokes it | pass |
| Hosted post-handoff confirmation exists | R49-R51, R50, PRCI-M001, `Post_merge_hosted_confirmation_records_fast_only_pr_cycle` | `shadow-run.md`, rollout tests, change metadata | Hosted PR #5 accepted run `26086191007` and current-head run `26087711964` passed `ci-fast-required` | pass |
| Release readiness and rollback remain explicit | R52-R53, PRCI-T029, PRCI-M003 | README, CONTRIBUTING, project map, explain-change | Guidance scan found required lane, release-readiness, and rollback wording; stale broad-shadow wording scan found no matches | pass |
| Lifecycle and review closeout | review log, review-resolution, explain-change | `change.yaml`, `docs/plan.md`, active plan, review records, this report | M1-M3 code reviews are clean-with-notes; PRCI-PMHR1 is closed; explain-change exists and is current for pre-verify handoff | pass |

## Dimension Assessment

| Dimension | Result | Evidence |
|---|---|---|
| Spec coverage | pass | The diff maps to R11-R13, R23-R24, R35-R36, R39-R40, R49-R53, R61, and AC13-AC16. |
| Requirement satisfaction | pass | Ordinary PR default workflow is fast-only, broad closeout remains explicit, ruleset handoff is recorded, and release readiness is not overclaimed. |
| Test coverage | pass | Workflow, rollout-evidence, runtime-summary, closeout preservation, and contributor-guidance tests cover the changed surfaces. |
| Test validity | pass | Tests inspect committed workflow/evidence/guidance contents and fail on stale broad-PR/default-closeout behavior. |
| Architecture coherence | pass | The change follows ADR 0012: `ci-fast-required` for ordinary PR confidence, `ci-full-closeout` for broad closeout, and external ruleset evidence for required-check handoff. |
| Artifact lifecycle state | pass | `docs/plan.md`, the active plan, `change.yaml`, review log, review-resolution, explain-change, and this report agree on verify completion and PR handoff as the next stage. |
| Plan completion | pass | M1, M2, and M3 are closed; final explain-change is complete; verify is now complete. |
| Validation evidence | pass with concern | Focused local validation and hosted CI passed. Local full Corpus `Fast|Contract` timed out in a pre-existing prepared-tool harness test; hosted current-head `ci-fast-required` passed that lane. |
| Drift detection | pass | Stale plan-index and handoff-artifact wording found during verify was corrected; the follow-up stale-wording scan found no matches. |
| Risk closure | pass | Rollback, release-readiness separation, branch-protection/ruleset distinction, and hosted evidence limitations are documented. |
| Release readiness | pass with scope note | Branch is ready for PR handoff. Release readiness still requires `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or another accepted release gate. |

## Validation Commands

All commands ran from the repository root unless noted.

| Command | Result | Important output |
|---|---|---|
| `dotnet --info` | pass | SDK `10.0.203`, Windows `10.0.26200`, x64. |
| `dotnet restore VeloFile.sln` | pass | All projects up to date. |
| `dotnet build VeloFile.sln -c Debug --no-restore` | pass | Build succeeded with 0 warnings and 0 errors. |
| `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` | pass | UI contract validation passed. |
| `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --no-build` | pass | 168 tests passed. |
| `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --no-build` | pass | 168 tests passed. |
| `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --no-build` | pass | 52 tests passed. |
| `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRuntimeSummary|FullyQualifiedName~ReleaseEvidenceTier"` | pass | 34 tests passed after lifecycle drift corrections. |
| `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "(TestCategory=Fast|TestCategory=Contract)&FullyQualifiedName!~PreparedToolHarnessTests"` | pass | 87 tests passed. |
| `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` | concern | Timed out locally twice. A diagnostic run with `--blame-hang --blame-hang-timeout 2m` showed `PreparedTool_current_run_executes_minimal_command` was incomplete after 72 tests had passed. This test is outside the post-merge handoff diff; hosted current-head `ci-fast-required` passed the full fast/contract lane. |
| `rg -n "M3 is blocked|M3 blocked|missing hosted PR evidence|collect hosted evidence|same workflow still runs the broad|Current merged workflow state|broad \`ci\` job remains unchanged|verify completed" docs\plan.md docs\plans\2026-05-19-pr-ci-post-merge-handoff.md docs\changes\2026-05-19-pr-ci-post-merge-handoff README.md CONTRIBUTING.md docs\project-map.md tests\VeloFile.Corpus.Tests\TestRuntime\CiRolloutEvidenceTests.cs` | pass | No stale lifecycle matches. |
| `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout|broad closeout|rollback" README.md CONTRIBUTING.md docs\project-map.md docs\changes\2026-05-19-pr-ci-post-merge-handoff docs\plans\2026-05-19-pr-ci-post-merge-handoff.md .github` | pass | Required lane, release-readiness, and rollback wording found. |
| `rg -n 'ci:|scripts/ci\.ps1|FullCloseoutStatus = "run"|ci-fast-required|pull_request|push:' .github\workflows\ci.yml .github\workflows\closeout.yml` | pass | Default CI contains `ci-fast-required`; broad closeout references appear only in `closeout.yml`. |
| `gh api repos/xiongxianfei/velofile/rulesets/16578519 --jq '{id, name, target, enforcement, conditions, rules}'` | pass | Active default-branch ruleset `protect` requires `ci-fast-required`. |
| `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` | expected nonzero with documented caveat | Returned `Branch not protected` (HTTP 404); artifacts record the handoff as ruleset-based. |
| `gh pr view 5 --json headRefOid,statusCheckRollup,url,state,isDraft` | pass | Draft PR #5 head `00f006a17c736439595dcc0b99c6953fe0fe6aac`; only `ci-fast-required` in rollup; conclusion `SUCCESS`. |
| `gh run view 26087711964 --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt` | pass | Workflow `ci`, event `pull_request`, head `00f006a17c736439595dcc0b99c6953fe0fe6aac`, conclusion `success`, job `ci-fast-required` passed in 6m8s. |
| `git diff --check` | pass | Passed with Git LF-to-CRLF working-copy warnings only. |

## Hosted CI

Latest observed hosted PR run before this verify report was recorded:

- Run: https://github.com/xiongxianfei/velofile/actions/runs/26087711964
- Job: https://github.com/xiongxianfei/velofile/actions/runs/26087711964/job/76704989517
- Commit: `00f006a17c736439595dcc0b99c6953fe0fe6aac`
- Workflow: `ci`
- Event: `pull_request`
- Result: success
- `ci-fast-required`: success in 6m8s

Warnings observed:

- GitHub Actions reported Node.js 20 deprecation warnings for `actions/checkout@v4`, `actions/setup-dotnet@v4`, and `actions/upload-artifact@v4`.
- GitHub Actions reported that `windows-latest` requests are being redirected to `windows-2025-vs2026` by June 15, 2026.

These warnings do not fail the current verification, but they should be tracked as separate CI platform maintenance if they become actionable.

## Artifact Drift

- Verify found and corrected stale lifecycle state in `docs/plan.md`, which still listed this plan under Blocked.
- Verify found and corrected stale introductory/current-state wording in `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`.
- Verify found and corrected stale `Until M2 lands` wording in `branch-protection-handoff.md`.
- `change.yaml`, `review-log.md`, `review-resolution.md`, `explain-change.md`, and code-review records now agree that implementation milestones and review are closed, explain-change is complete, and PR handoff is next after verify.

## Residual Risks

- Local full Corpus `Fast|Contract` validation currently hangs in the pre-existing prepared-tool harness test on this machine. Hosted `ci-fast-required` passed the full fast/contract lane on the current pushed head, and local verification directly passed the changed workflow/evidence/docs surfaces.
- GitHub classic branch protection still returns HTTP 404. The required-check handoff is ruleset-based, and artifacts intentionally avoid claiming classic branch protection is configured.
- This change is not release readiness. Release readiness still requires `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or another accepted release gate.

## Readiness

Branch-ready for PR handoff. This report does not claim PR body readiness, PR open readiness, or release readiness.
