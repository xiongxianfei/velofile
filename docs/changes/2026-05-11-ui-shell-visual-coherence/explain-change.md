# Explain Change

## M1: Shell Contract and Validator Extension

M1 implements the first proof layer for the shell visual-coherence follow-on before any production shell visuals change.

## What Changed

- Added shell visual-coherence corpus tests in `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`.
- Extended `docs/ui/ui-contract-scopes.v1.json` with planned follow-on shell scopes for:
  - shell surface foundation,
  - command band,
  - sidebar,
  - status/operation surfaces,
  - preview/details,
  - fixture icons,
  - full-shell visual evidence.
- Added a behavior-preservation matrix inventory to `docs/ui/ui-contract-scopes.v1.json`.
- Extended `tools/VeloFile.UiContracts` so static validation can:
  - reject governed fixture icon resources that use `SymbolIcon`, `PathIcon`, private-use glyph fonts, ellipsized text chips, or local icon sizes;
  - validate full-shell visual sidecar required fields, profile size/scale consistency, dynamic-region shape, and privacy exclusions through `--visual-root`;
  - treat planned follow-on scopes as declared contract inventory while validating only active scopes.

## Why This Changed

The approved spec requires shell-wide token/scope validation, governed fixture icon checks, screenshot sidecar checks, and behavior-preservation matrix evidence before visual slices are trusted. M1 creates those static proof surfaces without changing Core, Windows adapters, or production shell behavior.

## Validation

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"` failed before implementation with the expected missing-scope, missing-icon-check, and missing-`--visual-root` failures.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"` passed after implementation: 3 passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "UiContracts|Visual"` passed: 21 passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed after rerunning with a longer timeout.

## CR-001 Resolution

Code review found that governed fixture icon validation rejected icon controls, glyphs, text chips, and local sizes, but not raw local icon colors. The resolution keeps the change inside M1's contract-validation boundary:

- added direct tests for `Fill="#FFFFFF"` and `Color="#FFFFFF"` in governed icon resources;
- added a valid resource-reference icon fixture;
- added checked valid/invalid icon fixture files;
- extended the validator to reject raw local icon color literals for `Fill`, `Stroke`, `Foreground`, `Background`, `Color`, and `BorderBrush` in `Resources/Icons`.

Additional validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"` passed: 6 passed.
- `dotnet test VeloFile.sln -c Debug --filter ShellVisualCoherenceContractTests` passed: 6 passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 20 passed.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed.

## Deferred By Design

- M1 does not create shell surface, command band, sidebar, status, preview, or icon production resources.
- M1 does not capture screenshots.
- M1 does not change App/Core/Windows behavior routes.

Those surfaces belong to later milestones.

## M2: Shell Surface Foundation

M2 implements the first production visual slice for the follow-on: a tokenized shell surface layer applied to the existing WinUI shell containers.

## What Changed

- Added `src/VeloFile.App/Resources/Components/VeloFile.Shell.xaml` with shell-owned styles and resource aliases for app root, chrome, sidebar, content, command band container, status container, preview container, separators, text hierarchy, focus/accent, selection, hover, disabled, danger, warning, and success surfaces.
- Merged the shell resource dictionary through `src/VeloFile.App/App.xaml` after token dictionaries and before file-list resources.
- Updated `src/VeloFile.App/MainWindow.xaml` so the root shell, tab/chrome, command band container, sidebar, content region, status container, and preview pane consume the shell foundation resources.
- Activated the `shell-surface-foundation` scope in `docs/ui/ui-contract-scopes.v1.json`.
- Added `tests/VeloFile.App.Tests/UiDesign/ShellSurfaceResourceContractTests.cs` to prove the dictionary merge, governed resource exposure, scoped resource consumption, and existing route-control preservation.
- Updated UI-contract fixtures so the valid fixture set also represents the newly active shell surface scope.
- Recorded M2 soft visual evidence at `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`.

## Why This Changed

The approved spec requires the shell surface foundation to be the first follow-on implementation region. The current shell was mixing raw/default surfaces with the first-slice file-list treatment. M2 creates the shared resource layer before deeper file-list, command band, sidebar, status, or preview polish.

M2 reuses existing first-slice V1 tokens instead of adding new token IDs because the current token contract already covers the needed dark comfortable surfaces, text colors, focus/accent, state colors, spacing, radius, sizing, and disabled opacity.

## Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter ShellSurface` failed before implementation with 3 expected failures, then passed after implementation: 4 passed.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurface|UiDesign|Accessibility"` passed: 18 passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes docs\ui\ui-contract-scopes.v1.json --scope-root tests\fixtures\ui-contracts\valid` passed after fixture update.
- `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"` passed for matching App and Corpus tests; Core and Windows had no matching tests for this filter.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors after rerunning without the parallel testhost lock.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core/App/Windows/Corpus tests passed.

## Deferred By Design

- M2 does not reorder the sidebar, redesign command controls, replace fixture icon chips, or change file-list row state treatment.
- M2 does not add persisted theme/density settings or new token major versions.
- M2 does not change Core, Windows adapters, file operations, preview providers, terminal launch, diagnostics, persistence, or runtime behavior routes.
- M2 records a manual visual-review note rather than a screenshot because full-shell screenshot automation is not stable or implemented for this milestone.

## M4: Command Band Visual Coherence

M4 implements the command/path/filter/search visual-resource slice while keeping the existing navigation and search behavior routes intact.

## What Changed

- Added `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml` with governed command-band brushes, spacing, sizing, padding, button, icon-button, breadcrumb, input, path, radio, status, and skipped-location text styles.
- Merged the command-band resource dictionary through `src/VeloFile.App/App.xaml`.
- Updated the existing command/path/filter/search XAML in `src/VeloFile.App/MainWindow.xaml` to consume command-band resources and added `shell-command-band` scope markers.
- Activated `shell-command-band` in `docs/ui/ui-contract-scopes.v1.json`.
- Updated valid UI-contract fixtures so the active production scope also validates under `scripts/ci.ps1`.
- Added `tests/VeloFile.App.Tests/UiDesign/CommandBandResourceContractTests.cs` to prove dictionary merge, required style resources, scoped XAML usage, route preservation, and accessibility names/tooltips.
- Updated the corpus shell visual-coherence contract test so the command-band scope is expected to be active at M4.
- Tightened the existing icon-button invariant test to reject actual icon controls/glyphs instead of rejecting style names containing `Icon`.

## Why This Changed

The approved M4 contract requires the command band to read as one intentional navigation/path/filter/search region while preserving existing V1 command behavior. This change keeps the handlers and view-model routes stable and moves the visual treatment into governed WinUI resources so later shell regions can share the same tokenized model.

## Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "CommandBandResourceContractTests"` failed before implementation for the expected missing command-band resources/scope, then passed: 4 tests.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid` passed.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "AppShellCommandRouteTests|CommandBand|Accessibility"` passed: 67 tests.
- `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"` passed for matching App, Core, and Corpus tests; Windows had no matching tests for this filter.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed; build had 0 warnings and 0 errors, UI contract validation passed, and Core/App/Windows/Corpus tests passed.

## Deferred By Design

- M4 does not change Core, Windows adapters, file operations, preview, sidebar ordering, status/operation surfaces, or command route behavior.
- M4 is not ready for code review until accepted `shell-filter-active` and `shell-search-active` full-shell visual evidence is recorded for `shell-standard-1440x900-100`.
