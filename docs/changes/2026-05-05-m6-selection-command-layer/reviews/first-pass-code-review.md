# M6 First-Pass Code Review

## Review Status

changes-requested

## Review Inputs

- Diff range: `HEAD^..HEAD` (`61a28b3 M6: add selection and built-in command layer`)
- Review surface: committed M6 diff plus remaining working tree status
- Tracked governing branch state: `specs/v1-product-scope.md`, `specs/v1-product-scope.test.md`, `docs/plans/2026-05-04-v1-product-scope.md`, `docs/architecture/system/architecture.md`
- Spec: R47-R53
- Test spec: T021-T023
- Plan milestone: M6
- Architecture / ADR: command layer in the desktop app component view; ADR 0007 Explorer parity policy
- Validation evidence: committed change notes and local `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` pass

## Diff Summary

M6 adds Core selection state, a built-in command registry, keyboard command routing, copy path/name clipboard formatting, a Windows clipboard writer, App shell context-menu/accelerator routes, tests, and change-local documentation.

## Findings

### Major: WinUI context menu bypasses command availability

Evidence: [MainWindow.xaml](/D:/Data/20260428-velofile/src/VeloFile.App/MainWindow.xaml:323) declares static menu items and [MainWindow.xaml](/D:/Data/20260428-velofile/src/VeloFile.App/MainWindow.xaml:328) always exposes `Paste`, while [MainWindow.xaml.cs](/D:/Data/20260428-velofile/src/VeloFile.App/MainWindow.xaml.cs:348) invokes `Paste` directly. The Core registry correctly models availability, but the user-visible menu does not consult it before showing/enabling verbs.

Why this matters: R48 says Paste is included when applicable, and the M6 plan explicitly includes context-menu command availability. A static always-enabled menu can expose Paste and selection verbs when there is no applicable clipboard/selection state, so the shell can drift from the command-layer contract even though the Core registry tests pass.

Required outcome: the WinUI context menu must refresh item availability from the same built-in command surface used by tests, at minimum disabling or hiding Paste when not applicable and disabling selection verbs when no file item is selected. Add a shell contract or view-model test proving the route is connected.

Safe resolution path: name the menu items, add a thin `Opening`/refresh handler that asks `AppShellViewModel` for availability, and make menu clicks execute only commands that are available in the current context.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | concern | Core registry aligns with R47-R49, but the visible menu violates applicability for Paste/selection verbs. |
| Test coverage | concern | Tests prove Core availability but not that the shell applies it to the menu. |
| Edge cases | concern | No-selection and no-paste shell states are not covered at the UI route. |
| Error handling | pass | Clipboard no-selection is safe in Core, and Windows clipboard failures remain behind the adapter. |
| Architecture boundaries | pass | Command and clipboard logic stays in Core/Windows boundaries; code-behind is thin. |
| Compatibility | pass | No Shell extension enumeration is introduced. |
| Security/privacy | pass | No diagnostics, paths, or clipboard contents are logged. |
| Generated output drift | pass | No generated source is committed. |
| Unrelated changes | pass | The committed diff is scoped to M6 artifacts; unrelated pre-existing worktree changes remain unstaged. |
| Validation evidence | pass | Focused tests, solution build, and CI evidence are recorded and passed. |

## Recommended Next Stage

review-resolution
