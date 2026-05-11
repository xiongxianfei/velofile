# Review Resolution

## Status

All recorded M1 and M2 findings resolved. CR-M3-001 has been resolved and M3 is ready for rerun code-review.

## Pending findings

### CR-M4-001

- Source: [code-review-r5](reviews/code-review-r5.md)
- Status: open
- Required outcome: Listing results must still be validated against the current active listing request/tab at the point they mutate visible rows on the UI dispatcher. A stale completed listing must not be able to replace rows after the user navigates, switches tabs, closes tabs, or refreshes into a newer request.
- Safe resolution path: Keep the fix narrow in `AppShellViewModel` and app tests. Recheck `IsActiveListingResult(result)` inside the posted callback immediately before `ApplyListingState(result.State)`, or otherwise carry a request token that is checked on the UI dispatcher before mutation. Add a focused regression test with a queued test dispatcher proving a completed old listing does not apply after a newer navigation/request wins. Do not broaden M4 into a selection system, screenshot harness, or production UI redesign.

### CR-M3-001

- Source: [code-review-r4](reviews/code-review-r4.md)
- Status: resolved
- Required outcome: The fixture launch path must make selected, focused, selected+focused, and multi-selected fixture states deterministic through the app/UI boundary used for screenshots, or M3 must explicitly split those states into separate allowed fixtures/fixture instructions that the screenshot harness can apply deterministically before capture.
- Resolution: `UiFixtureDefinition` now carries `UiFixturePresentationState` with stable synthetic row IDs, selected row IDs, selected+focused row ID, multi-selected row IDs, focused row ID, and row ID to synthetic path mapping. `UiFixtureRegistry.CreateFileListV1ShellState` preserves that presentation state alongside the fixture view model. Accepted fixture launches pass the presentation state into `MainWindow`, where `ApplyUiFixturePresentationState` applies selected rows through `FileListSurface.SelectedItems`, updates the normal app selection route through `FileListSelectionMapper`, scrolls to the focused row, and focuses the generated row container through `Focus(FocusState.Keyboard)`.
- Evidence: Added fixture tests that assert presentation targets are explicit, row-ID based, and valid; that shell-state creation preserves presentation targets after conversion to `FileSystemEntrySnapshot`; that the presentation planner resolves selected/focused rows from the rendered row view models; and that the app startup/window path passes and applies fixture presentation state through `FileListSurface`.

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
- Status: resolved
- Required outcome: The M2 file-list row resources must either govern selected/focused row visuals through named first-slice resources, or explicitly record and verify a scoped Windows-native/system-focus decision that proves the default WinUI selected/focused visuals satisfy R54, A11Y1, and A11Y2 while preserving high-contrast/system behavior.
- Resolution: `VeloFile.FileList.xaml` now defines named row background, hover, selected, focused-border, selected-focused-border, and hidden/protected opacity resources. `VfFileListItemContainerStyle` consumes named focus brush/thickness resources through `FocusVisualPrimary*` and `FocusVisualSecondary*` setters. The scoped `FileListSurface.Resources` maps WinUI `ListViewItemBackground*` selected/hover keys to VeloFile token colors for the file-list region without changing selection behavior or adding a custom row control.
- Evidence: Added static app-shell tests that assert named selection/focus resources exist, focus setters consume those resources, selected row styling uses background resources rather than text color only, and no custom row control or behavior model is introduced.

## M2 review-resolution validation

- `dotnet test VeloFile.sln -c Debug --filter FileListResourceContractTests` passed: 8 app tests passed.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 corpus UI contract tests passed.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 364 tests passed.

## Pending M2 rerun findings

### CR-M2-002

- Source: [code-review-r3](reviews/code-review-r3.md)
- Status: resolved
- Required outcome: Hidden/protected file-list row styling must be governed by the accepted first-slice resources, or the implementation must record an explicit accepted deviation explaining why the row-view-model opacity remains the production authority.
- Resolution: `FileListRowViewModel` now exposes semantic visibility state (`IsHidden`, `IsProtectedOperatingSystemFile`, and `VisibilityKind`) instead of a rendered opacity value. `VeloFile.FileList.xaml` aliases hidden/protected row opacity resources to `VfStateHiddenOpacity`, adds `VfFileListRowOpacityConverter`, and binds row opacity through that converter so the rendered path resolves named resources rather than the old `RowOpacity` value.
- Evidence: Added static/resource tests that reject `Opacity="{Binding RowOpacity}"`, reject the stale `0.58` literal in the rendered path, assert hidden/protected opacity resources resolve from the accepted state token, and assert the selector maps semantic hidden/protected rows to `VfFileListRowHiddenOpacity` / `VfFileListRowProtectedOpacity`.

## M2 CR-M2-002 review-resolution validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter FileListResourceContractTests` failed before implementation for the expected missing opacity selector.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter FileListResourceContractTests` passed: 11 app tests passed.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet test VeloFile.sln -c Debug --filter FileListResourceContractTests` passed: 11 app tests passed.
- `dotnet test VeloFile.sln -c Debug --filter UiContracts` passed: 14 corpus UI contract tests passed.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests` passed: 18 app tests passed.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 367 tests passed.

## M3 CR-M3-001 review-resolution validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Fixture|UiContracts"` failed before the resolution for missing presentation-state code paths, then passed after resolution with 15 app tests.
- `dotnet test VeloFile.sln -c Debug --filter "Fixture|UiContracts"` passed: App fixture tests 15 passed, Corpus UI contract tests 14 passed, and Core/Windows projects had no matching tests.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Release` passed with 0 warnings and 0 errors.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build succeeded, UI contract validation passed, and 382 tests passed.
