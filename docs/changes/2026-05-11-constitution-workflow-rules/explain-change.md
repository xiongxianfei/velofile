# Constitution Workflow Rules

## What Changed

Updated `CONSTITUTION.md` so the final required governance section is `Standard Workflow And Manual Skill Use`. It now records the standard workflow chain, manual individual-stage isolation, milestone repetition rules, final closeout preconditions, and fast-lane exception limits.

Updated `AGENTS.md` to point contributors and agents at the same standard workflow while keeping the detailed governance in `CONSTITUTION.md`.

## Why

After `.codex` became local-only, tracked repository governance needed to own the workflow and manual-skill rules directly. Keeping the rules only in local ignored skill files would make them unavailable as tracked project guidance.

## Validation

- `pwsh -NoProfile -Command '$required = @("Project Purpose","Source Of Truth Order","Spec-Driven Rules","Test-Driven Rules","Architecture Rules","Security And Privacy Rules","Compatibility Rules","Verification Rules","Review Rules","Documentation Rules","Agent Behavior Rules","Standard Workflow And Manual Skill Use"); $text = Get-Content -Raw CONSTITUTION.md; foreach ($heading in $required) { if ($text -notmatch "## $([regex]::Escape($heading))") { throw "Missing constitution section: $heading" } }'`
- `pwsh -NoProfile -Command '$matches = rg -n "docs/(workflows|roadmap)\\.md" AGENTS.md CONSTITUTION.md docs specs .github README.md CONTRIBUTING.md; if ($LASTEXITCODE -eq 0) { $matches; exit 1 } elseif ($LASTEXITCODE -eq 1) { exit 0 } else { exit $LASTEXITCODE }'`
- `git diff --check`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1 -SkipPublish`
