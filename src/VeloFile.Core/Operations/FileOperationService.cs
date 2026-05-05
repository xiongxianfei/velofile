using VeloFile.Core;
using VeloFile.Core.Listing;

namespace VeloFile.Core.Operations;

public sealed class FileOperationService
{
    private readonly IFileOperationAdapter _adapter;
    private CancellationTokenSource? _currentOperationCancellation;

    public FileOperationService(IFileOperationAdapter adapter)
    {
        _adapter = adapter;
    }

    public event EventHandler? StateChanged;

    public FileOperationState State { get; private set; } = FileOperationState.Idle;

    public PermanentDeleteConfirmationRequest? PendingPermanentDeleteConfirmation { get; private set; }

    public FileOperationConflict? PendingConflict { get; private set; }

    public bool CanCancelCurrentOperation => State.CanCancel;

    public async Task CopyAsync(
        IReadOnlyList<ListedFileItem> items,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        EnsureSelection(items);
        EnsureTargetDirectory(targetDirectory);

        await ExecuteAdapterRequestAsync(FileOperationRequest.Copy(items, targetDirectory.Trim()), cancellationToken).ConfigureAwait(false);
    }

    public async Task MoveAsync(
        IReadOnlyList<ListedFileItem> items,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        EnsureSelection(items);
        EnsureTargetDirectory(targetDirectory);

        await ExecuteAdapterRequestAsync(FileOperationRequest.Move(items, targetDirectory.Trim()), cancellationToken).ConfigureAwait(false);
    }

    public async Task RenameAsync(ListedFileItem item, string targetName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(targetName))
        {
            throw new ArgumentException("A target name is required.", nameof(targetName));
        }

        await ExecuteAdapterRequestAsync(FileOperationRequest.Rename(item, targetName.Trim()), cancellationToken).ConfigureAwait(false);
    }

    public async Task ResolveConflictAsync(
        FileOperationConflictChoice choice,
        CancellationToken cancellationToken = default)
    {
        var conflict = PendingConflict;
        if (conflict is null)
        {
            return;
        }

        PendingConflict = null;
        var request = new FileOperationRequest(
            conflict.Kind,
            conflict.Items,
            TargetName: null,
            ConfirmedPermanentDelete: false,
            conflict.TargetDirectory,
            choice);
        await ExecuteAdapterRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteToRecycleBinAsync(IReadOnlyList<ListedFileItem> items, CancellationToken cancellationToken = default)
    {
        EnsureSelection(items);
        var request = FileOperationRequest.RecycleBinDelete(items);
        var result = await ExecuteAdapterRequestAsync(request, cancellationToken).ConfigureAwait(false);

        if (result.Status is FileOperationAdapterResultStatus.RecycleBinUnavailable)
        {
            RequestPermanentDelete(items, PermanentDeleteReason.RecycleBinUnavailable, result.ReasonCode);
        }
    }

    public void RequestPermanentDelete(
        IReadOnlyList<ListedFileItem> items,
        PermanentDeleteReason reason,
        string? reasonCode = null)
    {
        EnsureSelection(items);
        var confirmation = new PermanentDeleteConfirmationRequest(
            FileOperationKind.PermanentDelete,
            reason,
            items.Select(FileOperationTarget.FromListedItem).ToArray());

        PendingPermanentDeleteConfirmation = confirmation;
        SetState(FileOperationState.WaitingForConfirmation(confirmation, reasonCode));
    }

    public async Task ConfirmPermanentDeleteAsync(bool confirm, CancellationToken cancellationToken = default)
    {
        var confirmation = PendingPermanentDeleteConfirmation;
        if (confirmation is null)
        {
            return;
        }

        PendingPermanentDeleteConfirmation = null;
        if (!confirm)
        {
            SetState(State with
            {
                Status = FileOperationStatus.Cancelled,
                ReasonCode = "confirmation-cancelled",
                UndoEligibility = FileOperationUndoEligibility.None,
                CanCancel = false
            });
            return;
        }

        var request = new FileOperationRequest(
            FileOperationKind.PermanentDelete,
            confirmation.Items,
            TargetName: null,
            ConfirmedPermanentDelete: true);
        await ExecuteAdapterRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public void CancelCurrentOperation()
    {
        if (_currentOperationCancellation is null || !State.CanCancel)
        {
            return;
        }

        _currentOperationCancellation.Cancel();
        SetState(State with
        {
            Status = FileOperationStatus.Cancelling,
            ReasonCode = "cancelling",
            CanCancel = false
        });
    }

    private async Task<FileOperationAdapterResult> ExecuteAdapterRequestAsync(
        FileOperationRequest request,
        CancellationToken cancellationToken)
    {
        PendingPermanentDeleteConfirmation = null;
        PendingConflict = null;
        var operationCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _currentOperationCancellation = operationCancellation;
        SetState(FileOperationState.Running(request.Kind, request.Items.Count, CanCancel(request)));
        var progress = new OperationProgressSink(UpdateProgress);
        FileOperationAdapterResult result;

        try
        {
            result = await _adapter.ExecuteAsync(request, progress, operationCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (operationCancellation.Token.IsCancellationRequested)
        {
            result = FileOperationAdapterResult.Cancelled();
        }
        catch (Exception ex)
        {
            result = FileOperationAdapterResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
        finally
        {
            if (ReferenceEquals(_currentOperationCancellation, operationCancellation))
            {
                _currentOperationCancellation = null;
            }

            operationCancellation.Dispose();
        }

        ApplyResult(request, result);
        return result;
    }

    private void UpdateProgress(FileOperationProgress progress)
    {
        SetState(State with { Progress = progress });
    }

    private void ApplyResult(FileOperationRequest request, FileOperationAdapterResult result)
    {
        if (result.Status is FileOperationAdapterResultStatus.ConflictRequired && result.Conflict is not null)
        {
            PendingConflict = result.Conflict;
            SetState(FileOperationState.WaitingForConflict(result.Conflict, result.ReasonCode));
            return;
        }

        var undo = result.Status is FileOperationAdapterResultStatus.Completed
            && request.Kind is FileOperationKind.Move or FileOperationKind.Rename or FileOperationKind.RecycleBinDelete
            && result.UndoSupported
            ? new FileOperationUndoEligibility(CanUndo: true, request.Kind)
            : FileOperationUndoEligibility.None;

        var status = result.Status switch
        {
            FileOperationAdapterResultStatus.Completed => FileOperationStatus.Completed,
            FileOperationAdapterResultStatus.Cancelled => FileOperationStatus.Cancelled,
            FileOperationAdapterResultStatus.RecycleBinUnavailable => FileOperationStatus.Failed,
            _ => FileOperationStatus.Failed
        };

        SetState(State with
        {
            Status = status,
            ReasonCode = result.ReasonCode,
            UndoEligibility = undo,
            CanCancel = false
        });
    }

    private bool CanCancel(FileOperationRequest request)
    {
        return _adapter is ICancellableFileOperationAdapter cancellableAdapter
            && cancellableAdapter.CanCancel(request);
    }

    private static void EnsureSelection(IReadOnlyList<ListedFileItem> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(items));
        }
    }

    private static void EnsureTargetDirectory(string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            throw new ArgumentException("A target directory is required.", nameof(targetDirectory));
        }
    }

    private void SetState(FileOperationState state)
    {
        State = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class OperationProgressSink : IProgress<FileOperationProgress>
    {
        private readonly Action<FileOperationProgress> _report;

        public OperationProgressSink(Action<FileOperationProgress> report)
        {
            _report = report;
        }

        public void Report(FileOperationProgress value)
        {
            _report(value);
        }
    }
}
