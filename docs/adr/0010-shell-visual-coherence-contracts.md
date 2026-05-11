# ADR 0010: Shell Visual-Coherence Contracts and Evidence

## Status

accepted

## Context

The shell visual-coherence follow-on spec extends the first UI design-system slice from file-list resources into app-wide shell surfaces. The change affects multiple long-lived architecture surfaces: token and scope contract evolution, fixture icon resources, static validation, full-shell screenshot evidence, high-DPI evidence, sidebar behavior preservation, and the boundary between visual fixture proof and real V1 behavior proof.

ADR 0009 remains the decision record for first-slice UI design contracts, static validation, and deterministic file-list fixtures. This ADR records the follow-on decisions needed for shell-wide visual coherence without rewriting ADR 0009.

## Decision

Shell visual-coherence work extends the existing V1 UI contract chain additively:

```text
accepted shell visual-coherence spec
  -> docs/ui/tokens.v1.json additive entries
  -> docs/ui/ui-contract-scopes.v1.json additive governed scopes
  -> checked-in WinUI ResourceDictionary files
  -> fixture vector icon resources
  -> static UI contract validation
  -> full-shell soft-review visual evidence
  -> behavior-preservation matrix evidence
```

`docs/ui/tokens.v1.json` remains the V1 token contract and `docs/ui/ui-contract-scopes.v1.json` remains the V1 scope contract. A new major token/scope version is reserved for incompatible semantics, renamed resource contracts, or a deliberate redesign reset.

Deterministic fixture icons are VeloFile-owned XAML vector resources under:

```text
src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml
```

Fixture rows expose allowlisted icon kinds such as `Folder`, `Pdf`, or `ThumbnailFallback`. The fixture host maps those kinds to named vector geometry resources. Fixture input must not accept arbitrary icon resource keys. Governed fixture/file-list icon scopes reject `SymbolIcon`, `PathIcon`, private-use glyph fonts, ellipsized text chips, unapproved icon colors, and unapproved icon sizes.

Full-shell visual evidence uses effective-pixel app-window profiles:

```text
shell-min-900x560-100
shell-standard-1440x900-100
shell-standard-1440x900-200
```

`shell-standard-1440x900-200` is required evidence, but may be recorded as manual/release review evidence until automated scale control is stable. Screenshot evidence remains soft-review evidence; hard pixel or perceptual comparison requires a later accepted spec and architecture decision.

Every shell visual-coherence slice must be accompanied by behavior-preservation evidence. Fixture-only or screenshot-only proof does not verify real filesystem, Windows adapter, drag/drop, preview, file-operation, terminal, diagnostics, persistence, or accessibility behavior through the product boundary.

## Alternatives considered

- Create `tokens.v2.json` and a new scope contract immediately: rejected because the follow-on work adds regions and resources but does not require incompatible token semantics or a redesign reset.
- Keep icon fixture rendering as extension text chips: rejected because placeholder-looking text chips undermine deterministic visual evidence and repeat the defect class exposed by the earlier glyph regression.
- Use `SymbolIcon`, `PathIcon`, or private-use glyph fonts for deterministic fixture icons: rejected because shell icon glyph rendering already regressed and needs invariant static checks plus visual proof.
- Use real Windows Shell icons for first shell visual baselines: rejected because Shell icon extraction adds OS/theme/cache variance and belongs in later integration evidence.
- Require only the standard `1440 x 900` profile: rejected because minimum-size and high-DPI layout issues can hide at the standard profile.
- Make screenshot pixel comparison a hard gate in this follow-on slice: rejected because full-shell capture, dynamic regions, scaling, font rasterization, and CI environment behavior must stabilize before screenshots become release gates.
- Treat sidebar reordering as cosmetic only: rejected because sidebar grouping affects keyboard order, accessible names, discoverability, and user workflow.

## Consequences

- Existing first-slice UI contracts remain valid and are extended instead of forked.
- UI contract tooling expands from first-slice tokens and file-list scopes into shell-wide scopes, icon-resource invariants, and screenshot sidecar validation.
- A new icon resource dictionary becomes part of the WinUI app resource architecture.
- The fixture host gains an allowlisted icon-kind mapping responsibility while remaining Debug/test guarded and non-production.
- Full-shell screenshots become review evidence across required shell states and effective-pixel profiles.
- High-DPI review remains explicit even if initially manual.
- Architecture and test specs must distinguish visual fixture evidence from behavior evidence through App/Core/Windows routes.

## Follow-up

- The matching test spec must map shell visual-coherence requirements to static UI contract tests, icon invariant tests, screenshot sidecar tests, full-shell evidence inventory checks, high-DPI evidence classification, accessibility checks, and behavior-preservation matrix rows.
- The execution plan must keep shell surface foundation before file-list polish and deterministic icon treatment.
- A later ADR or architecture update is required before hard-gating screenshot pixel/perceptual diffs, introducing a token major version, adding a generated token pipeline, persisting theme/density settings, or relying on real Shell icons for deterministic baselines.
