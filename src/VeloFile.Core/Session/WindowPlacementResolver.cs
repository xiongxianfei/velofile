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
    WindowPlacementResolution Resolve(WindowPlacementState? requestedPlacement);
}

public enum WindowPlacementResolutionStatus
{
    DoNotApplyPersistedPlacement,
    UseResolvedPlacement,
    FallbackBecauseInvalidSize,
    FallbackBecauseMonitorMissing,
    FallbackBecauseOffscreen,
    FallbackBecauseMonitorEnumerationEmpty,
    FallbackBecauseMonitorEnumerationFailed,
    FallbackBecauseClamped
}

public sealed record WindowPlacementPolicy(
    int MinimumRestorableWidth,
    int MinimumRestorableHeight,
    int DefaultFallbackWidth,
    int DefaultFallbackHeight,
    int MinimumVisibleMargin,
    string CoordinateUnit)
{
    public static WindowPlacementPolicy Default { get; } = new(
        MinimumRestorableWidth: 900,
        MinimumRestorableHeight: 560,
        DefaultFallbackWidth: 900,
        DefaultFallbackHeight: 560,
        MinimumVisibleMargin: 48,
        CoordinateUnit: "logical-window-bounds");

    public bool HasRestorableSize(WindowPlacementState placement)
    {
        return placement.Width >= MinimumRestorableWidth
            && placement.Height >= MinimumRestorableHeight;
    }

    public bool HasPositiveSize(WindowPlacementState placement)
    {
        return placement.Width > 0 && placement.Height > 0;
    }
}

public sealed class WindowPlacementResolution
{
    private WindowPlacementResolution(
        WindowPlacementResolutionStatus status,
        WindowPlacementState? placement,
        bool isSafeToApply)
    {
        Status = status;
        Placement = placement;
        IsSafeToApply = isSafeToApply;
    }

    public WindowPlacementResolutionStatus Status { get; }

    public WindowPlacementState? Placement { get; }

    public bool IsSafeToApply { get; }

    public bool ShouldApply => IsSafeToApply && Placement is not null;

    public static WindowPlacementResolution DoNotApply(WindowPlacementResolutionStatus status)
    {
        return new WindowPlacementResolution(status, placement: null, isSafeToApply: false);
    }

    public static WindowPlacementResolution Use(WindowPlacementState placement)
    {
        return new WindowPlacementResolution(WindowPlacementResolutionStatus.UseResolvedPlacement, placement, isSafeToApply: true);
    }

    public static WindowPlacementResolution Fallback(WindowPlacementResolutionStatus status, WindowPlacementState placement)
    {
        return new WindowPlacementResolution(status, placement, isSafeToApply: true);
    }
}

public sealed class MonitorWindowPlacementResolver : IMonitorPlacementResolver, IWindowPlacementResolver
{
    private readonly IMonitorLayoutSource _monitorLayoutSource;
    private readonly WindowPlacementPolicy _policy;

    public MonitorWindowPlacementResolver(
        IMonitorLayoutSource monitorLayoutSource,
        WindowPlacementPolicy? policy = null)
    {
        _monitorLayoutSource = monitorLayoutSource;
        _policy = policy ?? WindowPlacementPolicy.Default;
    }

    public bool IsAvailable(string? monitorDeviceName)
    {
        if (string.IsNullOrWhiteSpace(monitorDeviceName))
        {
            return true;
        }

        return TryGetMonitors(out var monitors, out _) && monitors.Any(monitor => string.Equals(monitor.DeviceName, monitorDeviceName, StringComparison.OrdinalIgnoreCase));
    }

    public WindowPlacementState? Fallback(WindowPlacementState? requestedPlacement)
    {
        return Resolve(requestedPlacement).Placement;
    }

    public WindowPlacementResolution Resolve(WindowPlacementState? requestedPlacement)
    {
        if (requestedPlacement is null)
        {
            return WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement);
        }

        if (!TryGetMonitors(out var monitors, out var monitorFailureStatus))
        {
            return WindowPlacementResolution.DoNotApply(monitorFailureStatus);
        }

        var primary = monitors.FirstOrDefault(monitor => monitor.IsPrimary) ?? monitors[0];
        if (!_policy.HasPositiveSize(requestedPlacement) || !_policy.HasRestorableSize(requestedPlacement))
        {
            return WindowPlacementResolution.Fallback(
                WindowPlacementResolutionStatus.FallbackBecauseInvalidSize,
                DefaultPlacement(primary));
        }

        var requestedMonitor = string.IsNullOrWhiteSpace(requestedPlacement.MonitorDeviceName)
            ? null
            : monitors.FirstOrDefault(monitor => string.Equals(monitor.DeviceName, requestedPlacement.MonitorDeviceName, StringComparison.OrdinalIgnoreCase));

        if (requestedMonitor is not null)
        {
            return ResolveOnMonitor(requestedPlacement, requestedMonitor);
        }

        var intersectingMonitor = monitors.FirstOrDefault(monitor => Intersects(requestedPlacement, monitor));
        if (intersectingMonitor is not null)
        {
            var clamped = ClampToMonitor(requestedPlacement, intersectingMonitor);
            return WindowPlacementResolution.Fallback(
                WindowPlacementResolutionStatus.FallbackBecauseMonitorMissing,
                clamped);
        }

        return WindowPlacementResolution.Fallback(
            string.IsNullOrWhiteSpace(requestedPlacement.MonitorDeviceName)
                ? WindowPlacementResolutionStatus.FallbackBecauseOffscreen
                : WindowPlacementResolutionStatus.FallbackBecauseMonitorMissing,
            DefaultPlacement(primary, requestedPlacement.Width, requestedPlacement.Height));
    }

    private bool TryGetMonitors(
        out IReadOnlyList<MonitorWorkArea> monitors,
        out WindowPlacementResolutionStatus failureStatus)
    {
        try
        {
            monitors = _monitorLayoutSource.GetCurrentWorkAreas()
                .Where(monitor => monitor.Width > 0 && monitor.Height > 0)
                .ToArray();
            if (monitors.Count == 0)
            {
                failureStatus = WindowPlacementResolutionStatus.FallbackBecauseMonitorEnumerationEmpty;
                return false;
            }

            failureStatus = WindowPlacementResolutionStatus.DoNotApplyPersistedPlacement;
            return true;
        }
        catch
        {
            monitors = [];
            failureStatus = WindowPlacementResolutionStatus.FallbackBecauseMonitorEnumerationFailed;
            return false;
        }
    }

    private WindowPlacementResolution ResolveOnMonitor(WindowPlacementState requestedPlacement, MonitorWorkArea monitor)
    {
        var clamped = ClampToMonitor(requestedPlacement, monitor);
        return clamped == requestedPlacement
            ? WindowPlacementResolution.Use(clamped)
            : WindowPlacementResolution.Fallback(WindowPlacementResolutionStatus.FallbackBecauseClamped, clamped);
    }

    private WindowPlacementState DefaultPlacement(MonitorWorkArea monitor, int? requestedWidth = null, int? requestedHeight = null)
    {
        var width = requestedWidth is { } requestedW && requestedW >= _policy.MinimumRestorableWidth
            ? requestedW
            : _policy.DefaultFallbackWidth;
        var height = requestedHeight is { } requestedH && requestedH >= _policy.MinimumRestorableHeight
            ? requestedH
            : _policy.DefaultFallbackHeight;
        var resolvedWidth = Math.Min(Math.Max(1, width), monitor.Width);
        var resolvedHeight = Math.Min(Math.Max(1, height), monitor.Height);

        return new WindowPlacementState(
            monitor.Left,
            monitor.Top,
            resolvedWidth,
            resolvedHeight,
            monitor.DeviceName);
    }

    private WindowPlacementState ClampToMonitor(WindowPlacementState placement, MonitorWorkArea monitor)
    {
        var width = Math.Min(Math.Max(placement.Width, _policy.MinimumRestorableWidth), monitor.Width);
        var height = Math.Min(Math.Max(placement.Height, _policy.MinimumRestorableHeight), monitor.Height);
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
