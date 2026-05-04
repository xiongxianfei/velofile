using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Session;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Shell;

public enum ShellNavigationTargetKind
{
    Favorite,
    RecentLocation,
    Drive,
    Path
}

public sealed record ShellNavigationTarget(
    ShellNavigationTargetKind Kind,
    string DisplayName,
    string Path);

public sealed class AppShellCommandSurface
{
    private readonly IDefaultLaunchPathProvider _defaultLaunchPathProvider;
    private readonly IPathExistenceProbe _pathExistenceProbe;
    private readonly Func<DateTimeOffset> _utcNow;
    private NavigationEntryPointService _entryPoints;

    public AppShellCommandSurface(
        string windowTitle,
        NavigationWorkspace workspace,
        SidebarStateService sidebar,
        VisibilitySettingsService visibility,
        CrashRecoveryState crashRecovery,
        IDefaultLaunchPathProvider defaultLaunchPathProvider,
        IPathExistenceProbe pathExistenceProbe,
        Func<DateTimeOffset> utcNow)
    {
        WindowTitle = windowTitle;
        Workspace = workspace;
        Sidebar = sidebar;
        Visibility = visibility;
        CrashRecovery = crashRecovery;
        _defaultLaunchPathProvider = defaultLaunchPathProvider;
        _pathExistenceProbe = pathExistenceProbe;
        _utcNow = utcNow;
        _entryPoints = CreateEntryPointService();
    }

    public string WindowTitle { get; }

    public NavigationWorkspace Workspace { get; private set; }

    public SidebarStateService Sidebar { get; }

    public VisibilitySettingsService Visibility { get; }

    public CrashRecoveryState CrashRecovery { get; private set; }

    public IReadOnlyList<NavigationTab> Tabs => Workspace.Tabs;

    public NavigationTab ActiveTab => Workspace.ActiveTab;

    public int ActiveTabIndex => Workspace.ActiveTabIndex;

    public string ActivePath => Workspace.ActiveTab.Path;

    public IReadOnlyList<BreadcrumbSegment> BreadcrumbSegments => BreadcrumbPath.Parse(ActivePath);

    public bool CanNavigateBack => ActiveTab.BackHistory.Count > 0;

    public bool CanNavigateForward => ActiveTab.ForwardHistory.Count > 0;

    public bool MissingLocationVisible => ActiveTab.LocationState is NavigationTabLocationState.MissingLocation;

    public string? MissingLocationPath => ActiveTab.MissingPath;

    public IReadOnlyList<PinnedLocationState> Favorites => Sidebar.State.Favorites;

    public IReadOnlyList<RecentLocationState> RecentLocations => Sidebar.State.RecentLocations;

    public IReadOnlyList<DriveEntry> Drives => Sidebar.State.Drives;

    public VisibilitySettings VisibilitySettings => Visibility.Settings;

    public IReadOnlyList<ShellNavigationTarget> SidebarNavigationTargets
    {
        get
        {
            var targets = new List<ShellNavigationTarget>();
            targets.AddRange(Favorites.Select(favorite => new ShellNavigationTarget(ShellNavigationTargetKind.Favorite, favorite.DisplayName, favorite.Path)));
            targets.AddRange(RecentLocations.Select(recent => new ShellNavigationTarget(ShellNavigationTargetKind.RecentLocation, recent.Path, recent.Path)));
            targets.AddRange(Drives.Select(drive => new ShellNavigationTarget(ShellNavigationTargetKind.Drive, string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.RootPath : $"{drive.VolumeLabel} ({drive.RootPath})", drive.RootPath)));
            return targets;
        }
    }

    public void SubmitPath(string path)
    {
        _entryPoints.OpenTypedPath(path);
    }

    public void ActivateSidebarTarget(ShellNavigationTarget target)
    {
        _entryPoints.OpenSidebarLocation(target.Path);
    }

    public bool NavigateBack()
    {
        return Workspace.NavigateBack();
    }

    public bool NavigateForward()
    {
        return Workspace.NavigateForward();
    }

    public bool NavigateToParent()
    {
        var parent = TryGetParentPath(ActivePath);
        if (parent is null)
        {
            return false;
        }

        SubmitPath(parent);
        return true;
    }

    public void RefreshActiveTab()
    {
        Workspace.RefreshActive();
    }

    public NavigationTab NewTab()
    {
        return Workspace.OpenTab(_defaultLaunchPathProvider.GetDefaultLaunchPath());
    }

    public NavigationTab DuplicateActiveTab()
    {
        return Workspace.DuplicateTab(Workspace.ActiveTab.Id);
    }

    public void CloseActiveTab()
    {
        Workspace.CloseTab(Workspace.ActiveTab.Id);
    }

    public NavigationTab? ReopenClosedTab()
    {
        return Workspace.ReopenClosedTab();
    }

    public void SwitchToTab(int index)
    {
        Workspace.SwitchToTab(index);
    }

    public void SwitchNextTab()
    {
        Workspace.SwitchNextTab();
    }

    public void SwitchPreviousTab()
    {
        Workspace.SwitchPreviousTab();
    }

    public void ReorderTab(string tabId, int newIndex)
    {
        Workspace.ReorderTab(tabId, newIndex);
    }

    public void SetShowHiddenFiles(bool show)
    {
        Visibility.SetShowHiddenFiles(show);
    }

    public void SetShowFileExtensions(bool show)
    {
        Visibility.SetShowFileExtensions(show);
    }

    public VisibilityChangeStatus SetShowProtectedOperatingSystemFiles(bool show, bool confirmed)
    {
        return Visibility.SetShowProtectedOperatingSystemFiles(show, confirmed);
    }

    public void StartFresh()
    {
        Workspace = NavigationWorkspace.Create(_defaultLaunchPathProvider.GetDefaultLaunchPath());
        CrashRecovery = CrashRecoveryState.None;
        _entryPoints = CreateEntryPointService();
    }

    private NavigationEntryPointService CreateEntryPointService()
    {
        return new NavigationEntryPointService(Workspace, Sidebar, _utcNow, _pathExistenceProbe.Exists);
    }

    private static string? TryGetParentPath(string path)
    {
        var normalized = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var root = Path.GetPathRoot(path);
        if (!string.IsNullOrWhiteSpace(root) && string.Equals(
            normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Directory.GetParent(normalized)?.FullName;
    }
}
