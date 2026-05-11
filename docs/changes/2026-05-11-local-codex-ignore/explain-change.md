# Local Codex Ignore

## What Changed

Stopped tracking `.codex` as repository content and added `.codex/` to `.gitignore`. The local files remain on disk, but Git no longer tracks them.

Updated tracked governance and orientation docs so `.codex` is described as local ignored tooling rather than a durable source-of-truth artifact.

## Why

The maintainer clarified that `.codex` is for local use only. Keeping it tracked made local agent configuration look like project governance and caused stale local-skill drift to block repository verification.

## Validation

- `git ls-files .codex`
- `pwsh -NoProfile -Command '$matches = rg -n "docs/(workflows|roadmap)\\.md" AGENTS.md CONSTITUTION.md docs specs .github README.md CONTRIBUTING.md; if ($LASTEXITCODE -eq 0) { $matches; exit 1 } elseif ($LASTEXITCODE -eq 1) { exit 0 } else { exit $LASTEXITCODE }'`
- `git check-ignore .codex/skills/workflow/SKILL.md`
- `git diff --check`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1 -SkipPublish`
