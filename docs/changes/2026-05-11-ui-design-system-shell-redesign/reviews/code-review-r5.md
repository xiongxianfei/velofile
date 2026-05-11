# Code Review R5

## Review status

changes-requested

## Review inputs

- Diff range: `HEAD~1..HEAD` (`fc27f82 M4: add first UI visual baselines`)
- Review surface: M4 visual baseline script, baseline inventory tests, committed visual baselines/sidecars, fixture-launch hardening, active plan/change records
- Tracked governing branch state: spec, test spec, architecture, ADR 0009, and plan are tracked in the current branch
- Spec: `specs/ui-design-system-shell-redesign.md`
- Test spec: `specs/ui-design-system-shell-redesign.test.md`
- Plan milestone: `docs/plans/2026-05-11-ui-design-system-shell-redesign.md`, M4
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md`
- Validation evidence: M4 validation notes in the active plan and `change.yaml`

## Diff summary

M4 adds generated-output ignore rules, `scripts/update-ui-baselines.ps1`, nine committed first-slice WinUI baseline PNGs with JSON sidecars, visual baseline inventory/script tests, and narrow fixture-launch reliability changes needed to capture deterministic screenshots from the guarded WinUI fixture path.

## Findings

### CR-M4-001 - Posted listing completion can apply stale results after navigation changes

- Severity: major
- Evidence: `CompleteListingAsync` checks `IsActiveListingResult(result)` before scheduling the UI update, then posts `ApplyListingState(result.State)` without rechecking the active listing request or active tab inside the posted callback (`src/VeloFile.App/ViewModels/AppShellViewModel.cs:1070`, `src/VeloFile.App/ViewModels/AppShellViewModel.cs:1073`, `src/VeloFile.App/ViewModels/AppShellViewModel.cs:1078`). In production, `WinUiShellDispatcher.Post` enqueues the callback, so the active tab/path can change after the check but before `ApplyListingState` runs. That can apply stale rows to the newly active location. The M4 validation evidence does not include a regression test for this delayed-dispatch race.
- Requirement impact: The first-slice UI work must preserve V1 navigation/listing behavior. The spec requires existing V1 behavior tests to remain valid and not be replaced by fixture-only visual evidence (C5, C6, AC15). This M4 fixture-capture support change alters the production listing completion path outside the visual-baseline storage/script scope.
- Required outcome: Listing results must still be validated against the current active listing request/tab at the point they mutate visible rows on the UI dispatcher. A stale completed listing must not be able to replace rows after the user navigates, switches tabs, closes tabs, or refreshes into a newer request.
- Safe resolution path: Keep the fix narrow in `AppShellViewModel` and app tests. Recheck `IsActiveListingResult(result)` inside the posted callback immediately before `ApplyListingState(result.State)`, or otherwise carry a request token that is checked on the UI dispatcher before mutation. Add a focused regression test with a queued test dispatcher proving a completed old listing does not apply after a newer navigation/request wins. Do not broaden M4 into a selection system, screenshot harness, or production UI redesign.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | CR-M4-001 violates the behavior-preservation boundary for navigation/listing while supporting visual fixture capture. |
| Test coverage | concern | Visual baseline inventory and script guardrail tests cover R69-R75, but no test covers stale listing results with queued dispatcher delivery. |
| Edge cases | concern | Missing review ID, missing current screenshots, dimensions, sidecars, and ignored outputs are covered; delayed listing completion after navigation is not. |
| Error handling | pass | Baseline script rejects missing review ID, missing current screenshots, unsupported suite, missing profile, and missing sidecars. |
| Architecture boundaries | concern | Baseline work stays local, but `AppShellViewModel` production listing semantics changed to support fixture capture and needs a stale-result guard. |
| Compatibility | concern | Existing V1 listing/navigation expectations can regress if stale results apply after tab/path changes. |
| Security/privacy | pass | Sidecar tests reject local user/workspace path metadata; fixture rows remain synthetic and no arbitrary fixture data paths are introduced. |
| Derived artifact currency | pass | The nine required baseline PNGs and JSON sidecars exist under the required profile, and current/diff outputs are ignored. |
| Unrelated changes | concern | Most changes are M4-scoped; CR-M4-001 is a necessary fixture-capture reliability change but touches production listing completion and needs a guard. |
| Validation evidence | concern | Recorded validation is relevant and broad CI passed on rerun, but it does not exercise the stale posted-listing race. |

## Recommended next stage

Enter `review-resolution` for M4. Keep the fix scoped to the listing completion dispatcher guard and a targeted regression test, then return M4 to `code-review`.
