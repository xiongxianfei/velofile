# M15 Release Readiness Gates

M15 turns the earlier validation scaffolding into release-readiness evidence for benchmark reports, compatibility aggregation, diagnostics conformance, accessibility checks, and preview triage thresholds.

## What Changed

`tools/VeloFile.Corpus` now generates scaled `small`, `medium`, `large`, `deep`, `preview`, and `pathological` profiles. The profiles remain deterministic and scratch-local, but they are large enough to exercise the benchmark and compatibility report contracts.

The benchmark runner no longer emits a null-timing stub. `scripts/run-benchmarks.ps1` passes run-count and optional app launch inputs to the corpus tool, and the tool writes `benchmark-report.json` with environment metadata, reference corpus summaries, median, p95, p99, and non-gating release status. The legacy `benchmark-smoke-report.json` path is still written for compatibility. Current M15 measurements are explicitly classified as `infrastructure-only` with `releaseEvidence=false`, because they do not drive the VeloFile app boundary and cannot satisfy P1-P13 or AC15 release evidence.

The compatibility runner now supports `-Scope release`, aggregating operations, drag/drop, path compatibility, association, and DPI evidence into `release-compat-result.json`. The aggregation now consumes scope result documents and preserves their evidence status; fixture-only operation/drag-drop reports, skipped path cases, and missing association/DPI verifiers stay non-release evidence instead of being upgraded to `verified`.

`scripts/run-diagnostics-conformance.ps1` runs a local diagnostics conformance check. It writes diagnostic events through the production sanitizer/log store for navigation, preview, file operation, search, terminal launch, and session restore, records crash and last-action markers, exports redacted JSONL evidence, verifies that prohibited raw values do not appear in generated artifacts, and records triage decisions for below/at/above threshold cases.

`docs/release/preview-triage.md` documents the crash, hang, diagnostics, and p95 regression thresholds that block preview promotion. `docs/release/benchmark-baseline.md` now describes the M15 infrastructure-only report and the release-threshold rules for future app-level evidence.

App shell contract tests now directly prove keyboard routes, accessible names, user-visible status surfaces, and destructive confirmation text required for the accessibility gate. `docs/release/accessibility-checklist.md` adds the required manual release evidence for focus visibility, keyboard traversal, destructive confirmation readability, mixed-DPI readability, distinct states, and screen-reader metadata.

## Safety Notes

All corpus, benchmark, compatibility, and diagnostics output remains under a guarded scratch root. The diagnostics conformance runner records fingerprints and controlled reason codes rather than raw paths, command text, file contents, usernames, or secret-like values.

Benchmark runs remain `-NonGating` by default. Release owners must not make public performance claims from the current infrastructure-only report. A future app-level driver must mark `measurementKind=app-level`, `appBoundaryDriven=true`, and `substituteMeasurement=false` before the measurement can count as release evidence.

`compat --scope release` intentionally exits nonzero when required verifier evidence is missing. That failure is evidence of the gate working, not a script crash.

## Tests

Corpus tests cover scaled profiles, measured benchmark report shape, release compatibility aggregation without evidence upgrades, diagnostics conformance export/redaction/workflow coverage, triage threshold decisions, preview triage policy documentation, and existing compatibility/preview scopes.

App tests cover the accessibility shell contract for keyboard routes, accessible names, distinct empty/loading/failure/status surfaces, permanent-delete confirmation visibility, and the required manual accessibility checklist artifact.

## Validation

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug`
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter Accessibility`
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "Compatibility_release_scope_consumes_real_scope_results_without_upgrading_missing_evidence|Benchmark_harness_emits_measured_report_environment_and_release_status|Diagnostics_conformance_runner_writes_redacted_local_report_and_export"`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\generate-corpus.ps1 -Profile deep -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-benchmarks.ps1 -NonGating -ScratchRoot <scratch-root> -RunCount 3`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-compat-corpus.ps1 -Scope smoke -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-compat-corpus.ps1 -Scope release -ScratchRoot <scratch-root>` intentionally returns 1 and writes `status=incomplete` until release verifier evidence exists.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-diagnostics-conformance.ps1 -ScratchRoot <scratch-root>`
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
