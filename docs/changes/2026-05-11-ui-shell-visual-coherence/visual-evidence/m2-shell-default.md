# M2 Shell Default Visual Evidence

## Evidence Type

accepted manual full-shell visual review

## Profile

- Screen: `shell-default`
- Profile: `shell-standard-1440x900-100`
- Effective window size: `1440x900`
- Scale: `100%`
- Theme: dark
- Density: comfortable
- Fixture/state: default shell
- Date: 2026-05-16
- Reviewer: maintainer manual review reported in chat
- Review ID: `CR-M2-002`

## Launch / Review Method

The maintainer performed a manual full-shell review outside this Codex tool session and reported that the current M2 shell is acceptable. Per maintainer instruction, this manual review is recorded as the accepted `shell-default` evidence for `shell-standard-1440x900-100`.

This Codex session did not directly observe the desktop. The accepted visual result is therefore based on maintainer-provided manual evidence, which is allowed by the M2 closeout rule when screenshot automation is not stable or unavailable.

Prior invalid/failed evidence is retained as background only: this repository has `scripts/update-ui-baselines.ps1`, which copies already-reviewed current screenshots into baselines, but no checked-in script that launches the WinUI app and captures `shell-default` current screenshots. A bounded local capture attempt launched `src\VeloFile.App\bin\Debug\net8.0-windows10.0.19041.0\VeloFile.App.exe` and confirmed that the app exposes a visible `VeloFile` window, but that attempt did not produce accepted evidence because the local desktop reported `150%` scale instead of the required `100%` profile and an early capture targeted the foreground browser rather than the VeloFile window.

A later maintainer-reviewed diagnostic screenshot was recorded as failed visual evidence and drove M2 readability, toggle, icon, command-surface, and titlebar fixes. The current maintainer manual review supersedes that failed diagnostic result for M2 closeout.

## Observed Result

- App launched: yes, per maintainer manual review
- Required profile used: yes, accepted by maintainer for `shell-standard-1440x900-100`
- Whole shell visible: yes
- Evidence accepted: yes
- Reason: maintainer manually reviewed the current shell and reported that it is okay for M2 closeout.
- Prior blocking observations: addressed sufficiently for M2 shell surface foundation.
- New redesigned/non-redesigned mismatch observed: no accepted blocking mismatch reported
- Primary navigation visible and reachable: yes, per maintainer manual review
- Path/search controls visible and reachable: yes, per maintainer manual review
- File-list/content region visually integrated with shell surfaces: yes for M2 foundation scope, per maintainer manual review
- Sidebar visually integrated with shell surfaces: yes for M2 foundation scope, per maintainer manual review
- Status/preview regions visually integrated or acceptably deferred: yes for M2 foundation scope, per maintainer manual review

## Deviations

No accepted M2 mismatch requiring a deviation was reported by the maintainer manual review.

## Screenshot / Sidecar

- Current screenshot: not captured
- Sidecar: not captured
- Reason: screenshot capture automation is not implemented for M2; accepted soft evidence is maintainer manual full-shell review.

## Conclusion

M2 shell surface foundation visual evidence is accepted.

This record satisfies the M2 full-shell visual-evidence closeout rule through maintainer-provided manual review. M2 may return to code review.
