# ADR 0006: Session Restore Persistence

## Status

accepted; amended 2026-05-04 for session and settings persistence safety

## Context

V1 promises session restore but must avoid restoring surprising or dangerous state. The V1 product scope also requires session and settings persistence to tolerate partial writes, schema evolution, unknown fields, and missing restored paths. Persistence safety is part of the user-data safety boundary and cannot be deferred to implementation planning.

## Decision

Persist versioned local session state. Restore tab paths, active tab index, per-tab history, sort state, view mode, window placement, monitor target when available, sidebar state, and scroll anchored by first visible item name.

Do not restore selected files, filter text, recursive search query/results, clipboard contents, authentication state, or in-flight file operations. Missing paths remain visible with missing-location state, path display, and close-tab action.

Session, settings, favorites, and recent locations are durable local documents and must use this partial-write-safe protocol:

1. Serialize the complete document in memory with a schema header.
2. Validate parseability before touching the canonical file.
3. Write a unique temporary file in the same directory as the canonical file.
4. Flush and close the temporary file.
5. Replace the canonical file using Windows-safe same-volume atomic replacement or equivalent rename/swap behavior.
6. Preserve a last-known-good backup.
7. Recover from canonical failure by trying last-known-good, then safe defaults.
8. Ignore unknown fields and fall back per field wherever safe.
9. Log local redacted diagnostics for fallback and migration events.

The preferred Windows implementation is a same-directory temporary file plus `ReplaceFileW` for existing canonical files, or same-directory `MoveFileExW` with replace/write-through semantics for first creation. Equivalent APIs are acceptable only when they preserve this safety property: after crash or power loss, the app can read the old valid document, new valid document, or last-known-good document.

## Durable Document Header

Each durable document must include:

```json
{
  "documentType": "session|settings|favorites|recentLocations",
  "schemaVersion": 1,
  "minimumReaderVersion": 1,
  "appVersion": "<semantic-or-build-version>",
  "writtenAtUtc": "<ISO-8601 timestamp>",
  "payload": {}
}
```

## Migration and Fallback Rules

- Unknown fields are ignored.
- Missing fields fall back to documented defaults.
- Malformed optional fields fall back per field and produce a local redacted diagnostic event.
- Malformed required structural fields trigger last-known-good recovery.
- Newer schema versions degrade to known fields where safe.
- A failed session restore must not block app launch.
- Missing restored tab paths remain visible with a recoverable missing-location state, path display, and close-tab action. They are not silently skipped.

## Alternatives considered

- Restore every UI detail: higher surprise and safety risk.
- Restore only active path: loses V1 session value.
- Durable operation resume: separate feature, out of V1.
- Directly overwrite persistence files: simpler but risks losing all launch state on crash or power loss.

## Consequences

- Session schema exists from first release.
- Crash markers and start-fresh recovery are part of restore flow.
- Selection and filter restore are intentionally absent.
- Crash or power-loss windows during persistence writes do not corrupt all launch state.
- Schema changes can be deployed without turning older or partially understood documents into launch blockers.
- Diagnostics can identify fallback and migration frequency without exposing raw paths or filenames.
- Persistence implementation is more complex than direct overwrite and must maintain last-known-good files plus cleanup policy.

## Required Tests

ADR 0006 is not implementation-ready until tests cover:

- Crash before temporary file creation.
- Crash after temporary file creation but before replacement.
- Crash during replacement.
- Corrupt canonical file with valid last-known-good backup.
- Corrupt canonical file and corrupt backup, producing safe defaults without crash loop.
- Unknown fields retained or ignored safely.
- Malformed optional fields falling back per field.
- Newer schema with known fields recoverable.
- Missing restored path visible with recoverable missing-location state.
- Removed monitor/window placement fallback.
- Local diagnostic fallback event contains no raw path, filename, username, search query, clipboard content, or file content.

## Follow-up

Test-spec must map these required tests to the approved V1 scope before implementation. The execution plan must include persistence fault-injection coverage around write, replace, and startup recovery boundaries.
