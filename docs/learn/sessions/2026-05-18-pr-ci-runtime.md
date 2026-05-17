# Learn Session: PR CI Runtime

## Frame

- Trigger: maintainer asked why PR CI took about 16 minutes and what best practices should guide optimization.
- Trigger type: explicit maintainer request after PR #3 hosted CI completed.
- Scope: hosted GitHub Actions runtime for PR #3, the repository CI workflow, `scripts/ci.ps1`, Corpus runtime optimization evidence, and prior learn records.
- Evidence in scope:
  - PR #3 check status from `gh pr view 3 --json statusCheckRollup`.
  - Run `26002313667` from `gh run view 26002313667 --json jobs` and selected log lines from `gh run view 26002313667 --log`.
  - `.github/workflows/ci.yml`.
  - `scripts/ci.ps1`.
  - `docs/changes/2026-05-16-test-runtime-optimization/runtime/m1-baseline.md`.
  - `docs/changes/2026-05-16-test-runtime-optimization/runtime/m6-optimized-runtime.md`.
  - `docs/learn/sessions/2026-05-16-test-runtime-optimization.md`.
  - `tests/VeloFile.Corpus.Tests/MSTestSettings.cs`.
  - `tests/VeloFile.Corpus.Tests/CorpusToolingSmokeTests.cs`.
  - `tests/VeloFile.Corpus.Tests/TestRuntime/ReleaseEvidenceTierTests.cs`.
- Explicit exclusions:
  - No CI workflow changes in this session.
  - No test retagging or implementation change.
  - No topic-file policy update without maintainer confirmation.
- Prior learnings reviewed:
  - `docs/learn/sessions/2026-05-16-test-runtime-optimization.md` already captured the local Corpus slow path: PowerShell wrapper execution, scratch source copying, repeated `dotnet publish`, release-evidence checks mixed into broad validation, and assembly-wide serialization.
- Session record path: `docs/learn/sessions/2026-05-18-pr-ci-runtime.md`

## Observations

### O1. PR CI runtime is dominated by unfiltered Corpus tests

Evidence:

- PR #3 check `ci` started at `2026-05-17T20:48:33Z` and completed at `2026-05-17T21:04:36Z`, about `16 m 03 s` wall-clock.
- The `Run repository CI script` step started at `20:49:34Z` and completed at `21:04:32Z`, about `14 m 58 s`.
- `scripts/ci.ps1` runs broad closeout validation:
  - `dotnet --info`
  - `dotnet restore VeloFile.sln`
  - `dotnet build VeloFile.sln -c Debug --no-restore`
  - `dotnet run --project tools/VeloFile.UiContracts ...`
  - `dotnet test VeloFile.sln -c Debug --no-build`
- Hosted CI test output:
  - `VeloFile.Windows.Tests`: 52 tests, `3 s`.
  - `VeloFile.Core.Tests`: 168 tests, `1 s`.
  - `VeloFile.App.Tests`: 168 tests, `5 s`.
  - `VeloFile.Corpus.Tests`: 90 tests, `13 m 22 s`.

Interpretation:

The root cause is not the whole solution test suite evenly taking 16 minutes. The Corpus assembly dominates the PR runtime. The other three test projects complete quickly once the build is available.

### O2. The first runtime optimization slice improved local tiering, but PR CI intentionally still uses the broad closeout command

Evidence:

- `.github/workflows/ci.yml` has one Windows job and one validation command: `./scripts/ci.ps1`.
- `ReleaseEvidenceTierTests.Broad_closeout_ci_remains_unsplit_and_unfiltered` explicitly asserts that `scripts/ci.ps1` contains `dotnet test VeloFile.sln -c Debug --no-build` and does not include `--filter` or `TestCategory=`.
- The active test runtime plan says the first implementation slice keeps `scripts/ci.ps1` as the broad closeout command and defers CI job splitting.
- M6 runtime evidence records the faster focused tiers:
  - `TestCategory=Fast|TestCategory=Contract`: about `54 s` locally for 71 Corpus tests.
  - `TestCategory=CorpusScript&TestCategory=Smoke`: about `51 s` locally.
  - `TestCategory=ReleaseEvidence`: about `5 m 03 s` locally.
- M6 also records that full CI remains broad and that full-Corpus runtime is slower because additional category, prepared-tool, smoke, and runtime-report coverage is now present.

Interpretation:

The optimization work created the tiering needed for faster feedback, but it deliberately did not apply that tiering to hosted PR CI. Therefore PR CI still pays the full release-evidence and CorpusScript cost.

### O3. Hosted CI has cold-run overhead, but it is secondary to Corpus runtime

Evidence:

- `Set up .NET SDK` took about `50 s`.
- Restore began around `20:49:37Z`; the app project restore completed at `20:50:25Z`, about `48 s`.
- Build reported `Time Elapsed 00:00:30.72`.
- UI contract validation completed in roughly `7 s`.
- Corpus tests alone reported `13 m 22 s`.

Interpretation:

NuGet restore, SDK setup, and build matter, but optimizing only those cannot solve a 16-minute PR. The highest-leverage path is reducing what the PR-required test stage selects from the Corpus assembly or splitting that work across jobs.

### O4. The old local bottleneck remains structurally relevant in hosted CI

Evidence:

- `tests/VeloFile.Corpus.Tests/MSTestSettings.cs` still applies `[assembly: DoNotParallelize]`.
- `CorpusToolingSmokeTests` contains release-evidence tests that call public PowerShell scripts repeatedly.
- `ReleaseEvidenceTierTests` intentionally marks the full matrix script tests as `ReleaseEvidence` and not `Smoke`.
- The prior learn session and M6 runtime evidence identify repeated script/process execution and release-evidence wrapper paths as the slow path.

Interpretation:

Hosted CI is not showing a new mysterious bottleneck. It is the same Corpus release-evidence path, now visible in the required PR check after the branch added more coverage.

## Root Cause

The PR took about 16 minutes because the single required GitHub Actions job runs the broad, unfiltered `scripts/ci.ps1` path. That path runs every solution test, including the expensive `VeloFile.Corpus.Tests` release-evidence and script-wrapper tests. The Corpus assembly reported `13 m 22 s`, while Core, App, and Windows tests together completed in seconds.

The deeper design reason is that CI still uses the "full closeout" command for every PR. The test-runtime optimization work added useful categories and faster local commands, but CI splitting was explicitly deferred, and a regression test currently preserves the broad unfiltered CI behavior.

## Best-Practice Guidance

- Keep full release-evidence validation, but stop making every PR wait for it as the only required check.
- Use a two-tier hosted model:
  - PR required: build, UI contracts, Core/App/Windows tests, Corpus `Fast|Contract`, and minimal `CorpusScript&Smoke`.
  - Nightly, release, merge queue, or manual gate: full unfiltered `scripts/ci.ps1`, `ReleaseEvidence`, benchmark, compatibility, preview, diagnostics, and packaging checks.
- Keep one hermetic public-wrapper smoke path in the PR tier so wrapper breakage is still caught early.
- Keep full matrix release-evidence tests out of the fast PR gate unless the touched files require them.
- Split CI jobs by cost and ownership: build/cache, fast tests, Corpus contract, Corpus script smoke, release evidence. This exposes the slow lane and lets independent lanes run in parallel.
- Add CI runtime reporting: publish per-step durations and top slow tests from TRX as artifacts or job summaries.
- Add NuGet caching and consider a `global.json` SDK pin to reduce setup variability, but treat this as secondary. The main cost is Corpus test selection.
- Remove assembly-wide `DoNotParallelize` only in a separate measured slice after shared-state constraints are isolated.
- Optimize repeated wrapper tests by reusing the prepared tool where the public wrapper contract is not under test, while retaining a small hermetic wrapper publish test.

## Classification

Contributor confirmation for routing is not yet available, so these classifications are candidates only.

| Observation | Candidate classification | Secondary routes | Rationale |
|---|---|---|---|
| O1 | observation | none | Direct runtime breakdown explains the 16-minute PR. |
| O2 | process-follow-up | CI proposal or second-slice test-runtime plan | The project already has categories; the next optimization is deciding how hosted PR CI should use them. |
| O3 | observation | none | Hosted setup/restore/build are measurable but not the main root cause. |
| O4 | durable-lesson candidate | future topic entry only if maintainer confirms | This repeats the prior local slow-path finding with hosted CI evidence, but routing requires confirmation. |

## Recommended Follow-Ups

1. Open a second-slice CI/test-runtime proposal for hosted validation tiers.
2. Amend the current broad-CI preservation test only after the proposal/spec decide the new CI contract.
3. Add a GitHub Actions fast PR job using existing category filters.
4. Keep a separate full closeout/release-evidence job on schedule, manual dispatch, release branches, or merge queue.
5. Add TRX slow-test extraction to CI artifacts so future runtime regressions are visible by test name.

## Route

- No topic file was updated.
- No CI workflow or plan was changed.
- This session records evidence and candidate routing only. A maintainer decision is needed before turning the hosted-CI tiering recommendation into an authoritative proposal/spec/plan change.

## Validation

- `gh pr view 3 --json number,url,title,state,statusCheckRollup,headRefName,baseRefName`
- `gh run list --branch ui-design-system-shell-redesign --workflow ci --limit 5 --json databaseId,displayTitle,status,conclusion,createdAt,updatedAt,event,headSha,url`
- `gh run view 26002313667 --json jobs`
- `gh run view 26002313667 --log`
- `git diff --check -- docs/learn/sessions/2026-05-18-pr-ci-runtime.md`
