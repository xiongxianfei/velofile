# Benchmark Baseline

## Status

M15 promotes the M2 report shape into a non-gating measured benchmark harness. Contributor runs still use `-NonGating`; release gating requires comparing the generated report against the approved baseline and applying the preview triage policy.

Public performance claims remain blocked until a release owner records the corpus profile, environment metadata, run count, median, p95, p99, and triage threshold used for the decision.

## Current Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -ScratchRoot <velofile-corpus-scratch-root>
```

Optional app launch measurement inputs:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -ScratchRoot <velofile-corpus-scratch-root> -RunCount 5 -AppExecutablePath <exe> -AppArguments "<args>"
```

The scratch root must be a dedicated absolute path whose final segment contains both `velofile` and `corpus`. Existing non-empty directories must already contain the `.velofile-corpus-root` marker or the runner refuses to write.

## Report Artifacts

The M15 report is written to:

```text
<scratch-root>/benchmarks/benchmark-report.json
<scratch-root>/benchmarks/benchmark-smoke-report.json
```

The legacy `benchmark-smoke-report.json` name is retained for scripts and tests that still consume the M2 path.

## Report Contents

The report includes OS build, hardware class, CPU, RAM bytes, storage type, Windows Search state, antivirus state when observable, DPI configuration, processor architecture, run count, median, p95, p99, release status, reference corpus profile counts, p95 regression thresholds, and preview crash/hang triage policy pointers.

Current measured scenarios:

- app process launch when an executable path is supplied
- small, medium, and large folder switching
- current-folder filter over the medium corpus
- first and thousandth recursive-search result over the deep corpus
- context-menu opening hot path
- tab switching hot path
- ten-tab session restore hot path

## Release Thresholds

Per ADR 0003, p95 regression above 10% requires acknowledgement. p95 regression above 25% blocks promotion unless the release owner records an explicit exception.
