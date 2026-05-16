# Review Resolution

## Status

closed; all recorded material findings have a final disposition and required action

## Findings

### CR-002: M2 visual evidence note does not record an actual full-shell visual review

- Source reviews: [code-review-r4](reviews/code-review-r4.md), [code-review-r5](reviews/code-review-r5.md), [code-review-r6](reviews/code-review-r6.md), [code-review-r7](reviews/code-review-r7.md)
- Status: resolved
- Required outcome: before M2 can close, record actual `shell-default` full-shell visual evidence for `shell-standard-1440x900-100` through an automated/current screenshot, a manual screenshot review, or a manual visual-review note that records an observed whole-shell review result. Any accepted mismatch must be recorded in `docs/ui/design-deviations.md`.
- Resolution plan:
  - Launch or otherwise visually inspect the M2 default shell in the required profile when possible.
  - Update `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md` with reviewer/date, profile, observed whole-shell result, and deviation status; include screenshot/sidecar if automation becomes available.
  - If the shell cannot be launched in the current environment, keep M2 in `resolution-needed` and do not close the milestone.
  - Rerun M2 targeted validation after the evidence record is corrected.
- Validation:
  - `rg -n "capture-ui|screenshot|update-ui-baselines|visual/current|shell-default|UiFixture|test-ui-fixture" scripts src tests docs -g "!*bin*" -g "!*obj*"`
  - `Get-ChildItem scripts`
  - `Get-ChildItem src\VeloFile.App\bin -Recurse -Filter VeloFile.App.exe`
  - `dotnet test VeloFile.sln -c Debug --filter ShellSurfaceResourceContractTests`: passed after rerun; the first parallel attempt hit a transient file lock.
  - `dotnet test VeloFile.sln -c Debug --filter ShellVisualCoherenceContractTests`: passed after rerun; the first parallel attempt hit a transient file lock.
  - `dotnet test VeloFile.sln -c Debug --filter UiContracts`: passed.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 142/Windows 52/Corpus 37 tests passed.
- Resolution:
  - Confirmed there is no checked-in script that launches the WinUI app and captures `shell-default` current screenshots.
  - Replaced the prior static-only visual evidence note with an explicit unavailable evidence record at `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`.
  - A later bounded local capture attempt launched the WinUI app and confirmed it exposes a visible `VeloFile` window, but the local desktop reported `150%` scale and the attempt did not produce valid `shell-standard-1440x900-100` evidence. Generated invalid current screenshot artifacts were discarded.
  - Recorded the maintainer-reviewed diagnostic screenshot as failed visual evidence rather than accepted evidence. Blocking observations were readability/contrast, unexpected toggle state text, placeholder file-list icon chips, and disconnected command/search controls.
  - Added M2-scoped static coverage and resource fixes for shell text/control foregrounds, readable token contrast pairs, compact sidebar toggles without visible On/Off content, shell control styles, and deterministic non-text file-list icon treatment.
  - Recorded maintainer-provided manual full-shell visual review in `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`.
  - Maintainer accepted the current `shell-default` M2 shell evidence for `shell-standard-1440x900-100` and reported that the shell is okay for M2 closeout.
  - M2 may return to code review.
- Validation after accepted evidence:
  - `git diff --check -- docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md docs/plans/2026-05-11-ui-shell-visual-coherence.md`

### CR-001: Governed fixture icon validation does not reject local icon color literals

- Source review: [code-review-r2](reviews/code-review-r2.md)
- Status: resolved
- Required outcome: governed fixture icon validation rejects unapproved local icon color literals, and M1 tests include direct proof for the local icon color case.
- Resolution plan:
  - Add an invalid governed fixture icon case with a local color literal.
  - Extend `tools/VeloFile.UiContracts` icon scanning to reject raw `Fill`, `Stroke`, `Foreground`, `Background`, or `Color` values in governed icon resources.
  - Rerun M1 targeted validation and return M1 to `review-requested`.
- Validation: pending.
- Resolution:
  - Added direct fixture icon color tests for raw `Fill="#FFFFFF"` and `Color="#FFFFFF"` literals.
  - Added a valid resource-reference fixture icon case.
  - Added checked UI contract fixtures under `tests/fixtures/ui-contracts/invalid/fixture-icon-local-color/` and `tests/fixtures/ui-contracts/valid/Resources/Icons/`.
  - Extended `tools/VeloFile.UiContracts` to reject raw local icon color literals in governed `Resources/Icons` files for `Fill`, `Stroke`, `Foreground`, `Background`, `Color`, and `BorderBrush`.
- Validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "ShellVisualCoherenceContractTests"`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid\Resources`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\invalid\fixture-icon-local-color\Resources`
  - `dotnet test VeloFile.sln -c Debug --filter ShellVisualCoherenceContractTests`
  - `dotnet test VeloFile.sln -c Debug --filter UiContracts`
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`

### CR-003: Titlebar color path bypasses the governed token/resource contract

- Source review: [code-review-r8](reviews/code-review-r8.md)
- Status: resolved
- Required outcome: native titlebar colors must be derived from VeloFile-owned token/resource values or another governed resource bridge, not hard-coded local RGB values in app code. Focused validation must prevent governed shell chrome/titlebar color literals from reappearing outside token dictionaries or an accepted bridge.
- Resolution plan:
  - Move titlebar color values into governed tokens/resources and read/convert those resources when configuring `AppWindow.TitleBar`, or create a narrow titlebar resource bridge that is explicitly checked by tests.
  - Add a focused test that rejects raw `Color.FromArgb(...)` titlebar literals in `MainWindow.xaml.cs` unless values come from an approved token/resource bridge.
  - Rerun M2 focused UI contract tests and UI contract validation.
- Resolution:
  - Added governed `VfTitleBar*Color` aliases in `src/VeloFile.App/Resources/Components/VeloFile.Shell.xaml`.
  - Declared the titlebar resource keys in the active `shell-surface-foundation` scope in `docs/ui/ui-contract-scopes.v1.json`.
  - Replaced raw `Color.FromArgb(...)` titlebar assignments in `MainWindow.xaml.cs` with `ResolveTitleBarColor(...)` calls that read merged app resources.
  - Added focused `ShellSurfaceResourceContractTests` coverage for titlebar resource keys and for rejecting raw titlebar color literals in `MainWindow.xaml.cs`.
- Validation:
  - `dotnet build VeloFile.sln -c Debug`: passed.
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurfaceResourceContractTests|AppShellContract|UiDesign"`: passed, 39 tests. A first parallel attempt failed with a transient file lock while build was writing the test assembly, then passed on rerun.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed after the titlebar keys were added to the shell-surface scope.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`: passed, App 20 and Corpus 20 matching tests; Core/Windows reported no matching tests for the filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 147/Windows 52/Corpus 37 tests passed.
  - `git diff --check`: passed with CRLF warnings only.

### CR-004: Sidebar reordering enters M5 behavior scope without M5 evidence

- Source review: [code-review-r8](reviews/code-review-r8.md)
- Status: resolved
- Required outcome: either keep the M2 optimization strictly to surface/readability styling and defer sidebar reordering to M5, or explicitly reclassify this change as touching M5 sidebar behavior and provide the required M5-level evidence before accepting it.
- Resolution plan:
  - Preferred: revert only the sidebar reordering portion from the M2 diff while retaining M2 readability fixes such as hidden toggle content, text styles, and dark surface integration.
  - Alternative: formally treat this as M5-scoped implementation, update plan/handoff accordingly, and add/record keyboard order, accessible-name, discoverability, and behavior-preservation evidence required by the spec/test spec before re-review.
  - Rerun focused sidebar/app-shell tests and ensure the plan handoff summary accurately names the touched milestone.
- Resolution:
  - Reverted the structural sidebar ordering change from the M2 diff: visibility/terminal controls remain before the locations list until M5.
  - Kept M2 readability fixes: compact toggle rows, no visible On/Off text, tokenized label styling, terminal selector styling, dark shell integration, and accessible names.
  - Removed the M2 test that required navigation-first sidebar source ordering. Navigation-first ordering remains planned for M5.
- Validation:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurfaceResourceContractTests|AppShellContract|UiDesign"`: passed, 39 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`: passed, App 20 and Corpus 20 matching tests.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 147/Windows 52/Corpus 37 tests passed.
  - `git diff --check`: passed with CRLF warnings only.

### PR-001: Region milestones can close without required full-shell visual evidence

- Source review: [plan-review-r1](reviews/plan-review-r1.md)
- Status: resolved
- Required outcome: revise M2-M7 so each region milestone includes a soft full-shell screenshot or explicit manual visual evidence step before the milestone can close, and keep M8 as consolidation/baseline inventory rather than the first visual-evidence point.
- Resolution plan:
  - Update M2-M7 validation commands and milestone closeout criteria with per-region visual evidence.
  - Add a generic closeout bullet requiring full-shell screenshot or explicit manual visual evidence for the touched region, with deviations recorded when needed.
  - Keep M8 responsible for full-shell evidence inventory, baseline/profile consolidation, sidecar checks, and `200%` automated-or-manual classification.
- Validation: pending.
- Resolution:
  - Added a `Region-Slice Visual Evidence Rule` to `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
  - Updated M2-M7 validation commands and closeout criteria so each region milestone requires full-shell screenshot evidence or an explicit manual visual-review note before closeout.
  - Added milestone-specific evidence states for `shell-default`, `shell-file-list-selected-focused`, `shell-filter-active`, `shell-search-active`, `shell-sidebar-focused` or `shell-default`, `shell-operation-running`, `shell-destructive-confirmation`, and `shell-preview-open`.
  - Reframed M8 as full-shell evidence consolidation and baseline inventory rather than the first screenshot milestone.
- Validation:
  - `git diff --check -- docs/plans/2026-05-11-ui-shell-visual-coherence.md docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`
