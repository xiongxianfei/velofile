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
- In the second M2 review-resolution, removed the rendered `RowOpacity` path from the file-list row template, added semantic visibility state to `FileListRowViewModel`, and added an App-layer opacity converter that resolves hidden/protected row opacity from named VeloFile resources.

## Why it changed

The approved M2 slice requires production WinUI resources to conform to the repo-owned token contract and requires file-list rows to consume named resources. Extracting only the row template and item-container style gives reviewable design-system ownership while preserving the existing command, selection, context-menu, drag/drop, thumbnail, metadata, and semantic dimmed-row state paths.

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
- CR-M2-002 review-resolution validation passed with focused file-list contract tests, `dotnet build VeloFile.sln -c Debug`, scoped UI contract validation, `dotnet test VeloFile.sln -c Debug --filter UiContracts`, `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests`, and `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`.

# M3 Change Explanation

M3 adds the guarded non-production fixture path needed before screenshot evidence can be captured.

## What changed

- Added `src/VeloFile.App/Testing/UiFixtureLaunch.cs` with fixture command-line parsing and launch gating for `--test-ui-fixture`, `--theme`, `--density`, and `--viewport`.
- Added `src/VeloFile.App/Testing/UiFixtureRegistry.cs` with hardcoded `file-list-v1` and `file-list-empty-folder` fixtures.
- Wired `App.xaml.cs` to reject invalid fixture launches before normal window creation and to create a fixture shell view model only when the fixture request is accepted.
- Added `AppCompositionRoot.CreateFixtureShellViewModel` so fixture mode uses the existing app-shell/listing/view-model route.
- Added fixture tests for Release/production rejection, missing environment guard rejection, unknown fixture rejection, allowlisted acceptance, unsupported option/path rejection, static startup wiring, deterministic synthetic rows, required file-list visual states, empty-folder state, and disk-free view-model rendering.

## Why it changed

The approved M3 slice requires visual evidence fixtures to be deterministic and non-production. The launch guard prevents accidental production exposure, while the fixture registry gives M4 stable file-list states without generated disk fixtures, user paths, or filesystem timing.

## Boundaries preserved

M3 does not add screenshot baselines, fixture-data paths, a `DebugUiTest` configuration, a custom row control, a new selection system, persisted theme/density settings, or disk-backed integration fixtures.

Normal app launches without `--test-ui-fixture` still follow `CreateShellViewModel`. Fixture rows are synthetic and rooted at `C:\VeloFileFixture`; they do not read local workspace or user-profile files.

## Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Fixture|UiContracts"` failed before implementation for the expected missing fixture launch and registry files, then passed after implementation with 12 app tests.
- `dotnet test VeloFile.sln -c Debug --filter "Fixture|UiContracts"` passed: App fixture tests 12 passed, Corpus UI contract tests 14 passed, and Core/Windows projects had no matching tests.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Release` passed with 0 warnings and 0 errors.
- `rg "fixture-data|C:\\Users|xiongxianfei|20260428-velofile|--test-ui-fixture|VELOFILE_ENABLE_TEST_UI_FIXTURES" src\VeloFile.App tests\VeloFile.App.Tests docs -n` found only expected source, test, and documentation references.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed with build success, UI contract validation pass, and 379 tests.
- Process-level GUI launch checks were not run in this slice; fixture rejection and acceptance are covered by launch gate unit tests plus static startup wiring tests.

## Review resolution

CR-M3-001 found that selected/focused/multi-selected fixture states were labels only. The resolution adds explicit `UiFixturePresentationState` with stable synthetic row IDs and selected/focused targets, preserves that state through `UiFixtureShellState`, and passes it into `MainWindow` for accepted fixture launches. `MainWindow` now applies fixture selection through `FileListSurface.SelectedItems`, updates the normal selection mapping route, scrolls to the focused row, and focuses the generated row container.

Resolution validation:

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Fixture|UiContracts"` passed with 15 app tests.
- `dotnet test VeloFile.sln -c Debug --filter "Fixture|UiContracts"` passed.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Release` passed with 0 warnings and 0 errors.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed with build success, UI contract validation pass, and 382 tests.
