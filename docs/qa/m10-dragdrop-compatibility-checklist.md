# M10 Drag/Drop and Path Compatibility Checklist

Use this checklist for Windows behaviors that are brittle or unsafe to automate in CI. All file-system work must use a dedicated generated scratch root.

## Drag/Drop Peers

- Explorer to VeloFile: no modifier resolves to same-volume move and cross-volume copy.
- Explorer to VeloFile: Ctrl resolves to copy.
- Explorer to VeloFile: Shift resolves to move.
- Explorer to VeloFile: Ctrl+Shift resolves to shortcut intent before drop.
- Browser download/link drag to VeloFile: unsupported payloads are rejected visibly or file payloads are accepted through the file-drop boundary.
- IDE/project file drag to VeloFile: file payloads resolve to the same modifier actions.
- Office or representative document app drag to VeloFile: file payloads resolve to the same modifier actions, and unsupported embedded-object payloads are rejected visibly.

## Path Compatibility

- Long paths: enumerate and operate according to the compatibility corpus result or show a clear Windows-denied failure.
- Junctions: enumerate without escaping documented behavior; recursive traversal must not loop indefinitely.
- Symlinks: enumerate and operate according to Windows permissions and the compatibility corpus.
- Reparse loops: recursive search reports skipped loops rather than recursing unbounded.
- Access-denied paths: operations fail closed with recoverable status.
