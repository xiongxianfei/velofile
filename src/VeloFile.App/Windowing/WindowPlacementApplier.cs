using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using VeloFile.Core.Persistence;
using Windows.Graphics;
using WinRT.Interop;

namespace VeloFile.App.Windowing;

public interface IWindowPlacementApplier
{
    void Apply(Window window, WindowPlacementState? placement);
}

public sealed class WinUiWindowPlacementApplier : IWindowPlacementApplier
{
    public void Apply(Window window, WindowPlacementState? placement)
    {
        if (placement is null || placement.Width <= 0 || placement.Height <= 0)
        {
            return;
        }

        try
        {
            var windowHandle = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.MoveAndResize(new RectInt32(
                placement.Left,
                placement.Top,
                placement.Width,
                placement.Height));
        }
        catch
        {
            // Window placement restore is best-effort; launch must remain usable.
        }
    }
}
