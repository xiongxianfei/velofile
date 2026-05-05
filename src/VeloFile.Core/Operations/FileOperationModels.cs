using VeloFile.Core.Listing;

namespace VeloFile.Core.Operations;

public enum FileOperationKind
{
    Rename,
    RecycleBinDelete,
    PermanentDelete
}

public enum FileOperationStatus
{
    Idle,
    Running,
    WaitingForConfirmation,
    Completed,
    Failed,
    Cancelled
}

public enum FileOperationAdapterResultStatus
{
    Completed,
    Failed,
    Cancelled,
    RecycleBinUnavailable
}

public enum PermanentDeleteReason
{
    UserGesture,
    RecycleBinUnavailable
}

public sealed record FileOperationTarget(
    string Path,
    string Name,
    FileSystemEntryKind Kind)
{
    public static FileOperationTarget FromListedItem(ListedFileItem item)
    {
        return new FileOperationTarget(item.FullPath, item.Name, item.Kind);
    }
}

public sealed record FileOperationRequest(
    FileOperationKind Kind,
    IReadOnlyList<FileOperationTarget> Items,
    string? TargetName,
    bool ConfirmedPermanentDelete)
{
    public static FileOperationRequest Rename(ListedFileItem item, string targetName)
    {
        return new FileOperationRequest(
            FileOperationKind.Rename,
            [FileOperationTarget.FromListedItem(item)],
            targetName,
            ConfirmedPermanentDelete: false);
    }

    public static FileOperationRequest RecycleBinDelete(IReadOnlyList<ListedFileItem> items)
    {
        return new FileOperationRequest(
            FileOperationKind.RecycleBinDelete,
            items.Select(FileOperationTarget.FromListedItem).ToArray(),
            TargetName: null,
            ConfirmedPermanentDelete: false);
    }

    public static FileOperationRequest PermanentDelete(IReadOnlyList<ListedFileItem> items, bool confirmed)
    {
        return new FileOperationRequest(
            FileOperationKind.PermanentDelete,
            items.Select(FileOperationTarget.FromListedItem).ToArray(),
            TargetName: null,
            ConfirmedPermanentDelete: confirmed);
    }
}

public sealed record FileOperationProgress(
    FileOperationKind Kind,
    int CompletedItemCount,
    int TotalItemCount,
    string StatusText);

public sealed record FileOperationUndoEligibility(
    bool CanUndo,
    FileOperationKind? OperationKind)
{
    public static FileOperationUndoEligibility None { get; } = new(false, OperationKind: null);
}

public sealed record PermanentDeleteConfirmationRequest(
    FileOperationKind Kind,
    PermanentDeleteReason Reason,
    IReadOnlyList<FileOperationTarget> Items);

public sealed record FileOperationState(
    FileOperationStatus Status,
    FileOperationKind? Kind,
    FileOperationProgress Progress,
    string? ReasonCode,
    FileOperationUndoEligibility UndoEligibility)
{
    public static FileOperationState Idle { get; } = new(
        FileOperationStatus.Idle,
        Kind: null,
        new FileOperationProgress(FileOperationKind.RecycleBinDelete, 0, 0, ""),
        ReasonCode: null,
        FileOperationUndoEligibility.None);

    public static FileOperationState Running(FileOperationKind kind, int totalItemCount)
    {
        return new FileOperationState(
            FileOperationStatus.Running,
            kind,
            new FileOperationProgress(kind, CompletedItemCount: 0, totalItemCount, "Running"),
            ReasonCode: null,
            FileOperationUndoEligibility.None);
    }

    public static FileOperationState WaitingForConfirmation(
        PermanentDeleteConfirmationRequest confirmation,
        string? reasonCode = null)
    {
        return new FileOperationState(
            FileOperationStatus.WaitingForConfirmation,
            confirmation.Kind,
            new FileOperationProgress(confirmation.Kind, CompletedItemCount: 0, confirmation.Items.Count, "Waiting for confirmation"),
            reasonCode,
            FileOperationUndoEligibility.None);
    }
}

public sealed record FileOperationAdapterResult(
    FileOperationAdapterResultStatus Status,
    string? ReasonCode,
    bool UndoSupported)
{
    public static FileOperationAdapterResult Completed(bool undoSupported)
    {
        return new FileOperationAdapterResult(FileOperationAdapterResultStatus.Completed, ReasonCode: null, undoSupported);
    }

    public static FileOperationAdapterResult Failed(string reasonCode)
    {
        return new FileOperationAdapterResult(FileOperationAdapterResultStatus.Failed, reasonCode, UndoSupported: false);
    }

    public static FileOperationAdapterResult Cancelled()
    {
        return new FileOperationAdapterResult(FileOperationAdapterResultStatus.Cancelled, "cancelled", UndoSupported: false);
    }

    public static FileOperationAdapterResult RecycleBinUnavailable(string reasonCode)
    {
        return new FileOperationAdapterResult(FileOperationAdapterResultStatus.RecycleBinUnavailable, reasonCode, UndoSupported: false);
    }
}

public interface IFileOperationAdapter
{
    Task<FileOperationAdapterResult> ExecuteAsync(
        FileOperationRequest request,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken);
}
