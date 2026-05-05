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
    public async Task Copy_uses_adapter_with_target_directory_and_does_not_offer_undo()
    {
        var adapter = new RecordingFileOperationAdapter();
        var service = new FileOperationService(adapter);
        var item = Item(@"D:\scratch\source\copy-me.txt", "copy-me.txt");

        await service.CopyAsync([item], @"D:\scratch\target");

        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.AreEqual(FileOperationKind.Copy, adapter.LastRequest?.Kind);
        Assert.AreEqual(@"D:\scratch\target", adapter.LastRequest?.TargetDirectory);
        Assert.IsFalse(service.State.UndoEligibility.CanUndo);
    }

    [TestMethod]
    public async Task Move_uses_adapter_with_target_directory_and_records_undo_eligibility()
    {
        var adapter = new RecordingFileOperationAdapter();
        var service = new FileOperationService(adapter);
        var item = Item(@"D:\scratch\source\move-me.txt", "move-me.txt");

        await service.MoveAsync([item], @"D:\scratch\target");

        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.AreEqual(FileOperationKind.Move, adapter.LastRequest?.Kind);
        Assert.AreEqual(@"D:\scratch\target", adapter.LastRequest?.TargetDirectory);
        Assert.IsTrue(service.State.UndoEligibility.CanUndo);
        Assert.AreEqual(FileOperationKind.Move, service.State.UndoEligibility.OperationKind);
    }

    [TestMethod]
    public async Task Copy_conflict_pauses_for_resolution_and_replace_resumes_operation()
    {
        var item = Item(@"D:\scratch\source\copy-me.txt", "copy-me.txt");
        var adapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(item)],
                    @"D:\scratch\target",
                    "copy-me.txt"))
        };
        var service = new FileOperationService(adapter);

        await service.CopyAsync([item], @"D:\scratch\target");

        Assert.AreEqual(FileOperationStatus.WaitingForConflict, service.State.Status);
        Assert.IsNotNull(service.PendingConflict);
        Assert.AreEqual("copy-me.txt", service.PendingConflict.ExistingName);
        Assert.AreEqual(1, adapter.Requests.Count);

        adapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await service.ResolveConflictAsync(FileOperationConflictChoice.Replace);

        Assert.AreEqual(2, adapter.Requests.Count);
        Assert.AreEqual(FileOperationConflictChoice.Replace, adapter.LastRequest?.ConflictChoice);
        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.IsNull(service.PendingConflict);
    }

    [TestMethod]
    public async Task Move_cancel_after_completed_item_records_partial_cancel_and_no_undo()
    {
        var adapter = new PendingFileOperationAdapter(supportsCancellation: true);
        var service = new FileOperationService(adapter);
        var items = new[]
        {
            Item(@"D:\scratch\source\one.txt", "one.txt"),
            Item(@"D:\scratch\source\two.txt", "two.txt"),
            Item(@"D:\scratch\source\three.txt", "three.txt")
        };
        adapter.ProgressToReport = new FileOperationProgress(
            FileOperationKind.Move,
            CompletedItemCount: 1,
            TotalItemCount: items.Length,
            StatusText: "Moved 1 of 3");

        var operation = service.MoveAsync(items, @"D:\scratch\target");

        Assert.IsFalse(operation.IsCompleted);
        Assert.AreEqual(FileOperationKind.Move, service.State.Kind);
        Assert.IsTrue(service.State.CanCancel);
        Assert.AreEqual(1, service.State.Progress.CompletedItemCount);
        Assert.AreEqual(3, service.State.Progress.TotalItemCount);

        service.CancelCurrentOperation();
        await operation;

        Assert.IsTrue(adapter.CancellationObserved);
        Assert.AreEqual(FileOperationStatus.Cancelled, service.State.Status);
        Assert.AreEqual(1, service.State.Progress.CompletedItemCount);
        Assert.AreEqual(3, service.State.Progress.TotalItemCount);
        Assert.IsFalse(service.State.UndoEligibility.CanUndo);
    }

    [TestMethod]
    public async Task Copy_conflict_resolution_routes_skip_choice()
    {
        var item = Item(@"D:\scratch\source\copy-me.txt", "copy-me.txt");
        var adapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(item)],
                    @"D:\scratch\target",
                    "copy-me.txt"))
        };
        var service = new FileOperationService(adapter);

        await service.CopyAsync([item], @"D:\scratch\target");

        Assert.AreEqual(FileOperationStatus.WaitingForConflict, service.State.Status);

        adapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await service.ResolveConflictAsync(FileOperationConflictChoice.Skip);

        Assert.AreEqual(2, adapter.Requests.Count);
        Assert.AreEqual(FileOperationConflictChoice.Skip, adapter.LastRequest?.ConflictChoice);
        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.IsNull(service.PendingConflict);
    }

    [TestMethod]
    public async Task Copy_conflict_resolution_routes_keep_both_choice()
    {
        var item = Item(@"D:\scratch\source\copy-me.txt", "copy-me.txt");
        var adapter = new RecordingFileOperationAdapter
        {
            NextResult = FileOperationAdapterResult.ConflictRequired(
                "name-conflict",
                new FileOperationConflict(
                    FileOperationKind.Copy,
                    [FileOperationTarget.FromListedItem(item)],
                    @"D:\scratch\target",
                    "copy-me.txt"))
        };
        var service = new FileOperationService(adapter);

        await service.CopyAsync([item], @"D:\scratch\target");

        Assert.AreEqual(FileOperationStatus.WaitingForConflict, service.State.Status);

        adapter.NextResult = FileOperationAdapterResult.Completed(undoSupported: false);
        await service.ResolveConflictAsync(FileOperationConflictChoice.KeepBoth);

        Assert.AreEqual(2, adapter.Requests.Count);
        Assert.AreEqual(FileOperationConflictChoice.KeepBoth, adapter.LastRequest?.ConflictChoice);
        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.IsNull(service.PendingConflict);
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
        Assert.IsTrue(service.State.CanCancel);
        Assert.AreEqual(FileOperationKind.RecycleBinDelete, adapter.LastRequest?.Kind);

        adapter.Complete(FileOperationAdapterResult.Completed(undoSupported: true));
        await operation;

        Assert.AreEqual(FileOperationStatus.Completed, service.State.Status);
        Assert.IsFalse(service.State.CanCancel);
    }

    [TestMethod]
    public async Task Operation_service_cancel_current_operation_signals_in_flight_token_and_records_cancelled_state()
    {
        var adapter = new PendingFileOperationAdapter(supportsCancellation: true);
        var service = new FileOperationService(adapter);

        var operation = service.DeleteToRecycleBinAsync([Item(@"D:\scratch\slow-delete.txt", "slow-delete.txt")]);

        Assert.IsFalse(operation.IsCompleted);
        Assert.IsTrue(service.State.CanCancel);

        service.CancelCurrentOperation();
        await operation;

        Assert.IsTrue(adapter.CancellationObserved);
        Assert.AreEqual(FileOperationStatus.Cancelled, service.State.Status);
        Assert.IsFalse(service.State.CanCancel);
    }

    [TestMethod]
    public async Task Operation_service_hides_cancel_when_adapter_does_not_support_cancellation()
    {
        var adapter = new PendingFileOperationAdapter(supportsCancellation: false);
        var service = new FileOperationService(adapter);

        var operation = service.DeleteToRecycleBinAsync([Item(@"D:\scratch\slow-delete.txt", "slow-delete.txt")]);

        Assert.IsFalse(operation.IsCompleted);
        Assert.IsFalse(service.State.CanCancel);

        service.CancelCurrentOperation();
        adapter.Complete(FileOperationAdapterResult.Completed(undoSupported: true));
        await operation;

        Assert.IsFalse(adapter.CancellationObserved);
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

    private sealed class PendingFileOperationAdapter : IFileOperationAdapter, ICancellableFileOperationAdapter
    {
        private readonly TaskCompletionSource<FileOperationAdapterResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly bool _supportsCancellation;

        public PendingFileOperationAdapter(bool supportsCancellation = true)
        {
            _supportsCancellation = supportsCancellation;
        }

        public FileOperationRequest? LastRequest { get; private set; }

        public bool CancellationObserved { get; private set; }

        public FileOperationProgress? ProgressToReport { get; set; }

        public bool CanCancel(FileOperationRequest request)
        {
            return _supportsCancellation;
        }

        public Task<FileOperationAdapterResult> ExecuteAsync(
            FileOperationRequest request,
            IProgress<FileOperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (ProgressToReport is not null)
            {
                progress?.Report(ProgressToReport);
            }

            cancellationToken.Register(() =>
            {
                CancellationObserved = true;
                _completion.TrySetResult(FileOperationAdapterResult.Cancelled());
            });
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
