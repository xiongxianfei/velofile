# Plan Review R2: PR CI Post-Merge Handoff

## Review Status

approved

## Review Inputs

- Plan: `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`
- Plan index: `docs/plan.md`
- Change metadata: `docs/changes/2026-05-19-pr-ci-post-merge-handoff/change.yaml`
- Review resolution: `docs/changes/2026-05-19-pr-ci-post-merge-handoff/review-resolution.md`
- Previous review: `docs/changes/2026-05-19-pr-ci-post-merge-handoff/reviews/plan-review-r1.md`
- Proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- Spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Project map: `docs/project-map.md`
- `AGENTS.md`
- `CONSTITUTION.md`

`docs/workflows.md` does not exist in this repository, so artifact placement used the current change record and the existing `docs/changes/<change-id>/reviews/` pattern.

## Resolution Verification

PRCI-PMHR1 is resolved.

- The revised M2 implementation steps require removing or fully disabling the temporary broad `ci` job from `.github/workflows/ci.yml`.
- The revised M2 workflow contract tests are expected to fail if broad `ci` still runs on `pull_request` or `push` after handoff.
- The revised plan preserves broad closeout through `.github/workflows/closeout.yml` and local `scripts/ci.ps1`.
- The revised M3 hosted confirmation expects no default broad `ci` job on PR or push unless a later accepted spec or ADR amendment authorizes that lane.
- The decision log now rejects keeping broad `ci` on `push` to `main` as a default hosted lane.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | pass | The plan explains PR #4 state, current workflow shape, branch-protection API evidence, likely files, non-goals, and handoff blockers. |
| Source alignment | pass | M1-M3 now follow the proposal rollout, spec R13/R49-R53, AC13/AC14, and the accepted ADR lane split. |
| Milestone size | pass | M1 handoff evidence, M2 workflow cleanup, and M3 hosted confirmation/lifecycle closeout are coherent review slices. |
| Sequencing | pass | Branch-protection handoff evidence precedes broad-shadow removal; hosted confirmation follows workflow cleanup. |
| Scope discipline | pass | Fast-lane command selection, release-evidence policy, test categories, `scripts/ci.ps1`, production behavior, and branch-protection mutation remain guarded. |
| Validation quality | pass | Each milestone names focused test commands, `git diff --check`, branch-protection evidence, and hosted run evidence where required. |
| TDD readiness | pass | Workflow contract, rollout evidence, and validation documentation test updates are identified before implementation. |
| Risk coverage | pass | The plan covers absent branch protection, hidden required checks, rollback to broad closeout, workflow drift, hosted evidence drift, and release-evidence preservation. |
| Architecture alignment | pass | The plan follows ADR 0012: `ci-fast-required` for ordinary PR confidence, release evidence explicit, manual `ci-full-closeout` for broad closeout, and maintainer-operated branch protection. |
| Operational readiness | pass | Runtime/hosted evidence, branch-protection handoff, release readiness, rollback, and contributor guidance updates are planned. |
| Plan maintainability | pass | Progress, handoff summary, decisions, surprises, validation notes, and outcome sections are present and update-ready. |

## Missing Milestones Or Dependencies

No missing milestones were found.

The approved existing test spec `specs/pr-ci-validation-tiering.test.md` already covers the relevant workflow contract, rollout evidence, branch-protection handoff, release-readiness, rollback, and broad-PR preservation migration tests. A new test-spec stage is not required unless implementation discovers a new contract gap.

M2 remains correctly dependent on M1 handoff evidence. If `main` branch protection remains absent or does not name `ci-fast-required`, M1 should record the blocker and M2 must not remove the temporary broad job.

## Exact Suggested Edits

None required for plan approval.

Implementation should follow the revised M2 wording exactly: remove or fully disable the temporary broad `ci` job from `.github/workflows/ci.yml`; do not keep it as a default `push` to `main` lane without a later accepted spec or ADR amendment.

## Verdict

approve

The plan is source-aligned, sequenced, and verifiable. PRCI-PMHR1 is closed by the revised plan.

## Immediate Next Stage

No new `test-spec` stage is required because `specs/pr-ci-validation-tiering.test.md` is already approved and covers this follow-up. The next executable stage is `implement` for M1 branch-protection handoff evidence.

M2 implementation remains blocked until M1 records accepted handoff evidence showing `ci-fast-required` is the intended ordinary PR check and the plan is safe to remove the temporary broad `ci` shadow job.
