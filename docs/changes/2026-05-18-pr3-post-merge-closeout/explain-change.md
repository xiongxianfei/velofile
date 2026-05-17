# PR 3 Post-Merge Closeout

## What Changed

Moved the merged PR #3 work out of the Active plan index and into Done:

- `2026-05-11 UI design-system and shell redesign`
- `2026-05-11 UI shell visual coherence`
- `2026-05-16 test runtime optimization`

Updated each plan body from PR-handoff readiness to done, with the PR URL, merge commit, and hosted CI result.

## Why

PR #3 merged after hosted `ci` passed, so the prior lifecycle state was stale. The plan index still listed the related work as active with `pr` as the next step even though PR handoff and merge were complete.

## Validation

- `gh pr view 3 --json number,state,mergedAt,mergeCommit,url,headRefName,baseRefName,statusCheckRollup`
- `git diff --check -- docs/plan.md docs/plans/2026-05-11-ui-design-system-shell-redesign.md docs/plans/2026-05-11-ui-shell-visual-coherence.md docs/plans/2026-05-16-test-runtime-optimization.md docs/changes/2026-05-18-pr3-post-merge-closeout/change.yaml docs/changes/2026-05-18-pr3-post-merge-closeout/explain-change.md`
