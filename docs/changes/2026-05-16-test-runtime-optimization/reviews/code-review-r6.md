# Code Review R6: M4 Prepared Tool Harness Re-Review

## Review status

clean-with-notes

## Review inputs

- Diff range: `90b3a8d..8b4391b`
- Review surface: tracked commit `8b4391b M4: Resolve prepared tool repo output isolation`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/architecture/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M4
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M4 review-resolution validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`

## Diff summary

M4 now prepares the Corpus CLI from scratch-owned inputs before using the test-internal prepared-tool path:

- `PreparedCorpusToolHarness.Prepare` copies `tools/VeloFile.Corpus`, `src/VeloFile.Core`, and `src/VeloFile.Windows` into a test scratch source tree, excluding `bin` and `obj`.
- The harness publishes from that scratch source into a prepared-tool root under the same allowed scratch root, with .NET, NuGet, and temp locations pointed at scratch-owned directories for setup.
- `PreparedToolHarnessTests` snapshot repository output before `Prepare`, prove current-run repeated prepared-tool execution, prove scratch-owned prepared source/output roots, and keep stale/missing/outside-root manifest rejection tests.
- `RepoOutputSnapshot` now includes repository `bin` and `obj` paths for the Corpus tool and referenced projects, and fingerprints file length and last-write time.
- The plan, plan index, change metadata, review-resolution record, and change rationale record TRO-CR2 resolution and M4 validation evidence.

## Findings

No blocking or required-change findings.

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | The harness satisfies R34-R38: prepared execution remains test-internal, under the allowed scratch root, guarded by a current-run manifest, and without public wrapper options. The TRO-CR2 fix also addresses R36 by publishing from scratch-owned source and proving repo output boundaries. |
| Test coverage | pass | `PreparedToolHarnessTests` directly cover TTO-T019 through TTO-T027, including missing root, outside-root, missing manifest, setup mismatch, bad metadata, missing artifact, repo/global-state safety, and public script option absence. |
| Edge cases | pass | Invalid prepared roots fail before invocation with controlled diagnostics, and the repo-output mutation oracle now has a direct mutation-detection test. |
| Error handling | pass | `PreparedCorpusToolHarness.Run` returns pre-invocation diagnostics for rejected roots/manifests/artifacts instead of starting the prepared tool. |
| Architecture boundaries | pass | The change stays inside test harness and documentation surfaces. It does not expose `-PreparedToolPath`, split CI, add cross-run caching, change public wrappers, or change production App/Core/Windows behavior. |
| Compatibility | pass | Public corpus wrappers are untouched, and the retained `CorpusScript&Smoke` validation remains recorded for M4. |
| Security/privacy | pass | Boundary diagnostics avoid repo/user-profile paths in the rejection tests, and the manifest is asserted not to contain the repo root or user profile. |
| Derived artifact currency | pass | Plan, plan index, change metadata, review-resolution, and change rationale were updated with TRO-CR2 resolution and M4 validation evidence. |
| Unrelated changes | pass | Reviewed diff is limited to Corpus test harness/tests and test-runtime optimization documentation. |
| Validation evidence | pass | The plan/change metadata record build-producing M4 commands passing for `FullyQualifiedName~PreparedTool&TestCategory=Contract`, retained `CorpusScript&Smoke`, `CategoryInventoryTests`, and `git diff --check`. |

## No-finding rationale

No blocking findings were found because the reworked harness no longer publishes from repository source, the repo-output oracle observes setup plus invocation, `bin`/`obj` paths are included in the snapshot, and direct tests prove the named prepared-tool boundary and stale-tool failure paths.

## Residual risks

- Scratch source preparation raises the focused prepared-tool test runtime to about 29 seconds locally. This is acceptable for the M4 safety boundary; M6 remains responsible for consolidated runtime reporting and final before/after evidence.
- Full `scripts/ci.ps1` was not rerun for M4 re-review. The M4 targeted validation passed, and broad closeout validation remains assigned to later milestone/final closeout scope.

## Recommended next stage

Close M4 and proceed to `implement` M5.
