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

## M3 rationale

M3 installs the public-wrapper safety net that lets later milestones reduce broad wrapper assertions without losing public script confidence. Each public corpus script family now has minimal `CorpusScript` + `Smoke` coverage:

```text
generate-corpus.ps1
run-compat-corpus.ps1
run-preview-corpus.ps1
run-benchmarks.ps1
run-diagnostics-conformance.ps1
```

The generate smoke path also serves as the common hermetic wrapper isolation test. It proves the shared wrapper creates scratch source copies, publishes the Corpus tool under `.velofile-tools`, does not create repository-side generated outputs, and does not add scratch .NET tool paths to the user PATH.

## M3 boundaries

M3 does not remove release-evidence wrapper tests, expose prepared-tool options, split CI, or change public script command-line arguments. Full matrix/profile coverage remains categorized as `ReleaseEvidence`; smoke coverage uses representative output only.

## M3 evidence

The `CorpusScript&Smoke` tier selected 6 tests and completed in about 54 seconds locally. The broader `FullyQualifiedName~CategoryInventory|TestCategory=CorpusScript` validation selected 26 tests and still took about 6 m 48 s because it intentionally includes existing release-evidence wrapper tests. Full runtime comparison and slow-test reporting remain assigned to M6.

## M4 rationale

M4 adds a test-internal prepared corpus tool harness for process-based tests that need to invoke the Corpus CLI without proving scratch source copy and publish on every assertion. The harness publishes `tools/VeloFile.Corpus` into a prepared-tool root under the test-owned scratch root, writes `.velofile-prepared-tool.json`, validates the current-run manifest before invocation, and then runs the prepared `VeloFile.Corpus.dll` through `dotnet`.

The manifest records only test-harness metadata:

```text
schemaVersion
toolKind
setupId
configuration
targetFramework
entrypoint
createdUtc
```

It does not store local user paths or machine-specific private paths.

## M4 boundaries

M4 does not change public corpus scripts, add `-PreparedToolPath` or `-UseExistingToolBuild`, split CI, remove release evidence, remove assembly-wide `DoNotParallelize`, or change production App/Core/Windows behavior. Invalid prepared-tool roots fail before process invocation with controlled diagnostics.

## M4 evidence

The focused prepared-tool validation selected 9 tests and passed after the harness implementation. The tests cover current-run repeated execution through one prepared tool, missing root, outside-root path, missing manifest, previous setup id, bad metadata, missing artifact, global-state/repo-output safety, and public script option absence. The retained `CorpusScript&Smoke` tier selected 6 tests and completed in about 56 seconds locally.
