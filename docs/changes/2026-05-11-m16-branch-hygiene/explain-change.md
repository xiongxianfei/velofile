# M16 Branch Hygiene

## What Changed

Updated the V1 plan index and plan body so M16 lifecycle state matches the existing outcome evidence: M16 is complete, while V1 final verification remains pending.

Kept the earlier editorial `VISION.md` cleanup in this branch-hygiene slice by removing the stale open question about choosing the Windows UI/runtime stack and Shell integration strategy.

## Why

The M16 outcome and readiness text already recorded implementation, review, remediation, and verification evidence, but the progress checklist still left `M16 complete` unchecked. That mismatch would block final verification and PR handoff.

## Validation

- `pwsh -NoProfile -Command '$vision = Get-Content -Raw VISION.md; $words = ([regex]::Matches($vision, "\b\S+\b")).Count; if ($words -gt 900) { throw "VISION.md exceeds 900 words: $words" }; $required = @("Pitch","What makes this different","Who it is for","Who it is not for","What it commits to","What it refuses to be","What would prove this wrong"); foreach ($heading in $required) { if ($vision -notmatch "## $([regex]::Escape($heading))") { throw "Missing VISION section: $heading" } }'`
- `pwsh -NoProfile -Command '$matches = rg -n "docs/(workflows|roadmap)\\.md" AGENTS.md CONSTITUTION.md docs specs .github README.md CONTRIBUTING.md; if ($LASTEXITCODE -eq 0) { $matches; exit 1 } elseif ($LASTEXITCODE -eq 1) { exit 0 } else { exit $LASTEXITCODE }'`
- `git diff --check`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1 -SkipPublish`
