# Code Review R12: M4 Review-Resolution Recheck

## Result

changes-requested

## Review Inputs

- Reviewed milestone: M4 Command Band Visual Coherence.
- Review surface: current working tree after CR-006 review-resolution edits.
- Governing spec: `specs/ui-shell-visual-coherence.md`, including R44-R49 and R66.
- Test spec: `specs/ui-shell-visual-coherence.test.md`, including TSC013 and TSC020.
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`, M4 milestone and current handoff summary.
- Prior review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r11.md`.
- Resolution record: `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`.
- Change record: `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`.
- Visual evidence folder: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/`.

## Diff Summary

- Added the R11 M4 code-review record and indexed it in `review-log.md`.
- Updated `review-resolution.md` to mark CR-005 open and CR-006 resolved.
- Updated `change.yaml` so M4 has one current `implementation.validation` list and older M3 validation is stored under `history.prior_milestone_validation.M3`.
- Updated the active plan and plan index to reflect that M4 remains blocked on CR-005 visual evidence.

## Findings

### CR-005: M4 still lacks required command-band full-shell visual evidence

- Severity: major
- Location: `docs/plans/2026-05-11-ui-shell-visual-coherence.md:224`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:230`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:513`, `specs/ui-shell-visual-coherence.test.md:236`, `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md:13`
- Evidence:
  - The M4 plan still requires `shell-filter-active` and `shell-search-active` full-shell evidence for `shell-standard-1440x900-100`.
  - TSC013 requires M4 `shell-filter-active` and `shell-search-active` evidence before closeout.
  - The current visual-evidence directory contains only `m2-shell-default.md` and `m3-shell-file-list-selected-focused.md`.
  - `review-resolution.md` correctly records CR-005 as open and says M4 cannot pass code review until those evidence records exist.
- Required outcome:
  - Add accepted M4 full-shell visual evidence or accepted manual full-shell visual-review notes for both `shell-filter-active` and `shell-search-active`. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Safe resolution path:
  - Record `m4-shell-filter-active.md` and `m4-shell-search-active.md` under `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/`.
  - Each note must identify reviewer/date/profile/state, state whether the required profile was used, state whether the whole shell was visible, state whether evidence was accepted, and identify deviation status.
  - Update `review-resolution.md`, `change.yaml`, the active plan, and `docs/plan.md` to return M4 to `review-requested`.
  - Rerun focused M4 validation and code review.

## Resolved Finding Confirmation

### CR-006: `change.yaml` duplicate validation key

- Status: resolved
- Evidence:
  - `change.yaml` has one current `implementation.validation` key.
  - Older M3 validation is under `history.prior_milestone_validation.M3`.
  - The focused duplicate-key check found exactly one `^  validation:` entry.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | R66 and the M4 plan evidence gate remain unsatisfied because `shell-filter-active` and `shell-search-active` evidence is absent. |
| Test coverage | concern | Static tests and CI are recorded, but TSC013's region-slice evidence requirement is still missing for M4. |
| Edge cases | concern | Active filter/search command-band states are the named M4 visual states and are not yet reviewed in whole-shell context. |
| Error handling | pass | The reviewed resolution changes are metadata/evidence tracking only. |
| Architecture boundaries | pass | Changes remain in review records, plan metadata, and change metadata. |
| Compatibility | pass | No production command-band behavior was changed in this resolution slice. |
| Security/privacy | pass | No new screenshots or private-path evidence were added. |
| Derived artifact currency | pass | CR-006 is resolved: `change.yaml` now has one current M4 validation key and unambiguous M3 history. |
| Unrelated changes | pass | The reviewed changes are scoped to M4 review-resolution records and metadata. |
| Validation evidence | concern | `git diff --check` and the duplicate-key check passed, but accepted M4 visual evidence is still missing. |

## Reviewer Validation

- `git status --short`: inspected current modified/untracked review-resolution files.
- `git log -1 --oneline`: `9699b23 M4: Style command band pending visual evidence`.
- `Get-ChildItem docs\changes\2026-05-11-ui-shell-visual-coherence\visual-evidence`: confirmed only M2 and M3 evidence records exist.
- `Select-String -Path docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml -Pattern '^  validation:'`: found exactly one current `implementation.validation` key at line 34.
- `git diff --check`: passed with CRLF normalization warnings only.

No full app tests or CI were rerun during this re-review because the only unresolved finding is missing required visual evidence.

## Next Stage

Continue review-resolution for CR-005. M4 remains open and cannot return to `review-requested` until the required `shell-filter-active` and `shell-search-active` evidence records are added and accepted.
