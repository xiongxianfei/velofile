using System.Security;

namespace VeloFile.Core.Listing;

public sealed record DriveHintRefreshOptions(TimeSpan HintTimeout, int MaxConcurrentHintOperations)
{
    public void Validate()
    {
        if (HintTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(HintTimeout), "Drive hint timeout must be positive.");
        }

        if (MaxConcurrentHintOperations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxConcurrentHintOperations), "Drive hint concurrency must be positive.");
        }
    }
}

public sealed record DriveEntryRefresh(
    long Generation,
    IReadOnlyList<DriveEntry> InitialEntries,
    Task<DriveEntryRefreshResult> Completion);

public sealed record DriveEntryRefreshResult(
    long Generation,
    IReadOnlyList<DriveEntry> Entries,
    bool Applied);

public sealed class DriveEntryService
{
    private readonly IDriveEntrySource _entrySource;
    private readonly IDriveHintSource _hintSource;
    private readonly object _sync = new();
    private long _generation;
    private CancellationTokenSource? _refreshCancellation;
    private IReadOnlyList<DriveEntry> _currentEntries = [];

    public DriveEntryService(IDriveEntrySource entrySource, IDriveHintSource hintSource)
    {
        _entrySource = entrySource;
        _hintSource = hintSource;
    }

    public IReadOnlyList<DriveEntry> CurrentEntries
    {
        get
        {
            lock (_sync)
            {
                return _currentEntries;
            }
        }
    }

    public DriveEntryRefresh StartRefresh(
        DriveHintRefreshOptions options,
        CancellationToken cancellationToken = default)
    {
        options.Validate();

        var discoveredEntries = DiscoverFastEntries();
        long generation;
        IReadOnlyList<DriveEntry> initialEntries;
        CancellationTokenSource refreshCancellation;
        CancellationTokenSource? previousCancellation;

        lock (_sync)
        {
            previousCancellation = _refreshCancellation;
            generation = ++_generation;
            refreshCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _refreshCancellation = refreshCancellation;
            initialEntries = discoveredEntries
                .Select(static entry => WithoutHints(entry, DriveHintStatus.Loading))
                .ToArray();
            _currentEntries = initialEntries;
        }

        previousCancellation?.Cancel();

        var completion = Task.Run(
            () => EnrichAsync(generation, initialEntries, options, refreshCancellation.Token),
            CancellationToken.None);

        return new DriveEntryRefresh(generation, initialEntries, completion);
    }

    public void CancelRefresh()
    {
        CancellationTokenSource? cancellation;

        lock (_sync)
        {
            cancellation = _refreshCancellation;
            _refreshCancellation = null;
            _generation++;
            _currentEntries = _currentEntries
                .Select(static entry => WithoutHints(entry, DriveHintStatus.Cancelled))
                .ToArray();
        }

        cancellation?.Cancel();
    }

    private IReadOnlyList<DriveEntry> DiscoverFastEntries()
    {
        try
        {
            return _entrySource.GetDrives();
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return [];
        }
    }

    private async Task<DriveEntryRefreshResult> EnrichAsync(
        long generation,
        IReadOnlyList<DriveEntry> initialEntries,
        DriveHintRefreshOptions options,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(options.MaxConcurrentHintOperations);
        var tasks = initialEntries
            .Select(entry => EnrichEntryAsync(entry, options, semaphore, cancellationToken))
            .ToArray();
        var entries = await Task.WhenAll(tasks).ConfigureAwait(false);

        lock (_sync)
        {
            if (generation != _generation)
            {
                return new DriveEntryRefreshResult(generation, entries, Applied: false);
            }

            _currentEntries = entries;
            return new DriveEntryRefreshResult(generation, entries, Applied: true);
        }
    }

    private async Task<DriveEntry> EnrichEntryAsync(
        DriveEntry entry,
        DriveHintRefreshOptions options,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        var acquired = false;

        try
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            acquired = true;
            var hint = await ReadHintWithTimeoutAsync(entry.RootPath, options.HintTimeout, cancellationToken).ConfigureAwait(false);
            return ApplyHint(entry, hint);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return WithoutHints(entry, DriveHintStatus.Cancelled);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
        {
            return WithoutHints(entry, DriveHintStatus.AccessDenied);
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return WithoutHints(entry, DriveHintStatus.Unavailable);
        }
        catch (Exception)
        {
            return WithoutHints(entry, DriveHintStatus.Unavailable);
        }
        finally
        {
            if (acquired)
            {
                semaphore.Release();
            }
        }
    }

    private async Task<DriveHint> ReadHintWithTimeoutAsync(
        string rootPath,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var hintCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<DriveHint> hintTask;

        try
        {
            hintTask = _hintSource.GetHintAsync(rootPath, hintCancellation.Token);
        }
        catch
        {
            hintCancellation.Dispose();
            throw;
        }

        var timeoutTask = Task.Delay(timeout);
        var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        var completed = await Task.WhenAny(hintTask, timeoutTask, cancellationTask).ConfigureAwait(false);

        if (completed == hintTask)
        {
            try
            {
                return await hintTask.ConfigureAwait(false);
            }
            finally
            {
                hintCancellation.Dispose();
            }
        }

        hintCancellation.Cancel();
        _ = ObserveLateHintAsync(hintTask, hintCancellation);

        return completed == cancellationTask || cancellationToken.IsCancellationRequested
            ? DriveHint.Cancelled()
            : DriveHint.TimedOut();
    }

    private static async Task ObserveLateHintAsync(Task<DriveHint> hintTask, CancellationTokenSource hintCancellation)
    {
        try
        {
            await hintTask.ConfigureAwait(false);
        }
        catch
        {
            // Late hint failures are intentionally ignored after timeout/cancellation.
        }
        finally
        {
            hintCancellation.Dispose();
        }
    }

    private static DriveEntry ApplyHint(DriveEntry entry, DriveHint hint)
    {
        return hint.Status == DriveHintStatus.Available
            ? entry with
            {
                Name = string.IsNullOrWhiteSpace(hint.VolumeLabel) ? entry.RootPath : hint.VolumeLabel,
                IsReady = hint.IsReady,
                AvailableFreeSpaceBytes = hint.AvailableFreeSpaceBytes,
                TotalSizeBytes = hint.TotalSizeBytes,
                HintStatus = DriveHintStatus.Available,
                VolumeLabel = hint.VolumeLabel
            }
            : WithoutHints(entry, hint.Status);
    }

    private static DriveEntry WithoutHints(DriveEntry entry, DriveHintStatus status)
    {
        return entry with
        {
            Name = entry.RootPath,
            IsReady = false,
            AvailableFreeSpaceBytes = null,
            TotalSizeBytes = null,
            HintStatus = status,
            VolumeLabel = null
        };
    }
}
