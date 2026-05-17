# Code Review R8: M5 Release-Evidence Preservation Re-Review

## Review status

clean-with-notes

## Review inputs

- Diff range: `0b4e801..d87cadd`
- Review surface: tracked commit `d87cadd M5: Resolve ManualEvidence rationale proof gap`, with M5 implementation context from `code-review-r7`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/architecture/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M5
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M5 review-resolution validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`

## Diff summary

M5 review-resolution adds direct proof for the ManualEvidence rationale path:

- `CategoryInventoryTests` now directly rejects `ManualEvidence` + `Contract` and `ManualEvidence` + `Fast` descriptors when `EvidenceFastPathRationale` is missing.
- The same test file now rejects whitespace-only rationale values for both combinations.
- Non-empty rationale values are accepted for both `ManualEvidence` + `Contract` and `ManualEvidence` + `Fast`.
- The resolution, plan, plan index, change metadata, and change rationale were updated to record TRO-CR3 resolution and M5 validation evidence.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | The fix directly supports R42: visual/manual evidence checks remain excluded from fast defaults unless they satisfy `Fast` or `Contract` purpose with explicit rationale. R39-R43 remain preserved because release evidence is still selected by the explicit `ReleaseEvidence` command and full CI remains broad. |
| Test coverage | pass | `CategoryInventoryTests` now directly covers ManualEvidence + Contract/Fast missing rationale, whitespace rationale, and non-empty rationale cases. `ReleaseEvidenceTierTests.Visual_and_manual_evidence_fast_default_members_have_explicit_rationale` remains as the assembly-level drift guard. |
| Edge cases | pass | The named TRO-CR3 edge case is directly proven for both `Fast` and `Contract`, including null and whitespace rationale values. |
| Error handling | pass | The inventory validator returns actionable `category-rationale-required` diagnostics for visual/manual evidence selected by fast/default filters without a non-empty rationale. |
| Architecture boundaries | pass | The diff stays in test-runtime category inventory tests and change-record documentation. It does not change public wrappers, prepared-tool execution, CI routing, or production App/Core/Windows behavior. |
| Compatibility | pass | Public corpus scripts and release-evidence commands are unchanged; the explicit `ReleaseEvidence` validation command remains recorded as passing. |
| Security/privacy | pass | No secrets, private paths, or new runtime diagnostics are introduced. The added rationale strings are static test text. |
| Derived artifact currency | pass | `review-resolution.md`, `change.yaml`, the active plan, plan index, and change rationale were updated with TRO-CR3 disposition and validation evidence. |
| Unrelated changes | pass | Reviewed diff is limited to M5 category-inventory proof and test-runtime optimization records. |
| Validation evidence | pass | Recorded M5 review-resolution validation includes the focused ReleaseEvidenceTier/CategoryInventory run, explicit `ReleaseEvidence` command, build-producing fast/contract command, timing `--no-build` run, full `scripts\ci.ps1`, and `git diff --check`. |

## No-finding rationale

No blocking findings were found because the review-resolution adds targeted tests for the previously missing ManualEvidence branch, uses the same `CorpusCategoryInventory.Validate(...)` path as real inventory validation, preserves the release-evidence and broad CI boundaries, and records current validation evidence.

## Residual risks

- Release-evidence and full CI runs remain intentionally slow. M6 is still responsible for consolidated runtime reporting and slow-test evidence.

## Recommended next stage

Close M5 and proceed to `implement` M6.
