using VeloFile.Core.Persistence;

namespace VeloFile.Core.Session;

public sealed record MonitorWorkArea(
    string DeviceName,
    int Left,
    int Top,
    int Width,
    int Height,
    bool IsPrimary);

public interface IMonitorLayoutSource
{
    IReadOnlyList<MonitorWorkArea> GetCurrentWorkAreas();
}

public interface IWindowPlacementResolver
{
    WindowPlacementState? Resolve(WindowPlacementState? requestedPlacement);
}

public sealed class MonitorWindowPlacementResolver : IMonitorPlacementResolver, IWindowPlacementResolver
{
    public const int DefaultWidth = 900;
    public const int DefaultHeight = 560;

    private readonly IMonitorLayoutSource _monitorLayoutSource;

    public MonitorWindowPlacementResolver(IMonitorLayoutSource monitorLayoutSource)
    {
        _monitorLayoutSource = monitorLayoutSource;
    }

    public bool IsAvailable(string? monitorDeviceName)
    {
        if (string.IsNullOrWhiteSpace(monitorDeviceName))
        {
            return true;
        }

        return GetMonitors().Any(monitor => string.Equals(monitor.DeviceName, monitorDeviceName, StringComparison.OrdinalIgnoreCase));
    }

    public WindowPlacementState? Fallback(WindowPlacementState? requestedPlacement)
    {
        return Resolve(requestedPlacement);
    }

    public WindowPlacementState? Resolve(WindowPlacementState? requestedPlacement)
    {
        if (requestedPlacement is null)
        {
            return null;
        }

        var monitors = GetMonitors();
        if (monitors.Count == 0)
        {
            return HasValidDimensions(requestedPlacement) ? requestedPlacement : null;
        }

        var primary = monitors.FirstOrDefault(monitor => monitor.IsPrimary) ?? monitors[0];
        if (!HasValidDimensions(requestedPlacement))
        {
            return DefaultPlacement(primary);
        }

        var requestedMonitor = string.IsNullOrWhiteSpace(requestedPlacement.MonitorDeviceName)
            ? null
            : monitors.FirstOrDefault(monitor => string.Equals(monitor.DeviceName, requestedPlacement.MonitorDeviceName, StringComparison.OrdinalIgnoreCase));

        if (requestedMonitor is not null)
        {
            return ClampToMonitor(requestedPlacement, requestedMonitor);
        }

        var intersectingMonitor = monitors.FirstOrDefault(monitor => Intersects(requestedPlacement, monitor));
        if (intersectingMonitor is not null)
        {
            return ClampToMonitor(requestedPlacement, intersectingMonitor);
        }

        return DefaultPlacement(primary, requestedPlacement.Width, requestedPlacement.Height);
    }

    private IReadOnlyList<MonitorWorkArea> GetMonitors()
    {
        try
        {
            return _monitorLayoutSource.GetCurrentWorkAreas();
        }
        catch
        {
            return [];
        }
    }

    private static bool HasValidDimensions(WindowPlacementState placement)
    {
        return placement.Width > 0 && placement.Height > 0;
    }

    private static WindowPlacementState DefaultPlacement(MonitorWorkArea monitor, int width = DefaultWidth, int height = DefaultHeight)
    {
        var resolvedWidth = Math.Min(Math.Max(1, width), monitor.Width);
        var resolvedHeight = Math.Min(Math.Max(1, height), monitor.Height);
        return new WindowPlacementState(
            monitor.Left,
            monitor.Top,
            resolvedWidth,
            resolvedHeight,
            monitor.DeviceName);
    }

    private static WindowPlacementState ClampToMonitor(WindowPlacementState placement, MonitorWorkArea monitor)
    {
        var width = Math.Min(placement.Width, monitor.Width);
        var height = Math.Min(placement.Height, monitor.Height);
        var minLeft = monitor.Left;
        var maxLeft = monitor.Left + monitor.Width - width;
        var minTop = monitor.Top;
        var maxTop = monitor.Top + monitor.Height - height;

        return new WindowPlacementState(
            Math.Clamp(placement.Left, minLeft, maxLeft),
            Math.Clamp(placement.Top, minTop, maxTop),
            width,
            height,
            monitor.DeviceName);
    }

    private static bool Intersects(WindowPlacementState placement, MonitorWorkArea monitor)
    {
        var placementRight = placement.Left + placement.Width;
        var placementBottom = placement.Top + placement.Height;
        var monitorRight = monitor.Left + monitor.Width;
        var monitorBottom = monitor.Top + monitor.Height;

        return placement.Left < monitorRight
            && placementRight > monitor.Left
            && placement.Top < monitorBottom
            && placementBottom > monitor.Top;
    }
}
