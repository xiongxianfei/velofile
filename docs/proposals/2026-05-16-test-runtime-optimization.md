# Test Runtime Optimization Proposal

## Status

accepted

## Problem

VeloFile's validation suite provides useful coverage, but the current local feedback loop is too slow for everyday development. Recent measurement of the Corpus test path showed `VeloFile.Corpus.Tests` passing 37 tests in about 5 minutes and 49 seconds, with several individual tests spending tens of seconds to minutes in script/process execution, scratch source copying, and repeated `dotnet publish` work.

The problem is not that the tests are unimportant. The problem is that fast contract assertions, script-wrapper smoke checks, and release-evidence validation currently share the same broad execution path. This makes contributors pay release-evidence costs while making small changes, especially when `scripts/ci.ps1` or broad solution tests trigger the slow corpus path.

The project needs a validation strategy that preserves confidence while making the common edit-test-review loop faster and clearer.

## Goals

- Reduce local inner-loop validation time without weakening required release evidence.
- Separate fast contract tests, script smoke tests, and release/evidence tests into explicit tiers.
- Keep full `scripts/ci.ps1` available for milestone closeout and review gates.
- Reduce repeated PowerShell process launch, scratch copy, and `dotnet publish` overhead in corpus tests.
- Make slow tests visible by category and duration so future regressions are easier to diagnose.
- Keep hermetic script validation, but avoid making every corpus assertion pay the full hermetic wrapper cost.
- Preserve existing production behavior, release-readiness expectations, and validation credibility.

## Non-goals

- This proposal does not remove corpus validation.
- This proposal does not weaken compatibility, preview, diagnostics, benchmark, or release-evidence requirements.
- This proposal does not change production VeloFile behavior.
- This proposal does not replace the existing CI script in this proposal stage.
- This proposal does not make slow release-evidence tests optional for release readiness.
- This proposal does not introduce external test services or hosted infrastructure.
- This proposal does not require a full corpus tooling rewrite as the first implementation step.

## Vision fit

fits the current vision

VeloFile's vision commits to responsiveness, Windows compatibility, maintainable boundaries, and a disciplined open-source workflow. Faster validation supports that vision only if confidence remains explicit and trustworthy. This proposal improves maintainability by making the right checks easier to run at the right time, rather than encouraging contributors to skip validation because the default path is too expensive.

## Initial intent preservation

| Initial user goal | Proposal treatment | Where recorded |
|---|---|---|
| Tests are too slow and should be optimized. | in scope | Problem, Goals, Recommended direction |
| Finish the proposal before implementation. | in scope | Status, Next artifacts, Readiness |
| Preserve validation confidence while improving speed. | in scope | Goals, Non-goals, Testing and verification strategy |
| Separate fast checks from expensive corpus/release evidence. | in scope | Recommended direction, Expected behavior changes |
| Use measured evidence from the learn session. | in scope | Context, Testing and verification strategy |
| Avoid production behavior changes. | in scope | Non-goals, Architecture impact |
| Keep full CI meaningful for review gates. | in scope | Goals, Recommended direction, Rollout and rollback |

## Context

A learn session on 2026-05-16 measured the current slow path and found that corpus tests dominate broad validation runtime. The measured Corpus-only run passed 37 tests in `5 m 49 s`. The slowest tests were process/script integration tests rather than small in-process tests.

The same session found that `scripts/Invoke-CorpusTool.ps1` repeatedly creates scratch tool source trees, copies `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows`, deletes publish/bin/obj output, and runs `dotnet publish` on each invocation. `--no-build` at the outer solution test level does not materially improve wrapper-launched corpus script tests, because those scripts publish their own scratch copies.

The Corpus test assembly currently serializes all tests through assembly-level `DoNotParallelize`, even though many assertions could run independently with unique temp roots.

The current validation shape mixes at least three kinds of checks:

- fast contract tests that can run in process;
- public script smoke tests that need process execution;
- release/evidence tests that generate or aggregate broader compatibility, diagnostics, preview, benchmark, and release evidence.

Those categories should not all have the same local execution cost.

## Options considered

### Option A: Keep the validation suite unchanged

This preserves current behavior and avoids test-harness work, but it leaves contributors with a slow default path. Over time, slow feedback loops encourage skipped validation and make review-resolution work harder.

### Option B: Remove or skip slow corpus tests from normal validation

This would speed up local runs, but it risks weakening confidence and hiding release-evidence regressions. It treats slowness by discarding coverage rather than by improving test design.

### Option C: Split validation tiers and optimize corpus test execution

This keeps the coverage, but makes the cost explicit. Fast in-process contract tests become the default inner loop. A small number of process/script smoke tests preserve public-wrapper confidence. Release/evidence tests remain available for milestone, review, nightly, or release gates.

This is the recommended option.

### Option D: Fully rewrite corpus tooling as a library first

This may be valuable later, but it is larger than necessary for the first improvement. The project can get substantial speedups by tiering tests, preparing tools once, and avoiding repeated script invocations before a full tooling rewrite.

## Recommended direction

Adopt Option C: introduce explicit validation tiers and reduce repeated corpus wrapper overhead.

The project should keep the full validation path for review gates, but the normal contributor loop should favor focused in-process tests. Corpus tests should be reorganized so most assertions exercise command logic, JSON contracts, profiles, manifests, sidecars, redaction, and release classification in process. Public PowerShell wrapper tests should remain, but as smoke tests that prove representative script entrypoints, environment isolation, and scratch-root behavior.

The corpus wrapper path should avoid repeated scratch publish work where possible. For tests that need process execution but not hermetic scratch publishing, the harness should prepare or publish the corpus tool once per test class or run, then invoke that prepared tool repeatedly. At least one hermetic scratch-publish smoke test should remain for each public script family or for the common wrapper path to prove isolation still works.

The project should add test categories so developers and reviewers can choose the appropriate tier intentionally.

For the first implementation slice, `scripts/ci.ps1` should remain the broad validation command for milestone closeout and review gates. The first slice should add categories, focused local commands, corpus test tiering, test-internal prepared-tool execution, and runtime reporting. CI job splitting should be deferred until the category model and before/after measurements are accepted.

Recommended categories:

| Category | Purpose |
|---|---|
| `Fast` | Inner-loop tests expected to complete quickly. |
| `Contract` | Static/in-process contract tests for schemas, resources, state, and decisions. |
| `Smoke` | Small representative end-to-end or public-entrypoint checks. |
| `CorpusScript` | PowerShell/script-wrapper corpus tests. |
| `ReleaseEvidence` | Compatibility, diagnostics, benchmark, preview, and release-readiness evidence. |
| `Benchmark` | Performance measurements and benchmark harness checks. |
| `Visual` | UI visual evidence, sidecar, and baseline inventory checks. |
| `ManualEvidence` | Tests or records that validate manual evidence artifacts. |

The category taxonomy should be a contract, not only documentation. Every test in `VeloFile.Corpus.Tests` should have at least one accepted category. Tests may have multiple categories when appropriate, but expensive categories such as `ReleaseEvidence`, `Benchmark`, `CorpusScript`, `Visual`, and `ManualEvidence` should not be silently included in the default fast inner-loop command.

Initial local commands should be documented as part of the first slice:

```powershell
dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
```

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract"
```

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"
```

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"
```

Full milestone closeout remains:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
```

## Expected behavior changes

- Contributors can run focused fast tests for the touched area before invoking full CI.
- Corpus contract tests do not repeatedly launch PowerShell or publish scratch tool copies.
- Script-wrapper tests still prove public scripts and hermetic behavior, but in a smaller smoke set.
- Release-evidence tests remain explicit and available for milestone/release validation.
- Tiering may make release-evidence tests opt-in for local inner-loop work, but they remain part of milestone closeout, release readiness, or an explicitly documented full validation command.
- Full `scripts/ci.ps1` remains authoritative for broad validation and milestone closeout.
- Per-test duration reporting becomes part of review evidence when runtime is relevant.
- Corpus test parallelism can be restored in a later measured slice where tests do not share mutable process/global state.

## Architecture impact

Expected changes are limited to test projects, corpus tooling test harnesses, scripts, and validation documentation.

Likely touched areas:

- `tests/VeloFile.Corpus.Tests/`
- `tests/VeloFile.*.Tests/` category annotations where useful
- `scripts/Invoke-CorpusTool.ps1`
- `scripts/generate-corpus.ps1`
- `scripts/run-compat-corpus.ps1`
- `scripts/run-preview-corpus.ps1`
- `scripts/run-benchmarks.ps1`
- `scripts/run-diagnostics-conformance.ps1`
- `scripts/ci.ps1` documentation references only in the first slice; CI behavior remains unchanged initially
- optional helper under `tools/` or `tests/` for prepared corpus tool execution
- validation guidance in `AGENTS.md`, active plans, or future test-spec artifacts

No production App/Core/Windows behavior should change.

## Testing and verification strategy

Use test-first changes for the test harness itself.

Recommended proof points:

- Category filtering works and returns the expected subsets.
- A category inventory check fails when a Corpus test has no category, uses an unknown category, marks `ReleaseEvidence` as `Fast` without explicit rationale, or leaves `CorpusScript` uncategorized as `Smoke` or `ReleaseEvidence`.
- Fast/contract corpus tests run without PowerShell process execution.
- Wrapper smoke tests still execute representative public scripts.
- One common hermetic scratch-publish isolation test proves the wrapper still publishes into a scratch root, preserves scratch-root isolation, and leaves no repo-side output.
- Minimal public script smoke tests prove entrypoint routing and representative output for `generate-corpus.ps1`, `run-compat-corpus.ps1`, `run-preview-corpus.ps1`, `run-benchmarks.ps1`, and `run-diagnostics-conformance.ps1`.
- Prepared-tool paths remain internal to tests first and do not write outside their scratch root.
- Shared-state and parallel-safety constraints are identified before assembly-wide serialization is removed.
- Slow-test reporting records top test durations from TRX output.
- Broad `scripts/ci.ps1` still passes before milestone closeout.

The first implementation should record these runtime artifacts:

- baseline Corpus test runtime before changes;
- fast/contract runtime after changes;
- script-smoke runtime after changes;
- top 10 test durations after changes;
- whether full `scripts/ci.ps1` improved, stayed the same, or regressed.

Recommended measurement targets for the first implementation slice:

| Run | Target |
|---|---:|
| Focused contract test run | under 10 seconds when already built |
| Corpus fast/contract run | under 30 seconds when already built |
| Corpus script smoke run | materially lower than current multi-minute runtime |
| Full local CI | no worse than current behavior, then improved once tiering is accepted |

The proposal should not promise exact runtime reductions until the first implementation measures before/after results.

## Rollout and rollback

Roll out in small slices:

1. Add categories and document local validation commands.
2. Add category inventory enforcement for `VeloFile.Corpus.Tests`.
3. Split corpus tests into in-process contract tests and script smoke tests.
4. Add a test-internal prepared-tool execution path for tests that need process execution without scratch publishing each time.
5. Keep one common hermetic scratch-publish isolation test plus one minimal smoke test for each public corpus script family in scope.
6. Add slow-test reporting to validation notes or helper scripts.
7. Revisit assembly-wide `DoNotParallelize` in a later measured slice and replace it with narrower serialization where safe.
8. Decide later whether CI should split into fast PR and full release/nightly stages.

Rollback is straightforward: revert test harness changes and category usage. Production behavior is not affected.

## Risks and mitigations

- Risk: test tiering causes contributors to skip release-evidence validation. Mitigation: keep full CI required for milestone closeout and review gates.
- Risk: in-process tests diverge from script behavior. Mitigation: keep representative public-wrapper smoke tests and hermetic scratch-publish coverage.
- Risk: prepared-tool execution hides scratch-root isolation bugs. Mitigation: keep explicit isolation tests and one hermetic wrapper publish path.
- Risk: removing assembly-wide `DoNotParallelize` introduces flaky shared-state tests. Mitigation: categorize and measure first, then use unique temp roots and method/class-level serialization for shared environment tests in a later slice.
- Risk: categories become inconsistent. Mitigation: add a category inventory test or convention check for Corpus tests.
- Risk: runtime goals become overfit to one machine. Mitigation: report measured before/after durations without treating local numbers as universal performance claims.

## Proposal-level decisions

For the first implementation slice:

- `scripts/ci.ps1` remains the broad validation command for milestone closeout and review gates. CI splitting is deferred.
- Prepared-tool execution remains internal to the test harness. Public script options such as `-PreparedToolPath` or `-UseExistingToolBuild` are deferred.
- Hermetic wrapper validation uses one common scratch-publish isolation test plus one minimal smoke test for each public corpus script family in scope.
- Full profile and scope matrices move to `ReleaseEvidence` rather than script smoke.
- Assembly-wide `DoNotParallelize` is not removed in the first slice. The first slice categorizes tests and identifies parallel-safety constraints; narrower serialization can be implemented in a later measured slice.
- Runtime reporting is required: before/after timings and top slow tests must be recorded in validation evidence.

## Open questions

None blocking. Exact implementation names for helper classes, where to store runtime reports, and whether CI should later split into separate fast/full jobs belong in the follow-on spec and plan.

## Decision log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-16 | Recommend validation tiering and corpus harness optimization. | Measured evidence shows corpus script/process tests dominate broad validation runtime. | Keep suite unchanged; remove slow tests; full corpus tooling rewrite first. |
| 2026-05-16 | Keep full CI for review and milestone gates. | Faster local loops should not weaken final validation confidence. | Treat fast tier as sufficient for closeout. |
| 2026-05-16 | Keep `scripts/ci.ps1` unchanged for the first slice and defer CI splitting. | The category model and runtime data should stabilize before changing hosted validation behavior. | Split CI immediately. |
| 2026-05-16 | Keep prepared-tool execution internal to tests first. | Avoids changing the public script contract before the optimization proves reliable. | Add public `-PreparedToolPath` or `-UseExistingToolBuild` immediately. |
| 2026-05-16 | Preserve one common hermetic wrapper-publish isolation test plus minimal public script smoke tests. | Public script and scratch-root isolation behavior remains important without rebuilding for every assertion. | Replace all script tests with in-process tests; keep all full matrices as script smoke. |
| 2026-05-16 | Defer removal of assembly-wide `DoNotParallelize` until after categorization and parallel-safety discovery. | Parallelization risk should be measured instead of bundled into the first tiering refactor. | Remove assembly-wide serialization in the first slice. |

## Next artifacts

- `proposal-review` for this proposal.
- A test-runtime optimization spec or plan defining validation tiers, category rules, and corpus harness changes.
- A matching test spec for category inventory, wrapper smoke coverage, prepared-tool isolation, and runtime measurement evidence.
- An execution plan if CI scripts, corpus wrappers, test projects, and validation docs are changed.

## Follow-on artifacts

None yet.

## Readiness

Ready for proposal review. The proposal does not change production behavior and should be implemented only after the team accepts the validation-tiering policy and the scope of corpus wrapper optimization.
