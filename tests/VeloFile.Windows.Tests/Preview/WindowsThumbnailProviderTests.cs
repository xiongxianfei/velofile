using VeloFile.Core.Listing;
using VeloFile.Core.Preview;
using VeloFile.Windows.Preview;

namespace VeloFile.Windows.Tests.Preview;

[TestClass]
[TestCategory("Thumbnails")]
public sealed class WindowsThumbnailProviderTests
{
    [TestMethod]
    public async Task Thumbnails_windows_provider_returns_thumbnail_or_generic_icon_for_existing_file()
    {
        using var scratch = ScratchFile.CreateText("notes.txt", "thumbnail source");
        var provider = new WindowsThumbnailProvider();

        var result = await provider.GenerateAsync(
            scratch.ToListedFileItem(),
            new ThumbnailProviderContext(TimeSpan.FromMilliseconds(500)),
            CancellationToken.None);

        Assert.IsTrue(result.Status is ThumbnailProviderResultStatus.Success or ThumbnailProviderResultStatus.GenericIcon);
        Assert.IsNotNull(result.Artifact);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Artifact.DisplayText));
        scratch.AssertUnchanged();
    }

    [TestMethod]
    public async Task Thumbnails_windows_provider_caches_generic_icons_by_extension_class()
    {
        var provider = new WindowsThumbnailProvider();
        using var first = ScratchFile.CreateText("first.unknownthumb", "one");
        using var second = ScratchFile.CreateText("second.unknownthumb", "two");

        var firstResult = await provider.GenerateAsync(
            first.ToListedFileItem(kind: FileSystemEntryKind.Other),
            new ThumbnailProviderContext(TimeSpan.FromMilliseconds(1)),
            CancellationToken.None);
        var secondResult = await provider.GenerateAsync(
            second.ToListedFileItem(kind: FileSystemEntryKind.Other),
            new ThumbnailProviderContext(TimeSpan.FromMilliseconds(1)),
            CancellationToken.None);

        Assert.AreEqual(ThumbnailProviderResultStatus.GenericIcon, firstResult.Status);
        Assert.AreEqual(ThumbnailProviderResultStatus.GenericIcon, secondResult.Status);
        Assert.AreEqual(firstResult.Artifact?.DisplayText, secondResult.Artifact?.DisplayText);
        Assert.AreEqual(1, provider.CachedGenericIconCount);
    }

    private sealed class ScratchFile : IDisposable
    {
        private readonly string _root;
        private readonly FileSnapshot _snapshot;

        private ScratchFile(string root, string path)
        {
            _root = root;
            Path = path;
            _snapshot = Snapshot(path);
        }

        public string Path { get; }

        public static ScratchFile CreateText(string fileName, string content)
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "velofile-thumbnail-provider-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var path = System.IO.Path.Combine(root, fileName);
            File.WriteAllText(path, content);
            File.SetLastWriteTimeUtc(path, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
            File.SetAttributes(path, FileAttributes.Archive);
            return new ScratchFile(root, path);
        }

        public ListedFileItem ToListedFileItem(FileSystemEntryKind kind = FileSystemEntryKind.File)
        {
            var info = new FileInfo(Path);
            return new ListedFileItem(
                Path,
                info.Name,
                info.Name,
                kind,
                info.Length,
                info.LastWriteTimeUtc,
                info.Attributes,
                IsHidden: false,
                IsProtectedOperatingSystemFile: false,
                IsVisuallyDimmed: false,
                CreationTimeUtc: info.CreationTimeUtc,
                LastAccessTimeUtc: info.LastAccessTimeUtc);
        }

        public void AssertUnchanged()
        {
            Assert.AreEqual(_snapshot, Snapshot(Path));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch
            {
            }
        }

        private static FileSnapshot Snapshot(string path)
        {
            var info = new FileInfo(path);
            return new FileSnapshot(info.Length, info.LastWriteTimeUtc, info.Attributes);
        }
    }

    private sealed record FileSnapshot(
        long Length,
        DateTime LastWriteTimeUtc,
        FileAttributes Attributes);
}
