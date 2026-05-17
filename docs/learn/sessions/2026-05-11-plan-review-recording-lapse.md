# Learn Session: Plan Review Recording Lapse

## Frame

- Date: 2026-05-11
- Trigger: explicit maintainer questions after plan-review reported a material finding but did not create the required durable review record, then created a record that still did not preserve the material-finding shape completely.
- Trigger type: maintainer observation / workflow-process lapse.
- Scope: the `plan-review` result for `docs/plans/2026-05-11-ui-shell-visual-coherence.md`, the missing change-local review record, the incomplete first correction, and the corrected review record.
- Evidence in scope:
  - Plan-review final response reported material finding `PR-001`.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/` did not exist before this correction.
  - The first created review record for `PR-001` omitted an explicit `Location` field until the maintainer challenged it again.
  - The `plan-review` skill requires every material finding to be recorded under `docs/changes/<change-id>/reviews/<stage>-r<n>.md`, indexed in `review-log.md`, and resolved in `review-resolution.md`.
- Explicit exclusions:
  - This session does not change workflow policy.
  - This session does not resolve `PR-001`.
  - This session does not make the plan approved, implementation-ready, verified, branch-ready, or PR-ready.
- Prior learnings reviewed:
  - `docs/learn/sessions/2026-05-11-icon-glyph-regression.md`; it covers UI visual-regression diagnosis, not review-recording obligations.
- Session record path: `docs/learn/sessions/2026-05-11-plan-review-recording-lapse.md`

## Observe

### O1. The material finding was reported but not durably recorded

Evidence:

- The plan-review response classified `PR-001` as material and instructed that `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r1.md` should be created.
- The change-local directory did not exist when checked immediately after the maintainer question.

Observation:

The reviewer incorrectly treated the review record as a handoff instruction for someone else instead of part of the review output obligation.

### O2. The mistake was not caused by missing policy

Evidence:

- The `plan-review` skill explicitly says isolation governs handoff but does not suppress recording.
- It also says every material finding requires a durable change-local review record, review-log index, and review-resolution entry.

Observation:

This was an execution lapse, not a spec or skill gap. The correct behavior was already documented.

### O3. The first correction created the record but did not fully preserve the finding shape

Evidence:

- The maintainer asked why the finding was not recorded and provided the original material finding with `Location`, evidence, required outcome, and safe resolution path.
- The first created `plan-review-r1.md` recorded `PR-001`, severity, evidence, required outcome, and safe resolution path, but omitted an explicit `Location` field.
- The review record was then amended to include `Location: docs/plans/2026-05-11-ui-shell-visual-coherence.md, M2-M7 region milestones starting at M2; M8 full-shell evidence milestone.`

Observation:

The correction was too narrow. For material findings, the durable record should preserve the complete finding shape from the review result, including location, so the record can stand alone without reconstructing context from chat.

## Classify

| ID | Observation | Proposed classification | Final classification | Secondary routes | Confirmed by | Rationale |
|---|---|---|---|---|---|---|
| O1 | Material plan-review finding was not recorded. | artifact-update | artifact-update | Create missing review record, review log, review-resolution entry, and change metadata. | maintainer question | The missing artifacts are required by the existing review contract. |
| O2 | Existing skill already covered the rule. | no-durable-lesson | no-durable-lesson | Keep rationale in this session record only. | maintainer question | Single execution lapse with existing documented policy; no new durable guidance is justified. |
| O3 | First correction created an incomplete material-finding record. | artifact-update | artifact-update | Amend review record to preserve location and complete material-finding shape. | maintainer question | The required artifact existed but did not fully stand alone until amended. |

## Route

Created the missing review artifacts:

- `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`
- `docs/changes/2026-05-11-ui-shell-visual-coherence/review-log.md`
- `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`
- `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r1.md`

Then amended `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r1.md` to include the missing `Location` field for `PR-001`.

No topic file was created. The event is recorded as corrected artifact updates plus no-durable-lesson rationale because the governing rule already exists and the failures were execution errors against that rule, not evidence that new topic guidance or policy is missing.

## Session Outcome

- Lessons captured in this session record: no durable topic lesson.
- Durable topic updates: none.
- Follow-ups created: none beyond the required review artifacts.
- No-learn rationale: the process rule already exists in the `plan-review` skill; the failure was a one-off execution lapse, not evidence of a new reusable guidance gap.
