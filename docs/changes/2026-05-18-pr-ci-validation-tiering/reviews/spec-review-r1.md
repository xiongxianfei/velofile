# Spec Review R1: PR CI Validation Tiering

## Review Status

changes-requested

## Review Inputs

- Spec: `specs/pr-ci-validation-tiering.md`
- Proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- Proposal review: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/proposal-review-r1.md`
- Related spec: `specs/test-runtime-optimization.md`
- Project map: `docs/project-map.md`
- `AGENTS.md`
- `CONSTITUTION.md`

## Findings

### PRCI-SR1: Hosted runner and PowerShell shell contract is missing

- Severity: major
- Location:
  - `specs/pr-ci-validation-tiering.md`, Requirements section, R1-R39
  - `specs/pr-ci-validation-tiering.md`, Acceptance criteria, AC2-AC11
- Evidence:
  - The spec defines workflow identities, triggers, command selection, and failure behavior in R1-R39, but it does not require `ci-fast-required`, `ci-release-evidence`, or `ci-full-closeout` to run on a Windows GitHub Actions runner or to use `pwsh` for PowerShell/script steps.
  - `docs/project-map.md` records that current `.github/workflows/ci.yml` runs on `windows-latest`, installs .NET SDK 10.x, restores, builds, and invokes `scripts/ci.ps1`.
  - `AGENTS.md` records that the GitHub CI workflow runs on Windows with `pwsh`.
  - The new spec changes hosted CI behavior and will outrank those lower-priority guidance surfaces once approved, so the Windows/pwsh execution boundary needs to be part of the spec contract.
- Required outcome: Add a hosted execution-environment contract so the CI tiering spec requires Windows-hosted validation and PowerShell 7 for PowerShell/script steps, or explicitly records a reviewed exception with equivalent validation evidence.
- Safe resolution path:
  - Add requirements such as:
    - `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` MUST run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.
    - PowerShell/script steps in those lanes MUST use `pwsh` unless a step has a documented reason to use another shell.
    - Hosted lanes MUST install or select the repository-approved .NET SDK before running restore/build/test commands.
  - Add acceptance criteria proving the workflow contract tests check the runner OS, PowerShell shell, and .NET SDK setup.
  - Keep any exact SDK version pinning or `global.json` adoption decision in the spec, architecture, or plan; do not leave the runner family unspecified.
  - Rerun `spec-review` after the amendment.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Requirement clarity | pass | The lane names, triggers, command selection, release-evidence separation, summary content, and branch-protection boundary are mostly concrete. |
| Normative language | pass | Uppercase normative statements are requirement IDs, and acceptance criteria are observable. |
| Completeness | concern | PRCI-SR1: hosted runner OS, `pwsh` shell, and SDK setup boundary are absent from the contract. |
| Testability | pass | The stated requirements can map to workflow contract tests, summary-content tests, and manual branch-protection evidence. PRCI-SR1 needs additional checks before approval. |
| Examples | pass | Examples cover ordinary PR, uncategorized product tests, release evidence, closeout, runtime reporting, and rollback. |
| Compatibility | concern | PRCI-SR1: Windows CI compatibility is currently inherited from lower-priority guidance instead of the spec. Rollout and rollback are otherwise covered. |
| Observability | pass | Job summaries, selected tiers, durations, TRX slow-test details or limitations, and shadow-run comparison evidence are specified. |
| Security/privacy | pass | Secrets, token permissions, cache keys, new PR secrets, and private profile details are covered. |
| Non-goals | pass | Release evidence deletion, production behavior changes, public prepared-tool options, serialization changes, caching-as-primary, visual hard gates, and branch-protection claims are excluded. |
| Acceptance criteria | concern | AC2-AC11 prove workflow shape and commands but do not yet prove Windows runner, `pwsh`, or SDK setup. |

## Requirement Notes

- R1-R13: trigger and stable-name contract is sufficient.
- R14-R27: fast-lane command contract is sufficient except for the missing hosted execution environment in PRCI-SR1.
- R28-R34: release-evidence lane contract is sufficient at spec level; exact release branch/tag globs can remain downstream as long as documented patterns are required.
- R35-R39: closeout command preservation is clear, but the hosted closeout lane should also inherit the fixed Windows/pwsh environment after PRCI-SR1.
- R40-R48: runtime summary and artifact behavior is testable.
- R49-R53: shadow rollout and rollback behavior is clear.
- R54-R64: scope and security boundaries are explicit.

## Exact Wording Suggestions

Add requirements near the workflow identity or lane behavior section:

```text
R<new>. All hosted lanes introduced or changed by this spec MUST run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.

R<new>. PowerShell and repository script steps in those hosted lanes MUST use `pwsh` unless a step records a reviewed reason to use another shell.

R<new>. Each hosted lane MUST install or select the repository-approved .NET SDK before running restore, build, test, UI contract, release-evidence, or closeout commands.
```

Add acceptance criteria:

```text
AC<new>. Workflow contract validation proves `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` use a Windows runner, use `pwsh` for PowerShell/script steps, and set up the repository-approved .NET SDK before validation commands.
```

## Immediate Next Stage

`spec`

## Eventual Test-Spec Readiness

not-ready until PRCI-SR1 is resolved and an approving spec re-review is recorded.

## Stop Condition

Do not proceed to architecture, plan, or test-spec until the hosted execution-environment contract is added or an explicit reviewed exception is recorded.

## Isolation

This review is isolated. It records required spec changes but does not automatically revise the spec or start downstream stages.
