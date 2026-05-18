# Spec Review R2: PR CI Validation Tiering

## Review Status

approved

## Review Inputs

- Spec: `specs/pr-ci-validation-tiering.md`
- Proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- Prior spec review: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/spec-review-r1.md`
- Review resolution: `docs/changes/2026-05-18-pr-ci-validation-tiering/review-resolution.md`
- Related spec: `specs/test-runtime-optimization.md`
- Project map: `docs/project-map.md`
- `AGENTS.md`
- `CONSTITUTION.md`

## Findings

None.

## PRCI-SR1 Resolution Check

PRCI-SR1 is resolved. The amended spec now makes the hosted execution environment part of the CI tiering contract:

- R65 requires hosted lanes introduced or changed by this spec to run on `windows-latest` or another explicitly approved Windows GitHub Actions runner.
- R66 requires PowerShell and repository script steps to use `pwsh` unless a reviewed exception is recorded.
- R67 requires repository-approved .NET SDK setup before restore, build, test, UI contract, release-evidence, or closeout commands.
- R68 and R69 define narrow exception evidence for non-`windows-latest` Windows runners and non-`pwsh` PowerShell/script steps.
- E7 and E8 show valid and invalid hosted execution environment examples.
- AC17-AC20 require workflow contract validation for runner OS, `pwsh`, SDK setup ordering, and actionable diagnostics.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Requirement clarity | pass | Stable lane names, triggers, selected commands, hosted environment, rollout, rollback, and branch-protection boundaries have one intended interpretation. |
| Normative language | pass | `MUST`, `SHOULD`, `MAY`, and `MUST NOT` statements are scoped to observable CI contract behavior. |
| Completeness | pass | The spec covers ordinary PR, release-evidence, full closeout, product-test filtering, hosted runner/shell/SDK setup, reporting, security, rollback, and branch-protection handoff cases. |
| Testability | pass | Requirements map to workflow contract tests, summary-output checks, category-filter checks, release-evidence preservation tests, and shadow-run evidence. |
| Examples | pass | Examples cover fast PR feedback, uncategorized product tests, release evidence, closeout, runtime regression visibility, rollback, and valid/invalid hosted execution environments. |
| Compatibility | pass | `scripts/ci.ps1`, release-evidence tests, product behavior, Windows-hosted validation, and rollback remain preserved. |
| Observability | pass | Job summaries, selected categories, lane purpose, durations, slow-test details, missing TRX limitations, and shadow-run comparison evidence are specified. |
| Security/privacy | pass | Secrets, token permissions, cache keys, artifacts, summaries, and exception evidence are bounded. |
| Non-goals | pass | Production behavior changes, release-evidence deletion, public prepared-tool script options, serialization removal, caching-as-primary, and fast PR visual hard gates remain excluded. |
| Acceptance criteria | pass | AC1-AC20 are observable, including the new runner, shell, SDK setup, and actionable-diagnostic criteria. |

## Requirement Notes

- R1-R13: workflow identity, trigger, and branch-protection boundary is sufficient for architecture and planning.
- R65-R69: the hosted execution-environment contract closes PRCI-SR1 and is testable by static workflow validation.
- R14-R27: fast PR behavior avoids solution-level category filtering for Core/App/Windows tests and keeps Corpus filters explicit.
- R28-R39: release-evidence and closeout behavior preserves full evidence and the broad closeout command.
- R40-R48: runtime summaries are mandatory and include the required limitation paths.
- R49-R53: shadow rollout and rollback are explicit enough for an execution plan.
- R54-R64: scope, secrets, token permissions, caching, and production-behavior boundaries are clear.

## Immediate Next Stage

`architecture`

## Eventual Test-Spec Readiness

conditionally-ready after architecture and execution planning confirm workflow structure, reporting helper boundaries, and rollout sequencing.
