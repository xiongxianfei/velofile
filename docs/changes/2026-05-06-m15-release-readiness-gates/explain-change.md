# M15 Release Readiness Gates

M15 turns the earlier validation scaffolding into release-readiness evidence for benchmark reports, compatibility aggregation, diagnostics conformance, accessibility checks, and preview triage thresholds.

## What Changed

`tools/VeloFile.Corpus` now generates scaled `small`, `medium`, `large`, `deep`, `preview`, and `pathological` profiles. The profiles remain deterministic and scratch-local, but they are large enough to exercise the benchmark and compatibility report contracts.

The benchmark runner no longer emits a null-timing stub. `scripts/run-benchmarks.ps1` passes run-count and optional app launch inputs to the corpus tool, and the tool writes `benchmark-report.json` with environment metadata, reference corpus summaries, median, p95, p99, and non-gating release status. The legacy `benchmark-smoke-report.json` path is still written for compatibility.

The compatibility runner now supports `-Scope release`, aggregating operations, drag/drop, path compatibility, association, and DPI evidence into `release-compat-result.json`.

`scripts/run-diagnostics-conformance.ps1` runs a local diagnostics conformance check. It writes diagnostic events through the production sanitizer/log store, records crash and last-action markers, exports redacted JSONL evidence, and verifies that prohibited raw values do not appear in generated artifacts.

`docs/release/preview-triage.md` documents the crash, hang, diagnostics, and p95 regression thresholds that block preview promotion. `docs/release/benchmark-baseline.md` now describes the M15 measured report and release-threshold rules.

App shell contract tests now directly prove keyboard routes, accessible names, user-visible status surfaces, and destructive confirmation text required for the accessibility gate. `MainWindow.xaml` now supplies the missing automation names on the search, rename, file-operation, conflict, and PDF navigation controls.

## Safety Notes

All corpus, benchmark, compatibility, and diagnostics output remains under a guarded scratch root. The diagnostics conformance runner records fingerprints and controlled reason codes rather than raw paths, command text, file contents, usernames, or secret-like values.

Benchmark runs remain `-NonGating` by default. Release owners must compare p95 against a baseline and apply ADR 0003 thresholds before making public performance claims.

## Tests

Corpus tests cover scaled profiles, measured benchmark report shape, release compatibility aggregation, diagnostics conformance export/redaction, preview triage policy documentation, and existing compatibility/preview scopes.

App tests cover the accessibility shell contract for keyboard routes, accessible names, distinct empty/loading/failure/status surfaces, and permanent-delete confirmation visibility.

## Validation

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug`
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter Accessibility`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\generate-corpus.ps1 -Profile deep -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-benchmarks.ps1 -NonGating -ScratchRoot <scratch-root> -RunCount 3`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-compat-corpus.ps1 -Scope smoke -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-compat-corpus.ps1 -Scope release -ScratchRoot <scratch-root>`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\run-diagnostics-conformance.ps1 -ScratchRoot <scratch-root>`
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
