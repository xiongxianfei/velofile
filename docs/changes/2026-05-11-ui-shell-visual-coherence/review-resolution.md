# Review Resolution

## Status

blocked; CR-002 unresolved until accepted M2 full-shell visual evidence exists

## Findings

### CR-002: M2 visual evidence note does not record an actual full-shell visual review

- Source reviews: [code-review-r4](reviews/code-review-r4.md), [code-review-r5](reviews/code-review-r5.md), [code-review-r6](reviews/code-review-r6.md), [code-review-r7](reviews/code-review-r7.md)
- Status: blocked; visual evidence unavailable in this tool session
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
  - M2 remains `resolution-needed`; this does not satisfy R22, R26, TSC013, or the M2 visual-evidence closeout rule.
  - M2 still requires accepted `shell-default` evidence at `shell-standard-1440x900-100` before it can return to clean code review.

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
