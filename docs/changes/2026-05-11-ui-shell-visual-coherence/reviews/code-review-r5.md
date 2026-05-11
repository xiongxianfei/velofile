# Code Review R5

## Status

blocked

## Reviewed Milestone

M2. Shell Surface Foundation

## Review Inputs

- Resolution/blocker commit: `0f2d2c3 M2: record visual evidence blocker`
- Prior M2 review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r4.md`
- CR-002 resolution record: `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`
- M2 visual evidence record: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Architecture and ADR:
  - `docs/architecture/system/architecture.md`
  - `docs/adr/0010-shell-visual-coherence-contracts.md`
- Validation evidence recorded in the active plan and change record.

## Diff Summary

The CR-002 resolution attempt does not add new runtime code or shell resources. It changes lifecycle and evidence artifacts to state that `shell-default` full-shell visual evidence is unavailable in this tool session, records that no observed visual review was performed, keeps M2 in `resolution-needed`, and cites the focused validation and CI reruns after the evidence record was corrected.

## Findings

### CR-002: M2 still lacks accepted full-shell visual evidence

- Severity: major
- Status: unresolved; blocked in this tool session
- Evidence:
  - Spec R22 requires full-shell screenshots for a region slice to show no new mismatch between redesigned and non-redesigned regions, or the mismatch must be recorded as a deviation: `specs/ui-shell-visual-coherence.md:127`.
  - Spec R26 requires shell surface foundation compatibility in the default shell screenshot: `specs/ui-shell-visual-coherence.md:137`.
  - Test spec TSC013 requires M2 `shell-default` full-shell evidence before closeout: `specs/ui-shell-visual-coherence.test.md:231`.
  - The M2 visual evidence record now explicitly states that no observed full-shell visual review was performed, no screenshot or sidecar was captured, the whole shell was not observed, and the evidence is not accepted: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`.
- Required outcome:
  - M2 cannot close until `shell-default` full-shell visual evidence exists for `shell-standard-1440x900-100`. The evidence must be either an automated/current screenshot with sidecar, a manual screenshot review, or an observed manual full-shell visual-review note. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Safe resolution path:
  - Use a reviewer environment that can launch and observe the WinUI app at the required profile.
  - Record the observed result in `m2-shell-default.md`, including reviewer/date/profile/state, whether the whole shell was visible, whether any redesigned/non-redesigned mismatch was observed, and deviation status.
  - Rerun the M2 targeted validation after the observed evidence record is added.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | The CR-002 resolution attempt correctly refuses to fake visual evidence, but R22/R26 remain unsatisfied. |
| Test coverage | concern | Static and CI validation are recorded, but TSC013 still lacks accepted full-shell evidence for M2. |
| Edge cases | block | EC15/new mismatch cannot be assessed because the whole shell was not observed. |
| Error handling | pass | The evidence artifact now safely reports unavailable evidence instead of claiming an unobserved pass. |
| Architecture boundaries | pass | The resolution stays in lifecycle/evidence artifacts and does not alter runtime architecture. |
| Compatibility | pass | No app behavior, persistence, settings, or migration surface changed. |
| Security/privacy | pass | The updated evidence record does not expose paths, filenames, screenshots, terminal output, or secrets. |
| Derived artifact currency | concern | The plan/change records correctly keep M2 resolution-needed; the remaining gap is accepted visual evidence. |
| Unrelated changes | pass | Review scope is limited to M2 evidence and lifecycle records; untracked `hifi-design/` remains outside review. |
| Validation evidence | pass | The plan records focused M2 test reruns and `scripts/ci.ps1` after the evidence record was corrected. |

## Recommended Next Stage

Stay in `review-resolution` for CR-002, but this tool session cannot complete that resolution without a WinUI desktop observation or screenshot capture path. M2 remains `resolution-needed`; downstream M3 implementation and final closeout must not proceed from this review.
