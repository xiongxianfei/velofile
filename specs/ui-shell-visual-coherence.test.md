# UI Shell Visual Coherence Test Spec

## Status

active

This test spec is the active proof plan for the approved shell visual-coherence follow-on. It is not implementation evidence by itself; implementation must add or update the tests and visual/manual evidence described here.

## Related Spec and Plan

- Feature spec: [ui-shell-visual-coherence.md](ui-shell-visual-coherence.md)
- Execution plan: [2026-05-11-ui-shell-visual-coherence.md](../docs/plans/2026-05-11-ui-shell-visual-coherence.md)
- First-slice test spec: [ui-design-system-shell-redesign.test.md](ui-design-system-shell-redesign.test.md)
- Architecture: [architecture.md](../docs/architecture/system/architecture.md)
- ADR: [ADR 0010](../docs/adr/0010-shell-visual-coherence-contracts.md)

## Testing Strategy

The proof strategy is layered:

- Contract tests validate additive shell tokens, governed scopes, deviation records, icon-resource invariants, visual sidecars, and behavior-preservation matrix inventory.
- Tool tests validate `tools/VeloFile.UiContracts` with controlled valid and invalid XAML, sidecar, and icon fixtures before production shell regions are trusted.
- Static WinUI tests validate production resource dictionaries, merged app resources, governed region markers, resource references, and forbidden raw/default visuals.
- App-shell tests validate fixture launch guards, allowlisted fixture icon kinds, synthetic fixture state, command/sidebar/preview/operation route preservation, and accessibility metadata.
- Visual inventory tests validate required full-shell states, effective-pixel profiles, sidecar metadata, generated-output ignore rules, and reviewed/manual `200%` evidence classification.
- Manual/review checks validate whole-shell visual coherence, minimum-size usability, high-DPI readability, focus/selection clarity, and accepted deviation records while screenshots remain soft evidence.
- Existing V1 behavior tests remain the regression surface for real navigation, listing, selection, search, context menu, file operations, drag/drop, preview, terminal launch, diagnostics, persistence, and accessibility behavior.

## Requirement Coverage Map

| Requirement | Tests / verification |
|---|---|
| R1-R4 | TSC001, TSC002, TSC018 |
| R5 | TSC015, TSC016, TSC017, TSC018, TSC021 |
| R6-R8 | TSC001, TSC002, TSC018 |
| R9-R15 | TSC002, TSC003, TSC004, TSC005, TSC018 |
| R16-R22 | TSC006, TSC007, TSC013, TSC014, TSC018, TSC020 |
| R23-R27 | TSC006, TSC013, TSC015, TSC018 |
| R28-R34 | TSC007, TSC008, TSC013, TSC015, TSC020 |
| R35-R43 | TSC008, TSC009, TSC010, TSC013 |
| R44-R49 | TSC011, TSC013, TSC015, TSC016, TSC020 |
| R50-R56 | TSC012, TSC013, TSC015, TSC016, TSC020 |
| R57-R61 | TSC013, TSC015, TSC016, TSC020 |
| R62-R65 | TSC013, TSC015, TSC016, TSC020 |
| R66-R77 | TSC003, TSC004, TSC013, TSC014, TSC018, TSC019, TSC020 |
| R78-R82 | TSC015, TSC016, TSC017, TSC018, TSC021 |
| I1-I6 | TSC001, TSC002, TSC015, TSC018 |
| I7-I8 | TSC009, TSC010 |
| I9-I10 | TSC003, TSC004, TSC013, TSC014, TSC019 |
| C1-C7 | TSC001, TSC015, TSC018, TSC021 |
| O1-O5 | TSC003, TSC004, TSC009, TSC014, TSC019 |
| S1-S6 | TSC004, TSC009, TSC010, TSC014, TSC019 |
| A11Y1-A11Y9 | TSC006, TSC007, TSC011, TSC012, TSC013, TSC016, TSC020 |
| P1-P7 | TSC007, TSC008, TSC010, TSC013, TSC014, TSC015, TSC021 |
| AC1-AC5 | TSC001, TSC002, TSC003, TSC005, TSC006 |
| AC6-AC8 | TSC008, TSC009, TSC010 |
| AC9-AC13 | TSC003, TSC004, TSC013, TSC014, TSC019 |
| AC14-AC16 | TSC007, TSC011, TSC012, TSC015, TSC016, TSC020 |
| AC17-AC20 | TSC015, TSC016, TSC017, TSC018, TSC021 |

## Example Coverage Map

| Example | Tests / verification |
|---|---|
| E1 | TSC006, TSC013, TSC020 |
| E2 | TSC008, TSC009, TSC010, TSC013 |
| E3 | TSC012, TSC016, TSC020 |
| E4 | TSC013, TSC020 |
| E5 | TSC004, TSC014, TSC019, TSC020 |
| E6 | TSC015, TSC016, TSC017, TSC021 |

## Edge Case Coverage

| Edge case | Tests / verification |
|---|---|
| EC1 | TSC003, TSC006, TSC020 |
| EC2 | TSC007, TSC013, TSC020 |
| EC3 | TSC008, TSC009, TSC013 |
| EC4-EC5 | TSC009, TSC010 |
| EC6-EC8 | TSC008, TSC009 |
| EC9-EC11 | TSC013, TSC020 |
| EC12-EC14 | TSC004, TSC014, TSC019 |
| EC15 | TSC005, TSC013, TSC020 |
| EC16-EC17 | TSC012, TSC016, TSC020 |
| EC18 | TSC011, TSC016 |
| EC19 | TSC013, TSC016, TSC020 |
| EC20 | TSC007, TSC013, TSC016, TSC020 |
| EC21 | TSC015, TSC017, TSC021 |
| EC22 | TSC005, TSC018, TSC020 |
| EC23 | TSC002, TSC003 |
| EC24 | TSC008, TSC009, TSC010 |

## Behavior-Preservation Matrix

Each implementation milestone must mark touched rows, then cite automated tests or explicit manual evidence before closeout.

| Behavior row | M2 Shell foundation | M3 File list/icons | M4 Command band | M5 Sidebar | M6 Status/operations | M7 Preview/details | Required evidence |
|---|---|---|---|---|---|---|---|
| Navigation | touched | observe | touched | touched | observe | observe | `AppShellCommandRouteTests`, `AppShellContractTests`, manual keyboard route notes when needed |
| Tabs/session restore | observe | observe | observe | observe | observe | observe | existing App/Core shell startup/session tests or explicit not-touched note |
| Listing/virtualization | observe | touched | observe | observe | observe | touched | `FileListResourceContractTests`, listing/virtualization regression tests, no custom row-control check |
| Selection | observe | touched | observe | observe | observe | touched | file-list selection mapper/tests, selected/focused visual evidence |
| Filter/search | observe | observe | touched | observe | observe | observe | `AppShellCommandRouteTests`, search/filter tests, `shell-filter-active`, `shell-search-active` evidence |
| Context menu | observe | touched | observe | observe | observe | observe | context flyout route checks and existing app-shell tests |
| File operations | observe | observe | observe | observe | touched | observe | Core/App file-operation tests, operation/destructive shell evidence |
| Drag/drop | observe | touched | observe | observe | observe | observe | existing Core/Windows/App drag/drop tests; screenshots do not prove this row |
| Preview | observe | touched | observe | observe | observe | touched | App/Core preview tests, `shell-preview-open` evidence |
| Terminal launch | observe | observe | observe | touched | observe | observe | terminal command route tests or manual evidence if automation unavailable |
| Diagnostics | observe | observe | observe | observe | observe | observe | existing diagnostics tests or explicit no-route-change note |
| Persistence | observe | observe | observe | observe | observe | observe | existing persistence/session tests; no new theme/density persistence assertions |
| Accessibility routes | touched | touched | touched | touched | touched | touched | accessibility/static XAML tests, keyboard order notes, accessible-name/tooltip checks |

`observe` means the slice does not intend to change that behavior but must not break it. A slice may not close by citing only fixture screenshots for any behavior row.

## Test Cases

### Contract And Tooling

TSC001. Shell follow-on authority and non-goal contract
- Covers: R1-R8, I1-I6, C1-C7, AC1
- Level: contract
- Fixture/setup: `specs/ui-shell-visual-coherence.md`, first-slice spec/test spec, ADR 0010, plan file.
- Steps: Assert the follow-on spec exists as a separate file, links to the proposal and first-slice spec, does not supersede first-slice authority, keeps `hifi-design/` reference-only, and does not define persisted theme/density, theme engine, broad customization, real Shell icons for first baselines, hard pixel gates, or lower `720 x 500` minimum.
- Expected result: Source artifacts preserve the approved authority boundary and non-goals.
- Failure proves: The follow-on has expanded or forked the UI contract.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`.

TSC002. Additive V1 token and scope contract extension
- Covers: R9-R15, I5, C4, EC23, AC2-AC3, AC19
- Level: contract
- Fixture/setup: `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, `docs/ui/design-deviations.md`.
- Steps: Parse token and scope JSON; assert version remains `1`; assert shell-wide entries are additive or region documents why no new token is needed; assert new scopes activate only implemented regions; assert no `tokens.v2.json` or incompatible semantic rename is introduced without a later approved decision; assert deviation records have required fields and status values.
- Expected result: Shell regions extend the existing V1 contracts without premature version sprawl.
- Failure proves: Validation cannot rely on a stable token/scope authority.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/ShellTokenScopeContractTests.cs`.

TSC003. UI contract tool rejects governed shell visual drift
- Covers: R12-R14, R18, R22, R66-R77, O1, AC5, EC1, EC15
- Level: contract | integration
- Fixture/setup: Controlled valid and invalid shell XAML fixtures under `tests/fixtures/ui-contracts/`.
- Steps: Run `tools/VeloFile.UiContracts validate-tokens` against fixtures for shell foundation, command band, sidebar, status/operation, preview/details, and legacy out-of-scope regions. Include invalid cases for inline colors, spacing, radii, opacity, focus thickness, raw/default light surfaces, required-resource omissions, and unrecorded governed mismatch markers.
- Expected result: Governed visual drift fails with file, scope, rule, and observed violation; legacy out-of-scope literals do not fail until their scope is active.
- Failure proves: The static gate either misses shell drift or overreaches into legacy XAML.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/UiContractTests.cs` and new shell fixture cases.

TSC004. Screenshot sidecar contract and privacy validation
- Covers: R66-R77, O3-O4, S3-S6, AC9-AC13, EC12-EC14, E5
- Level: contract
- Fixture/setup: Sidecar JSON fixtures for current, baseline, automated, and manual/release visual evidence.
- Steps: Validate required sidecar fields: profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review ID. Assert profile size/scale consistency, no raw local paths/usernames/secrets/file contents/terminal commands/clipboard/preview text, and explicit manual/release classification for unavailable `shell-standard-1440x900-200`.
- Expected result: Sidecars are traceable, safe, and cannot silently omit required high-DPI evidence.
- Failure proves: Screenshot evidence is unsafe or not reviewable.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/ShellVisualSidecarTests.cs`.

TSC005. Design deviation records for accepted mismatches
- Covers: R15, R22, AC19, EC15, EC22
- Level: contract | manual
- Fixture/setup: `docs/ui/design-deviations.md`, full-shell screenshot evidence notes.
- Steps: For each accepted reference deviation or temporary redesigned/non-redesigned mismatch, assert the record names the affected region, reference pattern, VeloFile decision, reason, user impact, verification evidence, status, and review ID.
- Expected result: Meaningful visual differences are reviewable rather than accidental drift.
- Failure proves: Reviewers cannot distinguish intentional product decisions from regressions.
- Automation location: docs contract tests plus manual review during each milestone closeout.

### Shell Resources And Region Contracts

TSC006. Shell surface foundation resources are merged and tokenized
- Covers: R16-R27, R57, R62, A11Y1-A11Y3, A11Y6, AC4-AC5, E1
- Level: integration | contract
- Fixture/setup: `src/VeloFile.App/App.xaml`, `MainWindow.xaml`, `Resources/Tokens/`, `Resources/Components/VeloFile.Shell.xaml` or equivalent.
- Steps: Assert shell resource dictionaries are merged; app root, chrome, sidebar, content, command-band container, status area, preview/details container, separators, text hierarchy, focus/accent, selection, hover, disabled, warning, danger, and success surfaces use VeloFile resources in governed scopes.
- Expected result: The default shell consumes one dark comfortable surface model without hiding existing V1 routes.
- Failure proves: Shell coherence is still a local XAML patch rather than a governed surface foundation.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/ShellSurfaceResourceContractTests.cs`.

TSC007. File-list polish preserves row state and behavior contracts
- Covers: R28-R34, R64, A11Y1-A11Y3, A11Y8, P1-P4, AC16, EC2, EC20
- Level: integration | manual
- Fixture/setup: `VeloFile.FileList.xaml`, `MainWindow.xaml`, file-list view-model fixtures.
- Steps: Assert normal, hover, selected, focused, selected-focused, multi-selected, hidden, protected/system, long filename, folder, thumbnail fallback, and empty-folder states have tokenized resources. Assert focus is not danger/warning/error-like, selected/focused distinction is not text-color-only, hidden/protected rows remain readable, and all required states keep row height stable.
- Expected result: File-list row presentation is polished and behavior-preserving.
- Failure proves: The primary product surface regressed in accessibility, readability, or layout stability.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/FileListResourceContractTests.cs`; manual evidence in required shell screenshots.

TSC008. Fixture icon resource dictionary exposes required vector resources
- Covers: R35-R38, R41-R43, AC6-AC7, EC3, EC6-EC8, EC24, E2
- Level: contract | integration
- Fixture/setup: `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`.
- Steps: Assert the icon dictionary exists, is merged when needed, defines `VfFileListIconContainerStyle`, `VfFileListIconPathStyle`, `VfFileListFixtureIconTemplate`, and named `VfIconGeometry*` resources for `FileGeneric`, `Folder`, `Pdf`, `Image`, `Text`, `Spreadsheet`, `Executable`, `Markdown`, and `ThumbnailFallback`. Assert resources use raw `Path` geometry in a fixed container and tokenized foreground/background/size resources.
- Expected result: Fixture icons are deterministic XAML vector resources, not text chips or platform/glyph controls.
- Failure proves: Visual evidence may repeat the prior icon defect class.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/FixtureIconResourceContractTests.cs`.

TSC009. Icon invariant checker rejects forbidden icon rendering paths
- Covers: R35-R43, O2, S1, AC7-AC8, EC3-EC8, EC24
- Level: contract
- Fixture/setup: Controlled invalid XAML fixtures for `<SymbolIcon`, `<PathIcon`, private-use glyphs, ellipsized chip text, unapproved icon colors, unapproved icon sizes, missing geometries, and missing styles.
- Steps: Run UI contract validation over valid and invalid icon fixture scopes.
- Expected result: Invalid icon paths fail nonzero with the offending file/rule; valid raw vector icon resources pass.
- Failure proves: Static checks cannot prevent known icon-regression classes.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/ShellFixtureIconContractTests.cs`.

TSC010. Fixture rows expose allowlisted icon kinds only
- Covers: R39-R40, I7-I8, S1-S2, P3, AC8, EC4-EC5, E2
- Level: unit | integration
- Fixture/setup: `UiFixtureRegistry`, fixture row model, fixture launch parser.
- Steps: Assert fixture rows expose icon kinds from the allowlist only; assert unknown icon kind and raw resource key inputs fail; assert fixture launch remains Debug/test guarded and does not accept arbitrary fixture data paths.
- Expected result: Fixture icon selection is deterministic and cannot be redirected to arbitrary resources or user data.
- Failure proves: Fixture evidence is unsafe or nondeterministic.
- Automation location: `tests/VeloFile.App.Tests/UiFixtures/UiFixtureRegistryTests.cs` and `UiFixtureLaunchTests.cs`.

TSC011. Command band visual resources preserve command routes
- Covers: R44-R49, A11Y1-A11Y6, AC15, EC18
- Level: integration | contract
- Fixture/setup: `MainWindow.xaml`, `MainWindow.xaml.cs`, `VeloFile.CommandBand.xaml` or equivalent.
- Steps: Assert governed command controls share tokenized height, radius, spacing, typography, hover, focus, pressed, disabled, input border/background, and accessible-name/tooltip resources. Assert back, forward, up, refresh, path/breadcrumb entry, current-folder filter, recursive search, search start, cancel, and clear handlers/routes remain present.
- Expected result: The command band looks intentional while preserving V1 navigation/search behavior.
- Failure proves: Visual work changed command behavior or left raw/default controls exposed.
- Automation location: `tests/VeloFile.App.Tests/AppShellCommandRouteTests.cs` and `tests/VeloFile.App.Tests/UiDesign/CommandBandResourceContractTests.cs`.

TSC012. Navigation-first sidebar preserves access and accessibility
- Covers: R50-R56, A11Y1-A11Y6, AC14, EC16-EC17, E3
- Level: integration | manual
- Fixture/setup: `MainWindow.xaml`, sidebar resources, App shell view model.
- Steps: Assert locations, favorites, recent entries, drives, visibility toggles, and terminal controls are present. Assert visual grouping order is navigation-first, keyboard traversal matches visual grouping unless a deviation exists, and groups/controls have accessible names or tooltips.
- Expected result: Sidebar reordering improves hierarchy without removing or hiding existing routes.
- Failure proves: A visual sidebar change introduced an observable behavior/accessibility regression.
- Automation location: `tests/VeloFile.App.Tests/UiDesign/SidebarResourceContractTests.cs`; keyboard/accessibility manual notes for M5 if automation is incomplete.

### Visual Evidence, Profiles, And Manual Review

TSC013. Region-slice full-shell evidence exists before milestone closeout
- Covers: R16-R22, R26, R30-R33, R58-R65, R66-R73, A11Y1-A11Y9, P5-P7, AC9-AC11, AC16, EC1-EC3, EC9-EC11, EC15, EC19-EC20, E1-E5
- Level: manual | smoke | contract
- Fixture/setup: Required current screenshots or manual review notes from M2-M7.
- Steps: For each region milestone, assert required full-shell evidence exists before closeout: M2 `shell-default`; M3 `shell-file-list-selected-focused`; M4 `shell-filter-active` and `shell-search-active`; M5 `shell-sidebar-focused` or `shell-default`; M6 `shell-operation-running` and `shell-destructive-confirmation`; M7 `shell-preview-open`. Assert screenshots show the whole shell and any accepted mismatch has a deviation record.
- Expected result: Region slices are reviewed in whole-shell context, not as isolated component patches.
- Failure proves: A milestone can close without visual evidence for shell coherence.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/ShellVisualEvidenceInventoryTests.cs` plus manual review notes.

TSC014. Required profile and baseline inventory validation
- Covers: R66-R77, O3-O4, S3-S6, A11Y6-A11Y7, AC9-AC13, EC12-EC14, E4-E5
- Level: contract | script
- Fixture/setup: `tests/visual/baselines/winui/<profile>/`, `tests/visual/current/`, `tests/visual/diffs/`, `scripts/update-ui-baselines.ps1`.
- Steps: Assert the seven required shell states exist or have approved manual evidence; assert `shell-min-900x560-100` and `shell-standard-1440x900-100` are captured/reviewed; assert `shell-standard-1440x900-200` is captured or explicitly manual/release; assert generated current/diff outputs are ignored and normal CI does not mutate baselines.
- Expected result: Visual evidence inventory is complete, traceable, and non-mutating in normal CI.
- Failure proves: Screenshot evidence is incomplete or baseline mutation is uncontrolled.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/VisualBaselineInventoryTests.cs` and script tests.

TSC015. Behavior-preservation matrix enforcement
- Covers: R5, R27, R47, R51, R61, R65, R78-R82, AC17-AC20, EC21, E6
- Level: contract | integration
- Fixture/setup: This test spec behavior matrix, plan milestone closeout notes, existing V1 test results.
- Steps: For each implemented region slice, assert touched behavior rows are named and cite automated tests or explicit manual evidence. Assert fixture-only screenshots are never cited as proof of real filesystem, Windows adapter, drag/drop, preview, file-operation, terminal, diagnostics, persistence, or accessibility behavior.
- Expected result: Visual proof and behavior proof stay separate and complete.
- Failure proves: A visual slice overclaims behavior preservation.
- Automation location: `tests/VeloFile.Corpus.Tests/UiContracts/ShellBehaviorMatrixTests.cs`; plan closeout review.

TSC016. Region route preservation tests
- Covers: R5, R27, R47-R49, R51-R55, R58-R61, R63-R65, R78-R82, AC14-AC18
- Level: integration
- Fixture/setup: Existing App/Core/Windows tests and new focused App shell tests where gaps exist.
- Steps: Run or add focused tests for touched routes: command navigation/search, sidebar visibility/terminal controls, file-list context/selection/drag-drop, operation commands, destructive confirmation routes, preview loading/unsupported/failed/PDF navigation, diagnostics, persistence, and accessibility names/keyboard order.
- Expected result: Existing V1 routes remain reachable after visual redesign.
- Failure proves: A visual region slice changed product behavior.
- Automation location: `tests/VeloFile.App.Tests/`, `tests/VeloFile.Core.Tests/`, and `tests/VeloFile.Windows.Tests/`.

TSC017. Fixture-only evidence is labeled and limited
- Covers: R78-R82, AC17-AC20, EC21
- Level: contract
- Fixture/setup: screenshot sidecars, visual evidence notes, milestone validation notes.
- Steps: Assert fixture visual evidence uses `evidenceKind` values that do not imply release/integration proof; assert behavior rows require separate automated/manual evidence; assert reports do not call fixture screenshots proof of real filesystem, Shell icon, drag/drop, preview, file-operation, terminal, diagnostics, or persistence behavior.
- Expected result: Review evidence cannot be misread as platform integration proof.
- Failure proves: The project is over-trusting deterministic fixtures.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/ShellVisualEvidenceClassificationTests.cs`.

TSC018. Compatibility and rollback safety scan
- Covers: R1-R15, C1-C7, I1-I6, AC19-AC20, EC22-EC23
- Level: contract | regression
- Fixture/setup: changed files per milestone, architecture/ADR, plan closeout notes.
- Steps: Assert no Core/Windows adapter boundary changes are introduced by visual slices unless separately specified; assert no settings/session/favorites/recent/diagnostics schema migration is required; assert each region can be rolled back by reverting region resources, scoped XAML, tests, and visual evidence.
- Expected result: Visual-coherence work stays compatible with V1 and rollback remains region-local.
- Failure proves: The change has become architecture or migration work beyond this spec.
- Automation location: corpus/static architecture tests plus code review checklist.

TSC019. Visual evidence security and baseline update guardrails
- Covers: R74-R77, O3-O5, S3-S6, AC12-AC13, EC12-EC14
- Level: security | script
- Fixture/setup: baseline update script, sidecar fixtures, generated output directories.
- Steps: Run `scripts/update-ui-baselines.ps1` without review ID, without current screenshots, with reviewed current screenshots, and with generated diffs present. Assert missing inputs fail nonzero, review ID is recorded, current/diff folders remain uncommitted, and no upload/telemetry behavior is introduced.
- Expected result: Baseline mutation is local, explicit, and traceable.
- Failure proves: Visual artifacts can be mutated or leaked without review.
- Automation location: `tests/VeloFile.Corpus.Tests/Visual/UpdateUiBaselinesScriptTests.cs` or `tests/validation/UpdateUiBaselines.Tests.ps1`.

TSC020. Manual full-shell visual review checklist
- Covers: R16-R22, R26, R29-R33, R44-R49, R52-R56, R58-R65, R69-R73, A11Y1-A11Y9, E1-E5
- Level: manual | smoke
- Fixture/setup: App launched in allowed fixture mode; required profiles and shell states; design-deviation log.
- Steps: Review required full-shell states for one coherent dark comfortable surface model, current-location/action/file hierarchy, no raw/default governed controls, deterministic polished icons, no placeholder chips, focus/selection not danger-like, sidebar navigation-first hierarchy, operation/destructive clarity, preview balance, minimum-size usability, and high-DPI readability or manual/release notes.
- Expected result: A reviewer can judge whole-shell coherence without pixel-perfect gating.
- Failure proves: Static checks passed but the shell still looks visually fragmented or inaccessible.
- Automation location: manual review notes and `tests/visual/baselines/winui/<profile>/`.

TSC021. Existing broad V1 regression validation
- Covers: R5, R78-R82, C1-C7, P1-P7, AC17-AC20
- Level: regression | smoke
- Fixture/setup: solution test suites and CI script.
- Steps: After production shell changes, run focused tests first, then broad validation at milestone closeout: `dotnet build VeloFile.sln -c Debug`, `dotnet test VeloFile.sln -c Debug`, and `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` when environment permits.
- Expected result: Existing behavior proof remains applicable and passing.
- Failure proves: Visual redesign introduced product regression or broke the repository validation path.
- Automation location: existing `tests/VeloFile.*.Tests/` and `scripts/ci.ps1`.

## Fixtures and Data

- `tests/fixtures/ui-contracts/valid/`: controlled valid token, shell scope, sidecar, and icon resource dictionaries.
- `tests/fixtures/ui-contracts/invalid/`: invalid shell literals, raw/default controls, missing resources, forbidden icon controls, ellipsized chips, sidecar privacy leaks, profile mismatches, and missing review IDs.
- `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`: production fixture icon resource dictionary for deterministic first evidence.
- Full-shell fixture states: `shell-default`, `shell-file-list-selected-focused`, `shell-filter-active`, `shell-search-active`, `shell-preview-open`, `shell-operation-running`, and `shell-destructive-confirmation`.
- Review profiles: `shell-min-900x560-100`, `shell-standard-1440x900-100`, and `shell-standard-1440x900-200`.
- `tests/visual/current/` and `tests/visual/diffs/`: generated, ignored, uncommitted screenshot outputs.
- `docs/ui/design-deviations.md`: accepted or temporary reference and whole-shell mismatch records.

Fixture rules:

- Fixture names, shell states, icon kinds, file names, and metadata are hardcoded or compiled test data.
- Fixture data must not come from arbitrary local paths.
- Fixture icon selection uses allowlisted icon kinds, not arbitrary resource keys.
- Sidecars must use synthetic data and must not include raw paths, usernames, secrets, terminal commands, clipboard contents, preview text, or real file contents.

## Mocking/Stubbing Policy

- UI contract and visual inventory tests use fixture files on disk because static parsing and sidecar validation are the production validation boundary.
- App-shell fixture tests may use fake view models, fake row data, fake operation state, fake preview state, and test dispatchers to force deterministic visual states.
- Existing Core and Windows adapter behavior must not be mocked away when proving V1 behavior preservation; use existing real-boundary tests or explicit manual evidence.
- Screenshot tests may classify dynamic regions through sidecar metadata, but must not hide deterministic shell controls, icon surfaces, row text, selected/focused state, operation danger hierarchy, or sidebar grouping as dynamic.
- If full WinUI process capture or `200%` scaling cannot be automated locally, record manual/release evidence and keep static/fixture-sidecar tests automated.

## Migration or Compatibility Tests

No data migration is expected. This spec does not add theme or density persistence, new settings schemas, session schema changes, favorites/recent-location migrations, diagnostics schema changes, or release metadata migrations.

Compatibility proof is:

- TSC001 and TSC018 for source-of-truth, non-goal, and rollback boundaries.
- TSC015-TSC017 for behavior-preservation matrix enforcement.
- TSC021 for existing broad V1 regression validation.

Future persisted theme/density settings, real Shell icons as deterministic baselines, hard screenshot gates, or token major-version changes require a separate spec, architecture decision, and matching migration/compatibility tests.

## Observability Verification

Observability is local validation output, not telemetry.

Required assertions:

- UI contract validation reports file path, scope or token, expected rule, and observed violation when available.
- Fixture icon validation reports offending icon kind or resource usage.
- Unknown fixture icon kind and unsafe fixture launch failures exit nonzero with diagnosable messages.
- Sidecars identify profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review ID.
- Baseline update commands record the supplied review ID.
- No telemetry upload, remote diagnostics, or external reporting is added.

Covered by TSC003, TSC004, TSC009, TSC010, TSC014, TSC017, and TSC019.

## Security/Privacy Verification

Security and privacy assertions:

- Fixture mode accepts no arbitrary fixture data paths or arbitrary icon resource keys.
- Production builds cannot enter fixture mode.
- Visual sidecars and manual evidence records contain no raw local paths, usernames, secrets, file contents, terminal commands, clipboard contents, or preview text.
- Destructive confirmation evidence uses synthetic file names and no real local paths.
- UI contract validation treats command-line paths as local inputs and does not upload token, XAML, screenshot, or sidecar content.

Covered by TSC004, TSC009, TSC010, TSC014, TSC019, and TSC020.

## Performance Checks

Performance proof is contract-level unless a milestone introduces measurable runtime work.

- File-list virtualization remains intact.
- Row height remains stable across hover, focus, selection, icon kind, hidden/protected state, thumbnail fallback, and long-name states.
- Fixture icons do not require Shell icon extraction, thumbnail extraction, filesystem enumeration, or network access.
- Visual fixture capture prefers deterministic fixture state over disk-backed state.
- Static UI contract validation runs without launching the app.
- Screenshot comparison remains soft evidence and not a hard gate.

Covered by TSC007, TSC008, TSC010, TSC014, TSC015, TSC018, TSC020, and TSC021.

## Manual QA Checklist

Manual review is required when screenshot capture, high-DPI scaling, keyboard traversal, or accessibility metadata cannot be fully automated.

- Review `shell-default` at `shell-standard-1440x900-100` for one coherent app/chrome/sidebar/content/command/file-list/status/preview surface model.
- Review `shell-min-900x560-100` when layout or minimum-size behavior changes; confirm no primary navigation clipping, usable file list, unobscured content, reachable path/search band, visible selected/focused row, and non-obscuring operation/destructive surfaces.
- Review `shell-standard-1440x900-200` as automated or manual/release evidence; confirm readable text, crisp icons, visible focus ring, no clipped controls, and stable row rhythm.
- Review deterministic icons; confirm no `P...`, `D...`, `T...`, `SymbolIcon`, `PathIcon`, private-use glyph, or real Shell icon dependency appears in governed fixture evidence.
- Review sidebar grouping; confirm locations, favorites, recents, and drives are primary while visibility toggles and terminal controls remain discoverable and keyboard reachable.
- Review command band active filter/search states; confirm path, filter, search, cancel, clear, and disabled states remain reachable and visually integrated.
- Review operation/destructive states; confirm destructive actions use danger treatment only for destructive meaning and do not obscure primary navigation.
- Review preview-open state; confirm file-list selection/focus remains visible and preview loading/unsupported/failed/metadata states are distinct where touched.
- Confirm accepted mismatches are recorded in `docs/ui/design-deviations.md`.
- Confirm screenshots are soft-review evidence and are not cited as real filesystem or platform integration proof.

## What Not To Test

- Pixel parity with `hifi-design/`, because the spec treats it as reference input only.
- Hard screenshot pixel or perceptual thresholds, because this follow-on keeps screenshots as soft-review evidence.
- Persisted theme/density settings, runtime theme engine behavior, tweak panels, plugin UI, color labels, dual-pane browsing, or non-V1 features, because they are non-goals.
- Real Windows Shell icons in first deterministic fixture baselines, because they are future integration evidence and nondeterministic for this proof layer.
- A new custom file-list row control or new selection behavior model, because the spec explicitly excludes that first-slice change.
- New filesystem, drag/drop, preview provider, terminal, diagnostics, or Windows adapter behavior, because existing V1 tests own those boundaries.
- `shell-stress-720x500-100` as a pass/fail profile, unless a later accepted spec lowers VeloFile's supported minimum size.

## Uncovered Gaps

None requiring return to spec or architecture before implementation.

Implementation still needs to choose exact class names for some new tests, the final sidecar schema property spelling if existing first-slice sidecars use `viewport` rather than `effectiveWindowSize`, and the initial automation status for `shell-standard-1440x900-200`. Those are execution details covered by TSC004, TSC014, and TSC020.

## Next Artifacts

- `implement` after this test spec is accepted for use by the milestone implementation workflow.
- M1 implementation in [2026-05-11-ui-shell-visual-coherence.md](../docs/plans/2026-05-11-ui-shell-visual-coherence.md), starting with shell contract and validator extension.

## Follow-on Artifacts

None yet.

## Readiness

Active and ready for `implement` at M1. M1 should begin test-first against TSC001-TSC005, TSC009, TSC014, TSC015, TSC017, and TSC019.
