# PR CI Validation Tiering Explain Change

## Status

final explain-change; verification and PR handoff still pending

## Summary

This change splits hosted validation into explicit CI tiers. Ordinary pull requests now have a fast PR confidence lane, `ci-fast-required`, while expensive release evidence and broad closeout validation remain available through `ci-release-evidence`, `ci-full-closeout`, and the unchanged local `scripts/ci.ps1` command.

The change also adds runtime summaries, workflow contract tests, hosted shadow-run evidence, contributor guidance, and lifecycle records so reviewers can see what ran, what did not run, and why fast PR confidence is not release readiness.

## Problem

The prior required PR workflow ran the broad closeout path for every pull request. Hosted PR #3 took about 16 minutes, with the repository CI script step around 14m58s and `VeloFile.Corpus.Tests` around 13m22s. Core, App, and Windows tests completed in seconds once build output was available.

The repository already had category and evidence boundaries for fast, contract, smoke, and release-evidence tests. The missing piece was hosted PR policy: every ordinary PR still paid the full release-evidence cost.

## Decision Trail

| Source | Decision |
|---|---|
| Proposal | Choose hosted CI validation tiers, not deleting release evidence and not relying on caching as the main speed fix. |
| Proposal review | Split product tests from Corpus-filtered tests, decide release-evidence triggers, prefer separate workflows, shadow-run before branch-protection handoff, and make runtime reporting mandatory. |
| Spec | Requirements R1-R69 define `ci-fast-required`, `ci-release-evidence`, `ci-full-closeout`, hosted Windows/pwsh/.NET setup, command selection, summary status, rollout, and non-goals. |
| Architecture / ADR 0012 | Use separate workflows for release evidence and closeout, a shared PowerShell runtime-summary helper, structured workflow contract tests, and preserve `scripts/ci.ps1` as the broad closeout command. |
| Plan | Implement in milestones: M1 summary helper, M2 fast shadow lane, M3 release evidence, M4 full closeout, M5 shadow-run evidence and guidance. |
| Reviews | PRCI-SR1, PRCI-PLR1, PRCI-CR1, PRCI-CR2, PRCI-CR3, and PRCI-CR4 were all closed before final explain-change. |

## Diff Rationale By Area

| Area | Files | Why changed | Source/Test evidence |
|---|---|---|---|
| Runtime summary helper | `scripts/Write-CiRuntimeSummary.ps1` | Centralizes GitHub summary rendering, failed-command context, category status, TRX slow tests, and per-project durations without changing validation command semantics. | R40-R48; `CiRuntimeSummaryTests`; PRCI-CR1 and PRCI-CR2 resolution. |
| Fast PR lane | `.github/workflows/ci.yml` | Adds `ci-fast-required` with Windows, `pwsh`, SDK setup, `dotnet --info`, restore/build, UI contract validation, direct Core/App/Windows tests, Corpus `Fast|Contract`, and Corpus `CorpusScript&Smoke`. Keeps broad `ci` during rollout. | R1-R3, R11-R27, R65-R69; `CiWorkflowContractTests`; hosted run `26062568345`. |
| Release evidence lane | `.github/workflows/release-evidence.yml` | Moves expensive Corpus release evidence to an explicit manual/scheduled/release/merge-queue lane with clear summary status. | R4-R8, R28-R34; workflow contract tests. |
| Full closeout lane | `.github/workflows/closeout.yml` | Adds a manual `ci-full-closeout` lane that invokes `scripts/ci.ps1` unchanged and preserves failure semantics. | R9-R10, R35-R39; workflow contract tests; code-review-r7. |
| Workflow test model | `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`, `CiWorkflowContractTests.cs` | Parses workflow YAML structurally and guards triggers, hosted environment, command order, filters, summary wiring, artifacts, and failure semantics. | PRCI-T001 through PRCI-T026 and invalid fixture diagnostics. |
| Runtime-summary tests | `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs` | Proves summary output, missing-output limitations, failed command context, redaction, release status, and TRX-derived project durations. | R40-R48; PRCI-CR1/CR2. |
| Rollout evidence tests | `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs`, `ValidationCommandDocumentationTests.cs` | Makes M5 evidence and contributor guidance testable so future edits cannot remove the shadow-run record, no-handoff caveat, lane names, release-readiness warning, or rollback wording silently. | PRCI-T027, PRCI-T028, PRCI-M001, PRCI-M003. |
| Evidence and guidance | `shadow-run.md`, `branch-protection-handoff.md`, `README.md`, `CONTRIBUTING.md`, `docs/project-map.md` | Records the accepted hosted PR cycle, records that branch protection is not configured, and documents the three hosted lanes for contributors. | R13, R49-R53; code-review-r9. |
| Lifecycle records | `docs/plan.md`, active plan, `change.yaml`, review records, `review-log.md`, this file | Keeps milestone state, validation evidence, review outcomes, and next-stage routing consistent. | Implement/code-review workflow requirements. |

## Tests Added Or Changed

- `CiWorkflowContractTests`: verifies `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` identity, triggers, Windows runner, `pwsh`, SDK setup, command ordering, category filters, summary arguments, artifact paths, and failure-semantics guardrails.
- `CiRuntimeSummaryTests`: verifies summary rendering, missing structured output, failed command/outcome context, category status output, redaction, and TRX-derived per-project durations.
- `CiRolloutEvidenceTests`: verifies `shadow-run.md`, `branch-protection-handoff.md`, and rollout guidance contain the required evidence and no-handoff wording.
- `ValidationCommandDocumentationTests`: now also verifies hosted CI lane guidance in README and CONTRIBUTING.
- App test timing adjustments in `AppShellThumbnailUiTests.cs` and `AppShellCommandRouteTests.cs` stabilize hosted broad CI waits discovered during shadow-run attempts; they do not change production behavior.

## Validation Evidence Available Before Final Verify

- M1 focused summary tests passed after implementation and review-resolution.
- M2 workflow contract tests passed; local fast-lane commands passed for `dotnet --info`, restore, build, UI contract validation, Core/App/Windows direct tests, Corpus `Fast|Contract`, and Corpus `CorpusScript&Smoke`.
- M3 workflow contract and runtime-summary tests passed; local `TestCategory=ReleaseEvidence` validation passed.
- M4 workflow/documentation validation passed; local `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed with Core 168, App 168, Windows 52, and Corpus 110 tests.
- M5 validation passed: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"` passed with 20 tests.
- M5 guidance scan passed: `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout" README.md docs specs .github` found expected matches.
- M5 diff check passed with Git LF-to-CRLF working-copy warnings only.
- Code-review-r9 reviewer reruns passed: the focused 20-test command and `git diff --check HEAD~1..HEAD`.
- Hosted PR #4 run `26062568345` passed at commit `28de2d60faaa7fc2fbf0f3eade53f8467c26ff1a`: `ci-fast-required` passed in 7m20s and broad `ci` passed in 16m01s.
- Latest hosted run status is not claimed here. After the review-record push, run `26063756090` for commit `0e13d8ba537c4befd461fa2f712371ae00072ba6` was queued.

## Review Resolution Summary

- Spec review finding PRCI-SR1: accepted and closed by adding the hosted execution-environment contract.
- Plan review finding PRCI-PLR1: accepted and closed by making `dotnet --info` explicit and testable in M2.
- Code review findings PRCI-CR1, PRCI-CR2, and PRCI-CR3: accepted and closed through `docs/changes/2026-05-18-pr-ci-validation-tiering/review-resolution.md`.
- Code review finding PRCI-CR4: closed by M5 hosted shadow-run evidence and clean `code-review-r9`.
- No review-resolution is open. `review-log.md` records no open material findings after `code-review-r9`.

## Alternatives Rejected

- Do not delete or hide release-evidence tests. They remain in the repository and in explicit release/closeout paths.
- Do not narrow `scripts/ci.ps1`. It remains the broad closeout command.
- Do not filter Core/App/Windows tests with Corpus categories in the fast lane; those projects run directly.
- Do not rely on NuGet/package caching as the main speed fix.
- Do not claim GitHub branch protection changed from repository files. The handoff remains maintainer-operated.
- Do not make screenshot, visual, or manual evidence a hard gate in `ci-fast-required`.

## Scope Control

The change is limited to validation infrastructure, tests, documentation, and lifecycle records. It does not change production App/Core/Windows/Corpus behavior, public prepared-tool options, release-evidence taxonomy, or assembly/class serialization policy.

## Risks And Follow-Ups

- Branch protection is still external. `branch-protection-handoff.md` records that `main` branch protection was not configured (HTTP 404), so maintainers still need to perform any required-check handoff.
- The temporary broad `ci` PR job remains during rollout; this preserves rollback but continues to spend extra hosted minutes until handoff.
- Latest hosted CI for the final pushed review-record commit must be checked during `verify`; this explain-change does not claim branch-ready or PR-ready status.
- Release readiness still requires `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or another accepted release gate.

## Current Handoff

Implementation milestones are closed. The next stage is `verify`. This artifact does not claim final verification, branch readiness, PR body readiness, or PR open readiness.
