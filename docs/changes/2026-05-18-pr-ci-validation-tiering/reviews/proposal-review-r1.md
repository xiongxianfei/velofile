# Proposal Review R1: PR CI Validation Tiering

## Review Status

approved

## Review Inputs

- Proposal: `docs/proposals/2026-05-18-pr-ci-validation-tiering.md`
- User review request: `$proposal-review`
- Original proposal intent: reduce required hosted PR CI runtime while preserving release evidence and full closeout validation
- Prior user review: revise, then approve; material concerns CI-PR1 through CI-PR4 and should-fix concerns CI-PR5 through CI-PR8
- `AGENTS.md`
- `CONSTITUTION.md`
- `VISION.md`

## Findings

None.

## Review Dimensions

| Dimension | Result | Notes |
|---|---|---|
| Problem clarity | pass | The proposal names the actual operational problem: ordinary PRs pay the broad closeout/release-evidence cost, dominated by `VeloFile.Corpus.Tests`, rather than just asking for CI speedups. |
| User value | pass | Faster required PR feedback, preserved release evidence, stable closeout commands, and runtime visibility are concrete contributor and maintainer benefits. |
| Option diversity | pass | The proposal compares doing nothing, deleting slow tests, introducing validation tiers, and relying on caching. |
| Decision rationale | pass | Option C follows from the evidence that Corpus release-evidence tests dominate runtime while release confidence still needs to remain available. |
| Scope control | pass | Non-goals protect production behavior, release-evidence tests, public prepared-tool options, serialization policy, caching scope, and fast-lane visual gating. |
| Architecture awareness | pass | The proposal limits expected changes to workflow files, validation scripts/reporting, category contract tests, and contributor guidance, with no App/Core/Windows production behavior change. |
| Testability | pass | The proposed workflow contract tests cover fast lane existence, restore/build ordering, UI contracts, unfiltered Core/App/Windows tests, Corpus filters, release-evidence exclusion, and separate evidence/closeout workflow availability. |
| Risk honesty | pass | The proposal names the key risks: missed release-evidence regressions, contributor confusion about release readiness, wrapper coverage gaps, workflow-test inconsistency, cache masking, and ignored release-evidence lanes. |
| Rollout realism | pass | Shadow-running the fast lane, comparing runtime/failures, then changing branch protection is realistic and reversible. |
| Readiness for spec | pass | Remaining details such as cron timing and optional maintainer-triggered PR evidence are small enough for the spec or execution plan. |

## Vision Fit Review

pass

The proposal's `Vision fit` section uses the allowed value `fits the current vision`, and root `VISION.md` exists. The direction supports maintainable open-source workflow and does not expand product scope.

## Standing Artifact Gate Review

pass

`VISION.md` and `CONSTITUTION.md` exist. The proposal changes validation policy, so a proposal-review gate is appropriate before spec authoring. No bootstrap exception is needed.

## Scope Preservation Review

pass

The proposal includes `Initial intent preservation` and classifies the initial goals, explicit exclusions, resolved decisions, and former open questions. No initial user goal disappeared.

## Scope Budget Review

pass

The proposal includes a scope budget and classifies core policy work, same-slice dependencies, separate implementation slices, deferable caching, and out-of-scope public prepared-tool and visual-gating work. The branch-protection setting is correctly called out as potentially requiring a maintainer action outside repository file changes.

## Suggested Proposal Edits

None.

## Blocking Questions

None.

## Immediate Next Stage

`spec`

## Isolation

This review is isolated. It approves the proposal direction for spec authoring but does not automatically start the spec stage.
