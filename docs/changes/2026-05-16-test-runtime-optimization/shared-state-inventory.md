# Test Runtime Optimization Shared-State Inventory

## Status

M1 inventory; assembly-wide `DoNotParallelize` remains active.

## Scope

This inventory records known shared-state constraints before any later parallelization slice. M1 categorizes tests and records constraints only; it does not remove assembly-wide serialization.

## Known shared-state constraints

| Test area | Shared state or boundary | M1 treatment |
|---|---|---|
| `CorpusToolingSmokeTests` public script calls | Launches PowerShell wrappers, invokes `dotnet publish`, and exercises shared public scripts. | Keep serialized; categorize as `CorpusScript` plus `Smoke` or `ReleaseEvidence`. |
| `CorpusToolingSmokeTests` scratch roots | Uses temp scratch roots under `velofile-corpus-tests`; deletion can race with process/file handles. | Keep unique scratch roots and assembly-wide `DoNotParallelize`. |
| `CorpusToolingSmokeTests` user environment checks | Reads user `Path` and validates wrapper environment behavior. | Keep serialized; do not mutate user environment. |
| UI contract tool tests | Launch `dotnet run` for `tools/VeloFile.UiContracts` and read repository fixtures. | Keep as `Contract` or `Visual`; no parallelization change in M1. |
| Visual baseline update script tests | Launch PowerShell baseline-update script and create temp visual evidence roots. | Keep as `Visual`; no fast default selection. |

## Deferred parallelization rule

Removing assembly-wide `DoNotParallelize` remains deferred. A later slice must prove parallel safety, add narrower method/class-level serialization where needed, and keep scratch roots unique before broad parallel execution is enabled.
