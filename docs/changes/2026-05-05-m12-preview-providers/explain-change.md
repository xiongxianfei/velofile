# M12 Image Text And PDF Preview Providers

M12 adds bounded Windows preview providers on top of the M11 preview contract. The goal is to make production composition use content providers for supported images, text/code files, and PDFs while keeping the M11 safety properties: timeout routing, metadata fallback, stale-work ignore, redacted diagnostics, and source non-mutation.

## What Changed

`src/VeloFile.Windows/Preview/` now contains the Windows provider set:

- `WindowsImagePreviewProvider` supports common image extensions, reads only enough header data to identify dimensions, rejects files over 100 MB, and rejects decoded dimensions over 8192 by 8192.
- `WindowsTextPreviewProvider` supports common text/code extensions, reads at most the first 1 MB, marks truncation when content continues, rejects binary-looking inputs, and refuses files over 100 MB.
- `WindowsPdfPreviewProvider` validates a bounded first-page-capable PDF sample, returns first-page preview content, and refuses PDFs over 500 MB.
- `WindowsPreviewProviderFactory` returns image, text, PDF, then metadata fallback in production order.

`PreviewContent` now carries optional image dimensions and PDF page number so provider success can be asserted without adding M13 UI rendering work.

`AppCompositionRoot` now uses the Windows provider factory instead of the metadata-only provider list. Metadata fallback remains last in the provider chain.

`tools/VeloFile.Corpus` now supports `preview --scope providers`. The corpus wrapper copies `src/VeloFile.Windows` into its scratch build so provider evidence runs in the same isolated tool environment as other corpus scopes.

While validating the full Corpus test assembly, the added Windows project reference exposed an existing wrapper weakness: the wrapper forced all copied projects to share one intermediate-output directory, which could produce a `.deps.json` missing `VeloFile.Core`. The wrapper now relies on each copied scratch project’s default `bin/obj` folders and still publishes the final tool to the existing scratch-local publish directory.

## Test-First Evidence

The Windows provider tests were added before the `VeloFile.Windows.Preview` namespace existed. The first provider test run failed for missing provider types.

The App composition contract test was added before composition changed. It failed because production still instantiated `[new MetadataOnlyPreviewProvider()]`.

The Corpus provider test was added before the provider scope existed. It first failed with `Preview corpus scope 'providers' is not implemented in M2`, then failed again until the corpus wrapper copied the Windows adapter project into the scratch tool build.

## Tests

Windows provider tests cover:

- PNG success with decoded dimensions;
- image over-size fallback;
- image decoded-dimension fallback;
- text 1 MB prefix limit and truncation;
- binary text refusal;
- text over-size fallback;
- PDF first-page success;
- PDF over-size fallback;
- corrupt PDF failure;
- source non-mutation for image, text, and PDF provider paths;
- production provider factory order.

App tests prove production composition uses the Windows provider factory and no longer stops at metadata-only preview.

Corpus tests prove `preview --scope providers` writes provider behavior-verifier evidence for image success, text truncation, PDF first-page, over-size fallback, and source non-mutation.

## Scope Notes

This slice does not implement thumbnail/icon execution or preview UI rendering polish. Those remain M13 work. It also avoids third-party preview engines and Shell preview handler hosting per ADR 0004.

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
