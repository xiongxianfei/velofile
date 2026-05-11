# Code Review R6

## Status

blocked

## Reviewed Milestone

M2. Shell Surface Foundation

## Review Inputs

- Latest implement/blocker commit: `914929d M2: record visual capture blocker`
- Prior blocked review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r5.md`
- CR-002 resolution record: `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`
- M2 visual evidence record: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Architecture and ADR:
  - `docs/architecture/system/architecture.md`
  - `docs/adr/0010-shell-visual-coherence-contracts.md`
- Working tree state: only untracked `hifi-design/` is outside the reviewed diff.

## Diff Summary

The latest commit updates the CR-002 resolution record and active plan to document a bounded local capture attempt. The attempt launched the WinUI app and confirmed a visible `VeloFile` window, but the local desktop reported `150%` scale instead of the required `100%` profile, and the attempted screenshot did not produce valid `shell-standard-1440x900-100` evidence. The invalid generated current screenshot artifacts were discarded.

## Findings

### CR-002: M2 still lacks accepted full-shell visual evidence

- Severity: major
- Status: unresolved; blocked
- Evidence:
  - Spec R22 requires region-slice full-shell screenshots to show no new mismatch or record a deviation: `specs/ui-shell-visual-coherence.md:127`.
  - Spec R26 requires shell surface foundation compatibility in the default shell screenshot: `specs/ui-shell-visual-coherence.md:137`.
  - Test spec TSC013 requires M2 `shell-default` full-shell evidence before milestone closeout, and the required review profile includes `shell-standard-1440x900-100`: `specs/ui-shell-visual-coherence.test.md:231`, `specs/ui-shell-visual-coherence.test.md:393`.
  - The latest plan note states the attempted capture ran at `150%` scale, first captured the foreground browser instead of VeloFile, and discarded the invalid generated screenshot/sidecar outputs: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
  - The M2 evidence record still states that no accepted screenshot, sidecar, or observed full-shell review exists: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`.
- Required outcome:
  - Record accepted `shell-default` full-shell visual evidence for `shell-standard-1440x900-100`, either as a valid current screenshot with sidecar or as an observed manual full-shell visual-review note. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Safe resolution path:
  - Use a reviewer environment that can run the app at 100% scale, or an approved manual reviewer who can observe the app at the required effective profile.
  - Update `m2-shell-default.md` with the observed whole-shell result and deviation status.
  - Rerun the M2 targeted validation after valid evidence is recorded.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | The latest commit records the capture blocker honestly, but R22/R26 remain unsatisfied. |
| Test coverage | concern | Static validation remains recorded; TSC013 still lacks accepted M2 full-shell evidence. |
| Edge cases | block | The whole-shell mismatch case cannot be assessed without valid screenshot or observed manual review evidence. |
| Error handling | pass | Invalid generated visual artifacts were discarded instead of being recorded as evidence. |
| Architecture boundaries | pass | The change is limited to lifecycle/evidence records and does not alter runtime shell code. |
| Compatibility | pass | No product behavior, settings, persistence, or migration behavior changed. |
| Security/privacy | pass | The record does not commit screenshots or expose local file content, filenames, secrets, or preview text. |
| Derived artifact currency | concern | The plan and resolution record now agree that CR-002 is blocked; the accepted evidence artifact is still missing. |
| Unrelated changes | pass | The reviewed diff only updates M2 lifecycle records; untracked `hifi-design/` remains outside review. |
| Validation evidence | concern | `git diff --check` and `git diff --cached --check` are appropriate for the docs update, but they cannot prove visual evidence acceptance. |

## Recommended Next Stage

Stop on the existing blocker. M2 remains `resolution-needed`; do not proceed to M3 until CR-002 has accepted full-shell visual evidence and a clean M2 code-review rerun.
