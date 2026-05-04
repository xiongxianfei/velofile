using VeloFile.Core.Persistence;

namespace VeloFile.Core.Session;

public sealed record MonitorWorkArea(
    string DeviceName,
    int Left,
    int Top,
    int Width,
    int Height,
    bool IsPrimary,
    double? RasterizationScale = 1.0);

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
    FallbackBecauseMonitorScaleUnavailable,
    FallbackBecauseClamped
}

public sealed record WindowPlacementPolicy(
    int ShellMinimumWidthEffectivePixels,
    int ShellMinimumHeightEffectivePixels,
    int DefaultFallbackWidthEffectivePixels,
    int DefaultFallbackHeightEffectivePixels,
    int MinimumVisibleMarginPhysicalPixels,
    string PersistedBoundsUnit,
    string MonitorWorkAreaUnit,
    string ResolvedPlacementUnit,
    string ShellMinimumUnit)
{
    public static WindowPlacementPolicy Default { get; } = new(
        ShellMinimumWidthEffectivePixels: 900,
        ShellMinimumHeightEffectivePixels: 560,
        DefaultFallbackWidthEffectivePixels: 900,
        DefaultFallbackHeightEffectivePixels: 560,
        MinimumVisibleMarginPhysicalPixels: 48,
        PersistedBoundsUnit: "physical-pixels",
        MonitorWorkAreaUnit: "physical-pixels",
        ResolvedPlacementUnit: "physical-pixels",
        ShellMinimumUnit: "effective-pixels-before-monitor-scale-conversion");

    public int MinimumRestorableWidth => ShellMinimumWidthEffectivePixels;

    public int MinimumRestorableHeight => ShellMinimumHeightEffectivePixels;

    public int DefaultFallbackWidth => DefaultFallbackWidthEffectivePixels;

    public int DefaultFallbackHeight => DefaultFallbackHeightEffectivePixels;

    public bool HasRestorableSize(WindowPlacementState placement)
    {
        return placement.Width >= ShellMinimumWidthEffectivePixels
            && placement.Height >= ShellMinimumHeightEffectivePixels;
    }

    public bool HasRestorablePhysicalSize(WindowPlacementState placement, MonitorWorkArea monitor)
    {
        return placement.Width >= MinimumPhysicalWidth(monitor)
            && placement.Height >= MinimumPhysicalHeight(monitor);
    }

    public bool HasPositiveSize(WindowPlacementState placement)
    {
        return placement.Width > 0 && placement.Height > 0;
    }

    public bool HasKnownScale(MonitorWorkArea monitor)
    {
        return monitor.RasterizationScale is { } scale
            && !double.IsNaN(scale)
            && !double.IsInfinity(scale)
            && scale > 0;
    }

    public int MinimumPhysicalWidth(MonitorWorkArea monitor)
    {
        return ToPhysicalPixels(ShellMinimumWidthEffectivePixels, monitor);
    }

    public int MinimumPhysicalHeight(MonitorWorkArea monitor)
    {
        return ToPhysicalPixels(ShellMinimumHeightEffectivePixels, monitor);
    }

    public int DefaultFallbackPhysicalWidth(MonitorWorkArea monitor)
    {
        return ToPhysicalPixels(DefaultFallbackWidthEffectivePixels, monitor);
    }

    public int DefaultFallbackPhysicalHeight(MonitorWorkArea monitor)
    {
        return ToPhysicalPixels(DefaultFallbackHeightEffectivePixels, monitor);
    }

    private static int ToPhysicalPixels(int effectivePixels, MonitorWorkArea monitor)
    {
        return (int)Math.Ceiling(effectivePixels * monitor.RasterizationScale!.Value);
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
        var candidate = SelectCandidateMonitor(requestedPlacement, monitors, primary);
        if (!_policy.HasKnownScale(candidate.Monitor))
        {
            return WindowPlacementResolution.DoNotApply(WindowPlacementResolutionStatus.FallbackBecauseMonitorScaleUnavailable);
        }

        if (!_policy.HasPositiveSize(requestedPlacement) || !_policy.HasRestorablePhysicalSize(requestedPlacement, candidate.Monitor))
        {
            return WindowPlacementResolution.Fallback(
                WindowPlacementResolutionStatus.FallbackBecauseInvalidSize,
                DefaultPlacement(candidate.Monitor));
        }

        if (candidate.Kind == CandidateMonitorKind.RequestedMonitor)
        {
            return ResolveOnMonitor(requestedPlacement, candidate.Monitor);
        }

        if (candidate.Kind == CandidateMonitorKind.IntersectingMonitor)
        {
            var clamped = ClampToMonitor(requestedPlacement, candidate.Monitor);
            return WindowPlacementResolution.Fallback(
                WindowPlacementResolutionStatus.FallbackBecauseMonitorMissing,
                clamped);
        }

        return WindowPlacementResolution.Fallback(
            string.IsNullOrWhiteSpace(requestedPlacement.MonitorDeviceName)
                ? WindowPlacementResolutionStatus.FallbackBecauseOffscreen
                : WindowPlacementResolutionStatus.FallbackBecauseMonitorMissing,
            DefaultPlacement(candidate.Monitor, requestedPlacement.Width, requestedPlacement.Height));
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

    private CandidateMonitor SelectCandidateMonitor(
        WindowPlacementState requestedPlacement,
        IReadOnlyList<MonitorWorkArea> monitors,
        MonitorWorkArea primary)
    {
        if (!string.IsNullOrWhiteSpace(requestedPlacement.MonitorDeviceName))
        {
            var requestedMonitor = monitors.FirstOrDefault(monitor => string.Equals(monitor.DeviceName, requestedPlacement.MonitorDeviceName, StringComparison.OrdinalIgnoreCase));
            if (requestedMonitor is not null)
            {
                return new CandidateMonitor(CandidateMonitorKind.RequestedMonitor, requestedMonitor);
            }
        }

        var intersectingMonitor = monitors.FirstOrDefault(monitor => Intersects(requestedPlacement, monitor));
        if (intersectingMonitor is not null)
        {
            return new CandidateMonitor(CandidateMonitorKind.IntersectingMonitor, intersectingMonitor);
        }

        return new CandidateMonitor(CandidateMonitorKind.PrimaryFallback, primary);
    }

    private WindowPlacementState DefaultPlacement(MonitorWorkArea monitor, int? requestedWidth = null, int? requestedHeight = null)
    {
        var minimumPhysicalWidth = _policy.MinimumPhysicalWidth(monitor);
        var minimumPhysicalHeight = _policy.MinimumPhysicalHeight(monitor);
        var width = requestedWidth is { } requestedW && requestedW >= minimumPhysicalWidth
            ? requestedW
            : _policy.DefaultFallbackPhysicalWidth(monitor);
        var height = requestedHeight is { } requestedH && requestedH >= minimumPhysicalHeight
            ? requestedH
            : _policy.DefaultFallbackPhysicalHeight(monitor);
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
        var width = Math.Min(Math.Max(placement.Width, _policy.MinimumPhysicalWidth(monitor)), monitor.Width);
        var height = Math.Min(Math.Max(placement.Height, _policy.MinimumPhysicalHeight(monitor)), monitor.Height);
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

    private sealed record CandidateMonitor(CandidateMonitorKind Kind, MonitorWorkArea Monitor);

    private enum CandidateMonitorKind
    {
        RequestedMonitor,
        IntersectingMonitor,
        PrimaryFallback
    }
}
