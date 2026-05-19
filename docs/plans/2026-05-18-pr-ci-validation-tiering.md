# PR CI Validation Tiering Execution Plan

## Status

done

This plan was approved by `plan-review-r2`, and the matching test spec was approved by `test-spec-review-r1`. PR #4 merged with hosted CI passing.

## Purpose / Big Picture

This plan turns the approved PR CI validation tiering contract into staged, reviewable implementation slices. The goal is to make ordinary pull request validation faster by introducing `ci-fast-required` while preserving explicit release evidence and full closeout validation through separate hosted lanes and the unchanged broad `scripts/ci.ps1` command.

The plan deliberately stages the rollout. The fast lane must shadow-run before maintainers change branch protection, runtime reporting comes first, and repository artifacts must not claim external required-check settings changed until maintainers record that handoff.

## Source Artifacts

| Artifact | Path | Status |
|---|---|---|
| Proposal | [2026-05-18-pr-ci-validation-tiering.md](../proposals/2026-05-18-pr-ci-validation-tiering.md) | accepted |
| Proposal review | [proposal-review-r1.md](../changes/2026-05-18-pr-ci-validation-tiering/reviews/proposal-review-r1.md) | approved after revisions |
| Spec | [specs/pr-ci-validation-tiering.md](../../specs/pr-ci-validation-tiering.md) | approved by spec-review-r2 |
| Spec review | [spec-review-r2.md](../changes/2026-05-18-pr-ci-validation-tiering/reviews/spec-review-r2.md) | approved |
| Prior review resolution | [review-resolution.md](../changes/2026-05-18-pr-ci-validation-tiering/review-resolution.md) | PRCI-SR1 closed |
| Architecture | [architecture.md](../architecture/system/architecture.md) | approved by architecture-review-r1 |
| ADR 0012 | [0012-hosted-pr-ci-validation-tiers.md](../adr/0012-hosted-pr-ci-validation-tiers.md) | accepted by architecture-review-r1 |
| Architecture review | [architecture-review-r1.md](../changes/2026-05-18-pr-ci-validation-tiering/reviews/architecture-review-r1.md) | approved |
| Test spec | [specs/pr-ci-validation-tiering.test.md](../../specs/pr-ci-validation-tiering.test.md) | active; approved by test-spec-review-r1 |
| Project map | [project-map.md](../project-map.md) | refreshed 2026-05-18 |

## Context and Orientation

Current state:

- `.github/workflows/ci.yml` contains one hosted `ci` job for `pull_request` and `push` to `main`.
- The current hosted `ci` job runs on `windows-latest`, sets up .NET SDK `10.0.x`, and invokes `./scripts/ci.ps1` with `shell: pwsh`.
- `scripts/ci.ps1` runs `dotnet --info`, restore, build, UI contract validation against valid fixtures, and unfiltered solution tests with `--no-build`.
- `tests/VeloFile.Corpus.Tests` already owns validation-tier category inventory, public corpus script smoke coverage, release-evidence tier tests, and runtime-report tests from the test-runtime optimization work.
- No `specs/pr-ci-validation-tiering.test.md` exists yet.
- No `ci-fast-required`, `ci-release-evidence`, or `ci-full-closeout` workflow lanes exist yet.

Relevant implementation surfaces:

- Workflow files: `.github/workflows/ci.yml`, `.github/workflows/release-evidence.yml`, `.github/workflows/closeout.yml`.
- Reporting helper: expected under `scripts/`, for example `scripts/Write-CiRuntimeSummary.ps1`.
- Workflow contract tests: expected under `tests/VeloFile.Corpus.Tests/TestRuntime/` or a nearby CI-specific test folder in the same project.
- Possible test-only YAML dependency: `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj` if the test spec and implementation choose `YamlDotNet` for structured YAML parsing.
- Change evidence: `docs/changes/2026-05-18-pr-ci-validation-tiering/`, including shadow-run and branch-protection handoff notes.
- Contributor guidance: `README.md`, and any existing contribution or validation guidance touched by the final policy.

Execution constraints:

- Use Windows hosted runners and `pwsh` for hosted PowerShell and repository script steps unless a reviewed exception is recorded.
- Set up or select the repository-approved .NET SDK before restore, build, test, UI contract, release-evidence, or closeout commands.
- Keep `scripts/ci.ps1` broad and local-runnable. Do not narrow it to fast filters.
- Keep production App/Core/Windows/Corpus behavior unchanged.
- Do not expose new public prepared-tool script options.
- Treat dependency caching as optional follow-up, not a correctness path.

## Non-goals

- Do not delete, skip, hide, or retag release-evidence tests to make ordinary PR CI faster.
- Do not remove or narrow `scripts/ci.ps1`.
- Do not change production App, Core, Windows, or Corpus behavior.
- Do not expose new public prepared-tool script options.
- Do not remove assembly-wide or class-level serialization.
- Do not make NuGet or dependency caching the primary speed fix.
- Do not make screenshot, visual, or manual evidence a hard gate in `ci-fast-required`.
- Do not claim branch protection or required-check settings changed until maintainers record the external configuration change.

## Requirements Covered

| Requirements | Primary milestone coverage |
|---|---|
| R1-R3 | M2 creates `ci-fast-required` and pull request/push trigger coverage. |
| R4-R8 | M3 creates `ci-release-evidence` manual, scheduled, release branch/tag, and merge-queue-ready trigger coverage. |
| R9-R10 | M4 creates `ci-full-closeout` with manual dispatch. |
| R11-R13 | M2-M5 keep ordinary PR defaults fast, keep release/closeout lanes separate, and record required-check handoff boundaries. |
| R14-R27 | M2 implements fast PR commands, direct product test project selection, Corpus filters, TRX output, and fast-lane summary wording. |
| R28-R34 | M3 implements explicit release-evidence commands and release-evidence summary status. |
| R35-R39 | M4 invokes and preserves broad `scripts/ci.ps1`. |
| R40-R48 | M1 creates the summary helper and tests; M2-M4 wire it into each hosted lane. |
| R49-R53 | M2 shadow-runs fast CI; M5 records comparison, branch-protection handoff, and rollback path. |
| R54-R61 | M2-M4 preserve release evidence, production behavior, prepared-tool boundaries, serialization, caching boundaries, visual evidence scope, and workflow contract policy. |
| R62-R64 | M1-M4 keep ordinary PR workflows secret-free, permissions-scoped, and cache-key safe if caching is added later. |
| R65-R69 | M2-M4 enforce Windows runner, `pwsh`, SDK setup, and reviewed exception behavior by workflow contract tests. |
| AC1-AC20 | M1-M5, with detailed test mapping owned by the matching test spec. |

## Milestones

### M1. Runtime Summary Helper And Broad-CI Reporting Foundation

- Milestone state: closed
- Goal: Add the shared runtime summary helper and tests before introducing new hosted lanes, while preserving the current broad CI command semantics.
- Requirements: R40-R48, R62-R64, AC12, AC16.
- Files/components likely touched:
  - `scripts/Write-CiRuntimeSummary.ps1`
  - `.github/workflows/ci.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs` or equivalent
  - `tests/VeloFile.Corpus.Tests/fixtures/` if sample TRX files are needed
- Dependencies:
  - Approved spec and architecture.
  - Plan-review and matching test spec before implementation.
- Tests to add/update:
  - Summary helper writes lane name, trigger, selected categories, release-evidence status, Corpus script smoke status, full-closeout status, durations, and slow-test rows from sample TRX.
  - Summary helper reports missing TRX or missing timing data as a limitation rather than fabricating details.
  - Summary helper output redacts or excludes secrets, tokens, private profile paths, and unrelated local machine details.
- Implementation steps:
  - Add a shared PowerShell helper for GitHub job summary rendering and TRX slow-test extraction.
  - Add command timing inputs that workflows can pass without changing command exit behavior.
  - Optionally update the existing broad `ci` job to call the helper after `scripts/ci.ps1`, reporting full closeout status and any structured-output limitations.
  - Keep `scripts/ci.ps1` command selection unchanged.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"`
  - `git diff --check -- scripts\Write-CiRuntimeSummary.ps1 .github\workflows\ci.yml tests\VeloFile.Corpus.Tests`
- Expected observable result: A reusable summary helper exists, can be tested locally, and the current broad CI path can report its lane/status without claiming fast PR confidence.
- Implementation evidence:
  - Added `scripts/Write-CiRuntimeSummary.ps1`.
  - Added focused tests in `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`.
  - Updated `.github/workflows/ci.yml` to keep `./scripts/ci.ps1` unchanged and add an `if: always()` summary step with `contents: read` permissions.
  - Recorded M1 rationale in `docs/changes/2026-05-18-pr-ci-validation-tiering/explain-change.md`.
  - Focused M1 tests failed before implementation because the summary helper and workflow hook were absent, then passed after implementation.
- Commit message: `M1: add CI runtime summary foundation`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
  - code-review-r1 requested changes for PRCI-CR1
  - review-resolution implemented for PRCI-CR1
  - code-review-r2 returned clean-with-notes
  - milestone closed
- Risks:
  - Summary failures could mask validation failures.
  - TRX parsing could overfit one MSTest output shape.
- Rollback/recovery:
  - Remove the helper invocation from workflows while keeping the helper/tests for follow-up, or revert the helper if it breaks CI before new lanes rely on it.

### M2. Fast PR Shadow Lane And Workflow Contract Tests

- Milestone state: closed
- Goal: Add `ci-fast-required` as a shadow-running ordinary PR confidence lane while the existing broad `ci` check can remain required externally during the shadow period.
- Requirements: R1-R3, R11-R27, R40-R50, R54-R64, R65-R69, AC2-AC8, AC12-AC18, AC19-AC20.
- Files/components likely touched:
  - `.github/workflows/ci.yml`
  - `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs` or equivalent test-owned YAML model
  - `README.md` if fast-lane shadow status needs contributor guidance
- Dependencies:
  - M1 summary helper.
  - Matching test spec must define the workflow contract tests and diagnostics.
- Tests to add/update:
  - Workflow contract validation finds `ci-fast-required`.
  - `ci-fast-required` runs on `pull_request` and agreed active push branches.
  - `ci-fast-required` runs on a Windows runner, uses `pwsh`, and sets up .NET before validation commands.
  - `ci-fast-required` runs `dotnet --info` before restore/build validation.
  - `ci-fast-required` runs restore/build before any `--no-build` tests.
  - `ci-fast-required` validates UI contracts against production inputs.
  - `ci-fast-required` runs Core/App/Windows test projects directly without category filters.
  - `ci-fast-required` runs Corpus `Fast|Contract` and `CorpusScript&Smoke`.
  - `ci-fast-required` does not call `scripts/ci.ps1` and does not run `ReleaseEvidence` by default.
  - Contract diagnostics name the offending lane, runner, shell, SDK ordering, or command-selection failure.
- Implementation steps:
  - Add a structured workflow parser and test-owned model, using a reviewed test-only YAML parser dependency if the test spec accepts it.
  - Add `ci-fast-required` to `ci.yml` as a shadow job with `runs-on: windows-latest`, `defaults.run.shell: pwsh`, `actions/setup-dotnet`, `dotnet --info`, explicit restore/build, UI contract validation, direct product test commands, Corpus fast/contract, and Corpus script smoke.
  - Order the fast-lane validation commands as `dotnet --info`, then restore/build, then validation/test commands.
  - Emit TRX or equivalent structured test output for all test commands that start.
  - Call the summary helper in a way that still writes a summary after failures once the job has started.
  - Keep the existing broad `ci` job during shadow rollout unless maintainers have already recorded branch-protection handoff.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"`
  - M2 validation must include workflow-contract proof that `ci-fast-required` contains `dotnet --info` before `dotnet restore` and `dotnet build`.
  - `dotnet --info`
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug --no-restore`
  - `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --no-build`
  - `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --no-build`
  - `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --no-build`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=CorpusScript&TestCategory=Smoke"`
- Expected observable result: Hosted PRs can run `ci-fast-required` as a non-required shadow lane, and static workflow tests prove it has the required command selection and environment contract.
- Implementation evidence:
  - Added `ci-fast-required` to `.github/workflows/ci.yml` while keeping the existing broad `ci` job in place for shadow rollout.
  - Added test-owned structured workflow parsing in `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs` with test-scoped `YamlDotNet`.
  - Added workflow contract tests in `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`.
  - Updated `docs/changes/2026-05-18-pr-ci-validation-tiering/explain-change.md` with M2 rationale and scope notes.
- Commit message: `M2: add fast PR CI shadow lane`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
  - code-review-r3 requested changes for PRCI-CR2
  - review-resolution implemented for PRCI-CR2
  - code-review rerun requested
  - code-review-r4 returned clean-with-notes
  - milestone closed
- Risks:
  - Adding the fast lane may accidentally make contributors treat it as release readiness.
  - Workflow contract tests could become brittle if they inspect YAML by string matching instead of structure.
  - The old broad `ci` and new `ci-fast-required` can both run during shadow, temporarily increasing hosted minutes.
- Rollback/recovery:
  - Disable or remove the `ci-fast-required` job and leave the existing broad `ci` job unchanged.
  - Keep workflow contract tests failing until the intended fast lane is restored, rather than weakening the policy.

### M3. Release-Evidence Workflow

- Milestone state: closed
- Goal: Add `ci-release-evidence` so expensive release evidence is explicit, scheduled, manually runnable, release-triggered, and separate from ordinary PR defaults.
- Requirements: R4-R8, R11, R28-R34, R40-R48, R52, R54-R60, R62-R64, R65-R69, AC2, AC9, AC12, AC16-AC20.
- Files/components likely touched:
  - `.github/workflows/release-evidence.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `scripts/Write-CiRuntimeSummary.ps1`
  - `docs/changes/2026-05-18-pr-ci-validation-tiering/` for trigger decisions if needed
- Dependencies:
  - M1 summary helper.
  - M2 workflow parser/model.
  - M3 implementation must record the selected release branch/tag patterns in change evidence before the workflow can be considered closed.
- Tests to add/update:
  - Workflow contract validation finds `ci-release-evidence`.
  - `ci-release-evidence` supports `workflow_dispatch`, a daily or nightly schedule, release branch/tag patterns, and `merge_group`.
  - `ci-release-evidence` runs on Windows, uses `pwsh`, and sets up .NET before validation commands.
  - `ci-release-evidence` runs restore/build before `--no-build` release-evidence tests.
  - `ci-release-evidence` runs Corpus `TestCategory=ReleaseEvidence`.
  - Summary reports whether `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` ran, were absent, or were intentionally not selected.
  - Ordinary PR triggers do not run `ci-release-evidence` by default.
- Implementation steps:
  - Create `.github/workflows/release-evidence.yml` with `workflow_dispatch`, a non-top-of-hour nightly UTC cron, release branch/tag patterns, and `merge_group`.
  - Use `windows-latest`, `pwsh`, and the approved .NET SDK setup before restore/build/test commands.
  - Run explicit Corpus release-evidence validation with structured output.
  - Use the summary helper and upload TRX artifacts when present.
  - Keep benchmark, visual, and manual-evidence statuses explicit in the summary rather than silently implying they ran.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"`
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug --no-restore`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=ReleaseEvidence"`
- Expected observable result: Release evidence is available through its own hosted lane and cannot be mistaken for the ordinary PR default.
- Implementation evidence:
  - Added `.github/workflows/release-evidence.yml` with `workflow_dispatch`, non-top-of-hour nightly schedule `17 3 * * *`, `merge_group`, and release push patterns.
  - Selected release branch pattern: `release/**`.
  - Selected release tag patterns: `v*` and `v*-rc*`.
  - The workflow runs `ci-release-evidence` on `windows-latest` with job-level `pwsh`, `actions/setup-dotnet@v4`, `dotnet --info`, restore, build, and explicit Corpus `TestCategory=ReleaseEvidence` validation with TRX output.
  - The workflow summary reports `ReleaseEvidence=run`, `Benchmark=run`, `Visual=not selected in this lane`, `ManualEvidence=absent from current test inventory`, `CorpusScript Smoke=not selected in this lane`, and `Full closeout=not run`.
  - Extended the workflow model to parse push tag patterns and schedule crons.
  - Added workflow contract tests for release-evidence triggers, hosted execution environment, command ordering, ReleaseEvidence filter selection, summary status, TRX artifact upload, and category-status inventory drift.
  - Added runtime summary coverage for release-evidence category status output.
  - PRCI-CR3 resolution extended the workflow model to expose step `if` values and strengthened release-evidence contract tests for validation-step failure semantics and summary-after-failure behavior.
- Commit message: `M3: add release evidence workflow`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated
  - validation notes updated
  - milestone committed
- Risks:
  - The release-evidence lane could become too broad or too narrow without clear summary status.
  - The schedule or tag glob could miss a future release naming pattern.
- Rollback/recovery:
  - Keep `workflow_dispatch` while disabling schedule/tag triggers until the trigger issue is corrected.
  - Use local `scripts/ci.ps1` or manual release-evidence commands for release readiness while the workflow is repaired.

### M4. Full Closeout Workflow

- Milestone state: closed
- Goal: Add `ci-full-closeout` as a manual broad closeout lane that invokes `scripts/ci.ps1` unchanged.
- Requirements: R9-R10, R35-R39, R40-R48, R52-R53, R58-R59, R62-R64, R65-R69, AC2, AC10-AC12, AC16-AC20.
- Files/components likely touched:
  - `.github/workflows/closeout.yml`
  - `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`
  - `scripts/Write-CiRuntimeSummary.ps1`
  - `scripts/ci.ps1` only if non-semantic reporting hooks are accepted by the test spec
- Dependencies:
  - M1 summary helper.
  - M2 workflow parser/model.
- Tests to add/update:
  - Workflow contract validation finds `ci-full-closeout`.
  - `ci-full-closeout` supports `workflow_dispatch`.
  - `ci-full-closeout` runs on Windows, uses `pwsh`, and sets up .NET before invoking closeout.
  - `ci-full-closeout` invokes `scripts/ci.ps1`.
  - `scripts/ci.ps1` remains broad and is not narrowed to fast filters.
  - Ordinary PR triggers do not run `ci-full-closeout` by default.
- Implementation steps:
  - Create `.github/workflows/closeout.yml` with manual dispatch and stable job/check name `ci-full-closeout`.
  - Invoke `./scripts/ci.ps1` rather than duplicating its command sequence in YAML.
  - Call the summary helper with `Full closeout: run` and honest limitations when structured test output is unavailable.
  - Preserve `scripts/ci.ps1` local behavior and broad command semantics.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation"`
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- Expected observable result: Maintainers can run broad closeout in GitHub Actions manually, and workflow tests prove it remains separate from ordinary PR fast confidence.
- Implementation evidence:
  - Added `.github/workflows/closeout.yml` with manual `workflow_dispatch`, stable job/check name `ci-full-closeout`, `windows-latest`, job-level `pwsh`, `actions/setup-dotnet@v4`, and unchanged `./scripts/ci.ps1` invocation.
  - The closeout workflow writes a runtime summary with `Full closeout=run`, broad unfiltered status text for release evidence and Corpus script smoke, explicit failed-command context from `steps.full_closeout.outcome`, and best-effort TRX artifact upload when structured output exists.
  - Extended `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs` with closeout workflow contract tests, failure-semantics diagnostics, and a guard that `scripts/ci.ps1` remains a broad unfiltered closeout command.
  - Extended `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs` with `ValidateFullCloseoutLane` so future changes fail if closeout runs on the wrong hosted environment, skips SDK setup, narrows command selection, uses `continue-on-error`, or drops `if: always()` summary behavior.
  - Preserved `scripts/ci.ps1` unchanged.
- Commit message: `M4: add full closeout workflow`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
  - code-review-r7 returned clean-with-notes
  - milestone closed
- Risks:
  - Duplicating closeout commands in YAML would drift from `scripts/ci.ps1`.
  - Broad closeout runtime may still be expensive by design.
- Rollback/recovery:
  - Revert `closeout.yml` and continue using local `scripts/ci.ps1` until the hosted manual lane is corrected.

### M5. Shadow-Run Evidence, Final Policy Transition, And Contributor Guidance

- Milestone state: closed
- Goal: Record at least one shadow PR cycle, finalize the ordinary PR workflow policy after review, and update contributor-facing guidance without claiming external settings changed before maintainer handoff.
- Requirements: R11-R13, R49-R53, R61, AC13-AC15.
- Files/components likely touched:
  - `.github/workflows/ci.yml`
  - `README.md`
  - `CONTRIBUTING.md` if present or created by prior work
  - `docs/project-map.md`
  - `docs/changes/2026-05-18-pr-ci-validation-tiering/shadow-run.md`
  - `docs/changes/2026-05-18-pr-ci-validation-tiering/branch-protection-handoff.md`
  - `docs/plans/2026-05-18-pr-ci-validation-tiering.md`
- Dependencies:
  - M2 shadow lane has run for at least one PR cycle. Satisfied on 2026-05-18 by PR #4 run `26062568345`: `ci-fast-required` passed in 7m20s and the temporary broad `ci` shadow job passed in 16m01s.
  - M3 and M4 workflows exist or the plan is revised to keep their absence explicit.
  - Maintainers provide or confirm branch-protection handoff evidence before repo artifacts claim GitHub branch protection changed. Current evidence records `main` branch protection as not configured (HTTP 404), so no handoff is claimed.
- Tests to add/update:
  - Workflow contract validation preserves the final policy: ordinary PR defaults run `ci-fast-required`, while release evidence and full closeout are outside ordinary PR defaults.
  - Documentation tests or focused scans prove contributor guidance names fast PR confidence, release-evidence validation, and full closeout separately.
  - Change-record checks prove shadow-run evidence exists before required-check transition is claimed.
  - Rollout evidence tests prove the shadow-run and branch-protection handoff records exist and do not overclaim external settings.
- Implementation steps:
  - Record shadow-run comparison: fast lane runtime, failures, selected categories, broad check pass/fail when available, and any limitations.
  - If maintainers approve the handoff, record branch-protection evidence and update repository guidance to name `ci-fast-required` as the intended ordinary required check.
  - Remove the temporary broad PR job from ordinary PR triggers only after handoff evidence exists, leaving `ci-full-closeout` and release-evidence paths available.
  - Update `docs/project-map.md` to describe the implemented workflow topology.
  - Keep rollback instructions explicit: make the broad closeout check required again and leave `ci-fast-required` optional.
- Validation commands:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"`
  - `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout" README.md docs specs .github`
  - `git diff --check -- .github\workflows README.md CONTRIBUTING.md docs\project-map.md docs\changes\2026-05-18-pr-ci-validation-tiering docs\plans\2026-05-18-pr-ci-validation-tiering.md tests\VeloFile.Corpus.Tests`
- Expected observable result: The repository records shadow evidence, contributor guidance matches the new policy, and any branch-protection claim is backed by maintainer handoff evidence.
- Implementation evidence:
  - Recorded accepted hosted shadow-run evidence in `docs/changes/2026-05-18-pr-ci-validation-tiering/shadow-run.md` for PR #4 run `26062568345`.
  - Recorded branch-protection handoff status in `docs/changes/2026-05-18-pr-ci-validation-tiering/branch-protection-handoff.md`: GitHub returned `Branch not protected` (HTTP 404), so no maintainer handoff or external required-check change is claimed.
  - Added `CiRolloutEvidenceTests` and contributor guidance assertions to preserve the shadow-run evidence, handoff caveat, lane names, summary wording, release-readiness warning, and rollback path.
  - Updated `README.md`, `CONTRIBUTING.md`, and `docs/project-map.md` to describe implemented hosted CI lanes and the current rollout boundary.
  - Kept the temporary broad `ci` PR job in place because branch-protection handoff is not recorded.
- Commit message: `M5: record PR CI handoff evidence`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed
  - code-review-r9 returned clean-with-notes
  - milestone closed
- Risks:
  - Branch protection is external and may lag behind repository files.
  - Removing the temporary broad PR job too early could break the required check during rollout.
- Rollback/recovery:
  - Keep or restore the broad `ci` job on ordinary PRs until maintainers complete handoff.
  - Revert documentation claims that `ci-fast-required` is required if external settings are not confirmed.

### M6. Lifecycle Closeout

- Milestone state: closed
- Goal: Close the plan only after implementation milestones are closed and downstream lifecycle gates provide their own evidence.
- Requirements: all in-scope requirements through final verification and PR handoff.
- Files/components likely touched:
  - `docs/plans/2026-05-18-pr-ci-validation-tiering.md`
  - `docs/plan.md`
  - `docs/changes/2026-05-18-pr-ci-validation-tiering/explain-change.md`
  - verification and PR artifacts created by later skills
- Dependencies:
  - M1-M5 closed or explicitly removed by reviewed plan revision.
  - Required code-review and review-resolution cycles closed.
  - `explain-change`, `verify`, and `pr` stages completed when invoked by the workflow.
- Tests to add/update:
  - None by itself; this milestone records downstream evidence.
- Implementation steps:
  - Ensure all in-scope implementation milestones are closed.
  - Run `explain-change`.
  - Run final `verify`.
  - Prepare PR handoff through the `pr` skill when the branch is ready.
  - Move this plan from Draft or Active to Done only after final lifecycle evidence exists.
- Validation commands:
  - Final commands are owned by `verify`; expected broad command includes `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` unless `verify` records a limitation.
- Expected observable result: The lifecycle evidence, plan status, and plan index all agree on the final state.
- Commit message: `M6: close PR CI tiering plan`
- Milestone closeout:
  - validation passed
  - progress updated
  - decision log updated if needed
  - validation notes updated
  - milestone committed if changed
  - explain-change completed
  - verify completed
  - PR #4 updated and marked ready for review
  - milestone closed
- Risks:
  - Final closeout could be attempted before branch-protection handoff or review-resolution evidence is ready.
- Rollback/recovery:
  - Keep the plan Active and record the missing gate rather than claiming Done.

## Validation Plan

Plan-stage validation:

```powershell
git diff --check -- docs\plans\2026-05-18-pr-ci-validation-tiering.md docs\plan.md docs\changes\2026-05-18-pr-ci-validation-tiering\change.yaml docs\architecture\system\architecture.md docs\adr\0012-hosted-pr-ci-validation-tiers.md
Select-String -Path docs\plans\2026-05-18-pr-ci-validation-tiering.md,docs\plan.md,docs\changes\2026-05-18-pr-ci-validation-tiering\change.yaml,docs\architecture\system\architecture.md,docs\adr\0012-hosted-pr-ci-validation-tiers.md -Pattern '[ \t]+$'
rg -n "PR CI validation tiering|ci-fast-required|ci-release-evidence|ci-full-closeout|next_stage: test-spec|accepted by architecture-review-r1" docs\plans docs\plan.md docs\changes\2026-05-18-pr-ci-validation-tiering docs\architecture\system\architecture.md docs\adr\0012-hosted-pr-ci-validation-tiers.md
```

Implementation-stage validation is listed per milestone. The matching test spec translates the requirements and edge cases into concrete test names before implementation starts.

Final verification is owned by `verify` after implementation, code review, and any review-resolution cycles are closed.

## Risks And Recovery

- Risk: The fast lane misses a release-evidence regression. Recovery: keep `ci-release-evidence`, `ci-full-closeout`, and local `scripts/ci.ps1` explicit and required for release readiness or milestone closeout.
- Risk: Branch protection cannot be changed through repository files. Recovery: record maintainer handoff evidence separately and do not claim the setting changed until confirmed.
- Risk: Workflow YAML tests become brittle. Recovery: parse YAML into a test-owned model and assert semantic command/order properties instead of relying only on raw string checks.
- Risk: Summary helper reports false details. Recovery: fail or report limitations when structured output is missing; do not fabricate slow-test rows.
- Risk: Hosted lanes accidentally use non-Windows runners or Windows PowerShell. Recovery: workflow contract tests fail unless a reviewed exception records equivalent Windows validation.
- Risk: Broad closeout is removed too early. Recovery: restore broad ordinary PR job or make broad closeout required again while keeping fast CI optional.
- Risk: Cache setup complicates the correctness story. Recovery: defer caching or keep it strictly secondary; cache misses must never trigger full release evidence as a fallback.

## Dependencies

- `plan-review` must approve this plan before implementation.
- `specs/pr-ci-validation-tiering.test.md` must be created and reviewed before implementation.
- Workflow contract tests should be implemented before or alongside workflow changes in each milestone.
- M2 depends on M1 for summary helper wiring.
- M3 and M4 depend on the workflow parser/model from M2.
- M5 depends on at least one shadow PR cycle and maintainer evidence for any branch-protection claim.
- External branch-protection changes are maintainer-operated and cannot be completed by repository file edits alone.

## Progress

- [x] Proposal accepted.
- [x] Spec approved by `spec-review-r2`.
- [x] Architecture and ADR approved by `architecture-review-r1`.
- [x] Execution plan drafted.
- [x] Plan reviewed by `plan-review-r1`; PRCI-PLR1 accepted for revision.
- [x] Plan approved by `plan-review-r2`.
- [x] Test spec created.
- [x] Test spec reviewed by `test-spec-review-r1`.
- [x] M1 implemented; code-review requested.
- [x] M1 reviewed by `code-review-r1`; PRCI-CR1 required review-resolution.
- [x] M1 review-resolution completed for PRCI-CR1; code-review rerun requested.
- [x] M1 code-review rerun completed by `code-review-r2`; M1 closed.
- [x] M2 implemented; code-review requested.
- [x] M2 reviewed by `code-review-r3`; PRCI-CR2 requires review-resolution.
- [x] M2 review-resolution implemented for PRCI-CR2; code-review rerun requested.
- [x] M2 code-review rerun completed by `code-review-r4`; M2 closed.
- [x] M3 implemented; code-review requested.
- [x] M3 reviewed by `code-review-r5`; PRCI-CR3 requires review-resolution.
- [x] M3 review-resolution completed for PRCI-CR3; code-review rerun requested.
- [x] M3 code-review rerun completed by `code-review-r6`; M3 closed.
- [x] M4 implemented; code-review requested.
- [x] M4 reviewed by `code-review-r7`; M4 closed.
- [x] M5 implementation preflight stopped because hosted shadow-run evidence is missing.
- [x] M5 code-review-r8 recorded the hosted shadow-run evidence blocker.
- [x] Hosted PR #4 produced accepted `ci-fast-required` shadow-run evidence in run `26062568345`.
- [x] M5 implemented; code-review requested.
- [x] M5 reviewed by `code-review-r9`; M5 closed.
- [x] Final explain-change completed.
- [x] Final verify completed; branch-ready for PR handoff.
- [x] PR #4 updated, marked ready for review, and merged.
- [x] Final lifecycle closeout completed.

## Current Handoff Summary

- Current milestone: complete
- Current milestone state: done
- Last reviewed milestone: M5 code-review-r9
- Review status: M5 code-review-r9 returned clean-with-notes; PRCI-CR4 is closed by hosted PR #4 shadow-run evidence.
- Remaining in-scope implementation milestones: none
- Next stage: post-merge branch-protection handoff and broad-shadow cleanup, tracked by [2026-05-19-pr-ci-post-merge-handoff.md](2026-05-19-pr-ci-post-merge-handoff.md)
- Final closeout readiness: complete
- Reason final closeout is or is not ready: implementation milestones, explain-change, verify, PR handoff, and merge are complete. External branch-protection handoff remains separate follow-up work.

## Decision Log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-18 | Stage the rollout through reporting, shadow fast CI, release evidence, full closeout, and handoff evidence. | The spec requires shadow-running before branch protection changes and keeps release evidence explicit. | Replace broad PR CI immediately. |
| 2026-05-18 | Keep the existing broad `ci` job during the fast-lane shadow milestone unless maintainer handoff already exists. | This preserves rollback and avoids breaking external required-check settings during the shadow period. | Remove the broad PR job in the same slice that first adds `ci-fast-required`. |
| 2026-05-18 | Use static workflow contract tests over a structured YAML model. | Architecture-review approved structured YAML parsing as the guardrail against workflow drift. | Ad hoc raw string checks as the only contract protection. |
| 2026-05-18 | Put summary rendering in a shared PowerShell helper. | ADR 0012 rejects duplicated inline summary fragments and keeps the helper aligned with hosted `pwsh`. | A new .NET reporting tool in the first slice; duplicated YAML snippets. |
| 2026-05-18 | Use release branch pattern `release/**` and release tag patterns `v*` and `v*-rc*` for `ci-release-evidence`. | The spec requires documented release branch/tag patterns, and these match the repository's existing `v*` release trigger while adding an explicit release-candidate tag pattern. | Leave trigger patterns to later review; ordinary PR trigger for release evidence. |

## Surprises And Discoveries

- The PR CI validation tiering test spec is approved and active for M1 implementation.
- The current hosted PR workflow still contains one broad `ci` job that invokes `scripts/ci.ps1`.
- Existing corpus test infrastructure already has category inventory, release-evidence tier tests, public script smoke tests, and runtime report tests that the new workflow tests can extend.
- There is no central package props file; any test-only YAML parser dependency would be added directly to the relevant test project unless the test spec chooses another approach.
- The M4 closeout workflow can upload TRX only when the invoked broad script produces structured output; with the current unchanged `scripts/ci.ps1`, the summary helper honestly reports missing structured output instead of fabricating slow-test details.
- The 2026-05-18 M5 implementation preflight found no hosted shadow PR cycle for the local M2-M4 workflow changes. Local `main` is ahead of `origin/main` by 13 commits, GitHub Actions run history only shows the older broad `ci` workflow, PR #3 run `26002964319` had a single `ci` job, and the GitHub API reported `main` branch protection as not configured.
- PR #4 created the needed hosted shadow-run surface. Earlier runs exposed hosted-only issues in summary argument binding and timeout-sensitive App tests; after fixes, run `26062568345` passed `ci-fast-required` in 7m20s and broad `ci` in 16m01s.

## Validation Notes

M1 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` failed before implementation as expected: the summary helper script and workflow hook were missing.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed after implementation: 4 tests passed.
- `git diff --check -- scripts\Write-CiRuntimeSummary.ps1 .github\workflows\ci.yml tests\VeloFile.Corpus.Tests` passed with Git LF-to-CRLF working-copy warnings only.
- `Select-String -Path scripts\Write-CiRuntimeSummary.ps1,.github\workflows\ci.yml,tests\VeloFile.Corpus.Tests\TestRuntime\CiRuntimeSummaryTests.cs -Pattern '[ \t]+$'` produced no matches.

M1 PRCI-CR1 review-resolution validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` failed first after test updates because `-FailedOutcome` support and workflow failure-context wiring were missing.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed after resolution: 4 tests passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~WorkflowContract"` completed with no matching tests in this project.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed during code-review-r2: 4 tests passed.

M2 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` failed before workflow implementation as expected because `ci-fast-required` was missing.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed after implementation: 6 tests passed.
- `dotnet --info` passed: .NET SDK 10.0.203 on Windows.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug --no-restore` passed with 0 warnings and 0 errors.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .` passed.
- `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --no-build` passed: 168 tests.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --no-build` passed: 168 tests.
- `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --no-build` passed: 52 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"` passed: 81 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=CorpusScript&TestCategory=Smoke"` passed: 6 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed: 4 tests.

M3 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` failed before workflow implementation as expected because `.github/workflows/release-evidence.yml` was missing.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed after implementation: 9 tests passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed after release-evidence summary coverage: 6 tests passed.
- `dotnet restore VeloFile.sln` passed.
- `dotnet build VeloFile.sln -c Debug --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=ReleaseEvidence"` passed: 10 tests in 5 m 12 s after final build.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

M3 code-review-r5 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed during review: 9 tests passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed during review: 6 tests passed.
- `git diff --check` passed during review with Git LF-to-CRLF working-copy warnings only.
- Review finding PRCI-CR3 remains open because release-evidence workflow contract tests do not prove the `continue-on-error` and summary `if: always()` failure semantics required by PRCI-T020 and PRCI-T023.

M3 PRCI-CR3 review-resolution validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed after resolution: 10 tests passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed after resolution: 6 tests passed.
- `git diff --check` passed after resolution with Git LF-to-CRLF working-copy warnings only.
- The new invalid workflow fixture proves contract diagnostics are emitted for `ContinueOnError=true` on `test_release_evidence` and a missing release-evidence summary `if: always()` condition.

M3 code-review-r6 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed during review: 10 tests passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"` passed during review: 6 tests passed.
- `git diff --check` passed during review with no output.

M4 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` failed before workflow implementation as expected because `.github/workflows/closeout.yml` was missing; it passed after implementation with 14 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation"` passed with 16 tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: SDK 10.0.203 on Windows; build 0 warnings/0 errors; UI contract validation passed; Core 168, App 168, Windows 52, Corpus 110 tests passed.
- `git diff --check` passed with Git LF-to-CRLF working-copy warnings only.

M4 code-review-r7 validation:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"` passed during review: 14 tests.
- `git diff --check` passed during review with no output.

M5 implementation preflight:

- `git status --short --branch` showed local `main` clean and ahead of `origin/main` by 13 commits.
- `gh run list --limit 20 --json databaseId,workflowName,displayTitle,event,headBranch,headSha,status,conclusion,createdAt,updatedAt,url` showed no hosted run for local HEAD `9ec9c78ce54cc4c5dd701cec76ff1b49ba8293e7`; the newest hosted CI runs are older broad `ci` runs.
- `gh run view 26002964319 --json name,event,headSha,status,conclusion,jobs,url,createdAt,updatedAt` showed PR #3 had one `ci` job and no `ci-fast-required` shadow job.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` returned `Branch not protected` (HTTP 404), so no branch-protection handoff is recorded or claimed.
- M5 stopped before test/spec/workflow edits because PRCI-M001 requires at least one hosted PR cycle after M2 before shadow-run evidence can be recorded.

M5 code-review-r8:

- Review status: blocked.
- Finding: PRCI-CR4.
- Review record: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/code-review-r8.md`.
- The review confirmed the blocker bookkeeping is aligned with the spec, but M5 cannot enter implementation review or final closeout until hosted `ci-fast-required` shadow-run evidence exists.

M5 implementation validation:

- Hosted PR #4 run `26062568345` passed after the M2-M4 workflow commits and hosted fixes: `ci-fast-required` passed in 7m20s and broad `ci` passed in 16m01s.
- `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'` returned `Branch not protected` (HTTP 404), so `branch-protection-handoff.md` records no maintainer handoff and no external required-check claim.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` failed before evidence and guidance were added, as expected.
- Final M5 validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"` passed: 20 tests.
  - `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout" README.md docs specs .github` passed with expected matches in guidance, specs, workflows, and change records.
  - `git diff --check -- .github\workflows README.md CONTRIBUTING.md docs\project-map.md docs\changes\2026-05-18-pr-ci-validation-tiering docs\plans\2026-05-18-pr-ci-validation-tiering.md tests\VeloFile.Corpus.Tests` passed with Git LF-to-CRLF working-copy warnings only.

M5 code-review-r9 validation:

- Review status: clean-with-notes.
- Review record: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/code-review-r9.md`.
- Reviewer rerun `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"` passed: 20 tests.
- Reviewer rerun `git diff --check HEAD~1..HEAD` passed with no output.
- Latest hosted PR run for commit `91a843f` was in progress during review and is not claimed as passed.

Explain-change validation:

- `git diff --check -- docs\changes\2026-05-18-pr-ci-validation-tiering\explain-change.md` passed with Git LF-to-CRLF working-copy warnings only.
- Latest hosted PR run for commit `0e13d8ba537c4befd461fa2f712371ae00072ba6` was queued when explain-change ran and was not claimed as passed at that stage.

Verify validation:

- `gh run watch 26063842493 --interval 30 --exit-status` passed for the latest hosted PR run at commit `5188c33459218edd382cf50ab9adfca973b0c974`.
- `gh run view 26063842493 --json status,conclusion,jobs,url,headSha,createdAt,updatedAt` showed workflow `ci` completed with conclusion `success`; `ci-fast-required` passed in 6m32s and broad `ci` passed in 17m11s.
- `docs/changes/2026-05-18-pr-ci-validation-tiering/verify-report.md` records branch-ready verification with branch-protection handoff caveat and CI platform warnings.

PR handoff validation:

- `gh pr edit 4 --body <review-ready body>` passed and updated the PR body with summary, rationale, spec/plan/architecture links, tests, hosted CI evidence, review-resolution summary, risks, rollback, and reviewer notes.
- `gh pr ready 4` passed and marked PR #4 ready for review.

## Outcome And Retrospective

Not started. Fill this after implementation milestones and downstream lifecycle closeout complete.

## Readiness

See `Current Handoff Summary` for live state. Final lifecycle closeout is complete and PR #4 merged; post-merge branch-protection handoff remains separate follow-up work.
