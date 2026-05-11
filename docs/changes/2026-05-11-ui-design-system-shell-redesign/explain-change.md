# M1 Change Explanation

M1 establishes the static UI contract layer before any production XAML consumes new design resources.

## What changed

- Added `docs/ui/tokens.v1.json` as the machine-readable first-slice token contract for dark, comfortable defaults.
- Added `docs/ui/ui-contract-scopes.v1.json` with the first active file-list scope, required resource references, and targeted forbidden literal rules.
- Added `docs/ui/design-deviations.md` with deviation status values and the required review template.
- Added `tools/VeloFile.UiContracts`, a solution-included .NET console validator that parses token JSON and XAML resource dictionaries as static files.
- Added controlled valid and invalid XAML fixtures under `tests/fixtures/ui-contracts/`.
- Added focused MSTest coverage under `tests/VeloFile.Corpus.Tests/UiContracts/`.
- Added the M1 valid-fixture validator command to `scripts/ci.ps1`.

## Why it changed

The approved spec requires VeloFile-owned UI contracts instead of treating `hifi-design/` as the source of truth. The validator gives the repo a fast, local proof that accepted tokens and first-slice scope rules can be checked before the WinUI resource dictionaries are introduced in M2.

## Boundaries preserved

M1 does not change production UI resources, file-list rendering, selection behavior, fixture mode, screenshot baselines, persistence, diagnostics, Windows integration, or runtime app behavior.

The validator is intentionally static: it reads local JSON/XAML files, reports actionable failures, and does not launch the app or depend on WinUI runtime APIs.

## Validation

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter UiContracts` failed before implementation for the expected missing M1 artifacts, then passed after implementation.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid` passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed with 9 tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed with 351 tests.
