using System.Security.Cryptography;
using System.Text;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;
using VeloFile.Windows.Preview;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace VeloFile.Windows.Tests.Preview;

[TestClass]
[TestCategory("PreviewProviders")]
public sealed class WindowsPreviewProviderTests
{
    [TestMethod]
    public async Task PreviewProviders_image_provider_decodes_png_and_jpeg_to_render_artifacts()
    {
        using var normal = ScratchFile.CreateBytes(
            "normal.png",
            await CreateBitmapBytesAsync(BitmapEncoder.PngEncoderId, width: 32, height: 24));
        var provider = new WindowsImagePreviewProvider();

        var success = await provider.PreviewAsync(
            Request(normal.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, success.Status);
        Assert.AreEqual(PreviewContentKind.Image, success.Content?.Kind);
        AssertImageArtifact(success.Content, expectedWidth: 32, expectedHeight: 24);

        using var jpeg = ScratchFile.CreateBytes(
            "photo.jpg",
            await CreateBitmapBytesAsync(BitmapEncoder.JpegEncoderId, width: 40, height: 30));
        var jpegSuccess = await provider.PreviewAsync(
            Request(jpeg.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, jpegSuccess.Status);
        Assert.AreEqual(PreviewContentKind.Image, jpegSuccess.Content?.Kind);
        AssertImageArtifact(jpegSuccess.Content, expectedWidth: 40, expectedHeight: 30);
    }

    [TestMethod]
    public async Task PreviewProviders_image_provider_rejects_limits_and_corrupt_bodies()
    {
        using var normal = ScratchFile.CreateBytes(
            "normal.png",
            await CreateBitmapBytesAsync(BitmapEncoder.PngEncoderId, width: 32, height: 24));
        var provider = new WindowsImagePreviewProvider();

        var tooLarge = await provider.PreviewAsync(
            Request(normal.ToListedFileItem(length: 100 * 1024 * 1024L + 1)),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, tooLarge.Status);
        Assert.AreEqual("image-too-large", tooLarge.ReasonCode);

        var hugeProvider = new WindowsImagePreviewProvider(
            new ScriptedImagePreviewDecoder(new ImagePreviewArtifact(
                PixelWidth: 8193,
                PixelHeight: 1,
                EncodedFormat: "png",
                EncodedBytes: [1, 2, 3],
                SourceWasDownsampled: false)));
        var huge = await hugeProvider.PreviewAsync(
            Request(normal.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, huge.Status);
        Assert.AreEqual("image-dimensions-too-large", huge.ReasonCode);

        var corruptBytes = await CreateBitmapBytesAsync(BitmapEncoder.PngEncoderId, width: 8, height: 8);
        Array.Resize(ref corruptBytes, corruptBytes.Length / 2);
        using var corrupt = ScratchFile.CreateBytes("corrupt.png", corruptBytes);
        var corruptResult = await provider.PreviewAsync(
            Request(corrupt.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreNotEqual(PreviewProviderResultStatus.Success, corruptResult.Status);
        Assert.AreEqual("decode-error", corruptResult.ReasonCode);

        var accessDeniedProvider = new WindowsImagePreviewProvider(
            new ThrowingImagePreviewDecoder(new UnauthorizedAccessException()));
        var accessDenied = await accessDeniedProvider.PreviewAsync(
            Request(normal.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Failed, accessDenied.Status);
        Assert.AreEqual("access-denied", accessDenied.ReasonCode);
    }

    [TestMethod]
    public async Task PreviewProviders_text_provider_reads_bounded_prefix_and_refuses_binary_or_oversize_files()
    {
        var provider = new WindowsTextPreviewProvider();
        var prefix = new string('a', 1024 * 1024);
        using var text = ScratchFile.CreateBytes("bounded.txt", Encoding.UTF8.GetBytes(prefix + "TAIL"));

        var success = await provider.PreviewAsync(
            Request(text.ToListedFileItem()),
            Context(PreviewOperation.TextReadAndEncodingDetection),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, success.Status);
        Assert.AreEqual(PreviewContentKind.Text, success.Content?.Kind);
        Assert.IsTrue(success.Content?.IsTruncated);
        Assert.AreEqual(1024 * 1024, success.Content?.TextContent?.Length);
        Assert.IsFalse(success.Content?.TextContent?.Contains("TAIL", StringComparison.Ordinal) ?? true);

        using var binary = ScratchFile.CreateBytes("binary.txt", [0x41, 0x00, 0x42]);
        var unsupported = await provider.PreviewAsync(
            Request(binary.ToListedFileItem()),
            Context(PreviewOperation.TextReadAndEncodingDetection),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, unsupported.Status);
        Assert.AreEqual("text-binary", unsupported.ReasonCode);

        var tooLarge = await provider.PreviewAsync(
            Request(text.ToListedFileItem(length: 100 * 1024 * 1024L + 1)),
            Context(PreviewOperation.TextReadAndEncodingDetection),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, tooLarge.Status);
        Assert.AreEqual("text-too-large", tooLarge.ReasonCode);
    }

    [TestMethod]
    public async Task PreviewProviders_pdf_provider_renders_first_page_artifact_with_real_renderer()
    {
        var provider = new WindowsPdfPreviewProvider();
        using var pdf = ScratchFile.CreateBytes("document.pdf", MinimalPdfBytes());

        var success = await provider.PreviewAsync(
            Request(pdf.ToListedFileItem()),
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, success.Status);
        Assert.AreEqual(PreviewContentKind.Pdf, success.Content?.Kind);
        AssertPdfArtifact(success.Content, expectedPageNumber: 1);
    }

    [TestMethod]
    public async Task PreviewProviders_pdf_provider_renders_later_pages_only_after_navigation()
    {
        using var pdf = ScratchFile.CreateBytes("document.pdf", MinimalPdfBytes(pageCount: 2));
        var renderer = new RecordingPdfPageRenderer(pageCount: 2);
        var provider = new WindowsPdfPreviewProvider(renderer);

        var firstPage = await provider.PreviewAsync(
            Request(pdf.ToListedFileItem()),
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, firstPage.Status);
        CollectionAssert.AreEqual(new[] { 1 }, renderer.RequestedPages.ToArray());
        AssertPdfArtifact(firstPage.Content, expectedPageNumber: 1);

        var secondPage = await provider.PreviewPageAsync(
            Request(pdf.ToListedFileItem()),
            pageNumber: 2,
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, secondPage.Status);
        CollectionAssert.AreEqual(new[] { 1, 2 }, renderer.RequestedPages.ToArray());
        AssertPdfArtifact(secondPage.Content, expectedPageNumber: 2);
    }

    [TestMethod]
    public async Task PreviewProviders_pdf_provider_rejects_oversize_or_corrupt_files()
    {
        var provider = new WindowsPdfPreviewProvider();
        using var pdf = ScratchFile.CreateBytes("document.pdf", MinimalPdfBytes());

        var tooLarge = await provider.PreviewAsync(
            Request(pdf.ToListedFileItem(length: 500 * 1024 * 1024L + 1)),
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, tooLarge.Status);
        Assert.AreEqual("pdf-too-large", tooLarge.ReasonCode);

        using var corrupt = ScratchFile.CreateText("corrupt.pdf", "not a pdf");
        var failed = await provider.PreviewAsync(
            Request(corrupt.ToListedFileItem()),
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Failed, failed.Status);
        Assert.AreEqual("pdf-corrupt", failed.ReasonCode);

        var accessDeniedProvider = new WindowsPdfPreviewProvider(
            new ThrowingPdfPageRenderer(new UnauthorizedAccessException()));
        var accessDenied = await accessDeniedProvider.PreviewAsync(
            Request(pdf.ToListedFileItem()),
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Failed, accessDenied.Status);
        Assert.AreEqual("access-denied", accessDenied.ReasonCode);
    }

    [TestMethod]
    public async Task PreviewProviders_provider_paths_do_not_modify_source_files()
    {
        var cases = new (IPreviewProvider Provider, PreviewOperation Operation, ScratchFile File)[]
        {
            (new WindowsImagePreviewProvider(), PreviewOperation.ImageDecode, ScratchFile.CreateBytes("image.png", await CreateBitmapBytesAsync(BitmapEncoder.PngEncoderId, width: 16, height: 16))),
            (new WindowsTextPreviewProvider(), PreviewOperation.TextReadAndEncodingDetection, ScratchFile.CreateText("notes.txt", "preview text marker")),
            (new WindowsPdfPreviewProvider(), PreviewOperation.PdfFirstPageRender, ScratchFile.CreateBytes("paper.pdf", MinimalPdfBytes()))
        };

        foreach (var (provider, operation, file) in cases)
        {
            using (file)
            {
                var before = file.Snapshot();

                var result = await provider.PreviewAsync(
                    Request(file.ToListedFileItem()),
                    Context(operation),
                    CancellationToken.None);

                Assert.AreEqual(PreviewProviderResultStatus.Success, result.Status);
                file.AssertUnchanged(before);
            }
        }
    }

    [TestMethod]
    public void PreviewProviders_default_factory_orders_content_providers_before_metadata_fallback()
    {
        var providers = WindowsPreviewProviderFactory.CreateDefault();

        CollectionAssert.AreEqual(
            new[]
            {
                typeof(WindowsImagePreviewProvider),
                typeof(WindowsTextPreviewProvider),
                typeof(WindowsPdfPreviewProvider),
                typeof(MetadataOnlyPreviewProvider)
            },
            providers.Select(provider => provider.GetType()).ToArray());
    }

    private static PreviewRequest Request(ListedFileItem item)
    {
        return new PreviewRequest(item, new PreviewMetadataProvider().GetMetadata(item));
    }

    private static PreviewProviderContext Context(PreviewOperation operation)
    {
        return new PreviewProviderContext(operation, TimeSpan.FromSeconds(5));
    }

    private static void AssertImageArtifact(PreviewContent? content, int expectedWidth, int expectedHeight)
    {
        Assert.IsNotNull(content?.ImageArtifact);
        Assert.AreEqual(expectedWidth, content.ImageArtifact.PixelWidth);
        Assert.AreEqual(expectedHeight, content.ImageArtifact.PixelHeight);
        Assert.AreEqual(expectedWidth, content.WidthPixels);
        Assert.AreEqual(expectedHeight, content.HeightPixels);
        Assert.AreEqual("png", content.ImageArtifact.EncodedFormat);
        Assert.IsNotEmpty(content.ImageArtifact.EncodedBytes);
    }

    private static void AssertPdfArtifact(PreviewContent? content, int expectedPageNumber)
    {
        Assert.IsNotNull(content?.PdfPageArtifact);
        Assert.AreEqual(expectedPageNumber, content.PdfPageArtifact.PageNumber);
        Assert.AreEqual(expectedPageNumber, content.PageNumber);
        Assert.AreEqual("png", content.PdfPageArtifact.EncodedFormat);
        Assert.IsNotEmpty(content.PdfPageArtifact.EncodedBytes);
    }

    private static async Task<byte[]> CreateBitmapBytesAsync(Guid encoderId, int width, int height)
    {
        using var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
        using var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);
        encoder.SetSoftwareBitmap(bitmap);
        await encoder.FlushAsync();
        return await ReadAllBytesAsync(stream);
    }

    private static async Task<byte[]> ReadAllBytesAsync(IRandomAccessStream stream)
    {
        stream.Seek(0);
        using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)stream.Size);
        var bytes = new byte[stream.Size];
        reader.ReadBytes(bytes);
        return bytes;
    }

    private static byte[] MinimalPdfBytes(int pageCount = 1)
    {
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(0, pageCount).Select(index => $"{3 + index * 2} 0 R"))}] /Count {pageCount} >>"
        };

        for (var index = 0; index < pageCount; index++)
        {
            var pageObjectNumber = 3 + index * 2;
            var contentObjectNumber = pageObjectNumber + 1;
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 120] /Contents {contentObjectNumber} 0 R /Resources << /Font << /F1 {3 + pageCount * 2} 0 R >> >> >>");
            var content = $"BT /F1 18 Tf 20 60 Td (Page {index + 1}) Tj ET";
            objects.Add($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        var builder = new StringBuilder();
        builder.Append("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n");
            builder.Append(objects[index]).Append('\n');
            builder.Append("endobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n");
        builder.Append("0 ").Append(objects.Count + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("0000000000")).Append(" 00000 n \n");
        }

        builder.Append("trailer\n");
        builder.Append("<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\n");
        builder.Append("startxref\n");
        builder.Append(xrefOffset).Append('\n');
        builder.Append("%%EOF\n");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private sealed class ScriptedImagePreviewDecoder : IImagePreviewDecoder
    {
        private readonly ImagePreviewArtifact _artifact;

        public ScriptedImagePreviewDecoder(ImagePreviewArtifact artifact)
        {
            _artifact = artifact;
        }

        public ValueTask<ImagePreviewArtifact> DecodeAsync(string path, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(_artifact);
        }
    }

    private sealed class RecordingPdfPageRenderer : IPdfPageRenderer
    {
        private readonly int _pageCount;

        public RecordingPdfPageRenderer(int pageCount)
        {
            _pageCount = pageCount;
        }

        public List<int> RequestedPages { get; } = [];

        public ValueTask<PdfPagePreviewArtifact> RenderPageAsync(
            string path,
            int pageNumber,
            CancellationToken cancellationToken)
        {
            RequestedPages.Add(pageNumber);
            return ValueTask.FromResult(new PdfPagePreviewArtifact(
                PageNumber: pageNumber,
                PageCount: _pageCount,
                PixelWidth: 200,
                PixelHeight: 120,
                EncodedFormat: "png",
                EncodedBytes: [137, 80, 78, 71],
                SourceWasDownsampled: false));
        }
    }

    private sealed class ThrowingImagePreviewDecoder : IImagePreviewDecoder
    {
        private readonly Exception _exception;

        public ThrowingImagePreviewDecoder(Exception exception)
        {
            _exception = exception;
        }

        public ValueTask<ImagePreviewArtifact> DecodeAsync(string path, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }

    private sealed class ThrowingPdfPageRenderer : IPdfPageRenderer
    {
        private readonly Exception _exception;

        public ThrowingPdfPageRenderer(Exception exception)
        {
            _exception = exception;
        }

        public ValueTask<PdfPagePreviewArtifact> RenderPageAsync(
            string path,
            int pageNumber,
            CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }

    private sealed class ScratchFile : IDisposable
    {
        private readonly string _root;

        private ScratchFile(string root, string path)
        {
            _root = root;
            Path = path;
        }

        public string Path { get; }

        public static ScratchFile CreateText(string fileName, string content)
        {
            var file = CreateEmpty(fileName);
            File.WriteAllText(file.Path, content, Encoding.UTF8);
            SetStableMetadata(file.Path);
            return file;
        }

        public static ScratchFile CreateBytes(string fileName, byte[] bytes)
        {
            var file = CreateEmpty(fileName);
            File.WriteAllBytes(file.Path, bytes);
            SetStableMetadata(file.Path);
            return file;
        }

        public ListedFileItem ToListedFileItem(long? length = null)
        {
            var info = new FileInfo(Path);
            return new ListedFileItem(
                Path,
                info.Name,
                info.Name,
                FileSystemEntryKind.File,
                length ?? info.Length,
                info.LastWriteTimeUtc,
                info.Attributes,
                IsHidden: false,
                IsProtectedOperatingSystemFile: false,
                IsVisuallyDimmed: false,
                CreationTimeUtc: info.CreationTimeUtc,
                LastAccessTimeUtc: info.LastAccessTimeUtc);
        }

        public FileSnapshot Snapshot()
        {
            var info = new FileInfo(Path);
            return new FileSnapshot(
                Length: info.Length,
                Sha256: Hash(Path),
                CreationTimeUtc: info.CreationTimeUtc,
                LastWriteTimeUtc: info.LastWriteTimeUtc,
                Attributes: info.Attributes);
        }

        public void AssertUnchanged(FileSnapshot before)
        {
            var after = Snapshot();
            Assert.AreEqual(before.Length, after.Length);
            Assert.AreEqual(before.Sha256, after.Sha256);
            Assert.AreEqual(before.CreationTimeUtc, after.CreationTimeUtc);
            Assert.AreEqual(before.LastWriteTimeUtc, after.LastWriteTimeUtc);
            Assert.AreEqual(before.Attributes, after.Attributes);
        }

        public void Dispose()
        {
            try
            {
                File.SetAttributes(Path, FileAttributes.Normal);
                Directory.Delete(_root, recursive: true);
            }
            catch
            {
            }
        }

        private static ScratchFile CreateEmpty(string fileName)
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "velofile-preview-provider-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new ScratchFile(root, System.IO.Path.Combine(root, fileName));
        }

        private static void SetStableMetadata(string path)
        {
            File.SetCreationTimeUtc(path, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
            File.SetLastWriteTimeUtc(path, new DateTime(2026, 1, 3, 4, 5, 6, DateTimeKind.Utc));
            File.SetAttributes(path, FileAttributes.Archive | FileAttributes.ReadOnly);
        }

        private static string Hash(string path)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
    }

    private sealed record FileSnapshot(
        long Length,
        string Sha256,
        DateTime CreationTimeUtc,
        DateTime LastWriteTimeUtc,
        FileAttributes Attributes);
}
