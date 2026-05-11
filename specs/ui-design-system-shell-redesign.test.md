# UI Design System and Shell Redesign Test Spec

## Status

active

This test spec is the active proof surface for the approved first-slice UI design-system and shell redesign. It is not implementation evidence by itself; implementation must add the tests and validation artifacts described here.

## Related Spec and Plan

- Feature spec: [ui-design-system-shell-redesign.md](ui-design-system-shell-redesign.md)
- Execution plan: [2026-05-11-ui-design-system-shell-redesign.md](../docs/plans/2026-05-11-ui-design-system-shell-redesign.md)
- Architecture: [architecture.md](../docs/architecture/system/architecture.md)
- ADR: [ADR 0009](../docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md)

## Testing Strategy

The proof strategy is layered:

- Contract tests validate `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, and `docs/ui/design-deviations.md`.
- Tool tests validate `tools/VeloFile.UiContracts` with controlled valid and invalid XAML fixture dictionaries before production resources exist.
- Static XAML tests validate production WinUI resource dictionaries, merged app resources, named file-list resources, and scoped literal rules.
- App-shell tests validate fixture-mode guard behavior and deterministic row-state fixture data without treating fixture-only checks as release evidence for filesystem integration.
- Script tests validate visual baseline update behavior, sidecar metadata, generated-output ignore rules, and no CI baseline mutation.
- Manual/review checks validate first-slice screenshot readability and layout quality while screenshot comparison remains soft evidence.
- Existing V1 behavior tests remain the regression surface for navigation, listing, selection, search, preview, drag/drop, file operations, persistence, terminal, diagnostics, and accessibility.

## Requirement Coverage Map

| Requirement | Tests / verification |
|---|---|
| R1-R8 | T001, T014 |
| R9-R14 | T006, T007, T008, T018 |
| R15-R19 | T001, T002, T003 |
| R20-R24 | T002, T003 |
| R25 | T001, T015 |
| R26-R35 | T006, T007 |
| R36-R39 | T001, T004 |
| R40-R44 | T003, T004, T006, T007 |
| R45-R47 | T005 |
| R48-R53 | T006, T007, T008 |
| R54-R57 | T008, T018 |
| R58-R61 | T007, T014, T017 |
| R62-R68 | T009, T010, T011, T016 |
| R69-R72 | T011, T012, T018 |
| R73-R79 | T012, T013, T018 |
| R80-R84 | T015 |
| I1-I5 | T001, T014 |
| I6 | T008, T017, T018 |
| I7 | T009, T010 |
| I8-I9 | T012, T013 |
| I10 | T004, T006, T007 |
| C1-C6 | T014, T018 |
| C7 | T001, T014 |
| O1 | T003, T004, T005 |
| O2 | T009, T010 |
| O3-O4 | T012, T013 |
| O5 | T016 |
| S1-S2 | T009, T010, T016 |
| S3-S4 | T011, T012, T016 |
| S5 | T005, T016 |
| A11Y1-A11Y7 | T008, T018 |
| P1-P4 | T007, T008, T017 |
| P5 | T012, T018 |
| P6 | T005 |
| AC1-AC3 | T001, T002, T004, T015 |
| AC4-AC7 | T005, T006, T007 |
| AC8-AC10 | T011, T012, T013 |
| AC11-AC13 | T009, T010 |
| AC14 | T008, T018 |
| AC15 | T014 |

## Example Coverage Map

| Example | Tests / verification |
|---|---|
| E1 | T002, T003 |
| E2 | T006, T007 |
| E3 | T010, T011, T018 |
| E4 | T009 |
| E5 | T012, T013 |
| E6 | T015 |

## Edge Case Coverage

| Edge case | Tests / verification |
|---|---|
| EC1 | T002 |
| EC2 | T002 |
| EC3 | T003 |
| EC4 | T003 |
| EC5 | T003 |
| EC6 | T004, T007 |
| EC7 | T004 |
| EC8 | T007 |
| EC9 | T009 |
| EC10 | T009 |
| EC11 | T009 |
| EC12 | T013 |
| EC13 | T013 |
| EC14 | T012 |
| EC15 | T012 |
| EC16 | T008, T018 |
| EC17 | T008, T018 |
| EC18 | T008, T017, T018 |
| EC19 | T010, T011 |
| EC20 | T001, T015 |

## Test Cases

### Contract and Tooling

T001. UI authority and scope contract
- Covers: R1-R8, R19, R25, R36-R39, C7, I1-I5, EC20, AC1-AC3
- Level: contract
- Fixture/setup: Approved spec, `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`.
- Steps: Inspect contract files for VeloFile-owned names, required top-level fields, first-slice-only scope, dark/comfortable defaults, absence of persisted theme/density settings, and no `hifi-design` source-of-truth references.
- Expected result: Repo-owned contracts exist, are versioned where required, and do not delegate production authority to `hifi-design/`.
- Failure proves: The design system has no reliable repo-owned source of truth or has expanded beyond the approved first slice.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/` or `tests/VeloFile.App.Tests/UiContracts/`.

T002. Token JSON schema and first-slice values
- Covers: R15-R24, EC1-EC2, AC1, E1
- Level: unit | contract
- Fixture/setup: `docs/ui/tokens.v1.json`.
- Steps: Parse JSON; assert `version`, `theme`, `density`, and `tokens`; assert each token has `id`, `xamlKeys`, `type`, `value`, `category`, and `requiredInFirstSlice`; assert all Table T1-T5 token IDs and values are present.
- Expected result: Token contract is machine-readable and exactly represents the first-slice baseline.
- Failure proves: Automated UI conformance cannot trust the token contract.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/TokenContractTests.cs`.

T003. Static token-to-XAML resource validation
- Covers: R40-R42, O1, EC3-EC5, AC5-AC6, E1, ADR 0009
- Level: unit | integration
- Fixture/setup: Controlled valid and invalid XAML resource dictionaries under `tests/fixtures/ui-contracts/`.
- Steps: Run `tools/VeloFile.UiContracts validate-tokens` over valid fixtures and invalid variants for missing keys, wrong types, wrong comparable values, duplicate keys, and brush-to-wrong-color references.
- Expected result: Valid fixtures pass; every invalid fixture exits nonzero with file path, token ID or XAML key, expected value/type, and observed value/type where available.
- Failure proves: The validator cannot protect the token/resource boundary.
- Automation location: `tools/VeloFile.UiContracts` tests or `tests/VeloFile.Corpus.Tests/UiContracts/UiContractsToolTests.cs`.

T004. UI contract scopes and targeted literal rules
- Covers: R36-R44, I10, EC6-EC8, AC2, AC6
- Level: unit | contract
- Fixture/setup: `docs/ui/ui-contract-scopes.v1.json` and controlled XAML fixtures containing first-slice and legacy regions.
- Steps: Validate scope file shape; assert the file-list first-slice scope includes governed files, required resource references, and forbidden literal rules; run fixtures with forbidden row literals inside and outside active scope.
- Expected result: First-slice literals fail inside governed scope; old unrelated literals outside scope do not fail; missing `VfFileListRowTemplate` or `VfFileListItemContainerStyle` fails.
- Failure proves: The checker either misses first-slice drift or blocks unrelated legacy XAML cleanup.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/UiContractScopeTests.cs`.

T005. UI contract tool integration boundary
- Covers: R45-R47, P6, S5, O1, AC5, ADR 0009
- Level: smoke | contract
- Fixture/setup: `VeloFile.sln`, `tools/VeloFile.UiContracts`, controlled XAML fixtures.
- Steps: Restore/build solution; assert tool project is in the solution; run validator against static files; assert no app process is launched and the tool has no WinUI app runtime dependency.
- Expected result: UI contract validation is discoverable through the solution and runs as static local validation.
- Failure proves: The validation boundary is brittle or coupled to runtime UI automation too early.
- Automation location: solution/project tests plus `tests/VeloFile.Corpus.Tests/UiContracts/UiContractsToolBoundaryTests.cs`.

### WinUI Resources and File List

T006. Production resource dictionaries and merged resources
- Covers: R9-R14, R26-R35, R40-R42, AC4
- Level: integration | contract
- Fixture/setup: `src/VeloFile.App/App.xaml`, `src/VeloFile.App/Resources/Tokens/`, `src/VeloFile.App/Resources/Components/`.
- Steps: Build the app; validate production resources with `tools/VeloFile.UiContracts`; inspect merged dictionaries for token dictionaries and `VeloFile.FileList.xaml`; assert static resource use where required and theme-aware lookup only for allowed system/high-contrast cases.
- Expected result: Production resources represent first-slice tokens as typed WinUI resources and are merged for app consumption.
- Failure proves: The app resource bridge does not implement the accepted token contract.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/WinUiResourceContractTests.cs`.

T007. File-list named resource consumption
- Covers: R31-R33, R48-R53, R58-R61, P1-P4, EC6-EC8, AC7, E2
- Level: integration | contract
- Fixture/setup: `MainWindow.xaml`, `VeloFile.FileList.xaml`, app-shell XAML static inspection.
- Steps: Assert `FileListSurface` consumes `VfFileListRowTemplate` and `VfFileListItemContainerStyle`; assert row height, padding, text styles, thumbnail fallback size, focus/selection resources, and hidden/protected styling resolve from named resources; assert no custom row control, new selection behavior model, or virtualization replacement is introduced.
- Expected result: File-list presentation is factored into named resources while the existing row/view-model behavior remains the data source.
- Failure proves: The first slice is an inline XAML restyle or behavior rewrite rather than a design-system slice.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/FileListResourceContractTests.cs`.

T008. File-list row state presentation contract
- Covers: R11-R12, R53-R57, A11Y1-A11Y7, I6, EC16-EC19, AC14
- Level: integration | manual
- Fixture/setup: Deterministic file-list row states or app-shell row view-model fixtures for normal, selected, focused, selected-focused, multi-selected, hidden/protected, thumbnail fallback, long-name, metadata-heavy, and empty-folder states.
- Steps: Verify selected and focused states are distinct; hidden/protected rows remain readable and distinct from disabled rows; thumbnail fallback does not change row height; long names do not overlap metadata; empty state is distinct from loading, failed, and unsupported states.
- Expected result: Row state presentation satisfies the first-slice UX/accessibility contract.
- Failure proves: The redesigned core product surface is ambiguous, inaccessible, or layout-unstable.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/FileListRowStateTests.cs`; visual review evidence in `tests/visual/baselines/winui/dark-comfortable-1440x900-100/`.

T009. Fixture-mode rejection matrix
- Covers: R63-R66, I7, O2, S1-S2, EC9-EC11, AC11-AC13, E4, QS-UI-FIXTURE-01
- Level: integration | smoke
- Fixture/setup: App fixture launch parser/host, Debug/test build, Release build or Release-equivalent compiled path, environment variable variants.
- Steps: Launch or simulate startup with `--test-ui-fixture` in production/Release, Debug without `VELOFILE_ENABLE_TEST_UI_FIXTURES=1`, Debug with unknown fixture, and Debug with env guard plus allowlisted fixture.
- Expected result: All disallowed cases exit nonzero before rendering normal or fixture UI; only allowlisted Debug/test fixture launch with env guard succeeds.
- Failure proves: Hidden fixture mode can leak into production or silently capture the wrong UI.
- Automation location: `tests/VeloFile.App.Tests/UiFixtures/UiFixtureLaunchTests.cs`; manual process check when full app process launch is needed.

T010. Deterministic fixture data and no arbitrary input paths
- Covers: R62-R68, O2, S1-S4, EC19, AC13, E3
- Level: unit | integration
- Fixture/setup: `file-list-v1` fixture registry and fixture row factory.
- Steps: Assert fixture names are hardcoded; assert arbitrary fixture data paths are rejected or unsupported; assert generated rows use synthetic names only and include all required normal/selected/focused/multi-selected/hidden/protected/fallback/long-name/metadata-heavy/empty states.
- Expected result: Fixture data is deterministic, synthetic, non-sensitive, and not user-supplied.
- Failure proves: Visual evidence can leak local data or become nondeterministic.
- Automation location: `tests/VeloFile.App.Tests/UiFixtures/UiFixtureDataTests.cs`.

### Visual Evidence and Scripts

T011. Required visual baseline inventory
- Covers: R69-R72, S3-S4, AC8, E3
- Level: contract | manual
- Fixture/setup: `tests/visual/baselines/winui/dark-comfortable-1440x900-100/`.
- Steps: Assert the nine required screenshot names exist and each has a JSON sidecar with `theme`, `density`, `viewport`, `scale`, `screen`, `fixture`, `dynamicRegions`, and `reviewId`.
- Expected result: Visual evidence inventory is complete for the first profile.
- Failure proves: The first slice lacks the promised reviewed baseline evidence.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/VisualBaselineInventoryTests.cs`.

T012. Visual sidecar metadata and dynamic region policy
- Covers: R70-R78, O3-O4, S3, EC14-EC15, AC8-AC10, E5
- Level: contract
- Fixture/setup: Sidecar JSON files for current and committed visual baseline screenshots.
- Steps: Validate viewport/profile consistency, required metadata fields, declared dynamic regions, absence of raw user paths/usernames/secrets/preview text, and explicit handling for cursor, timestamps, progress, live counts, caret, file timestamps, and non-stable thumbnails.
- Expected result: Sidecars are safe, reviewable, and sufficient to explain comparison assumptions.
- Failure proves: Visual evidence is not reproducible, safe, or auditable.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/VisualSidecarTests.cs`.

T013. Baseline update script guardrails
- Covers: R73-R75, O4, EC12-EC13, AC9-AC10, E5, QS-UI-VISUAL-01
- Level: integration | script
- Fixture/setup: Temporary `tests/visual/current/`, `tests/visual/baselines/`, and `tests/visual/diffs/` roots under an isolated test workspace.
- Steps: Run `scripts/update-ui-baselines.ps1` without `-ReviewId`, without current screenshots, with one reviewed profile, and with generated diffs present; assert refusal or copy/update behavior as applicable.
- Expected result: Missing review id and missing current screenshots fail nonzero; approved current screenshots and sidecars copy to baselines; current/diff outputs are not committed and normal CI does not update baselines.
- Failure proves: Baseline mutation is not deliberate or traceable.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/UpdateUiBaselinesScriptTests.cs` or `tests/validation/UpdateUiBaselines.Tests.ps1`.

T014. Existing V1 behavior regression safety
- Covers: R4-R8, R58-R61, C1-C7, AC15
- Level: regression | integration
- Fixture/setup: Existing Core, App, Windows, and Corpus tests.
- Steps: Run the relevant focused tests for navigation, listing, selection, search, preview, drag/drop, file operations, persistence, terminal, diagnostics, and accessibility after each milestone that changes app code; run broad CI at milestone closeout.
- Expected result: Existing V1 behavior tests remain valid and pass; fixture-only visual evidence is not counted as release evidence for filesystem integration.
- Failure proves: The visual redesign altered product behavior or overclaimed proof.
- Automation location: existing `tests/VeloFile.*.Tests/`; `scripts/ci.ps1`.

T015. Design deviation record policy
- Covers: R80-R84, AC3, E6
- Level: contract | manual
- Fixture/setup: `docs/ui/design-deviations.md` and any implementation slice that intentionally differs from reference material.
- Steps: Assert the document exists with status values; for each meaningful deviation introduced in a slice, assert reference pattern, VeloFile decision, reason, user impact, verification, and status are present; assert no deviation is accepted solely because XAML was easier.
- Expected result: Intentional reference differences are reviewable and not accidental drift.
- Failure proves: The reference package is being copied or ignored without accountable design decisions.
- Automation location: docs contract tests plus manual review during code review.

T016. Security and privacy scan for UI contracts and visual evidence
- Covers: S1-S5, O5
- Level: security | contract
- Fixture/setup: UI contract files, fixture definitions, visual sidecars, generated screenshot metadata.
- Steps: Scan JSON/Markdown/fixture metadata for raw user paths, usernames, secrets, file contents, terminal commands, clipboard contents, preview text, arbitrary fixture data paths, telemetry URLs, or upload behavior.
- Expected result: UI contract tooling and visual evidence remain local-only and synthetic.
- Failure proves: The first slice leaks or uploads user-sensitive information.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/UiDesignPrivacyTests.cs`.

T017. Row rendering performance and motion guardrails
- Covers: R56, R61, P1-P4, I6, EC18
- Level: contract | integration
- Fixture/setup: File-list XAML resources and row view-model tests.
- Steps: Assert row template does not add synchronous filesystem, thumbnail, preview, or metadata calls; assert row height is fixed by token; assert selection/filtering/listing/file-operation states are not animated; assert only hover/focus affordance motion resources are used.
- Expected result: File-list rendering remains virtualization-friendly and row-height stable.
- Failure proves: Visual styling has introduced hot-path work or unstable layout.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/FileListPerformanceContractTests.cs`.

T018. Manual first-slice visual review
- Covers: R11-R14, R53-R57, R69-R78, A11Y1-A11Y7, C6, P5, AC8, AC14
- Level: manual | smoke
- Fixture/setup: App launched in allowed fixture mode at `dark-comfortable-1440x900-100`, reviewed screenshots and sidecars.
- Steps: Review the nine screens for readable row text, distinct focus/selection, hidden/protected distinction, no row-height jumps, no long-name overlap, active path/tab/file-list/metadata simultaneous visibility, destructive styling hierarchy, and no preview/operation reflow of the file list.
- Expected result: A reviewer can see that the file-list first slice meets the spec and that screenshots are soft review evidence, not hard release gates.
- Failure proves: The implementation may pass static checks but fail the visible product quality bar.
- Automation location: `tests/visual/baselines/winui/dark-comfortable-1440x900-100/` plus manual review notes.

## Fixtures and Data

- `tests/fixtures/ui-contracts/valid/`: controlled valid token/resource dictionaries for M1 validator tests.
- `tests/fixtures/ui-contracts/invalid/`: missing-key, duplicate-key, wrong-type, wrong-value, wrong-brush-reference, and forbidden-literal fixtures.
- `file-list-v1`: deterministic app-shell fixture containing synthetic normal, folder, selected, focused, selected-focused, multi-selected, hidden/protected, thumbnail fallback, long-name, metadata-heavy, and empty-folder states.
- `tests/visual/baselines/winui/dark-comfortable-1440x900-100/`: committed first-slice screenshot baselines and JSON sidecars.
- `tests/visual/current/` and `tests/visual/diffs/`: generated, ignored, uncommitted screenshot outputs.

Fixture rules:

- Fixture names and row data are hardcoded or compiled test data in the first slice.
- Fixture data must not come from arbitrary user paths.
- Screenshot sidecars must use synthetic file names and must not include raw local paths or preview text.
- Visual fixture evidence must not replace existing V1 integration/corpus evidence for real filesystem behavior.

## Mocking/Stubbing Policy

- UI contract tool tests use fixture files on disk because static parsing and file paths are the production boundary.
- App-shell fixture tests may use fake startup state, fake row data, fake thumbnail states, and test dispatchers to force visual states deterministically.
- Existing Core/Windows adapter behavior must not be mocked away when proving V1 regression safety; use the existing test suites for those boundaries.
- Screenshot tests may stub dynamic regions through sidecar metadata, but must not hide deterministic row text or state visuals as dynamic.
- Process-launch fixture rejection should be tested through the closest available startup boundary; if full WinUI process launch is unavailable locally, record manual process evidence and keep unit coverage over parser/guard decisions.

## Migration or Compatibility Tests

No data migration is expected because the first slice does not persist theme or density settings and does not change session/settings/favorites/recent-location schemas.

Compatibility proof is:

- T014 for existing V1 behavior regression safety.
- T009 for production/Release fixture rejection.
- T013 for generated visual outputs staying uncommitted.
- T018 manual review for the supported visual baseline profile.

Future persisted theme/density settings require a separate spec, architecture update, and migration test plan.

## Observability Verification

Observability is local validation output, not telemetry.

Required assertions:

- UI contract validation reports file path, token ID or scope ID, expected value/type where applicable, and observed value/type where available.
- Fixture rejection exits nonzero with a diagnosable reason suitable for CI.
- Baseline update records review ID in sidecars.
- Visual sidecars record enough metadata to identify fixture, profile, scale, viewport, and dynamic-region assumptions.
- No telemetry upload, remote diagnostics, or external reporting is added.

Covered by T003, T004, T005, T009, T012, T013, and T016.

## Security/Privacy Verification

Security and privacy assertions:

- Fixture mode accepts no arbitrary file paths or user-supplied fixture data paths.
- Production/Release builds cannot enter fixture mode.
- Sidecars and fixture metadata contain no raw user paths, usernames, secrets, file contents, terminal commands, clipboard contents, or preview text.
- `tools/VeloFile.UiContracts` treats command-line paths as local inputs and does not upload token, XAML, screenshot, or metadata content.

Covered by T009, T010, T012, T016, and T014.

## Performance Checks

Performance proof is contract-level in the first slice:

- Row height remains tokenized and stable across required states.
- Row rendering does not introduce synchronous filesystem, thumbnail, preview, or metadata work.
- Selection, filtering, listing, thumbnail loading, and file-operation states are not animated.
- Existing V1 responsiveness tests remain applicable.

Covered by T007, T008, T014, T017, and T018.

## Manual QA Checklist

Manual review is required for first-slice visual evidence:

- Launch the allowed fixture profile `dark-comfortable-1440x900-100`.
- Capture/review all nine required file-list screens.
- Confirm readable row names and metadata.
- Confirm selected, focused, selected-focused, multi-selected, hidden/protected, thumbnail fallback, long-name, and empty-folder states are visually clear.
- Confirm active tab/path, file list, row selection/focus, and metadata are simultaneously visible without overlap.
- Confirm screenshot sidecars match viewport, theme, density, fixture, and review ID.
- Confirm screenshots are treated as soft review evidence, not hard pixel-diff release gates.

## What Not To Test

- Pixel parity with `hifi-design/`, because the spec makes it reference input only.
- Runtime theme switching, compact/spacious density, or persisted density/theme settings, because they are non-goals for the first slice.
- Generated disk-backed visual fixtures, because first-slice visual evidence uses deterministic app-shell/view-model fixtures.
- Appium/WinAppDriver automation, because ADR 0009 and the spec defer it.
- Hard-gated screenshot pixel thresholds, because first-slice screenshots are review evidence until comparison is stable.
- New file-operation, preview, search, drag/drop, terminal, persistence, or diagnostics behavior, because existing V1 tests own those boundaries and the first slice must not change them.

## Uncovered Gaps

None requiring return to spec or architecture before implementation.

Implementation still needs to choose the exact test project placement for `tools/VeloFile.UiContracts` tests and the exact process-level fixture rejection harness. Those are execution details covered by T005 and T009.

## Next Artifacts

- M1 implementation under [2026-05-11-ui-design-system-shell-redesign.md](../docs/plans/2026-05-11-ui-design-system-shell-redesign.md).
- M1 creates the token/scope/deviation artifacts, static validation tool, and controlled fixture dictionaries that make this test spec executable over time.

## Follow-on Artifacts

None yet.

## Readiness

Active and ready for `implement` at M1. M1 is responsible for creating the initial contract artifacts, validator, and controlled fixtures that make the first proof layer executable.
