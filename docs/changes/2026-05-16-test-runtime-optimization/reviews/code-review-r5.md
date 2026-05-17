# Code Review R5: M4 Test-Internal Prepared Tool Harness

## Review status

changes-requested

## Review inputs

- Diff range: `90b3a8d..9a0256f`
- Review surface: tracked commit `9a0256f M4: Add test-internal prepared corpus tool harness`
- Tracked governing branch state: clean worktree before review; proposal/spec/test spec/plan/ADR tracked in branch
- Spec: `specs/test-runtime-optimization.md`
- Test spec: `specs/test-runtime-optimization.test.md`
- Plan milestone: `docs/plans/2026-05-16-test-runtime-optimization.md`, M4
- Architecture / ADR: `docs/architecture/system/architecture.md`, `docs/adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md`
- Validation evidence: M4 validation notes in the active plan and `docs/changes/2026-05-16-test-runtime-optimization/change.yaml`

## Diff summary

M4 adds a test-internal prepared Corpus tool harness and focused tests:

- `PreparedCorpusToolHarness` publishes `tools/VeloFile.Corpus` to a prepared root under a test scratch root, writes `.velofile-prepared-tool.json`, validates manifest metadata before invocation, and runs the prepared `VeloFile.Corpus.dll`.
- `PreparedToolHarnessTests` cover current-run execution, missing root, outside-root path, missing manifest, mismatched setup id, invalid metadata, missing artifact, global-state/repo-output safety, and public script option absence.
- The plan, plan index, change metadata, and change rationale record M4 validation evidence and set M4 to review-requested.

## Findings

### TRO-CR2: Prepared-tool publish can mutate repository `bin`/`obj` outputs

Severity: major

Location: `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedCorpusToolHarness.cs:62`; `tests/VeloFile.Corpus.Tests/TestRuntime/PreparedToolHarnessTests.cs:124`; `tests/VeloFile.Corpus.Tests/TestRuntime/RepoOutputSnapshot.cs:7`

Evidence:

- Spec R36 requires prepared-tool execution not to mutate repository build output outside the assigned scratch/temp root: `specs/test-runtime-optimization.md:183`.
- Test spec TTO-T026 requires the prepared-tool path to compare repo output locations and prove no global/repo output mutation: `specs/test-runtime-optimization.test.md:337`.
- `PreparedCorpusToolHarness.Prepare` runs `dotnet publish` directly against the repository project path `tools/VeloFile.Corpus/VeloFile.Corpus.csproj` and only redirects the publish output with `-o <preparedRoot>`. Without redirecting MSBuild intermediate/output paths or using a scratch source copy, `dotnet publish` can still write normal project `bin`/`obj` intermediates under the repository.
- The safety test calls `PreparedCorpusToolHarness.Prepare(context)` before taking `beforeRepoOutputs`, so any publish-time repo mutation is invisible to the assertion.
- `RepoOutputSnapshot.CaptureGeneratedOutputPaths` snapshots `.velofile-*`, top-level generated corpus/report folders, and `publish` directories, but it does not include repository `bin` or `obj` paths for `tools/VeloFile.Corpus`, `src/VeloFile.Core`, or `src/VeloFile.Windows`.

Required outcome:

- Prepared-tool setup must not write repository build outputs outside the assigned scratch/temp root, and TTO-T026 must directly prove that setup plus invocation preserve repo-side build output boundaries.

Safe resolution path:

- Keep the fix scoped to M4 test harness and tests.
- Either prepare the tool from a scratch source copy, or pass MSBuild properties that redirect `BaseIntermediateOutputPath`, `IntermediateOutputPath`, `OutputPath`, and related publish/build outputs for the Corpus project and its project references into the test-owned scratch root.
- Move the repo-output snapshot in `PreparedTool_execution_does_not_mutate_global_state_or_repo_outputs` so it is captured before `PreparedCorpusToolHarness.Prepare(context)`.
- Expand repo-output snapshot coverage to include the relevant repository `bin` and `obj` paths or add a targeted assertion proving their timestamps/contents do not change during prepared-tool setup and invocation.
- Rerun M4 validation:
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~PreparedTool&TestCategory=Contract"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CategoryInventoryTests"`
  - `git diff --check`

## Checklist coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | block | TRO-CR2 conflicts with R36's no repository build output mutation requirement for prepared-tool execution. |
| Test coverage | block | TTO-T026 is not directly proven because the repo-output snapshot is taken after setup and excludes repo `bin`/`obj` paths. |
| Edge cases | concern | Stale/missing/outside-root manifest cases are covered, but setup-time repo-output mutation is not covered. |
| Error handling | pass | Missing root, outside-root, missing manifest, setup mismatch, bad metadata, and missing artifact return controlled pre-invocation diagnostics. |
| Architecture boundaries | concern | The prepared-tool path remains test-internal, but publishing from the repository project path risks crossing the scratch-root boundary. |
| Compatibility | pass | No public prepared-tool script option is added, and public wrapper smoke validation remains recorded. |
| Security/privacy | pass | Boundary rejection diagnostics avoid repo/user-profile paths in tests, and manifests do not store local private paths. |
| Derived artifact currency | pass | Plan, plan index, change metadata, and rationale were updated for M4 review-requested state. |
| Unrelated changes | pass | Reviewed diff is limited to Corpus test harness/tests and test-runtime optimization documentation. |
| Validation evidence | concern | Recorded M4 commands are relevant, but they do not catch TRO-CR2 because the repo-output oracle is incomplete. |

## No-finding rationale

Not applicable. A required-change finding is present.

## Residual risks

- Full `scripts/ci.ps1` was not rerun for M4. This is not the blocking finding; the blocker is the targeted M4 repo-output boundary gap.

## Recommended next stage

Enter `review-resolution` for TRO-CR2. Keep M4 in `resolution-needed` until the prepared-tool setup path and repo-output oracle are corrected and re-reviewed.
