namespace VeloFile.Core.Listing;

public sealed record FolderListingLoadResult(
    FolderListingRequest Request,
    FolderListingState State,
    bool Applied);

public sealed record FolderListingOperation(
    FolderListingRequest Request,
    FolderListingState InitialState,
    Task<FolderListingLoadResult> Completion);

public sealed class FolderListingCoordinator
{
    private readonly FolderListingService _listingService;
    private readonly object _sync = new();
    private readonly Dictionary<string, TabListingSlot> _tabs = new(StringComparer.Ordinal);

    public FolderListingCoordinator(FolderListingService listingService)
    {
        _listingService = listingService;
    }

    public FolderListingOperation StartLoad(
        string tabId,
        string path,
        FolderListingOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);

        FolderListingRequest request;
        FolderListingState pendingState;
        FolderListingState? previousValidState;
        CancellationTokenSource requestCancellation;
        CancellationTokenSource? previousCancellation = null;

        lock (_sync)
        {
            previousValidState = null;
            var nextVersion = 1L;

            if (_tabs.TryGetValue(tabId, out var existing))
            {
                previousCancellation = existing.Cancellation;
                previousValidState = LastValidState(existing.State);
                nextVersion = existing.Version + 1;
            }

            request = new FolderListingRequest(tabId, nextVersion);
            requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            pendingState = FolderListingState.Pending(path, previousValidState);
            _tabs[tabId] = new TabListingSlot(nextVersion, path, pendingState, requestCancellation);
        }

        previousCancellation?.Cancel();

        var completion = Task.Run(
            () => CompleteLoadAsync(request, path, options, previousValidState, requestCancellation.Token),
            CancellationToken.None);

        return new FolderListingOperation(request, pendingState, completion);
    }

    public FolderListingState? GetState(string tabId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);

        lock (_sync)
        {
            return _tabs.TryGetValue(tabId, out var tab) ? tab.State : null;
        }
    }

    public void CloseTab(string tabId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);
        CancellationTokenSource? cancellation = null;

        lock (_sync)
        {
            if (_tabs.Remove(tabId, out var tab))
            {
                cancellation = tab.Cancellation;
            }
        }

        cancellation?.Cancel();
    }

    public void CancelTab(string tabId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);
        CancellationTokenSource? cancellation = null;

        lock (_sync)
        {
            if (!_tabs.TryGetValue(tabId, out var tab))
            {
                return;
            }

            cancellation = tab.Cancellation;
            var cancelled = FolderListingState.Failure(
                tab.Path,
                FolderListingStatus.Cancelled,
                "cancelled",
                LastValidState(tab.State));
            _tabs[tabId] = tab with
            {
                Version = tab.Version + 1,
                State = cancelled
            };
        }

        cancellation?.Cancel();
    }

    private async Task<FolderListingLoadResult> CompleteLoadAsync(
        FolderListingRequest request,
        string path,
        FolderListingOptions options,
        FolderListingState? previousValidState,
        CancellationToken cancellationToken)
    {
        FolderListingState state;

        try
        {
            state = await _listingService
                .LoadFirstViewportAsync(path, options, previousValidState, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            state = FolderListingState.Failure(path, FolderListingStatus.Failed, "unexpected", previousValidState);
        }

        lock (_sync)
        {
            if (!_tabs.TryGetValue(request.TabId, out var tab)
                || tab.Version != request.Version
                || !string.Equals(tab.Path, path, StringComparison.Ordinal))
            {
                return new FolderListingLoadResult(request, state, Applied: false);
            }

            _tabs[request.TabId] = tab with { State = state };
            return new FolderListingLoadResult(request, state, Applied: true);
        }
    }

    private static FolderListingState? LastValidState(FolderListingState? state)
    {
        return state?.Status is FolderListingStatus.Ready or FolderListingStatus.Empty
            ? state
            : state?.PreviousValidState;
    }

    private sealed record TabListingSlot(
        long Version,
        string Path,
        FolderListingState State,
        CancellationTokenSource Cancellation);
}
