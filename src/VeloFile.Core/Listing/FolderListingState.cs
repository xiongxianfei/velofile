namespace VeloFile.Core.Listing;

public enum FolderListingStatus
{
    Pending,
    Ready,
    Empty,
    AccessDenied,
    Unavailable,
    InvalidPath,
    Cancelled,
    Failed
}

public sealed record FolderListingState(
    string Path,
    FolderListingStatus Status,
    IReadOnlyList<ListedFileItem> FirstViewport,
    int KnownItemCount,
    bool IsComplete,
    string? ReasonCode,
    FolderListingState? PreviousValidState)
{
    public static FolderListingState Pending(string path, FolderListingState? previousValidState = null)
    {
        return new FolderListingState(path, FolderListingStatus.Pending, [], 0, IsComplete: false, ReasonCode: null, previousValidState);
    }

    public static FolderListingState Ready(
        string path,
        IReadOnlyList<ListedFileItem> firstViewport,
        int knownItemCount,
        bool isComplete)
    {
        return new FolderListingState(path, FolderListingStatus.Ready, firstViewport, knownItemCount, isComplete, ReasonCode: null, PreviousValidState: null);
    }

    public static FolderListingState Empty(string path)
    {
        return new FolderListingState(path, FolderListingStatus.Empty, [], 0, IsComplete: true, ReasonCode: null, PreviousValidState: null);
    }

    public static FolderListingState Failure(
        string path,
        FolderListingStatus status,
        string reasonCode,
        FolderListingState? previousValidState)
    {
        return new FolderListingState(path, status, [], 0, IsComplete: true, reasonCode, previousValidState);
    }
}
