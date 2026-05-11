# Review Resolution

## Status

resolved; ready for plan-review rerun

## Findings

### PR-001: Region milestones can close without required full-shell visual evidence

- Source review: [plan-review-r1](reviews/plan-review-r1.md)
- Status: resolved
- Required outcome: revise M2-M7 so each region milestone includes a soft full-shell screenshot or explicit manual visual evidence step before the milestone can close, and keep M8 as consolidation/baseline inventory rather than the first visual-evidence point.
- Resolution plan:
  - Update M2-M7 validation commands and milestone closeout criteria with per-region visual evidence.
  - Add a generic closeout bullet requiring full-shell screenshot or explicit manual visual evidence for the touched region, with deviations recorded when needed.
  - Keep M8 responsible for full-shell evidence inventory, baseline/profile consolidation, sidecar checks, and `200%` automated-or-manual classification.
- Validation: pending.
- Resolution:
  - Added a `Region-Slice Visual Evidence Rule` to `docs/plans/2026-05-11-ui-shell-visual-coherence.md`.
  - Updated M2-M7 validation commands and closeout criteria so each region milestone requires full-shell screenshot evidence or an explicit manual visual-review note before closeout.
  - Added milestone-specific evidence states for `shell-default`, `shell-file-list-selected-focused`, `shell-filter-active`, `shell-search-active`, `shell-sidebar-focused` or `shell-default`, `shell-operation-running`, `shell-destructive-confirmation`, and `shell-preview-open`.
  - Reframed M8 as full-shell evidence consolidation and baseline inventory rather than the first screenshot milestone.
- Validation:
  - `git diff --check -- docs/plans/2026-05-11-ui-shell-visual-coherence.md docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md docs/changes/2026-05-11-ui-shell-visual-coherence/change.yaml`
