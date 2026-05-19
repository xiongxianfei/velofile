# PR CI Validation Tiering Branch-Protection Handoff

Date recorded: 2026-05-18

## Status

- Branch protection status: not configured
- Evidence command: `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
- Result: GitHub returned `Branch not protected` (HTTP 404).
- No maintainer handoff recorded.
- Intended ordinary required check: `ci-fast-required`

## Interpretation

The repository has hosted shadow-run evidence for `ci-fast-required`, but repository files cannot change GitHub branch-protection settings by themselves. Do not claim GitHub branch protection has changed until maintainers record the external required-check handoff.

During rollout, the broad `ci` job remains available as a shadow/rollback path. If the handoff needs to be rolled back, make the broad closeout check required again and leave `ci-fast-required` optional until the issue is resolved.
