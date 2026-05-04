using VeloFile.Core.Persistence;
using VeloFile.Core.Session;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Session;

[TestClass]
[TestCategory("Session")]
public sealed class WindowPlacementResolverTests
{
    [TestMethod]
    public void Valid_restored_placement_is_preserved_when_monitor_is_available()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 100, Top: 120, Width: 1200, Height: 800, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.UseResolvedPlacement, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(requested, resolved.Placement);
    }

    [TestMethod]
    public void Removed_monitor_and_offscreen_bounds_fall_back_to_primary_work_area()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY2", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 5000, Top: 5000, Width: 1200, Height: 800, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseMonitorMissing, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(@"\\.\DISPLAY2", resolved.Placement!.MonitorDeviceName);
        Assert.AreEqual(0, resolved.Placement.Left);
        Assert.AreEqual(0, resolved.Placement.Top);
        Assert.AreEqual(1200, resolved.Placement.Width);
        Assert.AreEqual(800, resolved.Placement.Height);
    }

    [TestMethod]
    public void Offscreen_placement_on_available_monitor_is_clamped_into_visible_work_area()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 1800, Top: 1000, Width: 1000, Height: 700, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseClamped, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(@"\\.\DISPLAY1", resolved.Placement!.MonitorDeviceName);
        Assert.AreEqual(920, resolved.Placement.Left);
        Assert.AreEqual(380, resolved.Placement.Top);
        Assert.AreEqual(1000, resolved.Placement.Width);
        Assert.AreEqual(700, resolved.Placement.Height);
    }

    [TestMethod]
    public void Invalid_dimensions_use_default_visible_placement()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 10, Top: 10, Width: 0, Height: -20, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseInvalidSize, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(@"\\.\DISPLAY1", resolved.Placement!.MonitorDeviceName);
        Assert.AreEqual(0, resolved.Placement.Left);
        Assert.AreEqual(0, resolved.Placement.Top);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackWidth, resolved.Placement.Width);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackHeight, resolved.Placement.Height);
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(100, 100)]
    [DataRow(899, 560)]
    [DataRow(900, 559)]
    public void Below_minimum_positive_dimensions_use_default_visible_placement(int width, int height)
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 300, Top: 200, Width: width, Height: height, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseInvalidSize, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(0, resolved.Placement!.Left);
        Assert.AreEqual(0, resolved.Placement.Top);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackWidth, resolved.Placement.Width);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackHeight, resolved.Placement.Height);
    }

    [TestMethod]
    [DataRow(900, 560)]
    [DataRow(901, 561)]
    public void Minimum_or_larger_dimensions_are_accepted_when_visible(int width, int height)
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 100, Top: 100, Width: width, Height: height, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.UseResolvedPlacement, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(requested, resolved.Placement);
    }

    [TestMethod]
    public void Partially_offscreen_below_minimum_placement_uses_default_instead_of_tiny_clamp()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 1800, Top: 1000, Width: 100, Height: 100, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseInvalidSize, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(0, resolved.Placement!.Left);
        Assert.AreEqual(0, resolved.Placement.Top);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackWidth, resolved.Placement.Width);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackHeight, resolved.Placement.Height);
    }

    [TestMethod]
    public void Below_minimum_placement_on_missing_monitor_uses_safe_primary_default()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY2", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 5000, Top: 5000, Width: 100, Height: 100, MonitorDeviceName: @"\\.\REMOVED");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseInvalidSize, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(@"\\.\DISPLAY2", resolved.Placement!.MonitorDeviceName);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackWidth, resolved.Placement.Width);
        Assert.AreEqual(WindowPlacementPolicy.Default.DefaultFallbackHeight, resolved.Placement.Height);
    }

    [TestMethod]
    public void Empty_monitor_source_does_not_return_stale_requested_placement()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([]));
        var requested = new WindowPlacementState(Left: 5000, Top: 5000, Width: 1200, Height: 800, MonitorDeviceName: @"\\.\REMOVED");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseMonitorEnumerationEmpty, resolved.Status);
        Assert.IsFalse(resolved.ShouldApply);
        Assert.IsNull(resolved.Placement);
    }

    [TestMethod]
    public void Throwing_monitor_source_does_not_return_stale_requested_placement()
    {
        var resolver = new MonitorWindowPlacementResolver(new ThrowingMonitorLayoutSource());
        var requested = new WindowPlacementState(Left: 5000, Top: 5000, Width: 1200, Height: 800, MonitorDeviceName: @"\\.\REMOVED");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseMonitorEnumerationFailed, resolved.Status);
        Assert.IsFalse(resolved.ShouldApply);
        Assert.IsNull(resolved.Placement);
    }

    [TestMethod]
    public void Oversized_placement_clamps_without_going_below_minimum_when_possible()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1000, Height: 700, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 200, Top: 100, Width: 2400, Height: 1800, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseClamped, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(0, resolved.Placement!.Left);
        Assert.AreEqual(0, resolved.Placement.Top);
        Assert.AreEqual(1000, resolved.Placement.Width);
        Assert.AreEqual(700, resolved.Placement.Height);
        Assert.IsTrue(resolved.Placement.Width >= WindowPlacementPolicy.Default.MinimumRestorableWidth);
        Assert.IsTrue(resolved.Placement.Height >= WindowPlacementPolicy.Default.MinimumRestorableHeight);
    }

    [TestMethod]
    public void Constrained_work_area_uses_largest_visible_default_when_minimum_cannot_fit()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 800, Height: 500, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 10, Top: 10, Width: 100, Height: 100, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.AreEqual(WindowPlacementResolutionStatus.FallbackBecauseInvalidSize, resolved.Status);
        Assert.IsTrue(resolved.ShouldApply);
        Assert.AreEqual(800, resolved.Placement!.Width);
        Assert.AreEqual(500, resolved.Placement.Height);
    }

    private sealed class FakeMonitorLayoutSource : IMonitorLayoutSource
    {
        private readonly IReadOnlyList<MonitorWorkArea> _monitors;

        public FakeMonitorLayoutSource(IReadOnlyList<MonitorWorkArea> monitors)
        {
            _monitors = monitors;
        }

        public IReadOnlyList<MonitorWorkArea> GetCurrentWorkAreas()
        {
            return _monitors;
        }
    }

    private sealed class ThrowingMonitorLayoutSource : IMonitorLayoutSource
    {
        public IReadOnlyList<MonitorWorkArea> GetCurrentWorkAreas()
        {
            throw new InvalidOperationException("monitor enumeration failed");
        }
    }
}
