using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using VeloFile.Core.Session;
using Windows.Graphics;
using WinRT.Interop;

namespace VeloFile.App.Windowing;

public interface IWindowPlacementApplier
{
    void Apply(Window window, WindowPlacementResolution placementResolution);
}

public sealed class WinUiWindowPlacementApplier : IWindowPlacementApplier
{
    public void Apply(Window window, WindowPlacementResolution placementResolution)
    {
        if (!placementResolution.ShouldApply || placementResolution.Placement is not { } placement)
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
