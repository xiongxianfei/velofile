# Learn Session: Test Runtime Optimization

## Frame

- Trigger: maintainer asked why tests run slowly, how to optimize them, and what best practices apply.
- Trigger type: explicit maintainer request after M2 validation and code review.
- Scope: local validation runtime for the current VeloFile .NET/PowerShell test suite, especially the slow `scripts/ci.ps1` and `VeloFile.Corpus.Tests` path observed during M2 review.
- Evidence in scope:
  - `scripts/ci.ps1`
  - `scripts/Invoke-CorpusTool.ps1`
  - `scripts/generate-corpus.ps1`
  - `scripts/run-compat-corpus.ps1`
  - `scripts/run-preview-corpus.ps1`
  - `scripts/run-benchmarks.ps1`
  - `scripts/run-diagnostics-conformance.ps1`
  - `tests/VeloFile.Corpus.Tests/CorpusToolingSmokeTests.cs`
  - `tests/VeloFile.Corpus.Tests/MSTestSettings.cs`
  - Measured local run: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=learn-corpus.trx"`
- Explicit exclusions:
  - No production behavior changes.
  - No CI workflow changes.
  - No test optimization implementation.
  - No topic-file policy update without contributor confirmation.
- Prior learnings reviewed:
  - Existing session index was absent; existing sessions listed were `2026-05-11-icon-glyph-regression.md`, `2026-05-11-plan-review-recording-lapse.md`, and `2026-05-12-ui-gap-to-hifi-analysis.md`.
- Session record path: `docs/learn/sessions/2026-05-16-test-runtime-optimization.md`

## Observations

### O1. Corpus tests dominate broad validation runtime

Evidence:

- `scripts/ci.ps1` runs `dotnet --info`, restore, build, UI contract validation, then `dotnet test VeloFile.sln -c Debug --no-build`.
- The measured Corpus-only run passed 37 tests in `5 m 49 s`.
- Recent full `scripts/ci.ps1` output passed but spent nearly the whole wall time in `VeloFile.Corpus.Tests`, which reported `37` tests in about `5 m 57 s`.
- Parsed TRX top durations:
  - `Generate_placeholder_profiles_are_deterministic`: `00:02:05.2992646`
  - `Compatibility_and_preview_runners_validate_scope`: `00:01:15.6938263`
  - `M15_reference_profiles_are_scaled_and_release_scoped`: `00:00:47.5713536`
  - Next six corpus smoke tests each took roughly `10-12 s`.

Why this is slow:

- The slow tests are script/process integration tests, not small in-process contract tests.
- `tests/VeloFile.Corpus.Tests/MSTestSettings.cs` applies `[assembly: DoNotParallelize]`, so the whole Corpus test assembly runs serially.
- `CorpusToolingSmokeTests.RunScript` launches `powershell.exe` for every script invocation.
- `scripts/Invoke-CorpusTool.ps1` creates a scratch tool source tree, copies `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows`, deletes publish/bin/obj output, and runs `dotnet publish` on every invocation.
- `CorpusToolingSmokeTests.cs` contains many `RunScript(...)` invocations. The first three slow tests alone account for repeated publish/process overhead:
  - deterministic profile test: seven profiles generated twice;
  - M15 profile test: six profile generations;
  - compatibility/preview test: one generation, multiple compatibility scopes, multiple preview scopes, and one negative scope.

### O2. `--no-build` at the solution level does not help wrapper-launched corpus scripts

Evidence:

- `scripts/ci.ps1` correctly uses `dotnet build ... --no-restore` and `dotnet test ... --no-build`.
- However, corpus wrapper scripts do not reuse the solution build output. Each wrapper call publishes a copied scratch copy of the corpus tool by design.

Why this matters:

- Optimizing only the outer `dotnet test` command will not materially improve the slowest path.
- The largest opportunity is inside the corpus test/wrapper design: reduce repeated scratch copy + publish + PowerShell process startup work.

### O3. Current broad validation mixes fast contract tests with heavier smoke/release-style evidence

Evidence:

- Fast UI contract focused tests complete in seconds.
- Corpus tests include categories such as `Benchmarks`, `Compatibility`, `PreviewContract`, `PreviewProviders`, `Thumbnails`, `Diagnostics`, and `Release`.
- Some tests validate release-evidence semantics and script hermeticity rather than ordinary code-level behavior.

Why this matters:

- Running all evidence classes on every broad local validation is correct for high-confidence CI, but it is inefficient as the default inner loop.
- The suite needs explicit validation tiers so contributors can run fast focused checks first and reserve expensive script/process smoke for gated points.

## Classification

| Observation | Proposed classification | Final classification | Secondary routes | Confirmed by | Rationale |
|---|---|---|---|---|---|
| O1 | process-follow-up | process-follow-up | active plan or future CI/test optimization issue | maintainer asked for analysis; no implementation requested yet | The evidence shows a systemic test design cost that should be optimized deliberately. |
| O2 | observation | observation | none | measured local evidence | This explains why current `--no-build` still leaves the slow path expensive. |
| O3 | direction | direction | future proposal/plan update if validation tiering becomes project policy | maintainer asked for best practices; no policy update requested yet | Validation tiering affects contributor workflow and CI policy, so learn should not make it authoritative by itself. |

No `docs/learn/topics/` file was updated in this session. The observations are useful and evidence-bound, but any durable policy should be routed through the active plan, CI/test plan, or a dedicated proposal before it becomes authoritative.

## Recommended Follow-Ups

### Fast, low-risk improvements

- Add a measurement step or test logger option to the local validation notes so slow tests are visible by name, not guessed from wall time.
- Add guidance to use focused filters during implementation, for example `ShellSurfaceResourceContractTests`, `UiContracts`, or affected feature filters before running broad smoke.
- Keep full `scripts/ci.ps1` for review/verify gates, not every small edit.

### Medium-scope test design improvements

- Split corpus tests into tiers:
  - `CorpusContracts`: in-process tests for JSON shape, scope classification, manifests, sidecar validation, and redaction rules.
  - `CorpusScriptSmoke`: one or two wrapper/process tests proving scripts call the tool correctly and preserve hermetic environment behavior.
  - `CorpusReleaseEvidence`: compatibility, preview provider, diagnostics, benchmark, and release-evidence aggregation tests.
- Move `[assembly: DoNotParallelize]` out of the whole Corpus assembly if possible. Prefer method/class-level serialization only for tests that mutate process/user environment or share scratch state.
- Avoid repeated script invocations in loops when testing deterministic profile generation. Call the corpus tool in process for most profiles and keep a single wrapper smoke case.
- Publish or build the corpus tool once per test class/run for wrapper tests, then invoke the built DLL repeatedly. Keep one hermetic scratch-publish smoke test to prove the wrapper path.

### Larger design improvements

- Extract corpus command logic behind a testable library boundary so tests can call `CorpusCli` or a service directly without process startup for contract cases.
- Add an explicit script option or test-only path such as `-UseExistingToolBuild` or `-PreparedToolPath` for tests that need process execution but not scratch publishing.
- Cache scratch tool publish output using a source-content hash. Rebuild only when `tools/VeloFile.Corpus`, `src/VeloFile.Core`, or `src/VeloFile.Windows` inputs change.
- Consider separate CI jobs or stages:
  - fast PR validation;
  - full Windows smoke;
  - release/nightly corpus evidence.

## Best-Practice Guidance

- Keep hermetic script tests, but do not make every contract assertion pay the full hermetic publish/process cost.
- Use the test pyramid deliberately: many in-process deterministic tests, fewer process-bound script tests, and the smallest number of release-style smoke tests.
- Make expensive tests explicit with categories and documented triggers.
- Preserve confidence by keeping at least one end-to-end wrapper test for each public script family.
- Measure before optimizing, and keep per-test duration data available in review evidence when wall time becomes a blocker.
- Optimize the test harness before weakening assertions.

## Route

- No topic update was made.
- No active plan update was made in this session.
- Recommended next artifact if the maintainer wants this implemented: a scoped plan or M3/M9 follow-up for validation-tier/test-harness optimization.

## Validation

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=learn-corpus.trx"`: passed, 37 tests, `5 m 49 s`.
- Parsed `tests/VeloFile.Corpus.Tests\TestResults\learn-corpus.trx` for per-test duration evidence.
