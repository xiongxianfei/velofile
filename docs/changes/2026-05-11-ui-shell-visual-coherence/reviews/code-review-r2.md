# Code Review R2

## Status

changes-requested

## Reviewed Milestone

M1. Shell Contract and Validator Extension

## Review Inputs

- Commit: `4053721 M1: extend shell UI contract validation`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`
- Implementation files:
  - `tools/VeloFile.UiContracts/Program.cs`
  - `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`
  - `docs/ui/ui-contract-scopes.v1.json`
- Validation evidence recorded in the plan and change-local `explain-change.md`

## Diff Summary

M1 adds planned shell visual-coherence scopes, a behavior-preservation matrix inventory, corpus tests, and validator support for fixture icon invariant scanning and visual sidecar validation.

## Findings

### CR-001: Governed fixture icon validation does not reject local icon color literals

- Severity: major
- Location: `tools/VeloFile.UiContracts/Program.cs`, `FindForbiddenFixtureIconUsage`; `tests/VeloFile.Corpus.Tests/UiContracts/ShellVisualCoherenceContractTests.cs`, `Ui_contract_tool_rejects_forbidden_fixture_icon_resources`

Evidence:

- Spec AC7 requires static validation to fail if governed fixture/file-list icon resources use unapproved icon colors.
- Spec R42 requires governed fixture/file-list icon foreground, background, and size values to resolve from VeloFile resources rather than unapproved local literals.
- Test spec TSC009 explicitly covers invalid icon fixtures for unapproved icon colors and unapproved icon sizes.
- The implementation scans governed icon files for `SymbolIcon`, `PathIcon`, private-use glyphs, ellipsized text chips, and local numeric `Width`/`Height`, but it does not scan icon resources for local color literals such as `Foreground="#FFFFFF"`, `Fill="#FFFFFF"`, `Stroke="#FFFFFF"`, or `Color="#FFFFFF"`.
- The added test only creates `<SymbolIcon>` and `Text="P..."`; it does not include an invalid local icon color case, so the missing requirement is not directly proved.

Required outcome:

- Governed fixture icon validation must reject unapproved local icon color literals in `Resources/Icons` scopes.
- The M1 tests must include a direct failing/pass proof for at least one local icon color literal in a governed fixture icon resource.

Safe resolution path:

- Add an invalid icon fixture case to `ShellVisualCoherenceContractTests` using a governed icon resource with a local color literal such as `Fill="#FFFFFF"` or `Foreground="#FFFFFF"`.
- Extend `FindForbiddenFixtureIconUsage` to reject local icon color literals for relevant icon attributes such as `Fill`, `Stroke`, `Foreground`, `Background`, and `Color` when the value is a raw color rather than a resource reference.
- Rerun the M1 targeted validation commands and return M1 to `review-requested` for code-review rerun.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | CR-001 misses R42/AC7 local icon color enforcement. |
| Test coverage | block | CR-001 lacks direct proof for TSC009 unapproved icon colors. |
| Edge cases | block | EC8 covers governed icon resources hardcoding local foreground/background colors. |
| Error handling | pass | CLI failure aggregation and nonzero exit behavior are present for implemented checks. |
| Architecture boundaries | pass | Changes remain in UI contract tooling, docs, and corpus tests; no Core/Windows behavior changes. |
| Compatibility | pass | Planned scopes are not activated by runtime shell code and no migration is introduced. |
| Security/privacy | pass | Sidecar privacy checks were added and directly tested for raw local path patterns. |
| Derived artifact currency | pass | Plan and change-local records cite M1 validation evidence. |
| Unrelated changes | concern | The commit includes lifecycle artifacts plus M1 implementation; this follows the active workflow but should remain scoped in review-resolution. |
| Validation evidence | concern | Validation is credible for implemented checks but does not prove the missing local icon color case. |

## Required Next Stage

`review-resolution` for CR-001, then rerun M1 `code-review`.
