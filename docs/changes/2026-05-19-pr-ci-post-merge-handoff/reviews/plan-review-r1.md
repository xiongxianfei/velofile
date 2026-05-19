# Plan Review R1: PR CI Post-Merge Handoff

## Review Status

changes-requested

## Review Inputs

- Plan: `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`
- Plan index: `docs/plan.md`
- Change metadata: `docs/changes/2026-05-19-pr-ci-post-merge-handoff/change.yaml`
- Proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- Spec: `specs/pr-ci-validation-tiering.md`
- Test spec: `specs/pr-ci-validation-tiering.test.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Project map: `docs/project-map.md`
- `AGENTS.md`
- `CONSTITUTION.md`

`docs/workflows.md` does not exist in this repository, so artifact placement used the change ID implied by the reviewed plan and the existing `docs/changes/<change-id>/reviews/` pattern.

## Material Findings

### PRCI-PMHR1: M2 keeps broad CI as a push-to-main default after handoff

- Severity: material
- Location: `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`, M2 implementation steps and decision log.
- Evidence: The plan directs M2 to "add a job-level condition so broad `ci` runs on `push` to `main` but not on ordinary `pull_request` events" and adds a decision to "Keep broad `ci` on `push` to `main` for the first cleanup slice." That leaves the broad closeout path as a continuing default hosted workflow after the handoff. The approved proposal instead says, after comparison, make `ci-fast-required` the required ordinary PR check and "Move full closeout/release-evidence validation to manual, scheduled, release-branch, merge-queue, or milestone closeout triggers" (`docs/proposals/2026-05-18-pr-ci-validation-tiering.md`, Rollout and rollback). ADR 0012 defines `.github/workflows/ci.yml` for `ci-fast-required`, `.github/workflows/closeout.yml` for manual `ci-full-closeout`, and says the broad script remains stable through the closeout lane, while branch-protection handoff and rollback are recorded separately. The current project map also describes broad `ci` as a temporary shadow/rollback job "until maintainers record branch-protection handoff."
- Required outcome: The post-handoff plan must align the final workflow shape with the approved CI tiering contract. Broad closeout validation must remain available through `ci-full-closeout` and local `scripts/ci.ps1`, but the plan must not leave broad `ci` as a default `ci.yml` push job unless an approved spec or architecture amendment explicitly accepts that extra lane.
- Safe resolution path: Revise M2 to remove or fully disable the temporary broad `ci` job from `.github/workflows/ci.yml` after branch-protection handoff, instead of keeping it on `push` to `main`. Update workflow contract tests so they fail if the broad `ci` shadow job still runs on `pull_request` or `push` after the cleanup milestone. Keep tests that prove `.github/workflows/closeout.yml` and `scripts/ci.ps1` still preserve broad closeout. Update the decision log, project-map/guidance steps, and validation evidence accordingly, then rerun `plan-review`. If maintainers intentionally want a continuing push-to-main broad integration lane, route that as a spec or architecture amendment before approving this plan.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | pass | The plan explains PR #4 state, current workflows, branch-protection evidence, likely files, non-goals, and handoff blockers. |
| Source alignment | block | PRCI-PMHR1 conflicts with the approved rollout/ADR shape by retaining broad `ci` as a default push job after handoff. |
| Milestone size | pass | M1 evidence, M2 workflow cleanup, and M3 hosted confirmation are reviewable slices. |
| Sequencing | concern | M1 correctly precedes cleanup, but M2 closes with a residual broad default lane that the prior rollout treated as temporary. |
| Scope discipline | concern | Keeping push-main broad CI broadens the accepted post-handoff lane model instead of simply removing ordinary PR cost. |
| Validation quality | concern | Commands are concrete, but the proposed M2 tests would preserve a push-main broad job that needs source approval or removal. |
| TDD readiness | concern | Workflow/evidence tests are identified, but their expected final behavior must change with PRCI-PMHR1. |
| Risk coverage | pass | Branch-protection absence, rollback, release-evidence preservation, and hosted drift are covered. |
| Architecture alignment | block | ADR 0012 separates fast CI, release evidence, and manual closeout; broad `ci` as a continuing push-main lane is not part of that accepted design. |
| Operational readiness | concern | The plan preserves rollback, but leaving broad `ci` on push keeps extra hosted cost and ambiguity after the claimed cleanup. |
| Plan maintainability | pass | Progress, decisions, discoveries, validation notes, and closeout sections are ready to update. |

## Missing Milestones Or Dependencies

No additional milestone class is required if PRCI-PMHR1 is fixed by removing the broad `ci` job from `.github/workflows/ci.yml` during M2.

If the desired policy is to keep broad `ci` on `push` to `main`, this plan is missing an upstream source decision: a spec or ADR amendment accepting that fourth hosted lane or post-merge broad integration signal.

## Exact Suggested Edits

- In M2 implementation steps, replace the push-main broad condition with deletion or full disablement of the temporary broad `ci` job from `.github/workflows/ci.yml`.
- In M2 tests, replace "broad `ci` is limited to `push` to `main`" with "broad `ci` does not run from `.github/workflows/ci.yml` after handoff."
- Keep explicit tests that `.github/workflows/closeout.yml` exposes `ci-full-closeout` and `scripts/ci.ps1` remains broad.
- Remove the decision-log entry that keeps broad `ci` on push, or rewrite it to state that broad hosted validation remains through manual closeout and rollback can restore a broad required check if needed.
- Update M3 hosted confirmation to expect broad `ci` absent from ordinary PR and not a default push-main job, unless an upstream amendment accepts that lane.

## Verdict

revise

The plan is close, but PRCI-PMHR1 must be resolved before the plan can move to downstream test-spec or implementation work.

## Immediate Next Stage

`review-resolution` for PRCI-PMHR1, then rerun `plan-review`.

`test-spec` is blocked until this plan review approves the revised plan. Implementation is also blocked.
