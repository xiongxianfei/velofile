# PR CI Validation Tiering Shadow Run

Date recorded: 2026-05-18

## Accepted shadow cycle

- Pull request: https://github.com/xiongxianfei/velofile/pull/4
- Run: https://github.com/xiongxianfei/velofile/actions/runs/26062568345
- Workflow: `ci`
- Event: `pull_request`
- Commit: `28de2d60faaa7fc2fbf0f3eade53f8467c26ff1a`
- Overall result: passed

## Lane comparison

| Lane | Result | Runtime | Purpose |
|---|---:|---:|---|
| `ci-fast-required` | passed | 7m20s | Fast PR confidence. |
| `ci` | passed | 16m01s | Temporary broad PR shadow check through `scripts/ci.ps1`. |

## Fast-lane command evidence

- Selected categories: `Fast|Contract`; `CorpusScript&Smoke`
- ReleaseEvidence: not run in this lane
- CorpusScript Smoke: run
- Full closeout: not run
- Core tests: passed
- App tests: passed
- Windows tests: passed
- Corpus fast/contract tests: passed
- Corpus script smoke tests: passed
- Runtime summary: passed
- TRX artifact upload: passed
- No validation failures on accepted shadow run.

## Broad-check evidence

- Broad check: passed
- Command: `./scripts/ci.ps1`
- Runtime: 16m01s
- Release-evidence tests remained in the broad closeout path.

## Earlier shadow attempts

- Run `26060670719` failed because workflow summary calls used positional array splatting and hosted `pwsh` mis-bound `-ReleaseEvidenceStatus` as `TotalDurationSeconds`; broad `ci` also exposed a timeout-sensitive App preview assertion.
- Run `26061628483` passed `ci-fast-required` in 6m20s after summary wiring was fixed, but broad `ci` exposed a timeout-sensitive recursive-search assertion.
- Both issues were fixed before the accepted run above. They are retained here as rollout evidence and are not counted as the accepted clean shadow cycle.

## Limitations

- The accepted run is one hosted PR cycle, not a guarantee of future runtime.
- Branch protection was not changed by this repository update.
- `ci-fast-required` is fast PR confidence only; release readiness still requires release-evidence, full closeout, local `scripts/ci.ps1`, or another accepted release gate.
