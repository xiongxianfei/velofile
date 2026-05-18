# PR CI Validation Tiering Change Notes

## Status

implementation notes; not final explain-change

This file records milestone-local rationale while implementation is underway. The final durable explanation remains owned by the later `explain-change` lifecycle stage.

## M1 Runtime Summary Foundation

M1 adds a shared PowerShell summary helper and focused contract tests before introducing new hosted validation lanes.

Changed surfaces:

- `scripts/Write-CiRuntimeSummary.ps1` writes GitHub job-summary markdown for lane name, trigger, selected categories, release-evidence status, Corpus script smoke status, full-closeout status, optional command durations, optional per-project durations, slowest TRX tests, missing-output limitations, and redacted sensitive values.
- `.github/workflows/ci.yml` keeps the existing broad `./scripts/ci.ps1` command unchanged, gives that step the stable `repository_ci` id, and adds a follow-up `if: always()` summary step for the current broad CI lane.
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs` proves the helper output, missing-TRX limitation behavior, failed-command and command-outcome reporting, redaction behavior, and broad-CI summary hook.

PRCI-CR1 resolution:

- The summary step now uses `${{ steps.repository_ci.outcome }}` to pass `-FailedCommand` and `-FailedOutcome` only when the broad CI step does not succeed.
- `scripts/Write-CiRuntimeSummary.ps1` treats `-FailedCommand` and `-FailedOutcome` as explicit workflow-provided failure context rather than inferring failure from missing TRX.
- The broad CI step still runs `./scripts/ci.ps1` under `shell: pwsh`; no `continue-on-error` or command-selection change was introduced.

Scope notes:

- No fast PR lane, release-evidence workflow, closeout workflow, branch-protection claim, caching behavior, or production App/Core/Windows/Corpus behavior was changed in M1.
- The current broad CI workflow does not emit TRX output yet, so the M1 broad-CI summary honestly reports unavailable structured slow-test details until later lanes produce structured output.

## M2 Fast PR Shadow Lane

M2 adds `ci-fast-required` as a shadow-running fast PR confidence job in the existing `ci.yml` workflow while keeping the broad `ci` job in place.

Changed surfaces:

- `.github/workflows/ci.yml` now defines a `ci-fast-required` job with `name: ci-fast-required`, `runs-on: windows-latest`, job-level `pwsh`, .NET SDK setup, `dotnet --info`, restore, build, production UI contract validation, direct Core/App/Windows tests, Corpus `Fast|Contract`, and Corpus `CorpusScript&Smoke`.
- The fast job writes TRX output for every `dotnet test` command and uploads matching TRX files as a best-effort artifact.
- The fast job writes a summary through `scripts/Write-CiRuntimeSummary.ps1` with `ReleaseEvidence: not run in this lane`, `CorpusScript Smoke: run`, and `Full closeout: not run`.
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs` adds a test-owned structured YAML workflow model backed by the test-scoped `YamlDotNet` dependency.
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs` proves the fast lane identity, triggers, Windows/pwsh/SDK contract, command ordering, UI contract inputs, direct product tests, Corpus filters, no closeout/release-evidence default, TRX/artifact reporting, category selection, and actionable diagnostics for invalid workflow fixtures.

Scope notes:

- The existing broad `ci` job remains in the workflow for the shadow period.
- M2 does not claim branch protection has changed.
- M2 does not add release-evidence or full-closeout workflows; those remain M3 and M4.
- M2 does not change production App/Core/Windows/Corpus behavior, test category taxonomy, caching policy, or public prepared-tool options.

PRCI-CR2 resolution:

- `scripts/Write-CiRuntimeSummary.ps1` now derives per-test-project duration rows from TRX when explicit `-TestProjectDuration` inputs are absent.
- The helper prefers `TestMethod` `codeBase` assembly names from structured TRX output and falls back to the TRX file stem, avoiding private local path disclosure.
- `ci-fast-required` command selection and failure semantics remain unchanged; the existing `-TrxPath` summary input is now also the project-duration source.
- `CiRuntimeSummaryTests` prove TRX-derived project duration output, and `CiWorkflowContractTests` preserve the fast-lane summary source contract.

## M3 Release-Evidence Workflow

M3 adds a separate hosted release-evidence lane without making ordinary PRs pay that cost by default.

Changed surfaces:

- `.github/workflows/release-evidence.yml` defines `ci-release-evidence` with `workflow_dispatch`, nightly schedule `17 3 * * *`, `merge_group`, release branch pattern `release/**`, and release tag patterns `v*` and `v*-rc*`.
- The workflow uses `windows-latest`, job-level `pwsh`, `actions/setup-dotnet@v4`, `dotnet --info`, restore, build, and `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=ReleaseEvidence"` with TRX output.
- The workflow summary reports the lane as release evidence, records `ReleaseEvidence=run`, `Benchmark=run`, `Visual=not selected in this lane`, `ManualEvidence=absent from current test inventory`, `CorpusScript Smoke=not selected in this lane`, and `Full closeout=not run`.
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs` now parses push tag patterns and schedule cron entries so workflow contract tests can inspect the new trigger contract.
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs` proves release-evidence triggers, Windows/pwsh/SDK setup, restore/build before `--no-build`, explicit `ReleaseEvidence` filtering, summary status, TRX artifact upload, and category-status inventory drift.
- `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs` proves release-evidence summary category status rendering.

Scope notes:

- M3 does not change the ordinary PR `ci-fast-required` command selection.
- M3 does not add the full closeout workflow; that remains M4.
- M3 does not change `scripts/ci.ps1`, production App/Core/Windows/Corpus behavior, test category taxonomy, caching policy, or public prepared-tool options.
