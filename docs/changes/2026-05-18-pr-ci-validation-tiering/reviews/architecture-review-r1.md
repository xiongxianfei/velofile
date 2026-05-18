# Architecture Review R1: PR CI Validation Tiering

## Result

- Review surface: canonical-architecture-update
- Review status: approved
- Material findings: none
- Recording status: recorded
- Recording blocker: none
- Review record: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/architecture-review-r1.md`
- Review log: `docs/changes/2026-05-18-pr-ci-validation-tiering/review-log.md`
- Review resolution: not-required
- Open blockers: none
- Required canonical updates: none
- Required ADR updates: none
- Next stage: plan

## Review Surface

The primary review surface is the canonical architecture update in `docs/architecture/system/architecture.md`, including the C4 context and container diagram changes. ADR 0012 is reviewed as the durable decision record for the same architecture change.

## Review Inputs

- Canonical architecture: `docs/architecture/system/architecture.md`
- Context diagram: `docs/architecture/system/diagrams/context.mmd`
- Container diagram: `docs/architecture/system/diagrams/container.mmd`
- ADR under review: `docs/adr/0012-hosted-pr-ci-validation-tiers.md`
- Related ADR: `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Approved spec: `specs/pr-ci-validation-tiering.md`
- Spec approval: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/spec-review-r2.md`
- Accepted proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- Project map: `docs/project-map.md`
- Current workflow references: `.github/workflows/ci.yml`, `.github/workflows/release.yml`
- Governance: `CONSTITUTION.md`, `AGENTS.md`

## Findings

None.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Spec alignment | pass | The architecture implements the approved spec boundaries: `ci-fast-required`, `ci-release-evidence`, `ci-full-closeout`, direct Core/App/Windows test selection, explicit Corpus filters, no default `ReleaseEvidence`, and preserved broad `scripts/ci.ps1`. |
| Package shape | pass | The update uses the canonical arc42 package, source C4 context/container diagrams, and ADR 0012 for the durable workflow-structure decision. |
| Boundary clarity | pass | Hosted CI validation workflows are separated from product runtime, corpus tooling, UI contract tooling, release evidence, branch-protection handoff, and local closeout validation. |
| Data ownership | pass | Runtime summaries, TRX artifacts, shadow-run evidence, branch-protection handoff records, runner/shell exception evidence, and workflow contract test inputs have explicit ownership. |
| Interface safety | pass | The design preserves public wrapper behavior and `scripts/ci.ps1`, avoids new public prepared-tool options, and keeps branch protection as a maintainer-operated external surface. |
| Runtime and failure handling | pass | The runtime view covers validation failure summaries, missing TRX limitation reporting, release-evidence separation, closeout failure propagation, shadow rollout, and rollback by restoring the broad required gate. |
| Deployment and execution boundaries | pass | Workflow files, GitHub Actions Windows runners, `pwsh`, approved SDK setup, summaries, artifacts, and static workflow contract tests are covered in the deployment view. |
| Security/privacy | pass | The architecture prohibits secrets, signing material, release tokens, credentials, raw private profile details, and unrelated machine inventory in summaries, artifacts, cache keys, and exception evidence. |
| Quality and operations | pass | QS-PR-CI-01 makes fast hosted feedback and explicit release evidence measurable through workflow contract tests and runtime summaries. |
| Testing feasibility | pass | The design is verifiable through static YAML workflow contract tests, summary-helper tests, release-evidence preservation checks, and shadow-run evidence checks. |
| Complexity discipline | pass | Separate workflows, a shared PowerShell summary helper, and structured YAML contract tests are proportional to the spec; caching and cross-platform validation remain out of scope. |
| ADR quality | pass | ADR 0012 records context, decision, alternatives, consequences, and follow-up, and it is compatible with ADR 0011's earlier decision to defer hosted CI splitting until a later accepted decision. |
| Plan readiness | pass | No architecture questions block execution planning. Exact cron timing, release tag globs, push branch scope, and maintainer handoff mechanics can be settled in the plan without changing the architecture contract. |

## C4, Arc42, And ADR Notes

- The arc42 package retains lifecycle metadata followed by all 12 official sections in order.
- The context diagram includes GitHub Actions hosted Windows runners as an external system for contributor validation feedback.
- The container diagram adds hosted CI validation workflows as a repository workflow container, not as a product runtime component.
- No additional component diagram is required because the change affects repository validation orchestration rather than internal App/Core/Windows component responsibilities.
- The deployment view covers workflow files, runner and shell setup, SDK setup ordering, generated summaries, TRX artifacts, and static workflow contract tests.
- ADR 0012 records the long-lived workflow split and rejects broad PR closeout, deleted evidence, solution-level product-test filtering, inline summary duplication, caching as primary optimization, and Linux/macOS hosted validation for this spec.

## Suggested Changes

None required for architecture content.

Lifecycle note: this review record is the approval evidence for the architecture amendment and ADR 0012. The source artifact status strings still use pre-review wording, and can be normalized by a later status-settlement edit if the project wants the architecture and ADR text to carry the approval marker directly.

## Readiness

The architecture package is ready for `plan`. This `architecture-review` request is isolated and does not automatically start planning.
