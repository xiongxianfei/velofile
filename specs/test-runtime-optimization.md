# Test Runtime Optimization

## Status

approved

## Related proposal

- [Test Runtime Optimization Proposal](../docs/proposals/2026-05-16-test-runtime-optimization.md)

## Goal and context

This spec defines the validation-tier and corpus test-runtime contract for VeloFile. It governs how tests are categorized, how contributors select fast versus expensive validation paths, how corpus script smoke checks remain credible without forcing every assertion through a scratch publish, and how runtime evidence is recorded.

The goal is to improve local feedback speed without weakening release-readiness evidence. Production VeloFile behavior is out of scope; this spec affects test behavior, validation commands, test evidence, and contributor-facing validation workflow.

The current measured problem is that `VeloFile.Corpus.Tests` passed 37 tests in about `5 m 49 s`, with the slowest tests dominated by PowerShell wrapper execution, scratch source copying, repeated `dotnet publish`, and assembly-wide test serialization.

## Glossary

- **Validation tier**: a named category of checks with a known cost and purpose, such as `Fast`, `Contract`, `Smoke`, or `ReleaseEvidence`.
- **Inner-loop validation**: focused commands a contributor runs while developing a small change before broad CI or milestone closeout.
- **Full validation**: the broad closeout path used for milestone, review, verify, or release readiness.
- **Corpus contract test**: an in-process or low-overhead test that validates corpus schemas, command decisions, reports, manifests, redaction, or release classification without launching a public script wrapper.
- **Corpus script smoke test**: a small process/script test proving a public PowerShell wrapper still routes to the corpus tool and produces representative output.
- **Hermetic wrapper isolation test**: a scratch-publish test proving the shared corpus wrapper copies/builds under a scratch root and does not create repository-side output.
- **Prepared-tool execution**: a test harness path that builds or publishes the corpus tool once for repeated test invocations without making each assertion pay the full hermetic scratch-publish cost.
- **Prepared-tool manifest**: a small metadata file inside the prepared tool root that identifies the current test-harness setup invocation and expected corpus tool entrypoint.
- **Runtime report**: recorded before/after duration evidence for the optimized test suite, including slowest tests.
- **Public script family**: one of the maintained corpus wrapper entrypoints: `generate-corpus.ps1`, `run-compat-corpus.ps1`, `run-preview-corpus.ps1`, `run-benchmarks.ps1`, and `run-diagnostics-conformance.ps1`.

## Examples first

### Example E1: contributor runs the fast local loop

Given the solution has already been built
When a contributor changes a small contract-only area
Then they can run:

```powershell
dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
```

And the selected tests exclude corpus script smoke, benchmark, release-evidence, visual, and manual-evidence tests unless those tests are also explicitly selected.

### Example E2: corpus contract checks avoid PowerShell wrapper cost

Given a corpus report-shape assertion only needs generated JSON decisions
When the test runs under the `Contract` category
Then it does not launch a public PowerShell wrapper
And it does not perform a scratch source copy and `dotnet publish` for that assertion.

### Example E3: public wrappers keep representative smoke coverage

Given the public script `run-preview-corpus.ps1` remains supported
When the `CorpusScript` and `Smoke` tier is run
Then a minimal smoke test proves the wrapper route works and emits representative preview output
And full preview matrix coverage remains in `ReleaseEvidence`, not in the smoke test.

### Example E4: hermetic scratch-publish behavior remains proven

Given optimized tests use prepared-tool execution for repeated process calls
When the hermetic wrapper isolation test runs
Then it proves the shared wrapper can still publish from scratch under a scratch root
And no `bin`, `obj`, publish, report, or generated output is created under the repository as a side effect.

### Example E5: runtime evidence is recorded

Given the first optimization slice is complete
When the slice is reviewed
Then the review evidence records the baseline Corpus runtime, optimized contract runtime, optimized script-smoke runtime, top 10 slowest tests, and whether full `scripts/ci.ps1` improved, stayed the same, or regressed.

### Example E6: release evidence remains available

Given a release-readiness change needs corpus compatibility evidence
When the maintainer runs full validation or the explicit `ReleaseEvidence` tier
Then compatibility, preview, diagnostics, benchmark, and release-classification evidence remains runnable and is not hidden by fast-tier defaults.

## Requirements

### Authority and scope

R1. This spec MUST govern validation tiering, corpus test categorization, corpus wrapper smoke coverage, prepared-tool test execution, runtime reporting, and local validation command documentation.

R2. This spec MUST NOT change production VeloFile App, Core, or Windows behavior.

R3. This spec MUST NOT remove corpus validation, release-evidence validation, compatibility validation, preview validation, diagnostics validation, benchmark validation, visual evidence validation, or manual-evidence validation.

R4. `scripts/ci.ps1` MUST remain the broad validation command for the first implementation slice.

R5. CI job splitting MUST be deferred until a later accepted spec, plan, or proposal explicitly changes hosted validation behavior.

R6. Public corpus wrapper command-line contracts MUST remain backward compatible in the first implementation slice.

R7. Prepared-tool execution MUST remain internal to tests in the first implementation slice and MUST NOT introduce public wrapper options such as `-PreparedToolPath` or `-UseExistingToolBuild`.

### Category taxonomy

R8. The accepted validation category taxonomy MUST include `Fast`, `Contract`, `Smoke`, `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence`.

R9. Every test in `VeloFile.Corpus.Tests` MUST have at least one accepted category from the taxonomy.

R10. Tests MAY have multiple accepted categories when they serve multiple validation purposes.

R11. Category names in `VeloFile.Corpus.Tests` MUST be validated against the accepted taxonomy.

R12. A `VeloFile.Corpus.Tests` test marked `ReleaseEvidence` MUST NOT also be marked `Fast` unless an explicit rationale is recorded in the test or adjacent test documentation.

R13. A `VeloFile.Corpus.Tests` test marked `CorpusScript` MUST also be marked `Smoke` or `ReleaseEvidence`.

R14. The default documented fast inner-loop command MUST exclude tests categorized only as `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, or `ManualEvidence`.

R15. Category inventory validation MUST fail when a Corpus test has no category, uses an unknown category, violates the `ReleaseEvidence`/`Fast` rule, or violates the `CorpusScript` companion-category rule.

### Documented validation commands

R16. The contributor documentation for this change MUST include a fast solution-level command:

```powershell
dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
```

R17. The contributor documentation for this change MUST include a corpus contract command:

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract"
```

R18. The contributor documentation for this change MUST include a corpus script-smoke command:

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"
```

R19. The contributor documentation for this change MUST include a corpus release-evidence command:

```powershell
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"
```

R20. The contributor documentation for this change MUST continue to identify this command as full milestone closeout validation:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
```

R21. Documentation for `--no-build` commands MUST state that they assume the relevant projects have already been built.

### Corpus contract behavior

R22. Corpus contract tests SHOULD exercise corpus schemas, report shapes, manifests, sidecars, redaction, profile decisions, scope classification, and release classification without launching public PowerShell wrappers when process execution is not needed for the claim.

R23. A contract test MUST NOT be classified as `CorpusScript` only because it validates output that can be produced or inspected without a public wrapper.

R24. Moving a check from script-wrapper execution to in-process or prepared-tool execution MUST preserve the same observable contract claim or replace it with separate smoke coverage for the public wrapper.

R25. In-process or prepared-tool corpus tests MUST write outputs only under their assigned scratch/temp root.

R26. In-process or prepared-tool corpus tests MUST NOT write build output, generated corpus data, reports, diagnostics, or benchmark artifacts under the repository unless a test explicitly uses a repository fixture path that is already tracked.

### Corpus script smoke and hermetic coverage

R27. The first implementation slice MUST retain one common hermetic wrapper isolation test for the shared scratch copy/publish path.

R28. The hermetic wrapper isolation test MUST prove scratch-root resolution, scratch source copy/publish behavior, and absence of repository-side generated output.

R29. Each public script family in scope MUST have at least one minimal `CorpusScript` + `Smoke` test.

R30. Public script smoke tests MUST prove entrypoint routing and representative output only; they MUST NOT run full profile or scope matrices unless the test is explicitly categorized as `ReleaseEvidence`.

R31. Full profile and scope matrix validation MUST move to or remain in `ReleaseEvidence`.

R32. Script smoke tests MUST preserve public wrapper confidence for `generate-corpus.ps1`, `run-compat-corpus.ps1`, `run-preview-corpus.ps1`, `run-benchmarks.ps1`, and `run-diagnostics-conformance.ps1` when those script families remain in scope.

R33. The optimization MUST NOT hide a public wrapper failure by replacing all script coverage with in-process coverage.

### Prepared-tool execution

R34. Prepared-tool execution MAY be used by tests that need process execution but do not need to prove scratch source copy/publish behavior on every assertion.

R35. Prepared-tool execution MUST be isolated to a test-owned scratch/temp root.

R36. Prepared-tool execution MUST NOT mutate user PATH, global .NET tool paths, user profile environment, or repository build output outside its assigned scratch/temp root.

R37. Prepared-tool execution MUST reject prepared tool paths that are missing, outside the allowed scratch/temp root, missing the prepared-tool manifest, associated with a different test-harness setup invocation, declaring the wrong tool kind/configuration/target framework/entrypoint, or missing the expected tool artifact. Rejection MUST happen before running the tool and MUST produce an actionable diagnostic.

R38. Prepared-tool execution MUST NOT be exposed as a public corpus wrapper option in the first implementation slice.

For the first implementation slice, a prepared tool is current only when all of these conditions hold:

- the prepared tool root is inside the allowed scratch/temp root;
- the prepared tool root contains a prepared-tool manifest created by the current test-harness setup invocation;
- the manifest declares the expected tool kind, configuration, target framework, and entrypoint;
- the expected tool artifact exists;
- the prepared tool root has not been moved or resolved outside the approved scratch/temp root.

A prepared tool is stale or invalid when any of these conditions is false. Source-hash or cross-run cache staleness detection is deferred until a later slice unless cross-run prepared-tool reuse is introduced.

### Release evidence and full validation

R39. `ReleaseEvidence` tests MUST remain runnable through an explicit documented command.

R40. Tiering MAY make `ReleaseEvidence` opt-in for local inner-loop work, but `ReleaseEvidence` MUST remain part of milestone closeout, release readiness, or an explicitly documented full validation command.

R41. Benchmark-related tests that generate or validate performance evidence MUST be categorized as `Benchmark`, `ReleaseEvidence`, or both according to their purpose.

R42. Visual evidence and manual evidence checks MUST remain excluded from fast defaults unless they also satisfy the accepted `Fast` or `Contract` purpose with an explicit rationale.

R43. Full validation MUST still be able to detect release-evidence regressions that fast validation intentionally skips.

### Parallelism and shared state

R44. The first implementation slice MUST NOT remove assembly-wide `DoNotParallelize` unless a reviewed plan expands scope to include parallel-safety proof.

R45. The first implementation slice MUST identify tests that mutate process/global state, shared environment variables, shared scratch paths, user profile state, or public script state.

R46. Removing assembly-wide serialization MUST be a follow-on slice that records parallel-safety evidence and uses method/class-level serialization for shared-state tests.

R47. Tests that can run independently SHOULD use unique temp roots so later parallelization is safe.

### Runtime reporting

R48. The first implementation slice MUST record baseline Corpus test runtime before optimization.

R49. The first implementation slice MUST record optimized fast/contract runtime after optimization.

R50. The first implementation slice MUST record optimized script-smoke runtime after optimization.

R51. The first implementation slice MUST record the top 10 slowest tests after optimization.

R52. The first implementation slice MUST record whether full `scripts/ci.ps1` improved, stayed the same, or regressed.

R53. Runtime reports MUST identify the command, configuration, filter, date, and local machine/environment assumptions that materially affect interpretation.

R54. Runtime reports MUST NOT present one local machine's duration as a universal guarantee.

R55. Runtime reports SHOULD use TRX or similarly structured test output when available so slow tests are identified by test name rather than guessed from wall time.

### Performance expectations

R56. The optimized focused contract run SHOULD target completion under 10 seconds when projects are already built.

R57. The optimized Corpus fast/contract run SHOULD target completion under 30 seconds when projects are already built.

R58. The optimized Corpus script-smoke run SHOULD be materially lower than the measured multi-minute Corpus runtime.

R59. Full local CI SHOULD be no worse than the pre-optimization baseline in the first implementation slice.

R60. Missing a SHOULD-level runtime target MUST be recorded with measured evidence and follow-up rationale, but it MUST NOT by itself justify deleting coverage.

## Inputs and outputs

Inputs:

- MSTest category metadata on tests.
- Contributor validation commands and filters.
- Public corpus script invocations.
- Test-owned scratch/temp roots.
- Prepared-tool test harness inputs.
- Prepared-tool manifest values such as setup identifier, tool kind, configuration, target framework, and entrypoint.
- TRX or equivalent structured test output.

Outputs:

- Passing or failing category inventory validation.
- Passing or failing fast/contract, script-smoke, release-evidence, and full validation commands.
- Runtime report evidence with baseline, optimized durations, top slow tests, and full CI status.
- No repository-side generated output from optimized corpus tests unless explicitly tracked fixtures are used.

## State and invariants

- Corpus validation remains available after tiering.
- Public script wrapper behavior remains covered by representative smoke tests.
- Release-evidence validation remains available and required for closeout/full validation.
- Prepared-tool execution remains test-internal in the first implementation slice.
- Fast defaults exclude expensive evidence tiers unless explicitly selected.
- Production VeloFile behavior remains unchanged.
- Category names remain controlled by the accepted taxonomy.

## Error and boundary behavior

- Unknown Corpus test categories MUST fail inventory validation.
- Missing Corpus test categories MUST fail inventory validation.
- Invalid `ReleaseEvidence` + `Fast` combinations MUST fail inventory validation unless an explicit rationale is recorded.
- `CorpusScript` tests without `Smoke` or `ReleaseEvidence` MUST fail inventory validation.
- Prepared-tool execution outside the allowed scratch/temp root MUST fail.
- Prepared-tool execution with a missing root, missing manifest, mismatched setup identifier, wrong declared tool kind/configuration/target framework/entrypoint, or missing expected artifact MUST fail before tool invocation with an actionable diagnostic.
- Public script smoke failures MUST be reported as public wrapper failures, not hidden as in-process contract failures.
- If runtime reporting cannot parse structured output, the evidence MUST record the fallback method and limitation.
- If full `scripts/ci.ps1` cannot run in the local environment, the limitation MUST be recorded and the work MUST NOT claim full closeout validation.

## Compatibility and migration

- Existing public corpus wrapper scripts remain supported.
- Existing full validation through `scripts/ci.ps1` remains supported in the first implementation slice.
- Existing release-readiness evidence remains available.
- Existing tests may be recategorized, but recategorization MUST preserve the validation claim or provide replacement smoke/release evidence.
- CI job splitting is a deferred compatibility decision and MUST NOT happen as part of this first spec's implementation without a later accepted change.
- Rollback consists of reverting category enforcement, prepared-tool test harness use, and corpus test restructuring; production behavior is unaffected.

## Observability

- Runtime evidence MUST include command, filter, configuration, date, and measured duration.
- Runtime evidence MUST include the top 10 slowest tests after optimization.
- Review evidence MUST state whether full `scripts/ci.ps1` improved, stayed the same, regressed, or was not run.
- Diagnostics from category inventory failures SHOULD name the offending test and category issue.
- Diagnostics from prepared-tool boundary and manifest failures SHOULD name the rejected condition and, where needed, the allowed root without exposing unrelated private paths.

## Security and privacy

- Test runtime reports MUST NOT include secrets, tokens, credentials, signing material, or unrelated user profile data.
- Scratch roots and runtime reports SHOULD avoid publishing real private local paths when a sanitized path or relative path is enough.
- Prepared-tool manifests MUST NOT record raw local usernames, private profile paths, secrets, tokens, credentials, or machine-specific private data.
- Prepared-tool execution MUST NOT mutate user PATH, global .NET configuration, or user profile state.
- Public script smoke and hermetic wrapper tests MUST preserve existing scratch-root safety expectations.
- Generated corpus data and reports MUST remain under test-owned scratch/temp roots unless explicitly tracked fixtures are used.

## Accessibility and UX

This spec does not change product UI or accessibility behavior. Contributor-facing documentation SHOULD make command purpose clear enough that maintainers can choose the intended validation tier without guessing whether release evidence was included.

## Performance expectations

The first implementation slice should use the measured `5 m 49 s` Corpus-only run as the baseline reference unless a fresher pre-change baseline is recorded before implementation. Runtime targets in R56-R59 are review targets, not universal guarantees. The required behavior is measured improvement evidence and preserved validation credibility.

## Edge cases

E1. A test validates release report shape but does not need a public script wrapper.

- Expected: categorize as `Contract` and, if it contributes release readiness, also `ReleaseEvidence`; do not make it `CorpusScript` unless it launches the wrapper.

E2. A public script smoke test discovers a wrapper routing bug that in-process contract tests miss.

- Expected: the smoke test fails and the failure is treated as a public wrapper regression.

E3. A benchmark test is quick enough to run locally.

- Expected: speed alone does not make it `Fast`; categorize by evidence purpose as `Benchmark`, `ReleaseEvidence`, or both unless it is only a lightweight contract check.

E4. A test needs shared environment mutation.

- Expected: identify it as shared-state constrained and keep or add targeted serialization before any future parallelization slice.

E5. A contributor runs the fast command without building first.

- Expected: the command may fail due to `--no-build`; documentation must explain the prebuilt assumption.

E6. Runtime evidence improves Corpus contract time but makes full CI slower.

- Expected: record the regression and require follow-up before claiming full validation improvement.

E7. A public script family is removed or superseded by a later accepted spec.

- Expected: update the smoke coverage requirement in the later spec or plan; do not silently delete smoke coverage.

E8. A prepared-tool path points inside the repository.

- Expected: fail unless the path is an explicitly approved tracked fixture path; prepared-tool outputs must remain scratch/temp-local.

E9. A prepared-tool root contains a manifest from a previous setup invocation.

- Expected: fail before invoking the tool with a stale or setup-mismatch diagnostic.

E10. A prepared-tool root contains a current manifest but the expected entrypoint artifact is missing.

- Expected: fail before invoking the tool with an artifact-missing diagnostic.

## Non-goals

- This spec does not remove or weaken release evidence.
- This spec does not change production App, Core, Windows, preview, diagnostics, file operation, search, listing, or UI behavior.
- This spec does not split CI jobs in the first implementation slice.
- This spec does not expose prepared-tool paths as public script options in the first implementation slice.
- This spec does not remove assembly-wide `DoNotParallelize` in the first implementation slice.
- This spec does not require a full rewrite of `tools/VeloFile.Corpus` before runtime improvements can begin.
- This spec does not make local timing targets universal across all machines.

## Acceptance criteria

- AC1. The proposal status is accepted and this spec links to it.
- AC2. `VeloFile.Corpus.Tests` category inventory validation fails for missing categories, unknown categories, invalid `ReleaseEvidence` + `Fast` combinations, and invalid `CorpusScript` combinations.
- AC3. The documented fast, corpus contract, corpus script-smoke, release-evidence, and full closeout commands exist in contributor-facing guidance.
- AC4. Corpus contract tests can run without launching public PowerShell wrappers for assertions that do not need script behavior.
- AC5. A common hermetic wrapper isolation test proves scratch publish behavior and no repository-side generated output.
- AC6. Minimal script smoke coverage exists for every public corpus script family in scope.
- AC7. Full profile/scope matrix checks are categorized as `ReleaseEvidence`, not default fast/script-smoke checks.
- AC8. Given a prepared-tool path outside the allowed scratch/temp root, prepared-tool execution fails before invoking the tool and reports an outside-root diagnostic.
- AC9. Given a prepared-tool root with a missing manifest, mismatched setup identifier, wrong declared tool kind/configuration/target framework/entrypoint, or missing expected tool artifact, prepared-tool execution fails before invoking the tool and reports a controlled diagnostic such as `prepared-tool-stale`, `prepared-tool-manifest-missing`, `prepared-tool-setup-mismatch`, or `prepared-tool-artifact-missing`.
- AC10. Runtime evidence records baseline Corpus runtime, optimized contract runtime, optimized script-smoke runtime, top 10 slow tests, and full CI status.
- AC11. `scripts/ci.ps1` remains the broad closeout command for the first slice and can still be run before milestone closeout.
- AC12. No production App/Core/Windows behavior changes are required to satisfy this spec.

## Open questions

None blocking. Exact test helper names, runtime evidence file location, and later CI job split design belong in the test spec, architecture note, or execution plan.

## Next artifacts

- Architecture review for this spec and ADR 0011.
- A matching test spec mapping requirements to category inventory, wrapper smoke, prepared-tool, runtime report, and full validation tests.
- Execution planning after architecture review.

## Follow-on artifacts

None yet.

## Readiness

Approved for architecture review. The behavior contract is scoped to validation workflow and test harness behavior; implementation should not begin until architecture review, execution planning, and the matching test spec are complete.
