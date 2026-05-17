# Test Runtime Optimization Test Spec

## Status

active

## Related Spec And Plan

- Feature spec: [test-runtime-optimization.md](test-runtime-optimization.md)
- Execution plan: [2026-05-16-test-runtime-optimization.md](../docs/plans/2026-05-16-test-runtime-optimization.md)
- Architecture: [architecture.md](../docs/architecture/system/architecture.md)
- ADR: [0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md](../docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md)

## Testing Strategy

The proof strategy is tiered in the same way as the implementation contract.

- Contract tests prove category taxonomy, command documentation, corpus output contracts, scratch-root boundaries, prepared-tool manifest validation, release-evidence selection, and runtime-report shape.
- Smoke tests prove public corpus script entrypoints and one common hermetic scratch-publish path.
- Release-evidence tests remain runnable through explicit filters and broad closeout validation.
- Manual evidence is limited to runtime interpretation and full-CI environment limitations when a local machine cannot run the broad command.

The first implementation slice must not rely on screenshots, UI behavior, production App/Core/Windows changes, hosted CI job splitting, public prepared-tool options, cross-run caching, or broad test parallelization to prove this spec.

## Requirement Coverage Map

| Requirements | Coverage |
|---|---|
| R1, R2, R3 | TTO-T037, TTO-T040 |
| R4, R5 | TTO-T036, TTO-T040 |
| R6, R7 | TTO-T027, TTO-T038 |
| R8, R9, R10, R11, R15 | TTO-T001, TTO-T002, TTO-T003 |
| R12 | TTO-T004 |
| R13 | TTO-T005 |
| R14 | TTO-T007, TTO-T030 |
| R16, R17, R18, R19, R20, R21 | TTO-T006 |
| R22, R23 | TTO-T008 |
| R24 | TTO-T010, TTO-T011, TTO-T012, TTO-T013, TTO-T014, TTO-T015, TTO-T016, TTO-T038 |
| R25, R26 | TTO-T009, TTO-T039 |
| R27, R28 | TTO-T011 |
| R29, R32 | TTO-T012, TTO-T013, TTO-T014, TTO-T015, TTO-T016 |
| R30, R31 | TTO-T017, TTO-T018 |
| R33 | TTO-T038 |
| R34, R35 | TTO-T019 |
| R36 | TTO-T026 |
| R37 | TTO-T020, TTO-T021, TTO-T022, TTO-T023, TTO-T024, TTO-T025 |
| R38 | TTO-T027 |
| R39, R40, R43 | TTO-T028, TTO-T036 |
| R41 | TTO-T029 |
| R42 | TTO-T030 |
| R44, R45, R46, R47 | TTO-T031 |
| R48, R49, R50, R51, R52, R53, R54, R55 | TTO-T032, TTO-T033, TTO-T034 |
| R56, R57, R58, R59, R60 | TTO-T035 |
| AC1 | TTO-T037 |
| AC2 | TTO-T001, TTO-T002, TTO-T003, TTO-T004, TTO-T005 |
| AC3 | TTO-T006 |
| AC4 | TTO-T008 |
| AC5 | TTO-T011 |
| AC6 | TTO-T012, TTO-T013, TTO-T014, TTO-T015, TTO-T016 |
| AC7 | TTO-T017, TTO-T018 |
| AC8 | TTO-T021 |
| AC9 | TTO-T022, TTO-T023, TTO-T024, TTO-T025 |
| AC10 | TTO-T032, TTO-T033, TTO-T034, TTO-T035 |
| AC11 | TTO-T036 |
| AC12 | TTO-T037 |

## Example Coverage Map

| Example | Coverage |
|---|---|
| E1 contributor runs the fast local loop | TTO-T006, TTO-T007 |
| E2 corpus contract checks avoid PowerShell wrapper cost | TTO-T008 |
| E3 public wrappers keep representative smoke coverage | TTO-T012, TTO-T013, TTO-T014, TTO-T015, TTO-T016, TTO-T017 |
| E4 hermetic scratch-publish behavior remains proven | TTO-T011 |
| E5 runtime evidence is recorded | TTO-T032, TTO-T033, TTO-T034 |
| E6 release evidence remains available | TTO-T028, TTO-T036 |

## Edge Case Coverage

| Edge case | Coverage |
|---|---|
| E1 release report shape does not need a wrapper | TTO-T008, TTO-T018 |
| E2 wrapper routing bug missed by in-process tests | TTO-T038 |
| E3 quick benchmark is not automatically Fast | TTO-T029 |
| E4 shared environment mutation | TTO-T031 |
| E5 fast command without build first | TTO-T006 |
| E6 contract time improves but full CI regresses | TTO-T032, TTO-T035 |
| E7 public script family removed or superseded later | TTO-T010, TTO-T012, TTO-T013, TTO-T014, TTO-T015, TTO-T016 |
| E8 prepared-tool path points inside repo | TTO-T021, TTO-T039 |
| E9 prepared-tool manifest from previous setup | TTO-T023 |
| E10 current manifest but missing entrypoint artifact | TTO-T025 |

## Test Cases

### Category Taxonomy And Commands

TTO-T001. Corpus category inventory accepts only taxonomy categories
- Covers: R8, R9, R10, R11, R15, AC2
- Level: contract
- Fixture/setup: `VeloFile.Corpus.Tests` compiled test metadata and accepted category constants.
- Steps: Enumerate Corpus test methods/classes and collect `TestCategory` metadata.
- Expected result: Every category is one of `Fast`, `Contract`, `Smoke`, `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, or `ManualEvidence`.
- Failure proves: Category drift can make filters unreliable.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

TTO-T002. Corpus category inventory rejects missing categories
- Covers: R9, R15, AC2
- Level: contract
- Fixture/setup: synthetic or inline metadata fixture containing a test with no accepted category.
- Steps: Run the inventory validator against the fixture.
- Expected result: Validation fails and names the uncategorized test.
- Failure proves: New Corpus tests can silently bypass the tier model.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

TTO-T003. Corpus category inventory rejects unknown categories
- Covers: R11, R15, AC2
- Level: contract
- Fixture/setup: synthetic or inline metadata fixture containing legacy categories such as `UiContracts`, `Release`, or `Compatibility`.
- Steps: Run the inventory validator against the fixture.
- Expected result: Validation fails and identifies the unknown category.
- Failure proves: Legacy categories can survive without an explicit migration decision.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

TTO-T004. ReleaseEvidence plus Fast requires explicit rationale
- Covers: R12, R15, AC2
- Level: contract
- Fixture/setup: metadata fixture with a test marked `ReleaseEvidence` and `Fast`, once without rationale and once with accepted adjacent rationale.
- Steps: Run inventory validation for both cases.
- Expected result: Missing rationale fails; accepted rationale passes.
- Failure proves: Expensive release checks can leak into the fast loop without review.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

TTO-T005. CorpusScript requires Smoke or ReleaseEvidence
- Covers: R13, R15, AC2
- Level: contract
- Fixture/setup: metadata fixture with `CorpusScript` alone and with valid companion categories.
- Steps: Run inventory validation.
- Expected result: `CorpusScript` alone fails; `CorpusScript` + `Smoke` or `CorpusScript` + `ReleaseEvidence` passes.
- Failure proves: Public-wrapper tests can be categorized without a clear validation purpose.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

TTO-T006. Contributor validation commands are documented with no-build assumption
- Covers: R16, R17, R18, R19, R20, R21, AC3
- Level: contract
- Fixture/setup: contributor validation guidance selected by M1, plus the plan if it remains the only local-command surface.
- Steps: Read the guidance and assert exact fast, corpus contract, script-smoke, release-evidence, and full closeout commands are present.
- Expected result: Required commands exist, and `--no-build` commands state that projects must already be built.
- Failure proves: Contributors cannot reliably choose the intended validation tier.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ValidationCommandDocumentationTests.cs`

TTO-T007. Fast command excludes expensive-only tiers
- Covers: R14, E1
- Level: contract
- Fixture/setup: MSTest filter expression from documentation and category inventory data.
- Steps: Evaluate or inspect the documented fast filter against known category sets.
- Expected result: Tests categorized only as `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, or `ManualEvidence` are not selected by the default fast command.
- Failure proves: The inner-loop command still includes evidence tiers it is meant to skip.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ValidationCommandDocumentationTests.cs`

### Corpus Contract Split

TTO-T008. Corpus contract assertions run without public wrapper execution
- Covers: R22, R23, AC4, E2
- Level: integration
- Fixture/setup: test-owned scratch root and a corpus command/test seam selected during M2.
- Steps: Run representative contract checks for report shape, manifest, redaction, profile decisions, scope classification, or release classification without invoking PowerShell public wrappers.
- Expected result: Checks pass, no public script process is launched, and the test is categorized `Contract` rather than `CorpusScript`.
- Failure proves: Contract checks still pay wrapper cost or are mislabeled.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusContractTests.cs`

TTO-T009. Contract and prepared-tool outputs stay under scratch root
- Covers: R25, R26
- Level: integration
- Fixture/setup: unique scratch root plus repository snapshot of forbidden output locations.
- Steps: Run in-process or prepared-tool contract tests and inspect repo-side `bin`, `obj`, generated reports, diagnostics, benchmark, and corpus output locations.
- Expected result: New outputs appear only under the assigned scratch/temp root unless they are explicitly tracked fixtures.
- Failure proves: Optimized tests leak generated artifacts into the repository.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ScratchRootBoundaryTests.cs`

TTO-T010. Wrapper coverage migration ledger stays lossless
- Covers: R24
- Level: contract
- Fixture/setup: M2/M3 ledger in the active plan and the actual smoke/release tests after M3.
- Steps: Assert each existing public wrapper-backed claim is marked preserved until replacement evidence exists.
- Expected result: No claim is removed with only future replacement planned.
- Failure proves: The suite can lose public-wrapper coverage during migration.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/WrapperCoverageLedgerTests.cs`

### Public Script Smoke And Hermetic Wrapper

TTO-T011. Common hermetic wrapper isolation proves scratch publish and no repo-side output
- Covers: R24, R27, R28, AC5, E4
- Level: smoke
- Fixture/setup: clean test-owned scratch root and repository output snapshot.
- Steps: Invoke the shared wrapper path once through a public script that exercises scratch source copy and publish.
- Expected result: Scratch source copy and publish occur under the scratch root, no repository-side output is created, and the test is categorized `CorpusScript` + `Smoke`.
- Failure proves: The optimized suite no longer proves the hermetic wrapper behavior.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T012. generate-corpus public script smoke
- Covers: R24, R29, R32, AC6
- Level: smoke
- Fixture/setup: scratch root.
- Steps: Invoke `scripts/generate-corpus.ps1` with a minimal smoke profile.
- Expected result: The script routes successfully and emits representative corpus output.
- Failure proves: The generate-corpus public entrypoint is broken or no longer covered.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T013. run-compat-corpus public script smoke
- Covers: R24, R29, R32, AC6
- Level: smoke
- Fixture/setup: scratch root and minimal smoke corpus when needed.
- Steps: Invoke `scripts/run-compat-corpus.ps1` with minimal smoke scope.
- Expected result: The script routes successfully and emits representative compatibility output.
- Failure proves: The compatibility wrapper is broken or no longer covered.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T014. run-preview-corpus public script smoke
- Covers: R24, R29, R32, AC6, E3
- Level: smoke
- Fixture/setup: scratch root and minimal preview corpus.
- Steps: Invoke `scripts/run-preview-corpus.ps1` with minimal smoke/contract scope.
- Expected result: The script routes successfully and emits representative preview output.
- Failure proves: The preview wrapper is broken or no longer covered.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T015. run-benchmarks public script smoke when in scope
- Covers: R24, R29, R32, AC6
- Level: smoke
- Fixture/setup: scratch root and smallest benchmark-compatible input.
- Steps: Invoke `scripts/run-benchmarks.ps1` in the minimal supported smoke mode.
- Expected result: The script route is proven without running the full benchmark matrix.
- Failure proves: Benchmark public wrapper routing is broken or no longer covered.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T016. run-diagnostics-conformance public script smoke when in scope
- Covers: R24, R29, R32, AC6
- Level: smoke
- Fixture/setup: scratch root and minimal diagnostics corpus.
- Steps: Invoke `scripts/run-diagnostics-conformance.ps1` with minimal smoke input.
- Expected result: The script route is proven and representative diagnostics output appears.
- Failure proves: Diagnostics public wrapper routing is broken or no longer covered.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T017. Script smoke tests do not run full matrices
- Covers: R30, AC7
- Level: contract
- Fixture/setup: script-smoke tests and their argument builders.
- Steps: Inspect or assert smoke invocations use minimal smoke scopes and do not enumerate all profiles/scopes.
- Expected result: Full profile/scope matrices are absent from `Smoke`-only tests.
- Failure proves: Smoke tests still pay release-evidence cost.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

TTO-T018. Full matrices are categorized ReleaseEvidence
- Covers: R31, AC7, E1
- Level: contract
- Fixture/setup: release matrix tests after M2/M5 categorization.
- Steps: Inspect tests that cover compatibility, preview providers, thumbnails, diagnostics, benchmark, or release classification matrices.
- Expected result: Matrix tests have `ReleaseEvidence` and are not selected by default fast/script-smoke commands unless explicitly intended.
- Failure proves: Release evidence is either hidden or incorrectly included in the fast loop.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`

TTO-T038. Public wrapper failure remains visible
- Covers: R6, R24, R33, E2
- Level: smoke
- Fixture/setup: a controlled invalid wrapper invocation, such as unsupported scope or missing required input, that does not depend on private machine state.
- Steps: Invoke the public wrapper and assert failure is reported as a wrapper/script failure with controlled output.
- Expected result: Failure surfaces through the public script path and is not hidden behind in-process contract success.
- Failure proves: Optimized tests can mask public wrapper regressions.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

### Prepared Tool Harness

TTO-T019. Current-run prepared tool executes
- Covers: R34, R35
- Level: integration
- Fixture/setup: test-owned scratch root, prepared corpus tool root, and `.velofile-prepared-tool.json` generated by the current setup invocation.
- Steps: Prepare the tool, validate manifest metadata, and invoke a minimal command through the prepared-tool harness.
- Expected result: The command runs successfully and all tool artifacts remain inside the scratch root.
- Failure proves: The prepared-tool path is unusable or not isolated.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T020. Missing prepared-tool root fails before invocation
- Covers: R37
- Level: integration
- Fixture/setup: non-existent prepared-tool root under the allowed scratch parent.
- Steps: Ask the prepared-tool harness to invoke it.
- Expected result: Invocation fails before process start with a controlled missing-root diagnostic.
- Failure proves: Invalid tool paths can be executed or fail unclearly.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T021. Outside-root prepared tool fails before invocation
- Covers: R37, AC8, E8
- Level: integration
- Fixture/setup: prepared-tool path outside the allowed scratch/temp root, including a path inside the repository when not an explicitly tracked fixture.
- Steps: Ask the harness to invoke it.
- Expected result: Invocation fails before process start with an outside-root diagnostic.
- Failure proves: Prepared-tool execution can escape the test-owned boundary.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T022. Missing manifest fails before invocation
- Covers: R37, AC9
- Level: integration
- Fixture/setup: prepared-tool root with expected directory shape but no `.velofile-prepared-tool.json`.
- Steps: Ask the harness to invoke it.
- Expected result: Invocation fails before process start with `prepared-tool-manifest-missing` or equivalent.
- Failure proves: The harness can run unmanaged prepared tools.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T023. Previous setup manifest fails as stale
- Covers: R37, AC9, E9
- Level: integration
- Fixture/setup: prepared-tool root with manifest setup id from a different setup invocation.
- Steps: Ask the harness to invoke it.
- Expected result: Invocation fails before process start with stale/setup-mismatch diagnostic.
- Failure proves: Tests can accidentally reuse stale prepared tools.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T024. Wrong prepared-tool metadata fails
- Covers: R37, AC9
- Level: integration
- Fixture/setup: prepared-tool manifest declaring wrong tool kind, configuration, target framework, or entrypoint.
- Steps: Ask the harness to invoke each invalid manifest case.
- Expected result: Invocation fails before process start and identifies the rejected metadata field.
- Failure proves: The manifest contract is not enforced.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T025. Missing prepared-tool artifact fails
- Covers: R37, AC9, E10
- Level: integration
- Fixture/setup: current manifest that points to an entrypoint artifact that does not exist.
- Steps: Ask the harness to invoke it.
- Expected result: Invocation fails before process start with artifact-missing diagnostic.
- Failure proves: The harness can run or trust incomplete prepared-tool roots.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T026. Prepared-tool execution does not mutate global state
- Covers: R36
- Level: integration
- Fixture/setup: environment snapshot for PATH, user profile-related variables, global .NET tool paths, and repo output locations.
- Steps: Run a minimal prepared-tool command and compare snapshots.
- Expected result: No user PATH, global .NET tool path, user profile state, or repo output is mutated.
- Failure proves: The optimization has unsafe side effects.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs`

TTO-T027. Prepared-tool path is not a public script option
- Covers: R6, R7, R38
- Level: contract
- Fixture/setup: public script files under `scripts/`.
- Steps: Scan public corpus wrappers for options such as `PreparedToolPath`, `UseExistingToolBuild`, or equivalent first-slice public prepared-tool bypasses.
- Expected result: No public prepared-tool option exists.
- Failure proves: The test-internal optimization changed the public wrapper contract.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/PublicWrapperContractTests.cs`

### Release Evidence And Full Validation

TTO-T028. ReleaseEvidence command remains runnable and selects expected tests
- Covers: R39, R40, R43
- Level: integration
- Fixture/setup: categorized Corpus tests.
- Steps: Run or dry-run the documented `TestCategory=ReleaseEvidence` command and verify expected release-evidence tests are selected/runnable.
- Expected result: ReleaseEvidence remains explicit and available.
- Failure proves: Tiering removed or hid release evidence.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`

TTO-T029. Benchmark tests use Benchmark or ReleaseEvidence purpose categories
- Covers: R41, E3
- Level: contract
- Fixture/setup: Corpus benchmark-related tests.
- Steps: Inspect benchmark test categories.
- Expected result: Benchmark evidence tests use `Benchmark`, `ReleaseEvidence`, or both, and are not marked `Fast` merely because they are quick.
- Failure proves: Benchmark purpose is being confused with runtime cost.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`

TTO-T030. Visual and manual evidence stay outside fast defaults without rationale
- Covers: R14, R42
- Level: contract
- Fixture/setup: Corpus visual/manual evidence tests.
- Steps: Inspect `Visual` and `ManualEvidence` tests for fast-category rationale and test fast-filter selection.
- Expected result: Visual/manual evidence tests are excluded from fast defaults unless a specific accepted rationale exists.
- Failure proves: Evidence-only checks can unexpectedly slow the inner loop.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`

TTO-T036. scripts/ci.ps1 remains broad closeout validation
- Covers: R4, R39, R40, R43, AC11, E6
- Level: smoke
- Fixture/setup: repository scripts and, for milestone closeout, a local Windows/.NET environment.
- Steps: Verify docs still name `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` as broad closeout; run it during M5/M6 unless an environment limitation is recorded.
- Expected result: Full validation remains available and is not replaced by fast filters.
- Failure proves: The project lost broad closeout coverage.
- Automation location: documentation contract test plus M5/M6 validation notes.

TTO-T040. First slice does not split hosted CI behavior
- Covers: R4, R5
- Level: contract
- Fixture/setup: CI workflow files and `scripts/ci.ps1`.
- Steps: Inspect changed files for hosted CI job splitting or replacement of the broad closeout script.
- Expected result: Hosted CI split is absent unless a later accepted artifact changes scope.
- Failure proves: The implementation exceeded the approved first-slice boundary.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ScopeGuardTests.cs` or review validation.

### Parallelism And Shared State

TTO-T031. Shared-state inventory is recorded and assembly serialization remains
- Covers: R44, R45, R46, R47, E4
- Level: contract
- Fixture/setup: `tests/VeloFile.Corpus.Tests/MSTestSettings.cs`, shared-state inventory note or test metadata.
- Steps: Verify assembly-wide `DoNotParallelize` remains in the first slice, and tests that mutate process/global state, environment variables, shared scratch paths, user profile state, or public script state are identified.
- Expected result: Parallelization is deferred and future-safe unique temp root usage is documented or implemented where practical.
- Failure proves: The first slice changed parallelism without the required safety proof.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ParallelismBoundaryTests.cs`

### Runtime Reporting

TTO-T032. Runtime report records baseline and optimized timings
- Covers: R48, R49, R50, R51, R52, AC10, E5, E6
- Level: contract
- Fixture/setup: runtime report under `docs/changes/2026-05-16-test-runtime-optimization/runtime/`.
- Steps: Validate the report includes baseline Corpus runtime, optimized fast/contract runtime, optimized script-smoke runtime, top 10 slowest tests, and full `scripts/ci.ps1` status.
- Expected result: Required runtime fields exist and values are measured or explicitly unavailable with reason.
- Failure proves: Runtime improvement claims are not reviewable.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/RuntimeReportTests.cs`

TTO-T033. Runtime report metadata and privacy are controlled
- Covers: R53, R54
- Level: contract
- Fixture/setup: runtime report.
- Steps: Validate command, configuration, filter, date, and environment assumptions are present; verify wording does not present local duration as universal.
- Expected result: Report is interpretable and does not overclaim machine-specific timings.
- Failure proves: Runtime evidence is misleading.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/RuntimeReportTests.cs`

TTO-T034. Runtime report uses structured slow-test source or records fallback
- Covers: R55
- Level: contract
- Fixture/setup: TRX output or documented fallback evidence.
- Steps: Validate top slow tests come from TRX or similarly structured output; if not, verify fallback method and limitation are recorded.
- Expected result: Slow tests are identified by test name or limitation is explicit.
- Failure proves: Slow-test evidence is guessed and not actionable.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/RuntimeReportTests.cs`

TTO-T035. Missed runtime targets are recorded without deleting coverage
- Covers: R56, R57, R58, R59, R60
- Level: contract
- Fixture/setup: runtime report and category/smoke inventory.
- Steps: Compare measured timings to SHOULD targets and verify misses include measured evidence and follow-up rationale.
- Expected result: Misses are documented and do not remove release/script coverage.
- Failure proves: Timing targets are being used to weaken validation.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/RuntimeReportTests.cs`

### Scope And Privacy Guards

TTO-T037. First slice does not change production behavior
- Covers: R1, R2, R3, AC1, AC12
- Level: contract
- Fixture/setup: changed-file list for the implementation slice.
- Steps: Verify changed files are limited to test projects, test harness helpers, scripts, validation docs, architecture/plan/test-spec artifacts, or `tools/VeloFile.Corpus` test seams.
- Expected result: No production App/Core/Windows behavior change is needed to satisfy this spec.
- Failure proves: The implementation exceeded the approved validation-only scope.
- Automation location: review validation or `tests/VeloFile.Corpus.Tests/TestRuntime/ScopeGuardTests.cs` when feasible.

TTO-T039. Generated corpus artifacts do not escape scratch roots
- Covers: R25, R26, E8
- Level: integration
- Fixture/setup: repository output snapshot and scratch root.
- Steps: Run optimized in-process, prepared-tool, and smoke paths selected for the milestone.
- Expected result: Generated corpus data, reports, diagnostics, benchmarks, and build artifacts remain scratch/temp-local unless explicitly tracked fixtures are used.
- Failure proves: Optimization created repository-side artifact leaks.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ScratchRootBoundaryTests.cs`

## Fixtures And Data

- Use unique test-owned scratch roots for all contract, script smoke, hermetic wrapper, and prepared-tool tests.
- Use synthetic category metadata fixtures for invalid category cases instead of making committed tests intentionally invalid.
- Use minimal corpus profiles/scopes for smoke tests; full profile and scope matrices belong to `ReleaseEvidence`.
- Use `.velofile-prepared-tool.json` under the prepared-tool root for prepared-tool tests. Minimum fields: schema version, tool kind, setup id, configuration, target framework, entrypoint, and created UTC.
- Runtime evidence belongs under `docs/changes/2026-05-16-test-runtime-optimization/runtime/`.
- Private local usernames, private profile paths, secrets, tokens, credentials, and machine-specific private data must not be committed in fixtures, manifests, or runtime reports.

## Mocking And Stubbing Policy

- Public script smoke and hermetic wrapper tests must execute the public script boundary; do not mock those wrappers.
- Category inventory, command documentation, release-tier selection, and runtime-report shape may use static/contract inspection.
- Prepared-tool invalid cases may use fake prepared-tool roots and manifests because the claim is pre-invocation validation.
- In-process corpus contract tests may use test-visible command seams when the claim is output contract behavior rather than public wrapper behavior.
- Do not mock `scripts/ci.ps1` for closeout claims. If it cannot run locally, record the limitation instead of claiming broad validation.

## Migration Or Compatibility Tests

- Category migration must reject legacy category names until they are mapped to the accepted taxonomy.
- Public corpus wrapper command-line compatibility is protected by TTO-T027 and script smoke tests TTO-T012 through TTO-T016.
- Moving assertions away from wrappers must preserve observable claims through TTO-T010 and TTO-T038.
- Hosted CI splitting and public prepared-tool options are compatibility changes and must remain absent in this first slice.

## Observability Verification

- Category inventory diagnostics should identify the offending test and category issue.
- Prepared-tool diagnostics should identify the rejected condition without exposing unrelated private local paths.
- Runtime reports must include command, filter, configuration, date, measured duration, environment assumptions, top slow tests, and full CI status.
- Public wrapper smoke failures must be reported as public wrapper failures, not as generic in-process contract failures.

## Security And Privacy Verification

- Scratch-root and prepared-tool tests must prove no repository or user-profile mutation outside the assigned root.
- Prepared-tool manifests must not record raw local usernames, private profile paths, secrets, tokens, credentials, or machine-specific private data.
- Runtime reports must avoid private local paths when relative or sanitized paths are enough.
- Diagnostics may name allowed roots or rejected conditions but should not publish unrelated private paths.

## Performance Checks

Performance checks are review evidence, not universal guarantees.

- M1 records baseline `VeloFile.Corpus.Tests` runtime, using the measured `5 m 49 s` run unless a fresher baseline is captured.
- M6 records optimized fast/contract runtime, optimized Corpus contract runtime, optimized script-smoke runtime, top 10 slow tests, and full `scripts/ci.ps1` status.
- The SHOULD targets are under 10 seconds for focused contract, under 30 seconds for Corpus fast/contract when already built, materially lower script-smoke runtime than the multi-minute baseline, and no worse full local CI.
- Missed targets require measured evidence and follow-up rationale; they do not justify deleting coverage.

## Manual QA Checklist

Manual evidence is acceptable only for runtime interpretation or environment limitations.

- Confirm `scripts/ci.ps1` was run for closeout, or record why the local environment could not run it.
- Confirm runtime report timings were captured on the same machine/configuration when used for before/after comparison.
- Confirm no committed evidence includes private local paths, usernames, secrets, or machine-specific private data.
- Confirm release-evidence and script-smoke coverage was not removed solely to meet timing targets.

## What Not To Test

- Do not test production App/Core/Windows behavior for this feature; production behavior is out of scope.
- Do not test hosted CI job splitting; it is explicitly deferred.
- Do not test public `-PreparedToolPath` or `-UseExistingToolBuild` options; they must not exist in the first slice.
- Do not test source-hash or cross-run prepared-tool cache invalidation unless a later accepted change introduces cross-run reuse.
- Do not test broad parallel execution by removing assembly-wide `DoNotParallelize`; that is deferred to a later slice.
- Do not use timing-only assertions as hard pass/fail gates across machines.

## Uncovered Gaps

None blocking. Exact helper class names, chosen contributor guidance file, and prepared-tool setup boundary are implementation details for M1-M4 as long as the tests above remain traceable.

## Next Artifacts

- Implement M1: category taxonomy, baseline runtime, and local command documentation.
- Run focused M1 validation before proceeding to M2.

## Follow-On Artifacts

None yet.

## Readiness

This test spec is active and ready for M1 implementation. It is not branch-ready or PR-ready; implementation, code review, runtime evidence, verification, and PR handoff remain pending.
