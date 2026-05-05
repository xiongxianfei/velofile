# M11 Review Resolution

## Findings Addressed

### 1. Provider-specific preview timeout policy

Disposition: fixed.

Changes:

- Added explicit `PreviewOperation` values for image decode, text read/encoding detection, PDF first-page render, metadata fallback, and thumbnail generation.
- Added `PreviewTimeoutPolicy.Default` with the R67 budgets:
  - image decode: 2 seconds;
  - text read and encoding detection: 1 second;
  - PDF first-page render: 3 seconds;
  - thumbnail generation: 500 ms per item;
  - thumbnail concurrency: 4.
- Updated `IPreviewProvider` to expose its operation kind and receive a `PreviewProviderContext` containing the selected timeout budget.
- Updated `PreviewController` so timeout enforcement comes from the selected provider operation, not a single global timeout value.

Proof:

- `PreviewContract_default_timeout_policy_uses_R67_budgets`
- `PreviewContract_controller_supplies_selected_provider_operation_budget`
- `PreviewContract_provider_specific_timeout_uses_selected_operation_budget`
- Preview corpus `contract` scope now includes a `timeout-policy` behavior-verifier case.

### 2. Non-mutation and complete metadata fallback proof

Disposition: fixed.

Changes:

- Expanded `FileSystemEntrySnapshot`, `ListedFileItem`, and `PreviewMetadata` to carry creation, modified, and accessed timestamps when available.
- Updated Windows listing projection to provide creation and access timestamps from `FileSystemInfo`.
- Updated preview metadata fields so fallback exposes size, created/modified/accessed timestamps, attributes, and type.
- Added scratch-file preview tests that compare content hash, length, creation time, last-write time, and attributes after the controller/provider path runs.
- Added fallback tests for unsupported files and unavailable metadata.
- Updated app preview tests to prove the shell-visible metadata surface exposes the expanded fallback fields.

Proof:

- `PreviewContract_provider_path_does_not_modify_source_file`
- `PreviewContract_unsupported_metadata_fallback_does_not_modify_source_and_shows_standard_metadata`
- `PreviewContract_metadata_fallback_handles_unavailable_metadata_without_losing_available_fields`
- `PreviewContract_preview_toggle_and_selection_start_preview_state`

## Validation

- `dotnet test VeloFile.sln -c Debug --filter PreviewContract` passed: 17 Core, 4 App, and 1 Corpus preview-contract tests; Windows tests had no matching filter.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope contract -ScratchRoot <scratch-root>` passed with a compliant scratch root.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter "Listing|Visibility"` passed: 29 Core, 3 App, and 4 Windows tests; Corpus tests had no matching filter.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` first failed because the updated assertion used the wrong `Assert.IsGreaterThanOrEqualTo` parameter order; after fixing the test assertion, it passed.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` first failed for the same corpus assertion issue; final run passed restore, build with 0 warnings and 0 errors, and 262 tests across Windows, App, Core, and Corpus test assemblies.
