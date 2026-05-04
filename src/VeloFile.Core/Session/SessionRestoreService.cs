using VeloFile.Core.Listing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Session;

public interface IPathExistenceProbe
{
    bool Exists(string path);
}

public interface IScrollAnchorResolver
{
    bool Exists(string path, string anchorName);
}

public interface IMonitorPlacementResolver
{
    bool IsAvailable(string? monitorDeviceName);

    WindowPlacementState? Fallback(WindowPlacementState? requestedPlacement);
}

public interface ICrashRecoverySignal
{
    bool ShouldOfferStartFresh { get; }
}

public sealed record CrashRecoveryState(bool StartFreshOffered, string? ReasonCode)
{
    public static CrashRecoveryState None { get; } = new(StartFreshOffered: false, ReasonCode: null);
}

public sealed record SessionRestoreResult(
    NavigationWorkspace Workspace,
    SidebarState Sidebar,
    VisibilitySettings Visibility,
    WindowPlacementState? WindowPlacement,
    CrashRecoveryState CrashRecovery)
{
    public bool RestoresSelection => false;

    public bool RestoresFilterText => false;

    public bool RestoresSearchState => false;

    public bool RestoresClipboard => false;

    public bool RestoresInFlightOperations => false;
}

public sealed class SessionRestoreService
{
    private readonly IPathExistenceProbe _pathExistenceProbe;
    private readonly IMonitorPlacementResolver _monitorPlacementResolver;
    private readonly IScrollAnchorResolver _scrollAnchorResolver;
    private readonly ICrashRecoverySignal _crashRecoverySignal;

    public SessionRestoreService(
        IPathExistenceProbe pathExistenceProbe,
        IMonitorPlacementResolver monitorPlacementResolver,
        IScrollAnchorResolver scrollAnchorResolver,
        ICrashRecoverySignal crashRecoverySignal)
    {
        _pathExistenceProbe = pathExistenceProbe;
        _monitorPlacementResolver = monitorPlacementResolver;
        _scrollAnchorResolver = scrollAnchorResolver;
        _crashRecoverySignal = crashRecoverySignal;
    }

    public SessionRestoreResult Restore(
        SessionStatePayload session,
        FavoritesStatePayload favorites,
        RecentLocationsStatePayload recentLocations,
        SettingsStatePayload settings)
    {
        var restoredTabs = session.Tabs
            .Select((tab, index) => RestoreTab(tab, $"tab-{index + 1:D4}"))
            .ToArray();

        var workspace = restoredTabs.Length == 0
            ? NavigationWorkspace.FromRestoredTabs([], activeTabIndex: 0)
            : NavigationWorkspace.FromRestoredTabs(restoredTabs, session.ActiveTabIndex);
        var sidebar = SidebarStateService.Create(favorites, recentLocations, drives: []);
        var visibility = VisibilitySettingsService.FromPayload(settings).Settings;
        var placement = RestoreWindowPlacement(session.WindowPlacement);
        var crashRecovery = _crashRecoverySignal.ShouldOfferStartFresh
            ? new CrashRecoveryState(StartFreshOffered: true, ReasonCode: "repeated-crash-marker")
            : CrashRecoveryState.None;

        return new SessionRestoreResult(
            workspace,
            sidebar.State,
            visibility,
            placement,
            crashRecovery);
    }

    private NavigationTab RestoreTab(SessionTabState tab, string id)
    {
        var pathExists = _pathExistenceProbe.Exists(tab.Path);
        var scrollAnchor = pathExists && tab.ScrollAnchorName is { Length: > 0 } anchorName && _scrollAnchorResolver.Exists(tab.Path, anchorName)
            ? tab.ScrollAnchorName
            : null;

        return NavigationTab.Restored(
            id,
            tab.Path,
            tab.BackHistory,
            tab.ForwardHistory,
            tab.SortColumn,
            tab.SortDirection,
            ParseViewMode(tab.ViewMode),
            scrollAnchor,
            missingLocation: !pathExists);
    }

    private WindowPlacementState? RestoreWindowPlacement(WindowPlacementState? placement)
    {
        if (placement is null)
        {
            return null;
        }

        if (placement.MonitorDeviceName is null || _monitorPlacementResolver.IsAvailable(placement.MonitorDeviceName))
        {
            return placement;
        }

        return _monitorPlacementResolver.Fallback(placement);
    }

    private static FileListViewMode ParseViewMode(string viewMode)
    {
        return viewMode switch
        {
            "largeIcons" or "LargeIcons" or "large-icons" => FileListViewMode.LargeIcons,
            "list" or "List" => FileListViewMode.List,
            _ => FileListViewMode.Details
        };
    }
}
