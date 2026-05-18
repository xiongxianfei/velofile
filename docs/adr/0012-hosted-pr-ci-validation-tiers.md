# ADR 0012: Hosted PR CI Validation Tiers

## Status

accepted by architecture-review-r1

## Context

The current hosted PR workflow runs one broad Windows CI job that invokes `scripts/ci.ps1`. That broad closeout path gives strong confidence, but recent hosted evidence showed an ordinary PR took about 16 minutes, with `VeloFile.Corpus.Tests` dominating the runtime while Core, App, and Windows test projects completed in seconds.

ADR 0011 established the test-runtime taxonomy: `Fast`, `Contract`, `Smoke`, `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence`. The approved PR CI validation tiering spec now applies that taxonomy to hosted GitHub Actions without deleting release evidence, narrowing `scripts/ci.ps1`, changing production behavior, or treating caching as the primary speed fix.

The architecture decision is how hosted validation should be structured, how release evidence remains explicit, how future slowdowns become visible, and how the Windows execution environment remains part of the contract.

## Decision

Split hosted validation into stable, purpose-labeled lanes:

```text
ci-fast-required
  -> ordinary PR confidence

ci-release-evidence
  -> explicit expensive evidence and release-readiness validation

ci-full-closeout
  -> manual broad closeout through scripts/ci.ps1
```

Use separate workflow files for the three lanes unless a later accepted architecture decision changes the trigger model:

```text
.github/workflows/ci.yml
.github/workflows/release-evidence.yml
.github/workflows/closeout.yml
```

`ci-fast-required` restores, builds, validates production UI contracts, runs Core/App/Windows test projects directly without category filters, runs Corpus `Fast|Contract`, and runs Corpus `CorpusScript&Smoke`. It does not call `scripts/ci.ps1` and does not run `ReleaseEvidence` by default.

`ci-release-evidence` is runnable by `workflow_dispatch`, scheduled nightly or daily, release branches/tags, and merge queue when merge queue is used as a release-readiness gate. It runs explicit Corpus `ReleaseEvidence` and reports whether `Benchmark`, `Visual`, and `ManualEvidence` categories ran, were absent, or were intentionally not selected.

`ci-full-closeout` is a manual lane that invokes `scripts/ci.ps1`. The script remains the broad local and closeout command and must not be narrowed to fast filters as part of this change.

All hosted lanes introduced or changed by this decision run on Windows GitHub Actions runners, use `pwsh` for PowerShell and repository script steps unless a reviewed exception records equivalent validation behavior, and install or select the repository-approved .NET SDK before restore, build, test, UI contract, release-evidence, or closeout commands.

Runtime summary generation is owned by a shared PowerShell helper under `scripts/`, not duplicated as inline workflow fragments. Workflows pass lane identity, trigger, selected categories, release-evidence status, full-closeout status, command timings, and TRX paths to the helper. The helper writes the GitHub Actions job summary, extracts slow tests from TRX when available, reports missing structured output honestly, and avoids secrets or private local profile details.

Workflow contract tests are static tests over committed workflow YAML. They should use a structured YAML parser to load workflow structure into a test-owned model and prove lane names, triggers, Windows runner usage, `pwsh`, SDK setup ordering, command selection, release-evidence separation, and summary/reporting hooks. Those tests are the guardrail that replaces the old broad-PR preservation expectation.

Branch-protection changes remain an external maintainer-operated surface. The repository names `ci-fast-required` as the intended ordinary required PR check, shadow-runs it for at least one PR cycle, records comparison evidence, and does not claim required-check settings changed until maintainers record that handoff. Rollback is making the broad closeout check required again while leaving the fast lane optional.

## Alternatives considered

- Keep one broad hosted CI job: rejected because ordinary PR feedback continues to pay the full Corpus release-evidence cost.
- Remove expensive Corpus tests from hosted CI: rejected because it weakens release confidence and hides compatibility, diagnostics, preview, benchmark, wrapper, visual, or manual-evidence regressions.
- Use solution-level `Fast|Contract` filtering for the fast lane: rejected because Core/App/Windows tests are not guaranteed to be categorized, so product tests could be silently skipped.
- Put runtime summary logic inline in every workflow: rejected because summary wording, TRX parsing, privacy rules, and limitation reporting would drift across lanes.
- Create a new .NET reporting tool immediately: rejected for this slice because a PowerShell helper fits the existing hosted shell contract, can parse TRX XML, and avoids adding another tool project before the reporting shape stabilizes.
- Rely primarily on NuGet caching: rejected because measured hosted runtime is dominated by Corpus release-evidence execution, not restore/build.
- Run hosted validation on Linux or macOS: rejected for this spec because VeloFile is a Windows-native app and the hosted validation boundary remains Windows until a later accepted cross-platform design exists.

## Consequences

- Ordinary PRs can receive faster required confidence after shadow rollout and branch-protection handoff.
- Release evidence remains available and explicit through scheduled/manual/release/merge-queue validation and milestone closeout.
- The full closeout path remains stable through `scripts/ci.ps1`.
- Workflow contract tests become an architecture-critical proof surface for validation behavior.
- Runtime summaries become mandatory hosted observability and must distinguish fast PR confidence from release readiness.
- Branch protection cannot be fully changed by repository files; the maintainer handoff and rollback path must be recorded.
- Hosted validation stays Windows-native and `pwsh`-based, matching the app and current CI execution environment.

## Follow-up

- Architecture review approved this ADR and the matching canonical architecture amendment in `architecture-review-r1`.
- The execution plan must stage reporting, shadow-run fast CI, workflow contract tests, release-evidence workflow, full closeout workflow, and branch-protection handoff evidence without claiming external settings changed prematurely.
- The matching test spec must map PR CI requirements to workflow contract tests, runtime-summary tests, release-evidence preservation tests, and rollout evidence checks.
- A later accepted decision is required before adding Linux/macOS hosted validation, making dependency caching a correctness dependency, hard-gating screenshot evidence in the fast lane, or exposing new public prepared-tool script options.
