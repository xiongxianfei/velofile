# Contributing

Thanks for contributing.

## Before opening a pull request

1. Read `CONSTITUTION.md` and `AGENTS.md`.
2. Check whether the task needs a plan or spec.
3. Keep the change small and reviewable.
4. Run the relevant verification and list the commands in the PR.
5. Update docs or examples when behavior changes.

## CI validation tiers

- `ci-fast-required` is the intended ordinary PR confidence check after maintainer handoff. It is fast PR feedback, not release readiness. Its summary reports `ReleaseEvidence: not run in this lane`, `CorpusScript Smoke: run`, and `Full closeout: not run`.
- `ci-release-evidence` runs explicit release-evidence validation for manual, scheduled, release, and merge-queue gates.
- `ci-full-closeout` runs the full closeout path through `scripts/ci.ps1`.

During rollout, broad CI may still shadow ordinary PRs until branch-protection handoff is recorded. The rollback path is to make the broad closeout check required again and leave `ci-fast-required` optional.

## Pull request expectations

- One focused change per PR.
- Explain why the change exists.
- State what was verified.
- Call out assumptions, scope limits, and follow-up work.

## Good first contributions

- docs clarifications
- small bug fixes with regression coverage
- test improvements
- build and tooling cleanup with clear scope
