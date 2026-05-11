# VeloFile Shell Visual Coherence Follow-up Proposal

## Status

accepted

## Problem

The current WinUI shell exposes the right behavioral surface, but the visible UI still reads like a functional test harness rather than one designed product. The problem is systemic rather than isolated: chrome, sidebar, toolbar, path and search controls, file-list rows, focus, selection, icons, spacing, and density do not yet share a coherent visual system.

The accepted UI design-system and shell redesign proposal established the correct authority model: VeloFile owns its WinUI-native UI contract, while `hifi-design/` remains reference input only. The first approved slice focuses on design-system foundations and file-list row presentation. The next product-quality decision is whether to extend that work into whole-shell visual coherence, using full-shell screenshots to catch mismatches that static checks and isolated row fixtures cannot see.

The screenshot-driven concerns are concrete:

- the shell mixes light sidebar/chrome surfaces with a dark file list;
- the path, toolbar, filter/search controls, sidebar, and file list compete for attention;
- raw WinUI controls are still too visible as unrelated platform defaults;
- file-list rows are heavy and over-separated;
- focused rows use a loud red/pink outline that reads closer to debug or error state than keyboard focus;
- file-type chips show truncated labels such as `P...`, `D...`, and `T...`, which looks like placeholder data;
- spacing rhythm is inconsistent across toolbar buttons, path chips, filters, row gaps, and sidebar controls;
- the sidebar is structurally present but still feels settings-first rather than navigation-first.

## Goals

- Extend the accepted VeloFile UI design-system direction from first-slice file-list work into whole-shell visual coherence.
- Make app chrome, sidebar, toolbar, path/search band, file list, status area, preview pane, dialogs, and flyouts share one tokenized surface model.
- Keep `hifi-design/` as a quality benchmark only, not a production contract.
- Preserve approved V1 behavior for navigation, listing, selection, search, preview, drag/drop, file operations, persistence, diagnostics, terminal launch, and accessibility routes.
- Treat the earlier icon glyph regression as a reusable lesson: diagnose visual defect classes, scan affected surfaces, add invariant/static checks, and pair them with visual proof when feasible.
- Add full-shell screenshot review evidence in addition to focused file-list fixture evidence.
- Replace placeholder-like file-type chips with a deterministic, polished icon strategy for fixtures and later production icon work.
- Rebalance sidebar hierarchy so navigation is primary and visibility/settings controls are secondary.

## Non-goals

- This proposal does not replace the accepted UI design-system and shell redesign proposal.
- This proposal does not implement the UI changes.
- This proposal does not downgrade, supersede, or rewrite the approved first-slice spec, architecture, plan, or change evidence.
- This proposal does not make `hifi-design/` authoritative.
- This proposal does not introduce persisted theme or density preferences.
- This proposal does not add a broad theme engine, per-component customization, tweak panel, plugin UI, color-label system, dual-pane browsing, or other V1 non-goals.
- This proposal does not require pixel-perfect screenshot matching or hard pixel-diff gates.
- This proposal does not replace WinUI 3, C#, or the existing Core/App/Windows architecture.

## Vision fit

fits the current vision

The proposal supports VeloFile's current vision by improving the clarity and trustworthiness of the Windows-native daily file-manager experience without expanding V1 into a broader power-user suite. A coherent shell helps users browse, filter, search, preview, and act on files quickly while preserving the approved narrow product scope.

## Initial intent preservation

| Initial user goal | Proposal treatment | Where recorded |
|---|---|---|
| Treat the current screenshot as evidence that the behavioral surface exists but the UI lacks product coherence. | in scope | Problem, Context, Recommended direction |
| Stop patching individual controls and use design-system best practices. | in scope | Goals, Options considered, Recommended direction |
| Keep `hifi-design/` as a quality reference, not a literal source of truth. | in scope | Goals, Non-goals, Recommended direction |
| Apply the icon regression lesson to visual work. | in scope | Goals, Testing and verification strategy, Risks and mitigations |
| Define shell foundation surfaces before more isolated row patches. | in scope | Recommended direction, Architecture impact |
| Keep file list as the first product-critical visual region. | in scope | Recommended direction, Expected behavior changes |
| Replace truncated file-type chips with deterministic polished icons. | in scope | Goals, Expected behavior changes |
| Redesign top command/path/search area as one navigation band. | in scope | Expected behavior changes, Next artifacts |
| Reorder sidebar toward navigation before settings. | in scope | Goals, Expected behavior changes |
| Use tokenized visual states and static validation. | in scope | Architecture impact, Testing and verification strategy |
| Add screenshot-driven review without hard pixel gates. | in scope | Testing and verification strategy |
| Evaluate whole-shell coherence after each region slice. | in scope | Recommended direction, Testing and verification strategy |
| Preserve V1 behavior while improving UI quality. | in scope | Goals, Non-goals, Rollout and rollback |

## Context

The repository already has an accepted UI design-system and shell redesign proposal at `docs/proposals/2026-05-11-ui-design-system-shell-redesign.md`, an approved first-slice spec at `specs/ui-design-system-shell-redesign.md`, and an active execution plan at `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`.

That approved spec states that the first redesign slice is limited to design-system foundation resources and file-list row presentation. It also states that later shell regions should extend the spec or add follow-on specs before implementation. This proposal is that follow-on product-direction artifact for whole-shell visual coherence.

The screenshot critique suggests the first-slice design-system direction is necessary but not sufficient by itself. A row can be improved while the shell still looks incoherent if the surrounding surfaces remain raw, light/dark split, or visually unrelated. Full-shell visual evidence is therefore needed alongside focused file-list fixture evidence.

The earlier icon glyph regression lesson matters here because several visible problems are defect classes, not isolated symptoms. Placeholder-looking icon chips, loud focus outlines, ungoverned local literals, and inconsistent spacing should be addressed through contract and proof, not only through local XAML edits.

## Options considered

### Option A: Continue patching visible issues one control at a time

This would let the project react quickly to screenshot problems, but it would preserve the current failure mode. Local fixes to row colors, button styles, or sidebar spacing can make one region better while creating new mismatches elsewhere.

### Option B: Replace the current shell with a direct hi-fi port

This could produce a more dramatic visual change, but it would put the reference artifact in the wrong authority position and risk importing web/JSX/CSS assumptions into WinUI. It would also increase the chance of breaking existing V1 behavior.

### Option C: Treat the file-list slice as complete before touching the rest of the shell

This keeps the current plan narrow, but it risks polishing a dark file list inside an otherwise disconnected shell. The screenshot already shows that whole-shell coherence has to be reviewed while region work continues.

### Option D: Add a follow-on shell coherence spec using the accepted design-system authority model

This is the recommended option. It keeps the first-slice work intact, extends the same token/resource/static-validation approach across shell regions, and adds full-shell screenshot evidence so reviewers can evaluate coherence instead of only isolated components.

## Recommended direction

Choose Option D: add a separate follow-on shell visual-coherence spec that extends the accepted UI design-system work across the whole shell.

This proposal should produce a separate follow-on shell visual-coherence spec rather than rewriting the active first-slice UI design-system spec. The existing UI design-system spec remains authoritative for first-slice token and file-list row work. The follow-on spec governs shell-wide surface foundation, navigation band, sidebar, status/operation surfaces, preview/details visual treatment, deterministic icon strategy, and full-shell visual evidence.

Use these downstream artifact paths:

```text
specs/ui-shell-visual-coherence.md
specs/ui-shell-visual-coherence.test.md
docs/plans/2026-05-11-ui-shell-visual-coherence.md
```

The project should keep the accepted authority model:

- VeloFile-owned specs, token contracts, scope contracts, WinUI resources, fixture evidence, and deviation records govern production UI.
- `hifi-design/` remains a reference and quality benchmark only.
- Existing V1 product behavior remains authoritative unless a later accepted spec intentionally changes it.

Shell-wide visual-coherence tokens should be added as an additive extension to `docs/ui/tokens.v1.json` and `docs/ui/ui-contract-scopes.v1.json`. A new token major version should be introduced only for incompatible token semantics, renamed resource contracts, or a deliberate redesign reset.

A shell slice is visually coherent when:

- App root, chrome, sidebar, content, command band, file list, and status surfaces share one tokenized surface model.
- The current location, active tab, selected files, active operation, and unsafe actions are visually prioritized in that order.
- No governed region uses raw/default control visuals without an accepted deviation.
- Focus and selection are distinct, accessible, and not confused with warning/error states.
- File/folder icons are deterministic and polished; fixture rows do not use truncated placeholder chips.
- Sidebar hierarchy presents navigation before secondary visibility/settings controls.
- Full-shell screenshots show no new mismatch between redesigned and non-redesigned regions.

The next design direction should start with the shell surface foundation. Define and apply a coherent dark comfortable surface family for app root, chrome, sidebar, content, elevated/flyout surfaces, borders, separators, text hierarchy, accent/focus, selection, hover, danger/warning/success states, row height, toolbar height, path bar height, sidebar width, control radius, and focus thickness.

After the foundation pass, file-list polish should remain the first product-critical region. It should cover normal, hover, selected, focused, selected plus focused, multi-selected, hidden, protected/system, long filename, thumbnail fallback, folder, and empty-folder states as a complete region, not just a row template patch.

The top command/path/search area should be redesigned as an intentional navigation band with shared control height, radius, padding, typography, hover/focus states, and disabled states. A two-row structure should be considered: navigation plus breadcrumb/path on one row, and filter/search/status actions on the second row.

The sidebar should become navigation-first: locations, favorites, recent entries, and drives should lead; visibility toggles and terminal controls should become secondary compact sections.

Sidebar reordering is an observable shell behavior. The follow-on spec must preserve access to existing visibility toggles, terminal selection, locations, favorites, recents, and drives, and must define keyboard order, accessible names, and discoverability for the new grouping before implementation.

The icon strategy should move away from truncated text chips. Governed shell and file-list icon surfaces must use deterministic VeloFile icon resources or verified platform icon adapters. Visual fixtures must not use ellipsized text chips such as `P...`, `D...`, or `T...` as file-type icons. Critical shell icon buttons must avoid glyph-font/private-use rendering paths unless an accepted deviation and visual proof exist.

First-slice deterministic fixture icons should be XAML vector resources, not text chips or glyph-font icon controls. Add a dedicated resource dictionary:

```text
src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml
```

Represent fixture icons as named vector geometry resources consumed by raw `Path` elements inside a fixed icon container. Do not use `SymbolIcon`, `PathIcon`, private-use glyph fonts, or ellipsized extension chips such as `P...`, `D...`, or `T...`.

Recommended first resource shape:

```text
VfIconGeometryFileGeneric
VfIconGeometryFolder
VfIconGeometryPdf
VfIconGeometryImage
VfIconGeometryText
VfIconGeometrySpreadsheet
VfIconGeometryExecutable
VfIconGeometryMarkdown
VfIconGeometryThumbnailFallback

VfFileListIconContainerStyle
VfFileListIconPathStyle
VfFileListFixtureIconTemplate
```

Fixture rows should expose an allowlisted icon kind instead of display text:

```text
FileGeneric
Folder
Pdf
Image
Text
Spreadsheet
Executable
Markdown
ThumbnailFallback
```

The app/testing layer should map that icon kind to the corresponding named geometry resource through a hardcoded fixture-mode allowlist. It should not accept arbitrary resource keys from fixture input. The visual rule is fixed-size container plus deterministic vector path plus tokenized foreground/background, not extension text rendered inside a chip.

Each region slice should also produce a full-shell screenshot so reviewers can answer whether the whole app looks more coherent, whether a new mismatch was introduced, and whether focus, selection, disabled states, and minimum-size behavior remain clear.

First review evidence should use effective-pixel app-window sizes. Microsoft guidance says XAML layout should be designed in effective pixels and that breakpoint design should use the space available to the app window rather than the physical screen size. It also recommends multiples of 4 epx for XAML sizes, margins, and positions because XAML scales across common plateaus. The follow-on spec should use these required profiles:

| Profile ID | Effective window size | Scale | Purpose |
|---|---:|---:|---|
| `shell-min-900x560-100` | `900 x 560` | `100%` | Product minimum layout gate |
| `shell-standard-1440x900-100` | `1440 x 900` | `100%` | Primary visual baseline |
| `shell-standard-1440x900-200` | `1440 x 900` | `200%` | High-DPI readability and scaling evidence |

If CI cannot reliably set `200%` scaling yet, `shell-standard-1440x900-200` should remain required as manual/release review evidence until automation is stable. The `shell-stress-720x500-100` profile is advisory only unless a later accepted spec lowers VeloFile's supported minimum window size.

## Expected behavior changes

- The visible shell moves from mixed raw-control surfaces toward one coherent dark comfortable product system.
- The full-shell screenshot reads as one app rather than a dark file list beside a light functional harness.
- File-list focus uses VeloFile focus treatment rather than a loud debug-feeling red/pink outline.
- Selection, hover, hidden, protected/system, and disabled visual states are distinct but calmer.
- Heavy row separators are reduced or replaced by a subtler list rhythm.
- Placeholder-like file-type chips are replaced by deterministic, polished icon treatment in visual fixtures and by a clearer production icon direction later.
- Toolbar, path, filter, search, and sidebar controls share sizing, radius, spacing, and state rules.
- Sidebar hierarchy prioritizes navigation before settings-like visibility controls.
- Sidebar restructuring is treated as observable shell behavior and preserves keyboard access, accessible names, and discoverability for existing locations, favorites, recents, drives, visibility toggles, and terminal controls.
- Governed icon surfaces move away from placeholder text chips and toward deterministic VeloFile icon resources or verified platform icon adapters.
- Deterministic fixture icon rows expose an allowlisted icon kind and render through XAML vector geometry resources in `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`.
- First review evidence uses `900 x 560`, `1440 x 900` at `100%`, and `1440 x 900` at `200%` effective-pixel profiles, with the `200%` profile allowed as manual/release evidence until automated scale control is stable.
- Full-shell visual evidence becomes part of review for region changes, while screenshot evidence remains soft until the capture and comparison path is stable.

## Architecture impact

Expected impact remains mostly in `src/VeloFile.App`, `docs/ui`, tests, and visual tooling:

- Extend `docs/ui/tokens.v1.json` additively for shell-wide surface, command-band, sidebar, icon, and status-surface tokens.
- Extend `docs/ui/ui-contract-scopes.v1.json` additively for governed shell regions.
- Continue using WinUI `ResourceDictionary` files under `src/VeloFile.App/Resources/Tokens/` and `src/VeloFile.App/Resources/Components/`.
- Add `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` for deterministic fixture vector geometries and file-list icon styles.
- Add component resources for shell surfaces, command/path/search band, sidebar, status surfaces, and icon treatment as those regions enter scope.
- Extend `tools/VeloFile.UiContracts` validation for new governed scopes and tokenized visual states.
- Add static checks for governed fixture and file-list icon resources so placeholder text chips and unapproved glyph-font/private-use icon paths do not recur.
- Extend deterministic fixture mode and screenshot capture to include whole-shell states, not only isolated file-list rows.
- Record intentional deviations from reference material in `docs/ui/design-deviations.md`.

Core services, Windows adapters, file operations, listing coordination, search, preview providers, persistence, diagnostics, and terminal launch boundaries should not move as part of this proposal.

## Testing and verification strategy

Use layered evidence:

- Static contract checks for token presence, scoped resource references, duplicate keys, comparable values, and forbidden local literals in governed regions.
- Existing behavior tests to prove navigation, listing, selection, search, preview, drag/drop, file operations, persistence, diagnostics, and terminal launch routes still work.
- Focused file-list visual fixtures for normal, selected, focused, selected-focused, multi-selection, hidden/protected, thumbnail fallback, long names, and empty-folder states.
- New full-shell screenshot evidence for default shell, active filter, active search, preview open, operation running, and destructive confirmation states as those surfaces enter scope.
- Accessibility checks for visible focus, keyboard routes, accessible names, contrast, disabled states, and non-color-only distinctions.
- Minimum-size and responsive layout checks for shell coherence.

The follow-on spec must include a behavior-preservation matrix for navigation, tabs/session restore, listing/virtualization, selection, filter/search, context menu, file operations, drag/drop, preview, terminal launch, diagnostics, and accessibility routes. Each shell region slice should identify which behaviors it touches and which tests prove they still work.

The first required full-shell screenshot set should cover:

```text
shell-default
shell-file-list-selected-focused
shell-filter-active
shell-search-active
shell-preview-open
shell-operation-running
shell-destructive-confirmation
```

Each required screenshot should be captured for these first review profiles when automation is available:

```text
shell-min-900x560-100
shell-standard-1440x900-100
shell-standard-1440x900-200
```

For `shell-min-900x560-100`, evidence should prove that primary navigation controls are not clipped, the file list remains usable, the sidebar does not obscure content, the path/search band remains reachable, selected/focused rows remain visible, and destructive or operation status surfaces do not cover primary navigation.

For `shell-standard-1440x900-100`, evidence should prove the primary full-shell visual baseline: coherent app/chrome/sidebar/content surfaces, readable file-list icons and row states, and balanced navigation band/sidebar hierarchy.

For `shell-standard-1440x900-200`, evidence should prove high-DPI readability and scale behavior: readable text, crisp icons, visible focus ring, no clipped controls from scale conversion, and intentional row rhythm and spacing. Windows DPI guidance identifies clipped UI, incorrect layout, pixelated icons, and hit-testing problems as high-DPI failure modes, so this profile is layout-safety evidence, not decorative polish.

Screenshot sidecars should include profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review ID. Example:

```json
{
  "profile": "shell-standard-1440x900-100",
  "effectiveWindowSize": "1440x900",
  "scale": 1.0,
  "theme": "dark",
  "density": "comfortable",
  "fixture": "file-list-v1",
  "evidenceKind": "soft-review",
  "dynamicRegions": [],
  "reviewId": "..."
}
```

The UI contract checker should reject governed fixture/file-list icon resources that use `<SymbolIcon`, `<PathIcon`, glyph-font private-use values, ellipsized extension chip text, unapproved local color literals, or unapproved local icon sizes. It should require `VfIconGeometry*`, `VfFileListIconContainerStyle`, and `VfFileListIconPathStyle` for the governed fixture icon path.

Screenshot evidence should remain review evidence at this stage. Pixel comparison can be introduced later after capture stability, ignored dynamic regions, metadata sidecars, and baseline update workflow are reliable.

## Rollout and rollback

Roll out through small reviewable region scopes that extend the accepted first-slice design-system work. The first two regions are binding for the follow-on plan so another isolated row patch does not precede the shell surface model:

1. Shell surface foundation: app/chrome/sidebar/content surfaces, borders, separators, text colors, control radius, and spacing rhythm.
2. File-list polish: calmer row rhythm, selected/focused states, hidden/protected styling, subtle separators, deterministic fixture icons, long names, and empty state.

The remaining region order should be finalized in the follow-on plan:

3. Navigation/path/search band: consistent buttons, inputs, breadcrumb/path hierarchy, filter/search state, and disabled treatment.
4. Sidebar redesign: navigation-first grouping, compact visibility controls, terminal section, and selected/hover states.
5. Status and operation surfaces: progress, cancellation, failure, conflict, and destructive confirmation states.
6. Preview/details pane: loading, success, unsupported, failed, metadata, and document/image/PDF states.

Rollback should be possible per region by reverting that region's resources, scoped XAML usage, tests, and visual baselines. No data migration is expected because persisted theme and density settings are not part of this proposal.

## Risks and mitigations

- Risk: the follow-on proposal duplicates or conflicts with the accepted first-slice proposal. Mitigation: treat this as a follow-on shell coherence proposal and keep existing accepted artifacts authoritative.
- Risk: visual polish breaks V1 file-manager behavior. Mitigation: preserve command and service routes and run existing behavior tests after region changes.
- Risk: full-shell screenshots become subjective or over-trusted. Mitigation: pair screenshots with static contracts, behavior tests, accessibility checks, and explicit reviewer notes.
- Risk: screenshot comparison becomes noisy. Mitigation: keep screenshots soft evidence until capture stability and ignored dynamic regions are proven.
- Risk: local XAML literals keep reappearing. Mitigation: extend scoped UI contract validation as each region enters redesign.
- Risk: icon fixes repeat the glyph regression pattern. Mitigation: use deterministic vectors for fixtures, static scans for icon resource usage, and visual proof for affected surfaces.
- Risk: sidebar restructuring changes user workflows. Mitigation: specify observable sidebar behavior before implementation and preserve existing routes unless a later accepted spec changes them.
- Risk: scope expands into a settings/theme project. Mitigation: keep dark comfortable defaults fixed and defer persistence or user-selectable density/theme to a separate spec.

## Open questions

None blocking proposal review.

The follow-on spec should resolve exact vector geometry values, exact fixture row-to-icon examples, and whether the `200%` profile is automated or recorded as manual/release evidence in the first implementation slice.

## Decision log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-11 | Create a follow-on shell visual-coherence proposal instead of replacing the accepted first-slice proposal. | Existing proposal, spec, architecture, and plan artifacts are already active; this work should extend rather than rewrite that history. | Downgrading the accepted proposal to draft; creating a competing duplicate with the same scope. |
| 2026-05-11 | Use full-shell screenshot evidence in addition to focused file-list fixtures. | Whole-shell mismatches can survive isolated component checks. | File-list-only evidence; static validation only. |
| 2026-05-11 | Start the follow-on direction with shell surface foundation before another isolated row patch. | The screenshot's most visible issue is inconsistent surface and hierarchy, not only row styling. | Opportunistic row-only fixes. |
| 2026-05-11 | Keep the file list as the first product-critical region after foundation. | The file list is the daily-use core and exposes the most important visual states. | Sidebar-first or titlebar-first polish. |
| 2026-05-11 | Replace placeholder-like file-type chips with deterministic icon treatment for visual fixtures. | Truncated chips make the UI look unfinished and undermine screenshot evidence. | Real Shell icons as the first screenshot dependency; ellipsized extension chips. |
| 2026-05-11 | Produce separate follow-on artifacts at `specs/ui-shell-visual-coherence.md`, `specs/ui-shell-visual-coherence.test.md`, and `docs/plans/2026-05-11-ui-shell-visual-coherence.md`. | The active first-slice artifacts should remain stable and authoritative for their scope. | Amending the active first-slice spec for shell-wide work. |
| 2026-05-11 | Extend `docs/ui/tokens.v1.json` and `docs/ui/ui-contract-scopes.v1.json` additively for shell-wide scopes. | Additive V1 contracts avoid premature version sprawl while preserving a major-version escape hatch for incompatible changes. | Creating a new token major version immediately; embedding shell tokens outside the UI contract files. |
| 2026-05-11 | Treat sidebar reordering as observable shell behavior. | Navigation grouping affects keyboard order, accessibility, discoverability, and user workflow. | Treating sidebar hierarchy as cosmetic styling only. |
| 2026-05-11 | Require a visual-coherence review rubric and behavior-preservation matrix in the follow-on spec. | Screenshots need a stable review language and cannot prove behavior alone. | Subjective screenshot review; screenshot-only acceptance. |
| 2026-05-11 | Use XAML vector geometry resources in `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml` for deterministic fixture icons. | Raw vector resources avoid the prior glyph-font defect class and make fixture screenshots deterministic. | Text chips; `SymbolIcon`; `PathIcon`; private-use glyph fonts; real Shell icons for first fixture baselines. |
| 2026-05-11 | Require `shell-min-900x560-100`, `shell-standard-1440x900-100`, and `shell-standard-1440x900-200` as first review profiles. | Effective-pixel app-window profiles align with Windows XAML layout guidance and expose minimum-size and high-DPI layout risk. | Physical-pixel contracts; required `720 x 500` pass/fail gate before lowering the product minimum; standard-only screenshots. |

## Next artifacts

- `proposal-review` for this follow-on proposal.
- Separate follow-on UI shell visual-coherence spec at `specs/ui-shell-visual-coherence.md`.
- Matching test spec at `specs/ui-shell-visual-coherence.test.md` for shell-wide tokens, scoped validation, screenshot evidence, accessibility, icon invariants, and behavior preservation.
- Architecture or ADR update if token contract versioning, fixture launch, icon resources, screenshot evidence, or UI validation boundaries materially change.
- Execution plan at `docs/plans/2026-05-11-ui-shell-visual-coherence.md` after spec and architecture are accepted.

## References

- [Microsoft Learn: Screen sizes and breakpoints](https://learn.microsoft.com/en-us/windows/apps/design/layout/screen-sizes-and-breakpoints-for-responsive-design)
- [Microsoft Learn: DPI and device-independent pixels](https://learn.microsoft.com/en-us/windows/win32/learnwin32/dpi-and-device-independent-pixels)

## Follow-on artifacts

- Spec drafted at [specs/ui-shell-visual-coherence.md](../../specs/ui-shell-visual-coherence.md).

## Readiness

Approved by `proposal-review` and ready for `spec-review` after the follow-on spec is drafted. The direction is to extend the accepted VeloFile-owned UI design-system work into whole-shell visual coherence, using `hifi-design/` only as reference input, preserving V1 behavior, and adding full-shell visual evidence alongside static and behavior checks.
