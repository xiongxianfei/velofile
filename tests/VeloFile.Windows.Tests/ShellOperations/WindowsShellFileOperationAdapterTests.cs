using VeloFile.Core.Listing;
using VeloFile.Core.Operations;
using VeloFile.Windows.Shell;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.ShellOperations;

[TestClass]
[TestCategory("Operations")]
public sealed class WindowsShellFileOperationAdapterTests
{
    [TestMethod]
    public void Recycle_bin_delete_maps_to_shell_recycle_intent_not_permanent_delete()
    {
        var request = FileOperationRequest.RecycleBinDelete([Item(@"D:\scratch\delete-me.txt", "delete-me.txt")]);

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.Delete, intent.Kind);
        Assert.AreEqual(WindowsShellDeleteDisposition.RecycleBin, intent.DeleteDisposition);
        Assert.IsFalse(intent.AllowUndoBypassingDelete);
    }

    [TestMethod]
    public void Permanent_delete_requires_confirmed_permanent_delete_request()
    {
        var unconfirmed = new FileOperationRequest(
            FileOperationKind.PermanentDelete,
            [FileOperationTarget.FromListedItem(Item(@"D:\scratch\delete-me.txt", "delete-me.txt"))],
            TargetName: null,
            ConfirmedPermanentDelete: false);

        Assert.ThrowsExactly<InvalidOperationException>(() => WindowsShellFileOperationRequestMapper.Map(unconfirmed));

        var confirmed = FileOperationRequest.PermanentDelete(
            [Item(@"D:\scratch\delete-me.txt", "delete-me.txt")],
            confirmed: true);
        var intent = WindowsShellFileOperationRequestMapper.Map(confirmed);

        Assert.AreEqual(WindowsShellDeleteDisposition.Permanent, intent.DeleteDisposition);
    }

    [TestMethod]
    public void Rename_maps_to_shell_rename_intent_with_target_name()
    {
        var request = FileOperationRequest.Rename(Item(@"D:\scratch\old.txt", "old.txt"), "new.txt");

        var intent = WindowsShellFileOperationRequestMapper.Map(request);

        Assert.AreEqual(WindowsShellFileOperationKind.Rename, intent.Kind);
        Assert.AreEqual("new.txt", intent.TargetName);
        Assert.AreEqual(@"D:\scratch\old.txt", intent.Targets.Single().Path);
    }

    [TestMethod]
    public async Task Recycle_bin_delete_for_unc_path_returns_unavailable_without_executing_delete()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor();
        var adapter = new WindowsShellFileOperationAdapter(executor: executor);
        var request = FileOperationRequest.RecycleBinDelete([Item(@"\\server\share\delete-me.txt", "delete-me.txt")]);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.RecycleBinUnavailable, result.Status);
        Assert.AreEqual(0, executor.Intents.Count);
    }

    [TestMethod]
    public async Task Recycle_bin_probe_unsupported_result_returns_unavailable_without_executing_delete()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor();
        var adapter = new WindowsShellFileOperationAdapter(
            new StaticRecycleBinCapabilityProbe(RecycleBinCapability.NotRecyclable),
            executor);
        var request = FileOperationRequest.RecycleBinDelete([Item(@"D:\scratch\delete-me.txt", "delete-me.txt")]);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.RecycleBinUnavailable, result.Status);
        Assert.AreEqual(0, executor.Intents.Count);
    }

    [TestMethod]
    public async Task Ambiguous_recycle_bin_delete_failure_returns_failed_without_permanent_delete_fallback()
    {
        var executor = new RecordingWindowsShellFileOperationExecutor
        {
            ExceptionToThrow = new IOException("ambiguous shell failure")
        };
        var adapter = new WindowsShellFileOperationAdapter(
            new StaticRecycleBinCapabilityProbe(RecycleBinCapability.Unknown),
            executor);
        var request = FileOperationRequest.RecycleBinDelete([Item(@"D:\scratch\delete-me.txt", "delete-me.txt")]);

        var result = await adapter.ExecuteAsync(request, progress: null, CancellationToken.None);

        Assert.AreEqual(FileOperationAdapterResultStatus.Failed, result.Status);
        Assert.AreEqual(1, executor.Intents.Count);
        Assert.AreEqual(WindowsShellDeleteDisposition.RecycleBin, executor.Intents.Single().DeleteDisposition);
        Assert.AreNotEqual(WindowsShellDeleteDisposition.Permanent, executor.Intents.Single().DeleteDisposition);
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

    private sealed class StaticRecycleBinCapabilityProbe : IRecycleBinCapabilityProbe
    {
        private readonly RecycleBinCapability _capability;

        public StaticRecycleBinCapabilityProbe(RecycleBinCapability capability)
        {
            _capability = capability;
        }

        public RecycleBinCapability GetCapability(IReadOnlyList<FileOperationTarget> targets)
        {
            return _capability;
        }
    }

    private sealed class RecordingWindowsShellFileOperationExecutor : IWindowsShellFileOperationExecutor
    {
        public List<WindowsShellFileOperationIntent> Intents { get; } = [];

        public Exception? ExceptionToThrow { get; set; }

        public void Execute(
            WindowsShellFileOperationIntent intent,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            Intents.Add(intent);
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
        }
    }
}
