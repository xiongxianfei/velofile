# Project Map Bootstrap

## What Changed

Added `docs/project-map.md` as a high-level observed map of the VeloFile repository. It documents the current source projects, module boundaries, runtime flows, data flows, external Windows boundaries, test layout, CI/release scripts, architecture patterns, risks, and open questions.

## Why

`CONSTITUTION.md` and workflow guidance treat `docs/project-map.md` as the durable orientation surface for agents and reviewers. The file was absent, so cross-module work still required rediscovering source layout, ownership, test locations, and release evidence paths from scratch.

## Validation

- `git diff --check`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1 -SkipPublish`
