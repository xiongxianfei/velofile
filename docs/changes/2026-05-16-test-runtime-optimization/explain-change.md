# Test Runtime Optimization Change Rationale

## Summary

This change makes VeloFile's validation cost explicit. It splits Corpus validation into documented tiers, moves fast contract checks away from public PowerShell wrapper execution where the wrapper is not the claim under test, keeps public script smoke and release evidence available, adds a test-internal prepared corpus tool harness, and records measured runtime evidence.

The implementation intentionally does not change production App/Core/Windows behavior, public corpus script options, hosted CI routing, or release-readiness expectations. Full `scripts\ci.ps1` remains the broad closeout command for the first slice.

Current workflow state: M1-M7 are closed, review-resolution is resolved, and the active plan is ready for final `verify`. This artifact does not claim verification, PR readiness, or release readiness.

## Problem

The learn session and proposal measured `VeloFile.Corpus.Tests` at about `5 m 49 s` for 37 tests. The slow path was dominated by wrapper/script process execution, scratch source copying, and repeated `dotnet publish` work. The problem was not that those checks were unimportant; it was that fast contract assertions, public script smoke checks, and release-evidence validation shared the same broad execution path.

That made small local edits pay release-evidence and wrapper-publish costs. The approved direction was to preserve coverage while giving contributors and reviewers clear validation tiers.

## Decision Trail

| Source | Decision | Implementation consequence |
|---|---|---|
| Proposal | Adopt validation tiering and corpus harness optimization. | Added explicit categories and local validation commands instead of deleting slow tests. |
| Proposal | Keep `scripts\ci.ps1` unchanged for the first slice. | Full CI remains broad and unfiltered; no hosted CI split was implemented. |
| Proposal | Keep prepared-tool execution internal to tests first. | Added a test harness, not public `-PreparedToolPath` or `-UseExistingToolBuild` options. |
| Proposal | Preserve one hermetic wrapper path and minimal smoke per public script family. | M3 kept public wrapper confidence through `CorpusScript` + `Smoke` tests. |
| Spec R1-R7 | Scope is validation-tiering only; no production behavior change; no public prepared-tool options. | Changes stay in docs, test projects, corpus test seam, and test harness helpers. |
| Spec R8-R15 | Corpus tests must use the accepted taxonomy and reject invalid category combinations. | Added `CorpusTestCategories` and inventory validation. |
| Spec R16-R21 | Contributor commands must be documented and `--no-build` assumptions stated. | Updated `README.md` and validation command tests. |
| Spec R22-R33 | Contract tests may run in process, but public wrapper smoke must remain. | Added in-process Corpus contract tests and minimal public wrapper smoke. |
| Spec R34-R38 | Prepared-tool execution is test-internal, scratch-owned, manifest-validated, and pre-invocation rejecting. | Added the prepared corpus tool harness and rejection tests. |
| Spec R39-R43 | Release, benchmark, visual, and manual evidence remain explicit. | Added release-evidence tier tests and rationale checks for evidence categories in fast/default filters. |
| Spec R44-R47 | Do not remove assembly-wide serialization in this slice; record shared-state constraints. | Added shared-state inventory and parallelism boundary tests; left parallelization as follow-up. |
| Spec R48-R60 | Runtime reports must record baseline, optimized timings, slow tests, full CI status, and missed targets honestly. | Added `m6-optimized-runtime.md` and report contract tests. |
| ADR 0011 | Validation tiers are a test-harness architecture boundary, not a release-evidence shortcut. | Kept full closeout and release-evidence paths intact while adding faster focused paths. |
| Plan M1-M7 | Implement categories, contract split, wrapper smoke, prepared-tool harness, release-evidence preservation, runtime evidence, and lifecycle closeout in slices. | Each milestone closed through code review before final closeout. |

## Diff Rationale By Area

| Area / file | Change | Reason | Source artifact | Test/evidence |
|---|---|---|---|---|
| `docs/proposals/2026-05-16-test-runtime-optimization.md` | Added the decision-oriented proposal and proposal-level decisions. | Established the problem, non-goals, tiering direction, and deferred CI split/public prepared-tool/parallelization decisions. | Proposal review | Review log, plan decision log |
| `specs/test-runtime-optimization.md` | Added the approved contract, requirements, examples, and acceptance criteria. | Made validation tiers, wrapper smoke, prepared-tool boundaries, release-evidence preservation, and runtime reports testable. | Spec review | `spec-review-r2` |
| `specs/test-runtime-optimization.test.md` | Added traceable tests TTO-T001 through TTO-T040. | Mapped each spec requirement to concrete automation or review evidence. | Test spec review | `test-spec-review-r1` |
| `docs/architecture/system/architecture.md`, `docs/architecture/system/diagrams/container.mmd`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md` | Added the validation-tier architecture boundary and ADR. | Kept the optimization as test-harness architecture, separate from product behavior and CI job splitting. | Architecture review | `architecture-review-r2` |
| `README.md` | Added focused local validation commands. | Gives contributors the fast/default, Corpus contract, script-smoke, release-evidence, and full closeout commands. | Spec R16-R21 | `ValidationCommandDocumentationTests` |
| `docs/plans/2026-05-16-test-runtime-optimization.md`, `docs/plan.md` | Added and maintained the living plan, milestone states, validation notes, decisions, discoveries, and handoff summary. | Keeps the multi-slice workflow reviewable and prevents hidden coverage gaps. | Plan review | `plan-review-r2`, code reviews R1-R10 |
| `docs/changes/2026-05-16-test-runtime-optimization/*` | Added change metadata, review records, review-resolution, runtime reports, shared-state inventory, and this explanation. | Preserves durable evidence for decisions, findings, fixes, validation, and handoff state. | Workflow policy | Review log and `review-resolution.md` |
| `tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj` | Retargeted to the Corpus tool's Windows TFM and references the Corpus tool. | Allows in-process contract tests to call the tool without duplicating logic. | Spec R22-R26 | `CorpusContractTests` |
| `tools/VeloFile.Corpus/Program.cs` | Added a narrow test seam for the Corpus CLI. | Lets tests exercise Corpus command behavior in process while keeping public wrappers for public-entrypoint claims. | Spec R22-R24 | `CorpusContractTests` |
| `tests/VeloFile.Corpus.Tests/TestRuntime/CorpusTestCategories.cs` | Added accepted category constants, rationale attributes, reflection inventory, and validation diagnostics. | Enforces the category taxonomy and invalid category combinations. | Spec R8-R15, R42 | `CategoryInventoryTests`, `ReleaseEvidenceTierTests` |
| `CategoryInventoryTests.cs`, `ValidationCommandDocumentationTests.cs`, `ParallelismBoundaryTests.cs` | Added category, command-documentation, and shared-state inventory tests. | Proves category taxonomy, command filters, `--no-build` assumptions, and deferred parallelization boundaries. | TTO-T001-T007, TTO-T031 | M1 validation |
| `CorpusToolHarness.cs`, `CorpusContractTests.cs`, `ScratchRootBoundaryTests.cs`, `WrapperCoverageLedgerTests.cs` | Added in-process Corpus contract tests and coverage ledger checks. | Moves schema/report/manifest/scope/redaction/release-classification claims into a faster contract tier without removing wrapper coverage. | TTO-T008-T010, TTO-T039 | M2 validation |
| `PublicCorpusScriptHarness.cs`, `CorpusScriptSmokeTests.cs`, existing `CorpusToolingSmokeTests.cs` updates | Added minimal public wrapper smoke and hermetic wrapper isolation. | Preserves public script confidence and scratch publish isolation after contract tests move in process. | TTO-T011-T017, TTO-T038 | M3 validation |
| `PreparedCorpusToolHarness.cs`, `PreparedToolHarnessTests.cs`, `RepoOutputSnapshot.cs`, `ScratchWorkspace.cs`, `TestRepo.cs` | Added test-internal prepared tool setup, manifest validation, repo-output snapshot checks, and scratch-owned source/publish roots. | Avoids repeated publish cost for process-based tests while proving prepared tools cannot escape scratch boundaries or mutate repo `bin`/`obj`. | TTO-T019-T027 | M4 validation and TRO-CR2 resolution |
| `ReleaseEvidenceTierTests.cs` and category updates in UI/visual Corpus tests | Added explicit release-evidence, benchmark, visual, and manual-evidence tier checks. | Keeps expensive evidence runnable and prevents evidence-only checks from drifting into fast/default filters without rationale. | TTO-T018, TTO-T028-T030, TTO-T036, TTO-T040 | M5 validation and TRO-CR3 resolution |
| `RuntimeReportTests.cs`, `runtime/m1-baseline.md`, `runtime/m6-optimized-runtime.md` | Added baseline and optimized runtime evidence plus tests for report shape, privacy, slow-test evidence, and missed target disclosure. | Makes runtime claims reviewable and prevents timing targets from being used to delete coverage. | TTO-T032-T035 | M6 validation |

## Tests Added Or Changed

| Test area | What it proves | Test IDs |
|---|---|---|
| Category inventory | Corpus tests use only accepted categories; missing/unknown categories fail; `ReleaseEvidence` + `Fast` needs non-empty rationale; `CorpusScript` needs `Smoke` or `ReleaseEvidence`. | TTO-T001 through TTO-T005 |
| Validation command documentation | Local commands and `--no-build` assumptions are documented, and fast/default filters exclude expensive-only tiers. | TTO-T006, TTO-T007 |
| In-process Corpus contracts | Schema/report/manifest/profile/scope/redaction/release-classification claims can run without public wrapper execution when wrapper behavior is not the claim. | TTO-T008 |
| Scratch and coverage guards | In-process/prepared-tool outputs stay under scratch roots, generated artifacts do not escape, and wrapper migration remains lossless. | TTO-T009, TTO-T010, TTO-T039 |
| Public wrapper smoke | Public scripts still route and produce representative output; hermetic wrapper isolation still proves scratch source copy/publish behavior. | TTO-T011 through TTO-T017, TTO-T038 |
| Prepared-tool harness | Current-run prepared tools execute; missing/outside/stale/wrong/missing-artifact prepared tools fail before invocation; no public prepared-tool options exist; global/repo outputs are not mutated. | TTO-T019 through TTO-T027 |
| Release/evidence tiers | ReleaseEvidence command remains runnable; benchmark, visual, and manual evidence categories are explicit and guarded. | TTO-T018, TTO-T028 through TTO-T030, TTO-T036, TTO-T040 |
| Parallelism boundary | Shared-state inventory is recorded and assembly-wide serialization is not removed in this slice. | TTO-T031 |
| Runtime reporting | Runtime evidence records baseline, optimized timings, full CI status, top 10 slow tests, metadata/privacy controls, and missed targets without deleting coverage. | TTO-T032 through TTO-T035 |
| Scope guard | The first slice stays out of production behavior and public prepared-tool APIs. | TTO-T037 |

The test level is intentionally mixed: static contract tests for taxonomy and documentation, in-process tests for Corpus output contracts, public wrapper smoke for script entrypoints, prepared-tool process tests for repeated CLI execution, and runtime-report tests for evidence shape. That matches the proposal's separation of validation cost by claim.

## Validation Evidence Available Before Final Verify

The active plan and `change.yaml` record milestone validation. Key closeout evidence includes:

```text
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"
dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=m6-corpus.trx"
git diff --check
```

M7 recorded full `scripts\ci.ps1` passing with build 0 warnings/0 errors, UI contract validation passing, and Core 168/App 149/Windows 52/Corpus 90 tests passing. Code-review-r10 reran the fast/contract no-build solution filter and `git diff --check`; both passed.

This evidence is pre-final-verify evidence. The `verify` stage still owns the final verification claim.

## Review Resolution Summary

Durable review records are indexed in `docs/changes/2026-05-16-test-runtime-optimization/review-log.md`. Material findings are closed in `docs/changes/2026-05-16-test-runtime-optimization/review-resolution.md`.

Closed material findings:

- TRO-SR1: defined first-slice prepared-tool staleness through a current-run manifest.
- TRO-AR1: resolved architecture approval sequencing after spec re-review.
- TRO-PL1: added build-producing validation before timing-focused `--no-build` evidence and fixed ambiguous filters.
- TRO-PL2: added a public-wrapper coverage guard and migration ledger.
- TRO-CR1: required non-empty `ReleaseEvidenceFastRationale` for `ReleaseEvidence` + `Fast`.
- TRO-CR2: changed prepared-tool setup to publish from scratch-owned source and expanded repo-output mutation proof.
- TRO-CR3: added direct `ManualEvidence` + fast/default rationale regression proof.

Clean implementation reviews:

- `code-review-r2` closed M1.
- `code-review-r3` closed M2.
- `code-review-r4` closed M3.
- `code-review-r6` closed M4.
- `code-review-r8` closed M5.
- `code-review-r9` closed M6.
- `code-review-r10` closed M7.

No review-resolution findings are open.

## Alternatives Rejected

- Keep the suite unchanged: rejected because it preserved a slow default path that encourages skipped validation.
- Remove or skip slow Corpus tests: rejected because it weakens release evidence instead of optimizing the harness.
- Rewrite all Corpus tooling as a library first: rejected as larger than needed for the first slice.
- Split hosted CI immediately: rejected until category and runtime evidence stabilize.
- Expose `-PreparedToolPath` or `-UseExistingToolBuild`: rejected because public wrapper contracts should not change in the first slice.
- Remove assembly-wide `DoNotParallelize`: deferred because parallel safety needs a separate measured slice.
- Source-hash cross-run prepared-tool caching: deferred because this slice only needs same-run prepared-tool reuse.
- Delete wrapper/prepared-tool/release-evidence tests to hit timing targets: rejected by R60 and recorded as out of scope.

## Scope Control

Preserved non-goals:

- No production App/Core/Windows behavior change.
- No hosted CI split.
- No public corpus wrapper option changes.
- No removal of corpus, compatibility, preview, diagnostics, benchmark, visual, manual, or release-evidence validation.
- No assembly-wide parallelization change.
- No broad rewrite of Corpus tooling.
- No universal performance guarantee from one machine's timing data.

The only tool code change is the narrow Corpus CLI test seam needed for in-process contract tests. The prepared-tool path remains test-internal.

## Risks And Follow-Ups

- The fast/contract no-build run remains above the aspirational 30-second Corpus target on this machine. The M6 report records the miss instead of weakening coverage.
- Full local CI remained slower than the original Corpus-only baseline and is not directly comparable. This is recorded as evidence, not hidden.
- Assembly-wide `DoNotParallelize` removal is still deferred; future work needs parallel-safety proof and narrower serialization.
- Hosted CI splitting is still deferred; future work should use the stabilized category data before changing CI gates.
- Public prepared-tool options and cross-run prepared-tool caching are still deferred and would require a new accepted scope.

## Readiness

This explain-change artifact is current with M7 code-review-r10. The change is ready for the `verify` stage after this artifact is committed. It is not PR-ready until `verify` and PR handoff complete.
