# Architecture Review R1: Test Runtime Optimization

## Review Status

blocked

## Review Surface

- canonical-architecture-update
- ADR
- proposal-or-spec-gap

## Review Inputs

- Canonical architecture: `docs/architecture/system/architecture.md`
- Container diagram: `docs/architecture/system/diagrams/container.mmd`
- ADR: `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Spec: `specs/test-runtime-optimization.md`
- Proposal: `docs/proposals/2026-05-16-test-runtime-optimization.md`
- Spec review: `docs/changes/2026-05-16-test-runtime-optimization/reviews/spec-review-r1.md`
- Review resolution: `docs/changes/2026-05-16-test-runtime-optimization/review-resolution.md`
- Constitution: `CONSTITUTION.md`
- Project map: `docs/project-map.md`

## Findings

### TRO-AR1: Architecture review is downstream of an unresolved spec-review gate

- Severity: blocker
- Location:
  - `specs/test-runtime-optimization.md` status and readiness
  - `docs/changes/2026-05-16-test-runtime-optimization/reviews/spec-review-r1.md`
  - `docs/changes/2026-05-16-test-runtime-optimization/review-resolution.md`
  - `docs/architecture/system/architecture.md` lifecycle metadata and readiness
- Evidence:
  - The spec still records `Status` as `draft`.
  - `spec-review-r1.md` records `changes-requested` for `TRO-SR1` and says not to start architecture until the prepared-tool staleness requirement is made testable or deferred.
  - `review-resolution.md` records `TRO-SR1` as resolved but says the spec amendment is ready for spec re-review before downstream stages.
  - The canonical architecture already records the test runtime update as approved and ready for architecture review, creating lifecycle ambiguity before the spec has a durable approval record.
- Required outcome: Record an approving spec re-review for the amended test runtime optimization spec, or revise the spec/architecture lifecycle metadata so downstream architecture work is not treated as approved before the spec gate is closed.
- Safe resolution path:
  - Run and record `spec-review-r2` against the amended `specs/test-runtime-optimization.md`.
  - If the amended spec is approved, update the spec status to `approved`, add the review log entry, and then rerun architecture review.
  - If the amended spec needs more changes, keep architecture blocked and align the architecture/ADR only after the spec is corrected.
  - Do not proceed to `plan`, `test-spec`, or implementation until this gate is closed.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Spec alignment | block | TRO-AR1: the architecture is downstream of a spec that lacks a durable approving re-review after a material spec finding. |
| Package shape | pass | The canonical update uses the existing arc42 package, links authored diagram source, and adds ADR 0011. |
| Boundary clarity | pass | The architecture separates contributor validation tiers, corpus validation tooling, public wrappers, prepared-tool execution, and release evidence. |
| Data ownership | pass | Prepared-tool manifests, runtime reports, scratch roots, and generated outputs have clear ownership and privacy constraints. |
| Interface safety | pass | Public corpus wrapper command-line contracts remain backward compatible; prepared-tool execution stays test-internal. |
| Runtime and failure handling | pass | Prepared-tool failure modes, stale/invalid manifest handling, and release-evidence preservation are described. |
| Deployment and execution boundaries | pass | `scripts/ci.ps1`, public scripts, scratch/temp roots, generated outputs, and review evidence boundaries are covered. |
| Security/privacy | pass | The architecture forbids private paths, secrets, profile details, global PATH mutation, and repository output leaks from prepared-tool execution. |
| Quality and operations | pass | QS-TEST-RUNTIME-01 and runtime evidence requirements cover speed and validation credibility. |
| Testing feasibility | pass | Category inventory, smoke coverage, hermetic isolation, prepared-tool rejection, and runtime report checks are testable. |
| Complexity discipline | pass | CI splitting, public prepared-tool options, source-hash caching, and assembly-wide parallelization are deferred. |
| ADR quality | pass | ADR 0011 records context, decision, alternatives, consequences, and follow-up. |
| Plan readiness | block | Planning is blocked until the amended spec has an approving review record and the architecture review is rerun. |

## Required Updates

- Canonical architecture: no content change required for the design itself, but lifecycle metadata should not remain treated as approved unless the upstream spec gate is closed and architecture review passes.
- ADR: no content change required for ADR 0011 after the spec gate is closed.
- Review records: add this blocker to `review-resolution.md` and rerun architecture review after spec re-review.

## Next Stage

`spec-review`
