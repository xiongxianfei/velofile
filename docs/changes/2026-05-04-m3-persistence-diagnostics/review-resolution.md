# M3 Review Resolution

## Findings Addressed

- Persistence read failures now use a durable storage read-result boundary. Expected filesystem failures return `RecoverableFailure`, and repository reads continue from canonical to last-known-good to safe defaults.
- Settings, favorites, and recent locations now have durable document codecs. Session state now serializes and restores window placement, with malformed placement falling back per field.
- Diagnostic storage writes, marker writes, retention, rotation, and marker enumeration are best-effort for expected filesystem failures and do not throw into app workflows.
- Diagnostic serialization now uses deny-by-default field policies. Known diagnostic vocabulary and generated IDs pass only through explicit allowlists; unknown string values are replaced with the non-correlating constant token `redacted-string`.
- Repository reads now emit best-effort redacted diagnostics when a document-level read succeeds but codecs report malformed optional fields or per-field fallback metadata.

## Regression Tests

- `Repository_recovers_when_primary_storage_read_is_recoverable_failure`
- `Windows_storage_treats_missing_reads_as_recoverable_instead_of_using_check_then_read`
- `Session_document_round_trips_window_placement_and_falls_back_per_malformed_placement_field`
- `Durable_payloads_cover_settings_favorites_and_recent_locations`
- `Local_diagnostics_are_best_effort_when_storage_is_unavailable`
- `Diagnostic_serializer_redacts_dangerous_values_in_every_serialized_string_field`
- `Diagnostic_serializer_preserves_only_allowed_vocabulary_and_generated_ids`
- `Repository_logs_field_fallback_when_canonical_session_optional_field_is_malformed`
- `Repository_logs_field_fallback_when_canonical_settings_optional_field_is_malformed`
- `Repository_logs_field_fallback_when_favorites_or_recent_location_entries_are_malformed`
- `Repository_read_succeeds_when_field_fallback_diagnostic_sink_throws`

## Validation

- `dotnet test VeloFile.sln -c Debug --filter Persistence`
- `dotnet test VeloFile.sln -c Debug --filter Diagnostics`

Final CI validation is recorded in the active plan after closeout.

- Latest review-resolution validation:
  - `dotnet test VeloFile.sln -c Debug --filter Persistence` passed: 14 tests across Core and Windows test assemblies.
  - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` passed: 7 Core diagnostics tests.
  - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and 30 tests across 4 test assemblies.
  - Third review-resolution validation after replacing predictable SHA-based denied-string tokens:
    - `dotnet test VeloFile.sln -c Debug --filter Persistence` passed: 14 tests across Core and Windows test assemblies.
    - `dotnet test VeloFile.sln -c Debug --filter Diagnostics` passed: 7 Core diagnostics tests.
    - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed: restore, build, and 30 tests across 4 test assemblies.
