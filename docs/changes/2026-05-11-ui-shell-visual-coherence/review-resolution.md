# Review Resolution

## Status

resolved; ready for M1 code-review rerun

## Findings

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
