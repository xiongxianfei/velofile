# Code Review R2: M2 WinUI Token Resources and File-List Row Redesign

## Review status

changes-requested

## Review inputs

- Diff range: `edb1a7e^..edb1a7e`
- Review surface: M2 commit `edb1a7e` plus current tracked governing artifacts
- Spec: `specs/ui-design-system-shell-redesign.md`
- Test spec: `specs/ui-design-system-shell-redesign.test.md`
- Plan: `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md`
- Validation evidence: M2 validation recorded in `docs/changes/2026-05-11-ui-design-system-shell-redesign/change.yaml`

## Diff summary

M2 adds checked-in WinUI token dictionaries under `src/VeloFile.App/Resources/Tokens/`, adds `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml`, merges those dictionaries from `App.xaml`, and replaces the inline `FileListSurface` row template in `MainWindow.xaml` with `VfFileListRowTemplate` and `VfFileListItemContainerStyle`. It also adds static app tests for resource merging, row resource consumption, preserved row bindings, and no custom row control or behavior model.

## Findings

### CR-M2-001: File-list selected/focused states are not governed by first-slice resources

- Severity: major
- Evidence: `specs/ui-design-system-shell-redesign.md` requires the first redesigned region to include file-list rows and selection/focus states (R48), selected and focused states to be visually distinguishable (R54), visible keyboard focus on rows (A11Y1), and selected/focused row states distinguishable by more than text color alone (A11Y2). The test spec's T007 explicitly requires row height, padding, text styles, thumbnail fallback size, focus/selection resources, and hidden/protected styling to resolve from named resources. The implemented `VfFileListItemContainerStyle` in `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml` only sets `MinHeight`, `HorizontalContentAlignment`, and `UseSystemFocusVisuals` (lines 7-10). The dictionary defines `VfFocusBrush`, `VfFocusThickness`, and `VfBrushSurfaceSelected` in token files, but `VeloFile.FileList.xaml` does not consume any `VfFocus*` resource or `VfBrushSurfaceSelected`; the only focus/selection-related implementation is `UseSystemFocusVisuals="True"`. The new tests assert named row resources and preserved bindings, but they do not assert focus/selection resource consumption or direct proof that selected and focused states meet R54/A11Y1-A11Y2.
- Required outcome: The M2 file-list row resources must either govern selected/focused row visuals through named first-slice resources, or explicitly record and verify a scoped Windows-native/system-focus decision that proves the default WinUI selected/focused visuals satisfy R54, A11Y1, and A11Y2 while preserving high-contrast/system behavior.
- Safe resolution path: Extend the file-list component resources and tests so `VfFileListItemContainerStyle` or associated resources consume the accepted focus/selection tokens, or add a documented design deviation for relying on WinUI system focus/selection visuals plus targeted tests/manual evidence proving the selected/focused distinction. Keep the fix scoped to M2 resources/tests; do not introduce a custom row control, selection system, fixture mode, or screenshot baseline work.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | R48/R54/A11Y1/A11Y2 and T007 selection/focus resource expectations are not fully satisfied by the current file-list resources. |
| Test coverage | concern | `FileListResourceContractTests` covers dictionary merge, row template extraction, bindings, and no custom row control, but not selection/focus resource consumption or direct selected/focused-state proof. |
| Edge cases | concern | Named selected/focused and selected+focused row states have no direct M2 proof. Hidden/protected input remains available through `RowOpacity`, but visual proof is deferred. |
| Error handling | pass | No new runtime error path or parser path is introduced by M2. |
| Architecture boundaries | pass | Resources live in the app resource tree; no WinUI runtime dependency was added to the static validator; no Core/Windows boundary moved. |
| Compatibility | pass | Existing file-list command, selection, drag/drop, and context-menu routes remain wired in `MainWindow.xaml` and code-behind tests. |
| Security/privacy | pass | No fixture data, user paths, telemetry, uploads, secrets, or diagnostics payloads were introduced. |
| Derived artifact currency | pass | No generated token pipeline or generated derived artifacts are introduced. |
| Unrelated changes | pass | Diff is scoped to M2 resources, XAML consumption, tests, and workflow/change records. |
| Validation evidence | concern | Recorded build, validator, targeted tests, and CI evidence are credible, but they do not cover CR-M2-001. |

## Required resolution

Enter `review-resolution` for M2. Do not start M3 until CR-M2-001 is resolved and M2 returns to `code-review`.
