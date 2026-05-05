# M13 Review Resolution

## Findings Addressed

### 1. Thumbnail timeout depended on provider cancellation cooperation

Disposition: fixed.

Changes:

- Changed `ThumbnailController` to race each visible thumbnail request against the thumbnail budget instead of only awaiting the provider with `CancelAfter`.
- Added timeout-expired request tracking so a late provider success cannot overwrite the visible timeout fallback for the same generation.
- Kept provider work inside the semaphore-held task until the provider actually completes, faults, or observes cancellation. A timed-out but stuck provider still counts against the live thumbnail concurrency cap.
- Let queued rows hit the same visible deadline. If a row times out before it acquires a provider slot, it shows fallback and its stale provider work does not start later.

Proof:

- `Thumbnails_controller_times_out_noncooperative_provider_without_releasing_live_slot`
- `Thumbnails_controller_ignores_late_success_after_visible_timeout`
- Existing cooperative timeout/concurrency and stale-generation tests continue to pass.

### 2. Thumbnail row updates were not marshaled before WinUI-bound mutation

Disposition: fixed.

Changes:

- Added an app-layer `IShellDispatcher` abstraction.
- Updated `AppShellViewModel` so thumbnail state-change handling posts to the shell dispatcher before refreshing `FileListRowViewModel` state or raising `ShellStateChanged`.
- Wired `App.xaml.cs` and `AppCompositionRoot` so production launch passes a WinUI dispatcher-backed implementation before the view model subscribes to thumbnail events, and kept `MainWindow` setting the same dispatcher for injected shell instances.
- Added tests with a recording dispatcher proving row state and `PropertyChanged` do not change until dispatcher work is pumped.
- Added stale visible-row proof so a late thumbnail for an old row identity cannot update a recycled visible row.

Proof:

- `PreviewUi_thumbnail_completion_is_marshaled_before_row_mutation`
- `PreviewUi_stale_thumbnail_completion_does_not_update_replaced_visible_row`
- `PreviewUi_main_window_binds_file_rows_and_accessibility_preview_state`

## Validation

- `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 4 Core thumbnail tests.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Thumbnails|PreviewUi" --no-restore` passed: 5 App thumbnail/preview UI tests.
- `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 2 Windows thumbnail provider tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\run-preview-corpus.ps1 -Scope thumbnails -ScratchRoot <scratch-root>` passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 1 Corpus thumbnail evidence test.
- `dotnet test VeloFile.sln -c Debug --filter Thumbnails` passed: 4 Core, 2 Windows, and 1 Corpus thumbnail tests.
- `dotnet test VeloFile.sln -c Debug --filter PreviewUi` passed: 5 App preview UI tests.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --no-build` passed: 295 tests across App, Core, Windows, and Corpus assemblies.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed restore, build with 0 warnings and 0 errors, and 295 tests across App, Core, Windows, and Corpus test assemblies.

Notes:

- An intermediate parallel `dotnet test` plus `dotnet build` run hit the known App.Tests `obj` file-lock behavior. Sequential rerun after `dotnet build-server shutdown` passed.
- A later full-suite run caught an older app-launch contract assertion that still expected `CreateShellViewModel()` with no dispatcher. The test now asserts the production dispatcher-passing launch path.

## Second-Pass Review Resolution

Finding: timed-out providers could still exceed the R67 global thumbnail concurrency cap across generations because the live provider semaphore was generation-local.

Disposition: fixed.

Changes:

- Moved the live thumbnail provider gate to controller scope so all generations share one throttle.
- Kept the live slot held until the provider call actually completes, faults, or observes cancellation.
- Preserved the visible timeout behavior for queued rows. If a request cannot acquire a slot before its visible deadline, it becomes timeout fallback and does not start later as stale work.
- Kept generation and expired-request checks so late old results cannot update newer rows.

Proof:

- `Thumbnails_controller_counts_timed_out_provider_work_across_generations`
- `Thumbnails_controller_reuses_global_slot_for_new_generation_after_provider_completion`

Second-pass validation:

- `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 6 Core thumbnail tests.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Thumbnails|PreviewUi" --no-restore` passed: 5 App thumbnail/preview UI tests.
- `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 2 Windows thumbnail tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\run-preview-corpus.ps1 -Scope thumbnails -ScratchRoot <scratch-root>` passed.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Thumbnails --no-restore` passed: 1 Corpus thumbnail evidence test.
- `dotnet test VeloFile.sln -c Debug --filter Thumbnails` passed: 6 Core, 2 Windows, and 1 Corpus thumbnail tests.
- `dotnet test VeloFile.sln -c Debug --filter PreviewUi` passed: 5 App preview UI tests.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --no-build` passed: 297 tests across App, Core, Windows, and Corpus assemblies.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1` passed restore, build with 0 warnings and 0 errors, and 297 tests across App, Core, Windows, and Corpus test assemblies.
