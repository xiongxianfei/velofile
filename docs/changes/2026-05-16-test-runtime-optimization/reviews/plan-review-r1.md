# Plan Review R1: Test Runtime Optimization

## Review Status

revise

## Review Inputs

- Plan: `docs/plans/2026-05-16-test-runtime-optimization.md`
- Plan index: `docs/plan.md`
- Spec: `specs/test-runtime-optimization.md`
- Proposal: `docs/proposals/2026-05-16-test-runtime-optimization.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Spec reviews: `docs/changes/2026-05-16-test-runtime-optimization/reviews/spec-review-r1.md`, `docs/changes/2026-05-16-test-runtime-optimization/reviews/spec-review-r2.md`
- Architecture reviews: `docs/changes/2026-05-16-test-runtime-optimization/reviews/architecture-review-r1.md`, `docs/changes/2026-05-16-test-runtime-optimization/reviews/architecture-review-r2.md`
- Constitution: `CONSTITUTION.md`
- Project map: `docs/project-map.md`

## Findings

### TRO-PL1: Milestone validation can pass against stale builds or invalid filters

- Severity: material
- Review dimension: validation quality
- Location:
  - `docs/plans/2026-05-16-test-runtime-optimization.md:90-92`
  - `docs/plans/2026-05-16-test-runtime-optimization.md:133-135`
  - `docs/plans/2026-05-16-test-runtime-optimization.md:223-225`
  - `docs/plans/2026-05-16-test-runtime-optimization.md:264-267`
  - `docs/plans/2026-05-16-test-runtime-optimization.md:307-312`
- Evidence:
  - M1, M2, M5, M6 include `--no-build` validation commands even though those milestones add or change test code, category metadata, and documentation. Running those commands before a fresh build can validate stale assemblies rather than the just-edited test project.
  - Spec R21 allows `--no-build` commands only with the prebuilt assumption documented; it does not make `--no-build` sufficient as a milestone closeout command after code/test edits.
  - M4 uses `--filter "PreparedTool|TestCategory=Contract"`. `PreparedTool` is not part of the accepted category taxonomy in spec R8, and the command does not identify it as a `FullyQualifiedName` or `Name` filter. This makes the planned validation command ambiguous and potentially invalid.
- Required outcome: Milestone validation must include a build-producing command for every milestone that changes code or tests, and all test filters must use valid, explicit MSTest/VSTest filter syntax without introducing non-taxonomy categories.
- Safe resolution path:
  - For M1-M6, add at least one `dotnet test ... -c Debug --filter ...` command without `--no-build` after code/test changes and before any timing-focused `--no-build` measurement.
  - Keep `--no-build` commands only for documented inner-loop or runtime measurement evidence after the build-producing command has run.
  - Replace M4's ambiguous filter with an accepted category filter or an explicit non-category filter such as `FullyQualifiedName~PreparedTool` if test names/classes use that term.
  - Do not add `PreparedTool` as a new category unless the spec taxonomy is revised.

### TRO-PL2: M2 can close with a public wrapper coverage gap before M3 replaces it

- Severity: material
- Review dimension: sequencing
- Location:
  - `docs/plans/2026-05-16-test-runtime-optimization.md:112-135`
  - `docs/plans/2026-05-16-test-runtime-optimization.md:154-177`
  - `specs/test-runtime-optimization.md:155`
  - `specs/test-runtime-optimization.md:163-175`
- Evidence:
  - Spec R24 says moving a check from script-wrapper execution to in-process or prepared-tool execution must preserve the same observable claim or replace it with separate smoke coverage for the public wrapper.
  - Spec R27-R33 require hermetic wrapper isolation, minimal script smoke for public script families, and prohibit hiding public wrapper failures by replacing all script coverage with in-process coverage.
  - The plan places the corpus contract split in M2 and the replacement public script smoke/hermetic coverage in M3.
  - M2 implementation says to "split current broad script-wrapper tests by claim" and introduce non-wrapper command logic, but its closeout does not explicitly require existing public wrapper coverage to remain until M3's smoke coverage exists.
- Required outcome: The plan must prevent any milestone from closing after wrapper coverage is removed but before replacement script smoke/hermetic coverage is in place.
- Safe resolution path:
  - Preferred: revise M2 so it only adds in-process/low-overhead contract tests and does not remove or shrink existing public wrapper coverage; M3 then replaces or reduces wrapper coverage after the minimal smoke and hermetic isolation tests exist.
  - Alternative: combine M2 and M3 into one milestone so contract migration and replacement script smoke coverage are reviewed together.
  - Add a closeout bullet to the relevant milestone: no public wrapper coverage may be removed unless replacement `CorpusScript` + `Smoke` and hermetic wrapper evidence is already present in the same milestone.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | pass | The plan names current slow paths, source files, boundaries, and the active contract chain. |
| Source alignment | pass | Milestones trace to spec R1-R60, ADR 0011, and architecture constraints. |
| Milestone size | concern | M2 and M3 are individually reasonable, but their split needs a no-gap rule. |
| Sequencing | concern | TRO-PL2: contract migration can precede replacement wrapper smoke coverage. |
| Scope discipline | pass | CI splitting, public prepared-tool options, source-hash caching, production changes, and parallelization are guarded. |
| Validation quality | concern | TRO-PL1: some closeout commands can validate stale builds or use ambiguous filters. |
| TDD readiness | pass | Tests to add/update are named per milestone, pending the test spec. |
| Risk coverage | pass | Rollback, recovery, runtime variability, wrapper confidence, and prepared-tool false confidence are covered. |
| Architecture alignment | pass | The plan follows ADR 0011's first-slice decisions. |
| Operational readiness | pass | Runtime evidence, full CI status, and release-evidence preservation are planned. |
| Plan maintainability | pass | Progress, handoff summary, decision log, discoveries, validation notes, and retrospective sections are present. |

## Required Plan Updates

- Update milestone validation commands so changed tests/code are built before `--no-build` measurement commands run.
- Replace ambiguous non-taxonomy filters with valid explicit filters.
- Add a sequencing guard so wrapper coverage is not removed before replacement script smoke and hermetic evidence exists.

## Immediate Next Stage

`review-resolution`

## Eventual Test-Spec Readiness

not-ready until TRO-PL1 and TRO-PL2 are resolved and the revised plan is re-reviewed.
