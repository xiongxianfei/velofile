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

        _ = ShowLoadingAfterDelayAsync(generation, metadata, cancellation);
        _ = CompletePreviewAsync(generation, request, provider, cancellation);
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
        CancellationToken cancellationToken)
    {
        try
        {
            var previewTask = provider.PreviewAsync(request, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(_options.TimeoutBudget, cancellationToken);
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
