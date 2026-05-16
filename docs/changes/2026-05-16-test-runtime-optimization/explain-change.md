# Test Runtime Optimization Change Rationale

## M1 rationale

M1 establishes the validation taxonomy before moving any tests between execution paths. This keeps the first slice focused on reviewer-visible classification and local command guidance rather than changing public wrapper behavior.

The Corpus test project now has a single accepted category vocabulary:

```text
Fast
Contract
Smoke
CorpusScript
ReleaseEvidence
Benchmark
Visual
ManualEvidence
```

The category inventory tests make drift visible by rejecting missing categories, unknown legacy names, invalid `ReleaseEvidence` + `Fast` combinations without rationale, and `CorpusScript` tests that do not also declare `Smoke` or `ReleaseEvidence`.

## Boundaries

M1 does not optimize the wrapper implementation, add prepared-tool execution, split hosted CI, remove release evidence, or remove assembly-wide `DoNotParallelize`. Those changes remain assigned to later milestones in the approved plan.

## Evidence

M1 records the accepted pre-optimization Corpus runtime baseline and the shared-state inventory required before any future parallelization slice.

## M2 rationale

M2 adds low-overhead Corpus contract coverage without reducing public wrapper coverage. The new contract tests invoke the Corpus CLI in process through a narrow test-only internal seam, so report-shape, manifest, scope classification, preview, diagnostics redaction, scratch-root, and release-classification assertions no longer need to pay PowerShell wrapper and scratch publish cost.

The public wrapper tests remain in place and retain their `CorpusScript` categories until M3 installs the smaller replacement smoke and hermetic wrapper coverage. This avoids a coverage gap while making the contract tier useful for local validation.

## M2 boundaries

M2 does not expose prepared-tool options, split CI, remove release evidence, delete public wrapper tests, or change production App/Core/Windows behavior. `VeloFile.Corpus.Tests` now references `tools/VeloFile.Corpus` and targets the same Windows TFM so the contract tests can call the tool directly.

## M2 evidence

The Corpus contract/fast no-build run selected 42 tests and completed in about 23 seconds locally. Full `scripts/ci.ps1` passed, with the unfiltered Corpus project still taking about 5 m 46 s because M2 preserves existing public wrapper coverage until M3. Full runtime comparison and slow-test reporting remain assigned to M6.
