# Code Review R8: M2 UI Optimization and Reference-Gap Plan Sharpening

## Review Status

changes-requested

## Review Inputs

- Review surface: unstaged working-tree diff.
- Reviewed milestone: M2 Shell Surface Foundation, with plan-only reference-gap amendments.
- Spec: `specs/ui-shell-visual-coherence.md`.
- Test spec: `specs/ui-shell-visual-coherence.test.md`.
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
- Architecture / ADR:
  - `docs/architecture/system/architecture.md`
  - `docs/adr/0010-shell-visual-coherence-contracts.md`
- Changed files:
  - `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
  - `src/VeloFile.App/MainWindow.xaml`
  - `src/VeloFile.App/MainWindow.xaml.cs`
  - `tests/VeloFile.App.Tests/UiDesign/ShellSurfaceResourceContractTests.cs`
  - `docs/learn/sessions/2026-05-12-ui-gap-to-hifi-analysis.md` (untracked session record)
- Validation reviewed:
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "ShellSurfaceResourceContractTests|AppShellContract|UiDesign"`: passed, 39 tests.
  - `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
  - `git diff --check`: passed with CRLF warnings only.

## Diff Summary

The diff improves the visible shell by forcing the root shell into dark theme, styling additional shell text, removing visible sidebar toggle state text from the layout, moving sidebar locations before visibility and terminal controls, styling preview/status text, adding a dark native titlebar color method, and adding a static test for navigation-first sidebar ordering. The plan is also sharpened with reference-gap handling, privacy-safe evidence language, M2 usable-foundation closeout language, M3-M7 reference-quality targets, and M8 reference-comparison consolidation. A learn session records the current gap to `hifi-design/`.

## Findings

### CR-003: Titlebar color path bypasses the governed token/resource contract

- Severity: major
- Location: `src/VeloFile.App/MainWindow.xaml.cs:71`

Evidence:

- The new `ConfigureDarkTitleBar` method assigns app/titlebar colors through raw local RGB literals at `src/VeloFile.App/MainWindow.xaml.cs:78` through `src/VeloFile.App/MainWindow.xaml.cs:89`.
- Spec R12 requires governed shell regions to use VeloFile-owned token/resource references for colors when tokenized.
- Spec R24 requires shell surface foundation to define and apply a coherent surface family for app root, chrome, sidebar, content, borders, separators, text hierarchy, accent/focus, selection, hover, disabled, danger, warning, and success states.
- Plan M2 scopes shell foundation resources to app root, chrome, sidebar, content, command band, status area, preview/details, separators, and text hierarchy; the new titlebar method changes chrome color outside those resources.
- The current tests check XAML resources and `MainWindow.xaml`, but do not fail on raw chrome colors in `MainWindow.xaml.cs`.

Required outcome:

- Native titlebar colors must be derived from VeloFile-owned token/resource values or another governed resource bridge, not hard-coded local RGB values in app code.
- Add focused validation that prevents governed shell chrome/titlebar color literals from reappearing outside token dictionaries or an accepted bridge.

Safe resolution path:

- Move titlebar color values into governed tokens/resources and read/convert those resources when configuring `AppWindow.TitleBar`, or create a narrow titlebar resource bridge that is explicitly checked by tests.
- Add a focused test that rejects raw `Color.FromArgb(...)` titlebar literals in `MainWindow.xaml.cs` unless the values come from an approved token/resource bridge.
- Rerun the M2 focused UI contract tests and `tools/VeloFile.UiContracts` validation.

### CR-004: Sidebar reordering enters M5 behavior scope without M5 evidence

- Severity: major
- Location: `src/VeloFile.App/MainWindow.xaml:296`

Evidence:

- The diff moves `SidebarLocationsList` before `VisibilityControls` and `TerminalTargetComboBox` in `MainWindow.xaml`.
- Spec R21 requires navigation-first sidebar hierarchy, but spec R51-R53 and AC14 require preserved access, keyboard reachability, accessible names, and discoverability for locations, favorites, recent entries, drives, visibility toggles, and terminal controls.
- ADR 0010 explicitly rejects treating sidebar reordering as cosmetic because it affects keyboard order, accessible names, discoverability, and user workflow.
- The active plan assigns sidebar reordering and grouping to M5, including keyboard order and accessible-name evidence. M2 says to keep layout structure and command/event bindings intact unless a spec requirement explicitly allows region-level reorganization later.
- The new test `Sidebar_presents_navigation_before_secondary_visibility_controls` checks source order and names, but it does not prove keyboard traversal, discoverability for all named sidebar groups, or the M5 behavior-preservation surface.

Required outcome:

- Either keep this M2 optimization strictly to surface/readability styling and defer sidebar reordering to M5, or explicitly reclassify this change as touching M5 sidebar behavior and provide the required M5-level evidence before accepting it.

Safe resolution path:

- Preferred: revert only the sidebar reordering portion from this M2 diff while retaining M2 readability fixes such as hidden toggle content, text styles, and dark surface integration.
- Alternative: formally treat this as M5-scoped implementation, update the plan/handoff accordingly, and add/record keyboard order, accessible-name, discoverability, and behavior-preservation evidence required by the spec/test spec before re-review.
- In either path, rerun focused sidebar/app-shell tests and ensure the plan handoff summary accurately names the touched milestone.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | M2 readability and reference-gap plan changes align with R4, R16-R27, and the active plan, but raw titlebar colors conflict with token/resource governance and sidebar reordering crosses into M5 behavior scope. |
| Test coverage | concern | Focused tests and UI contract validation pass, but no test covers raw titlebar color literals in code, and the sidebar test does not prove keyboard/discoverability behavior required for sidebar reordering. |
| Edge cases | concern | Toggle visible state text and placeholder icon issues are addressed by existing static tests, but titlebar token drift and sidebar keyboard traversal are not directly proven. |
| Error handling | pass | Titlebar color configuration catches platform failures without blocking startup. |
| Architecture boundaries | concern | No Core/Windows service boundary is changed, but tokenized UI design-resource authority is bypassed for titlebar colors. |
| Compatibility | concern | Sidebar reordering can affect observable keyboard/workflow order and needs M5-level evidence or deferral. |
| Security/privacy | pass | Plan amendments correctly state that diagnostic screenshots with private paths are not accepted evidence; generated screenshots remain uncommitted. |
| Derived artifact currency | pass | No generated token/resource artifacts are expected from this diff. |
| Unrelated changes | concern | Plan sharpening and learn record are related to the user's latest request, but sidebar M5 behavior work is mixed into an M2 optimization diff. |
| Validation evidence | concern | Focused static validation passed, but the named findings are proof gaps not covered by those checks. M2 accepted visual evidence remains unresolved under CR-002. |

## Recommended Next Stage

- Keep M2 in `resolution-needed`.
- Enter review-resolution for CR-003 and CR-004 after this record is accepted.
- Do not proceed to M3 or final closeout while CR-002 remains unresolved and these new findings are open.

## Handoff Summary

- Reviewed milestone: M2 Shell Surface Foundation, plus plan/reference-gap documentation.
- Review status: changes-requested.
- Milestone state after review: resolution-needed.
- Required review-resolution: CR-002 remains open; CR-003 and CR-004 are newly open.
- Remaining in-scope implementation milestones: M2-M8.
- Next stage: isolated review stops here; if continuing workflow, enter review-resolution for CR-003 and CR-004, while CR-002 still blocks M2 closeout.
- Final closeout readiness: not ready because M2 is still unresolved and M3-M8 remain open.
