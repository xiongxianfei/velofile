using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;

namespace VeloFile.Core.Navigation;

public sealed class NavigationEntryPointService
{
    private readonly NavigationWorkspace _workspace;
    private readonly SidebarStateService _sidebar;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly Func<string, bool>? _pathExists;

    public NavigationEntryPointService(
        NavigationWorkspace workspace,
        SidebarStateService sidebar,
        Func<DateTimeOffset> utcNow,
        Func<string, bool>? pathExists = null)
    {
        _workspace = workspace;
        _sidebar = sidebar;
        _utcNow = utcNow;
        _pathExists = pathExists;
    }

    public void OpenTypedPath(string path)
    {
        NavigateAndRecord(path);
    }

    public void OpenPastedPath(string path)
    {
        NavigateAndRecord(path);
    }

    public void OpenBreadcrumbSegment(BreadcrumbSegment segment)
    {
        NavigateAndRecord(segment.FullPath);
    }

    public void OpenSidebarLocation(string path)
    {
        NavigateAndRecord(path);
    }

    public void OpenFavorite(PinnedLocationState favorite)
    {
        NavigateAndRecord(favorite.Path);
    }

    public void OpenRecent(RecentLocationState recentLocation)
    {
        NavigateAndRecord(recentLocation.Path);
    }

    public void OpenDrive(DriveEntry drive)
    {
        NavigateAndRecord(drive.RootPath);
    }

    private void NavigateAndRecord(string path)
    {
        var normalizedPath = path.Trim();
        var missingLocation = _pathExists is not null && !_pathExists(normalizedPath);
        _workspace.NavigateActive(normalizedPath, missingLocation);
        _sidebar.RecordRecent(_workspace.ActiveTab.Path, _utcNow());
    }
}
