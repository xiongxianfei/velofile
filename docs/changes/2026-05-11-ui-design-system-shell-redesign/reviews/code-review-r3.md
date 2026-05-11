# Code Review R3: M2 Review-Resolution Rerun

## Review status

changes-requested

## Review inputs

- Diff range: `0c9bf64..411e7f0`
- Review surface: M2 implementation commit `edb1a7e`, M2 review record `code-review-r2`, and M2 resolution commit `411e7f0`
- Spec: `specs/ui-design-system-shell-redesign.md`
- Test spec: `specs/ui-design-system-shell-redesign.test.md`
- Plan: `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md`
- Validation evidence: M2 and M2 review-resolution validation recorded in `docs/changes/2026-05-11-ui-design-system-shell-redesign/change.yaml`

## Diff summary

M2 adds first-slice WinUI token dictionaries, merges them through `App.xaml`, extracts the file-list row presentation into `Resources/Components/VeloFile.FileList.xaml`, and replaces the inline `FileListSurface` row template with named row template and item-container resources. The review-resolution commit adds named selected/focused row resources, wires WinUI `ListViewItemBackground*` resources in the scoped file-list region, and adds static tests for selection/focus resource consumption.

## Findings

### CR-M2-002: Hidden/protected row opacity still bypasses first-slice state resources

- Severity: major
- Evidence: Example E2 in `specs/ui-design-system-shell-redesign.md` requires first-slice hidden/protected state styling to come from VeloFile resources rather than local row literals. T007 in `specs/ui-design-system-shell-redesign.test.md` requires hidden/protected styling to resolve from named resources. The M2 resolution defines `VfFileListRowHiddenOpacity` and `VfFileListRowProtectedOpacity` in `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml` lines 13-14, but the actual row template still renders opacity through `Opacity="{Binding RowOpacity}"` at line 47. That bound value is produced by `src/VeloFile.App/ViewModels/FileListRowViewModel.cs` line 34 as a hardcoded `0.58`, while the accepted token contract defines `VfState.HiddenOpacity` / `VfStateHiddenOpacity` as `0.68`. The new tests only assert the resources exist and that dimmed row input remains available; they do not prove hidden/protected styling resolves from the named first-slice resources.
- Required outcome: Hidden/protected file-list row styling must be governed by the accepted first-slice resources, or the implementation must record an explicit accepted deviation explaining why the row-view-model opacity remains the production authority. The direct proof should fail if `VfFileListRowHiddenOpacity` / `VfStateHiddenOpacity` are unused or drift from the rendered hidden/protected row state.
- Safe resolution path: Keep the fix inside M2 file-list row resources/tests. Make the hidden/protected opacity path consume the named state resource without adding fixture mode, screenshots, a custom row control, or a new selection/listing behavior model. Add a targeted `FileListResourceContractTests` assertion that hidden/protected styling resolves from named resources and that the stale `0.58` hardcoded visual value cannot remain the rendered first-slice hidden/protected state.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | Selection/focus governance from CR-M2-001 is resolved, but E2/T007 hidden/protected resource governance remains unsatisfied by the rendered row path. |
| Test coverage | concern | Static tests cover selected/focused resources and row input availability, but not hidden/protected resource consumption or drift against `VfStateHiddenOpacity`. |
| Edge cases | concern | Hidden/protected visual distinction is still driven by a hardcoded view-model opacity value instead of the accepted state token. |
| Error handling | pass | No new runtime error path is introduced by the M2 resolution. |
| Architecture boundaries | pass | The implementation remains in app XAML/resources/tests and does not move Core/Windows boundaries. |
| Compatibility | pass | Existing file-list command, selection, drag/drop, and context-menu routes remain wired. |
| Security/privacy | pass | No fixture data, user paths, telemetry, uploads, or secrets are introduced. |
| Derived artifact currency | pass | No generated token pipeline or generated derived artifact is introduced. |
| Unrelated changes | pass | The reviewed M2 diff remains scoped to UI contract artifacts, WinUI resources, app XAML consumption, tests, and lifecycle records. |
| Validation evidence | concern | Recorded build, targeted tests, validator, and CI evidence are credible, but they do not cover CR-M2-002. |

## Required resolution

Enter `review-resolution` for M2. Do not start M3 until CR-M2-002 is resolved and M2 returns to `code-review`.
