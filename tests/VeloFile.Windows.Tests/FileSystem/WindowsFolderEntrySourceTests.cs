using VeloFile.Core.Listing;
using VeloFile.Windows.FileSystem;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.FileSystem;

[TestClass]
[TestCategory("Listing")]
public sealed class WindowsListingFolderEntrySourceTests
{
    [TestMethod]
    public async Task Windows_folder_entry_source_enumerates_local_entries_with_kind_size_and_attributes()
    {
        using var workspace = TemporaryWorkspace.Create();
        var filePath = Path.Combine(workspace.Root, "readme.txt");
        var hiddenPath = Path.Combine(workspace.Root, "hidden.txt");
        var directoryPath = Path.Combine(workspace.Root, "src");
        await File.WriteAllTextAsync(filePath, "hello");
        await File.WriteAllTextAsync(hiddenPath, "hidden");
        Directory.CreateDirectory(directoryPath);
        File.SetAttributes(hiddenPath, File.GetAttributes(hiddenPath) | FileAttributes.Hidden);

        var source = new WindowsFolderEntrySource();

        var entries = await ToListAsync(source.EnumerateAsync(workspace.Root, CancellationToken.None));

        var file = entries.Single(entry => entry.Name == "readme.txt");
        var hidden = entries.Single(entry => entry.Name == "hidden.txt");
        var directory = entries.Single(entry => entry.Name == "src");
        Assert.AreEqual(FileSystemEntryKind.File, file.Kind);
        Assert.AreEqual(5, file.Length);
        Assert.AreEqual(FileSystemEntryKind.Directory, directory.Kind);
        Assert.IsTrue(hidden.IsHidden);
    }

    [TestMethod]
    public async Task Windows_folder_entry_source_accepts_extended_local_paths()
    {
        using var workspace = TemporaryWorkspace.Create();
        await File.WriteAllTextAsync(Path.Combine(workspace.Root, "readme.txt"), "hello");
        var extendedPath = @"\\?\" + Path.GetFullPath(workspace.Root);
        var source = new WindowsFolderEntrySource();

        var entries = await ToListAsync(source.EnumerateAsync(extendedPath, CancellationToken.None));

        Assert.IsTrue(entries.Any(entry => entry.Name == "readme.txt"));
    }

    [TestMethod]
    public async Task Windows_folder_entry_source_observes_cancellation_before_enumeration()
    {
        using var workspace = TemporaryWorkspace.Create();
        await File.WriteAllTextAsync(Path.Combine(workspace.Root, "readme.txt"), "hello");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var source = new WindowsFolderEntrySource();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in source.EnumerateAsync(workspace.Root, cancellation.Token))
            {
            }
        });
    }

    [TestMethod]
    public void Windows_drive_entry_source_returns_drive_entries_with_optional_space_hints()
    {
        var source = new WindowsDriveEntrySource();

        var drives = source.GetDrives();

        Assert.IsTrue(drives.Count > 0);
        Assert.IsTrue(drives.All(drive => !string.IsNullOrWhiteSpace(drive.RootPath)));
        Assert.IsTrue(drives.All(drive => drive.HintStatus == DriveHintStatus.NotRequested));
        Assert.IsTrue(drives.All(drive => drive.AvailableFreeSpaceBytes is null));
        Assert.IsTrue(drives.All(drive => drive.TotalSizeBytes is null));
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var results = new List<T>();
        await foreach (var item in source)
        {
            results.Add(item);
        }

        return results;
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TemporaryWorkspace Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "velofile-windows-listing-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TemporaryWorkspace(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                var hiddenPath = Path.Combine(Root, "hidden.txt");
                if (File.Exists(hiddenPath))
                {
                    File.SetAttributes(hiddenPath, FileAttributes.Normal);
                }

                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
