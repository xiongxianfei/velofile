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

## Deferred By Design

- M1 does not create shell surface, command band, sidebar, status, preview, or icon production resources.
- M1 does not capture screenshots.
- M1 does not change App/Core/Windows behavior routes.

Those surfaces belong to later milestones.
