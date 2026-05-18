# PR CI Validation Tiering Verify Report

## Verdict

branch-ready for PR handoff

This verifies the PR CI validation tiering change pack after M1-M5 implementation, code review, review-resolution, and final explain-change. It does not claim PR body readiness or that GitHub branch protection has been changed.

## Scope

- Change ID: `2026-05-18-pr-ci-validation-tiering`
- Branch: `pr-ci-validation-tiering`
- Verified implementation HEAD: `5188c33459218edd382cf50ab9adfca973b0c974`
- Draft PR: https://github.com/xiongxianfei/velofile/pull/4
- Plan: `docs/plans/2026-05-18-pr-ci-validation-tiering.md`
- Spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Explain-change: `docs/changes/2026-05-18-pr-ci-validation-tiering/explain-change.md`
- Final code review: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/code-review-r9.md`

## Traceability

| Requirement area | Test IDs / proof | Files changed | Verification evidence | Status |
|---|---|---|---|---|
| Fast PR lane identity, triggers, command selection, summary, and hosted environment | PRCI-T001, PRCI-T002, PRCI-T006-T018, PRCI-T023-T026 | `.github/workflows/ci.yml`, workflow model/tests, runtime summary helper | Focused workflow tests passed; hosted `ci-fast-required` passed in run `26063842493` | pass |
| Release-evidence lane remains explicit and outside ordinary PR defaults | PRCI-T003, PRCI-T005, PRCI-T019-T020 | `.github/workflows/release-evidence.yml`, workflow tests | Workflow contract tests passed; release-evidence workflow exists without ordinary PR trigger | pass |
| Full closeout remains broad and manually runnable | PRCI-T004, PRCI-T021-T022 | `.github/workflows/closeout.yml`, workflow tests | Workflow contract tests passed; hosted broad `ci` and local broad closeout evidence passed | pass |
| Runtime summaries and reporting | PRCI-T023-T026 | `scripts/Write-CiRuntimeSummary.ps1`, workflow summary calls, runtime tests | Runtime summary tests passed; hosted summaries completed in run `26063842493` | pass |
| Shadow-run, branch-protection handoff, rollback, and contributor guidance | PRCI-T027-T029, PRCI-T033, PRCI-M001, PRCI-M003 | `shadow-run.md`, `branch-protection-handoff.md`, README, CONTRIBUTING, rollout tests | M5 focused tests passed; hosted shadow run recorded; branch protection status recorded as HTTP 404/no handoff | pass |
| Lifecycle and review closeout | review log, review-resolution, explain-change | `change.yaml`, active plan, review records, explain-change | Code-review-r9 clean-with-notes; no open review findings; explain-change complete | pass |

## Dimension Assessment

| Dimension | Result | Evidence |
|---|---|---|
| Spec coverage | pass | Implemented areas map to R1-R69 and AC1-AC20. |
| Requirement satisfaction | pass | Required workflow lanes, summaries, environment contract, rollout evidence, and no-overclaim behavior are covered by tests, hosted evidence, and review records. |
| Test coverage | pass | Workflow, runtime-summary, documentation, and rollout evidence tests cover the test-spec cases for the changed surfaces. |
| Test validity | pass | Tests inspect workflow YAML structurally, assert command ordering and failure semantics, and assert required rollout evidence strings rather than only checking file existence. |
| Architecture coherence | pass | The implementation follows ADR 0012: shared helper, separate release/closeout workflows, preserved broad closeout command, and structured workflow tests. |
| Artifact lifecycle state | pass | `docs/plan.md`, the active plan, `change.yaml`, review log, review-resolution, explain-change, and this report agree on next stage. |
| Plan completion | pass | M1-M5 are closed; M6 lifecycle closeout is in progress with verify complete and PR handoff next. |
| Validation evidence | pass | Focused local tests, diff checks, hosted latest PR run, and branch-protection API evidence are recorded. |
| Drift detection | pass | Project map, README, CONTRIBUTING, plan, and change metadata describe the implemented topology and rollout boundary. |
| Risk closure | pass | Release evidence is preserved, broad closeout remains available, rollback is documented, and branch-protection handoff is not overclaimed. |
| Release readiness | pass with scope note | Branch is ready for PR handoff. Release readiness still requires `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or another accepted release gate. |

## Validation Commands

All commands ran from the repository root unless noted.

| Command | Result | Important output |
|---|---|---|
| `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"` | pass | 20 tests passed. |
| `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout" README.md docs specs .github` | pass | Expected matches found in guidance, specs, workflows, and change records. |
| `git diff --check -- .github\workflows README.md CONTRIBUTING.md docs\project-map.md docs\changes\2026-05-18-pr-ci-validation-tiering docs\plans\2026-05-18-pr-ci-validation-tiering.md tests\VeloFile.Corpus.Tests` | pass | Passed with Git LF-to-CRLF working-copy warnings only. |
| `git diff --check HEAD~1..HEAD` | pass | Passed with no output during code-review-r9. |
| `git diff --check -- docs\changes\2026-05-18-pr-ci-validation-tiering\explain-change.md` | pass | Passed with Git LF-to-CRLF working-copy warnings only. |
| `gh run watch 26063842493 --interval 30 --exit-status` | pass | Latest hosted PR run for commit `5188c33459218edd382cf50ab9adfca973b0c974` passed. |
| `gh run view 26063842493 --json status,conclusion,jobs,url,headSha,createdAt,updatedAt` | pass | Workflow `ci` completed with conclusion `success`; `ci-fast-required` passed in 6m32s and broad `ci` passed in 17m11s. |
| `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` | pass with rollout caveat | Returned `Branch not protected` (HTTP 404), which is recorded as no maintainer handoff and no external required-check claim. |
| `python scripts\validate-change-metadata.py docs\changes\2026-05-18-pr-ci-validation-tiering\change.yaml` | not run | Script path does not exist in this repository; not part of the approved M5 validation set. |

## Hosted CI

Latest observed hosted PR run:

- Run: https://github.com/xiongxianfei/velofile/actions/runs/26063842493
- Commit: `5188c33459218edd382cf50ab9adfca973b0c974`
- Workflow: `ci`
- Result: success
- `ci-fast-required`: success in 6m32s
- broad `ci`: success in 17m11s

Warnings observed:

- GitHub Actions reported Node.js 20 deprecation warnings for `actions/checkout@v4`, `actions/setup-dotnet@v4`, and `actions/upload-artifact@v4`.
- GitHub Actions reported that `windows-latest` requests are being redirected to `windows-2025-vs2026` by June 15, 2026.

These warnings do not fail the current verification, but they should be monitored as CI platform follow-up.

## Artifact Drift

- `docs/plan.md` and `docs/plans/2026-05-18-pr-ci-validation-tiering.md` agree that M6 lifecycle closeout is active and PR handoff is next after verify.
- `change.yaml` points to the proposal, spec, architecture, ADR, plan, test spec, explain-change, review records, and validation evidence.
- `review-log.md` includes `code-review-r9` with no open findings.
- `review-resolution.md` remains the closed material-finding disposition record for earlier findings.
- `shadow-run.md` and `branch-protection-handoff.md` distinguish hosted evidence from external branch-protection configuration.

## Residual Risks

- GitHub branch protection is not configured for `main` according to the API evidence. Maintainers still need to perform any external required-check handoff.
- The broad `ci` PR job remains during rollout, so hosted minutes are temporarily higher until handoff.
- CI platform warnings for Node.js 20 action runtime deprecation and `windows-latest` redirection should be tracked as follow-up maintenance.

## Readiness

Branch-ready for PR handoff. The next stage is `pr`. This report does not claim PR body readiness, PR open readiness, release readiness, or branch-protection handoff completion.
