using Microsoft.UI.Dispatching;

namespace VeloFile.App.ViewModels;

public sealed class WinUiShellDispatcher : IShellDispatcher
{
    private readonly DispatcherQueue _dispatcherQueue;

    public WinUiShellDispatcher(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
    }

    public void Post(Action action)
    {
        _dispatcherQueue.TryEnqueue(() => action());
    }
}
