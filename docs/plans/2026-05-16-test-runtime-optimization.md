# Test Runtime Optimization Execution Plan

## Status

active

## Purpose / Big Picture

This plan turns the approved test runtime optimization contract into small reviewable implementation slices. The goal is to reduce the local validation feedback loop by separating fast contract checks, public script smoke checks, and release-evidence checks without removing release-readiness coverage.

The first implementation slice keeps `scripts/ci.ps1` as the broad closeout command, keeps prepared-tool execution internal to tests, preserves public corpus wrapper contracts, and defers CI job splitting, source-hash cross-run caching, and assembly-wide parallelization.

## Source Artifacts

- Proposal: `docs/proposals/2026-05-16-test-runtime-optimization.md`
- Spec: `specs/test-runtime-optimization.md`
- Spec reviews: `docs/changes/2026-05-16-test-runtime-optimization/reviews/spec-review-r1.md`, `docs/changes/2026-05-16-test-runtime-optimization/reviews/spec-review-r2.md`
- Architecture: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Architecture reviews: `docs/changes/2026-05-16-test-runtime-optimization/reviews/architecture-review-r1.md`, `docs/changes/2026-05-16-test-runtime-optimization/reviews/architecture-review-r2.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Test spec review: `docs/changes/2026-05-16-test-runtime-optimization/reviews/test-spec-review-r1.md`
- Project map: `docs/project-map.md`

## Context and Orientation

Current slow path:

- `tests/VeloFile.Corpus.Tests/CorpusToolingSmokeTests.cs` runs many PowerShell wrapper calls through `RunScript`.
- `scripts/Invoke-CorpusTool.ps1` copies `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows` into scratch source directories and runs `dotnet publish` for each wrapper invocation.
- `tests/VeloFile.Corpus.Tests/MSTestSettings.cs` applies assembly-wide `DoNotParallelize`; this plan identifies shared-state constraints but does not remove assembly-wide serialization in the first slice.
- Current test categories such as `Benchmarks`, `Compatibility`, `PreviewContract`, `PreviewProviders`, `Thumbnails`, `Diagnostics`, `Release`, and `UiContracts` are not the accepted taxonomy from the new spec.

Relevant boundaries:

- Production `src/VeloFile.App`, `src/VeloFile.Core`, and `src/VeloFile.Windows` behavior is out of scope.
- Public wrapper scripts remain supported: `scripts/generate-corpus.ps1`, `scripts/run-compat-corpus.ps1`, `scripts/run-preview-corpus.ps1`, `scripts/run-benchmarks.ps1`, and `scripts/run-diagnostics-conformance.ps1`.
- `tools/VeloFile.Corpus` may gain test-visible seams only to support the approved contract claims; it must not become a full library rewrite in this slice.
- Runtime evidence belongs under the change record, with private local paths avoided or sanitized when reported.

## Non-goals

- Do not split hosted CI jobs in this first slice.
- Do not expose public script options such as `-PreparedToolPath` or `-UseExistingToolBuild`.
- Do not remove corpus, compatibility, preview, diagnostics, benchmark, visual, manual, or release-evidence validation.
- Do not remove assembly-wide `DoNotParallelize` in this first slice.
- Do not introduce source-hash or cross-run prepared-tool caching.
- Do not change production App/Core/Windows behavior.
- Do not claim universal runtime guarantees from one local machine measurement.

## Requirements Covered

| Requirements | Plan coverage |
|---|---|
| R1-R7 | Overall scope, CI stability, public wrapper compatibility, and internal-only prepared-tool boundary across all milestones. |
| R8-R15 | M1 category taxonomy and inventory enforcement. |
| R16-R21 | M1 contributor command documentation. |
| R22-R26 | M2 corpus contract test split and scratch-root-only output rules. |
| R27-R33 | M3 public script smoke and hermetic wrapper coverage. |
| R34-R38 | M4 prepared-tool harness and current-run manifest validation. |
| R39-R43 | M5 release-evidence preservation and explicit expensive-tier commands. |
| R44-R47 | M1 shared-state inventory; assembly-wide parallelization remains deferred. |
| R48-R55 | M1 baseline runtime evidence and M6 optimized runtime reports. |
| R56-R60 | M6 measured targets and follow-up rationale when a target is missed. |

## Build-Producing Validation Rule

Any milestone that changes production code, test code, test categories, validation tooling, wrapper scripts, or documentation that is validated by tests must run at least one build-producing `dotnet test ... -c Debug --filter ...` command before any timing-focused `--no-build` command is recorded as evidence.

`--no-build` commands may be used only for inner-loop runtime measurement after a successful build-producing command has run in the same validation sequence.

All test filters must use explicit VSTest/MSTest syntax. Non-taxonomy words must not be used as category filters. If a filter targets a class or method name, it must use `FullyQualifiedName~...` or `Name~...`.

## Public-Wrapper Coverage Guard

No milestone may remove, disable, shrink, retag, or replace public script-wrapper coverage unless equivalent replacement coverage is already present in the same milestone or a prior closed milestone.

Moving an assertion from public wrapper execution to in-process or prepared-tool execution is allowed only when the same observable claim remains covered, or when separate `CorpusScript` + `Smoke` public-wrapper coverage and hermetic wrapper evidence already exist.

Until M3 closes, M2 must preserve existing public wrapper coverage.

### Wrapper Coverage Migration Ledger

| Existing wrapper-backed claim | Existing test/script | M2 action | Replacement evidence | Replacement milestone | M3 status |
|---|---|---|---|---|---|
| Generate corpus profile entrypoint works | `CorpusToolingSmokeTests` / `generate-corpus.ps1` | preserved in M2 | minimal `generate-corpus.ps1` smoke | M3 | covered by `HermeticWrapper_scratch_publish_isolation_and_path_safety` |
| Compatibility runner entrypoint works | `CorpusToolingSmokeTests` / `run-compat-corpus.ps1` | preserved in M2 | minimal `run-compat-corpus.ps1` smoke | M3 | covered by `Compat_public_script_smoke_routes_and_writes_representative_output` |
| Preview runner entrypoint works | `CorpusToolingSmokeTests` / `run-preview-corpus.ps1` | preserved in M2 | minimal `run-preview-corpus.ps1` smoke | M3 | covered by `Preview_public_script_smoke_routes_and_writes_representative_output` |
| Benchmark wrapper entrypoint works | `CorpusToolingSmokeTests` / `run-benchmarks.ps1` | preserved in M2 | minimal `run-benchmarks.ps1` smoke, if in scope | M3 | covered by `Benchmark_public_script_smoke_routes_and_writes_representative_output` |
| Diagnostics conformance wrapper works | `CorpusToolingSmokeTests` / `run-diagnostics-conformance.ps1` | preserved in M2 | minimal diagnostics smoke, if in scope | M3 | covered by `Diagnostics_public_script_smoke_routes_and_writes_representative_output` |
| Scratch publish does not write outside scratch root | `CorpusToolingSmokeTests` / `Invoke-CorpusTool.ps1` | preserved in M2 | common hermetic wrapper isolation test | M3 | covered by `HermeticWrapper_scratch_publish_isolation_and_path_safety` |

## Milestones

### M1. Category Taxonomy, Baseline Runtime, and Local Commands

- Milestone state: closed
- Goal: establish the validation category contract before moving tests between tiers.
- Requirements: R1-R21, R44-R45, R48, AC1-AC3
- Files/components likely touched:
  - `tests/VeloFile.Corpus.Tests/`
  - `docs/changes/2026-05-16-test-runtime-optimization/runtime/`
  - contributor validation guidance, if an existing document is selected by the test spec
- Dependencies:
  - approved spec and architecture
  - matching test spec before implementation
- Tests to add/update:
  - category inventory test rejects missing categories
  - category inventory test rejects unknown category names
  - category inventory test rejects `ReleaseEvidence` + `Fast` without rationale
  - category inventory test rejects `CorpusScript` without `Smoke` or `ReleaseEvidence`
- Implementation steps:
  - record a fresh or accepted baseline `VeloFile.Corpus.Tests` runtime before optimization
  - define accepted category constants for `Fast`, `Contract`, `Smoke`, `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence`
  - annotate all current `VeloFile.Corpus.Tests` tests with accepted categories
  - document the fast, corpus contract, corpus script-smoke, release-evidence, and full closeout commands
  - identify tests with process/global/shared-state constraints without removing assembly-wide serialization
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory"`
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"`
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"`
  - `git diff --check`
- Expected observable result:
  - every Corpus test has accepted category metadata
  - fast/contract filter excludes expensive tiers by default
  - baseline runtime evidence exists for later comparison
- Commit message: `M1: Add corpus validation category contract`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated with baseline runtime
  - milestone committed
- Risks:
  - category migration can misclassify evidence purpose
  - old category names may remain in tests and silently bypass filters
- Rollback/recovery:
  - revert category annotations, inventory test, and documentation changes
  - keep baseline runtime evidence as diagnostic context if useful

### M2. Corpus Contract Tests Without Wrapper Cost

- Milestone state: closed
- Goal: move assertions that do not need public wrapper behavior into in-process or low-overhead corpus contract tests.
- Requirements: R22-R26, R39-R43, AC4, AC7
- Files/components likely touched:
  - `tests/VeloFile.Corpus.Tests/CorpusToolingSmokeTests.cs`
  - possible new files under `tests/VeloFile.Corpus.Tests/`
  - possible test-visible seam in `tools/VeloFile.Corpus/Program.cs`
  - `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj`
- Dependencies:
  - M1 closed
  - test spec defines which current assertions are contract versus script smoke or release evidence
  - existing public wrapper coverage remains intact until M3 replacement smoke and hermetic coverage closes
- Tests to add/update:
  - contract tests prove report shapes, manifests, redaction, profile decisions, scope classification, and release classification without launching public PowerShell wrappers when wrapper behavior is not the claim
  - scratch-root tests prove in-process or low-overhead contract checks do not write under the repository
- Implementation steps:
  - add in-process or low-overhead contract tests beside existing public wrapper tests
  - introduce a small test helper for running corpus command logic without public wrapper execution where allowed
  - keep contract tests categorized as `Contract` and optionally `ReleaseEvidence` when they contribute release readiness
  - avoid marking tests as `CorpusScript` merely because they validate output that can be produced without a wrapper
  - update the wrapper coverage migration ledger to show each wrapper-backed claim is preserved in M2
  - do not delete, shrink, disable, or retag existing public wrapper coverage in M2 unless equivalent `CorpusScript` + `Smoke` and hermetic wrapper evidence is added in M2
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Contract|TestCategory=Fast"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract|TestCategory=Fast"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory|TestCategory=Contract"`
  - `git diff --check`
- Expected observable result:
  - corpus contract checks run without repeated public PowerShell wrapper invocation
  - contract output remains scratch/temp-local
  - existing public wrapper coverage remains intact until M3 installs replacement smoke and hermetic coverage
- Commit message: `M2: Split corpus contract tests from wrapper execution`
- Milestone closeout:
  - validation passed
  - no public wrapper coverage was removed, disabled, shrunk, or retagged unless equivalent `CorpusScript` + `Smoke` and hermetic wrapper evidence was added in M2
  - wrapper coverage migration ledger updated
  - existing public script-wrapper confidence remains intact until M3 replacement smoke coverage closes
  - progress updated
  - decision log updated if needed
  - validation notes updated with contract runtime
  - milestone committed
- Risks:
  - in-process checks may drift from public wrapper behavior
  - test-visible seams could become a premature tool rewrite
- Rollback/recovery:
  - restore affected tests to script wrapper execution
  - keep any reusable scratch-root helpers only if they remain covered and useful

### M3. Public Script Smoke and Hermetic Wrapper Coverage

- Milestone state: closed
- Goal: preserve public wrapper confidence with a minimal smoke set and one common hermetic scratch-publish isolation test.
- Requirements: R27-R33, AC5-AC7
- Files/components likely touched:
  - `tests/VeloFile.Corpus.Tests/CorpusToolingSmokeTests.cs`
  - `scripts/Invoke-CorpusTool.ps1`
  - public corpus scripts under `scripts/`
- Dependencies:
  - M1 closed
  - M2 closed or explicitly coordinated so contract and script claims do not duplicate work
  - M3 blocks any reduction of existing public wrapper coverage introduced by M2 contract migration
- Tests to add/update:
  - one common hermetic wrapper isolation test proves scratch root, source copy/publish behavior, and no repo-side generated output
  - one minimal `CorpusScript` + `Smoke` test for each public script family in scope
  - release/full matrix tests are categorized as `ReleaseEvidence`, not default script smoke
- Implementation steps:
  - reduce public script smoke to entrypoint routing and representative output
  - keep the hermetic publish test on the shared wrapper path
  - ensure wrapper smoke failures are reported as public wrapper failures
  - preserve existing public script command-line behavior
  - update the wrapper coverage migration ledger to identify which older broad wrapper tests may now be reduced or replaced
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~HermeticWrapper"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory|TestCategory=CorpusScript"`
  - `git diff --check`
- Expected observable result:
  - public scripts still have representative smoke coverage
  - hermetic scratch-publish behavior remains proven without every assertion paying that cost
- Commit message: `M3: Keep corpus script smoke and hermetic wrapper coverage`
- Milestone closeout:
  - validation passed
  - minimal public script smoke coverage exists for each public script family in scope
  - common hermetic scratch-publish isolation test passes
  - replacement smoke coverage is categorized with accepted taxonomy categories such as `CorpusScript` and `Smoke`
  - wrapper coverage migration ledger identifies which older broad wrapper tests may now be reduced or replaced
  - no public wrapper failure is hidden by replacing wrapper execution only with in-process or prepared-tool execution
  - progress updated
  - decision log updated if needed
  - validation notes updated with script-smoke runtime
  - milestone committed
- Risks:
  - smoke tests may become too thin to catch wrapper routing regressions
  - hermetic test may remain expensive if it exercises too much matrix behavior
- Rollback/recovery:
  - re-expand smoke coverage for any script family whose representative test proves insufficient
  - restore previous wrapper tests if public script confidence is weakened

### M4. Test-Internal Prepared Tool Harness

- Milestone state: closed
- Goal: add a prepared corpus tool path for tests that need process execution without hermetic scratch publishing on every assertion.
- Requirements: R34-R38, AC8-AC9
- Files/components likely touched:
  - `tests/VeloFile.Corpus.Tests/`
  - optional helper under `tests/` or `tools/`
  - no public script option changes
- Dependencies:
  - M1 closed
  - M2/M3 claim split is stable enough to identify process tests that can use a prepared tool
- Tests to add/update:
  - valid current-run prepared tool executes successfully
  - missing root fails before invocation
  - outside-root path fails before invocation
  - missing manifest fails before invocation
  - mismatched setup id fails before invocation
  - wrong tool kind/configuration/target framework/entrypoint fails before invocation
  - missing artifact fails before invocation
  - diagnostics avoid unrelated private local paths
- Implementation steps:
  - create a test-owned prepared-tool root under an allowed scratch/temp root
  - publish or prepare the corpus tool once per test setup boundary chosen by the test spec
  - write `.velofile-prepared-tool.json` with schema version, tool kind, setup id, configuration, target framework, entrypoint, and created timestamp
  - validate the manifest and artifact before invoking the tool
  - ensure no public wrapper options are added
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreparedTool&TestCategory=Contract"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"`
  - `git diff --check`
- Expected observable result:
  - repeated process-based tests can run through a validated prepared tool without repeated scratch source copy/publish
  - invalid or stale prepared tools fail before invocation with actionable diagnostics
- Commit message: `M4: Add test-internal prepared corpus tool harness`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks:
  - prepared-tool execution could hide wrapper isolation regressions
  - manifest diagnostics could leak private local paths
- Rollback/recovery:
  - disable prepared-tool use and run affected tests through existing wrapper path
  - keep hermetic wrapper isolation unchanged as the safety net

### M5. Release-Evidence Preservation and Full Validation Commands

- Milestone state: review-requested
- Goal: prove expensive evidence remains explicit and available after tiering.
- Requirements: R39-R43, AC6-AC7, AC11
- Files/components likely touched:
  - `tests/VeloFile.Corpus.Tests/`
  - contributor validation guidance
  - `scripts/ci.ps1` only if documentation text or command discovery needs a non-behavioral update
- Dependencies:
  - M1-M4 closed
- Tests to add/update:
  - release-evidence command selects expected release-evidence tests
  - full profile/scope matrix checks are categorized as `ReleaseEvidence`
  - benchmark-related tests use `Benchmark`, `ReleaseEvidence`, or both according to purpose
  - visual/manual evidence tests remain excluded from fast defaults unless explicitly justified
- Implementation steps:
  - verify release-evidence categorization after contract and smoke split
  - preserve or add explicit release-evidence command documentation
  - confirm `scripts/ci.ps1` remains the broad closeout command and is not split in this slice
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"`
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"`
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
  - `git diff --check`
- Expected observable result:
  - release evidence remains runnable and is not silently included in the default fast path
  - full closeout validation remains available through `scripts/ci.ps1`
- Commit message: `M5: Preserve corpus release-evidence validation`
- Milestone closeout:
  - validation passed or documented environment limitation recorded
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks:
  - release-evidence filters may become too broad and slow
  - full CI can remain slow even if local contract tests improve
- Rollback/recovery:
  - restore previous release-evidence test categorization
  - keep full CI as the authority if tiered filters are uncertain

### M6. Runtime Reporting and Optimization Evidence

- Milestone state: planned
- Goal: record before/after timing and slow-test evidence so the optimization is reviewable.
- Requirements: R48-R60, AC10
- Files/components likely touched:
  - `docs/changes/2026-05-16-test-runtime-optimization/runtime/`
  - optional helper script or test utility for TRX duration extraction
  - validation guidance
- Dependencies:
  - M1 baseline captured
  - M2-M5 closed
- Tests to add/update:
  - runtime report shape or helper test if a parser/helper is introduced
  - fallback evidence note when structured TRX parsing is unavailable
- Implementation steps:
  - record optimized fast/contract runtime
  - record optimized corpus script-smoke runtime
  - record top 10 slowest tests from TRX or a documented fallback method
  - record whether full `scripts/ci.ps1` improved, stayed the same, regressed, or was not run with reason
  - record missed SHOULD-level targets with measured evidence and follow-up rationale
- Validation commands:
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"`
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Contract"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
  - `git diff --check`
- Expected observable result:
  - runtime evidence exists for baseline, optimized contract, optimized script-smoke, top slow tests, and full CI status
  - fast runs are presented as local feedback evidence, not release proof
- Commit message: `M6: Record test runtime optimization evidence`
- Milestone closeout:
  - validation passed or limitations recorded
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
- Risks:
  - timing results can vary by machine
  - parsing TRX output may be brittle
- Rollback/recovery:
  - keep manual duration evidence if structured parsing fails
  - mark timing as local review evidence only

### M7. Lifecycle Closeout

- Milestone state: planned
- Goal: complete downstream lifecycle gates after implementation milestones close.
- Requirements: all acceptance criteria
- Files/components likely touched:
  - plan progress and validation notes
  - change explanation, verify notes, PR summary artifacts if requested by later stages
- Dependencies:
  - M1-M6 closed or explicitly removed by reviewed plan revision
  - required code-review and review-resolution cycles closed
- Tests to add/update:
  - none; this is a lifecycle-closeout milestone
- Implementation steps:
  - run `explain-change`
  - run `verify`
  - prepare PR handoff only when verification supports it
  - mark this plan done only after all required gates are complete
- Validation commands:
  - final commands determined by `verify`, with `scripts/ci.ps1` expected unless a limitation is recorded
- Expected observable result:
  - lifecycle evidence is coherent and no implementation milestone remains open
- Commit message: `M7: Close test runtime optimization lifecycle`
- Milestone closeout:
  - downstream lifecycle gates complete
  - plan outcome and retrospective filled
  - plan committed
- Risks:
  - final closeout may be attempted before review-resolution is complete
- Rollback/recovery:
  - keep plan active and name the missing gate

## Validation Plan

Focused commands:

```powershell
dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"
dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"
```

Broad closeout command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
```

Plan/document validation:

```powershell
git diff --check
rg -n "Test Runtime Optimization|TestCategory=Fast|TestCategory=Contract|ReleaseEvidence|FullyQualifiedName~PreparedTool|scripts\\ci.ps1" docs specs tests scripts tools
```

Full CI remains required before milestone closeout claims unless a limitation is explicitly recorded.

## Risks and Recovery

- Category drift: controlled by inventory tests and accepted taxonomy constants.
- Wrapper confidence loss: controlled by one common hermetic wrapper isolation test plus minimal public script smoke tests.
- Prepared-tool false confidence: controlled by current-run manifest validation and retained hermetic wrapper coverage.
- Runtime evidence overclaiming: controlled by reporting command, filter, configuration, date, and environment assumptions.
- Full CI regression: record the regression, keep `scripts/ci.ps1` authoritative, and do not claim final improvement until follow-up is defined.
- Shared-state flakiness: assembly-wide `DoNotParallelize` remains in place until a later reviewed parallel-safety slice.

Rollback is scoped to test harness, category metadata, scripts, and documentation. Production App/Core/Windows behavior is unaffected.

## Dependencies

- `plan-review` must approve or revise this plan before `test-spec`.
- Matching test spec must map the approved requirements and milestones to concrete tests before implementation.
- `scripts/ci.ps1` behavior must remain unchanged for first-slice closeout.
- Runtime measurements depend on a local Windows/.NET environment and should be treated as local evidence.

## Progress

- [x] Proposal accepted.
- [x] Spec approved.
- [x] Architecture review approved.
- [x] Plan review completed.
- [x] Test spec approved.
- [x] M1 closed.
- [x] M2 closed.
- [x] M3 closed.
- [x] M4 closed.
- [ ] M5 closed.
- [ ] M6 closed.
- [ ] M7 lifecycle closeout complete.

## Current Handoff Summary

- Current milestone: M5 release-evidence preservation and full validation commands
- Current milestone state: review-requested
- Last implemented milestone: M5 implementation handoff
- Last reviewed milestone: M4 code-review-r6
- Review status: M5 awaiting code review
- Remaining in-scope implementation milestones: M5-M6
- Next stage: `code-review` M5
- Final closeout readiness: not ready
- Reason final closeout is or is not ready: M5 code review, M6 implementation, runtime evidence, verify, and PR handoff are not complete.

## Decision Log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-16 | Keep `scripts/ci.ps1` unchanged in the first implementation slice. | Preserve broad closeout confidence while category/runtime evidence stabilizes. | Split CI immediately. |
| 2026-05-16 | Keep prepared-tool execution internal to tests. | Avoid changing public wrapper contracts before the harness path proves reliable. | Add public `-PreparedToolPath` or `-UseExistingToolBuild`. |
| 2026-05-16 | Use one common hermetic wrapper isolation test plus minimal public script smoke tests. | Preserve wrapper and scratch-root confidence without repeated full matrix publish cost. | Replace all script tests with in-process checks; keep full matrices in smoke. |
| 2026-05-16 | Defer assembly-wide `DoNotParallelize` removal. | Parallel safety needs separate evidence after categorization and shared-state inventory. | Remove assembly-wide serialization in this slice. |
| 2026-05-16 | Use `README.md` as the first contributor-facing validation tier command surface. | It is the existing build/test entry point and keeps local commands discoverable. | Add a new validation guide in M1. |
| 2026-05-16 | Replace legacy Corpus-only category names with the accepted taxonomy. | Spec R8-R15 require Corpus category inventory enforcement and reject unknown category names. | Preserve legacy aliases such as `UiContracts`, `Benchmarks`, or `Release` in `VeloFile.Corpus.Tests`. |
| 2026-05-16 | Use a narrow `InternalsVisibleTo` seam from `tools/VeloFile.Corpus` to `VeloFile.Corpus.Tests` for M2 contract checks. | The contract claim is corpus command output, not public wrapper routing, so in-process invocation avoids PowerShell and scratch publish cost while preserving wrapper tests for M3. | Expose a public prepared-tool/script option; keep every assertion behind public wrapper execution. |
| 2026-05-16 | Retarget `VeloFile.Corpus.Tests` to the Corpus tool's Windows TFM. | The Corpus tool already targets `net8.0-windows10.0.19041.0`; the test project must match to reference the tool for in-process contract checks. | Duplicate corpus logic in the test project; change the tool target framework. |
| 2026-05-16 | Use `generate-corpus.ps1` as the M3 hermetic wrapper isolation path. | It exercises the shared `Invoke-CorpusTool.ps1` scratch source copy and publish path while also serving as the minimal generate-script smoke. | Add a separate hermetic-only wrapper command; duplicate another generate smoke invocation. |
| 2026-05-16 | Use a test-owned current-run manifest for prepared-tool execution. | M4 only needs same-run prepared tool reuse; setup-id validation prevents stale cross-run reuse without adding source-hash caching. | Expose public prepared-tool script options; implement cross-run prepared-tool caching in the first slice. |
| 2026-05-16 | Require `EvidenceFastPathRationale` for Visual/ManualEvidence tests selected by `Fast` or `Contract`. | R42 allows evidence checks in fast/default filters only when their fast/contract purpose is explicit. | Let visual/manual evidence drift into fast/default filters based only on category names. |

## Surprises and Discoveries

- Existing Corpus tests use several non-taxonomy category names; M1 must migrate or wrap those names under the accepted taxonomy.
- Current broad script tests mix contract, smoke, and release-evidence claims in one class; M2 and M3 must split by validation claim, not merely by file.
- M1 category migration changes Corpus project category filters: Corpus UI contract tests now use `Contract` and/or `Visual` instead of the old `UiContracts` category, while broad unfiltered CI still runs them.
- M2's in-process contract tests can cover manifest, compatibility, preview, diagnostics, redaction, scratch-root, and release-classification contracts without creating `.velofile-tools` or invoking public PowerShell wrappers.
- The Corpus contract no-build run is now 42 selected tests in about 23 seconds locally, which meets the M2 target of staying under 30 seconds for the Corpus fast/contract run when already built.
- M3's required `FullyQualifiedName~CategoryInventory|TestCategory=CorpusScript` validation still selects release-evidence wrapper tests by design and took about 6 m 48 s locally. The smaller `CorpusScript&Smoke` tier selected 6 tests and completed in about 54 seconds.
- M4 prepared-tool tests must use a separate generated-corpus root under the scratch root. Reusing the prepared-tool root or its parent as corpus output makes the Corpus scratch-root guard correctly reject the non-empty unmarked directory.
- `ShellVisualCoherenceContractTests` is both `Visual` and `Contract`; M5 keeps it in the fast/default contract tier only with an explicit static-contract rationale.

## Validation Notes

Planning validation:

- `git diff --check -- docs\plans\2026-05-16-test-runtime-optimization.md docs\changes\2026-05-16-test-runtime-optimization\review-resolution.md`
- `rg -n "Build-Producing Validation Rule|Public-Wrapper Coverage Guard|Wrapper Coverage Migration Ledger|FullyQualifiedName~PreparedTool|--no-build" docs\plans\2026-05-16-test-runtime-optimization.md`

Implementation validation:

- M1 TDD failure:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory"` failed as expected before migration with missing categories and legacy category names such as `UiContracts`, `Benchmarks`, `Compatibility`, `PreviewContract`, `PreviewProviders`, `Thumbnails`, `Diagnostics`, and `Release`.
- M1 validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory"` passed: 6 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed: 31 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"` passed: 31 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
  - `rg -n 'TestCategory\("(Benchmarks|Compatibility|PreviewContract|PreviewProviders|Thumbnails|Diagnostics|Release|UiContracts)"\)|TestCategory\("' tests\VeloFile.Corpus.Tests` returned no legacy literal category attributes.
- M1 code review:
  - `code-review-r1` requested changes for TRO-CR1 because `ReleaseEvidence` + `Fast` rationale validation accepts attribute presence without proving the rationale text is non-empty.
- M1 review-resolution:
  - TRO-CR1 resolved by preserving rationale text in the category descriptor, requiring non-empty trimmed rationale text, and covering empty, whitespace, class-level, method-level, and override cases.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests.
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed: 36 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"` passed: 36 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build 0 warnings/0 errors; Core 168, App 149, Windows 52, Corpus 51 tests passed.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
  - `rg -n 'TestCategory\("(Benchmarks|Compatibility|PreviewContract|PreviewProviders|Thumbnails|Diagnostics|Release|UiContracts)"\)|TestCategory\("' tests\VeloFile.Corpus.Tests` returned no legacy literal category attributes.
- M1 code review rerun:
  - `code-review-r2` approved M1 with no findings and closed the milestone.
- M2 TDD failure:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CorpusContractTests|FullyQualifiedName~ScratchRootBoundaryTests|FullyQualifiedName~WrapperCoverageLedgerTests"` first failed because `VeloFile.Corpus.Tests` targeted `net8.0` while `VeloFile.Corpus` targets `net8.0-windows10.0.19041.0`, then failed because `CorpusCli` was internal and inaccessible to the test project.
- M2 validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CorpusContractTests|FullyQualifiedName~ScratchRootBoundaryTests|FullyQualifiedName~WrapperCoverageLedgerTests"` passed: 6 tests.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Contract|TestCategory=Fast"` passed: 42 tests, about 24 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract|TestCategory=Fast"` passed: 42 tests, about 23 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory|TestCategory=Contract"` passed: 42 tests, about 23 seconds test duration.
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed: 42 Corpus tests selected; Core/App/Windows reported no matching tests for this filter.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build 0 warnings/0 errors; Core 168, App 149, Windows 52, Corpus 57 tests passed. Corpus unfiltered test duration was about 5 m 46 s because M2 preserves existing public wrapper coverage until M3.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- M2 code review:
  - `code-review-r3` approved M2 with no findings and closed the milestone.
- M3 TDD failure:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"` first failed because the hermetic wrapper test incorrectly expected the scratch-copied project to have no `bin`/`obj` after `dotnet publish`; the assertion was corrected to focus on scratch source/publish existence and repo-side output isolation.
- M3 validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"` passed: 6 tests, about 54 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~HermeticWrapper"` passed: 1 test, about 10 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventory|TestCategory=CorpusScript"` passed: 26 tests, about 6 m 48 s test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~Script_smoke_cases_use_minimal_scopes"` passed: 1 test.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~WrapperCoverageLedgerTests|FullyQualifiedName~Script_smoke_cases_use_minimal_scopes"` passed after plan updates: 3 tests.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- M3 code review:
  - `code-review-r4` approved M3 with no findings and closed the milestone.
- M4 TDD failures:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreparedTool&TestCategory=Contract"` first failed because `PreparedCorpusToolContext` and `PreparedCorpusToolRunResult` did not exist.
  - The same command then failed because the initial fixture reused a non-empty scratch root for generated corpus output; the fixture was corrected to use a `velofile-corpus-run` subdirectory under the allowed scratch root.
- M4 validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreparedTool&TestCategory=Contract"` passed: 9 tests selected, about 2 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"` passed: 6 tests selected, about 56 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests selected.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- M4 code review:
  - `code-review-r5` requested changes for TRO-CR2 because the prepared-tool setup path can mutate repository `bin`/`obj` outputs and the current repo-output oracle does not prove setup-time build output isolation.
- M4 review-resolution:
  - TRO-CR2 fixed by preparing the Corpus tool from scratch-owned source copies and publishing into the scratch prepared-tool root.
  - `RepoOutputSnapshot` now covers `bin` and `obj` paths for `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows`, including file length and last-write-time fingerprints.
  - `PreparedTool_execution_does_not_mutate_global_state_or_repo_outputs` now captures the repo snapshot before `Prepare`.
  - Added `PreparedTool_prepare_uses_only_scratch_owned_source_and_output_roots` and `RepoOutputSnapshot_detects_bin_obj_file_mutation`.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreparedTool&TestCategory=Contract"` passed: 11 tests selected, about 29 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"` passed: 6 tests selected, about 57 seconds test duration.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventoryTests"` passed: 11 tests selected.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.
- M4 code re-review:
  - `code-review-r6` approved M4 with no findings and closed the milestone.
- M5 TDD failure:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~ReleaseEvidenceTierTests|FullyQualifiedName~CategoryInventoryTests"` first failed because `CorpusTestCategoryDescriptor` had no `EvidenceFastPathRationale` field for Visual/ManualEvidence tests selected by fast/default filters.
- M5 validation passed:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~ReleaseEvidenceTierTests|FullyQualifiedName~CategoryInventoryTests"` passed: 18 tests selected.
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"` passed: 10 tests selected, about 5 m 18 s test duration.
  - `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"` passed: 61 Corpus tests selected; Core/App/Windows reported no matching tests for this filter; about 53 seconds Corpus test duration.
  - `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"` passed: 61 Corpus tests selected; Core/App/Windows reported no matching tests for this filter; about 52 seconds Corpus test duration.
  - `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed: build 0 warnings/0 errors, UI contract validation passed, Core 168/App 149/Windows 52/Corpus 80 tests passed.
  - `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

## Outcome and Retrospective

Not started. Fill after implementation milestones and lifecycle closeout complete.

## Readiness

See Current Handoff Summary. M5 implementation is ready for code review, not milestone closeout or final closeout.
