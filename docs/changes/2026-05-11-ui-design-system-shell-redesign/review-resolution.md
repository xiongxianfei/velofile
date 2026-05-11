# Review Resolution

## Status

open

## Pending findings

### CR-M1-001

- Source: [code-review-r1](reviews/code-review-r1.md)
- Status: resolved
- Required outcome: The validator must fail with an actionable message when governed first-slice resource dictionaries contain unapproved extra first-slice resources.
- Resolution: `tools/VeloFile.UiContracts` now exposes parsed resource metadata and validates governed `Resources/Tokens` and `Resources/Components` dictionaries against allowed keys built from `tokens.v1.json` plus `ui-contract-scopes.v1.json` `allowedResourceKeys`.
- Evidence: Added invalid `extra-resource` and `token-undocumented-color` fixtures plus tests that assert `extra-resource` diagnostics include the offending key and fixture path.

### CR-M1-002

- Source: [code-review-r1](reviews/code-review-r1.md)
- Status: resolved
- Required outcome: New token/component resource dictionaries must be checked for forbidden unapproved literals as part of the static validator path, without imposing a global ban on legacy XAML outside first-slice scope.
- Resolution: `validate-tokens` now runs governed resource validation by default for `Resources/Tokens` and `Resources/Components`. Component dictionaries reject unapproved inline color, row-height, padding, and similar first-slice literals, while explicit scoped legacy XAML checks remain behind `--scopes`.
- Evidence: Added invalid `component-inline-color`, `component-inline-row-height`, and `component-inline-padding` fixtures plus tests that assert `forbidden-literal` diagnostics include the offending value/property and fixture path.

## Validation

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter UiContracts` passed: 14 tests.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes docs\ui\ui-contract-scopes.v1.json --scope-root tests\fixtures\ui-contracts\valid` passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 356 tests passed.

## Pending M2 findings

### CR-M2-001

- Source: [code-review-r2](reviews/code-review-r2.md)
- Status: pending
- Required outcome: The M2 file-list row resources must either govern selected/focused row visuals through named first-slice resources, or explicitly record and verify a scoped Windows-native/system-focus decision that proves the default WinUI selected/focused visuals satisfy R54, A11Y1, and A11Y2 while preserving high-contrast/system behavior.
- Resolution: pending
- Evidence: pending
