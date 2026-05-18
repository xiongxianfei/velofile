# PR CI Validation Tiering Proposal

## Status

accepted

## Problem

VeloFile's required PR CI currently uses the broad closeout validation path for every pull request. That gives strong confidence, but it makes routine PR feedback much slower than necessary.

The hosted PR #3 run took about 16 minutes wall-clock. The repository CI script step took about 14 minutes and 58 seconds, and `VeloFile.Corpus.Tests` alone took about 13 minutes and 22 seconds. By contrast, the Windows, Core, and App test projects completed in seconds once build output was available.

The project already has the validation-tiering foundation from the test-runtime optimization work: fast/contract checks, script smoke checks, and release-evidence checks are distinguishable. However, hosted PR CI still invokes the unfiltered `scripts/ci.ps1` closeout path, so every PR pays the full corpus release-evidence cost.

The problem is not that release evidence is unimportant. The problem is that the required PR gate currently uses the most expensive validation tier as the default feedback loop.

## Goals

- Reduce required PR CI runtime while preserving full release-evidence validation.
- Make fast PR validation the required default for ordinary pull requests.
- Keep full `scripts/ci.ps1` available for milestone closeout, manual validation, nightly validation, release branches, and merge/release gates.
- Preserve public wrapper smoke coverage in the fast PR path.
- Keep release-evidence, benchmark, compatibility, preview, diagnostics, visual, and manual-evidence checks explicit and runnable.
- Add runtime reporting so future slowdowns are visible by job, test project, and slowest tests.
- Avoid weakening test semantics or production behavior.

## Non-goals

- Do not delete or skip release-evidence tests from the repository.
- Do not remove the full closeout validation command.
- Do not change production App, Core, Windows, or Corpus behavior.
- Do not expose new public prepared-tool script options in this proposal.
- Do not remove assembly-wide or class-level serialization in this proposal.
- Do not make NuGet caching the primary optimization strategy.
- Do not hard-gate screenshot or visual evidence as part of the fast PR job.

## Vision fit

fits the current vision

Fast, clear validation supports VeloFile's maintainability and contributor workflow only if confidence remains explicit. This proposal keeps release evidence available while making the common PR feedback loop faster and more actionable.

## Initial intent preservation

| Initial user goal | Proposal treatment | Where recorded |
|---|---|---|
| Reduce hosted required PR CI runtime. | in scope | Problem, Goals, Recommended direction |
| Preserve full release-evidence validation. | in scope | Goals, Non-goals, Recommended direction |
| Make fast PR validation the ordinary required default. | in scope | Goals, Recommended direction, Expected behavior changes |
| Keep `scripts/ci.ps1` available as the broad closeout command. | in scope | Goals, Non-goals, Recommended direction |
| Preserve public wrapper smoke coverage in the fast PR path. | in scope | Goals, Proposed fast PR command set, Testing and verification strategy |
| Keep expensive evidence categories explicit and runnable. | in scope | Goals, Non-goals, Recommended direction |
| Add runtime reporting for jobs, test projects, and slow tests. | in scope | Goals, Runtime reporting, Testing and verification strategy |
| Avoid weakening production behavior or test semantics. | in scope | Non-goals, Architecture impact, Risks and mitigations |
| Avoid new public prepared-tool script options. | out of scope | Non-goals |
| Avoid changing serialization policy in this proposal. | out of scope | Non-goals |
| Treat caching as secondary, not the primary fix. | in scope | Non-goals, Caching policy |
| Decide exact release-evidence triggers. | in scope | Recommended direction, Rollout and rollback |
| Decide whether to stage the fast job as non-required first. | in scope | Rollout and rollback, Decision log |
| Decide workflow structure and stable check names. | in scope | Recommended direction, Architecture impact, Testing and verification strategy |

## Scope budget

| Work item | Treatment | Reason |
|---|---|---|
| Required fast PR validation lane | core to this proposal | This is the main policy decision for reducing ordinary PR feedback time. |
| Full release-evidence and closeout lanes | core to this proposal | The proposal depends on keeping release confidence explicit and available. |
| Release-evidence trigger policy | core to this proposal | Reviewers need to know where full evidence is enforced, not only that it is available. |
| Separate workflow structure and stable check names | core to this proposal | Branch protection and workflow contract tests depend on predictable names. |
| Public wrapper smoke in the fast lane | same-slice dependency | The fast lane should not remove early confidence in public script entrypoints. |
| Workflow contract tests for the new CI policy | same-slice dependency | Existing broad-CI preservation tests need to protect the accepted policy instead of the old one. |
| Runtime reporting and TRX summary output | same-slice dependency | Reporting is required in the first implementation slice so slowdowns and tier selection remain visible. |
| Branch protection or repository required-check settings | separate implementation slice | The repository file change can define jobs, but hosted required-check settings may need a maintainer action. |
| Scheduled, manual, release-branch, tag, or merge-queue release-evidence triggers | same-slice dependency | The trigger policy is part of the accepted CI contract, even if branch protection changes require maintainer action. |
| Dependency caching | deferable follow-up | Caching may help, but the measured bottleneck is Corpus runtime, not restore/build. |
| Screenshot or visual evidence as a fast PR hard gate | out of scope | The proposal keeps visual evidence explicit but not part of the default fast PR gate. |
| New public prepared-tool script options | out of scope | This proposal intentionally avoids expanding public script contracts. |

## Context

The current GitHub Actions workflow has one Windows CI job that runs `./scripts/ci.ps1`. That script restores, builds, validates UI contracts, and runs unfiltered solution tests. This is appropriate as a closeout command, but too expensive as the only required PR gate.

The accepted test-runtime optimization proposal created the category and tiering foundation for this change while intentionally deferring hosted CI splitting. That earlier proposal kept `scripts/ci.ps1` broad for the first slice so the category model and runtime evidence could stabilize before changing hosted validation policy.

Recent runtime evidence shows the hosted PR bottleneck is dominated by the Corpus test assembly rather than by all tests evenly. The run showed:

- PR check duration: about 16 minutes.
- Repository CI script step: about 14 minutes and 58 seconds.
- `VeloFile.Corpus.Tests`: about 13 minutes and 22 seconds.
- `VeloFile.Windows.Tests`: about 3 seconds.
- `VeloFile.Core.Tests`: about 1 second.
- `VeloFile.App.Tests`: about 5 seconds.

The first test-runtime optimization slice created useful categories and faster local tiers, but intentionally kept the broad CI closeout command unchanged. Local evidence already showed faster focused tiers: `Fast|Contract` and `CorpusScript&Smoke` were much shorter than full release-evidence runs, while `ReleaseEvidence` remained slower by design.

## Options considered

### Option A: Keep the required PR CI unchanged

This preserves maximum confidence in every PR, but it keeps routine PR feedback around the full release-evidence cost. It also makes contributors wait for expensive corpus validation even when they touch unrelated code.

### Option B: Remove expensive Corpus tests from CI

This would reduce runtime quickly, but it would weaken release confidence and risk hiding compatibility, diagnostics, preview, benchmark, or wrapper regressions.

### Option C: Add a required fast PR job and move full evidence to a separate gate

This keeps release evidence intact while making required PR feedback faster. The fast PR job runs build, UI contract validation, Core/App/Windows tests, Corpus fast/contract tests, and minimal Corpus script smoke. Full release-evidence validation remains available through manual, scheduled, release, merge-queue, or milestone closeout paths.

### Option D: Optimize only setup/restore/build caching

Caching is useful, but it cannot solve the main issue. The hosted run shows setup/restore/build are secondary to the Corpus test runtime. Caching should be added, but it is not the core solution.

## Recommended direction

Choose Option C: introduce hosted CI validation tiers.

The required PR check should become a fast confidence gate. It should run build, static contract validation, normal App/Core/Windows tests, Corpus fast/contract tests, and minimal public-script smoke. The full closeout path should remain available but should no longer be the only required PR check for every pull request.

The fast PR lane must run normal Core, App, and Windows test projects directly, without relying on category filters to select them. Corpus tests should be filtered by accepted categories. This prevents uncategorized product tests from being silently skipped while the job still passes. Solution-level filtering can be reconsidered later only after a category inventory proves all relevant Core/App/Windows tests are categorized.

Recommended hosted CI lanes:

| Lane | Trigger | Required for normal PR? | Purpose |
|---|---|---:|---|
| `ci-fast-required` | pull request, push to active development branches if desired | yes | Fast confidence for ordinary PRs. |
| `ci-release-evidence` | workflow dispatch, nightly schedule, release branches, release or release-candidate tags, merge queue when used as a release-readiness gate, or maintainer-triggered PR run | no by default | Full release-evidence and expensive corpus coverage. |
| `ci-full-closeout` | workflow dispatch or milestone/release closeout | no by default | Preserve `scripts/ci.ps1` broad validation. |

The fast PR lane should not claim release readiness. It should be labeled as fast PR confidence. Full release evidence remains the authority for release/milestone closure.

Use separate workflows with stable check names:

```text
.github/workflows/ci.yml
  ci-fast-required

.github/workflows/release-evidence.yml
  ci-release-evidence

.github/workflows/closeout.yml
  ci-full-closeout
```

Full release-evidence validation runs on manual dispatch, nightly schedule, release branches or release-candidate tags, and merge queue when the repository uses merge queue as a release-readiness gate. It is not required by default for ordinary pull requests. Milestone and release closeout still require the full closeout command or the accepted release-evidence workflow.

## Expected behavior changes

- Normal PRs receive a faster required CI result.
- Full release-evidence validation remains available and documented.
- `scripts/ci.ps1` remains the broad closeout command.
- The existing broad-CI preservation tests are updated only after the new CI contract is accepted.
- PR CI summaries show whether release evidence ran or only the fast tier ran.
- Corpus release-evidence failures are still visible in scheduled/manual/release validation.
- Contributors can understand which validation tier failed and why.
- The fast job summary explicitly reports `ReleaseEvidence: not run in this lane`, `CorpusScript Smoke: run`, and `Full closeout: not run`.

## Proposed fast PR command set

The fast PR job should run build-producing validation before filtered tests use `--no-build`.

Recommended command shape:

```powershell
dotnet --info
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug --no-restore

dotnet run --project tools\VeloFile.UiContracts -- validate-tokens `
  --contract docs\ui\tokens.v1.json `
  --xaml-root src\VeloFile.App\Resources `
  --scopes docs\ui\ui-contract-scopes.v1.json `
  --scope-root .

dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --no-build
dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --no-build
dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --no-build

dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build `
  --filter "TestCategory=Fast|TestCategory=Contract"

dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build `
  --filter "TestCategory=CorpusScript&TestCategory=Smoke"
```

The exact Corpus filters may be refined in the spec/test spec, but the policy should remain: fast PR CI includes unfiltered Core/App/Windows test projects, Corpus fast/contract tests, and representative public-wrapper smoke, not the full release-evidence matrix.

## Architecture impact

Expected changes are limited to validation infrastructure and tests:

- `.github/workflows/ci.yml`
- `.github/workflows/release-evidence.yml`
- `.github/workflows/closeout.yml`
- `scripts/ci.ps1` only if the accepted spec adds reporting hooks; the broad closeout role should remain unchanged
- test-runtime category contract tests
- release-evidence preservation tests
- CI runtime reporting helper or TRX summary parser
- validation documentation in plans, change records, and contributor guidance

No production App/Core/Windows behavior should change.

## Testing and verification strategy

Use test-first workflow contract changes.

Required proof points:

- The required PR workflow contains a fast validation lane.
- The fast lane runs build-producing restore/build before `--no-build` filtered tests.
- The fast lane includes UI contract validation.
- The fast lane runs Core/App/Windows test projects directly without category filtering.
- The fast lane includes minimal `CorpusScript` + `Smoke` public-wrapper coverage.
- The fast lane runs Corpus `Fast|Contract` tests.
- The fast lane does not include the full `ReleaseEvidence` matrix by default.
- A release-evidence workflow exists and can run `ReleaseEvidence`.
- A full closeout workflow exists and can run `scripts/ci.ps1`.
- Existing broad `scripts/ci.ps1` still exists and remains runnable.
- Release-evidence tests are not deleted or hidden.
- CI summary output records job duration, build duration, per test project duration, category selection, and slow test details where available.

The existing broad-CI preservation test should be revised after the spec is accepted. It should preserve the new policy instead of requiring every PR to run unfiltered closeout validation.

Workflow contract tests must prove that `ci-fast-required` exists, runs restore/build before `--no-build` tests, runs UI contract validation, runs Core/App/Windows tests, runs Corpus `Fast|Contract`, runs `CorpusScript&Smoke`, and does not run `ReleaseEvidence` by default. Separate workflow contract tests must prove that `ci-release-evidence` and `ci-full-closeout` remain available.

## Runtime reporting

Runtime reporting is required in the first implementation slice. Each hosted CI lane should write a GitHub job summary showing selected categories, whether release evidence ran, per test project duration, and slowest test details when TRX output is available.

Add a CI summary step that records:

- total job duration;
- build duration;
- per test project duration where available;
- top slow tests from TRX output;
- which categories were selected;
- whether `ReleaseEvidence` ran;
- whether `CorpusScript&Smoke` ran;
- whether full `scripts/ci.ps1` ran.

The report should be written to the GitHub Actions job summary and optionally uploaded as an artifact when TRX files are present.

## Caching policy

Add dependency caching as a secondary optimization if the repository has or adopts package lock files. Caching should not be treated as the main speed fix because the measured bottleneck is Corpus runtime, not restore/build alone.

Dependency caching may be added after the fast PR lane is introduced, but it is not the primary speed fix. Cache setup must not replace the fast/release tier split, and cache misses must not make the required PR job depend on full release-evidence execution.

Recommended direction:

- use `actions/setup-dotnet` cache support when lock files are available;
- otherwise use a carefully scoped NuGet package cache;
- do not rely on caching to make release-evidence tests fast enough for the required PR gate.

## Rollout and rollback

Roll out in phases:

1. Add runtime summary/reporting while keeping current required CI unchanged.
2. Add `ci-fast-required` as a shadow-running non-required workflow/job while keeping the current broad CI check required for at least one PR cycle, preferably one or two PR cycles.
3. Compare failures and runtime for several PRs.
4. Make `ci-fast-required` the required PR check after review.
5. Move full closeout/release-evidence validation to manual, scheduled, release-branch, merge-queue, or milestone closeout triggers.
6. Update workflow contract tests and contributor guidance.

Rollback is straightforward: make the broad `scripts/ci.ps1` job required again and keep the fast job as optional diagnostics.

After comparison, `ci-fast-required` becomes the required ordinary PR check, while full release-evidence and closeout workflows remain available for manual, scheduled, release, merge-queue, and milestone gates.

## Risks and mitigations

- Risk: fast PR CI misses a release-evidence regression. Mitigation: keep full evidence on scheduled/manual/release/merge-queue gates and require it for milestone closeout.
- Risk: contributors think the fast PR check means release readiness. Mitigation: name jobs clearly and include summary output showing which tier ran.
- Risk: wrapper regressions are hidden. Mitigation: include minimal `CorpusScript&Smoke` public-wrapper coverage in the fast PR lane.
- Risk: workflow tests become inconsistent. Mitigation: update preservation tests after accepting the new CI contract.
- Risk: caching masks flaky restore/build behavior. Mitigation: keep build-producing restore/build in the fast lane and treat caching only as an optimization.
- Risk: release-evidence lane is ignored. Mitigation: schedule it and require it for release/milestone gates.

## Open questions

None blocking.

Implementation details such as exact cron timing, whether push triggers include every active development branch, and how maintainers trigger optional PR release evidence can be settled in the spec or execution plan without changing the accepted direction.

## Decision log

| Date | Decision | Reason | Alternatives rejected |
|---|---|---|---|
| 2026-05-18 | Recommend hosted CI validation tiers for PR runtime. | PR #3 runtime was dominated by unfiltered Corpus release-evidence tests in the required CI path. | Keep required CI unchanged; remove slow tests; rely only on caching. |
| 2026-05-18 | Keep full release evidence outside the default required PR lane. | Release evidence remains important but should not be the default feedback cost for every PR. | Delete release evidence; mark fast PR check as release-ready. |
| 2026-05-18 | Preserve minimal public-wrapper smoke in the fast lane. | Public script entrypoints still need early confidence. | Replace all wrapper tests with in-process tests. |
| 2026-05-18 | Run Core/App/Windows test projects directly in fast PR CI. | Solution-level category filtering could silently skip uncategorized product tests. | Rely on `Fast|Contract` filters across the whole solution. |
| 2026-05-18 | Use separate workflows with stable check names. | Branch protection and workflow contract tests need predictable names and trigger boundaries. | Keep all tiers in one ambiguous workflow. |
| 2026-05-18 | Shadow-run `ci-fast-required` before making it required. | The fast lane should prove coverage and runtime before branch protection changes. | Replace broad required CI immediately. |
| 2026-05-18 | Require runtime reporting in the first implementation slice. | Hosted runtime regressions should be visible by lane, test project, category selection, and slow tests. | Treat reporting as an optional follow-up. |

## Next artifacts

- CI validation-tier spec or amendment to the test-runtime optimization spec.
- Matching test spec for workflow contract tests, category filters, runtime summaries, and release-evidence preservation.
- Execution plan for staged GitHub Actions changes.

## Follow-on artifacts

- Proposal review completed in the 2026-05-18 user review with verdict: revise, then approve.
- Spec drafted: [PR CI Validation Tiering](../../specs/pr-ci-validation-tiering.md).

## Readiness

Accepted after proposal-review revisions. Spec drafting is complete and ready for `spec-review`. The proposal changes hosted validation policy only; it does not change production behavior or remove release-evidence tests.
