# Code Review R1: Test Runtime Optimization M1

## Review Status

changes-requested

## Review Inputs

- Commit: `b33e9da M1: Add corpus validation category contract`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md` M1
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: commit message and M1 validation notes in `docs/plans/2026-05-16-test-runtime-optimization.md`

## Diff Summary

M1 adds the test runtime optimization governing artifacts, accepted Corpus test category constants, category inventory tests, local validation command documentation, shared-state inventory, baseline runtime evidence, and category annotations across `VeloFile.Corpus.Tests`.

## Findings

### TRO-CR1: `ReleaseEvidence` + `Fast` rationale can be empty

Severity: major

Evidence:

- Spec R12 requires a `VeloFile.Corpus.Tests` test marked `ReleaseEvidence` and `Fast` to have an explicit rationale.
- The new attribute stores a rationale string at `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs:39`, but the inventory descriptor only carries a boolean at `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs:47`.
- The assembly inventory sets that boolean based only on attribute presence at `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs:103` and `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs:112`.
- Validation then accepts the combination when the boolean is true at `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs:75`.
- The focused tests cover no-rationale and boolean-rationale paths in `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs:42` and `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs:56`, but they do not prove that an empty or whitespace rationale is rejected.

Required outcome:

Category inventory validation must reject `ReleaseEvidence` + `Fast` when the recorded rationale is missing, empty, or whitespace. The diagnostic should identify the offending test and explain that a non-empty rationale is required.

Safe resolution path:

- Change the inventory descriptor to carry the rationale text or an equivalent non-empty-rationale state.
- Update `CorpusCategoryInventory.FromAssembly` so class-level and method-level `ReleaseEvidenceFastRationaleAttribute` values must be non-empty after trimming.
- Add a focused test proving empty or whitespace rationale fails.
- Keep the fix scoped to M1 category inventory tests and helper code.
- Rerun M1 focused validation.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | R8-R11, R13-R15, R16-R21, R44-R45, and R48 are covered, but R12 is incomplete because empty rationale text can pass. |
| Test coverage | concern | Category inventory tests cover missing/unknown categories and boolean rationale presence, but not empty rationale text. |
| Edge cases | concern | The `ReleaseEvidence` + `Fast` failure path is covered only for absent rationale, not invalid rationale content. |
| Error handling | pass | Missing/unknown category and invalid companion-category diagnostics are controlled and actionable. |
| Architecture boundaries | pass | No production App/Core/Windows behavior changes are introduced; changes stay in tests, docs, scripts evidence, and validation architecture artifacts. |
| Compatibility | pass | Public wrapper command-line behavior is unchanged and existing wrapper coverage is preserved for M1. |
| Security/privacy | pass | Runtime baseline and shared-state inventory do not expose secrets; scratch and user environment risks are recorded for later slices. |
| Derived artifact currency | pass | Plan, plan index, change record, spec, test spec, architecture, and ADR are tracked in the reviewed commit. |
| Unrelated changes | pass | Diff matches the approved test runtime optimization proposal/spec/architecture/plan/test-spec plus M1 implementation surfaces. |
| Validation evidence | pass | M1 records the failing category test before migration and passing targeted validation after implementation. |

## Recommended Next Stage

`review-resolution` for TRO-CR1. M1 remains `resolution-needed` until the category rationale validation gap is fixed and re-reviewed.
