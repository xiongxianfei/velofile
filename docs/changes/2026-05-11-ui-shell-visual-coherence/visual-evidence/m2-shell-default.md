# M2 Shell Default Visual Evidence

## Evidence Type

failed diagnostic full-shell visual evidence

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

A maintainer-reviewed diagnostic screenshot showed the real shell and is recorded here as failed visual evidence. It is useful for diagnosing M2 defects, but it is not accepted closeout evidence.

## Observed Result

- App launched: yes, during bounded capture diagnostic
- Required profile used: no; local desktop reported `150%` scale instead of required `100%`
- Whole shell visible: yes in diagnostic screenshot
- Evidence accepted: no
- Reason: rendered shell is not visually acceptable for M2 shell surface foundation.
- Blocking observations:
  - Shell is too dark; several labels and controls have insufficient readable contrast.
  - Sidebar toggle state text renders as unexpected localized or garbled characters.
  - File-list icon chips still look like placeholder text rather than deterministic product icons.
  - Top navigation/search controls remain visually disconnected from the dark shell surface.
  - Whole-shell coherence is not yet acceptable.
- New redesigned/non-redesigned mismatch observed: not assessed
- Primary navigation visible and reachable: not assessed visually
- Path/search controls visible and reachable: not assessed visually
- File-list/content region visually integrated with shell surfaces: not assessed visually
- Sidebar visually integrated with shell surfaces: not assessed visually
- Status/preview regions visually integrated or acceptably deferred: not assessed visually

## Deviations

No deviation was recorded. The blocking observations are core M2 failures, not accepted temporary deviations. If a later reviewer observes and accepts a mismatch, it must be recorded in `docs/ui/design-deviations.md` before M2 closes.

## Screenshot / Sidecar

- Current screenshot: not captured
- Sidecar: not captured
- Reason: screenshot capture automation is not implemented for M2, the local capture attempt ran at `150%` desktop scale rather than the required `100%` profile, and the diagnostic screenshot is failed evidence rather than accepted closeout evidence.

## Conclusion

M2 shell surface foundation evidence is not accepted.

This record does not satisfy R22, R26, TSC013, or the M2 closeout rule. M2 remains `resolution-needed` until an automated/current screenshot, a manual screenshot review, or an observed manual full-shell visual-review note is recorded for `shell-default` at `shell-standard-1440x900-100`.
