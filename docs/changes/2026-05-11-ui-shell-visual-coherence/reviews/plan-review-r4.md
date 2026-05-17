# Plan Review R4: Visual-Evidence Gate Removal Amendment Recheck

## Verdict

approve

## Review Inputs

- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Approved spec amendment: `specs/ui-shell-visual-coherence.md`
- Spec review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/spec-review-r2.md`
- Approved architecture amendment: `docs/architecture/system/architecture.md`
- ADR: `docs/adr/0010-shell-visual-coherence-contracts.md`
- Architecture review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/architecture-review-r1.md`
- Prior plan review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r3.md`
- Resolution record: `docs/changes/2026-05-11-ui-shell-visual-coherence/review-resolution.md`

## Findings

No material findings.

## PR-002 Resolution Confirmation

PR-002 is resolved.

- `docs/plans/2026-05-11-ui-shell-visual-coherence.md` now says optional full-shell screenshot and sidecar inventory validation runs in M8 only when optional visual artifacts are recorded.
- The manual/review evidence section now says to review chosen optional full-shell screenshots for their recorded states and profiles when visual artifacts are recorded.
- M8 validation commands are conditional on optional visual artifacts existing and reviewed current screenshots being available for baseline update commands.
- M4 milestone state was normalized to `amendment-downstream-review-needed`.
- Focused stale hard-gate wording scan found no active matches in the plan.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Self-contained context | pass | The plan identifies accepted proposal/spec/architecture inputs, current closed milestones, and the amendment state. |
| Source alignment | pass | The plan now matches R66-R77 and AC9-AC13: visual artifacts are optional, sidecars are governed when present, and behavior proof remains separate. |
| Milestone size | pass | M4-M7 remain separate region slices; M8 is optional and scoped to artifact guardrails only if artifacts exist. |
| Sequencing | pass | The plan preserves the already-implemented M1-M3 sequence and correctly routes the amendment through test-spec review before M4 code review can rely on it. |
| Scope discipline | pass | Non-goals still prevent pixel gates, V1 behavior expansion, persisted theme settings, and production `hifi-design` authority. |
| Validation quality | pass | Required validation stays on static UI contracts, app route/build tests, behavior preservation, accessibility, CI, and deviations; visual artifact validation is conditional. |
| TDD readiness | pass | Each remaining region identifies focused tests to add/update; optional visual artifact tests are isolated to M8 when applicable. |
| Risk coverage | pass | Risks cover behavior regression, validation overreach, screenshots, high-DPI instability, sidebar behavior, region mismatch, and rollback. |
| Architecture alignment | pass | The plan follows architecture-review-r1 and ADR 0010: visual artifacts are optional soft-review context and never replace behavior proof. |
| Operational readiness | pass | Generated current/diff outputs remain uncommitted, full CI remains in milestone validation, and no release/evidence gate depends on screenshots. |
| Plan maintainability | pass | Current handoff summary, decision log, validation notes, and review-history notes identify the remaining test-spec review and M4 code-review blocker. |

## Reviewer Validation

- `git diff --check`: passed with CRLF normalization warnings only.
- `rg -n "required screenshot|required full-shell|required visual evidence|required states and profiles|Full-shell screenshot and sidecar inventory validation runs in M8|full-shell evidence|visual-evidence-needed|m4-visual-evidence-needed|M8 remains blocked" docs\plans\2026-05-11-ui-shell-visual-coherence.md`: no matches.
- `Select-String -Path docs\changes\2026-05-11-ui-shell-visual-coherence\change.yaml -Pattern '^  validation:'`: found exactly one current `implementation.validation` key.

## Immediate Next Stage

test-spec review for the visual-evidence gate removal amendment.

This direct plan-review request is isolated; it does not automatically continue to test-spec review.
