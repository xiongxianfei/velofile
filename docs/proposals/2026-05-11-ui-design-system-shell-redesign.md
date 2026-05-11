# VeloFile UI Design System and Shell Redesign Proposal

## Status

accepted

## Problem

The current WinUI shell implements the V1 file-manager behavior, but the visible product UI does not yet have a repo-owned product design system. `hifi-design/` shows a higher-quality reference direction, but it is not the production source of truth. The gap is not only visual polish: the repository needs its own UI principles, tokens, component contracts, density behavior, keyboard and tooltip expectations, layout gates, visual regression approach, and implementation traps recorded as durable engineering contracts.

Directly porting or mechanically matching the prototype in XAML would risk breaking already-implemented file-manager behavior, hiding design decisions inside one large UI diff, and allowing an external reference package to govern production UI decisions that should be owned by the VeloFile repository.

## Goals

- Establish the best-practice path for defining VeloFile's own WinUI-native product UI and design system without rewriting product behavior.
- Treat `hifi-design/` as benchmark reference material that can inform quality, not as a source of truth.
- Preserve V1 scope, Windows-native behavior, responsiveness, accessibility, and testability.
- Create a reviewable bridge from repo-owned UI principles, design tokens, and component specs into WinUI resources, controls, and verification.
- Avoid one large visual rewrite that mixes layout, behavior, token, accessibility, and performance changes.

## Non-goals

- This proposal does not implement the redesigned UI.
- This proposal does not adopt the prototype JSX as production code.
- This proposal does not require visual or token conformance to `hifi-design/`.
- This proposal does not introduce a broad theme engine, customizable toolbar, color labels, plugin UI, dual-pane browsing, or other V1 non-goals.
- This proposal does not replace WinUI 3, C#, or the existing Core/App/Windows architecture.
- This proposal does not treat design-time tweak controls as production settings.

## Vision fit

fits the current vision

The proposal supports the vision by making VeloFile feel fast, clear, Windows-native, and focused. It keeps the product inside the approved Windows desktop file-manager scope and treats visual fidelity as a way to improve daily workflows, not as a reason to expand V1 feature scope.

## Initial intent preservation

| Initial user goal | Proposal treatment | Where recorded |
|---|---|---|
| The current UI is far away from the hi-fi design. | in scope | Problem, Goals, Recommended direction |
| Identify best practice. | in scope | Options considered, Recommended direction, Testing and verification strategy |
| Use the proposal workflow. | in scope | Status, Next artifacts, Readiness |
| Clarify that `hifi-design/` is only a reference. | in scope | Context, Recommended direction, Architecture impact, Testing and verification strategy |

## Context

`hifi-design/` is a reference design package. It provides useful examples of visual quality, density, component anatomy, interaction polish, layout behavior, and implementation traps, but it is not the production UI source of truth.

The reference package contains `VeloFile.html`, component files, design tokens, a component spec sheet, and an engineering guide. These artifacts can help compare quality and reveal overlooked UI concerns, but they do not own production tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria.

The current app is a WinUI 3 desktop application in `src/VeloFile.App`. `MainWindow.xaml` already exposes core surfaces: tabs, sidebar, breadcrumb/path entry, filter/search, file list, context menu, operation states, preview pane, and status text. Those surfaces are behavior-first and use built-in theme resources and local dimensions rather than a repo-owned VeloFile design-token resource system.

The V1 product spec already excludes a full theme engine beyond light/dark and excludes broad UI customization. The constitution requires externally observable UI behavior to be specified before implementation, and behavior changes should use focused tests and preserve architecture boundaries.

## Options considered

### Option A: Direct visual port from the prototype into `MainWindow.xaml`

This is fastest to start, but it is the riskiest. It would likely create a large XAML diff, mix styling with behavior changes, hard-code design values, and make regressions difficult to isolate. It would also incorrectly treat the reference package as authoritative.

### Option B: Repo-owned VeloFile design system, then phased shell redesign

Define VeloFile's own UI principles, tokens, WinUI resources, and shell component contracts, using `hifi-design/` as one benchmark input. Then redesign visible surfaces in phases while keeping existing command, navigation, listing, preview, and file-operation behavior intact. This creates more upfront artifact work, but it keeps product ownership inside the repo and gives tests a stable target.

### Option C: Build a parallel redesigned shell and swap later

This protects the current shell during exploration, but it doubles UI maintenance and risks late integration failure. It also makes it easy for behavior to drift between old and new surfaces.

### Option D: Keep current UI and only fix obvious visual issues opportunistically

This minimizes immediate cost, but it allows design drift to continue and gives reviewers no objective way to decide whether changes meet the repo-owned UI standard.

### Option E: Adopt `hifi-design/` as the authoritative UI contract

This gives a clear target, but it is the wrong authority model. The reference package was not created as the repo-owned production contract and may contain web-specific, exploratory, or design-time choices that should not govern WinUI production behavior.

## Recommended direction

Choose Option B: repo-owned VeloFile design system, then phased shell redesign.

The best practice is not to port or mechanically align to the prototype. The project should define its own WinUI-native UI standard, informed by the reference design but governed by repo-owned specs, tokens, component contracts, accessibility rules, layout behavior, and visual evidence.

Design authority rule: `hifi-design/` is reference input only. It is not authoritative for production tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria. The production source of truth is the repo-owned UI redesign spec, WinUI design resources, and accepted test evidence. The final VeloFile UI may intentionally differ from the reference when the repo-owned design is clearer, more accessible, more Windows-native, more performant, more maintainable, or better aligned with V1 behavior.

Implementation should proceed in small slices: first define the VeloFile UI principles and token/resource system, then redesign shell regions one at a time while preserving existing command, navigation, listing, preview, drag/drop, file-operation, persistence, and diagnostics behavior.

The production app should keep existing Core command surfaces and WinUI event routes. The redesign should change visual structure, resource usage, layout, focus affordances, density, and component polish, but should not reimplement file operations, listing, search, session restore, drag/drop, preview, or diagnostics.

## Settled design policy

### Reference treatment

`hifi-design/` should be used as benchmark/reference input, not as the contract. The goal is to match or exceed the reference's quality, not copy its implementation.

Useful reference patterns:

| Reference area | How to use it |
|---|---|
| Overall visual quality bar | Use as a comparison target for whether VeloFile feels as polished or better. |
| Density concept | Keep the compact, comfortable, and spacious concept, but define WinUI-native values in the repo. |
| Component anatomy | Use as inspiration for shell regions, sidebar, toolbar, breadcrumb, list rows, preview, modals, and status surfaces. |
| Keyboard and tooltip expectations | Elevate the principle that interactive controls need keyboard paths and clear tooltips. |
| Layout gates | Keep the idea of testing from narrow desktop sizes to large screens. |
| Implementation traps | Preserve lessons around thumbnails, progress UI, shell-extension risk, tweak-panel overreach, and icon consistency. |
| Visual regression ambition | Keep as a desired evidence layer, but do not overclaim it before automation is stable. |

Intentionally redesign:

| Reference detail | Reason |
|---|---|
| Web/JSX component structure | Production is WinUI 3/C#, so JSX anatomy should not dictate implementation. |
| CSS token names and raw values | VeloFile should own WinUI resource names and values. |
| Web-only effects, shadows, and layout tricks | Some CSS effects do not map cleanly to WinUI or Windows-native expectations. |
| Prototype tweak controls | They are design-time exploration, not V1 production settings. |
| PDF/image/provider choices in the handoff | Product architecture and V1 specs should govern implementation. |
| Exact animation curves | Advisory unless explicitly accepted in the UI spec. |
| Any behavior that conflicts with V1 | Existing V1 product behavior wins unless changed by accepted spec or ADR. |

### UI principles

Density is global, predictable, and workflow-oriented. Compact favors large folders and keyboard-heavy use. Comfortable is the default. Spacious improves readability without changing product behavior. Density should affect row height, control spacing, sidebar rhythm, toolbar height, and preview/detail spacing consistently. It should not become per-component tweaking.

Hierarchy should make the current location, selected files, active tab, active operation, and unsafe actions obvious. Recommended priority is current tab and path first, file list/current task second, selection and operation state third, search/filter/preview context fourth, and secondary metadata/status last. Destructive actions should never visually compete with ordinary navigation.

Navigation should always make clear where the user is, which tab is active, whether they are viewing a folder, filter result, or recursive search result, whether a path entry failed without replacing the valid location, and how to return to normal browsing.

The file list is the core product surface. Prioritize stable row rhythm, clear selection, readable names and extensions, hidden/system distinctions, non-blocking thumbnails, sort/filter/search state clarity, and no layout jump during preview or status updates.

Preview should be helpful but subordinate to browsing. It should not reflow the file list unnecessarily, should show loading/failure/unsupported states clearly, should keep navigation responsive, should preserve PDF page navigation context, should expose metadata fallback cleanly, and should never imply unsupported formats are broken files.

File operations should make state and risk explicit. Recycle Bin delete is the default, permanent delete requires clear confirmation, "Permanently delete" should not receive default keyboard focus, progress and cancellation should be visible without modal lock-in, conflict resolution should be understandable and reversible where possible, and failed/cancelled operations should preserve visible rows and context.

### Token categories

VeloFile should define repo-owned WinUI tokens in at least these categories:

| Category | Examples |
|---|---|
| Color | app background, pane background, elevated surface, border, primary/secondary/muted text, accent, warning, danger, success, selection, focus |
| Typography | font family, title size, body size, metadata size, monospace size, weights |
| Spacing | shell padding, row padding, control gaps, pane gutters, modal padding |
| Sizing | titlebar height, toolbar height, breadcrumb height, statusbar height, row heights per density, sidebar width, preview width |
| Radius | small control radius, card/pane radius, modal radius |
| Border | subtle border, strong border, focus border, separator thickness |
| Focus | focus ring thickness, inset/outset behavior, high-contrast fallback |
| State | hover, pressed, selected, disabled, loading, failed, unsupported, hidden, protected, destructive |
| Iconography | icon sizes, stroke style, monochrome/current-color rule |
| Elevation | flyout shadow, modal shadow, pane elevation, or WinUI-equivalent treatment |
| Motion | duration, easing, reduced-motion behavior |
| Density | compact, comfortable, and spacious row and spacing values |
| Layout breakpoints | minimum shell size, preview collapse/resize rules, sidebar behavior |
| Z-order | tooltip, flyout, modal, context menu, drag/drop indicator |

Token drift checks should compare accepted VeloFile UI spec to WinUI resources, not `hifi-design` tokens to WinUI resources.

Token conformance checks should be introduced in this order:

1. Spec-to-resource table validation.
2. Static XAML key presence.
3. Rendered resource smoke tests.

The first risk is not whether a XAML key exists; it is whether the repo has a clear contract for what tokens are supposed to exist. The first check should validate a source-of-truth table such as `docs/ui/tokens.v1.json` or `docs/ui/tokens.v1.md` against the WinUI resource dictionaries.

Use `docs/ui/tokens.v1.json` as the first-slice token contract. Markdown may be added later for human explanation, examples, and screenshots, but JSON governs automated validation.

Recommended contract shape:

```json
{
  "version": 1,
  "theme": "dark",
  "density": "comfortable",
  "tokens": [
    {
      "id": "VfColor.Surface.Content",
      "xamlKeys": ["VfColorSurfaceContent", "VfBrushSurfaceContent"],
      "type": "ColorAndBrush",
      "value": "#202329",
      "category": "color",
      "requiredInFirstSlice": true
    },
    {
      "id": "VfFileList.RowHeight",
      "xamlKeys": ["VfFileListRowHeight"],
      "type": "Double",
      "value": 30,
      "category": "density",
      "requiredInFirstSlice": true
    }
  ]
}
```

This proves every accepted VeloFile token has a corresponding XAML resource, names match the VeloFile-owned naming convention, token categories are correct, values are present, and intentional deviations are recorded.

Example mappings:

```text
VfColor.Surface.Content -> VfColorSurfaceContent + VfBrushSurfaceContent
VfFileList.RowHeight -> VfFileListRowHeight
VfFocus.Thickness -> VfFocusThickness
```

Static XAML key checks should come after the token contract exists. They should inspect dictionaries for missing keys, misspelled names, duplicate keys, inline resources left in the redesigned region, and forbidden direct literals in the first redesigned region.

Rendered resource smoke tests should come third because they require a running app or XAML host and can be noisier across theme, DPI, font rendering, and CI environment. Use them for a few high-value checks: file-list row height, selected row background, visible focus ring, resolved text brushes, and absence of missing-resource fallbacks.

The first token validator should be a small purpose-built checker that uses XML DOM/XDocument parsing internally. Do not rely on MSBuild item metadata for first-slice resource validation. MSBuild can help discover included files later, but it is not the right source for validating actual XAML resource keys and values.

Preferred tool location:

```text
tools/VeloFile.UiContracts/
```

Add `tools/VeloFile.UiContracts` to `VeloFile.sln` in the first implementation slice. It should be a lightweight .NET console app with no app runtime dependency and no WinUI dependency unless absolutely necessary. It should parse token JSON and XAML files as static artifacts.

Recommended shape:

```text
tools/
  VeloFile.UiContracts/
    VeloFile.UiContracts.csproj
    Program.cs
```

Example validation command:

```powershell
dotnet run --project tools/VeloFile.UiContracts -- validate-tokens `
  --contract docs/ui/tokens.v1.json `
  --xaml-root src/VeloFile.App/Resources
```

The validator should check that every token has all required XAML keys, XAML resource types match expected token types, directly comparable values match, color brushes point to the expected color token, duplicate token keys are absent, first-slice redesigned files do not introduce unapproved local literals for tokenized values, and missing or extra first-slice resources are reported clearly.

Adding the tool to the solution early means CI restore/build catches broken tool code, dependency versions stay visible, contributors discover the tool through normal repo structure, and later checks can expand cleanly to screenshots, layout manifests, or design-deviation validation.

Tokenized-literal enforcement should use different strictness levels.

For new resource dictionaries under `src/VeloFile.App/Resources/Tokens/` and `src/VeloFile.App/Resources/Components/`, enforce strict validation:

- all token keys in `docs/ui/tokens.v1.json` exist;
- no duplicate resource keys;
- resource values match token contract where declared;
- brushes reference token colors where appropriate;
- no unexplained hardcoded first-slice color values outside token definitions;
- no ad hoc row-height, spacing, or radius values in component resources when a token exists.

For `MainWindow.xaml`, use targeted first-slice rules only. Do not ban all literals in the existing XAML file. For the first file-list slice, check that `MainWindow.xaml` uses named resources for file-list `ItemTemplate`, `ItemContainerStyle`, row height, row padding, row text styles, selection/focus brushes, hidden/protected state styling, and thumbnail fallback size/icon resources.

The checker should flag newly introduced direct literals in the file-list region for color literals, inline `SolidColorBrush` values, row `FontSize`, `Height`/`MinHeight`, `Padding`/`Margin`, `CornerRadius`, and focus/selection `BorderThickness`. It should not fail on old unrelated literals elsewhere until those regions enter redesign scope.

Prefer factoring file-list resources into `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml` so `MainWindow.xaml` only references named resources such as:

```xml
ItemTemplate="{StaticResource VfFileListRowTemplate}"
ItemContainerStyle="{StaticResource VfFileListItemContainerStyle}"
```

If region scanning remains necessary, use explicit scope configuration rather than whole-file bans:

```json
{
  "scopes": [
    {
      "name": "file-list-first-slice",
      "files": ["src/VeloFile.App/MainWindow.xaml"],
      "requiredResourceReferences": [
        "VfFileListRowTemplate",
        "VfFileListItemContainerStyle"
      ],
      "forbiddenLiteralPatterns": [
        "inlineColor",
        "rowHeight",
        "rowPadding",
        "cornerRadius"
      ]
    }
  ]
}
```

Use `docs/ui/ui-contract-scopes.v1.json` as the first-slice UI contract scope file. It is explicit, versioned, and can grow region by region without being tied only to file-list work.

Contract artifact relationship:

```text
docs/ui/tokens.v1.json
docs/ui/ui-contract-scopes.v1.json
docs/ui/design-deviations.md
```

`tokens.v1.json` defines accepted design tokens. `ui-contract-scopes.v1.json` defines where tokens and component resources are expected to be used. `design-deviations.md` records intentional differences from reference material.

Recommended first shape:

```json
{
  "version": 1,
  "scopes": [
    {
      "id": "file-list-first-slice",
      "status": "active",
      "files": [
        "src/VeloFile.App/MainWindow.xaml",
        "src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml"
      ],
      "requiredResourceReferences": [
        "VfFileListRowTemplate",
        "VfFileListItemContainerStyle",
        "VfFileListRowHeight",
        "VfFileListRowNameTextStyle",
        "VfFileListRowMetadataTextStyle"
      ],
      "forbiddenLiteralRules": [
        "inline-color",
        "inline-row-height",
        "inline-row-padding",
        "inline-selection-brush",
        "inline-focus-thickness"
      ]
    }
  ]
}
```

Keep this file small at first. It should describe enforcement scopes, not become a full design spec.

First-slice enforcement policy:

```text
strict: docs/ui/tokens.v1.json -> resource dictionaries
strict: new file-list component resources
targeted: MainWindow.xaml first-slice file-list references
deferred: global ban on existing literals
```

First slice conformance policy:

```text
required: spec-to-resource validation
required: static XAML key presence
optional but recommended: small rendered smoke test for file-list row resources
```

### First-slice baseline tokens

The first redesign slice should use a small VeloFile-owned token set. It should prove the design-system path without creating a theme engine. The baseline is dark, comfortable, and WinUI-native. These values are first-slice defaults, not permanent brand law.

Use checked-in WinUI `ResourceDictionary` files, merged from `App.xaml`, with token values represented as typed XAML resources. Do not create a generated token pipeline in the first slice. Start with reviewed checked-in dictionaries and add drift/conformance checks against the VeloFile UI spec.

Recommended first-slice structure:

```text
src/VeloFile.App/Resources/
  Tokens/
    VeloFile.Colors.xaml
    VeloFile.Typography.xaml
    VeloFile.Spacing.xaml
    VeloFile.Sizing.xaml
    VeloFile.Radius.xaml
    VeloFile.Focus.xaml
    VeloFile.Density.xaml
  Components/
    VeloFile.FileList.xaml
```

Recommended XAML representation:

| Token type | XAML representation |
|---|---|
| colors | `Color` plus derived `SolidColorBrush` |
| typography sizes | `x:Double` |
| font families | `FontFamily` |
| spacing/sizing | `x:Double`, `Thickness` where appropriate |
| radius | `CornerRadius` |
| focus | brush plus thickness/double tokens |
| density | row-height/padding tokens as `x:Double` / `Thickness` |
| component styles | named `Style`, `DataTemplate`, `ItemContainerStyle` |

Use semantic VeloFile names, not reference-design names. Examples: `VfColorSurfaceContent`, `VfBrushSurfaceContent`, `VfTextPrimaryBrush`, `VfFileListRowHeight`, `VfFileListItemContainerStyle`, and `VfFileListRowTemplate`.

Use mostly `{StaticResource}` in the first redesign slice because theme and density switching are not runtime features yet. Use `{ThemeResource}` only for resources that must respond to system theme or high-contrast behavior.

Color tokens:

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

Typography tokens should use platform-safe defaults first and should not require bundled fonts in the first slice. In WinUI XAML, numeric text-size values should be doubles, not CSS `px` strings.

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

Spacing, size, and radius tokens:

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

Density tokens should use comfortable only in the first implementation slice. Compact and spacious can be defined later, but should not be exposed or persisted until the settings contract is accepted.

| Token | Value |
|---|---:|
| `VfDensity.Current` | `comfortable` |
| `VfDensity.RowHeight` | `30` |
| `VfDensity.RowPaddingX` | `12` |
| `VfDensity.RowPaddingY` | `6` |

Focus and state tokens:

| Token | Value |
|---|---:|
| `VfFocus.Thickness` | `2` |
| `VfFocus.Inset` | `1` |
| `VfFocus.Color` | `VfColor.Accent.Line` |
| `VfState.HiddenOpacity` | `0.68` |
| `VfState.DisabledOpacity` | `0.46` |
| `VfMotion.FastMs` | `120` |
| `VfMotion.BaseMs` | `160` |

For the first slice, do not animate selection, filtering, listing, or file-operation state changes. Keep motion mostly for hover/focus affordance.

### Requirement elevation policy

Elevate reference ideas into VeloFile-owned V1 requirements only when they express user-visible quality, accessibility, safety, or workflow clarity.

Good candidates for V1 requirements: global density variants if accepted, consistent keyboard paths for interactive controls, tooltip/help text for non-obvious controls, visible focus states, clear file-list selection and row readability, non-modal operation progress/status, destructive confirmation hierarchy, layout gates for supported window sizes, preview-pane state clarity, visually distinct loading/failed/unsupported/hidden/protected/skipped states, icon-style consistency, and excluding design-time tweak panels from production settings.

Keep advisory: exact CSS token values, JSX structure, CSS implementation patterns, exact shadows/blur/effects, animation curves, prototype-specific spacing where WinUI needs different rhythm, specific preview technology recommendations, exact icon glyphs unless accepted separately, and screenshot pixel parity with the prototype.

### Theme and density persistence

The first redesign slice should use fixed defaults and may follow system light/dark only if already supported. It should not add new persistent theme or density preferences.

Persisted theme and density settings should come in a later specified slice with persistence tests, migration/default behavior, and release notes as needed. Persistence turns visual redesign into durable settings behavior, so it should not be mixed into the first shell redesign slice unless the accepted spec already covers it.

### Deviation policy

Create `docs/ui/design-deviations.md` during spec authoring, not during implementation. The first spec will already decide that `hifi-design/` is reference material, not the source of truth, so reviewers need a standard place to decide whether differences are improvements, compromises, or accidental drift.

Design deviations from the reference should be recorded when they are meaningful. A deviation is acceptable when it improves or preserves clarity, accessibility, Windows-native feel, performance, maintainability, V1 behavior correctness, or reviewability, and the reason is recorded. A deviation is not acceptable merely because XAML made it easier.

Each deviation record should include:

- Reference pattern: where the reference pattern appears.
- VeloFile decision: what production does instead.
- Reason: accessibility, Windows-native behavior, performance, maintainability, V1 behavior preservation, or no direct WinUI equivalent.
- User impact: what changes for users.
- Verification: tests, screenshots, checklist, or manual review.
- Status: proposed, accepted, temporary, or rejected.

Initial `docs/ui/design-deviations.md` contents should seed the purpose and status values:

```markdown
# UI Design Deviations

This document records intentional deviations from reference design material when VeloFile chooses a different production UI because it is clearer, more accessible, more Windows-native, more performant, easier to maintain, or better aligned with V1 behavior.

## Status values

- proposed
- accepted
- temporary
- rejected
```

## Expected behavior changes

- The app chrome, sidebar, toolbar, breadcrumb/path bar, file list, preview pane, modals, and status surfaces adopt a repo-owned VeloFile visual system whose quality is comparable to or better than the `hifi-design` reference.
- The shell uses repo-owned VeloFile design resources instead of scattered local dimensions and generic theme brushes.
- Light/dark appearance and density become explicit app-level presentation state within the V1 scope.
- Interactive controls consistently expose keyboard access, names, and tooltips.
- Layout behavior becomes testable across narrow, standard, and large desktop sizes.
- Existing file-manager behavior remains functionally compatible unless a later spec intentionally changes it.

## Architecture impact

Expected changes are mostly in `src/VeloFile.App`:

- Add or generate WinUI resource dictionaries from repo-owned VeloFile design tokens.
- Introduce a small presentation-state boundary for theme and density that does not leak into Core.
- Refactor `MainWindow.xaml` into clearer shell resources and component sections only where it reduces risk and improves reviewability.
- Keep command routing, listing coordination, file operations, preview services, drag/drop, persistence, and Windows adapters in their current layers.

VeloFile should define its own WinUI-native design tokens. The `hifi-design` tokens may be used as reference input, but production tokens must be selected, named, reviewed, and owned by the repository. Token comparison may be used as reference analysis, but token drift checks should verify consistency between the accepted VeloFile UI spec and the production WinUI resources, not conformance to the reference package.

## Testing and verification strategy

Use layered verification rather than relying on manual screenshots only:

- Design-system conformance checks: verify production WinUI resources match the accepted VeloFile UI token and component contract.
- Reference comparison review: optionally compare key screens against `hifi-design/` to explain intentional improvements, deviations, and quality gaps.
- Shell-region review: app frame/titlebar, sidebar, tabs, breadcrumb/path bar, toolbar/filter/search, file list rows, preview/details pane, status/progress, modals/dialogs, and context surfaces.
- Layout evidence: minimum supported size, standard laptop size, large desktop, 150% and 200% scaling, preview open/closed, and sidebar normal/collapsed if supported.
- State evidence: empty, loading, selected, hover/focus, failed, unsupported, hidden/protected files, search cap/skipped locations, destructive confirmation, and operation running/cancelled/failed.
- Static app-shell contract tests: continue proving named shell regions, command routes, accessibility names, keyboard routes, and tooltip presence where feasible.
- Layout checks: add focused checks for minimum supported window size, preview/sidebar breakpoints, and no hidden command surfaces.
- Visual regression: add screenshot or rendered-state comparison once a stable WinUI UI automation path exists.
- Accessibility checks: verify names, roles, focus visibility, contrast-sensitive states, and visually distinct loading, empty, failed, unsupported, hidden, and protected states.
- Existing behavior tests: keep the current app, core, and Windows tests as regression coverage for behavior that must not change during shell redesign.

The preferred first UI automation path is Microsoft UI Automation through FlaUI or direct UIA. Microsoft UI Automation exposes desktop UI elements to automated clients and supports automated test interaction. FlaUI is a .NET wrapper around native Microsoft UI Automation libraries and is a pragmatic fit for C# app-shell tests on Windows CI.

Use UIA/FlaUI for shell element existence, automation names, control patterns, button/menu invocation, keyboard traversal, focus movement, visible state checks, and layout bounding boxes.

Use screenshot capture as a secondary evidence layer for visual regression smoke tests, reference comparison snapshots, layout breakage, clipped controls, and density/theme snapshots. Keep screenshot tests limited at first because WinUI rendering, DPI, fonts, and CI GPU differences can be noisy.

Appium/WinAppDriver may be reconsidered later if the project wants WebDriver-style desktop automation. It should not be the first choice because it adds a server dependency and operational setup.

Recommended validation stack:

| Layer | Coverage |
|---|---|
| Fast CI | static XAML/resource checks, token conformance, view-model/component tests |
| Windows UI CI | FlaUI/UIA shell smoke tests, keyboard/focus traversal, layout bounds checks, selected screenshot captures |
| Manual/release evidence | mixed-DPI visual checklist, reference comparison review, accessibility pass |

A sufficient "meets or exceeds reference" claim means VeloFile passes its own UI spec and a reviewer can see why it is at least as clear, accessible, and workflow-effective as the reference.

Screenshot policy should start as review evidence, not a hard release gate. In the initial phase, store baseline screenshots, generate comparison diffs, do not fail CI on pixel difference, and fail CI only on missing screenshots, wrong dimensions, broken UIA/layout checks, or obvious capture failure.

After the automation path stabilizes, hard-gate thresholds may be introduced:

| Check | Initial threshold |
|---|---:|
| Pixel mismatch ratio | `<= 0.5%` |
| Perceptual diff threshold | `<= 0.03` if using a perceptual metric |
| Ignored dynamic regions | cursor, timestamps, progress animations, live counts, caret, file timestamps, thumbnail images unless fixture-stable |
| Required dimensions | exact viewport/DPI profile match |
| Baseline update | explicit review-only command |

Screenshot profiles should start with a small stable matrix:

```text
100% scale:
- 1280x800 comfortable dark
- 1440x900 comfortable dark
- 1920x1080 comfortable dark

200% scale:
- 1280x800 effective viewport if CI supports it, or manual evidence
```

Mixed-DPI evidence should remain manual/release evidence until CI is stable.

Store committed baselines and sidecars under:

```text
tests/visual/baselines/winui/<profile>/<screen>.png
tests/visual/baselines/winui/<profile>/<screen>.json
```

Do not commit generated current captures or diffs:

```text
tests/visual/current/
tests/visual/diffs/
```

The JSON sidecar should record app version, OS build, Windows App SDK version, scale, viewport, theme, density, screen name, and dynamic regions. Use Git LFS only if screenshot volume grows beyond a small first-slice baseline set.

Baseline updates must be explicit maintainer actions, not a side effect of normal tests.

Recommended command for one approved profile:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/update-ui-baselines.ps1 `
  -Suite winui `
  -Profile dark-comfortable-1440x900-100 `
  -ReviewId <issue-or-pr-id>
```

Recommended command for all approved profiles:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/update-ui-baselines.ps1 `
  -Suite winui `
  -AllReviewed `
  -ReviewId <issue-or-pr-id>
```

The baseline update script should copy from `tests/visual/current/` to `tests/visual/baselines/`, require an explicit `-ReviewId`, refuse to run if no current screenshots exist, write or update JSON sidecars with viewport/scale/theme/density/OS/app metadata and timestamp, and never run during normal CI. CI should generate current screenshots and diffs; humans approve; `update-ui-baselines.ps1` records the approved baseline.

Screenshot tooling should use PowerShell for orchestration and baseline updates. Add a .NET visual-diff helper only when comparison becomes more than file copying.

Tooling split:

| Responsibility | Tool |
|---|---|
| launch app | PowerShell |
| choose profiles | PowerShell |
| call UI test/screenshot capture | PowerShell |
| copy approved baselines | PowerShell |
| write sidecar metadata | PowerShell or .NET |
| pixel/perceptual comparison | .NET helper |
| diff image generation | .NET helper |

Phase 1 should implement `scripts/update-ui-baselines.ps1` as PowerShell-only baseline approval. Phase 2 may add a .NET helper at `tools/VeloFile.VisualDiff/` for deterministic PNG reading/writing, pixel mismatch percentage, ignored regions, diff image generation, perceptual comparison, and clearer error output.

Expected later helper shape:

```powershell
dotnet run --project tools/VeloFile.VisualDiff -- compare `
  --baseline tests/visual/baselines/winui/.../file-list.png `
  --current tests/visual/current/.../file-list.png `
  --diff tests/visual/diffs/.../file-list.diff.png `
  --metadata tests/visual/current/.../file-list.json
```

Add generated screenshot output folders to `.gitignore`:

```text
tests/visual/current/
tests/visual/diffs/
```

## Rollout and rollback

Roll out in small reviewable slices:

- Foundation: add token resources, shell layout constants, focus resources, and basic app background/chrome resources without changing major behavior.
- First region: redesign file list rows and selection/focus states.
- Then redesign one shell region at a time: sidebar/navigation, breadcrumb/path/filter/search area, operation/status surfaces, preview/details pane, dialogs/modals/context surfaces, and app frame/titlebar polish.
- Keep each slice reversible by avoiding mixed behavioral rewrites.
- If a visual slice causes regressions, rollback should remove that slice's resource and XAML changes without touching Core or Windows adapters.

No data migration is expected unless density or theme preferences become persisted settings in a later accepted spec.

The file list should be first because it is the primary product surface and the fastest place to prove that redesign improves usefulness rather than decoration. It exercises hard visual states: hover, selected, keyboard focus, hidden/protected files, metadata, icons/thumbnails, loading/fallback thumbnails, and large-folder virtualization. Starting with the titlebar or sidebar is visually tempting, but file-list rows create the most product value and expose regressions fastest.

The first file-list slice should factor reusable row presentation into named resources, but should stop short of a custom control. Named resources give reviewers a stable design-system surface and make visual regression easier, while a custom control adds API surface and lifecycle complexity before it is needed.

First file-list resources should include:

- `VfFileListItemContainerStyle`
- `VfFileListRowTemplate`
- `VfFileListRowNameTextStyle`
- `VfFileListRowMetadataTextStyle`
- `VfFileListRowIconSize`
- `VfFileListRowHeight`

The first slice should include named `DataTemplate`, named `ItemContainerStyle`, row state resources, and styling for selected, hover, focus, hidden/protected, and loading-thumbnail states. It should not add a new custom row control, new row behavior model, new selection system, or new virtualization behavior.

Create a custom row control later only if a subsequent file-list slice needs complex visual states that cannot stay cleanly in `DataTemplate` plus `ItemContainerStyle`.

First visual fixtures should capture states that prove the redesign improves the core product surface without breaking behavior:

```text
1. normal folder rows
2. selected row
3. keyboard-focused row
4. hover row, if stable to capture
5. multi-selection
6. hidden file row
7. protected/system file row
8. thumbnail loading/fallback row
9. metadata-heavy rows
10. empty folder state
```

The first committed baseline set should start small:

```text
tests/visual/baselines/winui/dark-comfortable-1440x900-100/
  file-list-normal.png
  file-list-selected-row.png
  file-list-focused-row.png
  file-list-selected-focused-row.png
  file-list-multi-selection.png
  file-list-hidden-protected.png
  file-list-thumbnail-fallback.png
  file-list-long-names.png
  file-list-empty-folder.png
```

Each screenshot should have a JSON sidecar:

```json
{
  "theme": "dark",
  "density": "comfortable",
  "viewport": "1440x900",
  "scale": 1.0,
  "screen": "file-list-selected-row",
  "fixture": "file-list-v1",
  "dynamicRegions": [],
  "reviewId": "..."
}
```

Use deterministic view-model fixtures exposed through a test launch mode for the first file-list screenshots. Do not use generated test files on disk for the first visual baseline. The first visual slice is about row styling, density, selection, focus, hidden/protected visual state, thumbnail fallback, and long-name layout; disk-backed fixtures add filesystem timing, icon extraction, thumbnail generation, metadata differences, antivirus effects, and platform variance.

Recommended shape:

```text
VeloFile --test-ui-fixture file-list-v1 --theme dark --density comfortable --viewport 1440x900
```

The fixture should produce deterministic rows through the app/view-model route: normal file, folder, long filename, hidden-style row, protected/system-style row, selected row, focused row, multi-selected rows, thumbnail fallback row, metadata-heavy row, and an empty-folder state as a separate fixture.

The test launch mode must be clearly non-production: enabled only in test/debug builds or guarded by an internal test flag, not exposed as a user setting, and not used to bypass normal app behavior in production.

Protect `--test-ui-fixture` with a layered test-host guard. It should be accepted only when:

1. the app is built with an internal test or Debug symbol;
2. `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` is present;
3. the fixture name is in a hardcoded allowlist.

Example:

```powershell
$env:VELOFILE_ENABLE_TEST_UI_FIXTURES = "1"
VeloFile.App.exe --test-ui-fixture file-list-v1
```

Do not make fixture mode a persisted setting. Do not allow fixture data paths from arbitrary user input in the first slice. The fixtures should be deterministic, compiled test fixtures. In production builds, `--test-ui-fixture` should exit with a clear nonzero test-only error. A later dedicated `DebugUiTest` build configuration may replace the initial Debug-plus-environment guard.

Production builds should reject `--test-ui-fixture` clearly and exit nonzero. They should not silently ignore the flag or launch the normal app, because silent ignore can hide CI/test misconfiguration and capture the wrong UI.

Fixture flag behavior:

| Build/context | Behavior |
|---|---|
| Production/Release | reject `--test-ui-fixture`, exit nonzero |
| Debug without env guard | reject, exit nonzero |
| Debug with `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` | allow only hardcoded fixture names |
| Unknown fixture name | reject, exit nonzero |

Defer a dedicated `DebugUiTest` build configuration until UI automation expands beyond first file-list fixtures. The first slice should use Debug plus `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` plus a fixture allowlist. Add `DebugUiTest` later if visual automation grows to multiple fixture families, UIA/FlaUI launch profiles, visual baseline CI jobs, fixture-only services, fake shell data providers, or deterministic animation/clock settings.

Generated disk-backed fixtures should be added later for integration screenshots where the point is to prove real listing, icon, thumbnail, metadata, search, or file-operation behavior.

Recommended fixture split:

| Evidence type | Fixture source |
|---|---|
| First file-list visual baseline | deterministic view-model fixture |
| Listing integration screenshot | generated test files on disk |
| Thumbnail/icon behavior screenshot | generated disk fixture after thumbnail path stabilizes |
| File-operation visual state | fake operation state fixture first; disk-backed later |
| Search results visual state | deterministic search-result fixture first; corpus-backed later |

Defer recursive search results, operation progress, conflict dialogs, drag/drop indicators, preview pane PDF navigation, context menu visual states, high-contrast mode, and mixed-DPI matrix until later slices because they involve more moving parts than first-slice file-list row styling.

## Risks and mitigations

- Risk: visual polish breaks file-manager behavior. Mitigation: keep command and service routes intact and verify existing behavior tests after each slice.
- Risk: the reference design accidentally becomes an unreviewed production contract. Mitigation: record `hifi-design/` as reference-only and make repo-owned specs and resources authoritative.
- Risk: design tokens are copied from web/CSS values without WinUI review. Mitigation: define VeloFile-native token categories and only use reference tokens as comparative input.
- Risk: density becomes an unsupported theme engine. Mitigation: keep density to the approved compact, comfortable, and spacious row/control spacing model.
- Risk: layout changes hurt performance in large folders. Mitigation: preserve virtualization and run listing/filter performance checks before claiming completion.
- Risk: accessibility regresses during custom styling. Mitigation: require accessible names, focus visibility, contrast checks, and keyboard routes in the spec and test spec.
- Risk: a large XAML rewrite becomes unreviewable. Mitigation: split by shell region and keep non-goals strict.

## Open questions

None blocking proposal review.

The remaining choices are implementation details for downstream spec, architecture, and plan artifacts:

- Whether `ui-contract-scopes.v1.json` validation is exposed as a separate `validate-scopes` command or as part of a broader UI contract validation command.
- The exact pre-window error reporting surface for rejected `--test-ui-fixture` launches.
- The future threshold for introducing `DebugUiTest`, such as multiple fixture families, fixture-only services, visual baseline CI jobs, or deterministic clock/animation services.

## Decision log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-11 | Recommend a repo-owned VeloFile design system followed by phased shell redesign. | It gives the project objective UI quality while preserving existing behavior, product ownership, and reviewability. | Direct prototype port, parallel shell rewrite, opportunistic polish only, adopting `hifi-design/` as authoritative. |
| 2026-05-11 | Treat `hifi-design/` as reference input only. | Production tokens, component contracts, layout, copy, and acceptance criteria must be owned by the repository. | Copying JSX structure directly into XAML; treating reference tokens as a production source of truth. |
| 2026-05-11 | Use UIA/FlaUI as the preferred first Windows UI automation path. | It matches the C#/.NET stack and tests desktop UI through Windows accessibility/control patterns without adding a WebDriver server dependency. | Appium/WinAppDriver as the first automation layer; screenshot-only validation. |
| 2026-05-11 | Defer persisted theme/density preferences from the first redesign slice. | Persistence creates durable settings behavior and migration expectations that should be specified separately. | Mixing new settings persistence into the first visual redesign slice. |
| 2026-05-11 | Record meaningful reference deviations explicitly. | Reviewers need to distinguish intentional VeloFile-native choices from accidental quality gaps. | Unrecorded deviations; pixel-parity enforcement against the reference. |
| 2026-05-11 | Use the small dark/comfortable VeloFile-owned token baseline for the first slice. | It proves the design-system path without creating a full theme engine or copying reference tokens. | Full reference token import; user-selectable density/theme in the first slice. |
| 2026-05-11 | Redesign file list rows first after the foundation resources. | The file list is the primary product surface and exercises the highest-risk visual states. | Starting with titlebar/sidebar polish; full shell rewrite. |
| 2026-05-11 | Treat screenshots as soft review evidence before hard-gating pixel diffs. | Desktop rendering can be noisy across GPU, font, DPI, theme, and antialiasing differences. | Immediate pixel-diff release gate; screenshots-only validation. |
| 2026-05-11 | Represent first-slice tokens as checked-in WinUI `ResourceDictionary` files merged from `App.xaml`. | This matches WinUI resource patterns, keeps the first slice reviewable, and avoids premature generated-token tooling. | Generated token pipeline in the first slice; inline values in `MainWindow.xaml`. |
| 2026-05-11 | Use named file-list `DataTemplate` and `ItemContainerStyle`, but no custom row control yet. | Named resources create a reusable design-system surface without adding unnecessary control lifecycle/API complexity. | Inline row styling only; new custom row control in the first slice. |
| 2026-05-11 | Use `scripts/update-ui-baselines.ps1` as an explicit review-gated baseline update command. | Tests should compare, humans should approve, and baseline mutation should be deliberate and traceable. | Baseline updates during normal CI; ad hoc manual file copying. |
| 2026-05-11 | Implement token conformance as spec-to-resource validation first, static XAML key checks second, and rendered smoke tests third. | The token contract must be clear before resource existence checks or rendered tests can be meaningful. | Static key checks as the only first gate; rendered resource tests as the first gate. |
| 2026-05-11 | Use PowerShell for baseline orchestration first and add a .NET visual-diff helper later. | Baseline approval is initially file orchestration; image comparison should become a focused helper when needed. | Overbuilding visual diff tooling in the first slice; doing image comparison in ad hoc PowerShell. |
| 2026-05-11 | Capture the first file-list visual fixtures for normal, selected, focused, selected+focused, multi-select, hidden/protected, thumbnail fallback, long names, and empty folder states. | These states prove the core product surface without pulling in later workflow surfaces. | Starting with recursive search, operation progress, dialogs, drag/drop, preview, high-contrast, or mixed-DPI visuals. |
| 2026-05-11 | Use `docs/ui/tokens.v1.json` as the first token contract artifact. | JSON supports reliable automated validation and future tooling without scraping prose. | Markdown as the authoritative token contract. |
| 2026-05-11 | Build a purpose-built token checker using XML DOM/XDocument parsing internally. | It can validate project-specific token rules against actual XAML resource dictionaries with useful errors. | MSBuild item metadata as the first validation source; generic XML parsing without token rules. |
| 2026-05-11 | Use deterministic view-model/test-launch fixtures for first file-list screenshots. | Early visual evidence should be stable and focused on row design rather than filesystem integration variance. | Generated disk-backed fixtures for the first visual baseline. |
| 2026-05-11 | Add `tools/VeloFile.UiContracts` to the solution in the first implementation slice. | CI restore/build should catch tool breakage and keep token validation discoverable. | Standalone orphan script/tool outside the solution. |
| 2026-05-11 | Guard `--test-ui-fixture` with Debug/test-build availability, `VELOFILE_ENABLE_TEST_UI_FIXTURES=1`, and a hardcoded fixture allowlist. | Fixture mode must be auditable and unavailable by accident in production/user launches. | Command-line flag alone; arbitrary fixture input paths; persisted fixture setting. |
| 2026-05-11 | Enforce tokenized literals strictly in new resources and only target the first-slice file-list references in `MainWindow.xaml`. | The checker should protect new design-system surfaces without turning the first slice into legacy XAML cleanup. | Global literal ban across `MainWindow.xaml`; no literal checks in the redesigned region. |
| 2026-05-11 | Use `docs/ui/ui-contract-scopes.v1.json` for first-slice enforcement scopes. | It is explicit, versioned, and can grow region by region while keeping token and scope contracts separate. | Unnamed ad hoc checker config; embedding scopes in the token contract. |
| 2026-05-11 | Reject `--test-ui-fixture` with nonzero exit outside an allowed Debug/test fixture context. | Failing loudly prevents screenshot CI from silently capturing the wrong production UI. | Silently ignoring the flag in production builds. |
| 2026-05-11 | Defer `DebugUiTest` until UI automation grows beyond first file-list fixtures. | A new build configuration adds project, CI, packaging, and contributor setup cost before it is needed. | Adding `DebugUiTest` in the first slice. |

## Next artifacts

- A UI redesign feature spec defining VeloFile-owned visual, interaction, and design-system acceptance criteria.
- A matching test spec for design-system conformance, shell contracts, layout, accessibility, reference comparison review, and visual regression.
- An architecture note or ADR for WinUI-native token/resource ownership, theme/density state, and UI automation approach.
- A living execution plan that splits implementation by shell region after spec and architecture are accepted.

## Follow-on artifacts

- Proposal review completed on 2026-05-11 with status `approved` and no material findings.

## Readiness

Ready for `spec`. The direction is repo-owned VeloFile UI design-system definition and shell redesign using `hifi-design/` as reference input, not hi-fi alignment or conformance. The open questions do not block specification; they should be resolved in spec, architecture, or plan artifacts before implementation as appropriate.
