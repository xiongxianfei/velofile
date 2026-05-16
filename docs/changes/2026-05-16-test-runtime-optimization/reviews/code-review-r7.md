# Code Review R7: M5 Release-Evidence Preservation

## Review status

changes-requested

## Review inputs

- Diff range: `6e022a9..46461b3`
- Review surface: tracked commit `46461b3 M5: Preserve corpus release-evidence validation`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/architecture/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M5
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M5 validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`

## Diff summary

M5 adds release-evidence tier guardrails and handoff evidence:

- `ReleaseEvidenceTierTests` checks the documented `ReleaseEvidence` command, expected release-evidence test categorization, full matrix tests remaining release-evidence instead of smoke-only, benchmark evidence categorization, visual/manual evidence rationale for fast-default membership, and broad unfiltered `scripts/ci.ps1` routing.
- `CorpusTestCategories` adds `EvidenceFastPathRationaleAttribute` and validation that rejects `Visual` or `ManualEvidence` tests selected by `Fast` or `Contract` without a rationale.
- `ShellVisualCoherenceContractTests` records a static-contract rationale for its `Visual` + `Contract` category combination.
- The plan, plan index, change metadata, and change rationale record M5 validation evidence and set M5 to review-requested.

## Findings

### TRO-CR3: ManualEvidence fast-default rationale path lacks direct regression proof

Severity: major

Location: `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs:141`; `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs:87`; `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs:97`

Evidence:

- Spec R42 requires both visual evidence and manual evidence checks to remain outside fast defaults unless they satisfy a fast/contract purpose with explicit rationale: `specs/test-runtime-optimization.md:207`.
- Test spec TTO-T030 explicitly names `Visual` and `ManualEvidence` tests in the expected proof: `specs/test-runtime-optimization.test.md:375`.
- The validator implementation covers `ManualEvidence` in the same branch as `Visual`, but the direct negative/positive tests added in `CategoryInventoryTests` only use `[Visual, Contract]`.
- `ReleaseEvidenceTierTests.Visual_and_manual_evidence_fast_default_members_have_explicit_rationale` scans the current assembly, but the current Corpus test assembly has `Visual` tests and no `ManualEvidence` tests, so it does not directly exercise a `ManualEvidence` fast-default case.

Required outcome:

- Add direct test coverage proving `ManualEvidence` + `Fast` or `ManualEvidence` + `Contract` without a non-empty `EvidenceFastPathRationale` fails inventory validation, and that a non-empty rationale allows the combination when appropriate.

Safe resolution path:

- Keep the fix scoped to M5 category inventory tests and helper tests.
- Add focused `CategoryInventoryTests` cases for `ManualEvidence` + `Contract` or `ManualEvidence` + `Fast` without rationale and with rationale.
- Rerun the M5 focused validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~ReleaseEvidenceTierTests|FullyQualifiedName~CategoryInventoryTests"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"`
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"`
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
  - `git diff --check`

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | ReleaseEvidence, benchmark, visual, and broad CI boundaries are mostly aligned, but R42's ManualEvidence branch lacks direct proof. |
| Test coverage | block | TTO-T030 requires visual/manual evidence coverage; only the Visual fast-default rationale case is directly tested. |
| Edge cases | block | The named ManualEvidence evidence-tier edge case is covered by code shape but not by a targeted test. |
| Error handling | pass | Category inventory diagnostics are actionable and identify missing non-empty rationale. |
| Architecture boundaries | pass | M5 stays in test/runtime documentation surfaces; it does not change public wrappers, split CI, expose prepared-tool options, or change production App/Core/Windows behavior. |
| Compatibility | pass | Existing release-evidence tests remain runnable through the documented command; public wrapper contracts are unchanged. |
| Security/privacy | pass | No secret or private-path outputs were added; changes are category metadata and documentation records. |
| Derived artifact currency | pass | Plan, plan index, change metadata, and change rationale were updated with M5 state and validation evidence. |
| Unrelated changes | pass | Reviewed diff is limited to M5 Corpus test-runtime category checks and test-runtime optimization records. |
| Validation evidence | concern | Recorded M5 commands are relevant and pass, but the selected tests do not directly prove the ManualEvidence variant required by TTO-T030. |

## No-finding rationale

Not applicable. A required-change finding is present.

## Residual risks

- The explicit `ReleaseEvidence` and full `scripts/ci.ps1` runs remain slow by design. Runtime summary and top slow-test reporting remain assigned to M6.

## Recommended next stage

Enter `review-resolution` for TRO-CR3. Keep M5 in `resolution-needed` until the ManualEvidence proof gap is fixed, targeted validation is rerun, and M5 is re-reviewed.
