using VeloFile.Core.FileAssociations;
using VeloFile.Core.Listing;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.FileAssociations;

[TestClass]
[TestCategory("FileAssociations")]
public sealed class FileAssociationLaunchServiceTests
{
    [TestMethod]
    public async Task Open_uses_default_file_association_without_mutating_associations()
    {
        var adapter = new RecordingFileAssociationLaunchAdapter();
        var service = new FileAssociationLaunchService(adapter);
        var item = Item(@"D:\scratch\report.txt", "report.txt");

        var result = await service.OpenAsync(item);

        Assert.AreEqual(FileAssociationLaunchStatus.Succeeded, result.Status);
        Assert.AreEqual(FileAssociationLaunchKind.Open, adapter.Requests.Single().Kind);
        Assert.AreEqual(item.FullPath, adapter.Requests.Single().Path);
        Assert.IsFalse(adapter.Requests.Single().ModifySystemAssociations);
    }

    [TestMethod]
    public async Task Open_with_uses_open_with_route_without_mutating_associations()
    {
        var adapter = new RecordingFileAssociationLaunchAdapter();
        var service = new FileAssociationLaunchService(adapter);
        var item = Item(@"D:\scratch\report.txt", "report.txt");

        var result = await service.OpenWithAsync(item);

        Assert.AreEqual(FileAssociationLaunchStatus.Succeeded, result.Status);
        Assert.AreEqual(FileAssociationLaunchKind.OpenWith, adapter.Requests.Single().Kind);
        Assert.AreEqual(item.FullPath, adapter.Requests.Single().Path);
        Assert.IsFalse(adapter.Requests.Single().ModifySystemAssociations);
    }

    [TestMethod]
    public async Task Broken_association_returns_recoverable_failure()
    {
        var adapter = new RecordingFileAssociationLaunchAdapter
        {
            NextResult = FileAssociationLaunchResult.Failed(FileAssociationLaunchKind.Open, "association-launch-failed")
        };
        var service = new FileAssociationLaunchService(adapter);

        var result = await service.OpenAsync(Item(@"D:\scratch\broken.xyz", "broken.xyz"));

        Assert.AreEqual(FileAssociationLaunchStatus.Failed, result.Status);
        Assert.AreEqual("association-launch-failed", result.ReasonCode);
    }

    private static ListedFileItem Item(string fullPath, string name)
    {
        return new ListedFileItem(
            fullPath,
            name,
            name,
            FileSystemEntryKind.File,
            Length: 1,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private sealed class RecordingFileAssociationLaunchAdapter : IFileAssociationLaunchAdapter
    {
        public List<FileAssociationLaunchRequest> Requests { get; } = [];

        public FileAssociationLaunchResult NextResult { get; set; } =
            FileAssociationLaunchResult.Succeeded(FileAssociationLaunchKind.Open);

        public Task<FileAssociationLaunchResult> LaunchAsync(
            FileAssociationLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(NextResult with { Kind = request.Kind });
        }
    }
}
