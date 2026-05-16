# Code Review R9: M2 Shell Surface Foundation Re-Review

## Review Status

clean-with-notes

## Review Inputs

- Review surface: unstaged working-tree diff after CR-002, CR-003, and CR-004 review-resolution.
- Reviewed milestone: M2 Shell Surface Foundation.
- Spec: `specs/ui-shell-visual-coherence.md`.
- Test spec: `specs/ui-shell-visual-coherence.test.md`.
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
- Architecture / ADR:
  - `docs/architecture/system/architecture.md`
  - `docs/adr/0010-shell-visual-coherence-contracts.md`
- Change record:
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`
  - `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m2-shell-default.md`
- Diff summary reviewed with:
  - `git diff --name-only`
  - `git diff --stat`
  - focused `git diff` for `MainWindow.xaml`, `MainWindow.xaml.cs`, `VeloFile.Shell.xaml`, `ui-contract-scopes.v1.json`, `ShellSurfaceResourceContractTests.cs`, evidence, plan, and change records.
- Validation evidence:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurfaceResourceContractTests|AppShellContract|UiDesign"`: passed, 39 tests.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `dotnet test VeloFile.sln -c Debug --filter "UiContracts|AppShellContract|Accessibility"`: passed; App and Corpus matching tests passed, Core/Windows reported no matching tests for the filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`: passed; build 0 warnings/0 errors, UI contract validation passed, Core 168/App 147/Windows 52/Corpus 37 tests passed.
  - `git diff --check`: passed with CRLF warnings only.

## Diff Summary

The M2 diff applies the shell surface foundation by using the dark shell resource family in `MainWindow.xaml`, styling the shell root, chrome, sidebar, command/file-list/status/preview text, and compact visibility toggles. It suppresses visible toggle On/Off text while preserving accessible names and existing event routes. It routes native titlebar colors through governed `VfTitleBar*Color` resources and records those keys in the active shell-surface scope. The M2 tests now cover shell token/resource presence, contrast pairs, no raw default light-theme control dependency, no raw titlebar color literals, compact toggle content, deterministic non-text file-list icon treatment, and V1 route preservation.

The change record now records maintainer-provided manual full-shell visual evidence for `shell-default` at `shell-standard-1440x900-100`, resolves CR-002 through CR-004, and keeps deeper file-list, command-band, sidebar, operation/status, and preview polish assigned to M3-M7.

## Findings

No blocking or required-change findings.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | M2 stays inside R23-R27 shell foundation scope: shell resources are tokenized and applied, routes remain present, and sidebar reordering is deferred to M5. |
| Test coverage | pass | `ShellSurfaceResourceContractTests` covers resources, contrast, titlebar resource bridge, toggle content, file-list placeholder chip removal, and route preservation. |
| Edge cases | pass | CR-002 manual evidence is recorded; CR-003 raw titlebar literals are rejected; CR-004 sidebar ordering is no longer in M2. |
| Error handling | pass | `ConfigureDarkTitleBar` catches platform/titlebar failures so visual titlebar polish cannot block startup. |
| Architecture boundaries | pass | Changes stay in App UI resources/XAML/tests and UI scope docs; Core and Windows adapters are unchanged. |
| Compatibility | pass | Existing named routes, handlers, visibility toggles, terminal selector, file list, preview, and operation controls remain present. |
| Security/privacy | pass | The accepted evidence note is manual and does not commit screenshots, local paths, usernames, secrets, file contents, or preview text. |
| Derived artifact currency | pass | Plan, plan index, change record, review log, review-resolution, and visual evidence are synchronized for M2 review handoff. |
| Unrelated changes | pass | The reviewed diff is scoped to M2 shell foundation, reference-gap plan sharpening, evidence, and review records. `hifi-design/` remains untracked and outside the reviewed diff. |
| Validation evidence | pass | Focused tests, UI contract validation, broad filtered tests, full CI script, and diff check passed. |

## No-Finding Rationale

No blocking findings were found because the prior material findings have been resolved in tracked artifacts:

- CR-002 has maintainer-accepted manual full-shell visual evidence in `m2-shell-default.md`.
- CR-003 is resolved by governed `VfTitleBar*Color` resources and a titlebar resource bridge instead of raw code literals.
- CR-004 is resolved by deferring sidebar reordering to M5 while keeping M2 readability fixes.

The remaining visible-quality gap to `hifi-design/` is intentionally assigned to later milestones: M3 file-list polish/icons, M4 command/path/search band, M5 sidebar component system, M6 operation/status surfaces, M7 preview/details pane, and M8 evidence consolidation.

## Milestone Handoff

- Reviewed milestone: M2 Shell Surface Foundation.
- Review status: clean-with-notes.
- Milestone closeout: M2 may close.
- Required review-resolution: none.
- Remaining in-scope implementation milestones: M3-M8.
- Next stage: `implement` M3 File-List Polish and Deterministic Fixture Icons.
- Final closeout readiness: not ready; M3-M8 remain open.
