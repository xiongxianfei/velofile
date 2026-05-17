# Spec Review R1: M3 Visual-Evidence Deferral Amendment

## Review outcome

approved

## Review inputs

- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Change request: maintainer requested relaxing the M3 evidence gate before M3 code review

## Scope reviewed

The amendment adds a narrow M3-only visual-evidence deferral:

- M3 may proceed to code review without accepted `shell-file-list-selected-focused` full-shell evidence.
- M3 must not claim whole-shell visual acceptance.
- Static icon/resource validation and touched behavior-preservation validation remain required.
- M8 remains blocked until `shell-file-list-selected-focused` is captured or manually reviewed.

## Findings

No material findings.

## Review notes

| Dimension | Result | Notes |
|---|---|---|
| Requirement clarity | pass | R22A defines the only allowed deferral case and names the replacement gate. |
| Normative language | pass | The amendment uses `MAY` for the M3 exception and `MUST` for required deferral conditions and M8 evidence replacement. |
| Completeness | pass | The spec keeps the seven-state evidence set in R66 and adds failure behavior for missing deferral or unresolved M8 evidence. |
| Testability | pass | The test spec maps R22A to TSC013, TSC014, and TSC020. |
| Compatibility | pass | The amendment does not change production behavior, token contracts, or behavior-preservation requirements. |
| Acceptance criteria | pass | AC21 makes the deferral observable and prevents visual acceptance from being inferred. |

## Immediate next stage

`plan-review` for the matching plan amendment.

## Eventual test-spec readiness

ready. The matching test spec was amended to cover the deferral and M8 replacement gate.
