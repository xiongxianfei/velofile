# Code Review R4

## Status

changes-requested

## Reviewed Milestone

M2. Shell Surface Foundation

## Review Inputs

- Implementation commit: `53d0df0 M2: apply shell surface foundation`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`
- App resources and XAML:
  - `src/VeloFile.App/App.xaml`
  - `src/VeloFile.App/MainWindow.xaml`
  - `src/VeloFile.App/Resources/Components/VeloFile.Shell.xaml`
- Tests:
  - `tests/VeloFile.App.Tests/UiDesign/ShellSurfaceResourceContractTests.cs`
  - `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`
- Visual evidence note:
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`
- Validation evidence recorded in the active plan and change record.

## Diff Summary

M2 adds `VeloFile.Shell.xaml`, merges it through `App.xaml`, activates the `shell-surface-foundation` UI contract scope, applies shell surface styles to the existing root/chrome/command/sidebar/content/status/preview containers in `MainWindow.xaml`, updates App and Corpus tests, updates valid UI-contract fixtures, and records a soft `shell-default` visual-evidence note.

## Findings

### CR-002: M2 visual evidence note does not record an actual full-shell visual review

- Severity: major
- Evidence:
  - Spec R22 requires full-shell screenshots for a region slice to show no new redesigned/non-redesigned mismatch, or record a deviation: `specs/ui-shell-visual-coherence.md:127`.
  - Spec R26 requires shell foundation compatibility in the default shell screenshot: `specs/ui-shell-visual-coherence.md:137`.
  - Test spec TSC013 requires M2 `shell-default` full-shell evidence and says screenshots should show the whole shell or accepted mismatches should have deviation records: `specs/ui-shell-visual-coherence.test.md:231`.
  - The plan allows either a current `shell-default` screenshot or manual screenshot review note for M2, and also requires full-shell visual evidence or an explicit manual visual-review note with reason: `docs/plans/2026-05-11-ui-shell-visual-coherence.md:125`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:132`, `docs/plans/2026-05-11-ui-shell-visual-coherence.md:135`.
  - The M2 evidence file records that no current PNG or sidecar baseline was captured and only cites static evidence; it does not state that the app was launched, the whole shell was visually reviewed, who reviewed it, what was observed, or whether the default shell actually has no visual mismatch: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md:16`, `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md:20`.
- Required outcome:
  - Before M2 can close, record actual `shell-default` full-shell visual evidence for `shell-standard-1440x900-100`. This can be an automated/current screenshot, a manual screenshot review, or a manual visual-review note that records an observed whole-shell review result. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Safe resolution path:
  - Launch the app in the required M2 state/profile if possible and capture or review the whole shell.
  - Update `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md` with reviewer/date, profile, observed result, whether the whole shell was visible, and deviation status; include screenshot/sidecar if automation becomes available.
  - If the shell cannot be launched in this environment, explicitly mark the evidence as unavailable and keep M2 in `resolution-needed` rather than closing it.
  - Rerun M2 targeted validation after the evidence record is corrected.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | Resource and XAML changes align with R23-R25/R27, but R22/R26 visual evidence is not satisfied. |
| Test coverage | pass | `ShellSurfaceResourceContractTests` cover dictionary merge, tokenized shell resources, scoped resource references, and route presence. |
| Edge cases | concern | EC15/new mismatch cannot be assessed without actual screenshot or observed full-shell review evidence. |
| Error handling | pass | No new runtime error paths are introduced; validator fixture updates preserve active-scope validation. |
| Architecture boundaries | pass | Changes stay inside App resources/XAML, tests, fixtures, and lifecycle docs; Core/Windows boundaries are unchanged. |
| Compatibility | pass | No persistence, schema, settings, or adapter migration is introduced. |
| Security/privacy | pass | The visual evidence note uses synthetic metadata and does not include local paths, secrets, terminal commands, or preview text. |
| Derived artifact currency | concern | Plan/change records are updated for M2 handoff, but visual evidence content is insufficient for closeout. |
| Unrelated changes | pass | Reviewed diff is scoped to M2 shell foundation and lifecycle artifacts; untracked `hifi-design/` remains outside review. |
| Validation evidence | pass | Recorded validation includes focused App tests, UI contract validation, filtered solution tests, build, and `scripts/ci.ps1`. |

## Recommended Next Stage

Enter `review-resolution` for CR-002. M2 should remain `resolution-needed` until actual full-shell visual evidence or a proper observed manual visual-review note is recorded.
