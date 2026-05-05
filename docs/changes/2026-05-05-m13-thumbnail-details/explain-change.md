# M13 Thumbnails Icons And Preview UI Concurrency

M13 adds thumbnail/icon enrichment and preview UI polish on top of the M11-M12 preview provider work. The goal is to make the visible file list richer without letting thumbnail generation block navigation, leak stale work, or violate the R67 thumbnail budget.

## What Changed

`VeloFile.Core.Preview` now has thumbnail-specific models and a `ThumbnailController`. The controller starts thumbnail work for the current visible item set, limits concurrent provider calls to four operations, applies the 500 ms thumbnail budget per item from `PreviewTimeoutPolicy`, cancels older generations, ignores stale results, and falls back to generic icons on timeout or provider failure.

Review resolution tightened the timeout contract for providers that ignore cancellation. The controller now races each visible thumbnail request against the configured thumbnail budget, marks the row with a timeout fallback when the visible deadline wins, and keeps the underlying provider task inside the semaphore-held work until it actually completes. That preserves both halves of R67: rows do not stay loading forever, and stuck provider calls still count against the live concurrency cap.

`VeloFile.Windows.Preview.WindowsThumbnailProvider` is the production Windows boundary. It asks Windows Storage APIs for file and folder thumbnails, reads returned thumbnail streams into immutable artifacts, and falls back to generic icon artifacts when thumbnails are unavailable or projection fails. Generic fallback artifacts are cached by directory or file-extension class.

`AppShellViewModel` now accepts a thumbnail controller, starts thumbnail work when the visible folder/search/filter rows change, clears stale work when the visible list is empty, and exposes `FileListRows` as the shell-facing row projection. The new `FileListRowViewModel` keeps the canonical `ListedFileItem` available for selection mapping while adding thumbnail display state and hidden/protected dimming. Rows are updated in place when thumbnail state changes so loading thumbnails do not replace visible row objects and risk clearing selection.

Review resolution also added `IShellDispatcher`. Thumbnail controller events can arrive from worker continuations, so `AppShellViewModel` now posts thumbnail row refreshes through the shell dispatcher before mutating row view models or raising shell binding notifications. Production launch passes a WinUI `DispatcherQueue` implementation into composition before the view model subscribes to thumbnail events, `MainWindow` refreshes the dispatcher for injected shell instances, and tests use immediate or recording dispatchers.

The WinUI file list binds to `FileListRows` and displays a compact thumbnail/icon slot before name, kind, and modified time. The preview/details pane now exposes a stable accessibility name derived from preview state and binds metadata details through the view model, so empty, loading, unsupported, and failed preview states are distinguishable to assistive technology.

`tools/VeloFile.Corpus` now supports `preview --scope thumbnails`. That scope records behavior-verifier evidence for thumbnail concurrency, per-item timeout fallback, generic icon fallback, and stale-result ignore.

While validating the corpus test wrapper, the new thumbnail scope exposed a `pwsh` child-host hang when launched from redirected MSTest. The production scripts still support and pass under `pwsh`; the test harness now uses `powershell.exe` on Windows, matching the repository's documented local fallback path, and keeps `pwsh` for non-Windows environments.

## Test-First Evidence

The Core thumbnail controller tests were added before the controller existed and initially failed for missing thumbnail APIs.

The Windows provider tests were added before the production thumbnail provider existed and initially failed for missing provider APIs.

The App preview UI tests were added before file-list rows exposed thumbnail state or preview accessibility names, and they first failed for missing row/presenter surface.

The Corpus thumbnail test was added before `preview --scope thumbnails` existed. It first failed for missing scope support, then exposed the MSTest-launched `pwsh` harness hang that is now avoided by the Windows-shell test path.

## Tests

Core tests cover:

- maximum four concurrent thumbnail provider calls;
- per-item timeout converting unresolved thumbnail work to a generic icon;
- visible timeout fallback when the provider ignores cancellation;
- timed-out provider work continuing to count against the live-operation cap;
- late provider success not overwriting a timed-out fallback for the stale request;
- old thumbnail generations being cancelled and ignored after a new visible item set starts.

Windows tests cover:

- existing files returning either a Windows thumbnail artifact or a safe generic fallback without mutating the source file;
- generic icon artifacts being cached by extension class.

App tests cover:

- file-list row projection exposing thumbnail state and hidden/protected dimming;
- stable row objects across thumbnail state updates;
- thumbnail completion being dispatched before any row mutation or `PropertyChanged`;
- stale thumbnail completion not updating a recycled visible row;
- preview accessibility names for empty, loading, unsupported, and failed states;
- details metadata fields including size, timestamps, attributes, and type;
- MainWindow binding the file list to row view models and setting the preview pane automation name.

Corpus tests cover:

- `preview --scope thumbnails` writing durable behavior evidence for concurrency, timeout, generic fallback, and stale-result ignore.

## Scope Notes

This slice does not change image/text/PDF content preview rendering, PDF page navigation, or file-operation behavior from earlier milestones. It also does not introduce Shell preview-handler hosting or thumbnail persistence; thumbnails are best-effort, bounded, and safe to drop.

## Validation

- `dotnet test tests\VeloFile.Core.Tests\VeloFile.Core.Tests.csproj -c Debug --filter Thumbnails --no-restore`
- `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter Thumbnails --no-restore`
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "Thumbnails|PreviewUi" --no-restore`
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter Thumbnails --no-restore`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope thumbnails -ScratchRoot <scratch-root>`
- `dotnet test VeloFile.sln -c Debug --filter Thumbnails`
- `dotnet test VeloFile.sln -c Debug --filter PreviewUi`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug --no-build`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
