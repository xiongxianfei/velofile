# M2 Shell Default Visual Evidence

## Evidence Type

unavailable full-shell visual evidence after failed capture attempt

## Profile

- Screen: `shell-default`
- Profile: `shell-standard-1440x900-100`
- Effective window size: `1440x900`
- Scale: `100%`
- Theme: dark
- Density: comfortable
- Fixture/state: default shell
- Date: 2026-05-11
- Reviewer: Codex
- Review ID: `CR-M2-002`

## Launch / Review Method

No accepted full-shell visual review was performed in this environment.

The repository currently has `scripts/update-ui-baselines.ps1`, which copies already-reviewed current screenshots into baselines, but no checked-in script that launches the WinUI app and captures `shell-default` current screenshots. This tool session also cannot directly observe the local WinUI desktop, so it cannot truthfully record a manual visual review result.

A later bounded local capture attempt launched `src\VeloFile.App\bin\Debug\net8.0-windows10.0.19041.0\VeloFile.App.exe` and confirmed that the app exposes a visible `VeloFile` window. The attempted screenshot path did not produce valid evidence because the local desktop reported `150%` scale instead of the required `100%` profile, and the first capture targeted the foreground browser rather than the VeloFile window. The invalid generated screenshot and sidecar outputs were discarded.

## Observed Result

- App launched: yes, during bounded capture diagnostic
- Required profile used: no; local desktop reported `150%` scale instead of required `100%`
- Whole shell visible: not accepted as observed evidence
- New redesigned/non-redesigned mismatch observed: not assessed
- Primary navigation visible and reachable: not assessed visually
- Path/search controls visible and reachable: not assessed visually
- File-list/content region visually integrated with shell surfaces: not assessed visually
- Sidebar visually integrated with shell surfaces: not assessed visually
- Status/preview regions visually integrated or acceptably deferred: not assessed visually

## Deviations

No deviation was recorded because no visual review was performed. If a later reviewer observes and accepts a mismatch, it must be recorded in `docs/ui/design-deviations.md` before M2 closes.

## Screenshot / Sidecar

- Current screenshot: not captured
- Sidecar: not captured
- Reason: screenshot capture automation is not implemented for M2, the local capture attempt ran at `150%` desktop scale rather than the required `100%` profile, and this tool session cannot truthfully record a manual full-shell visual-review result.

## Conclusion

M2 shell surface foundation evidence is not accepted.

This record does not satisfy R22, R26, TSC013, or the M2 closeout rule. M2 remains `resolution-needed` until an automated/current screenshot, a manual screenshot review, or an observed manual full-shell visual-review note is recorded for `shell-default` at `shell-standard-1440x900-100`.
