# Code Review R3: M2 Corpus Contract Tests Without Wrapper Cost

## Review status

clean-with-notes

## Review inputs

- Diff range: `9b7daba..4e3869b`
- Review surface: tracked commit `4e3869b M2: Split corpus contract tests from wrapper execution`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M2
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M2 validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`

## Diff summary

M2 adds in-process Corpus CLI contract coverage through a test-only internal seam:

- `tools/VeloFile.Corpus/Program.cs` grants `InternalsVisibleTo("VeloFile.Corpus.Tests")`.
- `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj` targets the Corpus tool's Windows TFM and references `tools/VeloFile.Corpus`.
- New contract tests cover manifest/scope output, compatibility release classification, preview contract output, diagnostics redaction, scratch-root boundaries, and wrapper coverage preservation.
- The plan, change metadata, and change rationale record the M2 decision, validation evidence, and handoff state.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | M2 addresses R22-R26 by adding in-process contract checks and scratch-root proof while preserving public wrapper coverage required by R24. |
| Test coverage | pass | `CorpusContractTests`, `ScratchRootBoundaryTests`, and `WrapperCoverageLedgerTests` cover TTO-T008, TTO-T009, and TTO-T010. |
| Edge cases | pass | Release classification remains incomplete when verifier evidence is missing; diagnostics redaction checks prohibited values; repo-side generated output snapshots are compared before/after. |
| Error handling | pass | Contract tests assert nonzero release compatibility status and durable incomplete release result rather than treating missing evidence as success. |
| Architecture boundaries | pass | The implementation keeps prepared-tool execution and public script option changes out of M2; the only tool seam is internal to tests. |
| Compatibility | pass | Public wrapper tests remain present and `CorpusScript` categorized until M3; `scripts/ci.ps1` remains the broad validation command. |
| Security/privacy | pass | Scratch-root tests prevent repo-side generated output; diagnostics test asserts redacted export does not include prohibited sensitive strings. |
| Derived artifact currency | pass | Plan, plan index, change metadata, and change rationale were updated with M2 state and validation evidence. |
| Unrelated changes | pass | Reviewed diff is limited to Corpus tests, the Corpus tool test seam, and test-runtime optimization docs. |
| Validation evidence | pass | Focused M2 tests, Corpus contract/fast filters, solution fast/contract filter, `scripts/ci.ps1`, and `git diff --check` are recorded as passing. |

## No-finding rationale

No blocking findings were found because the diff implements M2's low-overhead contract-test slice without reducing public wrapper coverage, the new tests directly prove the named M2 claims, and the recorded validation evidence includes both focused contract proof and broad `scripts/ci.ps1`.

## Residual risks

- Unfiltered Corpus validation remains slow because M2 intentionally preserves existing broad public-wrapper tests until M3 installs replacement smoke and hermetic coverage.
- The new test-only internal seam exposes all Corpus internals to the test assembly. This is acceptable for M2 but should not become a public script contract or production API.

## Recommended next stage

Close M2 and proceed to `implement` M3.
