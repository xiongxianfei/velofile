# Review Resolution

## Status

closed by plan-review-r2; ready for M1 implementation

## Findings

### PRCI-PMHR1: M2 keeps broad CI as a push-to-main default after handoff

- Source review: [plan-review-r1](reviews/plan-review-r1.md)
- Disposition: accepted
- Status: closed by plan-review-r2
- Severity: material
- Required outcome: The post-handoff plan must align the final workflow shape with the approved CI tiering contract. Broad closeout validation must remain available through `ci-full-closeout` and local `scripts/ci.ps1`, but the plan must not leave broad `ci` as a default `ci.yml` push job unless an approved spec or architecture amendment explicitly accepts that extra lane.
- Safe resolution path:
  - Revise M2 to remove or fully disable the temporary broad `ci` job from `.github/workflows/ci.yml` after branch-protection handoff, instead of keeping it on `push` to `main`.
  - Update workflow contract tests so they fail if the broad `ci` shadow job still runs on `pull_request` or `push` after the cleanup milestone.
  - Keep tests that prove `.github/workflows/closeout.yml` and `scripts/ci.ps1` still preserve broad closeout.
  - Update the plan decision log, project-map/guidance steps, and validation evidence accordingly.
  - Rerun `plan-review`.
  - If maintainers intentionally want a continuing push-to-main broad integration lane, route that as a spec or architecture amendment before approving this plan.
- Resolution:
  - Revised M2 so the temporary broad `ci` job is removed or fully disabled from `.github/workflows/ci.yml` after branch-protection handoff.
  - Replaced the push-to-main broad assertion with workflow contract expectations that fail if broad `ci` still runs on `pull_request` or `push` after handoff.
  - Kept explicit closeout preservation through `.github/workflows/closeout.yml` and local `scripts/ci.ps1`.
  - Updated M3 hosted confirmation so broad `ci` is expected to be absent from ordinary PR and default push validation unless an upstream spec or ADR amendment accepts that lane.
  - Rewrote the plan decision log to reject a continuing default push-to-main broad lane.
- Validation:
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
  - ``Select-String -Path docs\plans\2026-05-19-pr-ci-post-merge-handoff.md -Pattern 'add a job-level condition|workflow contract test proves broad `ci` is limited|push-only broad|Keep broad `ci` on `push` to `main` for the first cleanup slice' -Quiet`` returned `False`, so no stale push-main broad assertions remain in the revised plan.
  - ``Select-String -Path docs\plans\2026-05-19-pr-ci-post-merge-handoff.md,docs\changes\2026-05-19-pr-ci-post-merge-handoff\review-resolution.md -Pattern 'remove or fully disable|broad `ci` still runs on `pull_request` or `push`|default `push` to `main` lane|Remove broad `ci` from default CI'`` confirmed the revised removal/disablement language and closeout-preservation expectations.
- Closeout:
  - [plan-review-r2](reviews/plan-review-r2.md) approved the revised plan with no findings.
  - PRCI-PMHR1 is closed.
  - No new test-spec stage is required because the approved existing test spec covers the follow-up.
  - The next executable stage is M1 implementation for branch-protection handoff evidence.
