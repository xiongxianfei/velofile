# ADR 0011: Test Runtime Validation Tiers and Corpus Harness Optimization

## Status

accepted

## Context

The test runtime optimization spec addresses a measured validation bottleneck: `VeloFile.Corpus.Tests` took about `5 m 49 s` for 37 tests, with the slow path dominated by PowerShell wrapper execution, scratch source copying, repeated `dotnet publish`, and assembly-wide serialization.

The project still needs credible release evidence for compatibility, preview, diagnostics, benchmark, visual, and manual-evidence checks. The architecture decision is therefore not to remove evidence, but to separate validation cost by purpose and reduce repeated corpus wrapper overhead where the public wrapper is not the behavior under test.

## Decision

Introduce validation tiers as an explicit test-harness architecture boundary:

```text
Fast / Contract
  -> inner-loop validation

Smoke / CorpusScript
  -> representative public wrapper validation

ReleaseEvidence / Benchmark / Visual / ManualEvidence
  -> explicit evidence and closeout validation
```

For the first implementation slice:

- `scripts/ci.ps1` remains the broad validation command for milestone closeout and review gates.
- CI job splitting is deferred until a later accepted decision.
- Public corpus wrapper command-line contracts remain backward compatible.
- Prepared-tool execution remains internal to tests and is not exposed as `-PreparedToolPath` or `-UseExistingToolBuild`.
- One common hermetic wrapper isolation test proves scratch copy/publish behavior and absence of repository-side generated output.
- Minimal public script smoke tests cover supported public corpus script families.
- Full profile and scope matrices remain in `ReleaseEvidence`.
- Assembly-wide `DoNotParallelize` removal is deferred until a later measured parallel-safety slice.

Prepared-tool execution may build or publish the corpus tool once for repeated test process calls, but only under a test-owned scratch/temp root. Before invocation, the harness must validate a current-run prepared-tool manifest that identifies the setup invocation, tool kind, configuration, target framework, entrypoint, and expected tool artifact. Missing roots, outside-root paths, missing manifests, mismatched setup identifiers, wrong declared tool metadata, and missing artifacts fail before tool execution. Source-hash and cross-run cache staleness detection are deferred unless cross-run prepared-tool reuse is introduced.

Runtime evidence is part of the architecture contract. The first slice records baseline Corpus runtime, optimized contract runtime, optimized script-smoke runtime, top slow tests, and whether full `scripts/ci.ps1` improved, stayed the same, or regressed.

## Alternatives considered

- Keep the validation suite unchanged: rejected because slow local feedback encourages skipped validation and makes review-resolution expensive.
- Remove slow corpus tests from normal validation: rejected because it weakens release confidence by deleting evidence instead of separating tiers.
- Split CI immediately into fast and full hosted jobs: rejected for the first slice because the category model and runtime evidence should stabilize before changing hosted validation behavior.
- Expose `-PreparedToolPath` as a public script option immediately: rejected because it changes the public wrapper contract before the test harness optimization proves reliable.
- Use source-hash cross-run caching in the first slice: rejected as premature. A current-run manifest prevents accidental stale reuse within the first optimization boundary without committing to cache invalidation semantics.
- Remove assembly-wide `DoNotParallelize` immediately: rejected because hidden shared-state coupling should be discovered and measured before enabling broad parallel execution.

## Consequences

- Contributor workflow gains explicit fast/contract commands while preserving broad closeout validation.
- Corpus tests must carry accepted category metadata, and category inventory validation becomes a first-class guard.
- Some corpus checks move from script-wrapper execution into in-process or prepared-tool execution, with public wrapper confidence preserved by smoke coverage.
- The corpus test harness gains a prepared-tool manifest and scratch-root boundary.
- Runtime reports become review evidence, not universal performance claims.
- Public script wrappers remain stable for the first slice.
- Release-evidence tests remain available and are not silently included in fast defaults.

## Follow-up

- The matching test spec must map category inventory, prepared-tool manifest rejection, hermetic wrapper isolation, script smoke coverage, runtime reporting, and full validation preservation to concrete tests.
- The execution plan must keep CI splitting, public prepared-tool options, source-hash caching, and assembly-wide `DoNotParallelize` removal out of the first implementation slice unless a later review changes scope.
