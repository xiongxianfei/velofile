# ADR 0009: UI Design Contracts, Static Validation, and Visual Fixtures

## Status

accepted

## Context

The UI design-system and shell redesign first slice introduces a repo-owned visual contract for VeloFile's WinUI shell. The accepted direction treats `hifi-design/` as reference input only, not as an authoritative source for production tokens, component anatomy, layout, copy, implementation strategy, or acceptance criteria.

The first slice changes more than XAML styling. It adds durable contract artifacts, static validation tooling, a non-production fixture launch path, and reviewed screenshot evidence. These boundaries must be architecture-owned before execution planning so implementation does not turn the reference package, inline XAML values, fixture flags, or screenshot files into accidental sources of truth.

## Decision

VeloFile UI design-system authority flows from accepted repo artifacts to implementation:

```text
accepted UI spec
  -> docs/ui/tokens.v1.json
  -> docs/ui/ui-contract-scopes.v1.json
  -> checked-in WinUI ResourceDictionary files
  -> UI contract validation and accepted visual evidence
```

`hifi-design/` remains reference input only. It may inform comparison review and deviation records, but production conformance is checked against VeloFile-owned contracts.

First-slice tokens are represented as checked-in WinUI resource dictionaries merged from the app resource tree. The first implementation does not introduce a generated token pipeline. File-list row presentation uses named resources, a named row template, and a named item-container style rather than a custom row control.

Static UI contract validation is owned by a lightweight .NET tool under `tools/VeloFile.UiContracts` and included in `VeloFile.sln`. The tool parses token JSON and XAML files as static artifacts. It must not require a running app or the WinUI runtime for first-slice validation.

Visual baseline approval is orchestrated by PowerShell. The baseline update script is a review-gated maintainer command that copies already-reviewed current screenshots and JSON sidecars into committed baseline storage. Normal CI may generate current screenshots and diffs, but it must not update committed baselines.

First-slice visual fixtures use a deterministic Debug/test-only app launch path. `--test-ui-fixture` is accepted only when:

- the app is in an allowed Debug/test context;
- `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` is present;
- the fixture name is hardcoded in an allowlist.

Release or production builds supplied with `--test-ui-fixture`, Debug/test builds without the environment guard, and unknown fixture names all reject the launch with a nonzero exit before rendering normal or fixture UI. The first slice does not accept arbitrary fixture data paths.

## Alternatives considered

- Adopt `hifi-design/` tokens or component structure as the production contract: rejected because it gives an external reference package authority over WinUI production behavior.
- Keep tokens and file-list styling inline in `MainWindow.xaml`: rejected because it hides design-system decisions in a broad shell file and makes drift hard to validate.
- Generate WinUI resources from a token pipeline in the first slice: rejected as premature for a small dark/comfortable baseline.
- Use MSBuild item metadata as the first validation source: rejected because it can help discover files but does not validate actual resource keys, values, types, or scoped literals well.
- Make screenshot pixel diff a hard release gate immediately: rejected because desktop rendering can be noisy across DPI, font rasterization, GPU, Windows theme, and CI environment.
- Silently ignore `--test-ui-fixture` outside allowed test contexts: rejected because screenshot jobs could capture the wrong production UI and pass misleadingly.
- Add a dedicated `DebugUiTest` configuration immediately: rejected because the first fixture family does not justify new project, CI, packaging, and contributor setup surface.

## Consequences

- The UI redesign has an explicit contract-to-resource validation boundary before implementation begins.
- `tools/VeloFile.UiContracts` becomes part of the solution build surface and can expand later to scope validation, visual metadata validation, or design-deviation checks.
- WinUI resource dictionaries become the implementation bridge for VeloFile-owned tokens and file-list component resources.
- Fixture-mode security is explicit and testable through process exit behavior.
- First-slice screenshots are evidence for deterministic row presentation, not a substitute for V1 integration evidence through filesystem, Windows adapter, drag/drop, preview, or file-operation boundaries.
- Future persisted theme or density behavior requires a separate spec and architecture update because the first slice intentionally has fixed dark/comfortable defaults and no new durable settings.

## Required Tests

ADR 0009 is not implementation-ready until tests verify:

- `docs/ui/tokens.v1.json` is parsed and validated against governed XAML resource dictionaries.
- Missing required token keys, wrong directly comparable values, wrong resource types, duplicate governed keys, and invalid color-to-brush relationships fail with nonzero exits and actionable messages.
- `docs/ui/ui-contract-scopes.v1.json` governs the first file-list scope and flags forbidden literals only inside active first-slice scopes.
- The UI contract tool runs without launching the app.
- `tools/VeloFile.UiContracts` is included in `VeloFile.sln`.
- Release/production launch with `--test-ui-fixture` exits nonzero before rendering normal or fixture UI.
- Debug/test launch with `--test-ui-fixture` and no `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` exits nonzero.
- Debug/test launch accepts only hardcoded fixture names when the environment guard is present.
- Fixture mode does not accept arbitrary fixture data paths.
- Baseline update refuses to run without a review id or without current screenshots.
- Normal CI does not mutate committed visual baselines.
- Generated current screenshot and diff directories are ignored by Git and are not committed.

## Follow-up

The matching test spec must map the required tests above to the UI design-system and shell redesign requirements. A later ADR or architecture update is required before introducing persisted theme/density settings, a generated token pipeline, hard-gated screenshot pixel diffs, a dedicated `DebugUiTest` build configuration, or broad UI automation beyond the first file-list fixture family.
