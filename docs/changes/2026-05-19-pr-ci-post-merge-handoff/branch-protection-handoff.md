# PR CI Post-Merge Branch-Protection Handoff

Date recorded: 2026-05-19

## Status

- Branch: `main`
- Protection mechanism: repository ruleset `protect`
- Ruleset id: `16578519`
- Ruleset target: `branch`
- Ruleset enforcement: `active`
- Ruleset condition: default branch (`~DEFAULT_BRANCH`)
- Required status check: `ci-fast-required`
- Required status check integration id: `15368`
- Ruleset evidence command: `gh api repos/xiongxianfei/velofile/rulesets/16578519 --jq '{id, name, target, enforcement, conditions, rules}'`
- Ruleset result: active default-branch ruleset includes `required_status_checks` for `ci-fast-required`.
- Classic branch-protection evidence command: `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
- Classic branch-protection result: GitHub returned `Branch not protected` (HTTP 404).
- Maintainer handoff recorded: ruleset now requires `ci-fast-required`.
- Ordinary required check: `ci-fast-required`
- Do not claim classic GitHub branch protection is configured; the observed protection is ruleset-based.

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

The repository has hosted shadow-run evidence for `ci-fast-required`, and the active default-branch ruleset now requires `ci-fast-required`. Repository files still cannot mutate GitHub branch-protection settings by themselves, but the maintainer-operated handoff is now recorded through the active ruleset.

M2 removed the temporary broad `ci` job from default CI after this handoff evidence was recorded. Broad closeout validation remains available through `ci-full-closeout` and local `scripts/ci.ps1`. If rollback is needed, make the broad closeout check required again and leave `ci-fast-required` optional until the issue is resolved.
