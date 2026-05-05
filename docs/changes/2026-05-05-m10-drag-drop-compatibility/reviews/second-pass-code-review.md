# M10 Second-Pass Code Review

## Review status

changes-requested

## Findings

1. Drag/drop extraction failures could still escape the WinUI route.
   - `AppDragDropRoute` awaited payload extraction without converting failures into a no-drop result.
   - WinUI drag/drop handlers completed deferrals but did not have a final defensive catch.
   - `WindowsOleDragDropDataAdapter` called `Path.GetFullPath` without an expected path-projection boundary.

2. Path compatibility corpus results marked fixture creation as verified behavior.
   - Junction, symlink, and reparse-loop cases could return `verified` after the OS fixture existed.
   - The result schema did not distinguish fixture evidence from VeloFile behavior verification.

## Required outcome

- Malformed or inaccessible external drag/drop payloads must become controlled no-drop or recoverable failure states.
- A path compatibility case may count as verified only when fixture creation and VeloFile behavior verification both run and pass.

## Resolution tracking

Tracked in `../review-resolution.md`.
