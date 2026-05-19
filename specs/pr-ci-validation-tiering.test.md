# PR CI Validation Tiering Test Spec

## Status

active

This test spec was approved by maintainer review on 2026-05-18 and is active for implementation. Implementation should start at M1 in the approved execution plan.

## Related Spec And Plan

- Feature spec: [pr-ci-validation-tiering.md](pr-ci-validation-tiering.md)
- Spec review: [spec-review-r2.md](../docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/spec-review-r2.md)
- Architecture: [architecture.md](../docs/architecture/system/architecture.md)
- ADR: [0012-hosted-pr-ci-validation-tiers.md](../docs/adr/0012-hosted-pr-ci-validation-tiers.md)
- Architecture review: [architecture-review-r1.md](../docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/architecture-review-r1.md)
- Execution plan: [2026-05-18-pr-ci-validation-tiering.md](../docs/plans/2026-05-18-pr-ci-validation-tiering.md)
- Plan review: [plan-review-r2.md](../docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/plan-review-r2.md)

## Testing Strategy

The proof strategy is static-contract first, then focused helper execution, then manual hosted evidence only for boundaries that cannot be changed by repository files.

- Workflow contract tests parse committed GitHub Actions YAML into a test-owned model and assert lane names, triggers, runner OS, shell, SDK setup ordering, command ordering, command filters, summaries, permissions, artifacts, and release-evidence separation.
- Summary helper tests execute the PowerShell helper locally with temporary inputs, sample TRX files, missing-output cases, and privacy probes.
- Existing Corpus category inventory, release-evidence tier, public script smoke, runtime report, and validation-command tests remain the proof surface for test category semantics and public-wrapper smoke preservation.
- Manual/change-record evidence is limited to external GitHub branch-protection handoff, shadow-run comparison, and hosted workflow run observations that cannot be proven from committed files.
- Production App/Core/Windows/Corpus behavior is not changed or retested except through existing project test commands selected by the workflow contract.

## Requirement Coverage Map

| Requirements | Coverage |
|---|---|
| R1, R2, R3 | PRCI-T002 |
| R4, R5, R6, R7, R8 | PRCI-T003, PRCI-M002 |
| R9, R10 | PRCI-T004 |
| R11, R12, R31 | PRCI-T005 |
| R13, R51 | PRCI-T028, PRCI-M002 |
| R14, R15, R16 | PRCI-T010 |
| R17 | PRCI-T011 |
| R18, R19, R20 | PRCI-T012 |
| R21 | PRCI-T013 |
| R22 | PRCI-T014 |
| R23, R24 | PRCI-T015 |
| R25, R26 | PRCI-T016 |
| R27 | PRCI-T017, PRCI-T034 |
| R28, R29 | PRCI-T018 |
| R30, R33 | PRCI-T019 |
| R32, R34 | PRCI-T020 |
| R35, R39 | PRCI-T021 |
| R36, R37, R38 | PRCI-T022 |
| R40, R41, R42, R43, R44, R45 | PRCI-T023 |
| R46 | PRCI-T024 |
| R47 | PRCI-T025 |
| R48 | PRCI-T026 |
| R49, R50 | PRCI-T027, PRCI-M001 |
| R52, R53 | PRCI-T029, PRCI-M003 |
| R54 | PRCI-T030 |
| R55, R56, R57, R60 | PRCI-T031 |
| R58, R59, R62, R63, R64 | PRCI-T032 |
| R61 | PRCI-T033 |
| R65, R68 | PRCI-T006 |
| R66, R69 | PRCI-T007 |
| R67 | PRCI-T008 |
| AC1 | PRCI-T035 |
| AC2 | PRCI-T002, PRCI-T003, PRCI-T004 |
| AC3 | PRCI-T010 |
| AC4 | PRCI-T011 |
| AC5 | PRCI-T012 |
| AC6 | PRCI-T013 |
| AC7 | PRCI-T014 |
| AC8 | PRCI-T015 |
| AC9 | PRCI-T003, PRCI-T018, PRCI-T019 |
| AC10, AC11 | PRCI-T021, PRCI-T022 |
| AC12 | PRCI-T017, PRCI-T023, PRCI-T024 |
| AC13 | PRCI-T027, PRCI-M001 |
| AC14 | PRCI-T033 |
| AC15 | PRCI-T031 |
| AC16 | PRCI-T026, PRCI-T032 |
| AC17 | PRCI-T006 |
| AC18 | PRCI-T007 |
| AC19 | PRCI-T008 |
| AC20 | PRCI-T009 |

## Example Coverage Map

| Example | Coverage |
|---|---|
| E1 ordinary PR receives fast required feedback | PRCI-T002, PRCI-T010, PRCI-T011, PRCI-T012, PRCI-T013, PRCI-T014, PRCI-T017 |
| E2 uncategorized product tests are not skipped by fast filtering | PRCI-T012, PRCI-T015 |
| E3 release evidence remains explicit | PRCI-T003, PRCI-T018, PRCI-T019 |
| E4 manual closeout remains broad | PRCI-T004, PRCI-T021, PRCI-T022 |
| E5 runtime regression is visible | PRCI-T023, PRCI-T024, PRCI-M001 |
| E6 rollback restores the broad required PR gate | PRCI-T029, PRCI-M003 |
| E7 hosted lane uses the approved execution environment | PRCI-T006, PRCI-T007, PRCI-T008 |
| E8 invalid hosted lane environment fails contract validation | PRCI-T006, PRCI-T007, PRCI-T008, PRCI-T009 |

## Edge Case Coverage

| Edge case | Coverage |
|---|---|
| EC1 Core/App/Windows tests lack category metadata | PRCI-T012 |
| EC2 Corpus fast/contract filter selects zero tests | PRCI-T013, PRCI-T030 |
| EC3 Corpus script smoke fails while product tests pass | PRCI-T014, PRCI-T016, PRCI-T017 |
| EC4 Release-evidence workflow is triggered manually on a PR branch | PRCI-T003, PRCI-T019 |
| EC5 Merge queue is enabled after implementation | PRCI-T003, PRCI-M002 |
| EC6 A release tag uses a new naming pattern | PRCI-T003, PRCI-M002 |
| EC7 TRX files are missing because a build failed before tests started | PRCI-T024 |
| EC8 Branch protection still requires the old broad CI check during shadow rollout | PRCI-T027, PRCI-T028 |
| EC9 Dependency cache misses on a hosted run | PRCI-T032 |
| EC10 Maintainer needs release readiness before merging a risky PR | PRCI-T029, PRCI-M003 |

## Test Cases

### Workflow Contract Model

PRCI-T001. Workflow contract model parses committed workflows and fixture workflows
- Covers: R61, AC14
- Level: contract
- Fixture/setup: `.github/workflows/*.yml` plus test-owned valid and invalid workflow YAML fixtures.
- Steps: Parse workflow YAML into a test-owned model that exposes events, jobs, job names, runner labels, defaults, steps, `uses`, `run`, `shell`, permissions, artifacts, and command order.
- Expected result: The model can inspect committed workflows and invalid fixtures without relying only on raw string search.
- Failure proves: The workflow contract tests cannot reliably prove the hosted CI policy.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowModel.cs`

PRCI-T002. Fast PR lane exists with ordinary PR triggers
- Covers: R1, R2, R3, AC2, E1
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml`.
- Steps: Locate a workflow job/check named `ci-fast-required`; inspect workflow events for `pull_request` and the accepted active push branch set.
- Expected result: `ci-fast-required` exists, runs on `pull_request`, and any active push branch selection is explicit.
- Failure proves: Ordinary PRs do not have the required stable fast confidence lane.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T003. Release-evidence lane exists with explicit release-readiness triggers
- Covers: R4, R5, R6, R7, R8, AC2, AC9, E3, EC4, EC5, EC6
- Level: contract
- Fixture/setup: `.github/workflows/release-evidence.yml` and change evidence for selected release branch/tag patterns.
- Steps: Locate `ci-release-evidence`; assert `workflow_dispatch`, a daily or nightly schedule, documented release branch/tag patterns, and `merge_group` support or recorded merge-queue non-use.
- Expected result: Release evidence is explicitly runnable outside ordinary PR defaults and trigger patterns are documented in workflow or change evidence.
- Failure proves: Full evidence is only nominally available or release trigger coverage is ambiguous.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T004. Full closeout lane exists as manual validation
- Covers: R9, R10, AC2, E4
- Level: contract
- Fixture/setup: `.github/workflows/closeout.yml`.
- Steps: Locate `ci-full-closeout`; assert the workflow supports `workflow_dispatch`.
- Expected result: Full closeout is manually runnable through a stable lane.
- Failure proves: Maintainers lost a hosted broad closeout entrypoint.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T005. Ordinary PR triggers do not run expensive evidence lanes by default
- Covers: R11, R12, R31, AC8
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml`, `.github/workflows/release-evidence.yml`, `.github/workflows/closeout.yml`.
- Steps: Inspect trigger events and job routing for ordinary `pull_request` events.
- Expected result: Ordinary PR defaults run `ci-fast-required`; `ci-release-evidence` and `ci-full-closeout` are not ordinary PR default jobs.
- Failure proves: Routine PR validation still pays release-evidence or closeout cost by default.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

### Hosted Execution Environment

PRCI-T006. Hosted lanes use Windows runners or documented Windows exceptions
- Covers: R65, R68, AC17, E7, E8
- Level: contract
- Fixture/setup: committed workflow files plus invalid fixtures for `ubuntu-latest`, `macos-latest`, and non-approved Windows labels.
- Steps: Inspect `runs-on` for `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout`; inspect linked exception evidence when a runner is not `windows-latest`.
- Expected result: Each lane uses `windows-latest` or records a reviewed Windows runner exception with equivalent validation expectations; Linux/macOS fixtures fail.
- Failure proves: Hosted validation may silently move away from the Windows-native execution boundary.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T007. PowerShell and repository script steps use `pwsh`
- Covers: R66, R69, AC18, E7, E8
- Level: contract
- Fixture/setup: committed workflow files plus invalid fixtures with `shell: powershell`, omitted shell defaults, and script invocations.
- Steps: Inspect workflow-level/job-level defaults and step-level shell settings for all PowerShell and repository script `run` steps.
- Expected result: Script steps use `pwsh` through defaults or explicit step shell, or record a reviewed exception; invalid fixtures fail with the offending lane and step.
- Failure proves: PowerShell command semantics can change without review.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T008. .NET SDK setup precedes validation commands in every hosted lane
- Covers: R67, AC19, E7, E8
- Level: contract
- Fixture/setup: committed workflow files plus invalid fixtures where `dotnet test`, UI contract validation, release evidence, or closeout runs before SDK setup.
- Steps: Locate `actions/setup-dotnet` or another approved SDK setup step and compare its order with restore, build, test, UI contract, release-evidence, and closeout commands.
- Expected result: SDK setup precedes all validation commands in each hosted lane.
- Failure proves: Workflow success depends on runner defaults instead of the repository-approved SDK setup.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T009. Workflow contract failures produce actionable diagnostics
- Covers: AC20 and error behavior for missing lanes, wrong runner, wrong shell, SDK-order violations, bad filters, and forbidden closeout calls.
- Level: contract
- Fixture/setup: invalid workflow fixtures for each violation family.
- Steps: Run workflow contract validation against each invalid fixture and capture diagnostics.
- Expected result: Diagnostics name the contract family, lane, offending step or command, expected value, and actual value.
- Failure proves: Future CI drift will fail with vague or unreviewable errors.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

### Fast PR Lane Commands

PRCI-T010. Fast lane runs `dotnet --info`, restore, and build in order
- Covers: R14, R15, R16, AC3, E1
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml`.
- Steps: Inspect the ordered `ci-fast-required` steps.
- Expected result: `dotnet --info` appears in `ci-fast-required` before `dotnet restore VeloFile.sln`; restore appears before `dotnet build VeloFile.sln -c Debug --no-restore`; build appears before any `--no-build` test command.
- Failure proves: The fast lane omits required observable SDK evidence or relies on stale build output.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T011. Fast lane validates production UI contracts
- Covers: R17, AC4, E1
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml`.
- Steps: Inspect `ci-fast-required` for the UI contract validator command.
- Expected result: The lane runs `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens` against `docs\ui\tokens.v1.json`, `docs\ui\ui-contract-scopes.v1.json`, and `src\VeloFile.App\Resources` with `--scope-root .`.
- Failure proves: Fast PR confidence no longer protects production UI contract inputs.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T012. Fast lane runs product test projects directly without category filters
- Covers: R18, R19, R20, AC5, E1, E2, EC1
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml` plus invalid fixtures using solution-level category filters or project-level product filters.
- Steps: Inspect product test commands for Core, App, and Windows test project paths and filter arguments.
- Expected result: Each product test project is invoked directly, uses `--no-build` after build, and has no `--filter` or `TestCategory=` filter.
- Failure proves: Uncategorized product tests can be silently skipped.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T013. Fast lane runs Corpus Fast or Contract filter and inventory proves it is non-empty
- Covers: R21, AC6, EC2
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml` and `VeloFile.Corpus.Tests` category inventory.
- Steps: Inspect the Corpus fast/contract command and run category inventory selection against the compiled Corpus test assembly.
- Expected result: The workflow command uses `TestCategory=Fast|TestCategory=Contract`, and the inventory confirms the selection is non-empty and uses accepted categories.
- Failure proves: The fast lane can pass without exercising Corpus fast/contract tests.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

PRCI-T014. Fast lane runs CorpusScript Smoke public-wrapper coverage
- Covers: R22, AC7, E1, EC3
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml`, `CorpusScriptSmokeTests`, and category inventory.
- Steps: Inspect the Corpus script smoke command and verify at least one public script smoke test is categorized `CorpusScript` + `Smoke`.
- Expected result: The workflow command uses `TestCategory=CorpusScript&TestCategory=Smoke`, the selected tests are non-empty, and public script smoke remains present.
- Failure proves: Fast PR confidence can hide public wrapper regressions.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusScriptSmokeTests.cs`

PRCI-T015. Fast lane excludes broad closeout and ReleaseEvidence by default
- Covers: R23, R24, AC8, E2
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml` plus invalid fixtures that call `scripts/ci.ps1`, run `ReleaseEvidence`, or apply the fast Corpus filter to the solution/product projects.
- Steps: Inspect `ci-fast-required` commands and filters.
- Expected result: The fast lane does not call `scripts/ci.ps1`, does not run `TestCategory=ReleaseEvidence`, and does not use a solution-level fast/contract filter for product tests.
- Failure proves: The ordinary PR lane either reverts to broad closeout or skips product tests by filter misuse.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T016. Fast lane fails validation failures and emits structured test output
- Covers: R25, R26, EC3
- Level: contract
- Fixture/setup: `.github/workflows/ci.yml`.
- Steps: Inspect fast-lane restore, build, UI contract, product test, Corpus fast/contract, and Corpus script smoke steps for normal fail-fast shell behavior and TRX or equivalent logger/output configuration on test commands.
- Expected result: Each required command is in a failing step, and every `dotnet test` command that starts writes structured output.
- Failure proves: A failed required command or missing structured output can hide in the fast lane.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T017. Fast lane summary distinguishes fast confidence from release readiness
- Covers: R27, R42, AC12, E1
- Level: contract
- Fixture/setup: summary helper test inputs for `ci-fast-required`.
- Steps: Invoke the summary helper with fast-lane metadata.
- Expected result: The summary names the lane as fast PR confidence and includes `ReleaseEvidence: not run in this lane`, `CorpusScript Smoke: run`, and `Full closeout: not run`.
- Failure proves: Contributors can mistake fast PR success for release readiness.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`

### Release Evidence And Full Closeout

PRCI-T018. Release-evidence lane runs build-producing validation before ReleaseEvidence tests
- Covers: R28, R29, AC9, E3
- Level: contract
- Fixture/setup: `.github/workflows/release-evidence.yml`.
- Steps: Inspect ordered release-evidence commands.
- Expected result: Restore/build run before any `--no-build` test, and the Corpus test command uses `TestCategory=ReleaseEvidence`.
- Failure proves: Release evidence can rely on missing build output or fail to select release-evidence tests.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T019. Release-evidence summary keeps expensive categories explicit
- Covers: R30, R33, AC9, E3, EC4, EC5, EC6
- Level: contract
- Fixture/setup: summary helper inputs for release-evidence lane and category inventory data.
- Steps: Invoke or inspect summary generation for release-evidence metadata.
- Expected result: Summary reports whether `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` ran, were absent, or were intentionally not selected.
- Failure proves: Release evidence can be claimed without knowing which expensive evidence categories actually ran.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`

PRCI-T020. Release-evidence failures fail the job and optional closeout is disclosed
- Covers: R32, R34
- Level: contract
- Fixture/setup: `.github/workflows/release-evidence.yml` plus summary helper inputs for optional full closeout.
- Steps: Inspect release-evidence command steps and any `scripts/ci.ps1` invocation in the release-evidence lane.
- Expected result: Release-evidence command failures fail the job; if the broad closeout command is also invoked, the summary reports full closeout ran.
- Failure proves: Release-evidence failure or extra closeout validation can be hidden.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`

PRCI-T021. Full closeout workflow invokes `scripts/ci.ps1` and fails on failure
- Covers: R35, R39, AC10, E4
- Level: contract
- Fixture/setup: `.github/workflows/closeout.yml`.
- Steps: Inspect the full closeout lane.
- Expected result: `ci-full-closeout` invokes `./scripts/ci.ps1` in a step whose failure fails the job.
- Failure proves: Hosted closeout no longer uses the broad repository closeout command.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T022. `scripts/ci.ps1` remains broad and locally runnable
- Covers: R36, R37, R38, AC11, E4
- Level: smoke
- Fixture/setup: `scripts/ci.ps1` and milestone/final validation environment.
- Steps: Statically inspect `scripts/ci.ps1` for broad restore/build/UI-contract/unfiltered solution test commands and absence of fast-only filters; run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` during M4 or final verification unless an environment limitation is recorded.
- Expected result: The script remains the broad closeout command and is not narrowed to fast filters.
- Failure proves: The change traded release confidence for faster PR feedback.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ValidationCommandDocumentationTests.cs`, M4/M6 validation notes.

### Runtime Summaries And Artifacts

PRCI-T023. Runtime summary reports required lane, tier, duration, and slow-test fields
- Covers: R40, R41, R42, R43, R44, R45, AC12, E5
- Level: contract
- Fixture/setup: `scripts/Write-CiRuntimeSummary.ps1`, sample TRX files, command timing inputs, and temporary `GITHUB_STEP_SUMMARY`.
- Steps: Invoke the helper for fast, release-evidence, and closeout lane metadata; inspect workflows for `if: always()` or equivalent summary-step behavior after validation has started.
- Expected result: Summary is written after a started job, including failure cases, and includes lane name, trigger, selected categories, release-evidence status, Corpus script smoke status, full closeout status, total duration, build duration, per test project duration, and slowest tests when available.
- Failure proves: Hosted CI results are opaque or misleading.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`

PRCI-T024. Runtime summary reports missing TRX or timing limitations honestly
- Covers: R46, AC12, EC7
- Level: contract
- Fixture/setup: summary helper inputs with missing TRX, failed build before tests, and missing timing data.
- Steps: Invoke the helper for each missing-output case.
- Expected result: Summary reports the failed command or missing structured output limitation and does not fabricate slow-test rows.
- Failure proves: Runtime reporting can claim evidence that was not produced.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`

PRCI-T025. Hosted lanes upload structured test output when present
- Covers: R47
- Level: contract
- Fixture/setup: workflow files and artifact-upload steps.
- Steps: Inspect workflows for `actions/upload-artifact` or equivalent upload of TRX/structured output when generated.
- Expected result: Workflows upload TRX or equivalent output when it exists, without making artifact upload a substitute for test pass/fail behavior.
- Failure proves: Slow-test and failure evidence can be lost after hosted runs.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T026. Runtime summaries and artifacts avoid secrets and private local details
- Covers: R48, R62, AC16
- Level: contract
- Fixture/setup: summary helper privacy fixtures containing token-like strings, signing-material labels, `C:\Users\...` paths, and private profile details.
- Steps: Invoke the helper and inspect workflow artifact/cache/summary inputs.
- Expected result: Summaries and artifacts exclude secrets, tokens, credentials, signing material, and unrelated private profile details; ordinary PR workflows require no new repository secrets.
- Failure proves: Hosted evidence can leak sensitive or irrelevant local machine data.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRuntimeSummaryTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

### Rollout, Handoff, And Documentation

PRCI-T027. Shadow-run evidence exists before required-check transition is claimed
- Covers: R49, R50, AC13, E5, EC8
- Level: contract
- Fixture/setup: `docs/changes/2026-05-18-pr-ci-validation-tiering/shadow-run.md` and branch-protection handoff evidence when present.
- Steps: Inspect change evidence before repository documentation claims `ci-fast-required` is the ordinary required check.
- Expected result: Shadow-run comparison records fast-lane runtime, failures, selected categories, and broad-check pass/fail when available.
- Failure proves: The project can weaken required PR validation before observing the fast lane.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs`

PRCI-T028. Branch-protection handoff claims require maintainer evidence
- Covers: R13, R51, AC13, EC8
- Level: manual
- Fixture/setup: `docs/changes/2026-05-18-pr-ci-validation-tiering/branch-protection-handoff.md`.
- Steps: If docs or change records claim `ci-fast-required` is required, verify maintainer evidence records the external branch-protection setting and review date.
- Expected result: Repository artifacts name `ci-fast-required` as the intended ordinary PR required check but do not claim the external setting changed until evidence exists.
- Failure proves: Repository docs can overstate external GitHub configuration state.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs` plus manual maintainer confirmation.

PRCI-T029. Release readiness and rollback paths remain explicit
- Covers: R52, R53, E6, EC10
- Level: contract
- Fixture/setup: README/contributor guidance, change evidence, closeout workflow, release-evidence workflow, and local `scripts/ci.ps1`.
- Steps: Inspect guidance and change evidence for release-readiness commands and rollback instructions.
- Expected result: Release readiness requires `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or an accepted release gate; rollback says to make broad closeout required again and leave fast CI optional.
- Failure proves: Fast PR confidence can be treated as release readiness or rollback can be ambiguous.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ValidationCommandDocumentationTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CiRolloutEvidenceTests.cs`

PRCI-T034. Contributor guidance names lane purpose without release-readiness overclaim
- Covers: R27, R41, AC12
- Level: contract
- Fixture/setup: `README.md` and any contributor guidance touched by M5.
- Steps: Scan guidance for `ci-fast-required`, `ci-release-evidence`, `ci-full-closeout`, `ReleaseEvidence: not run in this lane`, `Full closeout`, and release-readiness wording.
- Expected result: Guidance distinguishes fast PR confidence, release evidence, and full closeout.
- Failure proves: Contributor-facing UX is ambiguous even if workflows are technically correct.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ValidationCommandDocumentationTests.cs`

PRCI-T035. Spec, proposal, and lifecycle links remain intact
- Covers: AC1
- Level: contract
- Fixture/setup: `specs/pr-ci-validation-tiering.md` and change records.
- Steps: Assert the feature spec links to the accepted proposal and proposal-review record, and change metadata points to the spec, test spec, architecture, ADR, plan, and latest review state.
- Expected result: Reviewers can trace the test spec back to approved source artifacts.
- Failure proves: The proof surface is detached from the approved contract.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ValidationCommandDocumentationTests.cs` or a small lifecycle metadata test.

### Scope, Compatibility, And Security Guards

PRCI-T030. Release-evidence tests stay present and selectable
- Covers: R54, EC2
- Level: contract
- Fixture/setup: `VeloFile.Corpus.Tests` category inventory and release-evidence tests.
- Steps: Enumerate Corpus test categories and expected release-evidence tests.
- Expected result: ReleaseEvidence tests remain present, explicit, and non-empty; fast/contract and script-smoke selections are also non-empty.
- Failure proves: Runtime reduction deleted, hid, or starved evidence tiers.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/CategoryInventoryTests.cs`

PRCI-T031. Scope guards prevent production behavior, public option, serialization, and visual/manual gate drift
- Covers: R55, R56, R57, R60, AC15
- Level: contract
- Fixture/setup: changed-file list, public scripts, MSTest settings, workflow files, and category inventory.
- Steps: Verify implementation changes remain in validation infrastructure/docs/tests; public prepared-tool script options were not added; assembly/class serialization settings are not removed; `ci-fast-required` does not hard-gate screenshot, visual, or manual-evidence categories.
- Expected result: The hosted CI tiering change does not alter production behavior or unrelated validation semantics.
- Failure proves: The change silently exceeds the approved validation-policy scope.
- Automation location: review validation plus `tests/VeloFile.Corpus.Tests/TestRuntime/ScopeGuardTests.cs` or existing category/parallelism tests.

PRCI-T032. Caching is secondary, privacy-safe, and never a release-evidence fallback
- Covers: R58, R59, R62, R63, R64, AC16, EC9
- Level: contract
- Fixture/setup: workflow cache steps, permissions blocks, and ordinary PR workflow definitions.
- Steps: Inspect ordinary PR workflows for cache use, token permissions, required secrets, and fallback behavior.
- Expected result: Cache setup is optional; cache misses still run restore/build and do not switch to full release evidence; cache keys and permissions avoid secrets, tokens, signing material, and private profile details.
- Failure proves: Caching became a correctness dependency or privacy risk.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`

PRCI-T033. Existing broad-PR preservation tests migrate to the new policy
- Covers: R61, AC14
- Level: contract
- Fixture/setup: old broad-CI preservation tests and new workflow contract tests.
- Steps: Inspect tests that previously required ordinary PR CI to call `scripts/ci.ps1`.
- Expected result: Tests now preserve `ci-fast-required` for ordinary PRs while separately preserving release-evidence and full closeout availability.
- Failure proves: The test suite is still enforcing the rejected broad-PR policy or no longer protects closeout.
- Automation location: `tests/VeloFile.Corpus.Tests/TestRuntime/CiWorkflowContractTests.cs`, `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`

### Manual And Hosted Evidence

PRCI-M001. Shadow hosted PR cycle is reviewed before branch-protection handoff
- Covers: R49, R50, AC13, E5
- Level: manual
- Fixture/setup: at least one hosted PR cycle after M2, `shadow-run.md`, and GitHub Actions run links or summarized evidence.
- Steps: Record fast-lane runtime, selected categories, failures, broad-check pass/fail when available, and comparison notes.
- Expected result: Maintainers can compare coverage and runtime before making `ci-fast-required` the ordinary required check.
- Failure proves: Required-check transition is unsupported by hosted evidence.
- Automation location: `docs/changes/2026-05-18-pr-ci-validation-tiering/shadow-run.md`

PRCI-M002. Release branch/tag and merge-queue policy evidence is recorded
- Covers: R7, R8, R13, R51, EC5, EC6
- Level: manual
- Fixture/setup: release workflow trigger patterns and change evidence.
- Steps: Record selected release branch/tag patterns and whether merge queue is used as a release-readiness gate.
- Expected result: Trigger policy is reviewable, and future release naming changes are routed through workflow/docs updates before release reliance.
- Failure proves: Release-evidence triggers are ambiguous or stale.
- Automation location: `docs/changes/2026-05-18-pr-ci-validation-tiering/`

PRCI-M003. Release readiness and rollback are exercised or explicitly recorded
- Covers: R52, R53, E6, EC10
- Level: manual
- Fixture/setup: release-evidence workflow, closeout workflow, local `scripts/ci.ps1`, and maintainer release/rollback notes.
- Steps: Before release or milestone closeout, run an accepted release-evidence or closeout path; if rollback is needed, record the broad check restored as required and fast lane optional.
- Expected result: Full evidence remains the release authority and rollback is operationally clear.
- Failure proves: Fast PR success is being used beyond its contract.
- Automation location: release/milestone closeout evidence under `docs/changes/2026-05-18-pr-ci-validation-tiering/`

## Validation Commands

Test-spec and artifact validation:

```powershell
git diff --check -- specs\pr-ci-validation-tiering.test.md docs\plans\2026-05-18-pr-ci-validation-tiering.md docs\plan.md docs\changes\2026-05-18-pr-ci-validation-tiering\change.yaml
Select-String -Path specs\pr-ci-validation-tiering.test.md,docs\plans\2026-05-18-pr-ci-validation-tiering.md,docs\plan.md,docs\changes\2026-05-18-pr-ci-validation-tiering\change.yaml -Pattern '[ \t]+$'
```

Implementation milestone validation commands are owned by the execution plan. The test groups above are expected to run through these focused commands as milestones land:

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiRuntimeSummary"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation"
```

Fast-lane command evidence required by M2:

```powershell
dotnet --info
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug --no-restore
dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --no-build
dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --no-build
dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --no-build
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=CorpusScript&TestCategory=Smoke"
```

Release-evidence and closeout validation:

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=ReleaseEvidence"
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1
```

## Fixtures And Data

- Positive workflow fixtures are the committed files under `.github/workflows/`.
- Negative workflow fixtures may be inline test strings or files under `tests/VeloFile.Corpus.Tests/fixtures/ci-workflows/`.
- Summary helper tests should use temporary directories, temporary `GITHUB_STEP_SUMMARY` paths, synthetic command timing inputs, and sample TRX files under `tests/VeloFile.Corpus.Tests/fixtures/ci-runtime/`.
- Category selection tests use `CorpusCategoryInventory` and the compiled `VeloFile.Corpus.Tests` assembly.
- Release-evidence preservation tests reuse `ReleaseEvidenceTierTests` expected-test inventory and category taxonomy.
- Shadow-run and branch-protection evidence belongs under `docs/changes/2026-05-18-pr-ci-validation-tiering/`.
- Fixture data must not contain real secrets, tokens, signing material, private local profile paths, or unrelated machine inventory.

## Mocking/Stubbing Policy

- Static workflow contract tests may use invalid workflow fixtures to prove diagnostics, but positive tests must inspect the committed workflow files.
- The YAML parser/model is test-owned. A test-only structured YAML dependency such as `YamlDotNet` may be added to `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj` if implementation keeps it test-scoped.
- Summary helper tests may invoke the PowerShell helper locally with fake timing/TRX inputs and temporary environment variables.
- Do not mock `scripts/ci.ps1` for full closeout claims. Static command preservation may be tested automatically, but actual broad closeout execution must be recorded in milestone or final validation when required.
- Do not mock public script smoke coverage for `CorpusScript&Smoke`; use the existing public script smoke tests.
- Do not call GitHub branch-protection APIs from automated tests. Branch protection remains a maintainer-operated manual evidence surface.

## Migration Or Compatibility Tests

- Existing broad-PR preservation tests migrate from "ordinary PR CI calls `scripts/ci.ps1`" to "ordinary PR CI runs `ci-fast-required`, while release-evidence and closeout workflows remain explicit and available".
- `scripts/ci.ps1` compatibility is protected by PRCI-T022 and broad final validation.
- Release evidence compatibility is protected by PRCI-T018, PRCI-T019, PRCI-T030, and manual release closeout evidence.
- Hosted Windows runner and `pwsh` assumptions are protected by PRCI-T006 through PRCI-T008.
- Linux/macOS hosted validation is not part of this migration and should fail the current workflow contract unless a later accepted cross-platform validation design changes the contract.

## Observability Verification

- Summary helper tests prove required lane metadata, tier selection, duration fields, per-project timing, slow-test details, and limitation text.
- Workflow contract tests prove summary helper invocation exists in each hosted lane introduced or changed by the spec.
- Failure diagnostics must name the offending lane, step, runner, shell, SDK-order violation, command filter, or summary field.
- Shadow-run records must distinguish hosted runtime evidence from local runtime evidence and must not present one run as a universal timing guarantee.

## Security/Privacy Verification

- Ordinary PR workflows must not require new repository secrets.
- Pull-request workflow permissions must be scoped to checkout, setup, validation, summary writing, and artifact upload.
- Cache keys, summaries, artifacts, and exception evidence must not include secrets, credentials, signing material, tokens, private profile paths, usernames, or unrelated machine inventory.
- Runner and shell exception evidence may identify approved infrastructure constraints but must avoid private local machine details.

## Performance Checks

Performance proof is comparative and descriptive, not a universal wall-clock gate.

- PRCI-M001 records hosted shadow-run runtime for `ci-fast-required` and, when available, the broad required check on the same PR cycle.
- Runtime summaries report total duration, build duration, per test project duration, and slowest tests when structured output exists.
- A fast lane that runs full release evidence by default fails the contract even if one hosted run happens to be short.
- Cache hits or misses must not be used as the primary proof of faster PR validation.

## Manual QA Checklist

- Confirm at least one shadow PR cycle is recorded before maintainers change ordinary PR branch protection.
- Confirm release branch/tag patterns and merge-queue policy are recorded before considering M3 closed.
- Confirm any branch-protection claim has maintainer evidence.
- Confirm release readiness uses `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or another accepted release gate.
- Confirm rollback notes explain how to make broad closeout required again and leave `ci-fast-required` optional.

## What Not To Test

- Do not add new production App/Core/Windows/Corpus behavior tests for this change; production behavior is out of scope.
- Do not test Linux or macOS hosted validation as accepted behavior.
- Do not require exact SDK version pinning beyond repository-approved SDK setup unless a later artifact chooses a pinning policy.
- Do not treat screenshots, visual evidence, or manual evidence as hard gates in `ci-fast-required`.
- Do not require branch protection to be changed by automated tests.
- Do not use timing-only assertions as hard pass/fail gates across machines or hosted runner classes.
- Do not treat dependency caching as a correctness path.

## Uncovered Gaps

None blocking.

Exact active push branches, release branch/tag glob patterns, nightly cron timing, and merge-queue policy are implementation/change-evidence decisions already allowed by the approved spec and plan. They do not need to return to spec or architecture as long as the workflow and change evidence preserve the required contract.

## Next Artifacts

- Enter `implement` for M1: runtime summary helper and broad-CI reporting foundation.
- Keep later milestones blocked until their prerequisite milestones close under the approved execution plan.

## Follow-on Artifacts

None yet.

## Readiness

Active and ready for `implement` at M1.

Not ready for final verification or PR handoff. Those remain blocked until implementation milestones, code review, review-resolution when triggered, explain-change, verify, and PR handoff are complete.
