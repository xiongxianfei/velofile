using System.Security.Cryptography;
using System.Text;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;
using VeloFile.Windows.Preview;

namespace VeloFile.Windows.Tests.Preview;

[TestClass]
[TestCategory("PreviewProviders")]
public sealed class WindowsPreviewProviderTests
{
    [TestMethod]
    public async Task PreviewProviders_image_provider_accepts_png_and_rejects_oversize_or_huge_dimensions()
    {
        using var normal = ScratchFile.CreateBytes("normal.png", PngBytes(width: 32, height: 24));
        var provider = new WindowsImagePreviewProvider();

        var success = await provider.PreviewAsync(
            Request(normal.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, success.Status);
        Assert.AreEqual(PreviewContentKind.Image, success.Content?.Kind);
        Assert.AreEqual(32, success.Content?.WidthPixels);
        Assert.AreEqual(24, success.Content?.HeightPixels);

        using var jpeg = ScratchFile.CreateBytes("photo.jpg", JpegBytes(width: 40, height: 30));
        var jpegSuccess = await provider.PreviewAsync(
            Request(jpeg.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Success, jpegSuccess.Status);
        Assert.AreEqual(PreviewContentKind.Image, jpegSuccess.Content?.Kind);
        Assert.AreEqual(40, jpegSuccess.Content?.WidthPixels);
        Assert.AreEqual(30, jpegSuccess.Content?.HeightPixels);

        var tooLarge = await provider.PreviewAsync(
            Request(normal.ToListedFileItem(length: 100 * 1024 * 1024L + 1)),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, tooLarge.Status);
        Assert.AreEqual("image-too-large", tooLarge.ReasonCode);

        using var hugeDimensions = ScratchFile.CreateBytes("huge.png", PngBytes(width: 8193, height: 1));
        var huge = await provider.PreviewAsync(
            Request(hugeDimensions.ToListedFileItem()),
            Context(PreviewOperation.ImageDecode),
            CancellationToken.None);
        Assert.AreEqual(PreviewProviderResultStatus.Unsupported, huge.Status);
        Assert.AreEqual("image-dimensions-too-large", huge.ReasonCode);
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
    public async Task PreviewProviders_pdf_provider_returns_first_page_and_rejects_oversize_or_corrupt_files()
    {
        var provider = new WindowsPdfPreviewProvider();
        using var pdf = ScratchFile.CreateBytes("document.pdf", MinimalPdfBytes());

        var success = await provider.PreviewAsync(
            Request(pdf.ToListedFileItem()),
            Context(PreviewOperation.PdfFirstPageRender),
            CancellationToken.None);

        Assert.AreEqual(PreviewProviderResultStatus.Success, success.Status);
        Assert.AreEqual(PreviewContentKind.Pdf, success.Content?.Kind);
        StringAssert.Contains(success.Content?.TextContent ?? "", "Page 1");

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
    }

    [TestMethod]
    public async Task PreviewProviders_provider_paths_do_not_modify_source_files()
    {
        var cases = new (IPreviewProvider Provider, PreviewOperation Operation, ScratchFile File)[]
        {
            (new WindowsImagePreviewProvider(), PreviewOperation.ImageDecode, ScratchFile.CreateBytes("image.png", PngBytes(width: 16, height: 16))),
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

    private static byte[] PngBytes(int width, int height)
    {
        var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAFgwJ/l3EX7wAAAABJRU5ErkJggg==");
        WriteBigEndian(bytes, 16, width);
        WriteBigEndian(bytes, 20, height);
        return bytes;
    }

    private static byte[] JpegBytes(int width, int height)
    {
        return
        [
            0xff, 0xd8,
            0xff, 0xe0, 0x00, 0x10,
            0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00,
            0x00, 0x01, 0x00, 0x01, 0x00, 0x00,
            0xff, 0xc0, 0x00, 0x11,
            0x08,
            (byte)((height >> 8) & 0xff),
            (byte)(height & 0xff),
            (byte)((width >> 8) & 0xff),
            (byte)(width & 0xff),
            0x03,
            0x01, 0x11, 0x00,
            0x02, 0x11, 0x00,
            0x03, 0x11, 0x00,
            0xff, 0xd9
        ];
    }

    private static void WriteBigEndian(byte[] bytes, int offset, int value)
    {
        bytes[offset] = (byte)((value >> 24) & 0xff);
        bytes[offset + 1] = (byte)((value >> 16) & 0xff);
        bytes[offset + 2] = (byte)((value >> 8) & 0xff);
        bytes[offset + 3] = (byte)(value & 0xff);
    }

    private static byte[] MinimalPdfBytes()
    {
        return Encoding.ASCII.GetBytes("""
            %PDF-1.4
            1 0 obj
            << /Type /Catalog /Pages 2 0 R >>
            endobj
            2 0 obj
            << /Type /Pages /Kids [3 0 R] /Count 1 >>
            endobj
            3 0 obj
            << /Type /Page /Parent 2 0 R /MediaBox [0 0 72 72] >>
            endobj
            trailer
            << /Root 1 0 R >>
            %%EOF
            """);
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
