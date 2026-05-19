# Code Review R3: PR CI Post-Merge Handoff M3

## Review status

clean-with-notes

## Review inputs

- Diff/review surface: `6d9f276..HEAD`, covering commits `b29fd24 M3: Fix post-handoff closeout contract tests` and `c2f4abe M3: Record hosted PR CI handoff confirmation`.
- Tracked governing branch state: the reviewed implementation surface was tracked on `pr-ci-post-merge-handoff` / `origin/pr-ci-post-merge-handoff` at `c2f4abe067612fec3dbadda4c5048e918d613c70` before this review receipt was recorded.
- Governing artifacts: `specs/pr-ci-validation-tiering.md`, `specs/pr-ci-validation-tiering.test.md`, `docs/adr/0012-hosted-pr-ci-validation-tiers.md`, and `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`.
- Validation evidence: M3 notes in `docs/changes/2026-05-19-pr-ci-post-merge-handoff/change.yaml`, hosted PR #5 runs `26086191007` and `26086704434`, the M3 hosted confirmation artifact, and independent rerun of the focused M3 validation command.

## Diff summary

M3 records hosted post-handoff confirmation for PR #5, including a passed `ci-fast-required` run, per-step durations, selected categories, and ruleset-required-check status. It also updates stale contract tests so broad closeout preservation points at `.github/workflows/closeout.yml` and `ci-full-closeout` rather than the removed broad `ci` default workflow job. The milestone adds rollout evidence coverage for the new hosted confirmation artifact and updates plan/change metadata to hand the milestone to review.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Evidence |
|---|---|---|
| Spec alignment | pass | The evidence records ordinary PR behavior with only `ci-fast-required`, confirms broad closeout is not run by default, and keeps release readiness tied to release-evidence/full closeout paths, matching R11-R13, R49-R53, AC13, and PRCI-M001. |
| Test coverage | pass | `Post_merge_hosted_confirmation_records_fast_only_pr_cycle` proves the new hosted confirmation artifact names PR #5, the hosted run, selected categories, fast-only status, and ruleset handoff status. |
| Edge cases | pass | The earlier failed hosted run is retained as evidence, and the stale broad-CI preservation assertions are redirected to the closeout workflow without changing fast-lane command selection. |
| Error handling | pass | The broad closeout runtime-summary test now checks `full_closeout` failure context and `if: always()` in `closeout.yml`; fast-lane failure semantics are unchanged. |
| Architecture boundaries | pass | The diff is limited to validation evidence, workflow contract tests, and lifecycle metadata; no production App/Core/Windows behavior changes. |
| Compatibility | pass | `ci-full-closeout` and `scripts/ci.ps1` remain the broad closeout path while ordinary PR evidence stays fast-only. |
| Security/privacy | pass | Hosted evidence records public PR/run/check metadata and no secrets or private local values. |
| Derived artifact currency | pass | Plan, change metadata, review log inputs, and rollout evidence now agree on M3 scope and hosted confirmation state. |
| Unrelated changes | pass | The diff is scoped to M3 hosted confirmation, stale closeout test alignment, and lifecycle artifacts. |
| Validation evidence | pass | Review reran `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~CiRolloutEvidence|FullyQualifiedName~ValidationCommandDocumentation"` with 23 passing tests and `git diff --check 6d9f276..HEAD` with no output. Hosted current head `c2f4abe` passed `ci-fast-required` in run `26086704434`. |

## No-finding rationale

The reviewed M3 slice closes the evidence gap that blocked post-merge handoff: hosted PR #5 demonstrates the default PR workflow runs `ci-fast-required` without a broad `ci` job, and the active ruleset handoff remains recorded without claiming classic branch protection is configured. The stale tests that caused the first hosted failure now preserve the accepted closeout contract through `closeout.yml`, which matches the approved post-handoff policy and avoids weakening `scripts/ci.ps1` or release-evidence validation.

## Residual risks

- The hosted confirmation artifact records accepted run `26086191007`; review also inspected current-head run `26086704434`, which passed `ci-fast-required` in 5m20s. The artifact is not rewritten to the latest run to avoid recursive evidence-only churn.
- GitHub runner deprecation notices for Node.js 20 actions and `windows-latest` redirection appeared on hosted runs. They are warnings outside this milestone's approved scope.

## Recommended next stage

Close M3. Because no implementation milestones remain open, the next lifecycle stage is `explain-change`; do not claim verify, PR readiness, or final closeout until those stages run.
