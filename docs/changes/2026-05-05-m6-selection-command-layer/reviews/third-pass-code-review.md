# M6 Third-Pass Code Review

## Outcome

Changes required.

## Findings

1. Blocker: `FileListSurface` still used static placeholder `ListViewItem` rows, while the selection handler only accepted selected values that were already `ListedFileItem`. The shell could visually select rows but leave `SelectedFileItems` empty, so Copy path/name did not work through the real WinUI selection route.

2. Blocker: WinUI file-command accelerator handlers routed every shortcut as if text input did not have focus. The Core router could suppress file commands for text-input focus, but the production shell never passed that focus state, so commands such as Copy path/name, Delete, F2, Escape, and Ctrl+A could run while the user typed in text fields.

## Required Resolution

- Bind the production file list to real `ListedFileItem` data and map selected UI rows back to file models before invoking commands.
- Route file-command accelerators through an app-layer focus-context provider and only mark accelerators handled when the command router accepts the shortcut.
