using System.Runtime.CompilerServices;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Listing;

[TestClass]
[TestCategory("Listing")]
public sealed class ListingTests
{
    [TestMethod]
    public async Task Listing_service_returns_first_viewport_without_draining_large_folder()
    {
        var source = new CountingFolderEntrySource(100_000);
        var service = new FolderListingService(source);

        var state = await service.LoadFirstViewportAsync(
            @"D:\large",
            new FolderListingOptions(ViewportItemCount: 25, VisibilitySettings.Default));

        Assert.AreEqual(FolderListingStatus.Ready, state.Status);
        Assert.AreEqual(25, state.FirstViewport.Count);
        Assert.IsFalse(state.IsComplete);
        Assert.IsTrue(source.YieldedCount <= 25, $"Expected first-viewport enumeration only, yielded {source.YieldedCount} items.");
        Assert.AreEqual("file-000000.txt", state.FirstViewport[0].DisplayName);
    }

    [TestMethod]
    public async Task Listing_service_reports_empty_state_without_treating_folder_as_error()
    {
        var service = new FolderListingService(new StaticFolderEntrySource([]));

        var state = await service.LoadFirstViewportAsync(
            @"D:\empty",
            new FolderListingOptions(ViewportItemCount: 25, VisibilitySettings.Default));

        Assert.AreEqual(FolderListingStatus.Empty, state.Status);
        Assert.AreEqual(0, state.FirstViewport.Count);
        Assert.IsTrue(state.IsComplete);
        Assert.IsNull(state.ReasonCode);
    }

    [TestMethod]
    public async Task Listing_service_returns_recoverable_failure_state_and_preserves_previous_valid_listing()
    {
        var previous = FolderListingState.Ready(
            @"D:\valid",
            [
                new ListedFileItem(
                    FullPath: @"D:\valid\README.md",
                    Name: "README.md",
                    DisplayName: "README.md",
                    Kind: FileSystemEntryKind.File,
                    Length: 10,
                    LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
                    Attributes: FileAttributes.Archive,
                    IsHidden: false,
                    IsProtectedOperatingSystemFile: false,
                    IsVisuallyDimmed: false)
            ],
            knownItemCount: 1,
            isComplete: true);
        var diagnostics = new CollectingDiagnosticSink();
        var service = new FolderListingService(
            new ThrowingFolderEntrySource(new UnauthorizedAccessException(@"C:\Users\alice\Documents\secret-plan")),
            diagnostics,
            new PathRedactor(Convert.FromHexString("00112233445566778899AABBCCDDEEFF")));

        var state = await service.LoadFirstViewportAsync(
            @"C:\Users\alice\Documents\secret-plan",
            new FolderListingOptions(ViewportItemCount: 25, VisibilitySettings.Default),
            previous);

        Assert.AreEqual(FolderListingStatus.AccessDenied, state.Status);
        Assert.AreEqual("access-denied", state.ReasonCode);
        Assert.AreSame(previous, state.PreviousValidState);

        var json = DiagnosticJsonSerializer.Serialize(diagnostics.Events.Single());
        StringAssert.Contains(json, "\"component\":\"navigation\"");
        Assert.IsFalse(json.Contains("alice", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(json.Contains("secret-plan", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Listing_service_distinguishes_invalid_and_unavailable_paths()
    {
        var service = new FolderListingService(new ThrowingFolderEntrySource(new DirectoryNotFoundException()));

        var invalid = await service.LoadFirstViewportAsync(
            "",
            new FolderListingOptions(ViewportItemCount: 25, VisibilitySettings.Default));
        var unavailable = await service.LoadFirstViewportAsync(
            @"D:\missing",
            new FolderListingOptions(ViewportItemCount: 25, VisibilitySettings.Default));

        Assert.AreEqual(FolderListingStatus.InvalidPath, invalid.Status);
        Assert.AreEqual("invalid-path", invalid.ReasonCode);
        Assert.AreEqual(FolderListingStatus.Unavailable, unavailable.Status);
        Assert.AreEqual("missing", unavailable.ReasonCode);
    }

    [TestMethod]
    public void Listing_request_gate_accepts_only_current_request_per_tab()
    {
        var gate = new FolderListingRequestGate();

        var slowTabRequest = gate.StartRequest("tab-a");
        var healthyTabRequest = gate.StartRequest("tab-b");
        var newerTabRequest = gate.StartRequest("tab-a");

        Assert.IsFalse(gate.IsCurrent(slowTabRequest));
        Assert.IsTrue(gate.IsCurrent(newerTabRequest));
        Assert.IsTrue(gate.IsCurrent(healthyTabRequest));
    }

    [TestMethod]
    public void Drive_entries_preserve_free_space_hints_when_available()
    {
        var drive = new DriveEntry(
            Name: "Local Disk",
            RootPath: @"C:\",
            DriveType: DriveType.Fixed,
            IsReady: true,
            AvailableFreeSpaceBytes: 10_000,
            TotalSizeBytes: 100_000);

        Assert.AreEqual(@"C:\", drive.RootPath);
        Assert.AreEqual(10_000, drive.AvailableFreeSpaceBytes);
        Assert.AreEqual(100_000, drive.TotalSizeBytes);
    }

    [TestMethod]
    public void File_list_view_modes_cover_v1_modes()
    {
        CollectionAssert.AreEquivalent(
            new[] { FileListViewMode.Details, FileListViewMode.List, FileListViewMode.LargeIcons },
            Enum.GetValues<FileListViewMode>());
    }

    private sealed class CountingFolderEntrySource : IFolderEntrySource
    {
        private readonly int _count;

        public CountingFolderEntrySource(int count)
        {
            _count = count;
        }

        public int YieldedCount { get; private set; }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();

            for (var i = 0; i < _count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                YieldedCount++;
                yield return new FileSystemEntrySnapshot(
                    FullPath: Path.Combine(path, $"file-{i:000000}.txt"),
                    Name: $"file-{i:000000}.txt",
                    Kind: FileSystemEntryKind.File,
                    Length: i,
                    LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
                    Attributes: FileAttributes.Archive);
            }
        }
    }

    private sealed class StaticFolderEntrySource : IFolderEntrySource
    {
        private readonly IReadOnlyList<FileSystemEntrySnapshot> _entries;

        public StaticFolderEntrySource(IReadOnlyList<FileSystemEntrySnapshot> entries)
        {
            _entries = entries;
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();

            foreach (var entry in _entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }

    private sealed class ThrowingFolderEntrySource : IFolderEntrySource
    {
        private readonly Exception _exception;

        public ThrowingFolderEntrySource(Exception exception)
        {
            _exception = exception;
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            if (_exception is not null)
            {
                throw _exception;
            }

            yield break;
        }
    }
}
