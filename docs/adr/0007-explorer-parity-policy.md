# ADR 0007: Explorer Parity Policy

## Status

accepted

## Context

VeloFile must feel Windows-correct without copying Explorer's slow or unsafe defaults.

## Decision

Match Explorer/Windows behavior where users rely on muscle memory or where deviation risks data loss: keyboard selection, file association open, drag/drop modifiers, Recycle Bin default, long-path/reparse behavior, and protected operating-system file default.

Diverge intentionally where safety or responsiveness require it:

- Show file extensions by default.
- Use a built-in context menu only in V1.
- Keep current-folder filter separate from recursive search.

## Alternatives considered

- Full Explorer parity: inherits unwanted slow paths and unsafe defaults.
- Broad VeloFile-specific behavior: surprises Windows users.

## Consequences

- Compatibility corpus must encode parity cases.
- User-facing docs must explain differences from File Explorer.
- Future deviations need spec or ADR justification.

## Follow-up

Test-spec must cover keyboard selection, drag/drop modifiers, extension defaults, and protected-system-file toggles.
