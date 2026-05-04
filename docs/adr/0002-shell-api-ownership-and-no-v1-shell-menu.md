# ADR 0002: Shell API Ownership and No V1 Shell Menu Integration

## Status

accepted

## Context

V1 must respect Windows behavior for file operations, drag/drop, file associations, icons, thumbnails, long paths, and Recycle Bin delete. The approved spec excludes OS shell menu integration and third-party Shell extension hosting from V1.

## Decision

Prefer high-level Shell APIs that own Windows-correct behavior. Use lower-level APIs only when needed for a V1 requirement. V1 does not expose OS Shell extension menu entries or enumerate third-party Shell context menu handlers.

Recommended ownership:

- File operations and Recycle Bin delete: Shell-owned operation adapter.
- Drag/drop: OLE drag/drop formats for cross-process interoperability.
- File association open and Open With: Shell execution adapter.
- Thumbnails/icons: Shell thumbnail/icon adapters with timeouts and fallback.
- Long paths and reparse points: canonicalization adapter with consistent long-path handling.

## Alternatives considered

- Reimplement operations directly with low-level file APIs: higher risk for data loss and parity bugs.
- Include OS shell menu integration in V1: conflicts with the approved spec and vision.
- Delay all Shell interop: prevents V1 Windows compatibility.

## Consequences

- Shell behavior stays behind adapters.
- Built-in context menu is the only V1 context menu.
- Post-V1 shell menu integration requires a separate proposal or ADR and shell-menu diagnostics markers.

## Follow-up

Define exact adapter API contracts during implementation planning and test-spec work.
