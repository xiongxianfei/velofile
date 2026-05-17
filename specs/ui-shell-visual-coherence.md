# UI Shell Visual Coherence

## Status

approved; amended 2026-05-17 to remove mandatory visual-evidence gates by spec-review-r2

## Related proposal

- [VeloFile Shell Visual Coherence Follow-up Proposal](../docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md)
- Builds on [UI Design System and Shell Redesign](ui-design-system-shell-redesign.md)

## Goal and context

This spec defines the shell-wide visual-coherence contract for VeloFile's WinUI shell after the first UI design-system slice. It governs how the app root, chrome, sidebar, command/path/search band, file list, status/operation surfaces, preview/details pane, dialogs, flyouts, deterministic fixture icons, and optional visual-review artifacts should behave as one product surface.

The existing first-slice spec remains authoritative for first-slice token resources, file-list row resources, deterministic file-list fixtures, and initial visual baselines. This spec is a separate follow-on contract. It extends the accepted VeloFile-owned design-system model across the whole shell without rewriting the first-slice spec or changing approved V1 file-manager behavior.

`hifi-design/` remains reference material only. It may inform quality review and design-deviation records, but it is not authoritative for production tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria.

## Glossary

- **Shell visual coherence**: the observable condition where app root, chrome, sidebar, command band, content/file list, status, operation, preview, dialog, and flyout surfaces share a tokenized visual model and do not look like unrelated regions.
- **Shell surface foundation**: the tokenized app-wide surface, border, text, focus, selection, spacing, radius, sizing, and state rules that other shell regions consume.
- **Command band**: the shell area containing navigation buttons, path/breadcrumb entry, filter controls, recursive search controls, and related command/status affordances.
- **Navigation-first sidebar**: a sidebar information architecture where locations, favorites, recent entries, and drives are visually primary, while visibility toggles and terminal controls remain discoverable but secondary.
- **Governed region**: a XAML/resource scope listed in `docs/ui/ui-contract-scopes.v1.json` where token and literal rules apply.
- **Fixture icon kind**: an allowlisted deterministic identifier such as `Folder` or `Pdf` used by fixture rows to select a named vector icon resource.
- **Effective window size**: the app-window layout size in effective pixels, not physical display pixels.
- **Review profile**: a named optional screenshot profile combining effective window size, scale, theme, and density.
- **Optional visual-review artifact**: a screenshot, sidecar, or manual visual note that may support human review but is not a milestone, final-closeout, or release-readiness gate under this spec.

## Examples first

### Example E1: shell surfaces share one visual model

Given the app renders the default shell in the `shell-standard-1440x900-100` profile
When a reviewer compares app root, chrome, sidebar, command band, file list, and status surfaces
Then those regions use the VeloFile dark comfortable surface family
And no governed region appears as a raw light/default WinUI surface without an accepted deviation.

### Example E2: fixture icons are deterministic vectors

Given a deterministic file-list fixture row has icon kind `Pdf`
When the row is rendered in a governed fixture/file-list icon scope
Then the icon is rendered from a named `VfIconGeometry*` vector resource through the governed icon container and path styles
And the row does not show an ellipsized text chip such as `P...`.

### Example E3: sidebar reordering preserves behavior

Given the sidebar is redesigned as navigation-first
When a keyboard user traverses the sidebar
Then locations, favorites, recent entries, drives, visibility toggles, and terminal controls remain reachable
And each reachable group/control has an accessible name that matches its purpose.

### Example E4: minimum shell layout remains usable

Given the shell runs at the supported minimum layout size
When the file-list selected/focused state is visible or exercised by focused layout/resource checks
Then primary navigation controls are not clipped
And the path/search band remains reachable
And the sidebar does not obscure the content region.

### Example E5: optional screenshots do not become gates

Given automated screenshot capture is unavailable, noisy, or not maintained for the shell slice
When the slice is reviewed
Then the slice can still close through static resource checks, behavior-preservation tests, accessibility checks, and deviation records
And any optional screenshot or manual visual note that is recorded is labeled as supporting review context only.

### Example E6: behavior preservation is explicit

Given a shell region slice changes the command band
When the slice is reviewed
Then the behavior-preservation matrix identifies affected navigation, path entry, filter/search, accessibility, and diagnostics routes
And cites tests or manual evidence for each touched behavior.

### Example E7: visual-review artifacts are optional across milestones

Given a region milestone changes the file list, command band, sidebar, status surface, or preview pane
When the milestone is reviewed
Then static resource tests, behavior-preservation tests, accessibility checks, and design-deviation records remain required
And the absence of screenshots or manual visual notes does not block the milestone.

## Requirements

### Authority and scope

R1. This spec MUST be a separate follow-on contract and MUST NOT rewrite or supersede `specs/ui-design-system-shell-redesign.md`.

R2. `specs/ui-design-system-shell-redesign.md` MUST remain authoritative for first-slice token and file-list row work unless a later approved spec explicitly changes that scope.

R3. This spec MUST govern shell-wide surface foundation, command band, sidebar, status/operation surfaces, preview/details visual treatment, deterministic fixture icon strategy, and optional visual-review artifact guardrails.

R4. `hifi-design/` MUST remain reference input only and MUST NOT become the production source of truth for tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria.

R5. The follow-on shell visual-coherence work MUST preserve approved V1 behavior for navigation, tabs/session restore, listing/virtualization, selection, filter/search, context menu, file operations, drag/drop, preview, terminal launch, diagnostics, persistence, and accessibility routes unless another accepted spec changes that behavior.

R6. This spec MUST NOT introduce persisted theme or density preferences.

R7. This spec MUST NOT introduce a broad theme engine, per-component customization system, tweak panel, plugin UI, color-label system, dual-pane browsing, or non-V1 file-manager feature expansion.

R8. The shell visual-coherence baseline MUST remain dark and comfortable for the first follow-on implementation slice.

### Token and scope contracts

R9. Shell-wide visual-coherence tokens MUST extend `docs/ui/tokens.v1.json` additively unless an incompatible token semantics change, renamed resource contract, or deliberate redesign reset is approved.

R10. Shell-wide governed scopes MUST extend `docs/ui/ui-contract-scopes.v1.json` additively unless an incompatible scope semantics change or deliberate redesign reset is approved.

R11. A new token major version MUST NOT be introduced solely because new shell regions enter scope.

R12. Governed shell regions MUST use VeloFile-owned token/resource references for colors, brushes, typography, spacing, sizing, radius, border/separator treatment, focus, selection, hover, disabled, warning, danger, success, and icon sizing when those values are tokenized.

R13. Governed shell regions MUST NOT use unapproved local visual literals for tokenized colors, sizes, spacing, radii, focus thickness, opacity, or icon sizes.

R14. Governed shell regions MAY retain old local literals outside the active scope until that region enters redesign scope.

R15. Meaningful differences from `hifi-design/` that affect shell visual behavior, accessibility, Windows-native behavior, performance, maintainability, V1 correctness, or reviewability MUST be recorded in `docs/ui/design-deviations.md`.

### Visual-coherence rubric

R16. A shell slice MUST be considered visually coherent only when app root, chrome, sidebar, content, command band, file list, and status surfaces share one tokenized surface model.

R17. A shell slice MUST visually prioritize current location, active tab, selected files, active operation, and unsafe actions in that order.

R18. Governed regions MUST NOT use raw/default control visuals without an accepted design deviation.

R19. Focus and selection states MUST be distinct, accessible, and not confused with warning or error states.

R20. File and folder icon surfaces in governed fixture/file-list scopes MUST be deterministic and polished, and fixture rows MUST NOT use truncated placeholder chips.

R21. Sidebar hierarchy MUST present navigation before secondary visibility/settings controls.

R22. Region-slice visual coherence MUST be evaluated through static resource contracts, behavior-preservation evidence, accessibility checks, and design-deviation records. Optional full-shell screenshots or manual visual notes MAY support review, but their absence MUST NOT block milestone closeout.

R22A. No shell visual-coherence region slice requires a visual-evidence deferral record. Prior M3-only visual-evidence deferral rules are superseded by this amendment.

### Shell surface foundation

R23. The first follow-on implementation region MUST be shell surface foundation.

R24. Shell surface foundation MUST define and apply a coherent surface family for app root, chrome, sidebar, content, elevated/flyout surfaces, borders, separators, text hierarchy, accent/focus, selection, hover, disabled, danger, warning, and success states.

R25. Shell surface foundation MUST define or reuse tokenized values for row height, toolbar height, path bar height, sidebar width, control radius, spacing rhythm, and focus thickness.

R26. Shell surface foundation MUST make the app root, chrome, sidebar, command band, file list, status area, and preview/details region visually compatible through governed resources, focused UI contract tests, and behavior-preservation checks.

R27. Shell surface foundation MUST NOT remove or hide existing V1 routes for navigation, tabs/session restore, listing, selection, filter/search, context menu, file operations, drag/drop, preview, terminal launch, diagnostics, or accessibility.

### File-list polish and deterministic icons

R28. The second follow-on implementation region MUST be file-list polish and deterministic icon treatment.

R29. File-list polish MUST cover normal, hover, selected, focused, selected-focused, multi-selected, hidden, protected/system, long filename, folder, thumbnail fallback, and empty-folder states.

R30. File-list focused state MUST use VeloFile focus treatment and MUST NOT use danger/warning/error color treatment unless the row itself is in an error state.

R31. File-list selected, focused, and selected-focused states MUST remain visually distinguishable by more than text color alone.

R32. File-list hidden and protected/system rows MUST remain readable and MUST NOT look disabled or broken.

R33. File-list row rhythm MUST remain stable; hover, focus, selection, icon kind, long name, hidden/protected state, and thumbnail fallback MUST NOT change row height.

R34. Heavy row separators SHOULD be replaced or reduced by tokenized list rhythm because separators must not dominate row content.

R35. Governed fixture/file-list icon resources MUST live in `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`.

R36. Deterministic fixture icons MUST be represented as named vector geometry resources consumed by raw `Path` elements inside a fixed icon container.

R37. Governed fixture/file-list icon resources MUST include `VfFileListIconContainerStyle`, `VfFileListIconPathStyle`, and `VfFileListFixtureIconTemplate`.

R38. Governed fixture/file-list icon resources MUST include named geometry resources for `FileGeneric`, `Folder`, `Pdf`, `Image`, `Text`, `Spreadsheet`, `Executable`, `Markdown`, and `ThumbnailFallback`.

R39. Fixture rows MUST expose an allowlisted icon kind and MUST NOT expose arbitrary icon resource keys.

R40. Fixture icon-kind mapping MUST be hardcoded and allowlisted for fixture mode.

R41. Governed fixture/file-list icon surfaces MUST NOT use `SymbolIcon`, `PathIcon`, private-use glyph fonts, or ellipsized extension text chips such as `P...`, `D...`, or `T...`.

R42. Governed fixture/file-list icon foreground, background, and size values MUST resolve from VeloFile resources rather than unapproved local literals.

R43. Real Windows Shell icons SHOULD be deferred to later integration evidence and MUST NOT be required for the first deterministic fixture baselines.

### Command band

R44. The command band MUST present navigation/path context and filter/search/status actions as an intentional region rather than unrelated raw controls.

R45. Command band controls that enter governed scope MUST share tokenized height, radius, spacing, typography, hover, focus, pressed, disabled, and input border/background states.

R46. The command band MUST keep current location visually clear without making path chips or path input look like unrelated selected tabs.

R47. The command band MUST preserve access to back, forward, up, refresh, path/breadcrumb entry, current-folder filter, recursive search, search start, cancel, and clear actions that already exist in V1.

R48. Disabled command band controls MUST be visibly disabled and remain distinguishable from active controls without relying on color alone.

R49. The follow-on spec MAY allow a two-row command band if needed for clarity, but the behavior contract MUST preserve keyboard and accessibility routes for each command.

### Sidebar

R50. Sidebar restructuring MUST be treated as observable shell behavior, not cosmetic styling only.

R51. The sidebar MUST preserve access to existing locations, favorites, recent entries, drives, visibility toggles, and terminal controls.

R52. Sidebar grouping MUST present locations, favorites, recent entries, and drives before secondary visibility/settings controls in the default navigation-first layout.

R53. Sidebar visibility toggles and terminal controls MUST remain discoverable and keyboard reachable after reordering.

R54. Sidebar keyboard traversal order MUST match the visual grouping order unless an accepted accessibility deviation records a better order.

R55. Sidebar groups and controls MUST have accessible names that identify their purpose.

R56. Sidebar selected, hover, focus, and disabled states in governed scope MUST use VeloFile resources.

### Status, operation, and destructive surfaces

R57. Status and operation surfaces MUST share the shell surface foundation and MUST NOT look like unrelated default platform controls.

R58. Active operation state MUST be visible without obscuring primary navigation controls in supported shell layouts.

R59. Destructive confirmation state MUST be visually distinct from ordinary navigation and operation states.

R60. Permanent delete or other destructive actions MUST use danger treatment only for destructive meaning and MUST NOT share ordinary focus/accent styling.

R61. Operation-running, cancelled, failed, conflict, and destructive confirmation visuals MUST preserve existing operation behavior unless another accepted spec changes that behavior.

### Preview/details pane

R62. Preview/details pane visual treatment MUST share the shell surface foundation.

R63. Preview/details pane loading, success, unsupported, failed, metadata, image, text, and PDF states MUST remain visually distinguishable when those states enter scope.

R64. Preview/details pane visual changes MUST NOT destabilize file-list row height or selection/focus visibility.

R65. Preview/details pane visual changes MUST preserve existing preview selection, loading, timeout, unsupported, failed, and PDF navigation behavior unless another accepted spec changes that behavior.

### Optional visual-review artifacts

R66. Full-shell screenshots and manual visual-review notes are optional supporting artifacts. They MUST NOT be required before region milestone closeout, M8 closeout, final closeout, verification, or release readiness unless a later accepted spec reinstates a visual-evidence gate.

R67. Optional full-shell screenshots MAY use states such as `shell-default`, `shell-file-list-selected-focused`, `shell-filter-active`, `shell-search-active`, `shell-preview-open`, `shell-operation-running`, and `shell-destructive-confirmation` when they are useful for review.

R68. Optional screenshots MAY use review profiles such as `shell-min-900x560-100`, `shell-standard-1440x900-100`, and `shell-standard-1440x900-200`, but missing profile coverage MUST NOT block a milestone.

R69. Minimum-size and high-DPI usability MUST be protected by static resource/layout checks, focused app tests, accessibility checks, or explicit manual behavior notes when touched. Optional screenshots MAY support that review but are not the required proof.

R70. If optional screenshots are recorded, `shell-standard-1440x900-100` SHOULD be treated as the primary review profile unless the reviewer records another profile rationale.

R71. If optional `shell-standard-1440x900-200` evidence is recorded, it SHOULD be used to review readable text, crisp icons, visible focus ring, no clipped controls from scale conversion, and stable row rhythm/spacing.

R72. `shell-stress-720x500-100` MAY be used as advisory stress evidence but MUST NOT be treated as a required pass/fail profile unless a later accepted spec lowers VeloFile's supported minimum.

R73. Screenshot evidence MUST remain optional soft-review evidence until visual comparison stability is accepted by a later spec or architecture decision.

R74. If screenshot sidecars are committed or referenced as review artifacts, they MUST include profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review ID.

R75. Screenshot sidecars and manual visual notes MUST NOT include raw local user file paths, usernames, secrets, file contents, terminal commands, clipboard contents, or preview text.

R76. Generated current screenshots and diffs MUST NOT be committed.

R77. Normal CI MUST NOT mutate committed screenshot baselines.

### Behavior preservation

R78. The follow-on test spec MUST include a behavior-preservation matrix covering navigation, tabs/session restore, listing/virtualization, selection, filter/search, context menu, file operations, drag/drop, preview, terminal launch, diagnostics, persistence, and accessibility routes.

R79. Each shell region slice MUST identify which behavior-preservation matrix rows it touches.

R80. Each touched behavior row MUST cite automated tests or explicit manual evidence before the slice is accepted.

R81. Fixture-only visual artifacts MUST NOT be presented as proof of real filesystem, Windows adapter, drag/drop, preview, file-operation, terminal, or diagnostics behavior.

R82. Existing V1 behavior tests MUST remain applicable and MUST NOT be replaced by screenshot-only or fixture-only proof.

## Inputs and outputs

### Inputs

- Accepted shell visual-coherence proposal.
- Existing first-slice UI design-system spec and artifacts.
- `hifi-design/` reference material, used only for comparison and deviation review.
- `docs/ui/tokens.v1.json`.
- `docs/ui/ui-contract-scopes.v1.json`.
- `docs/ui/design-deviations.md`.
- WinUI resource dictionaries for shell, component, and icon resources.
- Deterministic fixture launch arguments and fixture rows.
- Environment variable `VELOFILE_ENABLE_TEST_UI_FIXTURES`.
- Optional current screenshots and sidecar metadata under generated visual-output directories.
- Maintainer review ID for optional visual baseline updates.

### Outputs

- Additive token and scope contract entries for shell-wide governed regions.
- Checked-in shell, component, and fixture icon resource dictionaries.
- Static UI contract validation output for governed shell and fixture icon scopes.
- Deterministic full-shell fixture UI when fixture mode is allowed.
- Optional full-shell screenshot artifacts and sidecar metadata.
- Design deviation records for meaningful reference or implementation deviations.
- User-visible shell UI that preserves approved V1 behavior.

## State and invariants

I1. The accepted first-slice UI design-system spec remains authoritative for its scope.

I2. `hifi-design/` remains reference input only.

I3. The shell visual-coherence baseline is dark and comfortable.

I4. Theme and density preferences are not persisted by this spec.

I5. Shell-wide tokens and scope rules are additive V1 extensions unless an incompatible redesign reset is approved.

I6. Core services and Windows adapter boundaries remain unchanged by visual-coherence work.

I7. Deterministic fixture icon selection uses allowlisted icon kinds, not arbitrary resource keys.

I8. Fixture mode remains non-production and guarded by the existing fixture-mode rules.

I9. Full-shell screenshots are optional review artifacts until hard comparison gates are specified by a later accepted contract.

I10. Generated current screenshots and diffs remain uncommitted.

## Error and boundary behavior

- Missing required shell token: validation fails nonzero and reports the missing token/key.
- Missing required shell scope reference: validation fails nonzero and reports the scope and missing reference.
- Governed region uses an unapproved local visual literal: validation fails nonzero and reports the file and rule.
- Governed fixture icon resource uses `SymbolIcon`, `PathIcon`, private-use glyph font, or ellipsized chip text: validation fails nonzero and reports the offending file and rule.
- Fixture row requests an unknown icon kind: fixture launch or fixture validation fails nonzero and does not fall back to arbitrary resource lookup.
- Optional screenshot sidecar is missing profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, or review ID: optional artifact validation fails or marks the screenshot unusable for review.
- Automated `200%` scale capture is unavailable: the milestone may still close; optional high-DPI review can use static checks, app tests, or manual notes when high-DPI risk is touched.
- Screenshot dimensions or sidecar effective window size do not match the declared profile: optional artifact validation fails or marks the capture unusable for that profile.
- A full-shell state cannot be produced by deterministic fixture mode: the follow-on plan must not claim automated visual artifact coverage for that state.
- A milestone closes without screenshots or manual visual notes: the milestone remains valid when static resource validation, behavior-preservation checks, accessibility checks, and required deviation records pass.
- Optional visual artifacts are presented as behavior or release proof: review fails because visual artifacts are supporting context only.
- Sidebar reordering removes access to an existing sidebar function: the slice fails behavior preservation.
- A visual slice changes Core/Windows behavior without an accepted behavior spec: the change is out of scope and must be reverted or respecified.

## Compatibility and migration

C1. This spec MUST remain compatible with the existing V1 Windows 10/11 desktop app scope.

C2. This spec MUST NOT require migration of existing session, settings, favorites, recent locations, diagnostics, release metadata, or visual baseline metadata from the first-slice work.

C3. This spec MUST NOT add persisted theme or density settings.

C4. This spec MUST NOT introduce a new token major version unless an incompatible token semantics change, renamed resource contract, or deliberate redesign reset is approved.

C5. Rollback MUST be possible per governed region by reverting that region's resources, scoped XAML usage, tests, and optional visual artifacts without changing Core or Windows adapters.

C6. Existing first-slice visual baselines remain valid for the first-slice scope; optional follow-on full-shell baselines supplement rather than replace them when they exist.

C7. Real Windows Shell icons remain future integration evidence and are not required for deterministic fixture baselines.

## Observability

O1. UI contract validation failures MUST identify the file, scope or token, expected rule, and observed violation when available.

O2. Fixture icon validation failures MUST identify the fixture icon kind or resource usage that violated the allowlist.

O3. Screenshot sidecars, when recorded, MUST make optional artifacts traceable by profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review ID.

O4. Baseline update commands, when used, MUST make baseline mutation traceable through the supplied review ID.

O5. This spec MUST NOT add telemetry upload, remote diagnostics, or external reporting.

## Security and privacy

S1. Fixture mode MUST NOT accept arbitrary fixture data paths or arbitrary icon resource keys.

S2. Fixture mode MUST NOT be enabled in production builds.

S3. Full-shell screenshot sidecars and manual visual notes MUST NOT include raw local user paths, usernames, secrets, file contents, terminal commands, clipboard contents, or preview text.

S4. Deterministic fixture file names, folder names, and metadata used for optional visual artifacts MUST be synthetic and non-sensitive.

S5. UI contract validation MUST treat file paths passed on the command line as local validation inputs only and MUST NOT upload token, XAML, screenshot, or sidecar content.

S6. Destructive confirmation fixture evidence MUST use synthetic file names and MUST NOT display real local file paths.

## Accessibility and UX

A11Y1. Keyboard focus MUST remain visible in every governed shell region.

A11Y2. Focus, selection, warning, danger, and disabled states MUST be distinguishable by more than color alone where user action or risk depends on the distinction.

A11Y3. Focus treatment MUST NOT use danger/warning/error color semantics for ordinary keyboard focus.

A11Y4. Icon-only or ambiguous controls in governed shell regions MUST have accessible names or tooltips.

A11Y5. Sidebar reordered groups and controls MUST have accessible names and keyboard order matching visual grouping unless a deviation is accepted.

A11Y6. Text in governed shell regions MUST remain readable and must not overlap adjacent controls.

A11Y7. High-DPI readability for text, icons, focus rings, and clipped controls MUST be covered by static/resource tests, app tests, or explicit manual notes when a touched region creates high-DPI risk.

A11Y8. Hidden/protected file-list visual treatment MUST NOT make file names unreadable.

A11Y9. Destructive confirmation styling MUST make the destructive action clear without stealing default focus from safer navigation or cancellation paths unless another accepted spec requires it.

## Performance expectations

P1. Visual-coherence work MUST preserve file-list virtualization.

P2. Visual-coherence work MUST NOT add synchronous filesystem, thumbnail, preview, metadata, terminal discovery, or diagnostics work to row rendering.

P3. Deterministic fixture icons MUST NOT require Shell icon extraction, thumbnail extraction, filesystem enumeration, or network access.

P4. Hover, focus, selection, icon kind, hidden/protected state, thumbnail fallback, and long-name states MUST NOT change row height.

P5. Full-shell visual fixture capture SHOULD use deterministic fixture state rather than disk-backed state for first review evidence.

P6. Static UI contract validation SHOULD run without launching the app.

P7. Screenshot comparison MUST NOT become a hard gate under this spec.

## Edge cases

EC1. A governed shell surface still uses a raw light/default control style.

EC2. A redesigned file-list row uses a red/pink danger-like focus outline for ordinary keyboard focus.

EC3. A fixture row attempts to show `P...`, `D...`, `T...`, or another ellipsized text chip as its icon.

EC4. A fixture row supplies an unknown icon kind.

EC5. A fixture row supplies a raw resource key instead of an allowlisted icon kind.

EC6. `VeloFile.FixtureIcons.xaml` omits a required `VfIconGeometry*` resource.

EC7. Governed icon resources use `SymbolIcon`, `PathIcon`, or private-use glyph font values.

EC8. Governed icon resources hardcode local foreground/background colors or icon sizes when tokenized resources exist.

EC9. Minimum supported shell size clips primary navigation controls.

EC10. Minimum supported shell size lets the sidebar obscure the content/file-list region.

EC11. High-DPI execution shows clipped controls or unreadable text in a touched region.

EC12. Optional screenshot automation is unavailable or noisy.

EC13. Optional screenshot sidecar scale does not match the declared profile.

EC14. Optional screenshot sidecar or manual visual note includes raw local paths or user-identifying data.

EC15. Optional full-shell screenshots show a new mismatch between redesigned and non-redesigned regions.

EC16. Sidebar reordering hides visibility toggles or terminal controls.

EC17. Sidebar keyboard order does not match visual grouping.

EC18. Command band restyle changes path entry, filter, or search behavior.

EC19. Operation/status visual changes obscure primary navigation.

EC20. Preview/details visual changes cause row height instability or hide selected/focused rows.

EC21. A region slice claims behavior preservation using screenshots only.

EC22. A design deviation is required but not recorded.

EC23. A follow-on token change requires incompatible semantics but is added silently to `tokens.v1.json`.

EC24. Real Shell icons are used in first fixture baselines and make screenshots nondeterministic.

## Non-goals

- Replacing the accepted first-slice UI design-system spec.
- Pixel parity with `hifi-design/`.
- Importing or generating production resources from `hifi-design`.
- Porting JSX, CSS, or web component structure.
- Persisted theme or density preferences.
- Broad theme engine, tweak panel, per-component customization, color-label system, plugin UI, or dual-pane browsing.
- New navigation, listing, selection, search, preview, drag/drop, file-operation, terminal, diagnostics, persistence, or Windows adapter behavior.
- New file-list selection system, row behavior model, virtualization behavior, or custom row control.
- Real Windows Shell icons for first deterministic fixture baselines.
- Hard-gated screenshot pixel comparison.
- New `DebugUiTest` build configuration unless a later architecture decision approves it.
- Lowering the supported minimum shell size to `720 x 500`.
- Treating fixture-only or screenshot-only evidence as proof of real platform integration behavior.

## Acceptance criteria

AC1. `specs/ui-shell-visual-coherence.md` exists as a separate follow-on spec and links to the accepted proposal and first-slice spec.

AC2. `docs/ui/tokens.v1.json` contains additive shell-wide token entries or the follow-on implementation documents why no new token entry is needed for a governed region.

AC3. `docs/ui/ui-contract-scopes.v1.json` contains additive governed shell scopes for each implemented follow-on region.

AC4. Governed shell surface resources render app root, chrome, sidebar, command band, file list, status, and preview/details surfaces through VeloFile resources.

AC5. Static UI contract validation fails for unapproved local literals in governed shell regions.

AC6. `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` exists and defines the required icon geometry resources and file-list icon styles.

AC7. Static validation fails if governed fixture/file-list icon resources use `SymbolIcon`, `PathIcon`, private-use glyph fonts, ellipsized text chips, unapproved icon colors, or unapproved icon sizes.

AC8. Deterministic fixture rows use allowlisted icon kinds for `FileGeneric`, `Folder`, `Pdf`, `Image`, `Text`, `Spreadsheet`, `Executable`, `Markdown`, and `ThumbnailFallback`.

AC9. Region milestones can close without screenshot or manual visual-review artifacts when static resource validation, behavior-preservation checks, accessibility checks, and required deviation records pass.

AC10. Optional screenshot or manual visual-review artifacts, when recorded, are labeled as supporting review context and not behavior or release proof.

AC11. Optional profile coverage gaps, including `shell-standard-1440x900-200`, do not block closeout unless a later accepted spec reinstates a visual-evidence gate.

AC12. Screenshot sidecars that are committed or referenced include profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review ID.

AC13. Generated current screenshots and diffs remain uncommitted.

AC14. Sidebar redesigned layout preserves access, keyboard reachability, accessible names, and discoverability for locations, favorites, recent entries, drives, visibility toggles, and terminal controls.

AC15. Command band redesigned layout preserves existing navigation, path, filter, search, cancel, and clear routes.

AC16. File-list focused and selected states remain distinguishable and ordinary focus does not look like warning/error/danger state.

AC17. The behavior-preservation matrix exists in the matching test spec and covers every behavior listed in R78.

AC18. Each implemented shell region slice cites automated tests or manual evidence for touched behavior-preservation matrix rows.

AC19. Design deviations are recorded for meaningful mismatches between the reference material and VeloFile's accepted product UI.

AC20. Existing V1 behavior tests remain applicable and are not replaced by fixture-only visual evidence.

AC21. Prior M3-only visual-evidence deferral records remain historical context only and no longer block M8 or final closeout.

## Open questions

This amendment removes mandatory visual-evidence gates and should return to `spec-review` before implementation reviews rely on the changed contract.

The exact vector geometry path data and exact fixture row-to-icon examples are downstream architecture, test-spec, and plan details.

## Next artifacts

- `spec-review` for this spec.
- Architecture or ADR update for shell-wide additive token/scope validation, fixture icon resources, optional visual-review artifact guardrails, and high-DPI behavior handling if the existing architecture does not already cover them.
- Matching test spec at `specs/ui-shell-visual-coherence.test.md`.
- Execution plan at `docs/plans/2026-05-11-ui-shell-visual-coherence.md` after spec and architecture review.

## Follow-on artifacts

- Spec reviews completed with status `approved` and no material findings, including spec-review-r2 for the 2026-05-17 visual-evidence gate removal amendment.
- Architecture update drafted in [docs/architecture/system/architecture.md](../docs/architecture/system/architecture.md).
- ADR 0010 drafted at [docs/adr/0010-shell-visual-coherence-contracts.md](../docs/adr/0010-shell-visual-coherence-contracts.md).
- 2026-05-17 amendment drafted to remove mandatory visual-evidence gates and make screenshots/manual visual notes optional supporting artifacts.

## Readiness

Spec-review approved for the 2026-05-17 amendment that removes mandatory visual-evidence gates. The amended contract preserves V1 behavior, keeps first-slice artifacts authoritative for their scope, and treats screenshots/manual visual notes as optional supporting review artifacts rather than closeout gates. Next required repository stage is architecture-review for the matching architecture and ADR amendment.

## References

- [Microsoft Learn: Screen sizes and breakpoints](https://learn.microsoft.com/en-us/windows/apps/design/layout/screen-sizes-and-breakpoints-for-responsive-design)
- [Microsoft Learn: DPI and device-independent pixels](https://learn.microsoft.com/en-us/windows/win32/learnwin32/dpi-and-device-independent-pixels)
