using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;

namespace VeloFile.Core.Preview;

public sealed class PreviewController
{
    private readonly IReadOnlyList<IPreviewProvider> _providers;
    private readonly PreviewMetadataProvider _metadataProvider;
    private readonly PreviewControllerOptions _options;
    private readonly IDiagnosticSink? _diagnostics;
    private readonly PathRedactor? _pathRedactor;
    private readonly object _gate = new();
    private CancellationTokenSource? _activeCancellation;
    private PreviewRequest? _activeRequest;
    private IPreviewProvider? _activeProvider;
    private int _generation;
    private PreviewState _state = PreviewState.Empty;

    public PreviewController(
        IReadOnlyList<IPreviewProvider> providers,
        PreviewMetadataProvider metadataProvider,
        PreviewControllerOptions? options = null,
        IDiagnosticSink? diagnostics = null,
        PathRedactor? pathRedactor = null)
    {
        _providers = providers;
        _metadataProvider = metadataProvider;
        _options = options ?? PreviewControllerOptions.Default;
        _diagnostics = diagnostics;
        _pathRedactor = pathRedactor;
    }

    public event EventHandler? StateChanged;

    public PreviewState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    public void StartPreview(ListedFileItem? item)
    {
        CancellationTokenSource? previous;
        CancellationTokenSource? current;
        int generation;
        lock (_gate)
        {
            previous = _activeCancellation;
            current = item is null ? null : new CancellationTokenSource();
            _activeCancellation = current;
            _activeRequest = null;
            _activeProvider = null;
            generation = ++_generation;
            _state = PreviewState.Empty;
        }

        previous?.Cancel();
        RaiseStateChanged();

        if (item is null)
        {
            return;
        }

        var cancellation = current?.Token ?? CancellationToken.None;
        var metadata = _metadataProvider.GetMetadata(item);
        var request = new PreviewRequest(item, metadata);
        var provider = _providers.FirstOrDefault(candidate => candidate.CanPreview(request));
        if (provider is null)
        {
            TryApply(generation, PreviewState.Unsupported(metadata, "unsupported"));
            return;
        }

        lock (_gate)
        {
            if (generation != _generation)
            {
                return;
            }

            _activeRequest = request;
            _activeProvider = provider;
        }

        var providerContext = new PreviewProviderContext(
            provider.Operation,
            _options.TimeoutPolicy.GetBudget(provider.Operation));
        _ = ShowLoadingAfterDelayAsync(generation, metadata, cancellation);
        _ = CompletePreviewAsync(generation, request, provider, providerContext, cancellation);
    }

    public bool RequestPreviewPage(int pageNumber)
    {
        if (pageNumber < 1)
        {
            return false;
        }

        CancellationTokenSource? previous;
        CancellationTokenSource current;
        PreviewRequest request;
        IPagedPreviewProvider provider;
        int generation;
        lock (_gate)
        {
            if (_activeRequest is null || _activeProvider is not IPagedPreviewProvider pagedProvider)
            {
                return false;
            }

            previous = _activeCancellation;
            current = new CancellationTokenSource();
            _activeCancellation = current;
            request = _activeRequest;
            provider = pagedProvider;
            generation = ++_generation;
            _state = PreviewState.Empty;
        }

        previous?.Cancel();
        RaiseStateChanged();

        var providerContext = new PreviewProviderContext(
            provider.Operation,
            _options.TimeoutPolicy.GetBudget(provider.Operation));
        _ = ShowLoadingAfterDelayAsync(generation, request.Metadata, current.Token);
        _ = CompletePagedPreviewAsync(generation, request, provider, pageNumber, providerContext, current.Token);
        return true;
    }

    public void Clear()
    {
        StartPreview(item: null);
    }

    private async Task ShowLoadingAfterDelayAsync(int generation, PreviewMetadata metadata, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_options.LoadingDelay, cancellationToken).ConfigureAwait(false);
            TryApply(generation, PreviewState.Loading(metadata), onlyIfPending: true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task CompletePreviewAsync(
        int generation,
        PreviewRequest request,
        IPreviewProvider provider,
        PreviewProviderContext providerContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var previewTask = provider.PreviewAsync(request, providerContext, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(providerContext.TimeoutBudget, cancellationToken);
            var completed = await Task.WhenAny(previewTask, timeoutTask).ConfigureAwait(false);
            if (completed != previewTask)
            {
                TryCancelActive(generation);
                var timeoutState = PreviewState.Failed(request.Metadata, "timeout");
                TryApply(generation, timeoutState);
                WritePreviewFailure(request, timeoutState.ReasonCode!);
                return;
            }

            var result = await previewTask.ConfigureAwait(false);
            var state = ToState(request.Metadata, result);
            TryApply(generation, state);
            if (state.Status is PreviewStatus.Failed)
            {
                WritePreviewFailure(request, state.ReasonCode ?? "unknown");
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (UnauthorizedAccessException)
        {
            var state = PreviewState.Failed(request.Metadata, "access-denied");
            TryApply(generation, state);
            WritePreviewFailure(request, state.ReasonCode!);
        }
        catch (Exception)
        {
            var state = PreviewState.Failed(request.Metadata, "decode-error");
            TryApply(generation, state);
            WritePreviewFailure(request, state.ReasonCode!);
        }
    }

    private async Task CompletePagedPreviewAsync(
        int generation,
        PreviewRequest request,
        IPagedPreviewProvider provider,
        int pageNumber,
        PreviewProviderContext providerContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var previewTask = provider.PreviewPageAsync(request, pageNumber, providerContext, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(providerContext.TimeoutBudget, cancellationToken);
            var completed = await Task.WhenAny(previewTask, timeoutTask).ConfigureAwait(false);
            if (completed != previewTask)
            {
                TryCancelActive(generation);
                var timeoutState = PreviewState.Failed(request.Metadata, "timeout");
                TryApply(generation, timeoutState);
                WritePreviewFailure(request, timeoutState.ReasonCode!);
                return;
            }

            var result = await previewTask.ConfigureAwait(false);
            var state = ToState(request.Metadata, result);
            TryApply(generation, state);
            if (state.Status is PreviewStatus.Failed)
            {
                WritePreviewFailure(request, state.ReasonCode ?? "unknown");
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (UnauthorizedAccessException)
        {
            var state = PreviewState.Failed(request.Metadata, "access-denied");
            TryApply(generation, state);
            WritePreviewFailure(request, state.ReasonCode!);
        }
        catch (Exception)
        {
            var state = PreviewState.Failed(request.Metadata, "decode-error");
            TryApply(generation, state);
            WritePreviewFailure(request, state.ReasonCode!);
        }
    }

    private void TryCancelActive(int generation)
    {
        lock (_gate)
        {
            if (generation == _generation)
            {
                _activeCancellation?.Cancel();
            }
        }
    }

    private static PreviewState ToState(PreviewMetadata metadata, PreviewProviderResult result)
    {
        return result.Status switch
        {
            PreviewProviderResultStatus.Success when result.Content is not null => PreviewState.Success(metadata, result.Content),
            PreviewProviderResultStatus.Unsupported => PreviewState.Unsupported(metadata, result.ReasonCode ?? "unsupported"),
            PreviewProviderResultStatus.Failed => PreviewState.Failed(metadata, result.ReasonCode ?? "unknown"),
            _ => PreviewState.Failed(metadata, "decode-error")
        };
    }

    private void TryApply(int generation, PreviewState state, bool onlyIfPending = false)
    {
        var shouldRaise = false;
        lock (_gate)
        {
            if (generation != _generation)
            {
                return;
            }

            if (onlyIfPending && _state.Status is not PreviewStatus.Empty)
            {
                return;
            }

            _state = state;
            shouldRaise = true;
        }

        if (shouldRaise)
        {
            RaiseStateChanged();
        }
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void WritePreviewFailure(PreviewRequest request, string reasonCode)
    {
        if (_diagnostics is null || _pathRedactor is null)
        {
            return;
        }

        try
        {
            _diagnostics.Write(DiagnosticEvent.CreateFailure(
                Guid.NewGuid().ToString("N"),
                sequenceNumber: 0,
                component: "preview",
                operationKind: "preview",
                reasonCode: reasonCode,
                path: request.Item.FullPath,
                redactor: _pathRedactor,
                timestampUtc: DateTimeOffset.UtcNow));
        }
        catch
        {
            // Preview diagnostics are best-effort; the preview state is the product behavior.
        }
    }
}
