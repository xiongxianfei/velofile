using VeloFile.Core.Listing;

namespace VeloFile.Core.Preview;

public sealed class ThumbnailController
{
    private readonly IThumbnailProvider _provider;
    private readonly PreviewTimeoutPolicy _policy;
    private readonly SemaphoreSlim _liveProviderGate;
    private readonly object _gate = new();
    private Dictionary<string, ThumbnailState> _states = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _expiredRequests = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _activeCancellation;
    private int _generation;

    public ThumbnailController(IThumbnailProvider provider, PreviewTimeoutPolicy? policy = null)
    {
        _provider = provider;
        _policy = policy ?? PreviewTimeoutPolicy.Default;
        _liveProviderGate = new SemaphoreSlim(Math.Max(1, _policy.ThumbnailConcurrencyLimit));
    }

    public event EventHandler? StateChanged;

    public IReadOnlyDictionary<string, ThumbnailState> Snapshot
    {
        get
        {
            lock (_gate)
            {
                return new Dictionary<string, ThumbnailState>(_states, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    public ThumbnailState GetState(ListedFileItem item)
    {
        lock (_gate)
        {
            return _states.TryGetValue(item.FullPath, out var state)
                ? state
                : ThumbnailState.NotLoaded;
        }
    }

    public void Start(IReadOnlyList<ListedFileItem> items)
    {
        CancellationTokenSource? previous;
        CancellationTokenSource current;
        int generation;
        lock (_gate)
        {
            previous = _activeCancellation;
            current = new CancellationTokenSource();
            _activeCancellation = current;
            generation = ++_generation;
            _states = items
                .GroupBy(item => item.FullPath, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    _ => ThumbnailState.Loading,
                    StringComparer.OrdinalIgnoreCase);
            _expiredRequests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        previous?.Cancel();
        RaiseStateChanged();

        if (items.Count == 0)
        {
            return;
        }

        _ = RunGenerationAsync(items, generation, current.Token);
    }

    public void Clear()
    {
        CancellationTokenSource? previous;
        lock (_gate)
        {
            previous = _activeCancellation;
            _activeCancellation = null;
            _generation++;
            _states = new Dictionary<string, ThumbnailState>(StringComparer.OrdinalIgnoreCase);
            _expiredRequests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        previous?.Cancel();
        RaiseStateChanged();
    }

    private async Task RunGenerationAsync(
        IReadOnlyList<ListedFileItem> items,
        int generation,
        CancellationToken cancellationToken)
    {
        var tasks = items.Select(item => RunItemWithVisibleDeadlineAsync(item, generation, cancellationToken)).ToArray();
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunItemWithVisibleDeadlineAsync(
        ListedFileItem item,
        int generation,
        CancellationToken cancellationToken)
    {
        var timeoutBudget = _policy.GetBudget(PreviewOperation.ThumbnailGeneration);
        using var visibleTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var workTask = RunItemAsync(item, generation, visibleTimeout.Token);
        var timeoutTask = Task.Delay(timeoutBudget, cancellationToken);
        var completed = await Task.WhenAny(workTask, timeoutTask).ConfigureAwait(false);

        if (completed == workTask)
        {
            await workTask.ConfigureAwait(false);
            return;
        }

        ExpireRequest(generation, item, "thumbnail-timeout");
        visibleTimeout.Cancel();
        _ = ObserveLateCompletionAsync(workTask);
    }

    private static async Task ObserveLateCompletionAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
    }

    private async Task RunItemAsync(
        ListedFileItem item,
        int generation,
        CancellationToken cancellationToken)
    {
        try
        {
            await _liveProviderGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            try
            {
                var result = await _provider.GenerateAsync(
                    item,
                    new ThumbnailProviderContext(_policy.GetBudget(PreviewOperation.ThumbnailGeneration)),
                    cancellationToken).ConfigureAwait(false);
                ApplyState(generation, item, ToState(item, result));
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                ApplyState(generation, item, ThumbnailState.GenericIcon(GenericIconFor(item), "thumbnail-failed"));
            }
        }
        finally
        {
            _liveProviderGate.Release();
        }
    }

    private void ExpireRequest(int generation, ListedFileItem item, string reasonCode)
    {
        var shouldRaise = false;
        lock (_gate)
        {
            if (generation != _generation || !_states.ContainsKey(item.FullPath))
            {
                return;
            }

            _expiredRequests.Add(item.FullPath);
            _states[item.FullPath] = ThumbnailState.GenericIcon(GenericIconFor(item), reasonCode);
            shouldRaise = true;
        }

        if (shouldRaise)
        {
            RaiseStateChanged();
        }
    }

    private void ApplyState(int generation, ListedFileItem item, ThumbnailState state)
    {
        var shouldRaise = false;
        lock (_gate)
        {
            if (generation != _generation || !_states.ContainsKey(item.FullPath))
            {
                return;
            }

            if (_expiredRequests.Contains(item.FullPath))
            {
                return;
            }

            _states[item.FullPath] = state;
            shouldRaise = true;
        }

        if (shouldRaise)
        {
            RaiseStateChanged();
        }
    }

    private static ThumbnailState ToState(ListedFileItem item, ThumbnailProviderResult result)
    {
        return result.Status switch
        {
            ThumbnailProviderResultStatus.Success when result.Artifact is not null => ThumbnailState.Ready(result.Artifact),
            ThumbnailProviderResultStatus.GenericIcon when result.Artifact is not null => ThumbnailState.GenericIcon(result.Artifact, result.ReasonCode ?? "generic-icon"),
            ThumbnailProviderResultStatus.Failed => ThumbnailState.GenericIcon(GenericIconFor(item), result.ReasonCode ?? "thumbnail-failed"),
            _ => ThumbnailState.GenericIcon(GenericIconFor(item), "thumbnail-failed")
        };
    }

    private static ThumbnailArtifact GenericIconFor(ListedFileItem item)
    {
        if (item.Kind is FileSystemEntryKind.Directory)
        {
            return ThumbnailArtifact.GenericIcon("DIR");
        }

        var extension = Path.GetExtension(item.Name).TrimStart('.');
        return ThumbnailArtifact.GenericIcon(string.IsNullOrWhiteSpace(extension)
            ? "FILE"
            : extension[..Math.Min(extension.Length, 4)].ToUpperInvariant());
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
