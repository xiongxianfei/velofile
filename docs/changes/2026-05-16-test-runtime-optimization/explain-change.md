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
