# Code Review R4

## Review status

changes-requested

## Review inputs

- Diff range: `HEAD~1..HEAD` (`ab8410c M3: add guarded UI fixture mode`)
- Review surface: M3 fixture launch/registry implementation, M3 app tests, active plan/change records
- Tracked governing branch state: spec, test spec, architecture, ADR 0009, and plan are tracked in the current branch
- Spec: `specs/ui-design-system-shell-redesign.md`
- Test spec: `specs/ui-design-system-shell-redesign.test.md`
- Plan milestone: `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`, M3
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md`
- Validation evidence: M3 validation notes in the active plan and `change.yaml`

## Diff summary

M3 adds `UiFixtureLaunch` parsing/gating, `UiFixtureRegistry` deterministic fixture definitions, fixture startup routing in `App.xaml.cs`, fixture view-model creation in `AppCompositionRoot`, app test project links, focused fixture tests, and workflow record updates.

## Findings

### CR-M3-001 - Deterministic selected/focused fixture states are labels only

- Severity: major
- Evidence: `UiFixtureRegistry` records selected/focused/multi-selected states only as `UiFixtureRowState` values on `UiFixtureRow` (`src/VeloFile.App/Testing/UiFixtureRegistry.cs:14`, `src/VeloFile.App/Testing/UiFixtureRegistry.cs:36`, `src/VeloFile.App/Testing/UiFixtureRegistry.cs:130`). `CreateViewModel` discards that state when it converts rows into `FileSystemEntrySnapshot` values and constructs `AppShellViewModel` (`src/VeloFile.App/Testing/UiFixtureRegistry.cs:87`, `src/VeloFile.App/Testing/UiFixtureRegistry.cs:113`, `src/VeloFile.App/Testing/UiFixtureRegistry.cs:119`). The app selection path is still driven by `FileListSurface.SelectedItems` in `MainWindow.xaml.cs`, but M3 does not apply fixture selection or focus to that surface.
- Requirement impact: M3 is supposed to populate deterministic rows/states for selected, focused, selected-and-focused, and multi-selected fixture evidence. The spec requires deterministic view-model or app-shell fixtures for first visual baselines (R62), and ADR 0009 says allowed fixtures render deterministic app-shell or view-model states. As implemented, launching `file-list-v1` can list rows named `selected-report.docx` or `keyboard-focus.md`, but those rows will render as ordinary unselected/unfocused rows until an external actor changes UI selection/focus.
- Required outcome: The fixture launch path must make the selected, focused, selected+focused, and multi-selected states deterministic through the app/UI boundary used for screenshots, or M3 must explicitly split those states into separate allowed fixtures/fixture instructions that the screenshot harness can apply deterministically before capture.
- Safe resolution path: Keep the fix in the app presentation/testing layer. Add a hardcoded fixture presentation state, expose selected/focused fixture targets from the registry, and apply them only for accepted fixture launches without changing normal selection behavior. Add tests that fail if fixture row state metadata is not consumed by the rendered app/startup path. Do not introduce a new production selection system, custom row control, screenshot baselines, or disk-backed fixtures in this resolution.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | CR-M3-001: R62/ADR deterministic visual states are not actually rendered through fixture launch. |
| Test coverage | concern | Tests assert enum labels and launch gating, but do not prove selected/focused/multi-selected state consumption by the app surface. |
| Edge cases | concern | Release, missing env guard, unknown fixture, and arbitrary path cases are tested; selected/focused fixture visual-state edge cases are not. |
| Error handling | pass | Parser/gate reject production, missing env guard, unknown fixture, unsupported options, and arbitrary fixture arguments with nonzero result semantics. |
| Architecture boundaries | pass | Fixture code stays in App/testing layer and does not touch Core/Windows adapters or add disk-backed fixtures. |
| Compatibility | pass | Normal launch without fixture flag still routes through `CreateShellViewModel`; no persistence migration or `DebugUiTest` config is added. |
| Security/privacy | pass | Fixture data is synthetic, rooted at `C:\VeloFileFixture`, hardcoded, and arbitrary fixture-data paths are rejected. |
| Derived artifact currency | pass | No generated visual baselines or sidecars are introduced in M3. |
| Unrelated changes | pass | Diff is scoped to M3 fixture code, tests, and workflow records. |
| Validation evidence | concern | Recorded validation is relevant, but it does not catch CR-M3-001 because tests only verify fixture state labels. |

## Recommended next stage

Enter `review-resolution` for M3. Keep the fix scoped to fixture presentation state and tests. M4 should not begin until CR-M3-001 is resolved and M3 returns to `code-review`.
