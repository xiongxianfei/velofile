# Review Resolution

## Status

resolved; M4 closed by code-review-r6; ready for M5 implementation

## Findings

### TRO-CR2: Prepared-tool publish can mutate repository `bin`/`obj` outputs

- Source review: [code-review-r5](reviews/code-review-r5.md)
- Status: resolved
- Required outcome: prepared-tool setup must not write repository build outputs outside the assigned scratch/temp root, and TTO-T026 must directly prove that setup plus invocation preserve repo-side build output boundaries.
- Safe resolution path:
  - Keep the fix scoped to M4 test harness and tests.
  - Either prepare the tool from a scratch source copy, or pass MSBuild properties that redirect `BaseIntermediateOutputPath`, `IntermediateOutputPath`, `OutputPath`, and related publish/build outputs for the Corpus project and its project references into the test-owned scratch root.
  - Move the repo-output snapshot in `PreparedTool_execution_does_not_mutate_global_state_or_repo_outputs` so it is captured before `PreparedCorpusToolHarness.Prepare(context)`.
  - Expand repo-output snapshot coverage to include relevant repository `bin` and `obj` paths or add a targeted assertion proving their timestamps/contents do not change during prepared-tool setup and invocation.
  - Rerun M4 focused validation and return M4 to code review.
- Resolution:
  - Changed `PreparedCorpusToolHarness.Prepare` to copy `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows` into a scratch-owned source tree before publishing.
  - Published from the scratch source project into the scratch prepared-tool root, with `DOTNET_CLI_HOME`, NuGet caches, `TEMP`, and `TMP` pointed at scratch-owned directories.
  - Moved the repo-output snapshot before prepared-tool setup.
  - Expanded `RepoOutputSnapshot` to include `bin` and `obj` paths for `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows`, with file length and last-write-time fingerprints.
  - Added tests proving snapshot mutation detection and scratch-owned prepared source/output roots.
- Validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreparedTool&TestCategory=Contract"` passed: 11 tests, about 29 seconds.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"` passed: 6 tests, about 57 seconds.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- Closeout:
  - [code-review-r6](reviews/code-review-r6.md) approved M4 with no findings.
  - M4 is closed.

### TRO-CR1: `ReleaseEvidence` + `Fast` rationale can be empty

- Source review: [code-review-r1](reviews/code-review-r1.md)
- Status: resolved
- Required outcome: category inventory validation must reject `ReleaseEvidence` + `Fast` when the recorded rationale is missing, empty, or whitespace.
- Safe resolution path:
  - Change the inventory descriptor to carry rationale text or an equivalent non-empty-rationale state.
  - Update reflection extraction so class-level and method-level `ReleaseEvidenceFastRationaleAttribute` values must be non-empty after trimming.
  - Add a focused test proving empty or whitespace rationale fails.
  - Keep the fix scoped to M1 category inventory tests and helper code.
  - Rerun M1 focused validation.
- Resolution:
  - Changed `CorpusTestCategoryDescriptor` to preserve `ReleaseEvidenceFastRationale` text instead of only a boolean.
  - Updated category inventory validation to require a non-empty rationale after trimming for `ReleaseEvidence` + `Fast`.
  - Added direct empty/whitespace rationale tests.
  - Added reflection-path coverage for class-level whitespace rationale, method-level non-empty rationale, and method-level whitespace override over a class-level rationale.
  - Preserved method-level override semantics.
- Validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed: 36 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"` passed: 36 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build 0 warnings/0 errors; Core 168, App 149, Windows 52, Corpus 51 tests passed.
- Closeout:
  - [code-review-r2](reviews/code-review-r2.md) approved M1 with no findings.
  - M1 is closed.

### TRO-SR1: Prepared-tool staleness is a required failure mode but is undefined

- Source review: [spec-review-r1](reviews/spec-review-r1.md)
- Status: resolved
- Required outcome: define a testable stale-tool contract in `specs/test-runtime-optimization.md` or remove/defer stale-tool rejection from the first-slice `MUST` behavior.
- Resolution plan:
  - Decide whether first-slice prepared-tool staleness means a missing expected artifact, missing current-run marker, source hash mismatch, build marker older than setup invocation, or no stale detection in slice one.
  - Update R37, the error/boundary section, and AC8 or add a new acceptance criterion so the behavior is testable.
- Resolution:
  - Kept stale-tool rejection in first-slice scope.
  - Defined first-slice prepared-tool currency through a current-run prepared-tool manifest inside the allowed scratch/temp root.
  - Updated R37 so prepared-tool rejection covers missing root, outside-root path, missing manifest, different setup invocation, wrong manifest-declared tool kind/configuration/target framework/entrypoint, and missing expected artifact.
  - Deferred source-hash and cross-run cache staleness detection until cross-run prepared-tool reuse is introduced.
  - Added acceptance coverage for outside-root rejection and stale/invalid manifest/artifact rejection.
  - Preserved the internal-to-tests prepared-tool boundary.
- Validation:
  - `git diff --check -- specs\test-runtime-optimization.md docs\changes\2026-05-16-test-runtime-optimization\review-resolution.md`
  - `rg -n "stale|prepared-tool manifest|setup identifier|AC8|AC9|R37" specs\test-runtime-optimization.md`
- Closeout:
  - Spec review R2 approved the amended spec with no findings.
  - `specs/test-runtime-optimization.md` status is `approved`.

### TRO-AR1: Architecture review is downstream of an unresolved spec-review gate

- Source review: [architecture-review-r1](reviews/architecture-review-r1.md)
- Status: resolved
- Required outcome: record an approving spec re-review for the amended test runtime optimization spec, or revise lifecycle metadata so downstream architecture work is not treated as approved before the spec gate is closed.
- Resolution plan:
  - Run and record `spec-review-r2` against the amended `specs/test-runtime-optimization.md`.
  - If approved, update the spec status to `approved`, record the review log entry, and rerun architecture review.
  - If not approved, keep architecture blocked and update architecture/ADR only after the spec is corrected.
- Resolution:
  - Added [spec-review-r2](reviews/spec-review-r2.md), with status `approved` and no findings.
  - Updated `specs/test-runtime-optimization.md` status to `approved`.
  - Added the spec approval to `review-log.md`.
  - The upstream spec-review gate that blocked architecture review is closed.
- Follow-up:
  - `architecture-review-r1` remains a historical blocked review record.
  - [architecture-review-r2](reviews/architecture-review-r2.md) approved the canonical architecture update and ADR 0011.

### TRO-PL1: Milestone validation can pass against stale builds or invalid filters

- Source review: [plan-review-r1](reviews/plan-review-r1.md)
- Status: resolved
- Required outcome: milestone validation must include a build-producing command for every milestone that changes code or tests, and all test filters must use valid, explicit MSTest/VSTest filter syntax without introducing non-taxonomy categories.
- Resolution plan:
  - Revise M1-M6 validation commands so code/test-changing milestones run `dotnet test ... -c Debug --filter ...` without `--no-build` before timing-focused `--no-build` commands.
  - Replace M4's ambiguous `PreparedTool|TestCategory=Contract` filter with an accepted category filter or an explicit non-category filter such as `FullyQualifiedName~PreparedTool`.
  - Keep `PreparedTool` out of the category taxonomy unless the spec is revised.
- Resolution:
  - Added a build-producing validation rule to `docs/plans/2026-05-16-test-runtime-optimization.md`.
  - Updated M1-M6 validation commands so build-producing `dotnet test ... -c Debug --filter ...` commands precede timing-focused `--no-build` commands.
  - Replaced M4's ambiguous filter with `FullyQualifiedName~PreparedTool&TestCategory=Contract`.
  - Kept `PreparedTool` out of the accepted category taxonomy.

### TRO-PL2: M2 can close with a public wrapper coverage gap before M3 replaces it

- Source review: [plan-review-r1](reviews/plan-review-r1.md)
- Status: resolved
- Required outcome: the plan must prevent any milestone from closing after wrapper coverage is removed but before replacement script smoke/hermetic coverage is in place.
- Resolution plan:
  - Revise M2 so it only adds in-process/low-overhead contract tests and does not remove or shrink existing public wrapper coverage.
  - Let M3 replace or reduce wrapper coverage only after minimal `CorpusScript` + `Smoke` and hermetic wrapper isolation evidence exists.
  - Alternatively combine M2 and M3 into one milestone if contract migration and replacement script smoke coverage must be reviewed together.
- Resolution:
  - Added a public-wrapper coverage guard to the plan.
  - Added a wrapper coverage migration ledger.
  - Revised M2 so it adds lower-overhead contract tests while preserving existing public wrapper coverage.
  - Revised M2 closeout to forbid removing, disabling, shrinking, or retagging public wrapper coverage unless equivalent smoke and hermetic evidence is already present.
  - Revised M3 closeout to require minimal public script smoke, common hermetic wrapper isolation, accepted taxonomy categories, and ledger updates before old broad wrapper tests may be reduced or replaced.
- Validation:
  - `git diff --check -- docs\plans\2026-05-16-test-runtime-optimization.md docs\changes\2026-05-16-test-runtime-optimization\review-resolution.md`
  - `rg -n "Build-Producing Validation Rule|Public-Wrapper Coverage Guard|Wrapper Coverage Migration Ledger|FullyQualifiedName~PreparedTool|--no-build" docs\plans\2026-05-16-test-runtime-optimization.md`
- Closeout:
  - [plan-review-r2](reviews/plan-review-r2.md) approved the revised plan with no findings.
