# Code Review R11: M4 Command Band Visual Coherence

## Result

changes-requested

## Review Inputs

- Reviewed milestone: M4 Command Band Visual Coherence.
- Review surface: committed implementation in `9699b23`.
- Governing spec: `specs/ui-shell-visual-coherence.md`, including R44-R49 and R66.
- Test spec: `specs/ui-shell-visual-coherence.test.md`, including TSC011, TSC013, TSC015, TSC016, and TSC020.
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`, M4 milestone and current handoff summary.
- Architecture: `docs/architecture/system/architecture.md` UI Design Contracts and ADR 0010 shell visual-coherence contracts.
- Change record: `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`.
- Visual evidence folder: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/`.

## Diff Summary

- Added `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml` and merged it through `App.xaml`.
- Scoped the navigation/path area and filter/search/status row in `MainWindow.xaml` under `shell-command-band` markers and applied command-band styles.
- Activated `shell-command-band` in `docs/ui/ui-contract-scopes.v1.json` and mirrored the governed fixtures.
- Added `CommandBandResourceContractTests` and updated app/corpus contract tests for command-band resource usage and active scope metadata.
- Updated plan/change records to state that M4 code/static validation passed but M4 visual evidence remains missing.

## Findings

### CR-005: M4 lacks required command-band full-shell visual evidence

- Severity: major
- Location: `docs/plans/2026-05-11-ui-shell-visual-coherence.md:224`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:230`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:706`, `specs/ui-shell-visual-coherence.test.md:236`, `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml:51`
- Evidence:
  - The M4 validation plan requires current full-shell `shell-filter-active` and `shell-search-active` evidence for `shell-standard-1440x900-100`, or manual screenshot review notes if automation is not stable.
  - M4 closeout requires both `shell-filter-active` and `shell-search-active` evidence.
  - TSC013 requires those M4 states before closeout.
  - The current plan handoff says M4 is `visual-evidence-needed` and is not ready for code review because those states are missing.
  - The visual-evidence folder contains only `m2-shell-default.md` and `m3-shell-file-list-selected-focused.md`; there is no M4 `shell-filter-active` or `shell-search-active` evidence record.
- Required outcome:
  - Record accepted M4 full-shell visual evidence for `shell-filter-active` and `shell-search-active` at `shell-standard-1440x900-100`, or record accepted manual full-shell visual-review notes for those states. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Safe resolution path:
  - Capture or manually review both M4 command-band states in the required profile.
  - Add evidence records under `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/` with reviewer/date/profile/state/result/deviation status.
  - Update the plan and change record to move M4 back to `review-requested`.
  - Rerun the targeted M4 validation commands, then rerun code review.

### CR-006: `change.yaml` has duplicate `implementation.validation` keys

- Severity: major
- Location: `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml:34`, `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml:42`
- Evidence:
  - `change.yaml` defines `implementation.validation` twice.
  - The first list records the current M4 command-band validation.
  - The second list records older M3/file-list validation, so ordinary YAML consumers can treat the second key as overriding the first and hide the M4 validation evidence.
- Required outcome:
  - Make the change record unambiguous: `implementation.validation` must have one current M4 validation list, and any older M3 validation must be moved to an explicitly named historical field or removed from the current M4 implementation record.
- Safe resolution path:
  - Consolidate `implementation.validation` into one parseable list for M4.
  - Preserve older M3 validation only under a non-conflicting key if it is still needed for history.
  - Run `git diff --check` and, if available, a YAML parse/duplicate-key check before returning M4 to code review.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | R44-R49 command-band static/resource direction appears represented, but R66/TSC013/plan evidence gates are not satisfied for M4. |
| Test coverage | concern | Static command-band contract and route-preservation tests were added, but the required full-shell visual evidence tests/records are absent. |
| Edge cases | concern | Active filter/search visual states are the named M4 states and have not been reviewed in whole-shell context. |
| Error handling | pass | The reviewed diff does not add new command execution or error-handling code paths. |
| Architecture boundaries | pass | The implementation stays in App XAML/resources, UI contract scopes, fixtures, and tests. |
| Compatibility | pass | Existing back/forward/up/refresh/path/filter/search/cancel/clear handlers remain wired in `MainWindow.xaml` and app contract tests. |
| Security/privacy | pass | No private screenshot artifacts or local user paths were added in the M4 visual-evidence folder. |
| Derived artifact currency | concern | `change.yaml` has duplicate `implementation.validation` keys, making the change record ambiguous for validation consumers. |
| Unrelated changes | pass | The reviewed implementation surface is scoped to M4 command-band resources, UI contract metadata, fixtures, tests, and change records. |
| Validation evidence | concern | The plan records passing static/CI validation, but required M4 visual evidence is explicitly missing and the change record duplicates the validation key. |

## Reviewer Validation

- `git status --short`: clean before review-record edits.
- `git log -1 --oneline`: `9699b23 M4: Style command band pending visual evidence`.
- `git show --name-status --oneline --no-renames HEAD`: inspected the committed M4 surface.
- `Get-ChildItem docs\changes\2026-05-11-ui-shell-visual-coherence\visual-evidence`: confirmed only M2 and M3 evidence records exist.
- `rg` checks against the plan, spec, test spec, architecture, and change record confirmed the M4 evidence requirement and missing state.
- `git diff --check HEAD`: passed before review-record edits.

No full app tests or CI were rerun during this review because the review is already stopped by missing required visual evidence and an ambiguous change record.

## Next Stage

Enter review-resolution for CR-005 and CR-006. M4 remains open and cannot close until accepted `shell-filter-active` and `shell-search-active` full-shell evidence is recorded and `change.yaml` has a single unambiguous current validation field.
