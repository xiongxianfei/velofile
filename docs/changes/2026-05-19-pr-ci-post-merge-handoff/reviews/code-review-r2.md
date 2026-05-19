# Code Review R2: PR CI Post-Merge Handoff M2

## Review status

clean-with-notes

## Review inputs

- Diff/review surface: commit `0bf7298 M2: Remove broad PR CI shadow after handoff`.
- Tracked governing branch state: local `main` ahead of `origin/main` by the M1 and M2 commits.
- Governing artifacts: `specs/pr-ci-validation-tiering.md`, `specs/pr-ci-validation-tiering.test.md`, `docs/adr/0012-hosted-pr-ci-validation-tiers.md`, and `docs/plans/2026-05-19-pr-ci-post-merge-handoff.md`.
- Validation evidence: M2 notes in `docs/changes/2026-05-19-pr-ci-post-merge-handoff/change.yaml`, the active plan, the M2 commit body, and independent rerun of the focused M2 validation command.

## Diff summary

M2 removes the broad `ci` job from `.github/workflows/ci.yml`, leaving `ci-fast-required` as the only default PR/push job in that workflow. It preserves broad closeout through `.github/workflows/closeout.yml` and local `scripts/ci.ps1`, records the ruleset-based `ci-fast-required` handoff, and updates README, CONTRIBUTING, project-map, workflow-contract tests, rollout-evidence tests, plan state, and change metadata.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Evidence |
|---|---|---|
| Spec alignment | pass | The default workflow keeps `ci-fast-required` and no longer runs broad closeout by default, matching R11-R12, R23-R24, R52-R53, and AC14. |
| Test coverage | pass | `Default_ci_workflow_no_longer_runs_broad_closeout_job` proves broad `ci` and `scripts/ci.ps1` are absent from `.github/workflows/ci.yml`; rollout tests prove guidance no longer describes broad `ci` as an ordinary PR shadow. |
| Edge cases | pass | The handoff evidence distinguishes the active repository ruleset from the classic branch-protection API 404, and rollback guidance remains explicit. |
| Error handling | pass | Fast-lane failure semantics and summary wiring remain unchanged; broad closeout failure semantics remain covered by `ci-full-closeout`. |
| Architecture boundaries | pass | The change is limited to validation infrastructure, tests, and documentation; no production App/Core/Windows behavior changes. |
| Compatibility | pass | `ci-full-closeout` and `scripts/ci.ps1` remain the broad closeout path, while release evidence remains in its separate workflow. |
| Security/privacy | pass | Workflow permissions remain `contents: read`; no secrets, tokens, or private local data are added to summaries or artifacts. |
| Derived artifact currency | pass | Change metadata, plan, plan index, project map, and rollout guidance now reflect the ruleset handoff and default fast-only PR workflow. |
| Unrelated changes | pass | The diff is scoped to the M2 workflow cleanup, handoff evidence, tests, and lifecycle/guidance artifacts. |
| Validation evidence | pass | Focused M2 test command passed with 22 tests; `git diff --check HEAD~1..HEAD` passed. |

## No-finding rationale

The reviewed slice removes the temporary broad PR shadow path only after recorded ruleset handoff evidence names `ci-fast-required` as required. It does not change fast-lane command selection, release-evidence policy, test categories, `scripts/ci.ps1`, or production behavior. Contract tests now fail if broad closeout returns to the default PR/push workflow, while still preserving explicit broad closeout through the manual closeout lane.

## Residual risks

- Hosted confirmation for the M2 workflow change has not run yet; M3 must capture that before lifecycle closeout.
- The required-check handoff is external GitHub ruleset state and should be rechecked when collecting M3 hosted evidence.

## Recommended next stage

Close M2. Move to M3 only after a hosted PR run exists for the M2 workflow change; until then the plan is blocked on hosted confirmation evidence.
