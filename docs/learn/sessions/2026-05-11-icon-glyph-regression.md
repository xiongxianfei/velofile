# Learn Session: Icon Glyph Regression

## Frame

- Date: 2026-05-11
- Trigger: explicit maintainer request after repeated failed fixes for garbled shell icons.
- Trigger type: incident / repeated fix failure / maintainer request.
- Scope: VeloFile WinUI shell icon rendering in `MainWindow.xaml`, related regression tests, and workflow records for the post-verify bugfix.
- Evidence in scope:
  - Commits `15f3564`, `1cb8299`, and `28ea5f1`.
  - `src/VeloFile.App/MainWindow.xaml`.
  - `tests/VeloFile.App.Tests/AppShellContractTests.cs`.
  - `docs/changes/2026-05-11-ui-design-system-shell-redesign/change.yaml`.
  - `docs/changes/2026-05-11-ui-design-system-shell-redesign/explain-change.md`.
  - `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`.
- Explicit exclusions:
  - No new workflow policy is made by this session.
  - No topic guidance is updated without contributor confirmation.
  - No claim is made that the branch is verified or PR-ready.
- Prior learnings reviewed: no existing `docs/learn` records were present.

## Observe

### O1. The initial fix was symptom-scoped, not defect-class scoped

Evidence:

- `15f3564 Fix tab switch icon rendering` replaced only Previous tab and Next tab `SymbolIcon` usage with `PathIcon`.
- The user reported the problem still existed.
- `1cb8299 Use raw vector tab switch icons` replaced only the same two buttons with raw vector paths.
- The user then clarified the problem still existed outside those two icons.
- `28ea5f1 Replace shell SymbolIcons with raw vectors` finally removed all `SymbolIcon` and `PathIcon` controls from `MainWindow.xaml`.

Observation:

The visible symptom named two icons, but the root defect class was broader: shell icon buttons depended on icon glyph/control rendering. The first fixes did not search and remove every instance of that defect class.

### O2. The regression test initially encoded the narrow fix, not the root invariant

Evidence:

- The first regression test checked only Previous tab and Next tab.
- The second regression test still focused only those two buttons, although it rejected `PathIcon`.
- The final regression test `Main_window_icon_buttons_use_raw_vectors` rejects `<SymbolIcon` and `<PathIcon` globally in `MainWindow.xaml` and checks all shell icon buttons listed in the test.

Observation:

The effective test was the one that captured the invariant: shell icon buttons must not depend on icon font or icon-control rendering. Tests that only covered the originally reported two buttons allowed the same defect class to remain elsewhere.

### O3. Static tests caught implementation shape, but there was no visual/manual proof before claiming the UI symptom was fixed

Evidence:

- The validation evidence for the bugfix was static XAML tests plus build.
- No screenshot or running-app visual check was recorded for the bugfix.
- The user still observed the problem after earlier fixes.

Observation:

For visual rendering bugs, static XAML checks are useful but insufficient by themselves. A fix should include either a running UI screenshot/manual confirmation or an explicit statement that the visual symptom was not directly observed after the change.

## Root Cause

Technical root cause:

The shell used `SymbolIcon` for toolbar icon buttons. `SymbolIcon` renders private-use glyphs from an icon font. When that icon font or icon-control rendering path does not resolve correctly in the target environment, users can see garbled text instead of icons.

Process root cause:

The first fixes treated the report as a two-control defect instead of a defect class. They did not perform a complete blast-radius scan for all shell icon controls, and the regression tests initially enforced the local patch rather than the broader invariant.

## Best Practices

1. Diagnose the defect class, not only the named controls.
   - If two icons are garbled because of glyph rendering, search for all glyph/icon-control usage in the same UI surface before fixing.

2. Add a regression test for the invariant.
   - Better: `MainWindow.xaml contains no SymbolIcon or PathIcon for shell icon buttons`.
   - Weaker: `Previous tab and Next tab do not use SymbolIcon`.

3. Verify blast radius with a simple source scan.
   - For this incident: `rg -n "<SymbolIcon|<PathIcon" src\VeloFile.App\MainWindow.xaml`.

4. Prefer deterministic vectors for critical shell controls when glyph rendering is unreliable.
   - Raw XAML `Path` inside `Viewbox` avoids icon-font private-use glyph lookup and icon-control rendering.
   - Keep automation names and tooltips on the button, not inside the visual shape.

5. For visual bugs, pair static proof with visual proof when possible.
   - Static tests can prove the risky API is gone.
   - A screenshot or manual running-app check proves the user-visible symptom is gone.

6. After a post-verify code change, do not preserve prior branch-ready claims.
   - The active plan was correctly moved back to code-review / re-verify after the bugfix.

## Classify

| ID | Observation | Proposed classification | Final classification | Secondary routes | Confirmed by | Rationale |
|---|---|---|---|---|---|---|
| O1 | Initial fixes were symptom-scoped instead of defect-class scoped. | durable-lesson | candidate durable-lesson | Possible topic entry after confirmation. | pending maintainer confirmation | Repeated failed fixes show a reusable pattern. |
| O2 | Tests initially encoded the narrow fix instead of the root invariant. | durable-lesson | candidate durable-lesson | Possible topic entry after confirmation. | pending maintainer confirmation | Regression test scope directly affected recurrence. |
| O3 | Visual bug was not backed by direct visual proof before user confirmation. | process-follow-up | candidate process-follow-up | Consider plan/test-spec follow-up for visual bug verification expectations. | pending maintainer confirmation | Useful process improvement, but requires owner decision before becoming policy. |

## Route

No topic file or authoritative artifact was updated in this learn session because contributor confirmation for final classification/routing is pending.

Recommended follow-ups for maintainer confirmation:

- Add a curated topic entry under `docs/learn/topics/ui-visual-regressions.md` covering defect-class blast radius and invariant tests.
- Consider a proposal or workflow/spec update requiring direct visual evidence for future UI rendering bug fixes when feasible.
- Keep the current implementation handoff at `code-review` for the post-verify icon bugfix, then rerun final verification after review.

## Session Outcome

- Lessons captured in this session record: 3 candidate lessons.
- Durable topic updates: none pending confirmation.
- Follow-ups created: none pending confirmation.
- No-learn rationale: not applicable; evidence supports reusable candidate lessons, but routing requires confirmation.
