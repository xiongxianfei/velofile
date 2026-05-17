# Spec Review R2: Visual-Evidence Gate Removal Amendment

## Review outcome

approved

## Review inputs

- Spec: `specs/ui-shell-visual-coherence.md`
- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Proposal: `docs/proposals/2026-05-11-shell-visual-coherence-follow-up.md`
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`
- Change request: maintainer requested removing or relaxing the mandatory visual-evidence requirement before M4 code review
- Related blocker: CR-007 in `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r13.md`

## Scope reviewed

The amendment changes the shell visual-coherence proof model:

- screenshots, screenshot sidecars, and manual visual-review notes become optional supporting artifacts;
- region milestones, M8, final closeout, verification, and release readiness cannot be blocked only because screenshots/manual visual notes are absent;
- static resource contracts, behavior-preservation evidence, accessibility checks, and required design-deviation records remain hard closeout proof;
- optional screenshot/profile/sidecar artifacts remain governed for privacy, traceability, generated-output hygiene, and non-release-proof classification;
- the prior M3-only visual-evidence deferral becomes historical context only.

## Findings

No material findings.

## Review notes

| Dimension | Result | Notes |
|---|---|---|
| Requirement clarity | pass | R22, R22A, R66-R69, AC9-AC13, and AC21 clearly separate required proof from optional visual artifacts. |
| Normative language | pass | The amendment uses `MUST NOT` for closeout gates and keeps `MUST` obligations for static validation, behavior proof, accessibility, deviation records, sidecar privacy, generated-output hygiene, and behavior preservation. |
| Completeness | pass | The amendment covers missing/noisy screenshot automation, optional sidecar metadata, high-DPI/min-size proof alternatives, privacy, rollback, and prior deferral cleanup. |
| Testability | pass | The amended contract maps to TSC013 for no hidden visual gate, TSC014/TSC019 for optional sidecar/privacy rules, TSC015 for behavior-preservation matrix proof, and TSC020 for optional manual review only when used. |
| Examples | pass | E5 and the edge cases for missing optional screenshots, sidecar/profile mismatch, private-path leakage, and screenshot-only behavior proof match the amended requirements. |
| Compatibility | pass | The amendment changes review evidence policy only; it does not change V1 shell behavior, persistence, public scripts, token major versioning, or Core/Windows boundaries. |
| Observability | pass | Optional artifacts remain traceable through sidecar metadata and review IDs when recorded; static/behavior/deviation evidence remains the required observable proof. |
| Security/privacy | pass | S3-S6 and R75-R77 keep private paths, user data, generated screenshots, diffs, and baseline mutation governed even when visual artifacts are optional. |
| Non-goals | pass | The spec still excludes pixel-perfect screenshot gates, fixture-only proof of platform behavior, broad theme settings, and unrelated V1 feature expansion. |
| Acceptance criteria | pass | AC9-AC13 and AC20-AC21 make the new closeout rule observable and prevent screenshots from replacing behavior proof. |

## Non-material follow-up

The matching test spec still has stale readiness text saying it is active for M1 implementation. That should be corrected during the matching test-spec review, but it does not block approval of the feature-spec amendment because the feature spec itself is clear and testable.

## Immediate next repository stage

`architecture-review` for the visual-evidence gate removal amendment.

## Eventual test-spec readiness

conditionally-ready. The amended test spec contains the right coverage shape for TSC013, TSC014, TSC015, TSC019, and TSC020, but it still needs its own review and readiness-text cleanup before M4 code review can rely on the full amended proof model.
