# Review Resolution

## Status

Closeout status: closed

All tracked material findings have final dispositions. M1-M7 are closed, optional M8 is not triggered, and no unresolved decision finding remains.

## Findings

### CR-008: Command-band state tokens are declared but not consumed by governed controls

- Source review: [code-review-r14](reviews/code-review-r14.md)
- Status: resolved
- Required outcome: governed command-band controls must actually consume tokenized hover, pressed, disabled, focus, and input-border state resources, or the governing spec must be revised to narrow that requirement.
- Evidence:
  - Spec R45 requires command-band controls in governed scope to share tokenized hover, focus, pressed, disabled, and input border/background states.
  - Spec R48 requires disabled command-band controls to remain visibly disabled and distinguishable from active controls.
  - `src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml` declares state resources such as `VfCommandBandButtonHoverBackgroundBrush`, `VfCommandBandButtonPressedBackgroundBrush`, `VfCommandBandButtonDisabledBackgroundBrush`, `VfCommandBandButtonDisabledForegroundBrush`, `VfCommandBandInputFocusBorderBrush`, and `VfCommandBandDisabledOpacity`, but a focused scan found those keys are not consumed by production command-band control state styling except for placeholder foreground wiring.
  - `CommandBandResourceContractTests` checks key existence but does not prove state-resource consumption.
- Safe resolution path:
  - Keep the fix scoped to M4 command-band resources, XAML, UI contract tests, and fixtures.
  - Wire command-band `Button` and `TextBox` state visuals through WinUI-recognized scoped resources or an equivalent accepted style mechanism, using VeloFile resource aliases rather than local literals.
  - Add focused tests proving hover, pressed, disabled, and focused-border resources are consumed by governed command-band controls.
  - Rerun focused M4 validation and return M4 to code review.
- Resolution:
  - Added command-band state-resource consumption tests for button pointer-over, pressed, disabled, disabled opacity, TextBox scoped input state resources, and governed MainWindow command-band style usage.
  - Wired `VfCommandBandButtonStyle` through a command-band button template that consumes hover, pressed, disabled background, disabled foreground, and disabled opacity resources.
  - Added scoped WinUI `TextControl*` resource aliases around governed command-band TextBoxes so normal, pointer-over, focused, disabled, placeholder, and border states resolve through VeloFile command-band resources.
  - Added `VfCommandBandTextBoxStyle`, `VfCommandBandPathTextBoxStyle`, and `VfCommandBandSearchTextBoxStyle` so path/search inputs use explicit governed command-band TextBox styles.
  - Updated `docs/ui/ui-contract-scopes.v1.json` and valid UI-contract fixtures to include the command-band state resources and styles.
- Validation:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FullyQualifiedName~CommandBandResourceContractTests"`: passed, 7 tests.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellCommandRouteTests|Search|Accessibility"`: passed; Core 6, App 64, Corpus 20 matching tests passed; Windows had no matching tests for this filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168, App 156, Windows 52, and Corpus 90 tests passed.
  - `git diff --check`: passed with CRLF normalization warnings only.

### TSR-001: Test spec readiness still points to stale M1 implementation

- Source review: [test-spec-review-r1](reviews/test-spec-review-r1.md)
- Status: resolved
- Required outcome: update the test spec status, next artifacts, follow-on artifacts, and readiness sections so they describe the visual-evidence gate amendment review state instead of directing the workflow to already-closed M1 implementation.
- Resolution plan:
  - Replace stale M1 `Next Artifacts` and `Readiness` wording with amendment-specific wording.
  - Record upstream approvals already completed: spec-review-r2, architecture-review-r1, and plan-review-r4.
  - Run `git diff --check` and a focused scan for stale `ready for implement at M1`, `M1 should begin`, and `draft amendment ... pending review` wording in the test spec.
  - Return to test-spec review.
- Validation:
  - `git diff --check`: passed with CRLF normalization warnings only.
  - `rg -n "ready for `implement` at M1|M1 should begin|draft amendment.*pending review" specs\ui-shell-visual-coherence.test.md`: no matches.
  - `Select-String -Path docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml -Pattern '^  validation:'`: found exactly one current `implementation.validation` key.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/test-spec-review-r2.md`: approved with no material findings.

### PR-002: Validation plan still describes required full-shell screenshot states/profiles

- Source review: [plan-review-r3](reviews/plan-review-r3.md)
- Status: resolved
- Required outcome: revise the active plan validation wording so screenshot and sidecar inventory validation is conditional on optional visual artifacts being recorded, and so no active plan section refers to required screenshot states/profiles under the amended contract.
- Resolution plan:
  - Change the validation section's full-shell screenshot/M8 wording to optional and conditional.
  - Replace "required states and profiles" with "chosen optional states and profiles when visual artifacts are recorded" or equivalent wording.
  - Normalize the stale M4 milestone state wording noted in plan-review-r3.
  - Rerun `git diff --check` and a focused `rg` scan for stale hard-gate visual evidence wording.
  - Return to plan-review before test-spec review.
- Validation:
  - `git diff --check`: passed with CRLF normalization warnings only.
  - `rg -n "required screenshot|required full-shell|required visual evidence|required states and profiles|Full-shell screenshot and sidecar inventory validation runs in M8|full-shell evidence" docs\plans\2026-05-11-ui-shell-visual-coherence.md`: no active stale hard-gate matches after the plan revision.
  - `Select-String -Path docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml -Pattern '^  validation:'`: found exactly one current `implementation.validation` key.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r4.md`: approved with no material findings.

### CR-007: M4 code review cannot rely on an unreviewed visual-evidence gate amendment

- Source review: [code-review-r13](reviews/code-review-r13.md)
- Status: resolved
- Required outcome: complete and record the upstream reviews for the 2026-05-17 visual-evidence gate removal amendment before using the amended contract to close CR-005 or pass M4 code review. Align stale amendment readiness text before those reviews complete.
- Progress:
  - `spec-review-r2` approved the feature-spec amendment with no material findings.
  - `architecture-review-r1` approved the canonical architecture and ADR amendment with no material findings.
  - `plan-review-r4` approved the plan amendment with no material findings.
  - `test-spec-review-r2` approved the test-spec amendment with no material findings.
- Resolution plan:
  - Return M4 to code review under the amended proof model.
- Validation:
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/spec-review-r2.md`: approved with no material findings.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/architecture-review-r1.md`: approved with no material findings.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r3.md`: revise with PR-002.
  - PR-002 resolved by plan wording updates.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r4.md`: approved with no material findings.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/test-spec-review-r1.md`: revise with TSR-001.
  - TSR-001 resolved by test spec lifecycle/readiness updates.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/test-spec-review-r2.md`: approved with no material findings.

### CR-005: M4 lacks required command-band full-shell visual evidence

- Source reviews: [code-review-r11](reviews/code-review-r11.md), [code-review-r12](reviews/code-review-r12.md)
- Status: resolved/superseded by approved visual-evidence gate removal amendment
- Original required outcome: before M4 could close under the previous contract, record accepted full-shell visual evidence for `shell-filter-active` and `shell-search-active` at `shell-standard-1440x900-100`, or accepted manual full-shell visual-review notes for those states. Any accepted mismatch had to be recorded in `docs/ui/design-deviations.md`.
- Amendment outcome: the approved 2026-05-17 spec/test-spec/architecture/plan amendment removes mandatory visual-evidence gates. CR-005 no longer requires `m4-shell-filter-active.md` or `m4-shell-search-active.md` evidence records and is superseded by the amended contract.
- Resolution plan:
  - Return M4 to code review without requiring `m4-shell-filter-active.md` or `m4-shell-search-active.md`.
- Validation:
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/spec-review-r2.md`: approved.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/architecture-review-r1.md`: approved.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r4.md`: approved.
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/test-spec-review-r2.md`: approved.

### CR-006: `change.yaml` has duplicate `implementation.validation` keys

- Source review: [code-review-r11](reviews/code-review-r11.md)
- Status: resolved
- Required outcome: make `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml` unambiguous by keeping one current M4 `implementation.validation` list and moving or removing older M3 validation entries from that key.
- Resolution plan:
  - Consolidate the duplicated `implementation.validation` field.
  - Preserve older M3 validation only under a clearly named historical field if it is still needed.
  - Run `git diff --check` and any available YAML validation before returning M4 to code review.
- Resolution:
  - Kept one current `implementation.validation` field for M4 command-band validation.
  - Moved older M3 validation under `history.prior_milestone_validation.M3`.
- Validation:
  - `git diff --check docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml`: passed with CRLF normalization warning only.
  - `Select-String -Path docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml -Pattern '^  validation:'`: found exactly one current `implementation.validation` key at line 34.

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
- Validation: resolved by the focused and broad validation evidence listed below.
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
- Status: superseded by 2026-05-17 visual-evidence-gate removal amendment
- Required outcome: revise M2-M7 so each region milestone includes a soft full-shell screenshot or explicit manual visual evidence step before the milestone can close, and keep M8 as consolidation/baseline inventory rather than the first visual-evidence point.
- Resolution plan:
  - Update M2-M7 validation commands and milestone closeout criteria with per-region visual evidence.
  - Add a generic closeout bullet requiring full-shell screenshot or explicit manual visual evidence for the touched region, with deviations recorded when needed.
  - Keep M8 responsible for full-shell evidence inventory, baseline/profile consolidation, sidecar checks, and `200%` automated-or-manual classification.
- Validation: superseded by the approved visual-evidence gate removal amendment and the amendment review evidence listed for CR-005/CR-007/PR-002/TSR-001.
- Resolution:
  - Added a `Region-Slice Visual Evidence Rule` to `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
  - Updated M2-M7 validation commands and closeout criteria so each region milestone requires full-shell screenshot evidence or an explicit manual visual-review note before closeout.
  - Added milestone-specific evidence states for `shell-default`, `shell-file-list-selected-focused`, `shell-filter-active`, `shell-search-active`, `shell-sidebar-focused` or `shell-default`, `shell-operation-running`, `shell-destructive-confirmation`, and `shell-preview-open`.
  - Reframed M8 as full-shell evidence consolidation and baseline inventory rather than the first screenshot milestone.
  - 2026-05-17 amendment: maintainer requested an M3-only deferral so `shell-file-list-selected-focused` full-shell visual evidence is no longer required before M3 code review. That deferral is now superseded by the approved visual-evidence-gate removal amendment.
- Validation:
  - `git diff --check -- docs/plans/2026-05-11-ui-shell-visual-coherence.md docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`
