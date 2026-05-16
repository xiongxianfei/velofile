# M3 shell-file-list-selected-focused visual evidence

## Evidence type

deferred full-shell visual review

## Profile

- Screen: shell-file-list-selected-focused
- Profile: shell-standard-1440x900-100
- Effective window size: 1440x900
- Scale: 100%
- Theme: dark
- Density: comfortable
- Fixture/state: file-list-v1 with selected/focused row
- Date: 2026-05-16
- Reviewer: deferred by maintainer request

## Implementation status

M3 implementation added deterministic file-list icon resources, allowlisted icon kinds, file-list row template binding, and a tokenized details header. Static contract validation and focused behavior-preservation tests passed.

## Observed result

- Whole shell visible: not recorded
- Required profile used: not recorded
- Evidence accepted: deferred, not accepted
- Reason: maintainer requested relaxing the M3 closeout gate so M3 code review can proceed on static icon/resource contracts and behavior-preservation tests. No whole-shell visual acceptance is claimed for M3.

## Deviations

- None recorded.

## Screenshot / sidecar

- Current screenshot: not captured
- Sidecar: not captured
- Reason if not captured: M3 visual evidence is deferred to M8 by the 2026-05-17 spec/plan amendment.

## Conclusion

M3 implementation may proceed to code review under the approved visual-evidence deferral. The deferred `shell-file-list-selected-focused` state remains required for M8 evidence consolidation and final closeout.
