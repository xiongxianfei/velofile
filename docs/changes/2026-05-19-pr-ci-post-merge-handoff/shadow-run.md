# PR CI Post-Merge Hosted Confirmation

Date recorded: 2026-05-19

## Accepted post-handoff cycle

- Pull request: https://github.com/xiongxianfei/velofile/pull/5
- Run: https://github.com/xiongxianfei/velofile/actions/runs/26086191007
- Job: https://github.com/xiongxianfei/velofile/actions/runs/26086191007/job/76699802555
- Workflow: `ci`
- Event: `pull_request`
- Commit: `b29fd249df61c370dcd069edde664a4c7281cec6`
- Overall result: passed
- PR check rollup: `ci-fast-required` passed
- No broad `ci` job appeared in the accepted hosted PR run.

## Lane evidence

| Lane | Result | Runtime | Purpose |
|---|---:|---:|---|
| `ci-fast-required` | passed | 5m22s | Fast PR confidence after broad PR shadow cleanup. |

Broad closeout: not run on ordinary PR.

## Step durations

| Step | Result | Duration |
|---|---:|---:|
| Set up .NET SDK | passed | 38s |
| Show .NET SDK info | passed | 2s |
| Restore solution | passed | 43s |
| Build solution | passed | 29s |
| Validate UI token contracts | passed | 5s |
| Test Core | passed | 3s |
| Test App | passed | 6s |
| Test Windows | passed | 2s |
| Test Corpus fast and contract | passed | 1m32s |
| Test Corpus script smoke | passed | 1m28s |
| Write fast PR runtime summary | passed | 1s |
| Upload fast PR test results | passed | 1s |

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
- No validation failures on accepted post-handoff run.

## Required-check handoff evidence

- Ruleset required check: `ci-fast-required`
- Protection mechanism: repository ruleset `protect`
- Ruleset id: `16578519`
- Ruleset enforcement: `active`
- Ruleset condition: default branch (`~DEFAULT_BRANCH`)
- Classic branch-protection result: GitHub returned `Branch not protected` (HTTP 404).
- Do not claim classic GitHub branch protection is configured; the observed required-check handoff is ruleset-based.

## Earlier post-handoff attempt

- Run `26085553757` failed in `ci-fast-required` after 4m25s.
- Failure cause: fast/contract Corpus tests still contained stale broad-`ci` preservation assertions against `.github/workflows/ci.yml`.
- Resolution: tests now preserve broad closeout through `.github/workflows/closeout.yml` and `ci-full-closeout`.
- This failed run is retained as implementation evidence and is not counted as the accepted hosted confirmation cycle.

## Limitations

- The accepted run is one hosted PR cycle, not a guarantee of future runtime.
- The PR remains a draft implementation vehicle until M3 code review, explain-change, verify, and PR handoff complete.
- `ci-fast-required` is fast PR confidence only; release readiness still requires release-evidence, full closeout, local `scripts/ci.ps1`, or another accepted release gate.
