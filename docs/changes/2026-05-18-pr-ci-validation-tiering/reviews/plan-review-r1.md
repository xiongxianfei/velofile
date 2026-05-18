# Plan Review R1: PR CI Validation Tiering

## Review Status

changes-requested

## Review Inputs

- Plan: `docs/plans/2026-05-18-pr-ci-validation-tiering.md`
- Plan index: `docs/plan.md`
- Change metadata: `docs/changes/2026-05-18-pr-ci-validation-tiering/change.yaml`
- Proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- Spec: `specs/pr-ci-validation-tiering.md`
- Spec review: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/spec-review-r2.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Architecture review: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/architecture-review-r1.md`
- Project map: `docs/project-map.md`
- `AGENTS.md`
- `CONSTITUTION.md`

## Material Findings

### PRCI-PLR1: Fast-lane plan omits the required `dotnet --info` command from M2 detail

- Severity: material
- Location: `docs/plans/2026-05-18-pr-ci-validation-tiering.md`, M2 tests/implementation/validation bullets; `specs/pr-ci-validation-tiering.md` R14.
- Evidence: The approved spec requires `ci-fast-required` to run `dotnet --info` in R14. M2 claims R14-R27 coverage, but its workflow-contract test bullets list trigger, runner/shell/SDK, restore/build ordering, UI contract validation, product tests, Corpus filters, and release-evidence exclusions without mentioning `dotnet --info`. Its implementation step lists restore/build and validation commands, and its local validation command list starts with restore/build but omits `dotnet --info`.
- Required outcome: The plan must explicitly require `ci-fast-required` to run `dotnet --info`, and must require workflow contract validation or milestone validation that proves the command is present in the fast lane.
- Safe resolution path: Amend M2 to add a workflow-contract test bullet for `dotnet --info`, update the implementation step so the fast lane runs `dotnet --info` before restore/build, and add either a focused workflow-contract validation command or direct local command evidence for `dotnet --info`. Then update review-resolution and rerun plan-review.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | pass | The plan describes current workflow state, existing test surfaces, planned files, and lifecycle gates. |
| Source alignment | concern | The milestone sequence follows the spec and architecture, but M2 omits explicit R14 `dotnet --info` coverage while claiming R14-R27. |
| Milestone size | pass | M1-M5 are coherent review slices: reporting, fast shadow lane, release evidence, closeout, and handoff evidence. |
| Sequencing | pass | Reporting and parser/model work come before workflow lanes; shadow evidence precedes branch-protection handoff. |
| Scope discipline | pass | Production behavior, release-evidence deletion, `scripts/ci.ps1` narrowing, prepared-tool public options, caching-as-correctness, and visual hard gates remain guarded. |
| Validation quality | concern | Validation is concrete overall, but M2 is missing the required `dotnet --info` proof. |
| TDD readiness | concern | Workflow contract tests are identified, but the M2 test list must include R14 before the test spec can map every fast-lane MUST. |
| Risk coverage | pass | Rollback, shadow-run, external branch protection, summary limitations, workflow drift, and cache risks are covered. |
| Architecture alignment | pass | The plan follows ADR 0012: separate workflows, shared PowerShell summary helper, structured YAML workflow tests, and Windows/pwsh/SDK contract. |
| Operational readiness | pass | Observability, summaries, TRX artifacts, release-evidence triggers, closeout, and maintainer handoff evidence are planned. |
| Plan maintainability | pass | The plan has progress, decision log, surprises, validation notes, handoff summary, and lifecycle closeout structure. |

## Missing Milestones Or Dependencies

No missing milestone class was found. The plan correctly blocks implementation on plan-review and the matching test spec, and it keeps branch-protection changes external until maintainer evidence exists.

One dependency detail should be tightened while fixing PRCI-PLR1: M3 says release branch/tag patterns may be selected by plan-review or test spec. Since plan-review does not choose implementation values, the revised plan should either record default patterns directly or state that M3 implementation must record the chosen patterns in the change evidence before the workflow can be considered closed.

## Exact Suggested Edits

- In M2 tests to add/update, add: `ci-fast-required runs dotnet --info before restore/build validation.`
- In M2 implementation steps, change the fast-lane command description to include `dotnet --info`, then restore/build, then validation/test commands.
- In M2 validation commands, add `dotnet --info` or make the workflow-contract test command explicitly prove the `dotnet --info` step.
- In M3 dependencies or implementation steps, replace "selected by plan-review or test spec" with a deterministic implementation-decision note for documented release branch/tag patterns.

## Verdict

revise

The plan is directionally sound and close to ready, but PRCI-PLR1 must be fixed and re-reviewed before handoff to `test-spec`.

## Immediate Next Stage

`review-resolution` for PRCI-PLR1, then rerun `plan-review`.

`test-spec` is blocked until the plan revision is approved. Implementation remains blocked after that until the matching test spec is created and reviewed.
