# Code Review R8: PR CI Validation Tiering M5

## Review Status

blocked

## Reviewed Milestone

M5. Shadow-Run Evidence, Final Policy Transition, And Contributor Guidance

## Review Inputs

- Review surface: unstaged M5 blocker bookkeeping diff in `docs/plan.md`, `docs/plans/2026-05-18-pr-ci-validation-tiering.md`, and `docs/changes/2026-05-18-pr-ci-validation-tiering/change.yaml`
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M5
- Feature spec: `specs/pr-ci-validation-tiering.md` R49-R53 and AC13
- Test spec: `specs/pr-ci-validation-tiering.test.md` PRCI-T027, PRCI-T028, PRCI-M001, and PRCI-M003
- Validation evidence reviewed:
  - `git status --short --branch`
  - `git rev-parse HEAD`
  - `gh run list --limit 20 --json databaseId,workflowName,displayTitle,event,headBranch,headSha,status,conclusion,createdAt,updatedAt,url`
  - `gh run view 26002964319 --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt`
  - `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
  - `git diff --check -- docs\plan.md docs\plans\2026-05-18-pr-ci-validation-tiering.md docs\changes\2026-05-18-pr-ci-validation-tiering\change.yaml`

## Diff Summary

The diff does not implement M5. It moves the active plan from ready to blocked, records that no hosted `ci-fast-required` shadow PR cycle exists for the local M2-M4 workflow changes, and records that branch-protection handoff evidence is unavailable. It also keeps final closeout unavailable and routes the next stage to collecting hosted shadow-run evidence.

## Findings

### PRCI-CR4: M5 cannot enter implementation review without hosted shadow-run evidence

- Severity: blocker
- Location: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M5 dependency and current handoff summary; `docs/changes/2026-05-18-pr-ci-validation-tiering/change.yaml` `m5_preflight`
- Evidence: The spec requires `ci-fast-required` to shadow-run for at least one PR cycle before branch-protection transition (R49) and requires shadow-run comparison evidence (R50, AC13). The test spec requires at least one hosted PR cycle after M2 in PRCI-M001. Reviewer rerun of GitHub Actions history showed no run for local HEAD `9ec9c78ce54cc4c5dd701cec76ff1b49ba8293e7`; the latest PR run inspected, `26002964319`, had a single broad `ci` job and no `ci-fast-required` job. The branch-protection API returned `Branch not protected` (HTTP 404), so there is also no maintainer handoff evidence to review.
- Required outcome: Do not mark M5 implemented, review-requested, closed, or ready for final closeout until a hosted PR cycle after M2 records `ci-fast-required` runtime, failures, selected categories, and broad-check pass/fail when available.
- Safe resolution path: Push or open a PR containing the M2-M4 workflow changes, let `ci-fast-required` shadow-run for at least one PR cycle, record the hosted evidence in `shadow-run.md`, record branch-protection handoff status without overclaiming external settings, then rerun `implement` for M5 before requesting code review again.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | The blocker bookkeeping aligns with R49-R51 by refusing to proceed, but M5 itself cannot satisfy R50/AC13 until hosted shadow-run evidence exists. |
| Test coverage | block | PRCI-M001 is manual hosted evidence, and the required hosted PR cycle is absent. No new M5 workflow/documentation tests were expected after the implementation stopped. |
| Edge cases | pass | The diff avoids the named overclaim edge case by not creating `shadow-run.md`, not claiming `ci-fast-required` is required, and recording branch protection as unavailable. |
| Error handling | pass | The plan now routes to evidence collection instead of fabricating runtime or handoff data. |
| Architecture boundaries | pass | No workflow or production architecture changed in this diff. |
| Compatibility | pass | The blocked state preserves current workflow behavior and does not alter fast-lane, release-evidence, closeout, or category policy. |
| Security/privacy | pass | The recorded evidence contains run IDs, commit IDs, workflow names, and branch-protection status only; no secrets or local private data were added. |
| Derived artifact currency | pass | `docs/plan.md`, the active plan, and `change.yaml` agree that M5 is blocked on hosted shadow-run evidence. |
| Unrelated changes | pass | The diff is limited to lifecycle state and validation notes for the M5 blocker. |
| Validation evidence | concern | `git diff --check` passed, and the GitHub CLI evidence supports the blocker. The prior `scripts\validate-change-metadata.py` command could not run because that script is not present in this repository. |

## No-Finding Rationale

Not applicable. The review is blocked by PRCI-CR4.

## Residual Risks

- GitHub Actions and branch-protection state are external and can change after this review. M5 should use fresh hosted run and branch-protection evidence when it is retried.
- The current local branch is ahead of `origin/main`; hosted evidence cannot exist for these local commits until they are pushed to a PR or branch that runs Actions.

## Recommended Next Stage

Stop. Collect hosted shadow-run evidence, then rerun `implement` M5. Do not enter review-resolution, final closeout, verify, or PR handoff from this blocked review.
