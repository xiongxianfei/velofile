# UI Design System and Shell Redesign

## Status

approved

## Related proposal

- [VeloFile UI Design System and Shell Redesign Proposal](../docs/proposals/2026-05-11-ui-design-system-shell-redesign.md)

## Goal and context

This spec defines the VeloFile-owned UI design-system contract for the first shell redesign slice. The goal is to improve visual quality, file-list clarity, keyboard/focus affordance, and reviewability without changing the approved V1 file-manager behavior in [v1-product-scope.md](v1-product-scope.md).

`hifi-design/` is reference material only. It can inform quality review, but it is not authoritative for production tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria.

The first redesign slice is limited to design-system foundation resources and file-list row presentation. Later shell regions must extend this spec or add approved follow-on specs before implementation.

## Glossary

- **Design token**: a named VeloFile UI value such as a color, brush, row height, spacing value, focus thickness, opacity, font family, or text size.
- **Token contract**: the machine-readable JSON contract that defines accepted first-slice tokens and their required WinUI resource keys.
- **UI contract scope**: the machine-readable JSON contract that defines which files and resource references are governed by first-slice token/literal rules.
- **Reference design package**: the untracked or externally supplied `hifi-design/` material used only as benchmark/reference input.
- **First slice**: the initial UI redesign scope covering token resources, shell layout/focus constants, basic app background/chrome resources, and file-list row presentation.
- **File-list row**: the visible row representation for a listed file or folder, including name, metadata, thumbnail/icon fallback, selection, focus, hidden/protected distinction, and row spacing.
- **Fixture mode**: a non-production test launch path that renders deterministic UI states for visual evidence.
- **Visual baseline**: a reviewed screenshot and JSON sidecar stored as visual evidence for a stable profile.
- **Generated current screenshot**: a transient screenshot captured during validation and not committed.
- **Design deviation**: an intentional difference from reference design material recorded because VeloFile chooses a clearer, more accessible, more Windows-native, more performant, more maintainable, or more V1-correct UI.

## Examples first

### Example E1: token contract governs resource dictionaries

Given `docs/ui/tokens.v1.json` defines `VfColor.Surface.Content` with XAML keys `VfColorSurfaceContent` and `VfBrushSurfaceContent`
When UI contract validation runs against the WinUI resource dictionaries
Then validation passes only if both keys exist, have the expected resource types, and match the declared first-slice value or allowed reference relationship.

### Example E2: file-list rows use named resources

Given the file list is in first-slice redesign scope
When the app renders file rows
Then `MainWindow.xaml` uses named resources for the file-list row template and item container style
And first-slice row height, padding, text styles, selection, focus, and hidden/protected state styling come from VeloFile resources rather than local row literals.

### Example E3: deterministic fixture mode renders visual baselines

Given the app is built in an allowed Debug/test context
And `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` is present
When the app starts with `--test-ui-fixture file-list-v1 --theme dark --density comfortable --viewport 1440x900`
Then the app renders deterministic file-list rows for normal, selected, focused, multi-selected, hidden/protected, thumbnail fallback, long-name, metadata-heavy, and empty-folder states.

### Example E4: production rejects fixture mode

Given a production or Release build
When the app starts with `--test-ui-fixture file-list-v1`
Then the app rejects the flag and exits nonzero
And it does not silently launch the normal app or fixture UI.

### Example E5: screenshot baselines are reviewed deliberately

Given current screenshots exist under `tests/visual/current/`
When a maintainer runs `scripts/update-ui-baselines.ps1 -Suite winui -Profile dark-comfortable-1440x900-100 -ReviewId <issue-or-pr-id>`
Then the script copies approved screenshots and JSON sidecars into `tests/visual/baselines/winui/`
And normal CI does not mutate committed baselines.

### Example E6: reference deviation is recorded

Given the reference package uses a CSS-only shadow that has no appropriate WinUI equivalent
When VeloFile uses a Windows-native border/elevation treatment instead
Then the decision is recorded in `docs/ui/design-deviations.md` with reference, VeloFile decision, reason, user impact, verification, and status.

## Requirements

### Authority and scope

R1. The production UI contract MUST be owned by repository artifacts, not by `hifi-design/`.

R2. `hifi-design/` MUST be treated as reference input only and MUST NOT be used as the source of truth for production tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria.

R3. The first redesign slice MUST be limited to foundation design resources and file-list row presentation unless a later accepted spec extends the scope.

R4. The first redesign slice MUST NOT change approved V1 behavior for navigation, listing, selection, search, preview, drag/drop, file operations, persistence, diagnostics, terminal launch, or Windows integration.

R5. The first redesign slice MUST NOT introduce a broad theme engine, customizable toolbar, color labels, dual-pane browsing, plugin UI, or production tweak panel.

R6. The first redesign slice MUST use dark, comfortable, WinUI-native defaults only.

R7. The first redesign slice MUST NOT persist theme or density preferences.

R8. Later persisted theme or density behavior MUST be specified separately before implementation.

### UI principles

R9. First-slice file-list row height and row padding MUST resolve from the comfortable density tokens rather than per-row local values.

R10. Comfortable density MUST be the only exposed density in the first slice.

R11. File-list row rhythm MUST remain stable across normal, selected, focused, hidden/protected, thumbnail fallback, long-name, and metadata-heavy states.

R12. In the first visual baseline profile, the active path/tab surface, file-list rows, row selection/focus state, and row metadata MUST remain simultaneously visible without overlapping each other.

R13. Destructive action styling MUST NOT visually compete with ordinary navigation styling.

R14. Preview and operation visual changes introduced by this spec MUST remain subordinate to browsing and MUST NOT reflow the file list in the first slice.

### Token contract

R15. The first-slice token contract MUST be `docs/ui/tokens.v1.json`.

R16. `docs/ui/tokens.v1.json` MUST be machine-readable JSON and MUST include `version`, `theme`, `density`, and a `tokens` array.

R17. Every token entry in `docs/ui/tokens.v1.json` MUST include `id`, `xamlKeys`, `type`, `value`, `category`, and `requiredInFirstSlice`.

R18. Markdown documentation MAY explain tokens, but MUST NOT replace `docs/ui/tokens.v1.json` as the validation source of truth.

R19. First-slice token IDs MUST use VeloFile-owned semantic names, not reference-design names.

R20. The first-slice token contract MUST include the color tokens listed in Table T1.

R21. The first-slice token contract MUST include the typography tokens listed in Table T2.

R22. The first-slice token contract MUST include the spacing, sizing, and radius tokens listed in Table T3.

R23. The first-slice token contract MUST include the comfortable density tokens listed in Table T4.

R24. The first-slice token contract MUST include the focus, state, and motion tokens listed in Table T5.

R25. First-slice token values SHOULD be treated as baseline design values, not permanent brand law.

#### Table T1: first-slice color tokens

| Token | Value | Use |
|---|---:|---|
| `VfColor.Surface.App` | `#181A1E` | outer window/app background |
| `VfColor.Surface.Chrome` | `#14161A` | titlebar/top chrome/statusbar |
| `VfColor.Surface.Sidebar` | `#1C1E23` | sidebar |
| `VfColor.Surface.Content` | `#202329` | file list/content |
| `VfColor.Surface.Elevated` | `#282C33` | flyouts, dialogs, cards |
| `VfColor.Surface.Hover` | `#2E333B` | row/control hover |
| `VfColor.Surface.Selected` | `#343A44` | selected row background |
| `VfColor.Surface.Input` | `#16181C` | path/search/filter inputs |
| `VfColor.Border.Subtle` | `#2A2E35` | separators |
| `VfColor.Border.Default` | `#3A404A` | controls |
| `VfColor.Border.Strong` | `#586170` | hover/focus-adjacent border |
| `VfColor.Text.Primary` | `#F2F4F8` | filenames, primary labels |
| `VfColor.Text.Secondary` | `#D3D8E0` | body/chrome labels |
| `VfColor.Text.Muted` | `#9AA3AE` | metadata |
| `VfColor.Text.Faint` | `#747E8C` | section labels, hints |
| `VfColor.Accent` | `#9BE15D` | focus, active state, brand accent |
| `VfColor.Accent.Soft` | `#9BE15D24` | selected sidebar/filter hit |
| `VfColor.Accent.Line` | `#9BE15D66` | focus ring/accent border |
| `VfColor.Accent.OnAccent` | `#10240E` | text on accent fill |
| `VfColor.Danger` | `#FF7768` | permanent delete/destructive |
| `VfColor.Danger.Soft` | `#FF776824` | destructive warning background |
| `VfColor.Warning` | `#FFD166` | caution/limited state |
| `VfColor.Success` | `#82E084` | completed/safe state |

#### Table T2: first-slice typography tokens

| Token | Value |
|---|---:|
| `VfFont.Ui` | `Segoe UI Variable, Segoe UI, system-ui, sans-serif` |
| `VfFont.Mono` | `Cascadia Mono, Consolas, monospace` |
| `VfText.Size.Xs` | `10` |
| `VfText.Size.Sm` | `11` |
| `VfText.Size.Base` | `12.5` |
| `VfText.Size.Md` | `13` |
| `VfText.Size.Lg` | `14` |
| `VfText.Weight.Regular` | `400` |
| `VfText.Weight.Medium` | `500` |
| `VfText.Weight.Semibold` | `600` |

#### Table T3: first-slice spacing, sizing, and radius tokens

| Token | Value |
|---|---:|
| `VfSpace.1` | `4` |
| `VfSpace.2` | `8` |
| `VfSpace.3` | `12` |
| `VfSpace.4` | `16` |
| `VfSpace.5` | `24` |
| `VfSpace.6` | `32` |
| `VfRadius.Sm` | `4` |
| `VfRadius.Base` | `6` |
| `VfRadius.Lg` | `10` |
| `VfSize.TitlebarHeight` | `40` |
| `VfSize.ToolbarHeight` | `44` |
| `VfSize.BreadcrumbHeight` | `36` |
| `VfSize.StatusbarHeight` | `26` |
| `VfSize.SidebarWidth` | `240` |
| `VfSize.SidebarMinWidth` | `200` |
| `VfSize.PreviewPaneWidth` | `300` |
| `VfSize.IconSm` | `14` |
| `VfSize.IconMd` | `16` |

#### Table T4: first-slice density tokens

| Token | Value |
|---|---:|
| `VfDensity.Current` | `comfortable` |
| `VfDensity.RowHeight` | `30` |
| `VfDensity.RowPaddingX` | `12` |
| `VfDensity.RowPaddingY` | `6` |

#### Table T5: first-slice focus, state, and motion tokens

| Token | Value |
|---|---:|
| `VfFocus.Thickness` | `2` |
| `VfFocus.Inset` | `1` |
| `VfFocus.Color` | `VfColor.Accent.Line` |
| `VfState.HiddenOpacity` | `0.68` |
| `VfState.DisabledOpacity` | `0.46` |
| `VfMotion.FastMs` | `120` |
| `VfMotion.BaseMs` | `160` |

### WinUI resources and scoped conformance

R26. First-slice tokens MUST be represented as checked-in WinUI resource dictionaries merged from the app resource tree.

R27. First-slice resource dictionaries MUST represent colors as `Color` resources plus derived `SolidColorBrush` resources where a brush is required.

R28. First-slice typography sizes, spacing, sizing, focus thickness, opacity, and motion duration values MUST be represented as typed numeric resources where WinUI supports numeric resource values.

R29. First-slice font families MUST be represented as font resources.

R30. First-slice radius tokens MUST be represented as corner-radius resources.

R31. First-slice component resources MUST use named styles, named templates, and named item-container styles where the first-slice UI consumes reusable visual structure.

R32. First-slice XAML resource keys MUST use semantic VeloFile names, such as `VfColorSurfaceContent`, `VfBrushSurfaceContent`, `VfTextPrimaryBrush`, `VfFileListRowHeight`, `VfFileListItemContainerStyle`, and `VfFileListRowTemplate`.

R33. First-slice file-list row template and item-container resources MUST use static resource lookup for first-slice token and component resources unless a resource is explicitly governed by R34.

R34. Theme-aware resources MAY use theme-aware lookup only where system theme or high-contrast behavior must be preserved.

R35. The first implementation MUST NOT introduce a generated token pipeline.

R36. The first-slice UI contract scope file MUST be `docs/ui/ui-contract-scopes.v1.json`.

R37. `docs/ui/ui-contract-scopes.v1.json` MUST include `version` and a `scopes` array.

R38. The first active scope MUST cover the file-list first slice and MUST identify the governed files, required resource references, and forbidden literal rules.

R39. The first implementation MUST keep `docs/ui/ui-contract-scopes.v1.json` small and MUST NOT use it as a full design prose spec.

R40. The token checker MUST validate `docs/ui/tokens.v1.json` against the first-slice XAML resource dictionaries.

R41. The token checker MUST validate all required token keys, expected resource types, directly comparable values, color-to-brush relationships, duplicate resource keys, and missing or extra first-slice resources.

R42. The token checker MUST enforce strict tokenized-literal rules in new token and component resource dictionaries.

R43. The token checker MUST enforce targeted first-slice tokenized-literal rules for the file-list region in `MainWindow.xaml`.

R44. The first-slice checker MUST NOT impose a global ban on existing literals outside the first redesigned file-list scope.

R45. First-slice validation MUST run through a lightweight .NET tool project included in the solution.

R46. The UI contract tool MUST parse token JSON and XAML files as static artifacts and MUST NOT require a running app.

R47. The UI contract tool MUST NOT depend on the WinUI app runtime unless a later accepted spec or architecture decision requires it.

### File-list first slice

R48. The first shell region redesigned after foundation resources MUST be file-list rows and selection/focus states.

R49. The first file-list slice MUST use a named file-list item-container style.

R50. The first file-list slice MUST use a named file-list row template.

R51. The first file-list slice MUST provide named row name and metadata text styles.

R52. The first file-list slice MUST provide named row height and icon-size resources.

R53. The first file-list slice MUST cover normal, selected, keyboard-focused, selected-and-focused, multi-selected, hidden/protected, thumbnail fallback, long-name, metadata-heavy, and empty-folder states.

R54. Selected and focused states MUST be visually distinguishable from each other.

R55. Hidden and protected/system rows MUST remain readable while being visually distinguishable from ordinary rows and disabled rows.

R56. Thumbnail fallback/loading presentation MUST NOT change row height or cause row layout jumps.

R57. Long file names MUST truncate or otherwise constrain within the row without overlapping metadata or adjacent UI.

R58. The first file-list slice MUST NOT add a new custom row control.

R59. The first file-list slice MUST NOT add a new selection system, row behavior model, or virtualization behavior.

R60. A custom row control MAY be specified later only if named templates and item-container styles cannot cleanly represent later required visual states.

R61. First-slice motion MUST be limited to hover/focus affordances and MUST NOT animate selection, filtering, listing, or file-operation state changes.

### Fixture mode and visual evidence

R62. The first visual baseline MUST use deterministic view-model or app-shell test fixtures, not generated disk-backed files.

R63. Fixture mode MUST be accepted only when the app is built in an allowed Debug/test context, `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` is present, and the requested fixture name is in a hardcoded allowlist.

R64. Production or Release builds supplied with `--test-ui-fixture` MUST reject the flag and exit nonzero.

R65. Debug/test builds supplied with `--test-ui-fixture` without `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` MUST reject the flag and exit nonzero.

R66. Unknown fixture names MUST be rejected with nonzero exit.

R67. Fixture mode MUST NOT be exposed as a user setting.

R68. Fixture mode MUST NOT accept arbitrary fixture data paths in the first slice.

R69. The first visual fixture set MUST include screenshots for `file-list-normal`, `file-list-selected-row`, `file-list-focused-row`, `file-list-selected-focused-row`, `file-list-multi-selection`, `file-list-hidden-protected`, `file-list-thumbnail-fallback`, `file-list-long-names`, and `file-list-empty-folder`.

R70. First-slice visual baselines MUST use the `dark-comfortable-1440x900-100` profile.

R71. Every committed first-slice visual baseline screenshot MUST have a JSON sidecar.

R72. Each first-slice screenshot sidecar MUST include theme, density, viewport, scale, screen, fixture, dynamic regions, and review ID.

R73. Generated current screenshots and diffs MUST NOT be committed.

R74. Visual baseline updates MUST require an explicit review ID.

R75. Normal CI MUST NOT update committed visual baselines.

R76. Before screenshot comparison is stable, screenshot differences MUST be treated as review evidence rather than a hard release gate.

R77. Once screenshot comparison becomes a hard gate, the accepted threshold SHOULD start at no more than 0.5 percent pixel mismatch or an equivalent accepted perceptual threshold.

R78. Screenshot validation MUST ignore approved dynamic regions such as cursor, timestamps, progress animations, live counts, caret, file timestamps, and non-stable thumbnails.

R79. Generated disk-backed fixtures SHOULD be deferred to later integration visual evidence.

### Design deviations

R80. `docs/ui/design-deviations.md` MUST exist before first-slice implementation begins.

R81. Meaningful deviations from reference material MUST be recorded when they affect visual behavior, accessibility, Windows-native behavior, performance, maintainability, V1 correctness, or reviewability.

R82. Each deviation record MUST include reference pattern, VeloFile decision, reason, user impact, verification, and status.

R83. Deviation status MUST be one of proposed, accepted, temporary, or rejected.

R84. A deviation MUST NOT be accepted solely because XAML makes the reference behavior harder to implement.

## Inputs and outputs

### Inputs

- Accepted proposal decisions for the UI design-system and shell redesign.
- `hifi-design/` reference material, used only for comparison and inspiration.
- `docs/ui/tokens.v1.json`.
- `docs/ui/ui-contract-scopes.v1.json`.
- `docs/ui/design-deviations.md`.
- WinUI resource dictionaries for tokens and first-slice components.
- The first-slice deterministic fixture launch arguments.
- Environment variable `VELOFILE_ENABLE_TEST_UI_FIXTURES`.
- Current screenshots under generated visual-output directories.
- Maintainer-supplied review ID for baseline updates.

### Outputs

- VeloFile-owned token contract.
- VeloFile-owned UI contract scope file.
- Checked-in WinUI resource dictionaries for first-slice tokens and file-list resources.
- Validation output from the UI contract tool.
- Deterministic file-list fixture UI when fixture mode is allowed.
- Committed visual baseline screenshots and JSON sidecars.
- Generated current screenshots and diffs that remain uncommitted.
- Design deviation records.
- User-visible app UI that preserves approved V1 behavior.

## State and invariants

I1. `hifi-design/` remains reference input only.

I2. The first-slice production UI is governed by repo-owned spec, token contract, UI contract scopes, resource dictionaries, and accepted test evidence.

I3. First-slice theme is dark and first-slice density is comfortable.

I4. Theme and density are not persisted by the first slice.

I5. Existing V1 command routes, selection behavior, listing behavior, preview behavior, drag/drop behavior, file-operation behavior, diagnostics behavior, and persistence behavior remain unchanged unless another approved spec changes them.

I6. File-list row visual states do not change row height or break virtualization.

I7. Production builds never enter fixture mode.

I8. Normal CI never mutates committed screenshot baselines.

I9. Generated screenshot current/diff outputs remain uncommitted.

I10. New design-system resources are subject to stricter literal rules than legacy XAML outside the first-slice scope.

## Error and boundary behavior

- Invalid token contract JSON: validation fails with a nonzero exit and reports the invalid file and reason.
- Missing required token key: validation fails with a nonzero exit and reports the token ID and missing XAML key.
- Wrong resource type: validation fails with a nonzero exit and reports expected and observed type.
- Mismatched direct value: validation fails with a nonzero exit unless the token contract explicitly allows non-direct comparison.
- Duplicate resource key in governed dictionaries: validation fails with a nonzero exit.
- Forbidden direct literal in first-slice scope: validation fails with a nonzero exit and reports the file and scope.
- `--test-ui-fixture` in production/Release: app exits nonzero before showing fixture UI.
- `--test-ui-fixture` in Debug without environment guard: app exits nonzero before showing fixture UI.
- Unknown fixture name: app exits nonzero and does not fall back to normal UI.
- Missing current screenshots during baseline update: baseline update command exits nonzero and does not change committed baselines.
- Missing review ID during baseline update: baseline update command exits nonzero and does not change committed baselines.
- Screenshot dimensions do not match the declared profile: validation fails or marks the capture unusable for baseline comparison.
- Dynamic screenshot region not declared: the region is included in comparison until the sidecar or validation policy declares it.

## Compatibility and migration

C1. The first slice MUST be compatible with the existing V1 Windows 10/11 desktop app scope.

C2. The first slice MUST NOT require migration of existing session, settings, favorites, recent locations, diagnostics, or release metadata.

C3. The first slice MUST NOT add persisted theme or density settings.

C4. Rollback of the first slice MUST be possible by removing the first-slice resource, fixture, and validation changes without changing Core or Windows adapter behavior.

C5. Existing tests for approved V1 behavior MUST remain valid and SHOULD continue to pass after the first slice.

C6. First-slice visual evidence MAY use deterministic fixtures, but release claims about filesystem integration still require existing V1 integration/corpus evidence.

C7. A future `DebugUiTest` build configuration MAY be added only after UI automation scope expands beyond the first file-list fixture family.

## Observability

O1. The UI contract validator MUST report actionable validation failures with file path, token ID or scope ID, expected value/type where applicable, and observed value/type where available.

O2. Fixture-mode rejection MUST produce a clear nonzero failure path suitable for CI diagnosis.

O3. Visual baseline sidecars MUST record enough metadata to identify profile, fixture, review, and dynamic-region assumptions.

O4. Baseline update commands MUST make baseline mutation traceable through the supplied review ID.

O5. The first slice MUST NOT add telemetry upload, remote diagnostics, or external reporting.

## Security and privacy

S1. Fixture mode MUST NOT accept arbitrary file paths or untrusted fixture data paths in the first slice.

S2. Fixture mode MUST NOT be enabled in production builds.

S3. Visual baseline sidecars MUST NOT contain raw user file paths, usernames, secrets, file contents, terminal commands, clipboard contents, or preview text.

S4. Deterministic fixture names and file names used for visual evidence MUST be synthetic and non-sensitive.

S5. The UI contract validator MUST treat file paths passed on the command line as local validation inputs only and MUST NOT upload token, XAML, screenshot, or metadata content.

## Accessibility and UX

A11Y1. Keyboard focus MUST remain visible on file-list rows in the first slice.

A11Y2. Selected and focused row states MUST be distinguishable by more than text color alone.

A11Y3. Hidden/protected visual distinction MUST NOT make row text unreadable.

A11Y4. First-slice row text MUST remain legible at the supported visual baseline profile.

A11Y5. Icon-only or ambiguous controls touched by first-slice resources MUST retain accessible names or tooltips already required by V1.

A11Y6. Empty file-list state in the first visual fixture set MUST remain visually distinct from loading, failed, and unsupported states.

A11Y7. Destructive color tokens MUST be reserved for destructive or danger states and MUST NOT be used as ordinary navigation decoration.

## Performance expectations

P1. The first slice MUST preserve file-list virtualization.

P2. The first slice MUST NOT add synchronous filesystem, thumbnail, preview, or metadata work to row rendering.

P3. The first slice MUST NOT animate selection, filtering, listing, thumbnail loading, or file-operation state changes.

P4. The first slice MUST NOT introduce row layout changes that cause row height instability across the first visual fixture states.

P5. Visual validation SHOULD start as soft review evidence and SHOULD NOT block release on pixel-diff thresholds until the automation path is stable.

P6. UI contract validation SHOULD run without launching the app so it can be included in normal local and CI validation.

## Edge cases

EC1. `docs/ui/tokens.v1.json` is missing.

EC2. `docs/ui/tokens.v1.json` is invalid JSON.

EC3. A token declares a XAML key that no resource dictionary defines.

EC4. A color token exists but its brush key points to a different color.

EC5. A governed resource dictionary contains duplicate keys.

EC6. A file-list template uses a hardcoded row height despite a row-height token existing.

EC7. `MainWindow.xaml` contains old unrelated literals outside first-slice scope.

EC8. `MainWindow.xaml` omits `VfFileListRowTemplate` after the first file-list slice.

EC9. Production app is launched with `--test-ui-fixture`.

EC10. Debug app is launched with `--test-ui-fixture` without `VELOFILE_ENABLE_TEST_UI_FIXTURES=1`.

EC11. Debug app is launched with an unknown fixture name.

EC12. Baseline update command is run without `-ReviewId`.

EC13. Baseline update command is run before current screenshots exist.

EC14. Screenshot sidecar viewport does not match actual screenshot dimensions.

EC15. Screenshot includes a live count or caret not declared as dynamic.

EC16. Hidden/protected row styling becomes visually indistinguishable from disabled rows.

EC17. Long filename overlaps metadata columns.

EC18. Thumbnail fallback changes row height.

EC19. Empty-folder fixture accidentally renders stale rows.

EC20. A reference design behavior conflicts with approved V1 behavior.

## Non-goals

- Pixel parity with `hifi-design/`.
- Importing or generating production resources from `hifi-design` tokens.
- Porting JSX or web component structure.
- Runtime theme switching.
- Persisted theme or density settings.
- Compact or spacious density exposure.
- A broad theme engine.
- Design-time tweak controls as production settings.
- Customizable toolbar, color labels, dual-pane browsing, plugin UI, or other V1 non-goals.
- New file-list selection behavior.
- New file-list virtualization behavior.
- New file-operation, preview, search, drag/drop, terminal, persistence, or diagnostics behavior.
- A custom file-list row control in the first slice.
- Hard-gated screenshot pixel diff in the first validation slice.
- Appium/WinAppDriver as the first UI automation path.
- `DebugUiTest` build configuration in the first slice.
- Generated disk-backed visual fixtures for the first file-list baseline.

## Acceptance criteria

AC1. `docs/ui/tokens.v1.json` exists and defines the required first-slice token contract.

AC2. `docs/ui/ui-contract-scopes.v1.json` exists and defines the first active file-list scope.

AC3. `docs/ui/design-deviations.md` exists with status values and records any meaningful reference deviations.

AC4. First-slice WinUI resource dictionaries exist for token resources and file-list component resources.

AC5. The UI contract validator is included in the solution and validates the token contract against the resource dictionaries without launching the app.

AC6. UI contract validation fails for missing required token keys, duplicate governed resource keys, wrong directly comparable values, and forbidden first-slice file-list literals.

AC7. The file-list XAML consumes `VfFileListRowTemplate` and `VfFileListItemContainerStyle` through named resources.

AC8. First-slice file-list visual evidence includes the nine required baseline screens under `tests/visual/baselines/winui/dark-comfortable-1440x900-100/` with JSON sidecars.

AC9. Generated current screenshots and diffs are ignored by Git and are not committed.

AC10. Baseline update refuses to run without a review ID and refuses to run when current screenshots are absent.

AC11. Production/Release app launch with `--test-ui-fixture` exits nonzero and does not render fixture or normal UI.

AC12. Debug/test app launch with `--test-ui-fixture` and no `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` exits nonzero.

AC13. Debug/test app launch with `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` accepts only hardcoded fixture names.

AC14. First-slice row visuals show distinct selected, focused, selected-and-focused, multi-selected, hidden/protected, thumbnail fallback, long-name, and empty-folder states.

AC15. Existing V1 behavior tests for navigation, selection, listing, preview, drag/drop, file operations, persistence, terminal, diagnostics, and accessibility remain applicable and are not replaced by fixture-only visual evidence.

## Open questions

None blocking spec review.

The remaining choices are downstream implementation details:

- Whether scope validation is exposed as `validate-scopes` or as part of a broader `validate-ui-contracts` command.
- The exact process-level error surface used before a WinUI window exists for rejected fixture launches.
- The future threshold for introducing a `DebugUiTest` build configuration.

## Next artifacts

- Matching test spec for UI design-system and shell redesign.
- Execution plan review for the UI design-system and shell redesign plan.

## Follow-on artifacts

- Spec review completed on 2026-05-11 with status `approved` and no material findings.
- Architecture update completed in [docs/architecture/system/architecture.md](../docs/architecture/system/architecture.md).
- ADR 0009 records UI design contracts, static validation, and visual fixture decisions.

## Readiness

Approved by `spec-review` and ready for downstream planning and test-spec artifacts. Implementation must wait for required plan-review and test-spec gates.
