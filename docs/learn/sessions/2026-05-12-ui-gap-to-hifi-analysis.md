# Learn Session: UI Gap to Hi-Fi Analysis

## Frame

- Date: 2026-05-12
- Trigger: explicit maintainer request to document the remaining gap between the current WinUI shell UI and `hifi-design/`, and analyze how to improve it.
- Trigger type: maintainer request / contributor observation / product-quality retrospective.
- Scope: current WinUI shell visual state after the icon/readability/sidebar improvements, local diagnostic screenshots, and `hifi-design/` reference artifacts.
- Evidence in scope:
  - Current uncommitted WinUI shell optimization diff in `src/VeloFile.App/MainWindow.xaml`, `src/VeloFile.App/MainWindow.xaml.cs`, and `tests/VeloFile.App.Tests/UiDesign/ShellSurfaceResourceContractTests.cs`.
  - Diagnostic screenshot `tests/visual/current/winui/diagnostic-ui-optimization/shell-default-x86-diagnostic-pass4.png`.
  - `hifi-design/tokens.json`.
  - `hifi-design/styles.css`.
  - `hifi-design/handoff/Engineering Guide.md`.
  - `docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md`.
  - `specs/ui-shell-visual-coherence.md`.
  - `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
- Explicit exclusions:
  - This session does not approve or close M2.
  - This session does not claim accepted `shell-standard-1440x900-100` visual evidence.
  - This session does not change the feature spec, architecture, or active plan by itself.
  - This session does not make `hifi-design/` authoritative over production WinUI.
- Prior learnings reviewed:
  - `docs/learn/sessions/2026-05-11-icon-glyph-regression.md`.
  - `docs/learn/sessions/2026-05-11-plan-review-recording-lapse.md`.
- Session record path: `docs/learn/sessions/2026-05-12-ui-gap-to-hifi-analysis.md`

## Observe

### O1. The current UI improved from the failed evidence state, but remains less composed than `hifi-design`

Evidence:

- The current diagnostic screenshot shows dark shell surfaces, readable sidebar labels, navigation-first sidebar ordering, no visible localized toggle On/Off text, and vector file icons instead of placeholder text chips.
- The same screenshot still shows a loose command/path/search area, chunky default-like control treatment, sparse file-list metadata treatment, and limited sidebar item anatomy.
- `hifi-design/styles.css` defines a more complete shell anatomy: sidebar sections and items, toolbar, search cluster, breadcrumb row, file-list header/body rows, preview pane, metadata rows, and action surfaces.

Observation:

The major blocker moved from "broken visual evidence" to "incomplete product-level composition." The shell is now more coherent, but it still reads as styled WinUI controls rather than a mature file-manager interface.

### O2. The largest remaining gap is component anatomy, not color choice

Evidence:

- `hifi-design/tokens.json` defines surface, border, foreground, accent, typography, spacing, radius, size, density, and motion roles.
- `hifi-design/styles.css` uses those tokens in concrete component anatomy: 44px toolbar, 36px breadcrumb row, 240px sidebar, compact toolbar buttons, section headers, selected sidebar rail, search input cluster, file-list columns, selected-row rail, metadata text, and preview sections.
- The current WinUI shell has shell tokens and broad styles, but many production surfaces still depend on direct XAML layout and platform control anatomy.

Observation:

Color and icon fixes are necessary but insufficient. The remaining quality gap is that VeloFile needs named WinUI component resources for command band, breadcrumb, sidebar item, file-list header/row anatomy, preview metadata, and status/operation surfaces.

### O3. The command/path/search band is the most visible next weakness

Evidence:

- The diagnostic screenshot shows navigation buttons, breadcrumb buttons, raw path input, filter input, search input, and action buttons as visually adjacent controls without a strong band hierarchy.
- `hifi-design/styles.css` separates toolbar and breadcrumb responsibilities and gives the search cluster, filter input, breadcrumb segment, and toolbar buttons distinct but related styles.

Observation:

After the sidebar/icon/readability fixes, the command/path/search area now carries the strongest "test harness" signal. It should be the next product-quality target unless file-list row polish is intentionally prioritized first.

### O4. The file list still lacks the reference-level details-view rhythm

Evidence:

- The current file list now avoids placeholder text chips and uses vector icons.
- `hifi-design/styles.css` defines a file-list header, column grid, selected-row accent rail, metadata columns, hidden-row opacity, mono metadata, filter-hit highlight, and controlled row height.
- The current shell screenshot still lacks the full details-view information hierarchy seen in the reference.

Observation:

The icon issue is fixed, but the file list still needs details-view polish: header, columns, metadata rhythm, selection rail, focus state, hidden/protected treatment, and deterministic empty/selected states.

### O5. Sidebar ordering improved, but sidebar component quality is still shallow

Evidence:

- The current diff moves locations before visibility and terminal controls.
- `hifi-design/styles.css` defines sidebar section headers, item hover/selected states, selected accent rail, item icons, badges, and drive meter treatment.

Observation:

Navigation-first ordering fixes information architecture, but the sidebar still needs a real item component system: section grouping, active item state, icon treatment, drive/favorite/recents anatomy, and compact secondary controls.

### O6. The improvement path should preserve the VeloFile-owned source of truth

Evidence:

- The follow-on shell visual-coherence proposal and spec explicitly treat `hifi-design/` as reference input, not an implementation contract.
- The active plan already sequences shell surface foundation, file-list polish/icons, navigation/path/search band, sidebar, status/operation surfaces, preview/details, and evidence consolidation.

Observation:

The right action is not to port CSS/JSX. The right action is to translate the reference's principles into VeloFile-owned WinUI tokens, resource dictionaries, component styles, fixtures, and screenshot/manual evidence.

## Improvement Analysis

### Gap Summary

| Area | Current state | Hi-fi reference strength | Improvement target |
|---|---|---|---|
| Shell surface | Dark, readable, more coherent after recent fixes. | Layered app/titlebar/sidebar/content/elevated surfaces. | Keep tokenized surfaces and remove remaining raw/default-looking regions. |
| Command/path/search | Functional but visually loose. | Separate toolbar and breadcrumb/search clusters with fixed heights and compact controls. | Build named command-band and breadcrumb styles before deeper region work. |
| File list | Icons fixed; rows usable. | Details-view header, columns, metadata, selection rail, hidden states. | Add file-list header/column rhythm and complete row-state anatomy. |
| Sidebar | Navigation now appears before settings. | Sections, selected rail, icons, badges, drive meters, compact items. | Create sidebar item/section resources and drive/favorite/recents grouping. |
| Preview/details | Present but simple. | Structured thumb, metadata rows, sections, actions, empty states. | Treat preview as a designed pane with state-specific layouts. |
| Status/operations | Present as functional surfaces. | Operation/action hierarchy and safety states. | Design progress, conflict, failure, and destructive-confirmation components. |
| Typography | Broad shell text styles. | UI vs mono roles, small metadata sizes, section-header tracking. | Add role-specific WinUI text resources for path/data/metadata/section/status text. |
| Evidence | Diagnostic screenshots exist but not accepted M2 evidence. | Reference expects whole-shell and component-level visual review. | Keep diagnostics separate from acceptance evidence; capture region screenshots when profiles are valid. |

### Recommended Improvement Order

1. Command/path/search band polish.
   - Define compact icon button, breadcrumb segment, path input, filter input, search action, disabled button, and status text styles.
   - Preserve existing navigation, raw path, filter, search, cancel, and clear behavior.

2. File-list details-view polish.
   - Add a details header, stable column rhythm, selected-row accent rail, focus state, hidden/protected treatment, and metadata typography.
   - Keep deterministic vector fixture icons and avoid text-chip regression.

3. Sidebar component system.
   - Add section header, sidebar item, selected state, hover state, active rail, icon, badge, and drive item anatomy.
   - Keep visibility and terminal controls discoverable but secondary.

4. Preview/details pane.
   - Add structured empty/loading/unsupported/success states, thumbnail/code/text treatment, metadata rows, and action buttons.

5. Status/operation surfaces.
   - Design running operation, cancellation, conflict, failure, and destructive confirmation as explicit safety states.

6. Full-shell evidence pass.
   - Re-capture current full-shell diagnostics and formal evidence only in valid review profiles.
   - Record accepted deviations separately from blocking defects.

### Best-Practice Rules for Future UI Work

- Improve by region, but evaluate the whole shell after each region.
- Tokenize dimensions and state rules before styling individual controls.
- Prefer named WinUI resource dictionaries and component styles over local XAML literals.
- Keep `hifi-design/` as a benchmark and anatomy reference, not a source to copy.
- Preserve behavior routes while changing visuals.
- Pair static resource checks with running-app visual diagnostics whenever feasible.
- Do not accept a screenshot that proves the shell still looks incomplete as closeout evidence.

## Classify

| ID | Observation | Proposed classification | Final classification | Secondary routes | Confirmed by | Rationale |
|---|---|---|---|---|---|---|
| O1 | UI improved but remains less composed than reference. | observation | observation | Keep in session record. | maintainer request | Evidence is useful context, but not a policy or contract change. |
| O2 | Remaining gap is component anatomy, not color choice. | direction | direction | Use as input to future plan/spec slices. | maintainer request | This is an implementation direction for already-planned UI work, not a durable topic rule. |
| O3 | Command/path/search band is the most visible next weakness. | process-follow-up | process-follow-up | Feed into the next implementation slice. | maintainer request | This identifies likely next work but should be owned by the active plan/spec, not learn topics. |
| O4 | File list still lacks reference-level details-view rhythm. | process-follow-up | process-follow-up | Feed into file-list polish slice. | maintainer request | The active plan already owns file-list polish; this session refines the gap analysis. |
| O5 | Sidebar ordering improved but component quality remains shallow. | process-follow-up | process-follow-up | Feed into sidebar redesign slice. | maintainer request | Existing plan owns sidebar redesign; this captures evidence and direction. |
| O6 | Improvements should remain VeloFile-owned, not CSS/JSX ports. | observation | observation | Keep in session record; already covered by proposal/spec. | maintainer request | Existing authoritative artifacts already state this rule. |

## Route

- Session record created: `docs/learn/sessions/2026-05-12-ui-gap-to-hifi-analysis.md`.
- Topic files updated: none.
- Authoritative artifacts updated: none.
- Follow-ups created: none in this session.

Routing rationale:

- The active proposal/spec/plan already own the shell visual-coherence direction.
- This learn session records a concrete gap analysis and recommended improvement sequence for future agents.
- No new topic entry is added because the main principles are already present in the shell visual-coherence proposal/spec/plan, and this session is primarily product-direction analysis rather than a new durable workflow lesson.

## Session Outcome

- Lessons captured in this session record: no new durable topic lesson.
- Durable topic updates: none.
- Follow-ups created: none.
- No-learn rationale: not applicable; useful observations and directions were captured, but no new durable topic guidance was routed.
