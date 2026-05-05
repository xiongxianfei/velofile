# M12 Image Text And PDF Preview Providers

M12 adds bounded Windows preview providers on top of the M11 preview contract. The goal is to make production composition use content providers for supported images, text/code files, and PDFs while keeping the M11 safety properties: timeout routing, metadata fallback, stale-work ignore, redacted diagnostics, and source non-mutation.

## What Changed

`src/VeloFile.Windows/Preview/` now contains the Windows provider set:

- `WindowsImagePreviewProvider` supports common image extensions, decodes image bodies through Windows imaging, returns PNG render artifacts, rejects files over 100 MB, and rejects decoded dimensions over 8192 by 8192.
- Review resolution replaced the header-only image path with `IImagePreviewDecoder` and `WindowsImagePreviewDecoder`, which use Windows imaging decode APIs to produce PNG render artifact bytes before image preview success.
- `WindowsTextPreviewProvider` supports common text/code extensions, reads at most the first 1 MB, marks truncation when content continues, rejects binary-looking inputs, and refuses files over 100 MB.
- `WindowsPdfPreviewProvider` renders pages through `IPdfPageRenderer` and `WindowsPdfPageRenderer`, returns encoded page artifact bytes, renders page 1 initially, and exposes a paged preview hook so later pages render only after user navigation.
- `WindowsPreviewProviderFactory` returns image, text, PDF, then metadata fallback in production order.

`PreviewContent` now carries image and PDF page render artifacts with encoded bytes, dimensions, format, and PDF page metadata. The app preview pane exposes content text and an image surface that loads those artifact bytes for image/PDF previews.

The final M12 review-resolution tightened two production boundaries. `WindowsImagePreviewDecoder` and `WindowsPdfPageRenderer` now enforce the actual opened stream length before BitmapDecoder or PdfDocument work begins, so null or stale listing metadata cannot bypass the 100 MB image cap or 500 MB PDF cap. The preview pane also exposes visible PDF Previous/Next controls that call view-model page commands, making later-page rendering reachable from the production shell route rather than only from direct tests.

`AppCompositionRoot` now uses the Windows provider factory instead of the metadata-only provider list. Metadata fallback remains last in the provider chain.

`tools/VeloFile.Corpus` now supports `preview --scope providers`. The corpus wrapper copies `src/VeloFile.Windows` into its scratch build so provider evidence runs in the same isolated tool environment as other corpus scopes.

While validating the full Corpus test assembly, the added Windows project reference exposed an existing wrapper weakness: the wrapper forced all copied projects to share one intermediate-output directory, which could produce a `.deps.json` missing `VeloFile.Core`. The wrapper now relies on each copied scratch project’s default `bin/obj` folders and still publishes the final tool to the existing scratch-local publish directory.

While rerunning solution-level provider validation, the Corpus test harness could deadlock while reading redirected subprocess output sequentially. The harness now starts asynchronous stdout and stderr reads before waiting for the corpus subprocess to exit, which keeps the validation wrapper from blocking while scripts are still writing output.

## Test-First Evidence

The Windows provider tests were added before the `VeloFile.Windows.Preview` namespace existed. The first provider test run failed for missing provider types.

The App composition contract test was added before composition changed. It failed because production still instantiated `[new MetadataOnlyPreviewProvider()]`.

The Corpus provider test was added before the provider scope existed. It first failed with `Preview corpus scope 'providers' is not implemented in M2`, then failed again until the corpus wrapper copied the Windows adapter project into the scratch tool build.

## Tests

Windows provider tests cover:

- PNG and JPEG decode success with non-empty render artifacts;
- image over-size fallback;
- image over-size fallback when listing length is null or stale and actual opened stream length is over the cap;
- exact image-cap behavior and unavailable stream-length fail-closed behavior;
- image decoded-dimension fallback;
- corrupt image body failure so header-only fixtures cannot pass;
- text 1 MB prefix limit and truncation;
- binary text refusal;
- text over-size fallback;
- PDF first-page render artifact success;
- PDF later-page rendering only after explicit navigation;
- PDF over-size fallback;
- PDF over-size fallback when listing length is null or stale and actual opened stream length is over the cap;
- corrupt PDF failure;
- source non-mutation for image, text, and PDF provider paths;
- production provider factory order.

Core and App tests prove the PDF page navigation hook: initial preview requests page 1 only, and a later page is rendered only after the shell/view model requests it. App tests also prove the preview pane declares Previous/Next controls and code-behind handlers that call `RequestPreviousPdfPage()` and `RequestNextPdfPage()`, and that production composition uses the Windows provider factory instead of stopping at metadata-only preview.

Corpus tests prove `preview --scope providers` writes provider behavior-verifier evidence for image artifact success, text truncation, PDF page artifact success, over-size fallback, and source non-mutation. The provider corpus fixtures now use decodable image/PDF files and verify non-empty artifact bytes.

## Scope Notes

This slice does not implement thumbnail/icon execution or advanced preview UI polish. Those remain M13 work. It also avoids third-party preview engines and Shell preview handler hosting per ADR 0004.

## Validation

- `dotnet test tests\VeloFile.Windows.Tests\VeloFile.Windows.Tests.csproj -c Debug --filter PreviewProviders`
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter PreviewProviders`
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter PreviewProviders`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -Scope providers -ScratchRoot <scratch-root>`
- `dotnet test VeloFile.sln -c Debug --filter PreviewProviders`
- `dotnet test VeloFile.sln -c Debug --filter PreviewContract`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
