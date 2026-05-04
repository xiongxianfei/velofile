# ADR 0008: Diagnostics Lifecycle, Redaction, and Preview Triage Boundary

## Status

accepted; amended 2026-05-04 for diagnostics lifecycle, redaction, retention, and triage ownership

## Context

V1 needs release readiness evidence without adding remote telemetry or exposing user data. VeloFile operates on user file locations, previews local documents, records crash markers, and may later evaluate high-risk Shell menu experiments. Diagnostics are therefore privacy-sensitive architecture data, not an implementation detail.

The architecture must define allowed fields, path redaction, local-only storage, retention/rotation, and ownership of preview-release triage inputs before planning relies on diagnostics.

## Decision

V1 diagnostics are local-only by default and limited to redacted operational events, crash markers, and last-action markers needed for recovery and release triage. No diagnostics, telemetry, crash reports, paths, filenames, or preview-derived content are uploaded without a separate approved proposal and explicit user opt-in.

Any post-V1 preview enabling OS shell menu integration must add shell-menu last-action markers and diagnostics privacy tests before enablement.

## Allowed Event Fields

Diagnostic events may include:

- Event id, event type, UTC timestamp, monotonic sequence number, severity, component, operation id, and correlation id.
- App version, build channel, package identity, OS version/build, process architecture, and app uptime bucket.
- Operation kind, result state, reason/error code, duration, timeout budget, retry count, and cancellation flag.
- Bounded counts such as item count, result-count bucket, conflict count, tab count, queue depth, and byte-size bucket.
- Preview provider id, file category, extension class if needed, size bucket, dimension bucket, and timeout/fallback state.
- Persistence document type, schema version, migration result, fallback source, unknown-field count, and corrupt-field count.
- Release triage inputs such as crash-marker presence, last-action marker category, preview failure category, and post-V1 shell-menu marker category.
- Path classification such as local, removable, mapped, network, cloud-placeholder, protected, or unknown.
- Optional non-reversible per-installation path fingerprint when repeated local failure correlation is needed.

## Prohibited Event Fields

Diagnostic events must not include:

- File contents.
- Raw full paths.
- Raw file names.
- Usernames from paths.
- Raw environment variables.
- Search query text.
- Clipboard contents.
- Authentication state, tokens, cookies, credentials, or secrets.
- Raw terminal command lines or shell-composed command strings.
- Text extracted from previews.

## Path Redaction

The default path rule is: classify, do not record.

When repeated local failure correlation is needed, diagnostics may store a non-reversible path fingerprint generated with a per-installation local salt/key. The fingerprint must not allow path reconstruction. The salt/key remains local and rotates when the user clears diagnostics or resets diagnostics privacy.

User-initiated diagnostic export defaults to redacted content and should show the export payload or a summary before export. Raw paths require a separate explicit per-export choice.

## Local Storage and Lifecycle

- Store diagnostics under the app's local data area.
- Retain rotating logs for at most 30 days or 50 MB total, whichever limit is reached first.
- Rotate individual log files at or before 5 MB.
- Retain at most the latest 10 crash markers.
- Retain only the latest last-action marker per marker category needed for crash attribution.
- Delete or overwrite oldest diagnostics first when limits are reached.
- Diagnostic retention failure must not block app launch, navigation, preview, search, or file operations.

## Preview-Release Triage Ownership

- ADR 0008 owns the diagnostic boundary, allowed/prohibited fields, redaction rules, local-only storage, retention, and release triage input categories.
- The release policy owns exact numeric promotion thresholds.
- The release owner decides promotion/blocking when preview thresholds are crossed.
- The diagnostics owner owns conformance tests for schema, redaction, retention, marker recording, and export behavior.
- New diagnostic field categories require architecture review before implementation.

A preview release must not be promoted when release-policy thresholds are exceeded until triage, mitigation, or an explicit exception is recorded. Post-V1 Shell menu preview experiments must not be enabled until shell-menu last-action markers and diagnostics privacy tests are implemented.

## Alternatives considered

- Remote telemetry by default: outside V1 and privacy-sensitive.
- No diagnostics: makes release gating and crash recovery weak.
- Full path/content logging: easier debugging but unacceptable privacy exposure.
- Defer field and retention rules to implementation planning: leaves privacy and release gates undefined during architecture review.

## Consequences

- Diagnostics service is a core architecture component.
- Architecture owns schema, redaction, storage lifecycle, and new field-category review.
- Release process must document exact numeric triage thresholds.
- Test-spec must verify redaction, local-only behavior, retention, marker recording, path fingerprint behavior, and export defaults.
- Diagnostics are less convenient for debugging raw user path issues, but avoid exposing the data VeloFile is trusted to manage.

## Required Tests

ADR 0008 is not implementation-ready until tests verify:

- Diagnostic events contain only allowed fields.
- Redaction tests find no raw paths, filenames, usernames, search queries, clipboard contents, credentials, file contents, or preview text.
- Path fingerprints are non-reversible and use a local per-installation salt/key.
- Local logs rotate at or before 5 MB.
- Total logs retain at most 30 days or 50 MB.
- Crash markers retain at most the latest 10 markers.
- Last-action markers retain only latest marker per category.
- Diagnostics are not uploaded by default.
- User-initiated export defaults to redacted content.
- Preview failure, persistence fallback, crash marker, and last-action marker events provide enough redacted triage input for release policy decisions.

## Follow-up

Test-spec must map these required tests to the approved V1 scope before implementation. The release policy must define numeric preview-release promotion thresholds before preview builds rely on diagnostic triage.
