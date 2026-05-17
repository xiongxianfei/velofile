# Test Spec Review R1: Visual-Evidence Gate Removal Amendment

## Verdict

revise

## Review Inputs

- Test spec: `specs/ui-shell-visual-coherence.test.md`
- Approved feature spec amendment: `specs/ui-shell-visual-coherence.md`
- Spec review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/spec-review-r2.md`
- Architecture review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/architecture-review-r1.md`
- Plan review: `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/plan-review-r4.md`
- Active plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`
- Blocking review context: CR-007 in `docs/changes/2026-05-11-ui-shell-visual-coherence/reviews/code-review-r13.md`

## Findings

### TSR-001: Test spec readiness still points to stale M1 implementation

- Severity: material
- Location: `specs/ui-shell-visual-coherence.test.md:5`, `specs/ui-shell-visual-coherence.test.md:421`, `specs/ui-shell-visual-coherence.test.md:423`, `specs/ui-shell-visual-coherence.test.md:424`, `specs/ui-shell-visual-coherence.test.md:430`, `specs/ui-shell-visual-coherence.test.md:432`
- Evidence:
  - The amended test cases themselves correctly make screenshots/manual visual notes optional: TSC013 asserts M2-M7 closeout does not require visual artifacts, TSC014 validates optional sidecars only when present, and TSC020 is optional manual review only when performed.
  - The test spec status still says the amendment is pending review.
  - The `Next Artifacts` and `Readiness` sections still direct the workflow to `implement` M1 and say M1 should begin test-first, even though M1-M3 are already closed and the current workflow needs the amendment to unblock M4 code review.
  - The active plan says the next stage is test-spec review for this amendment before returning M4 to code review.
- Required outcome:
  - Update the test spec status, next artifacts, follow-on artifacts, and readiness sections so they describe the current amendment state rather than the original M1 implementation start.
  - The revised readiness should say the amendment is ready for test-spec re-review, and once approved can unblock returning M4 to code review under the amended proof model.
  - Keep TSC013/TSC014/TSC020 semantics unchanged unless the revision reveals a new coverage issue.
- Safe resolution path:
  - Replace the stale M1 `Next Artifacts` and `Readiness` wording with amendment-specific wording.
  - Record the upstream approvals already completed: spec-review-r2, architecture-review-r1, and plan-review-r4.
  - Run `git diff --check` and a focused scan for stale `ready for implement at M1`, `M1 should begin`, and `draft amendment ... pending review` wording in the test spec.
  - Return to test-spec review.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Requirement coverage | pass | R66-R77 and AC9-AC13 are mapped to TSC013/TSC014/TSC018/TSC019/TSC020. |
| Test case clarity | pass | TSC013 clearly asserts no hidden visual-evidence gate; TSC014 and TSC019 govern optional artifacts when present. |
| Negative cases | pass | Hidden visual gate, sidecar privacy leak, generated-output mutation, and screenshot-only behavior proof are covered. |
| Automation feasibility | pass | Contract/script/app/manual levels are separated, and optional artifacts do not become mandatory automation. |
| Source alignment | concern | The proof cases align with the approved amendment, but lifecycle/readiness text still points to stale M1 work. |
| Readiness | block | The test spec cannot be the final amendment proof surface while its readiness section directs implementation to already-closed M1. |

## Immediate Next Stage

review-resolution for TSR-001. After the test spec wording is revised, rerun test-spec review before returning M4 to code review.

This direct test-spec review request is isolated; it does not automatically enter review-resolution.
