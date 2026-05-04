# M3 Persistence and Diagnostics Change Explanation

## Scope

M3 adds the local state and diagnostics foundation required before navigation, session restore, settings, and release triage depend on durable data:

- Versioned durable document envelopes for session, settings, favorites, and recent locations.
- Session payload parsing with unknown-field ignore and per-field fallback for malformed optional fields.
- A durable repository abstraction that recovers canonical state, then last-known-good backup, then safe defaults.
- A Windows storage adapter that writes same-directory temporary files, flushes them, and replaces canonical files while preserving last-known-good backup data.
- Local diagnostic events, path redaction, non-reversible per-installation fingerprints, rotating diagnostic logs, crash markers, last-action markers, and repeated-crash detection primitives.

This does not implement the UI restore flow, missing-location tabs, monitor placement UI, or preview-release numeric promotion policy. Those remain assigned to later milestones.

## Test-First Evidence

M3 tests were added before production implementation:

- `tests/VeloFile.Core.Tests/Persistence/DurableDocumentTests.cs`
- `tests/VeloFile.Core.Tests/Diagnostics/DiagnosticsTests.cs`
- `tests/VeloFile.Windows.Tests/Storage/WindowsDurableDocumentStorageTests.cs`

The initial targeted run failed for the expected reason: `VeloFile.Core.Persistence`, `VeloFile.Core.Diagnostics`, and `VeloFile.Windows.Storage` did not exist yet, and `VeloFile.Windows` had no Core adapter-boundary reference.

## Design Choices

`VeloFile.Core.Persistence` owns document schema, schema-tolerant reads, and recovery decisions. `VeloFile.Windows.Storage` owns the file-system replacement primitive because atomic replacement behavior is platform-specific.

The session payload model deliberately records excluded restore behavior as false for selection and filter text. This keeps the M3 schema aligned with the V1 rule that those fields are not restored, while deferring UI session restore to M5.

Diagnostics are local-only primitives. Events expose allowed operational fields, path classification, and optional HMAC-based path fingerprints. They do not accept or serialize raw path fields, raw filenames, search text, clipboard content, or preview text.

## Validation

M3 validation passed with:

- `dotnet test VeloFile.sln -c Debug --filter Persistence`
- `dotnet test VeloFile.sln -c Debug --filter Diagnostics`

Final milestone closeout also passed:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`

## Deferred By Plan

- UI crash recovery and start-fresh prompts: M5.
- Full diagnostics conformance/export tests and preview-release threshold policy: M15.
- Durable app settings UI and recent-location workflows: M5 and later workflow milestones.
