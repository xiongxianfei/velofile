# M11 Preview Contract And Metadata Fallback

M11 establishes the preview contract before rich content decoders are added. The implementation keeps content provider work out of this slice and focuses on state safety: clear old preview state on selection changes, show loading only after the delay, terminate with success/unsupported/failed states, time out slow providers, and keep metadata available as fallback.

## What Changed

`src/VeloFile.Core/Preview/` adds the preview model, provider interface, metadata provider, metadata-only fallback provider, and `PreviewController`. The controller assigns each request a generation and cancellation token, clears the previous preview immediately, applies the 200 ms default loading delay, applies the timeout budget, suppresses stale completions, and emits best-effort redacted diagnostics for preview failures.

`AppShellViewModel` now exposes preview pane state, preview status text, and metadata fields. It starts preview only for a single selected visible item when the pane is open, and clears preview state on pane close, tab changes, listing/filter refreshes, and recursive-search clearing.

`MainWindow.xaml` adds the preview pane and Ctrl+P accelerator route. The preview column collapses to zero width when the pane is closed so the file list remains the primary surface.

`AppCompositionRoot` wires the preview controller with a metadata-only provider, diagnostics, and a retained path-redaction salt under app data. Rich text/image/PDF providers remain in M12.

`tools/VeloFile.Corpus` now supports `preview --scope contract`. The contract result is no longer a static pass document: it invokes an in-process preview behavior verifier for loading delay, timeout, metadata fallback, and stale selection before reporting verified evidence.

## Review Resolution

The first-pass review found two M11 contract gaps.

Provider timeouts are now operation-specific. `PreviewTimeoutPolicy.Default` encodes the R67 budgets for image decode, text read/encoding detection, PDF first-page render, thumbnail generation, and thumbnail concurrency. Providers expose their `PreviewOperation`, and `PreviewController` passes a `PreviewProviderContext` with the selected operation budget into the provider and timeout race.

Metadata fallback now represents size, created/modified/accessed timestamps, attributes, and type when available. The Windows listing source populates creation and access timestamps from `FileSystemInfo`, and the preview metadata fields expose them to the shell. Scratch-file tests prove the controller/provider path does not change file bytes, length, creation time, last-write time, or attributes.

## Tests

Core preview tests cover fast success, delayed loading, timeout failure, unsupported/failure terminal states, stale selection cancellation/ignore behavior, metadata fallback, and redacted diagnostics.

App tests cover Ctrl+P/open-pane behavior through the view model, selection-driven preview start, immediate clearing of stale preview state, closing the pane, and a shell contract check that the WinUI surface is wired to preview state.

Corpus tests cover the `preview contract` runner scope and assert the contract result contains behavior-verifier evidence for the expected cases.

## Validation

- `dotnet test VeloFile.sln -c Debug --filter PreviewContract`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope contract -ScratchRoot <scratch-root>`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug --filter "Listing|Visibility"`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
