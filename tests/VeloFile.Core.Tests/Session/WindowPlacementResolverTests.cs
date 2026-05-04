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

        Assert.AreEqual(requested, resolved);
    }

    [TestMethod]
    public void Removed_monitor_and_offscreen_bounds_fall_back_to_primary_work_area()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY2", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 5000, Top: 5000, Width: 1200, Height: 800, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.IsNotNull(resolved);
        Assert.AreEqual(@"\\.\DISPLAY2", resolved!.MonitorDeviceName);
        Assert.AreEqual(0, resolved.Left);
        Assert.AreEqual(0, resolved.Top);
        Assert.AreEqual(1200, resolved.Width);
        Assert.AreEqual(800, resolved.Height);
    }

    [TestMethod]
    public void Offscreen_placement_on_available_monitor_is_clamped_into_visible_work_area()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 1800, Top: 1000, Width: 600, Height: 400, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.IsNotNull(resolved);
        Assert.AreEqual(@"\\.\DISPLAY1", resolved!.MonitorDeviceName);
        Assert.AreEqual(1320, resolved.Left);
        Assert.AreEqual(680, resolved.Top);
        Assert.AreEqual(600, resolved.Width);
        Assert.AreEqual(400, resolved.Height);
    }

    [TestMethod]
    public void Invalid_dimensions_use_default_visible_placement()
    {
        var resolver = new MonitorWindowPlacementResolver(new FakeMonitorLayoutSource([
            new MonitorWorkArea(@"\\.\DISPLAY1", Left: 0, Top: 0, Width: 1920, Height: 1080, IsPrimary: true)
        ]));
        var requested = new WindowPlacementState(Left: 10, Top: 10, Width: 0, Height: -20, MonitorDeviceName: @"\\.\DISPLAY1");

        var resolved = resolver.Resolve(requested);

        Assert.IsNotNull(resolved);
        Assert.AreEqual(@"\\.\DISPLAY1", resolved!.MonitorDeviceName);
        Assert.AreEqual(0, resolved.Left);
        Assert.AreEqual(0, resolved.Top);
        Assert.AreEqual(MonitorWindowPlacementResolver.DefaultWidth, resolved.Width);
        Assert.AreEqual(MonitorWindowPlacementResolver.DefaultHeight, resolved.Height);
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
}
