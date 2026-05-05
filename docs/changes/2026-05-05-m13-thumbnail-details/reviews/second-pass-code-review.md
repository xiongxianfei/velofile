# M13 Second-Pass Code Review

## Review Status

changes-requested

## Review Inputs

- Diff range: `9520e39 M13: resolve thumbnail timeout and dispatcher review findings`
- Review surface: committed M13 review-resolution diff
- Tracked governing branch state: V1 spec, V1 test spec, M13 plan, and M13 change evidence are tracked
- Spec: `specs/v1-product-scope.md` R67
- Test spec: `specs/v1-product-scope.test.md` T032
- Plan milestone: `docs/plans/2026-05-04-v1-product-scope.md` M13
- Architecture: `docs/architecture/system/architecture.md` preview/provider boundaries
- Validation evidence: `docs/changes/2026-05-05-m13-thumbnail-details/review-resolution.md`

## Diff Summary

The reviewed diff added visible thumbnail timeouts for non-cooperative providers and dispatcher-marshaled App row updates. The Core controller still allocated the thumbnail provider semaphore inside each generation.

## Findings

### 1. Blocker: timed-out providers can still exceed the global thumbnail concurrency cap across generations

Evidence: `src/VeloFile.Core/Preview/ThumbnailController.cs` created a new `SemaphoreSlim` inside `RunGenerationAsync`. Starting a new generation cancelled the old visible work but did not stop non-cooperative provider calls. A new generation therefore had a fresh semaphore and could start another full batch while the old timed-out provider calls were still live.

Requirement: R67 requires thumbnail generation to have no more than 4 concurrent thumbnail operations. The cap applies to live provider operation lifetime, not only to one visible generation.

Required outcome: the live thumbnail provider throttle must be controller-wide, and old timed-out provider calls must continue to count until actual provider completion, fault, or cancellation.

Safe resolution path:

- move the semaphore/gate to `ThumbnailController` scope;
- keep visible timeout cancellation separate from live-slot release;
- add a cross-generation regression proving Generation B cannot start provider work while Generation A has all slots occupied by timed-out non-cooperative providers;
- prove a future explicit generation can use a slot after old provider work actually completes.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | R67's global four-operation cap was not preserved across generations. |
| Test coverage | concern | Existing tests covered only one generation. |
| Edge cases | block | Non-cooperative old provider calls plus new generation bypassed the cap. |
| Error handling | pass | Visible timeout and late-result ignore existed within one generation. |
| Architecture boundaries | pass | Core remained UI-agnostic and App dispatcher stayed in the App layer. |
| Compatibility | concern | Fix needed to avoid changing thumbnail retry semantics beyond explicit new generations. |
| Security/privacy | pass | No sensitive logging or upload path was introduced. |
| Generated output drift | pass | No generated outputs involved. |
| Unrelated changes | pass | Reviewed commit scope was M13-only. |
| Validation evidence | concern | Validation was credible but did not cover the cross-generation live-slot case. |

## Recommended Next Stage

Enter review-resolution for the R67 blocker, rerun focused thumbnail tests, direct thumbnail corpus, solution thumbnail/preview filters, and CI.
