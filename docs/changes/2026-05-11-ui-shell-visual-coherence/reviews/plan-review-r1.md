# Plan Review R1: UI Shell Visual Coherence

## Review Status

changes-requested

## Review Inputs

- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Proposal: `docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md`
- Spec: `specs/ui-shell-visual-coherence.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`
- Project map: `docs/project-map.md`

## Findings

### PR-001: Region milestones can close without required full-shell visual evidence

- Severity: material
- Location: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`, M2-M7 region milestones starting at M2; M8 full-shell evidence milestone.
- Evidence:
  - `specs/ui-shell-visual-coherence.md` R22 requires full-shell screenshots for a region slice to show no new mismatch, or for the mismatch to be recorded as a deviation.
  - `specs/ui-shell-visual-coherence.md` R26 specifically requires shell surface foundation compatibility in the default shell screenshot.
  - `docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md` states that each region slice should produce a full-shell screenshot.
  - `docs/architecture/system/architecture.md` QS-UI-SHELL-02 frames full-shell evidence as part of reviewing a shell visual-coherence region slice.
  - `docs/plans/2026-05-11-ui-shell-visual-coherence.md` centralizes full-shell screenshot capture in M8 after M2-M7, while M2-M7 validation commands contain no screenshot or manual visual-evidence step.
- Required outcome: Each region milestone M2-M7 must include a soft full-shell visual evidence step appropriate to that slice before the milestone can close.
- Safe resolution path:
  - Revise M2-M7 validation commands and closeout criteria to include current screenshot/manual visual review evidence for the touched state/profile.
  - Keep M8 as the consolidation/baseline inventory milestone, but do not make it the first time region screenshots are produced.
  - Suggested mapping:
    - M2: `shell-default` at `shell-standard-1440x900-100`, plus `shell-min-900x560-100` if layout changed.
    - M3: `shell-file-list-selected-focused`.
    - M4: `shell-filter-active` and `shell-search-active`.
    - M5: `shell-default` or sidebar-focused shell state, with keyboard/accessibility notes.
    - M6: `shell-operation-running` and `shell-destructive-confirmation`.
    - M7: `shell-preview-open`.
  - Add a generic milestone closeout bullet: full-shell screenshot or explicit manual visual evidence recorded for touched region, with deviations recorded when needed.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | pass | A new contributor can follow source artifacts, existing UI surfaces, milestones, dependencies, and current handoff state. |
| Source alignment | concern | PR-001: region-slice visual evidence is required by spec/proposal/architecture but deferred to M8. |
| Milestone size | pass | M1-M8 are reviewable slices with bounded ownership. |
| Sequencing | concern | PR-001: screenshot evidence arrives too late for region-slice acceptance. |
| Scope discipline | pass | Non-goals and behavior-preservation boundaries are explicit. |
| Validation quality | concern | PR-001: M2-M7 lack required per-region visual evidence commands or manual evidence steps. |
| TDD readiness | pass | Tests to add/update are identified, with implementation correctly blocked on the matching test spec. |
| Risk coverage | pass | Rollback and recovery are per governed region with Core/Windows boundaries protected. |
| Architecture alignment | concern | PR-001: QS-UI-SHELL-02 expects full-shell evidence during region-slice review. |
| Operational readiness | pass | CI, generated-output, screenshot-sidecar, and manual high-DPI constraints are covered. |
| Plan maintainability | pass | Progress, handoff summary, decisions, surprises, validation notes, and outcome sections are present. |

## Required Resolution

Enter review-resolution for PR-001. Do not proceed to `test-spec` until the plan is revised and this finding is resolved or explicitly re-reviewed.

## Next Stage

`review-resolution`
