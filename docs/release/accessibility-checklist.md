# Accessibility Release Checklist

This checklist is required M15 release-readiness evidence. Static XAML scans can support it, but they do not replace a keyboard, focus, mixed-DPI, and screen-reader smoke pass.

## Run Metadata

| Field | Value |
| --- | --- |
| Status | Not run |
| Tester |  |
| Date |  |
| Build |  |
| Environment | Windows version, display layout, scale factors, input devices, assistive technology |
| Notes |  |
| Linked issue |  |

## Required Results

Use `Pass`, `Fail`, or `Blocked` for every item. Any `Fail` or `Blocked` item needs Notes and a Linked issue before release evidence can be accepted.

| Area | Check | Result | Notes | Linked issue |
| --- | --- | --- | --- | --- |
| Keyboard | Keyboard-only navigation reaches the sidebar, tab strip, path box, file list, preview pane, operation panels, and command surfaces. | Blocked |  |  |
| Keyboard | Focus order is logical when moving through tabs, sidebar, file list rows, path entry, preview controls, search controls, rename controls, and conflict controls. | Blocked |  |  |
| Focus | Focus indicator is visible on every interactive control, including icon buttons, PDF navigation, recursive search, rename, conflict, cancel, and destructive confirmation actions. | Blocked |  |  |
| Destructive actions | Destructive delete and permanent delete confirmation text is readable, distinguishable, and exposes clear action labels. | Blocked |  |  |
| Operations | Operation cancel, cancelling, cancelled, failed, and completed states are visible and distinguishable. | Blocked |  |  |
| Search | Recursive search cap, skipped locations, cancelled, failed, and completed states are visible and distinguishable. | Blocked |  |  |
| Preview | Preview loading, preview failed, metadata fallback, and unsupported states are visible and distinguishable. | Blocked |  |  |
| Screen-reader | Key controls expose automation name, role, and state metadata in a screen-reader smoke pass. | Blocked |  |  |
| DPI | Mixed-DPI readability is checked at 100%, 150%, 200%, and during movement between differently scaled monitors. | Blocked |  |  |
| DPI | Text, icons, file list, context menu, preview pane, destructive confirmations, and operation panels remain readable without clipped critical controls. | Blocked |  |  |
