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
