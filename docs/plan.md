# Execution Plan Index

`docs/plan.md` is the lifecycle index for execution plans. Plan bodies live under `docs/plans/`.

## Draft

| Plan | Scope | Status | Next step |
|---|---|---|---|
| None. | | | |

## Active

| Plan | Scope | Status | Next step |
|---|---|---|---|
| [2026-05-19 PR CI post-merge handoff](plans/2026-05-19-pr-ci-post-merge-handoff.md) | Record branch-protection handoff after PR #4, then remove the broad closeout job from default CI while preserving release-evidence and full-closeout lanes. | M2 implementation complete; code-review requested | code-review M2 |

## Blocked

| Plan | Scope | Status | Next step |
|---|---|---|---|
| None. | | | |

## Done

| Plan | Scope | Status | Completed |
|---|---|---|---|
| [2026-05-18 PR CI validation tiering](plans/2026-05-18-pr-ci-validation-tiering.md) | Stage hosted CI validation tiers: runtime summaries, fast PR shadow lane, release-evidence workflow, full closeout workflow, and branch-protection handoff evidence. | PR #4 merged; hosted CI passed | 2026-05-19 |
| [2026-05-16 test runtime optimization](plans/2026-05-16-test-runtime-optimization.md) | Split validation tiers, optimize corpus test runtime, preserve public script smoke and release evidence, and record runtime evidence. | PR #3 merged; hosted CI passed | 2026-05-18 |
| [2026-05-11 UI shell visual coherence](plans/2026-05-11-ui-shell-visual-coherence.md) | Implement the follow-on shell-wide visual-coherence work: shell surface foundation, deterministic fixture icons, command band, sidebar, status/operation, preview/details, and optional visual-review artifacts. | PR #3 merged; hosted CI passed | 2026-05-18 |
| [2026-05-11 UI design-system and shell redesign](plans/2026-05-11-ui-design-system-shell-redesign.md) | Implement the first UI design-system slice: repo-owned tokens, WinUI resources, file-list row redesign, fixture mode, and visual evidence. | PR #2 merged; follow-up PR #3 merged; hosted CI passed | 2026-05-18 |
| [2026-05-04 V1 product scope](plans/2026-05-04-v1-product-scope.md) | Implement the approved VeloFile V1 product scope from empty/template-stage repository through MSIX preview readiness. | PR #1 merged; hosted CI passed | 2026-05-11 |

## Superseded

None.
