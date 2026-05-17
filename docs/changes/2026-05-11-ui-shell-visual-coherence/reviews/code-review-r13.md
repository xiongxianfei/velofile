# Code Review R13: Visual-Evidence Gate Amendment Recheck

## Result

blocked

## Review Inputs

- Reviewed milestone: M4 Command Band Visual Coherence and the 2026-05-17 visual-evidence gate removal amendment.
- Review surface: current working tree after the amendment changed the spec, test spec, architecture, ADR, active plan, plan index, change metadata, and review-resolution record.
- Governing order: `CONSTITUTION.md` source-of-truth order gives authority to approved specs, test specs, architecture, ADRs, and active plans before implementation or unapproved draft artifacts.
- Feature spec: `specs/ui-shell-visual-coherence.md`.
- Test spec: `specs/ui-shell-visual-coherence.test.md`.
- Architecture: `docs/architecture/system/architecture.md`.
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`.
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
- Change record: `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`.
- Prior reviews: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r11.md` and `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r12.md`.

## Diff Summary

- Reframed screenshots, screenshot sidecars, and manual visual-review notes as optional supporting artifacts rather than mandatory milestone, final-closeout, verification, or release-readiness gates.
- Updated the architecture and ADR to describe visual artifacts as optional review context.
- Updated the active plan and plan index so M4 remains in `spec-amendment-review-needed` while the visual-evidence gate removal amendment waits for upstream review.
- Updated `change.yaml` and `review-resolution.md` so CR-005 is pending supersession by the draft amendment, while CR-006 remains resolved.

## Findings

### CR-007: M4 code review cannot rely on an unreviewed visual-evidence gate amendment

- Severity: blocker
- Location: `specs/ui-shell-visual-coherence.md:5`, `specs/ui-shell-visual-coherence.test.md:5`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:20`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:25`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:483`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:485`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:487`, `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml:3`, `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml:33`, `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml:57`, `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md:5`, `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md:12`
- Evidence:
  - `CONSTITUTION.md` ranks approved feature specs, matching test specs, approved architecture/ADRs, and active plans above implementation code and unapproved drafts.
  - The feature spec status is still `draft amendment ... pending spec-review`.
  - The test spec status is still `draft amendment ... pending review`.
  - The active plan explicitly says the feature spec and test spec are draft amendments pending review, the current milestone state is `spec-amendment-review-needed`, and M4 code review cannot rely on the amendment until amendment review completes.
  - `change.yaml` records the change as `visual-evidence-gate-amendment-draft`, with M4 implementation status `spec-amendment-review-needed` and next stage `spec-review for visual-evidence-gate removal amendment`.
  - `review-resolution.md` says CR-005 is only pending supersession by the drafted amendment. It is not resolved by an approved contract change yet.
  - The test spec still contains a stale readiness line, `Active and ready for implement at M1`, even though the same file status says the amendment is pending review.
- Required outcome:
  - Complete and record the upstream reviews for the visual-evidence gate removal amendment before using the amended contract to close CR-005 or pass M4 code review.
  - Align the test spec readiness text with the amendment state so it does not claim M1 implementation readiness while the amendment is pending review.
  - If the amendment is approved, update `review-resolution.md`, `change.yaml`, the active plan, and `docs/plan.md` to mark CR-005 resolved/superseded and return M4 to code review under the amended contract.
  - If the amendment is rejected or revised materially, keep M4 blocked or restore the prior M4 visual-evidence resolution path.
- Safe resolution path:
  - Run `spec-review` for the visual-evidence gate removal amendment.
  - Run matching `test-spec` review, `architecture-review`, and `plan-review` for the amendment because it changes the approved proof model across the spec, test spec, architecture, ADR, and plan closeout rules.
  - Fix any upstream review findings and update artifact statuses to approved/amended only after those reviews pass.
  - Re-run M4 `code-review` after CR-005 has a resolved/superseded disposition grounded in approved artifacts.

## Resolved Finding Confirmation

### CR-006: `change.yaml` duplicate validation key

- Status: resolved
- Evidence:
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml` has exactly one current `implementation.validation` key at line 35.
  - Older M3 validation is stored under `history.prior_milestone_validation.M3`.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | The amendment removes CR-005's evidence gate, but it is explicitly still a draft pending spec review. M4 cannot use it as approved authority yet. |
| Test coverage | concern | The test spec amendment changes TSC013/TSC014/TSC020 semantics, but the test spec remains pending review and has stale readiness text. |
| Edge cases | concern | The prior named M4 filter/search visual states are no longer mandatory only if the amendment is approved. |
| Error handling | pass | The reviewed diff is governance and metadata only. |
| Architecture boundaries | block | Architecture and ADR text were amended, but the plan requires architecture-review/ADR-aligned review before M4 can rely on the changed proof model. |
| Compatibility | concern | Milestone closeout semantics change for M4-M8; this needs upstream review before contributor expectations are changed. |
| Security/privacy | pass | The amendment reduces screenshot requirements and does not introduce new private path artifacts. |
| Derived artifact currency | concern | Plan, change metadata, and review-resolution agree that the amendment is pending review; test spec readiness text is stale. |
| Unrelated changes | pass | The reviewed changes are scoped to the visual-evidence gate amendment and associated review metadata. |
| Validation evidence | concern | `git diff --check` and duplicate-key checks passed, but validation cannot approve the unreviewed contract amendment. |

## Reviewer Validation

- `git status --short`: inspected current modified and untracked review artifacts.
- `rg -n "Status:|draft amendment|spec-amendment-review-needed|next_stage|pending-superseded|Current milestone state|Review status|Next stage|CR-005|CR-006|implementation:" ...`: confirmed the amendment remains pending review and CR-005 is not resolved.
- `Select-String -Path docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml -Pattern '^  validation:'`: found exactly one current `implementation.validation` key at line 35.
- `git diff --check`: passed with CRLF normalization warnings only.
- `rg -n "must produce full-shell|required screenshot|MUST include shell-default|M4 needs full-shell|visual-evidence-needed|m4-visual-evidence-needed|M8 remains blocked until|M8/final closeout must fail|M3 closes without accepted" ...`: found only historical review-record references and the first-slice spec's separate baseline wording, not active hard-gate wording in the amended M4 contract.

No full app tests or CI were rerun during this review because the blocker is upstream governance approval, not a code or resource implementation failure.

## Next Stage

Stop on `blocked`. This direct `code-review` invocation is isolated and does not automatically hand off to review-resolution or upstream review stages.

Next required stage is `spec-review` for the visual-evidence gate removal amendment, followed by matching test-spec, architecture, and plan reviews before M4 code review can pass under the amended contract.
