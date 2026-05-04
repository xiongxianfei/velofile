using System.Security;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Listing;

public interface IFolderEntrySource
{
    IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(string path, CancellationToken cancellationToken);
}

public sealed record FolderListingOptions(
    int ViewportItemCount,
    VisibilitySettings VisibilitySettings)
{
    public void Validate()
    {
        if (ViewportItemCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ViewportItemCount), "Viewport item count must be positive.");
        }
    }
}

public sealed class FolderListingService
{
    private readonly IFolderEntrySource _entrySource;
    private readonly IDiagnosticSink? _diagnostics;
    private readonly PathRedactor? _pathRedactor;

    public FolderListingService(
        IFolderEntrySource entrySource,
        IDiagnosticSink? diagnostics = null,
        PathRedactor? pathRedactor = null)
    {
        _entrySource = entrySource;
        _diagnostics = diagnostics;
        _pathRedactor = pathRedactor;
    }

    public async Task<FolderListingState> LoadFirstViewportAsync(
        string path,
        FolderListingOptions options,
        FolderListingState? previousValidState = null,
        CancellationToken cancellationToken = default)
    {
        options.Validate();

        if (string.IsNullOrWhiteSpace(path))
        {
            var invalid = FolderListingState.Failure(path, FolderListingStatus.InvalidPath, "invalid-path", previousValidState);
            WriteNavigationFailure(invalid);
            return invalid;
        }

        try
        {
            var feed = await VirtualizedListingFeed.CreateAsync(
                _entrySource.EnumerateAsync(path, cancellationToken),
                options,
                cancellationToken);

            return feed.FirstViewport.Count == 0 && feed.IsComplete
                ? FolderListingState.Empty(path)
                : FolderListingState.Ready(path, feed.FirstViewport, feed.KnownItemCount, feed.IsComplete);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FolderListingState.Failure(path, FolderListingStatus.Cancelled, "cancelled", previousValidState);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
        {
            var failure = FolderListingState.Failure(path, FolderListingStatus.AccessDenied, ExpectedFileSystemExceptions.ReasonCode(ex), previousValidState);
            WriteNavigationFailure(failure);
            return failure;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            var failure = FolderListingState.Failure(path, FolderListingStatus.Unavailable, ExpectedFileSystemExceptions.ReasonCode(ex), previousValidState);
            WriteNavigationFailure(failure);
            return failure;
        }
        catch (Exception ex) when (ex is IOException)
        {
            var failure = FolderListingState.Failure(path, FolderListingStatus.Unavailable, ExpectedFileSystemExceptions.ReasonCode(ex), previousValidState);
            WriteNavigationFailure(failure);
            return failure;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            var failure = FolderListingState.Failure(path, FolderListingStatus.InvalidPath, "invalid-path", previousValidState);
            WriteNavigationFailure(failure);
            return failure;
        }
    }

    private void WriteNavigationFailure(FolderListingState failure)
    {
        if (_diagnostics is null || _pathRedactor is null || failure.ReasonCode is null)
        {
            return;
        }

        try
        {
            _diagnostics.Write(DiagnosticEvent.CreateFailure(
                eventId: Guid.NewGuid().ToString("N"),
                sequenceNumber: 0,
                component: "navigation",
                operationKind: "navigation",
                reasonCode: failure.ReasonCode,
                path: failure.Path,
                redactor: _pathRedactor,
                timestampUtc: DateTimeOffset.UtcNow));
        }
        catch
        {
            // Diagnostics are best-effort; navigation state must not fail because logging failed.
        }
    }
}

public sealed record VirtualizedListingFeed(
    IReadOnlyList<ListedFileItem> FirstViewport,
    int KnownItemCount,
    bool IsComplete)
{
    public static async Task<VirtualizedListingFeed> CreateAsync(
        IAsyncEnumerable<FileSystemEntrySnapshot> entries,
        FolderListingOptions options,
        CancellationToken cancellationToken)
    {
        var viewport = new List<ListedFileItem>(options.ViewportItemCount);
        var knownItemCount = 0;
        var isComplete = true;

        await using var enumerator = entries
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false)
            .GetAsyncEnumerator();

        while (await enumerator.MoveNextAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            knownItemCount++;

            var projected = FileVisibilityProjector.Project(enumerator.Current, options.VisibilitySettings);
            if (projected is null)
            {
                continue;
            }

            viewport.Add(projected);
            if (viewport.Count == options.ViewportItemCount)
            {
                isComplete = false;
                break;
            }
        }

        return new VirtualizedListingFeed(viewport, knownItemCount, isComplete);
    }
}

public sealed record FolderListingRequest(string TabId, long Version);

public sealed class FolderListingRequestGate
{
    private readonly Dictionary<string, long> _currentVersions = new(StringComparer.Ordinal);

    public FolderListingRequest StartRequest(string tabId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);

        _currentVersions.TryGetValue(tabId, out var current);
        var next = current + 1;
        _currentVersions[tabId] = next;
        return new FolderListingRequest(tabId, next);
    }

    public bool IsCurrent(FolderListingRequest request)
    {
        return _currentVersions.TryGetValue(request.TabId, out var current)
            && current == request.Version;
    }
}
