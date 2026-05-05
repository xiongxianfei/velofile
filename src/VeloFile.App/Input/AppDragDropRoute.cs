using VeloFile.App.ViewModels;
using VeloFile.Core.DragDrop;

namespace VeloFile.App.Input;

public enum AppDropAcceptedOperation
{
    None,
    Copy,
    Move,
    Link
}

public sealed record AppDragDropPayload(
    bool CanDrop,
    IReadOnlyList<DropItem> Items,
    bool SupportsShortcut,
    string? ReasonCode)
{
    public static AppDragDropPayload Supported(IReadOnlyList<DropItem> items, bool supportsShortcut = true)
    {
        return new AppDragDropPayload(CanDrop: items.Count > 0, items, supportsShortcut, ReasonCode: null);
    }

    public static AppDragDropPayload Unsupported(string reasonCode)
    {
        return new AppDragDropPayload(CanDrop: false, [], SupportsShortcut: false, reasonCode);
    }
}

public sealed record AppDragDropRouteResult(
    AppDropAcceptedOperation AcceptedOperation,
    string IndicatorText,
    bool CanDrop,
    string? ReasonCode);

public interface IAppDragDropPayloadExtractor
{
    ValueTask<AppDragDropPayload> ExtractAsync(object? data, CancellationToken cancellationToken = default);
}

public sealed class AppDragDropRoute
{
    private readonly AppShellViewModel _viewModel;
    private readonly IAppDragDropPayloadExtractor _payloadExtractor;

    public AppDragDropRoute(AppShellViewModel viewModel, IAppDragDropPayloadExtractor payloadExtractor)
    {
        _viewModel = viewModel;
        _payloadExtractor = payloadExtractor;
    }

    public async ValueTask<AppDragDropRouteResult> DragOverAsync(
        object? data,
        DragDropKeyModifiers modifiers,
        CancellationToken cancellationToken = default)
    {
        var payload = await _payloadExtractor.ExtractAsync(data, cancellationToken).ConfigureAwait(false);
        return PreviewPayload(payload, modifiers);
    }

    public async Task<AppDragDropRouteResult> DropAsync(
        object? data,
        DragDropKeyModifiers modifiers,
        CancellationToken cancellationToken = default)
    {
        var payload = await _payloadExtractor.ExtractAsync(data, cancellationToken).ConfigureAwait(false);
        var preview = PreviewPayload(payload, modifiers);
        if (!preview.CanDrop)
        {
            return preview;
        }

        await _viewModel.CommitDropAsync(
            payload.Items,
            modifiers,
            DropVolumeRelationshipClassifier.Classify(payload.Items, _viewModel.ActivePath),
            payload.SupportsShortcut).ConfigureAwait(false);

        return preview;
    }

    public void DragLeave()
    {
        _viewModel.ClearDropAction();
    }

    private AppDragDropRouteResult PreviewPayload(AppDragDropPayload payload, DragDropKeyModifiers modifiers)
    {
        if (!payload.CanDrop)
        {
            _viewModel.ClearDropAction();
            return new AppDragDropRouteResult(
                AppDropAcceptedOperation.None,
                "Drop unavailable",
                CanDrop: false,
                payload.ReasonCode ?? "drop-unsupported-payload");
        }

        _viewModel.UpdateDropAction(
            payload.Items,
            modifiers,
            DropVolumeRelationshipClassifier.Classify(payload.Items, _viewModel.ActivePath),
            payload.SupportsShortcut);

        return Map(_viewModel.CurrentDropAction);
    }

    private static AppDragDropRouteResult Map(DropActionResolution resolution)
    {
        return new AppDragDropRouteResult(
            resolution.Action switch
            {
                DropAction.Copy => AppDropAcceptedOperation.Copy,
                DropAction.Move => AppDropAcceptedOperation.Move,
                DropAction.Shortcut => AppDropAcceptedOperation.Link,
                _ => AppDropAcceptedOperation.None
            },
            resolution.IndicatorText,
            resolution.CanDrop,
            resolution.ReasonCode);
    }
}
