using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Shell;

[TestClass]
[TestCategory("Navigation")]
[TestCategory("Session")]
public sealed class AppShellCommandSurfaceTests
{
    [TestMethod]
    public void Navigation_entry_points_mutate_the_active_tab_through_one_command_surface()
    {
        var favorite = new PinnedLocationState("Projects", @"D:\projects");
        var recent = new RecentLocationState(@"D:\recent", DateTimeOffset.Parse("2026-05-03T00:00:00Z"));
        var drive = new DriveEntry(@"E:\", @"E:\", DriveType.Fixed, IsReady: false, AvailableFreeSpaceBytes: null, TotalSizeBytes: null);
        var shell = CreateShell(
            initialPath: @"D:\start",
            favorites: [favorite],
            recents: [recent],
            drives: [drive],
            existingPaths: [@"D:\typed", @"D:\projects", @"D:\recent", @"E:\"]);

        shell.SubmitPath(@"D:\typed");
        Assert.AreEqual(@"D:\typed", shell.ActivePath);

        shell.ActivateSidebarTarget(shell.SidebarNavigationTargets.Single(target => target.Kind == ShellNavigationTargetKind.Favorite));
        Assert.AreEqual(@"D:\projects", shell.ActivePath);

        shell.ActivateSidebarTarget(shell.SidebarNavigationTargets.First(target => target.Kind == ShellNavigationTargetKind.RecentLocation && target.Path == @"D:\recent"));
        Assert.AreEqual(@"D:\recent", shell.ActivePath);

        shell.ActivateSidebarTarget(shell.SidebarNavigationTargets.Single(target => target.Kind == ShellNavigationTargetKind.Drive));
        Assert.AreEqual(@"E:\", shell.ActivePath);
        Assert.IsTrue(shell.RecentLocations.Any(item => item.Path == @"E:\"));
    }

    [TestMethod]
    public void History_parent_refresh_tab_lifecycle_and_reopen_are_command_surface_behavior()
    {
        var shell = CreateShell(
            initialPath: @"D:\start",
            existingPaths: [@"D:\start", @"D:\start\child", @"D:\other"]);

        shell.SubmitPath(@"D:\start\child");
        Assert.AreEqual(@"D:\start\child", shell.ActivePath);

        shell.NavigateBack();
        Assert.AreEqual(@"D:\start", shell.ActivePath);

        shell.NavigateForward();
        Assert.AreEqual(@"D:\start\child", shell.ActivePath);

        shell.NavigateToParent();
        Assert.AreEqual(@"D:\start", shell.ActivePath);

        var historyBeforeRefresh = shell.ActiveTab.BackHistory.ToArray();
        var reloadVersionBefore = shell.ActiveTab.ReloadVersion;
        shell.RefreshActiveTab();
        CollectionAssert.AreEqual(historyBeforeRefresh, shell.ActiveTab.BackHistory.ToArray());
        Assert.AreEqual(reloadVersionBefore + 1, shell.ActiveTab.ReloadVersion);

        shell.NewTab();
        Assert.AreEqual(2, shell.Tabs.Count);
        Assert.AreEqual(@"D:\start", shell.ActivePath);

        shell.SubmitPath(@"D:\other");
        shell.DuplicateActiveTab();
        Assert.AreEqual(3, shell.Tabs.Count);
        Assert.AreEqual(@"D:\other", shell.ActivePath);

        shell.CloseActiveTab();
        Assert.AreEqual(2, shell.Tabs.Count);
        Assert.AreEqual(@"D:\other", shell.ActivePath);

        shell.ReopenClosedTab();
        Assert.AreEqual(3, shell.Tabs.Count);
        Assert.AreEqual(@"D:\other", shell.ActivePath);
    }

    [TestMethod]
    public void Missing_path_navigation_surfaces_recoverable_active_tab_state()
    {
        var shell = CreateShell(initialPath: @"D:\start", existingPaths: [@"D:\start"]);

        shell.SubmitPath(@"Z:\missing");

        Assert.AreEqual(@"Z:\missing", shell.ActivePath);
        Assert.AreEqual(NavigationTabLocationState.MissingLocation, shell.ActiveTab.LocationState);
        Assert.AreEqual(@"Z:\missing", shell.MissingLocationPath);
        Assert.IsTrue(shell.MissingLocationVisible);
    }

    [TestMethod]
    public void Start_fresh_replaces_restored_state_with_a_safe_default_workspace()
    {
        var shell = CreateShell(
            initialPath: @"D:\restored",
            defaultPath: @"C:\Users\alice",
            crashRecovery: new CrashRecoveryState(StartFreshOffered: true, ReasonCode: "repeated-crash-marker"),
            existingPaths: [@"D:\restored", @"C:\Users\alice"]);

        shell.StartFresh();

        Assert.IsFalse(shell.CrashRecovery.StartFreshOffered);
        Assert.AreEqual(1, shell.Tabs.Count);
        Assert.AreEqual(@"C:\Users\alice", shell.ActivePath);
    }

    private static AppShellCommandSurface CreateShell(
        string initialPath,
        string? defaultPath = null,
        IReadOnlyList<PinnedLocationState>? favorites = null,
        IReadOnlyList<RecentLocationState>? recents = null,
        IReadOnlyList<DriveEntry>? drives = null,
        CrashRecoveryState? crashRecovery = null,
        IReadOnlyList<string>? existingPaths = null)
    {
        var workspace = NavigationWorkspace.Create(initialPath);
        var sidebar = SidebarStateService.Create(
            new FavoritesStatePayload(favorites ?? []),
            RecentLocationsStatePayload.Create(recents ?? []),
            drives ?? []);
        var visibility = VisibilitySettingsService.FromPayload(SettingsStatePayload.Default);
        var pathProbe = new SetPathExistenceProbe(existingPaths ?? [initialPath]);

        return new AppShellCommandSurface(
            windowTitle: "VeloFile",
            workspace,
            sidebar,
            visibility,
            crashRecovery ?? CrashRecoveryState.None,
            new TestDefaultLaunchPathProvider(defaultPath ?? initialPath),
            pathProbe,
            utcNow: () => DateTimeOffset.Parse("2026-05-04T00:00:00Z"));
    }

    private sealed class SetPathExistenceProbe : IPathExistenceProbe
    {
        private readonly HashSet<string> _existingPaths;

        public SetPathExistenceProbe(IEnumerable<string> existingPaths)
        {
            _existingPaths = new HashSet<string>(existingPaths, StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string path)
        {
            return _existingPaths.Contains(path);
        }
    }

    private sealed class TestDefaultLaunchPathProvider : IDefaultLaunchPathProvider
    {
        private readonly string _path;

        public TestDefaultLaunchPathProvider(string path)
        {
            _path = path;
        }

        public string GetDefaultLaunchPath()
        {
            return _path;
        }
    }
}
