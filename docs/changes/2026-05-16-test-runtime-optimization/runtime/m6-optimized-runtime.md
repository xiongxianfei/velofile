# M6 Optimized Runtime Evidence

## Evidence Type

local runtime report

## Scope

This report records the first-slice runtime evidence for the test runtime optimization work. It is local review evidence, not a universal runtime guarantee. The measurements describe this repository state, local Windows/.NET environment, Debug configuration, and the commands listed below.

## Local Environment Assumptions

- Date recorded: 2026-05-16
- OS: Windows local developer environment
- .NET SDK observed during validation: 10.0.203
- Configuration: Debug
- Shell: Windows PowerShell for `scripts\ci.ps1`, `dotnet test` for test commands
- Local environment assumptions: single local workstation, warm package caches after prior milestone validation, assembly-wide Corpus test serialization still enabled, and release-evidence wrapper tests intentionally retained.
- Privacy: command paths are repository-relative; raw TRX files are not committed because they may contain machine-local absolute paths.
- Interpretation: local durations are review evidence for this slice, not a universal runtime guarantee.

## Baseline Corpus Runtime

- Source: `docs/changes/2026-05-16-test-runtime-optimization/runtime/m1-baseline.md`
- Date recorded: 2026-05-16
- Command: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug`
- Configuration: Debug
- Filter: none
- Result: passed
- Test count: 37
- Measured duration: about `5 m 49 s`
- Interpretation: accepted pre-optimization Corpus-only baseline from the learn/proposal session.

## Optimized Runtime Measurements

| ID | Command | Configuration | Filter | Date recorded | Result | Test count / selection | Measured duration | Notes |
|---|---|---|---|---|---|---|---|---|
| focused-runtime-report-tests-no-build | `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~RuntimeReportTests"` | Debug | `FullyQualifiedName~RuntimeReportTests` | 2026-05-16 | passed | 4 tests selected | `17 ms` test duration | Focused report-contract timing evidence after build-producing validation. |
| solution-fast-contract | `dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast\|TestCategory=Contract"` | Debug | `TestCategory=Fast\|TestCategory=Contract` | 2026-05-16 | passed | 71 Corpus tests selected; Core/App/Windows reported no matching tests | about `54 s` Corpus test duration | Build-producing fast/default evidence. |
| solution-fast-contract-no-build | `dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast\|TestCategory=Contract"` | Debug | `TestCategory=Fast\|TestCategory=Contract` | 2026-05-16 | passed | 71 Corpus tests selected; Core/App/Windows reported no matching tests | about `1 m 7 s` Corpus test duration | Timing evidence after build-producing validation; slower than the build-producing run on this machine. |
| corpus-contract | `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Contract"` | Debug | `TestCategory=Contract` | 2026-05-16 | passed | 71 tests selected | about `52 s` | Required M6 command. |
| corpus-contract-no-build | `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract"` | Debug | `TestCategory=Contract` | 2026-05-16 | passed | 71 tests selected | about `53 s` | Required M6 timing command after build-producing validation. |
| corpus-script-smoke | `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"` | Debug | `TestCategory=CorpusScript&TestCategory=Smoke` | 2026-05-16 | passed | 6 tests selected | about `51 s` | Script smoke tier remains materially below the multi-minute baseline. |
| corpus-release-evidence | `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"` | Debug | `TestCategory=ReleaseEvidence` | 2026-05-16 | passed | 10 tests selected | about `5 m 3 s` | Explicit release-evidence tier remains available and intentionally expensive. |
| full-ci | `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` | Debug | none | 2026-05-16 | passed | Core 168; App 149; Windows 52; Corpus 90 | about `7 m 3 s` Corpus test duration in CI output | Broad closeout command remains available. |

## Top 10 Slowest Tests

- Structured slow-test source: TRX
- Source command: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=m6-corpus.trx"`
- Source handling: raw TRX not committed because it may contain machine-local absolute paths.
- Method: sorted TRX unit-test results by duration descending.

| Rank | Test | Duration |
|---:|---|---:|
| 1 | Generate_placeholder_profiles_are_deterministic | about `2 m 5 s` |
| 2 | Compatibility_and_preview_runners_validate_scope | about `1 m 15 s` |
| 3 | M15_reference_profiles_are_scaled_and_release_scoped | about `48 s` |
| 4 | Diagnostics_conformance_runner_writes_redacted_local_report_and_export | about `22 s` |
| 5 | HermeticWrapper_scratch_publish_isolation_and_path_safety | about `13 s` |
| 6 | Benchmark_harness_emits_measured_report_environment_and_release_status | about `12 s` |
| 7 | Preview_public_script_smoke_routes_and_writes_representative_output | about `12 s` |
| 8 | Benchmark_public_script_smoke_routes_and_writes_representative_output | about `12 s` |
| 9 | Diagnostics_public_script_smoke_routes_and_writes_representative_output | about `12 s` |
| 10 | PreviewProviders_scope_records_provider_behavior_evidence | about `11 s` |

## Full CI Status

- Command: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Configuration: Debug
- Filter: none
- Date recorded: 2026-05-16
- Result: passed in M6 validation.
- Comparison: full CI remains the broad closeout command. The first slice optimized local focused tiers rather than deleting broad validation, and the full Corpus run remains slower than the pre-optimization Corpus-only baseline because additional category, prepared-tool, smoke, and runtime-report coverage is now present.

## Runtime Target Outcomes

| Requirement | Target | Outcome | Evidence and rationale |
|---|---|---|---|
| R56 | Optimized focused contract run should complete under 10 seconds when already built. | satisfied | The focused `RuntimeReportTests` command passed in less than 1 second of test duration after build. |
| R57 | Optimized Corpus fast/contract run should complete under 30 seconds when already built. | missed | `corpus-contract-no-build` measured about `53 s` and `solution-fast-contract-no-build` measured about `1 m 7 s`; additional contract invariants and retained release-safety boundaries keep this above target. |
| R58 | Optimized Corpus script-smoke run should be materially lower than the multi-minute Corpus runtime. | satisfied | `corpus-script-smoke` measured about `51 s`, materially below the `5 m 49 s` Corpus-only baseline. |
| R59 | Full local CI should be no worse than the pre-optimization baseline. | missed / not directly comparable | Full CI includes more projects and now 90 Corpus tests versus the 37-test Corpus-only baseline. It passed, but the Corpus duration in full CI is higher than the baseline; this is recorded without deleting coverage. |
| R60 | Missed SHOULD-level targets must be recorded with evidence and follow-up rationale and must not justify deleting coverage. | satisfied | Misses are recorded here. Coverage preservation is stated below. |

Coverage preservation: No coverage was deleted to satisfy runtime targets. Slow release-evidence, wrapper, and full-CI paths remain explicit and available for milestone/release validation.
