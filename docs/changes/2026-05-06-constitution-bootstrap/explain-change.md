# Constitution Bootstrap

## What Changed

Added `CONSTITUTION.md` as the durable governance source for VeloFile agentic development. It defines project purpose, source-of-truth order, spec/test discipline, architecture boundaries, privacy/security rules, compatibility expectations, verification requirements, review routing, documentation ownership, agent behavior rules, and fast-lane exceptions.

Updated `AGENTS.md` so its instruction precedence and required-reading sections point to `CONSTITUTION.md` and tracked architecture/plan artifacts instead of deleted workflow documents. `CONTRIBUTING.md` now asks contributors to read both the constitution and agent operating guide.

Recorded that local `.codex` files are ignored and are not tracked repository governance.

## Why

The repository had no `CONSTITUTION.md`, while active guidance still pointed to deleted workflow and roadmap documents. That made source-of-truth resolution ambiguous and caused verification to block on governance drift.

## Validation

- `pwsh -NoProfile -Command '$matches = rg -n "docs/(workflows|roadmap)\\.md" AGENTS.md CONSTITUTION.md docs specs .github README.md CONTRIBUTING.md; if ($LASTEXITCODE -eq 0) { $matches; exit 1 } elseif ($LASTEXITCODE -eq 1) { exit 0 } else { exit $LASTEXITCODE }'`
- `git diff --check`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1 -SkipPublish`
