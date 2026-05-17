# Spec Review R1: Test Runtime Optimization

## Review Status

changes-requested

## Review Inputs

- Spec: `specs/test-runtime-optimization.md`
- Proposal: `docs/proposals/2026-05-16-test-runtime-optimization.md`
- Learn session: `docs/learn/sessions/2026-05-16-test-runtime-optimization.md`
- Constitution: `CONSTITUTION.md`
- Project map: `docs/project-map.md`

## Findings

### TRO-SR1: Prepared-tool staleness is a required failure mode but is undefined

- Severity: material
- Location: `specs/test-runtime-optimization.md` R37; Error and boundary behavior prepared-tool stale-tool bullet.
- Evidence:
  - R37 says prepared-tool execution must fail clearly when the prepared tool path is "missing, stale, or outside the allowed scratch/temp root."
  - The error and boundary section repeats that prepared-tool execution with a "missing or stale tool" must fail with an actionable diagnostic.
  - The spec defines prepared-tool execution as a path that builds or publishes the corpus tool once for repeated test invocations, but it does not define what makes such a tool stale.
  - Acceptance criterion AC8 only requires rejecting paths outside allowed scratch/temp roots, so it does not close the staleness requirement.
- Required outcome: Define a testable stale-tool contract or remove `stale` from the first-slice `MUST` behavior.
- Safe resolution path:
  - Preferred: define staleness as a concrete observable condition, for example a missing expected tool artifact, missing current-run marker, source hash mismatch, or build marker older than the current prepared-tool setup invocation.
  - Add matching acceptance criteria for stale-tool rejection, or explicitly defer stale/source-hash detection to a later slice and keep the first slice limited to missing-path and outside-root failures.
  - Keep the prepared-tool path internal to tests and preserve the existing no-public-script-option boundary.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Requirement clarity | concern | TRO-SR1: `stale` is a `MUST` failure mode but has no single testable meaning. |
| Normative language | concern | TRO-SR1 makes an undefined term normative. Other `MUST` and `SHOULD` usage is appropriate. |
| Completeness | pass | Normal, boundary, error, compatibility, rollback, and evidence cases are broadly covered. |
| Testability | concern | Most requirements are testable; R37 stale-tool behavior needs a concrete oracle. |
| Examples | pass | Examples match the intended category, smoke, hermetic, runtime, and release-evidence behavior. |
| Compatibility | pass | Public wrappers, full CI, rollback, and deferred CI splitting are addressed. |
| Observability | pass | Runtime reports, diagnostics, and full CI status are specified. |
| Security/privacy | pass | Scratch roots, secrets, user PATH, and path exposure are covered. |
| Non-goals | pass | Production behavior, CI splitting, public prepared-tool options, and parallelization are excluded. |
| Acceptance criteria | concern | AC8 does not include stale-tool rejection even though R37 requires it. |

## Required Resolution

Enter review-resolution for TRO-SR1. Do not approve the spec or start architecture/plan/test-spec until the prepared-tool staleness requirement is made testable or deferred.

## Immediate Next Stage

`review-resolution`

## Eventual Test-Spec Readiness

not-ready until TRO-SR1 is resolved and the spec is re-reviewed or explicitly accepted after amendment
