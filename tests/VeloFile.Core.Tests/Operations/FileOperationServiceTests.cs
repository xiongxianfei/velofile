using VeloFile.Core.Listing;
using VeloFile.Core.Operations;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Operations;

[TestClass]
[TestCategory("Operations")]
public sealed class FileOperationServiceTests
{
    [TestMethod]
    public async Task Rename_uses_the_file_operation_adapter_and_records_undo_eligibility()
    {
        var adapter = new RecordingFileOperationAdapter();
        var service = new FileOperationService(adapter);
        var item = Item(@"D:\scratch\old-name.txt", "old-name.txt");

        await service.RenameAsync(item, "new-name.txt");

        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.AreEqual(FileOperationKind.Rename, adapter.LastRequest?.Kind);
        Assert.AreEqual(item.FullPath, adapter.LastRequest?.Items.Single().Path);
        Assert.AreEqual("new-name.txt", adapter.LastRequest?.TargetName);
        Assert.IsTrue(service.State.UndoEligibility.CanUndo);
        Assert.AreEqual(FileOperationKind.Rename, service.State.UndoEligibility.OperationKind);
    }

    [TestMethod]
    public async Task Normal_delete_uses_recycle_bin_delete_when_supported()
    {
        var adapter = new RecordingFileOperationAdapter();
        var service = new FileOperationService(adapter);
        var item = Item(@"D:\scratch\delete-me.txt", "delete-me.txt");

        await service.DeleteToRecycleBinAsync([item]);

        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.AreEqual(FileOperationKind.RecycleBinDelete, adapter.LastRequest?.Kind);
        Assert.IsFalse(adapter.LastRequest!.ConfirmedPermanentDelete);
        Assert.IsNull(service.PendingPermanentDeleteConfirmation);
        Assert.IsTrue(service.State.UndoEligibility.CanUndo);
    }

    [TestMethod]
    public async Task Unsupported_recycle_bin_delete_requires_permanent_delete_confirmation_before_destructive_fallback()
    {
        var adapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.RecycleBinUnavailable("recycle-bin-unavailable")
        };
        var service = new FileOperationService(adapter);
        var item = Item(@"\\server\share\delete-me.txt", "delete-me.txt");

        await service.DeleteToRecycleBinAsync([item]);

        Assert.AreEqual(FileOperationStatus.WaitingForConfirmation, service.State.Status);
        Assert.AreEqual(FileOperationKind.PermanentDelete, service.PendingPermanentDeleteConfirmation?.Kind);
        Assert.AreEqual(PermanentDeleteReason.RecycleBinUnavailable, service.PendingPermanentDeleteConfirmation?.Reason);
        Assert.AreEqual(1, adapter.Requests.Count);

        await service.ConfirmPermanentDeleteAsync(confirm: true);

        Assert.AreEqual(2, adapter.Requests.Count);
        Assert.AreEqual(FileOperationKind.PermanentDelete, adapter.LastRequest?.Kind);
        Assert.IsTrue(adapter.LastRequest!.ConfirmedPermanentDelete);
        Assert.IsFalse(service.State.UndoEligibility.CanUndo);
    }

    [TestMethod]
    public async Task Permanent_delete_request_does_not_call_adapter_until_confirmation()
    {
        var adapter = new RecordingFileOperationAdapter();
        var service = new FileOperationService(adapter);
        var item = Item(@"D:\scratch\delete-me.txt", "delete-me.txt");

        service.RequestPermanentDelete([item], PermanentDeleteReason.UserGesture);

        Assert.AreEqual(FileOperationStatus.WaitingForConfirmation, service.State.Status);
        Assert.AreEqual(0, adapter.Requests.Count);

        await service.ConfirmPermanentDeleteAsync(confirm: false);

        Assert.AreEqual(FileOperationStatus.Cancelled, service.State.Status);
        Assert.AreEqual(0, adapter.Requests.Count);

        service.RequestPermanentDelete([item], PermanentDeleteReason.UserGesture);
        await service.ConfirmPermanentDeleteAsync(confirm: true);

        Assert.AreEqual(FileOperationKind.PermanentDelete, adapter.LastRequest?.Kind);
        Assert.IsTrue(adapter.LastRequest!.ConfirmedPermanentDelete);
        Assert.IsFalse(service.State.UndoEligibility.CanUndo);
    }

    [TestMethod]
    public async Task Adapter_progress_and_failure_are_visible_in_state()
    {
        var adapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.Failed("access-denied")
        };
        adapter.ProgressToReport = new FileOperationProgress(
            FileOperationKind.RecycleBinDelete,
            CompletedItemCount: 0,
            TotalItemCount: 1,
            StatusText: "Deleting");
        var service = new FileOperationService(adapter);

        await service.DeleteToRecycleBinAsync([Item(@"D:\scratch\denied.txt", "denied.txt")]);

        Assert.AreEqual(FileOperationStatus.Failed, service.State.Status);
        Assert.AreEqual("access-denied", service.State.ReasonCode);
        Assert.AreEqual(0, service.State.Progress.CompletedItemCount);
        Assert.AreEqual(1, service.State.Progress.TotalItemCount);
        Assert.IsFalse(service.State.UndoEligibility.CanUndo);
    }

    [TestMethod]
    public async Task Adapter_exception_becomes_visible_failure_without_escaping()
    {
        var adapter = new ThrowingFileOperationAdapter(new UnauthorizedAccessException());
        var service = new FileOperationService(adapter);

        await service.DeleteToRecycleBinAsync([Item(@"D:\scratch\denied.txt", "denied.txt")]);

        Assert.AreEqual(FileOperationStatus.Failed, service.State.Status);
        Assert.AreEqual("access-denied", service.State.ReasonCode);
    }

    [TestMethod]
    public async Task Operation_service_exposes_running_state_while_adapter_work_is_pending()
    {
        var adapter = new PendingFileOperationAdapter();
        var service = new FileOperationService(adapter);

        var operation = service.DeleteToRecycleBinAsync([Item(@"D:\scratch\slow-delete.txt", "slow-delete.txt")]);

        Assert.IsFalse(operation.IsCompleted);
        Assert.AreEqual(FileOperationStatus.Running, service.State.Status);
        Assert.AreEqual(FileOperationKind.RecycleBinDelete, adapter.LastRequest?.Kind);

        adapter.Complete(FileOperationAdapterResult.Completed(undoSupported: true));
        await operation;

        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
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

    private sealed class RecordingFileOperationAdapter : IFileOperationAdapter
    {
        public List<FileOperationRequest> Requests { get; } = [];

        public FileOperationRequest? LastRequest => Requests.LastOrDefault();

        public FileOperationAdapterResult NextResult { get; set; } = FileOperationAdapterResult.Completed(undoSupported: true);

        public FileOperationProgress? ProgressToReport { get; set; }

        public Task<FileOperationAdapterResult> ExecuteAsync(
            FileOperationRequest request,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (ProgressToReport is not null)
            {
                progress?.Report(ProgressToReport);
            }

            return Task.FromResult(NextResult);
        }
    }

    private sealed class PendingFileOperationAdapter : IFileOperationAdapter
    {
        private readonly TaskCompletionSource<FileOperationAdapterResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public FileOperationRequest? LastRequest { get; private set; }

        public Task<FileOperationAdapterResult> ExecuteAsync(
            FileOperationRequest request,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return _completion.Task;
        }

        public void Complete(FileOperationAdapterResult result)
        {
            _completion.SetResult(result);
        }
    }

    private sealed class ThrowingFileOperationAdapter : IFileOperationAdapter
    {
        private readonly Exception _exception;

        public ThrowingFileOperationAdapter(Exception exception)
        {
            _exception = exception;
        }

        public Task<FileOperationAdapterResult> ExecuteAsync(
            FileOperationRequest request,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}
