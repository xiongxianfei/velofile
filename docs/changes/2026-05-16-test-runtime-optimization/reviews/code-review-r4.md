# Code Review R4: M3 Public Script Smoke and Hermetic Wrapper Coverage

## Review status

clean-with-notes

## Review inputs

- Diff range: `c35a6a4..181e557`
- Review surface: tracked commit `181e557 M3: Keep corpus script smoke and hermetic wrapper coverage`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M3
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M3 validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`

## Diff summary

M3 adds a minimal public-wrapper smoke layer and strengthens the common hermetic wrapper proof:

- `CorpusScriptSmokeTests` adds representative `CorpusScript` + `Smoke` coverage for `run-compat-corpus.ps1`, `run-preview-corpus.ps1`, `run-benchmarks.ps1`, and `run-diagnostics-conformance.ps1`.
- The existing generate script smoke path is renamed and extended as `HermeticWrapper_scratch_publish_isolation_and_path_safety`, proving scratch source copy, scratch publish output, repository-output isolation, and user PATH safety.
- `PublicCorpusScriptHarness` centralizes public PowerShell script invocation for M3 smoke tests.
- `RepoOutputSnapshot` centralizes repository-side output snapshots for contract and hermetic wrapper checks.
- The wrapper coverage migration ledger, plan state, change metadata, and change rationale record M3 evidence and handoff state.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | M3 covers R27-R33 by retaining a common hermetic wrapper isolation path, adding one minimal public-script smoke path per in-scope script family, and keeping full matrix coverage out of smoke-only tests. |
| Test coverage | pass | `HermeticWrapper_scratch_publish_isolation_and_path_safety` covers TTO-T011; `CorpusScriptSmokeTests` covers TTO-T013 through TTO-T016; existing `Generate_corpus_refuses_repository_root` covers a public wrapper failure path for TTO-T038. |
| Edge cases | pass | Smoke invocations use smoke scope/minimal run count checks, representative JSON output checks, and a controlled unsafe scratch-root failure through the public wrapper boundary. |
| Error handling | pass | Public script failures remain visible through `Generate_corpus_refuses_repository_root`, which asserts a nonzero wrapper exit and controlled `unsafe scratch root` output. |
| Architecture boundaries | pass | M3 stays in test harness and documentation surfaces; it does not expose prepared-tool options, split CI, or change production App/Core/Windows behavior. |
| Compatibility | pass | Public wrapper command-line behavior remains unchanged; all five public corpus script families retain representative wrapper coverage. |
| Security/privacy | pass | Hermetic wrapper checks assert no scratch .NET tools path is added to the user PATH and repository-side generated output snapshots remain unchanged. |
| Derived artifact currency | pass | Plan, plan index, change metadata, migration ledger, and change rationale were updated with M3 state and validation evidence. |
| Unrelated changes | pass | Reviewed diff is limited to Corpus test harness/tests and test-runtime optimization documentation. |
| Validation evidence | pass | The plan records passing M3 build-producing test commands for `CorpusScript&Smoke`, `FullyQualifiedName~HermeticWrapper`, `FullyQualifiedName~CategoryInventory|TestCategory=CorpusScript`, minimal-scope checks, ledger checks, and `git diff --check`. |

## No-finding rationale

No blocking findings were found because the diff installs the M3 replacement public-wrapper smoke and hermetic coverage without removing release-evidence tests, the named M3 script families have direct wrapper-boundary tests, and the recorded validation evidence covers the milestone commands.

## Residual risks

- The broader `FullyQualifiedName~CategoryInventory|TestCategory=CorpusScript` validation remains slow because it intentionally includes existing `ReleaseEvidence` wrapper tests. Runtime reduction and slow-test reporting remain assigned to later milestones.
- Full `scripts/ci.ps1` was not rerun for M3; the active M3 validation commands passed, and broad closeout validation remains in later milestone/final closeout scope.

## Recommended next stage

Close M3 and proceed to `implement` M4.
