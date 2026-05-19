# PR CI Post-Merge Branch-Protection Handoff

Date recorded: 2026-05-19

## Status

- Branch: `main`
- Branch protection status: not configured
- Evidence command: `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
- Result: GitHub returned `Branch not protected` (HTTP 404).
- No maintainer handoff recorded.
- Intended ordinary required check: `ci-fast-required`
- Do not claim GitHub branch protection has changed.

## Merge And Shadow Evidence

- Pull request: https://github.com/xiongxianfei/velofile/pull/4
- PR state: `MERGED`
- Merge commit: `37a17cb1ced1f8d213aad258d8b4514434454b3d`
- Merged at: `2026-05-19T06:44:21Z`
- Final observed hosted PR run: https://github.com/xiongxianfei/velofile/actions/runs/26065439926
- Final observed hosted PR head: `85fbb0bc5e6bee98c9055c4ad284579474f8a8b0`
- `ci-fast-required`: passed
- Broad `ci`: passed
- Accepted shadow-run evidence remains recorded in `docs/changes/2026-05-18-pr-ci-validation-tiering/shadow-run.md`.

## Interpretation

The repository has hosted shadow-run evidence for `ci-fast-required`, but repository files cannot change GitHub branch-protection settings by themselves. Because `main` branch protection is not configured, M2 blocked: do not remove or disable the temporary broad `ci` job from default CI until maintainers record the external handoff requiring `ci-fast-required`.

During this blocked state, the broad `ci` job remains unchanged as a shadow/rollback path. Broad closeout validation must remain available through `ci-full-closeout` and local `scripts/ci.ps1` after handoff. If rollback is needed, make the broad closeout check required again and leave `ci-fast-required` optional until the issue is resolved.
