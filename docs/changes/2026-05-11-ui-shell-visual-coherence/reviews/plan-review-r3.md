# Plan Review R3: Visual-Evidence Gate Removal Amendment

## Verdict

revise

## Review Inputs

- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Approved spec amendment: `specs/ui-shell-visual-coherence.md`
- Spec review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/spec-review-r2.md`
- Approved architecture amendment: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`
- Architecture review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/architecture-review-r1.md`
- Blocking review context: CR-007 in `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r13.md`

## Findings

### PR-002: Validation plan still describes required full-shell screenshot states/profiles

- Severity: material
- Location: `docs/plans/2026-05-11-ui-shell-visual-coherence.md:421`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:433`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:435`
- Evidence:
  - The approved spec amendment says screenshots and manual visual-review notes are optional supporting artifacts and must not be required before milestone, M8, final closeout, verification, or release readiness.
  - The plan's Optional Visual Artifact Rule says region milestones do not require screenshots or manual full-shell visual-review notes before closeout.
  - The plan's validation section still says full-shell screenshot and sidecar inventory validation runs in M8, and its manual/review evidence section says to review full-shell screenshots for "required states and profiles." That wording reintroduces a hard-gate interpretation for the same evidence the amendment makes optional.
- Required outcome:
  - Revise the validation plan so screenshot/sidecar inventory validation runs only when optional visual artifacts are recorded.
  - Replace "required states and profiles" with "chosen optional states and profiles when visual artifacts are recorded" or equivalent wording.
  - Keep static/resource validation, behavior-preservation tests, accessibility checks, high-DPI static/app/manual behavior notes when touched, generated-output hygiene, and deviation records as required proof.
- Safe resolution path:
  - Update the validation section wording in the active plan.
  - Update M8 wording only if needed so it is unambiguously conditional on optional visual artifacts existing.
  - Rerun `git diff --check` and a focused `rg` scan for stale "required screenshot/full-shell evidence" wording in active plan sections.
  - Return the plan amendment to plan-review.

## Non-Material Notes

- `docs/plans/2026-05-11-ui-shell-visual-coherence.md:198` still says M4 is `spec-amendment-review-needed`, while the current handoff summary says `amendment-downstream-review-needed`. Normalize this during the same plan edit so milestone state and handoff state agree.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | concern | The plan has strong context, but stale validation wording conflicts with the amended proof model. |
| Source alignment | block | Active validation wording contradicts R66/AC9 by implying required full-shell screenshot states/profiles. |
| Milestone size | pass | M4-M7 remain reviewable region slices and M8 is optional in concept. |
| Sequencing | pass | The amendment review sequence is clear: spec and architecture are approved; plan and test-spec remain. |
| Scope discipline | pass | Non-goals still protect V1 behavior, `hifi-design` non-authority, and screenshot pixel gates. |
| Validation quality | block | The validation section needs conditional optional-artifact wording before the plan can be relied on. |
| TDD readiness | concern | Test-spec review is still pending; plan wording should not preempt it with stale hard-gate language. |
| Risk coverage | pass | Risks cover screenshot subjectivity, high-DPI instability, sidebar behavior, and rollback per region. |
| Architecture alignment | concern | The architecture says optional artifacts are not closeout blockers; the validation section must match. |
| Operational readiness | pass | Generated current/diff outputs remain uncommitted and CI/static validation remain required. |
| Plan maintainability | concern | One milestone state is stale and should be normalized with the validation wording fix. |

## Immediate Next Stage

review-resolution for PR-002. After the plan wording is revised, rerun plan-review before test-spec review.

This direct plan-review request is isolated; it does not automatically enter review-resolution.
