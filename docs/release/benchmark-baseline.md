# Benchmark Baseline

## Status

M2 provides only the benchmark report shape and non-gating runner stub. It does not define performance targets, release gates, or public performance claims.

The benchmark harness becomes release-gating in M15 after the generated corpus, measured workflows, and preview-release triage policy are implemented.

## Current Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -Root <velofile-corpus-scratch-root>
```

The scratch root must be a dedicated absolute path whose final segment contains both `velofile` and `corpus`. Existing non-empty directories must already contain the `.velofile-corpus-root` marker or the runner refuses to write.

## M2 Report Shape

The M2 report is written to:

```text
<scratch-root>/benchmarks/benchmark-smoke-report.json
```

It includes:

- OS build.
- Hardware class.
- CPU.
- RAM.
- Storage type.
- Windows Search state.
- Antivirus state.
- DPI configuration.
- Run count.
- Median.
- p95.
- p99.
- Release-gating status.

M2 reports use `releaseGatingStatus: non-gating` and null timing values. That is intentional until M15 supplies real benchmark measurements.
