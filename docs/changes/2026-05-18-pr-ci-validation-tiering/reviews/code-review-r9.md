# Code Review R9: PR CI Validation Tiering M5

## Review Status

clean-with-notes

## Reviewed Milestone

M5. Shadow-Run Evidence, Final Policy Transition, And Contributor Guidance

## Review Inputs

- Review surface: commit `91a843f` (`M5: record PR CI handoff evidence`)
- Plan milestone: `docs/plans/2026-05-18-pr-ci-validation-tiering.md` M5
- Feature spec: `specs/pr-ci-validation-tiering.md` R11-R13 and R49-R53
- Test spec: `specs/pr-ci-validation-tiering.test.md` PRCI-T027, PRCI-T028, PRCI-T029, PRCI-T033, PRCI-M001, and PRCI-M003
- Prior blocked review: `docs/changes/2026-05-18-pr-ci-validation-tiering/reviews/code-review-r8.md`
- Validation evidence reviewed:
  - Hosted PR #4 run `26062568345`
  - `gh api repos/xiongxianfei/velofile/branches/main/protection --jq '{required_status_checks: .required_status_checks.contexts, checks: .required_status_checks.checks}'`
  - `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"`
  - `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout" README.md docs specs .github`
  - `git diff --check -- .github\workflows README.md CONTRIBUTING.md docs\project-map.md docs\changes\2026-05-18-pr-ci-validation-tiering docs\plans\2026-05-18-pr-ci-validation-tiering.md tests\VeloFile.Corpus.Tests`

## Diff Summary

M5 records the accepted hosted PR shadow cycle in `shadow-run.md`: PR #4 run `26062568345` passed `ci-fast-required` in 7m20s and broad `ci` in 16m01s at commit `28de2d60faaa7fc2fbf0f3eade53f8467c26ff1a`. It records branch-protection handoff status separately in `branch-protection-handoff.md`; GitHub returned `Branch not protected` (HTTP 404), so no maintainer handoff or external required-check change is claimed.

The diff adds rollout evidence tests, extends contributor guidance tests, updates `README.md` and `CONTRIBUTING.md` with lane names and release-readiness/rollback wording, refreshes the project map for the implemented workflow topology, and moves the plan/change metadata from blocked to M5 review-requested.

## Findings

No blocking or required-change findings.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | The diff satisfies R49-R50/AC13 by recording hosted shadow-run runtime, selected categories, failures, and broad-check result. It satisfies R13/R51 by naming `ci-fast-required` as intended while recording no branch-protection handoff claim. |
| Test coverage | pass | `CiRolloutEvidenceTests` prove the shadow-run evidence, branch-protection no-handoff record, and rollout guidance wording. `ValidationCommandDocumentationTests` now covers hosted CI lane guidance in README and CONTRIBUTING. |
| Edge cases | pass | EC8 is covered by keeping the broad PR job during rollout and recording HTTP 404 branch-protection status. Release readiness and rollback wording cover R52-R53 and PRCI-M003. |
| Error handling | pass | No runtime failure semantics or workflow commands changed in M5. The evidence records earlier failed hosted attempts rather than hiding them. |
| Architecture boundaries | pass | The separate workflow topology from ADR 0012 remains unchanged; M5 only records evidence and guidance. |
| Compatibility | pass | `scripts/ci.ps1`, fast-lane command selection, release-evidence policy, test categories, and production App/Core/Windows/Corpus behavior are unchanged. |
| Security/privacy | pass | The new evidence records public PR/run IDs, commit SHAs, workflow names, durations, and branch-protection status only; no secrets or private local paths are added. |
| Derived artifact currency | pass | Plan index, active plan, change metadata, project map, and change notes are synchronized to M5 review-requested state before this review. |
| Unrelated changes | pass | The diff is scoped to M5 evidence, guidance, workflow map metadata, lifecycle records, and tests. |
| Validation evidence | pass | Implementation validation passed the focused workflow/documentation/rollout selector with 20 tests, the guidance scan, and diff check. Reviewer reran the focused selector and `git diff --check HEAD~1..HEAD`. |

## Validation Evidence Reviewed

- Hosted evidence: PR #4 run `26062568345` completed successfully; `ci-fast-required` passed in 7m20s and broad `ci` passed in 16m01s.
- Branch-protection evidence: GitHub API returned `Branch not protected` (HTTP 404), so no external required-check handoff is claimed.
- Implementation evidence: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"` passed with 20 tests.
- Implementation evidence: `rg -n "ci-fast-required|ci-release-evidence|ci-full-closeout|ReleaseEvidence: not run in this lane|Full closeout" README.md docs specs .github` found expected matches.
- Implementation evidence: `git diff --check -- .github\workflows README.md CONTRIBUTING.md docs\project-map.md docs\changes\2026-05-18-pr-ci-validation-tiering docs\plans\2026-05-18-pr-ci-validation-tiering.md tests\VeloFile.Corpus.Tests` passed with Git LF-to-CRLF working-copy warnings only.
- Reviewer rerun: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "FullyQualifiedName~CiWorkflowContract|FullyQualifiedName~ValidationCommandDocumentation|FullyQualifiedName~CiRolloutEvidence"` passed with 20 tests.
- Reviewer rerun: `git diff --check HEAD~1..HEAD` passed with no output.

## No-Finding Rationale

PRCI-CR4 is resolved because the hosted shadow-run evidence now exists and is recorded with the required fast-lane and broad-check data. The branch-protection handoff record avoids the overclaim case by explicitly saying branch protection is not configured and no maintainer handoff is recorded. The temporary broad PR job remains available for rollback, and release readiness continues to point to release-evidence, full closeout, local `scripts/ci.ps1`, or another accepted release gate.

## Residual Risks

- The latest hosted PR run for commit `91a843f` was still in progress during this review. This review does not claim latest CI success, branch readiness, PR readiness, or final verification.
- Branch-protection handoff remains external maintainer work and is not completed by repository files.

## Recommended Next Stage

Close M5 and proceed to lifecycle closeout, starting with `explain-change`. Do not claim final verification or PR readiness until the downstream lifecycle stages provide their own evidence.
