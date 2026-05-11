# Code Review R7

## Status

blocked

## Reviewed Milestone

M2. Shell Surface Foundation

## Review Inputs

- Latest evidence correction commit: `2f6003e M2: clarify unavailable visual evidence`
- Prior blocked review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r6.md`
- M2 visual evidence record: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`
- CR-002 resolution record: `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Architecture and ADR:
  - `docs/architecture/system/architecture.md`
  - `docs/adr/0010-shell-visual-coherence-contracts.md`
- Working tree state: only untracked `hifi-design/` is outside the reviewed diff.

## Diff Summary

The latest commit updates `m2-shell-default.md` to record that the app was launched during a bounded capture diagnostic and that the app exposed a visible `VeloFile` window. It also records why the attempt is not accepted evidence: the local desktop was at `150%` scale instead of the required `100%`, the first capture targeted the foreground browser, and invalid generated screenshot/sidecar outputs were discarded.

## Findings

### CR-002: M2 still lacks accepted full-shell visual evidence

- Severity: major
- Status: unresolved; blocked
- Evidence:
  - Spec R22 requires region-slice full-shell screenshots to show no new mismatch or record a deviation: `specs/ui-shell-visual-coherence.md:127`.
  - Spec R26 requires shell surface foundation compatibility in the default shell screenshot: `specs/ui-shell-visual-coherence.md:137`.
  - Test spec TSC013 requires M2 `shell-default` full-shell evidence before milestone closeout and explicitly names the `shell-standard-1440x900-100` profile for default shell review: `specs/ui-shell-visual-coherence.test.md:231`, `specs/ui-shell-visual-coherence.test.md:393`.
  - The updated evidence note states `Required profile used: no`, `Whole shell visible: not accepted as observed evidence`, no screenshot or sidecar captured, and the conclusion says M2 evidence is not accepted: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`.
- Required outcome:
  - Record accepted `shell-default` full-shell visual evidence for `shell-standard-1440x900-100`, either as a valid current screenshot with sidecar, a manual screenshot review, or an observed manual full-shell visual-review note. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Safe resolution path:
  - Use a reviewer environment that can observe the app at the required effective profile and 100% scale, or record an approved manual visual review that truthfully covers the required profile.
  - Update `m2-shell-default.md` with the observed whole-shell result and deviation status.
  - Rerun the M2 focused validation and CI after accepted evidence is recorded.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | The evidence note is now clearer, but it explicitly says the required profile was not used and evidence is not accepted. |
| Test coverage | concern | Static validation and prior CI remain recorded; TSC013 still lacks accepted full-shell evidence. |
| Edge cases | block | New redesigned/non-redesigned mismatch cannot be assessed because the whole shell was not validly reviewed. |
| Error handling | pass | The invalid capture output was discarded and not represented as accepted evidence. |
| Architecture boundaries | pass | The latest diff changes only visual evidence documentation. |
| Compatibility | pass | No runtime behavior, settings, persistence, migration, or platform adapter behavior changed. |
| Security/privacy | pass | No screenshot, local file content, paths from the rendered UI, secrets, or preview text were committed. |
| Derived artifact currency | concern | Change records correctly keep M2 in `resolution-needed`; accepted visual evidence is still absent. |
| Unrelated changes | pass | The reviewed diff is limited to `m2-shell-default.md`; untracked `hifi-design/` remains outside review. |
| Validation evidence | concern | `git diff --check`/`git diff --cached --check` are sufficient for the docs correction, but they cannot satisfy visual evidence acceptance. |

## Recommended Next Stage

Stop on the existing blocker. M2 remains `resolution-needed`; the next action is still CR-002 review-resolution in an environment that can produce or observe valid `shell-standard-1440x900-100` full-shell evidence.
