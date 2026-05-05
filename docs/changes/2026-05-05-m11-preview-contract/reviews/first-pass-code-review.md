# M11 First-Pass Code Review

## Review status

changes-requested

## Review inputs

- Diff range: `7cc1a56..957c4cf`
- Review surface: committed M11 change, 17 files
- Spec: `specs/v1-product-scope.md`
- Test spec: `specs/v1-product-scope.test.md`
- Plan milestone: M11 in `docs/plans/2026-05-04-v1-product-scope.md`
- Architecture/ADR: ADR 0004 and ADR 0008
- Validation evidence: M11 change record plus commit body; focused preview tests, preview corpus contract, build, and CI were recorded as passing

## Diff summary

M11 added `VeloFile.Core.Preview`, a metadata-only preview provider, controller orchestration for loading/timeout/stale selection, App shell preview pane wiring, redacted preview failure diagnostics, PreviewContract tests, and a preview corpus contract scope.

## Findings

1. major: Preview timeout contract cannot enforce the V1 per-provider budgets.

   R67 requires distinct budgets: image `2s`, text/encoding `1s`, PDF first page `3s`, thumbnails `500ms`, and thumbnail concurrency `4`. The M11 controller exposed and applied a single global timeout budget, so tests proved only a generic timeout race.

2. major: Non-mutation and complete metadata fallback are not directly proven, and timestamp metadata is incomplete.

   R71/R72 and T031 require direct source non-mutation proof and metadata fallback with size, timestamps, attributes, and type. M11 used synthetic listed items, modeled only last-write time, and did not compare scratch file bytes/timestamps/attributes before and after preview.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | R67 timeout policy and R71/R72 metadata/non-mutation proof were incomplete. |
| Test coverage | concern | Preview states were covered, but per-provider budgets and scratch-file non-mutation were not. |
| Edge cases | concern | Stale selection and timeout were covered; T031 direct proof was missing. |
| Error handling | pass | Controller mapped timeout/access-denied/decode errors to failed states and kept unsupported distinct. |
| Architecture boundaries | concern | Generic provider boundary existed, but timeout policy was not encoded in the contract. |
| Compatibility | pass | Existing preview corpus smoke remained and contract scope was added. |
| Security/privacy | pass | Preview diagnostics used redacted paths and tests checked no username/file name leaks. |
| Generated output drift | pass | No generated output appeared stale in the reviewed diff. |
| Unrelated changes | pass | Reviewed commit was scoped to M11; unrelated worktree changes were not included. |
| Validation evidence | pass | Relevant commands/results were recorded, but did not cover the two gaps above. |

## Recommended next stage

Enter review-resolution for the two M11 findings, then rerun `code-review`.
