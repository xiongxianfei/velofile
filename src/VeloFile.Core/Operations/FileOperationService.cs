using VeloFile.Core;
using VeloFile.Core.Listing;

namespace VeloFile.Core.Operations;

public sealed class FileOperationService
{
    private readonly IFileOperationAdapter _adapter;

    public FileOperationService(IFileOperationAdapter adapter)
    {
        _adapter = adapter;
    }

    public event EventHandler? StateChanged;

    public FileOperationState State { get; private set; } = FileOperationState.Idle;

    public PermanentDeleteConfirmationRequest? PendingPermanentDeleteConfirmation { get; private set; }

    public async Task RenameAsync(ListedFileItem item, string targetName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(targetName))
        {
            throw new ArgumentException("A target name is required.", nameof(targetName));
        }

        await ExecuteAdapterRequestAsync(FileOperationRequest.Rename(item, targetName.Trim()), cancellationToken).ConfigureAwait(false);
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
                UndoEligibility = FileOperationUndoEligibility.None
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

    private async Task<FileOperationAdapterResult> ExecuteAdapterRequestAsync(
        FileOperationRequest request,
        CancellationToken cancellationToken)
    {
        PendingPermanentDeleteConfirmation = null;
        SetState(FileOperationState.Running(request.Kind, request.Items.Count));
        var progress = new OperationProgressSink(UpdateProgress);
        FileOperationAdapterResult result;

        try
        {
            result = await _adapter.ExecuteAsync(request, progress, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result = FileOperationAdapterResult.Cancelled();
        }
        catch (Exception ex)
        {
            result = FileOperationAdapterResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
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
        var undo = result.Status is FileOperationAdapterResultStatus.Completed
            && request.Kind is not FileOperationKind.PermanentDelete
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
            UndoEligibility = undo
        });
    }

    private static void EnsureSelection(IReadOnlyList<ListedFileItem> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(items));
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
