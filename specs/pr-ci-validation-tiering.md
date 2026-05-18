# PR CI Validation Tiering

## Status

approved

## Related proposal

- [PR CI Validation Tiering Proposal](../docs/proposals/2026-05-18-pr-ci-validation-tiering.md)
- [Proposal review R1](../docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/proposal-review-r1.md)

## Goal and context

This spec defines the hosted GitHub Actions validation contract for VeloFile pull requests, release evidence, and manual closeout validation.

The current hosted PR workflow runs the broad `scripts/ci.ps1` closeout path for every pull request. Recent hosted evidence showed PR #3 took about 16 minutes, with the repository CI script step at about 14 minutes and 58 seconds and `VeloFile.Corpus.Tests` alone at about 13 minutes and 22 seconds. Core, App, and Windows tests completed in seconds once build output was available.

The accepted test-runtime optimization contract already separates `Fast`, `Contract`, `CorpusScript`, `Smoke`, `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` validation tiers. This spec defines how hosted CI uses those tiers without deleting release evidence, weakening `scripts/ci.ps1`, or changing production VeloFile behavior.

## Glossary

- **Ordinary PR**: a normal pull request that needs fast required feedback but is not itself a release or milestone closeout gate.
- **Fast PR lane**: the hosted required confidence lane named `ci-fast-required`.
- **Release-evidence lane**: the hosted expensive evidence lane named `ci-release-evidence`.
- **Full closeout lane**: the hosted manual broad validation lane named `ci-full-closeout`.
- **Broad closeout command**: `scripts/ci.ps1`, which remains the unfiltered local and closeout validation entrypoint.
- **Product test projects**: `VeloFile.Core.Tests`, `VeloFile.App.Tests`, and `VeloFile.Windows.Tests`.
- **Corpus fast/contract tests**: `VeloFile.Corpus.Tests` selected by `TestCategory=Fast|TestCategory=Contract`.
- **Corpus script smoke tests**: `VeloFile.Corpus.Tests` selected by `TestCategory=CorpusScript&TestCategory=Smoke`.
- **Runtime summary**: the GitHub Actions job summary content that reports lane purpose, selected tiers, durations, and slow-test details when available.

## Examples first

### Example E1: ordinary PR receives fast required feedback

Given an ordinary pull request updates Core behavior
When GitHub Actions runs the required PR workflow
Then the `ci-fast-required` lane restores, builds, validates UI contracts, runs Core/App/Windows test projects directly, runs Corpus `Fast|Contract`, and runs Corpus `CorpusScript&Smoke`
And the lane summary states that `ReleaseEvidence` and full closeout did not run in that lane.

### Example E2: uncategorized product tests are not skipped by fast filtering

Given Core/App/Windows tests do not all carry `Fast` or `Contract` categories
When the fast PR lane runs product tests
Then it invokes each product test project directly without a category filter
And product tests are selected according to the test projects' normal behavior.

### Example E3: release evidence remains explicit

Given a release branch or release-candidate tag is validated
When the release-evidence workflow runs
Then `ci-release-evidence` runs explicit release-evidence validation for expensive Corpus evidence tiers
And the result is separate from ordinary PR fast confidence.

### Example E4: manual closeout remains broad

Given a maintainer needs milestone closeout evidence
When they run the full closeout workflow manually
Then `ci-full-closeout` invokes the broad `scripts/ci.ps1` command
And the job summary states that full closeout ran.

### Example E5: runtime regression is visible

Given a PR causes Corpus script smoke tests to slow down
When hosted CI completes
Then the job summary records per test project duration and slowest test details from TRX output when available
And the maintainer can see whether the slow lane was fast PR, release evidence, or full closeout.

### Example E6: rollback restores the broad required PR gate

Given the fast PR lane misses an unacceptable regression during shadow rollout
When maintainers roll back branch protection
Then the broad closeout workflow or previous broad CI check can become required again
And the fast PR lane can remain optional diagnostics.

### Example E7: hosted lane uses the approved execution environment

Given `ci-fast-required` is defined in GitHub Actions
When workflow contract validation inspects the lane
Then the lane runs on `windows-latest`, uses `pwsh` for run steps, sets up the repository-approved .NET SDK, and only then runs restore, build, and validation commands:

```yaml
jobs:
  ci-fast-required:
    name: ci-fast-required
    runs-on: windows-latest
    defaults:
      run:
        shell: pwsh
    steps:
      - uses: actions/checkout@v4
      - name: Set up .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.x
      - name: Restore
        run: dotnet restore VeloFile.sln
      - name: Build
        run: dotnet build VeloFile.sln -c Debug --no-restore
```

### Example E8: invalid hosted lane environment fails contract validation

Given a hosted lane uses `ubuntu-latest`
When workflow contract validation runs
Then it fails because the hosted lane is not using a Windows runner.

Given a repository script step invokes `./scripts/ci.ps1` with `shell: powershell`
When no reviewed exception is recorded
Then workflow contract validation fails because PowerShell/script steps are required to use `pwsh`.

Given a workflow runs `dotnet test VeloFile.sln -c Debug` before .NET SDK setup
When workflow contract validation runs
Then it fails because validation commands ran before the approved SDK setup.

## Requirements

### Workflow identity and triggers

R1. The repository MUST define a hosted fast PR workflow with a stable check or job name `ci-fast-required`.

R2. `ci-fast-required` MUST run on `pull_request`.

R3. `ci-fast-required` SHOULD run on pushes to active development branches when those pushes are used as integration signals.

R4. The repository MUST define a hosted release-evidence workflow with a stable check or job name `ci-release-evidence`.

R5. `ci-release-evidence` MUST be runnable by `workflow_dispatch`.

R6. `ci-release-evidence` MUST run on a nightly or daily `schedule`.

R7. `ci-release-evidence` MUST run for release branches, release tags, or release-candidate tags using documented branch/tag patterns.

R8. `ci-release-evidence` MUST support `merge_group` when merge queue is used as a release-readiness gate; if merge queue is not used, the spec or execution plan MUST record that condition.

R9. The repository MUST define a hosted full closeout workflow with a stable check or job name `ci-full-closeout`.

R10. `ci-full-closeout` MUST be runnable by `workflow_dispatch`.

R11. Ordinary pull requests MUST NOT run `ci-release-evidence` by default.

R12. Ordinary pull requests MUST NOT run `ci-full-closeout` by default.

R13. Branch-protection or repository required-check settings are a maintainer-operated configuration surface; repository artifacts MUST name `ci-fast-required` as the intended ordinary PR required check and MUST NOT claim the hosted setting changed unless maintainers record that change.

### Hosted execution environment

R65. All hosted lanes introduced or changed by this spec MUST run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.

R66. PowerShell and repository script steps in hosted lanes introduced or changed by this spec MUST use `pwsh` unless a step records a reviewed reason to use another shell.

R67. Each hosted lane introduced or changed by this spec MUST install or select the repository-approved .NET SDK before running restore, build, test, UI contract, release-evidence, or closeout commands.

R68. If a hosted lane uses a non-`windows-latest` Windows runner, the workflow or a linked evidence artifact MUST record the runner label, reason, and equivalent validation expectation.

R69. If a PowerShell or repository script step does not use `pwsh`, the workflow or a linked evidence artifact MUST record the exception reason and equivalent validation expectation.

### Fast PR lane behavior

R14. `ci-fast-required` MUST run `dotnet --info`.

R15. `ci-fast-required` MUST run `dotnet restore VeloFile.sln`.

R16. `ci-fast-required` MUST run `dotnet build VeloFile.sln -c Debug --no-restore` before any `--no-build` test command.

R17. `ci-fast-required` MUST validate the production UI token contract against `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, and `src/VeloFile.App/Resources`.

R18. `ci-fast-required` MUST run `tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj` directly without a category filter.

R19. `ci-fast-required` MUST run `tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj` directly without a category filter.

R20. `ci-fast-required` MUST run `tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj` directly without a category filter.

R21. `ci-fast-required` MUST run `tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj` with `TestCategory=Fast|TestCategory=Contract`.

R22. `ci-fast-required` MUST run `tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj` with `TestCategory=CorpusScript&TestCategory=Smoke`.

R23. `ci-fast-required` MUST NOT call `scripts/ci.ps1`.

R24. `ci-fast-required` MUST NOT run Corpus `ReleaseEvidence` by default.

R25. `ci-fast-required` MUST fail if any restore, build, UI contract validation, direct product test command, Corpus fast/contract command, or Corpus script smoke command fails.

R26. `ci-fast-required` MUST produce structured test output for test commands using TRX or an equivalent format when the test command starts.

R27. `ci-fast-required` MUST label itself and its summary as fast PR confidence, not release readiness.

### Release-evidence lane behavior

R28. `ci-release-evidence` MUST run build-producing restore/build validation before any `--no-build` release-evidence test command.

R29. `ci-release-evidence` MUST run explicit Corpus release-evidence validation using `TestCategory=ReleaseEvidence`.

R30. `ci-release-evidence` MUST keep benchmark, compatibility, preview, diagnostics, visual, and manual-evidence checks explicit and runnable when those checks exist in the accepted category taxonomy or release tooling.

R31. `ci-release-evidence` MUST NOT be the default required check for ordinary pull requests.

R32. `ci-release-evidence` MUST fail when a release-evidence command fails.

R33. `ci-release-evidence` MUST report whether `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` categories ran, were absent, or were intentionally not selected.

R34. `ci-release-evidence` MAY call the broad closeout command only when the lane is being used as a release-readiness gate and the job summary clearly reports that full closeout also ran.

### Full closeout lane behavior

R35. `ci-full-closeout` MUST invoke `scripts/ci.ps1`.

R36. `ci-full-closeout` MUST preserve `scripts/ci.ps1` as the broad closeout command rather than replacing it with fast filters.

R37. `scripts/ci.ps1` MUST remain runnable locally for milestone closeout and verification.

R38. `scripts/ci.ps1` MUST NOT be narrowed to `TestCategory=Fast|TestCategory=Contract`, `CorpusScript&Smoke`, or any other fast-only filter as part of this spec.

R39. `ci-full-closeout` MUST fail when `scripts/ci.ps1` fails.

### Runtime summaries and reporting

R40. Every hosted CI lane introduced or changed by this spec MUST write a GitHub Actions job summary even when validation fails after the job has started.

R41. Each runtime summary MUST report the lane name, trigger, selected validation categories, whether `ReleaseEvidence` ran, whether `CorpusScript&Smoke` ran, and whether full closeout ran.

R42. The `ci-fast-required` summary MUST explicitly report `ReleaseEvidence: not run in this lane`, `CorpusScript Smoke: run`, and `Full closeout: not run`.

R43. Each runtime summary MUST report total job duration and build duration when those values are available.

R44. Each runtime summary MUST report per test project duration when structured test output or command timing data is available.

R45. Each runtime summary MUST report the slowest tests from TRX or equivalent structured test output when available.

R46. If TRX or equivalent structured output is unavailable, the runtime summary MUST report that limitation instead of fabricating slow-test details.

R47. Runtime summaries SHOULD upload TRX or equivalent structured test output as artifacts when that output exists.

R48. Runtime summaries MUST NOT include secrets, tokens, credentials, signing material, or unrelated private local profile details.

### Rollout, rollback, and required-check transition

R49. The first implementation MUST shadow-run `ci-fast-required` as a non-required check for at least one PR cycle before maintainers change ordinary PR branch protection to require it.

R50. The shadow-run comparison MUST record `ci-fast-required` runtime, failures, selected categories, and whether the existing broad required check failed or passed for the same PR cycle when that evidence is available.

R51. Maintainers MAY make `ci-fast-required` the ordinary required PR check only after the shadow-run comparison is reviewed.

R52. Full release-evidence validation MUST remain required for release readiness and milestone closeout through either `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or an explicitly accepted release gate.

R53. Rollback MUST be possible by making the broad closeout check required again and leaving `ci-fast-required` optional.

### Scope boundaries

R54. This spec MUST NOT delete, skip, or hide release-evidence tests from the repository.

R55. This spec MUST NOT change production VeloFile App, Core, Windows, or Corpus behavior.

R56. This spec MUST NOT expose new public prepared-tool script options.

R57. This spec MUST NOT remove assembly-wide or class-level serialization.

R58. Dependency caching MUST remain secondary to validation-tier selection and MUST NOT be required for correctness.

R59. Cache misses MUST NOT cause the ordinary required PR job to run full release evidence as a fallback.

R60. Screenshot, visual, or manual evidence MUST NOT become a hard gate in `ci-fast-required`.

R61. Existing workflow contract tests that preserve broad unfiltered PR CI MUST be updated after this spec is approved so they preserve the new hosted CI policy instead.

R62. Ordinary pull-request workflows MUST NOT require new repository secrets.

R63. Pull-request workflows MUST NOT grant broader token permissions than needed for checkout, setup, validation, summary writing, and artifact upload.

R64. Dependency cache keys MUST NOT include secrets, credentials, tokens, signing material, or private local profile details.

## Inputs and outputs

Inputs:

- GitHub Actions events: `pull_request`, `push`, `workflow_dispatch`, `schedule`, release branch/tag pushes, and `merge_group` when used.
- Workflow files under `.github/workflows/`.
- `scripts/ci.ps1`.
- `VeloFile.sln` and the Core/App/Windows/Corpus test project files.
- MSTest category filters from the accepted test-runtime taxonomy.
- UI contract inputs under `docs/ui/` and `src/VeloFile.App/Resources`.
- TRX or equivalent structured test output.
- Branch-protection settings recorded by maintainers when changed.

Outputs:

- Hosted checks named `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout`.
- Passing or failing hosted workflow results.
- GitHub Actions job summaries for each lane.
- Optional uploaded test result artifacts.
- Change-record evidence for shadow-run comparison and branch-protection transition.

## State and invariants

- `scripts/ci.ps1` remains the broad closeout command.
- Ordinary PRs use fast required confidence after the shadow period and maintainer branch-protection update.
- Release evidence remains explicit and available outside the ordinary PR default path.
- Product test projects are not selected through Corpus category filters.
- Runtime summaries distinguish fast PR confidence from release readiness.
- No production App/Core/Windows behavior changes are required.

## Error and boundary behavior

- If a workflow file omits a required stable job name, workflow contract validation is expected to fail.
- If `ci-fast-required` applies `TestCategory=Fast|TestCategory=Contract` to the entire solution or to Core/App/Windows project tests, workflow contract validation is expected to fail.
- If `ci-fast-required` calls `scripts/ci.ps1`, workflow contract validation is expected to fail.
- If `ci-fast-required` runs `ReleaseEvidence` by default, workflow contract validation is expected to fail.
- If a hosted lane uses `ubuntu-latest`, `macos-latest`, or another non-Windows runner without a later accepted cross-platform validation design, workflow contract validation is expected to fail.
- If a PowerShell or repository script step omits `pwsh` at workflow, job, or step scope and has no reviewed exception, workflow contract validation is expected to fail.
- If a hosted lane runs restore, build, test, UI contract validation, release-evidence validation, or closeout commands before .NET SDK setup, workflow contract validation is expected to fail.
- If `ci-release-evidence` cannot identify whether release-evidence categories ran, the job summary reports the limitation and the spec/test review decides whether that is acceptable for the trigger.
- If a test command fails before TRX output is produced, the job summary reports the failed command and missing structured output.
- If GitHub Actions summary writing fails, the validation job either fails or reports the summary failure as part of job output rather than silently claiming reporting succeeded.
- If branch protection cannot be changed by repository files, the change record states the required maintainer action.

## Compatibility and migration

- The existing local closeout command `scripts/ci.ps1` remains compatible.
- Existing release evidence and expensive Corpus tests remain in the repository.
- Existing broad-CI preservation tests migrate from "PR CI always calls `scripts/ci.ps1`" to the new policy: fast PR workflow required, release-evidence and closeout workflows available.
- The current single-workflow hosted CI can coexist with `ci-fast-required` during shadow rollout.
- Branch protection can be rolled back to the broad closeout check if the fast lane proves insufficient.
- Dependency caching can be added after or alongside the fast lane only if cache keys are scoped and misses preserve the validation-tier contract.
- Runner and shell exceptions are for reviewed infrastructure constraints, not convenience. A lane that uses another Windows runner label or another shell still proves equivalent Windows validation behavior and does not silently change command semantics.
- Linux or macOS hosted validation is out of scope until a later accepted cross-platform validation design exists.

## Observability

- Job summaries are the primary hosted observability surface for this spec.
- Summaries make lane purpose and evidence tier selection explicit.
- Per test project duration and slowest tests come from structured output when available.
- Shadow-run comparison records include enough evidence to compare fast-lane failures and runtime against the existing broad check.
- Hosted success for `ci-fast-required` is not described as release readiness.

## Security and privacy

- Workflows avoid printing secrets, signing material, release tokens, or private local profile details in summaries or artifacts.
- TRX artifact upload avoids unnecessary private machine paths where feasible.
- Dependency cache keys do not contain secrets.
- Ordinary PR validation remains usable without new repository secrets.
- Pull-request workflows use the narrowest permissions needed for checkout, setup, test execution, summary writing, and artifact upload.
- Exception evidence for runner or shell changes avoids exposing secrets, private local profile details, or unrelated machine inventory.

## Accessibility and UX

This spec does not change product UI or accessibility behavior. Contributor-facing UX is in scope:

- Workflow names and summaries make the difference between fast PR confidence, release evidence, and full closeout clear.
- Failure output points to the failing lane and selected validation tier.
- Summary text avoids implying that a fast PR pass means release readiness.

## Performance expectations

This spec sets selection and reporting requirements, not a universal hosted duration guarantee.

- `ci-fast-required` is expected to be materially faster than the observed about-16-minute broad PR check by excluding default `ReleaseEvidence` and full closeout validation.
- Runtime success is not judged by one absolute wall-clock threshold because hosted runners vary.
- A fast lane that still runs full release evidence by default fails the contract even if it happens to be faster on one run.
- Runtime reports are required so future slowdowns are visible by lane, test project, and slowest tests.

## Edge cases

EC1. Core/App/Windows tests lack category metadata.

- Expected: `ci-fast-required` still runs those project tests directly without category filters.

EC2. Corpus fast/contract filter selects zero tests because category metadata regressed.

- Expected: workflow contract or category inventory validation fails before the lane is treated as healthy.

EC3. Corpus script smoke fails while product tests pass.

- Expected: `ci-fast-required` fails and reports `CorpusScript Smoke: run`.

EC4. Release-evidence workflow is triggered manually on a PR branch.

- Expected: it runs explicit release-evidence validation and reports that it is not the ordinary required PR lane.

EC5. Merge queue is enabled after the spec is implemented.

- Expected: `ci-release-evidence` adds or activates `merge_group` coverage, or maintainers record why merge queue is not a release-readiness gate.

EC6. A release tag uses a new naming pattern.

- Expected: release-evidence trigger patterns or release docs are updated before relying on that tag pattern for release readiness.

EC7. TRX files are missing because a build failed before tests started.

- Expected: the summary reports the build failure and records that slow-test details are unavailable.

EC8. Branch protection still requires the old broad CI check during shadow rollout.

- Expected: the change record says `ci-fast-required` is shadow-running and does not claim the required-check transition is complete.

EC9. Dependency cache misses on a hosted run.

- Expected: restore/build still run normally, and the job does not switch to full release-evidence validation to compensate.

EC10. A maintainer needs release readiness before merging a risky PR.

- Expected: they run `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or the accepted release gate explicitly rather than treating `ci-fast-required` as sufficient.

## Non-goals

- This spec does not remove or weaken release evidence.
- This spec does not delete expensive Corpus tests.
- This spec does not change production App, Core, Windows, or Corpus behavior.
- This spec does not expose public prepared-tool script options.
- This spec does not remove assembly-wide or class-level serialization.
- This spec does not make dependency caching the primary optimization strategy.
- This spec does not hard-gate screenshot or visual evidence in ordinary fast PR validation.
- This spec does not claim branch protection has changed until maintainers record that external configuration change.

## Acceptance criteria

- AC1. `specs/pr-ci-validation-tiering.md` links to the accepted proposal and proposal-review record.
- AC2. Workflow contract validation finds `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` with their required triggers.
- AC3. `ci-fast-required` runs restore/build before `--no-build` tests.
- AC4. `ci-fast-required` validates the production UI token contract.
- AC5. `ci-fast-required` runs Core/App/Windows test projects directly without category filters.
- AC6. `ci-fast-required` runs Corpus `Fast|Contract`.
- AC7. `ci-fast-required` runs Corpus `CorpusScript&Smoke`.
- AC8. `ci-fast-required` does not call `scripts/ci.ps1` and does not run `ReleaseEvidence` by default.
- AC9. `ci-release-evidence` can run explicit `ReleaseEvidence` validation on manual, scheduled, release branch/tag, and merge-queue release-readiness triggers when applicable.
- AC10. `ci-full-closeout` invokes `scripts/ci.ps1`.
- AC11. `scripts/ci.ps1` remains broad and is not narrowed to fast filters.
- AC12. Runtime summaries report lane purpose, selected categories, `ReleaseEvidence` status, `CorpusScript&Smoke` status, full-closeout status, durations, and slow-test details or a limitation.
- AC13. Shadow-run evidence exists before maintainers mark `ci-fast-required` as the ordinary required PR check.
- AC14. Workflow contract tests are updated so the new policy replaces the old broad-PR preservation expectation.
- AC15. No production App/Core/Windows behavior changes are required to satisfy this spec.
- AC16. Ordinary PR workflow definitions require no new repository secrets, use scoped token permissions, and do not put secrets or private local profile details into cache keys, summaries, or artifacts.
- AC17. Workflow contract validation proves `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.
- AC18. Workflow contract validation proves PowerShell and repository script steps in `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` use `pwsh`, unless the step records a reviewed exception.
- AC19. Workflow contract validation proves each hosted lane installs or selects the repository-approved .NET SDK before restore, build, test, UI contract validation, release-evidence validation, or closeout commands.
- AC20. Given a workflow lane omits a Windows runner, omits `pwsh` for repository script steps, or runs validation commands before .NET SDK setup, workflow contract validation fails with an actionable diagnostic.

## Open questions

None blocking.

Exact nightly cron timing, exact active development push branches, exact release tag glob names, and the maintainer process for recording external branch-protection changes belong in architecture, plan, or implementation artifacts as long as they preserve this contract.

## Next artifacts

- Architecture or ADR amendment for hosted CI validation tiering.
- Execution plan for workflow, reporting, and workflow contract test changes.
- Matching test spec for workflow contracts, runtime summaries, release-evidence preservation, and rollout evidence.

## Follow-on artifacts

None yet.

## Readiness

Approved by [spec-review-r2](../docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/spec-review-r2.md). The spec defines hosted validation behavior and reviewable evidence contracts. It does not authorize implementation before the required downstream architecture, plan, and test-spec stages.
