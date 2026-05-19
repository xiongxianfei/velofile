# Code Review R1: PR CI Post-Merge Handoff M1

## Review status

clean-with-notes

## Review inputs

- Diff/review surface: commit `e0b4ea8 M1: Record PR CI post-merge handoff evidence`.
- Tracked governing branch state: local `main` ahead of `origin/main` by the M1 implementation commit.
- Governing artifacts: `specs/pr-ci-validation-tiering.md`, `specs/pr-ci-validation-tiering.test.md`, `docs/adr/0012-hosted-pr-ci-validation-tiers.md`, `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`, and `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`.
- Validation evidence: M1 notes in `docs/changes/2026-05-19-pr-ci-post-merge-handoff/change.yaml` and the active plan, including the focused rollout-evidence test, branch-protection API result, PR/run inspection, and `git diff --check`.

## Diff summary

M1 adds a post-merge branch-protection handoff artifact, records that GitHub reports `main` as unprotected with HTTP 404, preserves `ci-fast-required` as the intended ordinary PR check without claiming branch protection changed, and blocks M2 until maintainers record the external handoff. The milestone also adds rollout evidence coverage for the new handoff artifact and updates the active plan/change metadata with the hosted PR #4 and final observed run evidence.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Evidence |
|---|---|---|
| Spec alignment | pass | The handoff artifact follows R13 by naming `ci-fast-required` as intended while explicitly recording that branch protection is not configured. |
| Test coverage | pass | `Post_merge_handoff_records_current_branch_protection_blocker` fails without the handoff artifact and asserts the current 404/no-handoff state. |
| Edge cases | pass | The missing-branch-protection case is handled by blocking M2 instead of changing workflow behavior. |
| Error handling | pass | The nonzero GitHub API result is recorded as external-policy evidence rather than hidden or treated as proof of success. |
| Architecture boundaries | pass | No CI lane command selection, release-evidence policy, or closeout workflow behavior changes are made in M1. |
| Compatibility | pass | Broad `ci` remains available as the shadow/rollback path until maintainer-operated branch protection is reconciled. |
| Security/privacy | pass | The artifact records public repository metadata and does not expose secrets or tokens. |
| Derived artifact currency | pass | Plan, change metadata, review log, and rollout evidence test point at the same M1 state. |
| Unrelated changes | pass | The diff is limited to rollout evidence, lifecycle artifacts, plan/index state, and the focused test. |
| Validation evidence | pass | The recorded evidence includes focused test failure-before/pass-after behavior, `gh api` 404, PR/run inspection, and `git diff --check`. |

## No-finding rationale

The reviewed slice satisfies the approved post-merge handoff contract for M1: it records the current external branch-protection state, avoids overclaiming that `ci-fast-required` is required, keeps the temporary broad `ci` path unchanged, and makes the blocker testable before any M2 workflow cleanup. The implementation does not exceed the milestone scope or alter fast-lane, release-evidence, or closeout semantics.

## Residual risks

- External branch protection is still absent for `main`; M2 remains blocked until maintainers configure or record the `ci-fast-required` handoff.
- The reviewed evidence does not claim branch readiness, PR readiness, final verification, or CI policy completion.

## Recommended next stage

Close M1. Keep the plan blocked before M2 until maintainer-operated branch-protection handoff evidence exists.
