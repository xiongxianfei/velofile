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

# M2 Change Explanation

M2 introduces the first production WinUI resources and moves file-list row presentation out of `MainWindow.xaml` without changing file-list behavior.

## What changed

- Added checked-in token dictionaries under `src/VeloFile.App/Resources/Tokens/` for the accepted dark/comfortable first-slice contract.
- Added `src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml` with `VfFileListRowTemplate`, `VfFileListItemContainerStyle`, row text styles, and row padding.
- Merged the token and file-list dictionaries from `App.xaml`.
- Replaced the inline `FileListSurface` row template in `MainWindow.xaml` with `ItemTemplate="{StaticResource VfFileListRowTemplate}"` and `ItemContainerStyle="{StaticResource VfFileListItemContainerStyle}"`.
- Added explicit `ui-contract-scope:file-list-first-slice` markers around the redesigned file-list region.
- Updated static app-shell tests to follow the extracted file-list component resource.
- In M2 review-resolution, added named row state resources for selected, hover, focused, selected-focused, hidden, and protected row states.
- In M2 review-resolution, scoped `ListViewItemBackground*` selected/hover resource mappings to `FileListSurface.Resources` and wired focus visuals through named row focus resources.

## Why it changed

The approved M2 slice requires production WinUI resources to conform to the repo-owned token contract and requires file-list rows to consume named resources. Extracting only the row template and item-container style gives reviewable design-system ownership while preserving the existing command, selection, context-menu, drag/drop, thumbnail, metadata, and dimmed-row state paths.

## Boundaries preserved

M2 does not add fixture mode, screenshot baselines, runtime theme/density switching, persisted settings, a custom row control, a behavior model, a new selection system, or virtualization changes.

## Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~FileListResourceContractTests.App_resources_merge"` failed before implementation for the expected missing resource dictionaries.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~FileListResourceContractTests"` passed with 6 tests.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources` passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"` passed.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed on rerun with build success, UI contract validation pass, and 362 tests.
- M2 review-resolution validation passed with `dotnet test VeloFile.sln -c Debug --filter FileListResourceContractTests`, scoped UI contract validation, `dotnet build VeloFile.sln -c Debug`, `dotnet test VeloFile.sln -c Debug --filter UiContracts`, and `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`.
