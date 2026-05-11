# M2 Shell Default Visual Evidence

## Evidence

- Milestone: M2 Shell Surface Foundation
- Screen: `shell-default`
- Profile: `shell-standard-1440x900-100`
- Evidence kind: `manual-visual-review-note`
- Theme: dark
- Density: comfortable
- Date: 2026-05-11
- Review ID: `m2-shell-surface-foundation`

## Result

Full-shell screenshot automation is not stable or implemented for this milestone, so no current PNG or sidecar baseline was captured in M2. This note records the soft visual evidence path allowed by the plan for early region slices.

The M2 slice applies the shared shell surface resources to the existing full-shell containers: app root, chrome/tab strip, command band container, sidebar, content region, status container, and preview pane. The slice does not intentionally change layout dimensions, minimum window behavior, route handlers, or information architecture.

## Static Evidence

- `App.xaml` merges `Resources/Components/VeloFile.Shell.xaml` after token dictionaries and before file-list resources.
- `MainWindow.xaml` consumes `VfShellAppRootStyle`, `VfShellChromeStyle`, `VfShellSidebarStyle`, `VfShellContentStyle`, `VfShellCommandBandContainerStyle`, `VfShellStatusContainerStyle`, and `VfShellPreviewContainerStyle`.
- `docs/ui/ui-contract-scopes.v1.json` marks `shell-surface-foundation` active and governs the M2 shell resource scope.
- Static tests verify the route controls and event handlers remain present.

## Deviations

No accepted reference deviation or temporary redesigned/non-redesigned mismatch was recorded for M2. If code review or manual app launch finds a visible mismatch, it must be recorded in `docs/ui/design-deviations.md` before M2 closes.
