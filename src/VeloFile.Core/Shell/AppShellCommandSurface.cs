using VeloFile.Core.Diagnostics;
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

public sealed record PathEntryError(
    string SubmittedPath,
    string ReasonCode);

public sealed record PathSubmissionResult(
    bool Accepted,
    PathEntryError? Error)
{
    public static PathSubmissionResult AcceptedNavigation { get; } = new(Accepted: true, Error: null);

    public static PathSubmissionResult Rejected(PathEntryError error)
    {
        return new PathSubmissionResult(Accepted: false, error);
    }
}

public sealed class AppShellCommandSurface
{
    private readonly IDefaultLaunchPathProvider _defaultLaunchPathProvider;
    private readonly IPathExistenceProbe _pathExistenceProbe;
    private readonly ISettingsStateWriter _settingsStateWriter;
    private readonly IDiagnosticSink? _diagnostics;
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
        : this(
            windowTitle,
            workspace,
            sidebar,
            visibility,
            crashRecovery,
            defaultLaunchPathProvider,
            pathExistenceProbe,
            NoOpSettingsStateWriter.Instance,
            utcNow,
            diagnostics: null)
    {
    }

    public AppShellCommandSurface(
        string windowTitle,
        NavigationWorkspace workspace,
        SidebarStateService sidebar,
        VisibilitySettingsService visibility,
        CrashRecoveryState crashRecovery,
        IDefaultLaunchPathProvider defaultLaunchPathProvider,
        IPathExistenceProbe pathExistenceProbe,
        ISettingsStateWriter settingsStateWriter,
        Func<DateTimeOffset> utcNow,
        IDiagnosticSink? diagnostics = null)
    {
        WindowTitle = windowTitle;
        Workspace = workspace;
        Sidebar = sidebar;
        Visibility = visibility;
        CrashRecovery = crashRecovery;
        _defaultLaunchPathProvider = defaultLaunchPathProvider;
        _pathExistenceProbe = pathExistenceProbe;
        _settingsStateWriter = settingsStateWriter;
        _diagnostics = diagnostics;
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

    public PathEntryError? PathEntryError { get; private set; }

    public bool PathEntryErrorVisible => PathEntryError is not null;

    public IReadOnlyList<PinnedLocationState> Favorites => Sidebar.State.Favorites;

    public IReadOnlyList<RecentLocationState> RecentLocations => Sidebar.State.RecentLocations;

    public IReadOnlyList<DriveEntry> Drives => Sidebar.State.Drives;

    public VisibilitySettings VisibilitySettings => Visibility.Settings;

    public string? PreferredTerminalTargetId => Visibility.PreferredTerminalTargetId;

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

    public PathSubmissionResult SubmitPath(string path)
    {
        return ApplyNavigationAttempt(_entryPoints.OpenTypedPath(path));
    }

    public PathSubmissionResult ActivateSidebarTarget(ShellNavigationTarget target)
    {
        return ApplyNavigationAttempt(_entryPoints.OpenSidebarLocation(target.Path));
    }

    public bool NavigateBack()
    {
        var navigated = Workspace.NavigateBack();
        if (navigated)
        {
            PathEntryError = null;
        }

        return navigated;
    }

    public bool NavigateForward()
    {
        var navigated = Workspace.NavigateForward();
        if (navigated)
        {
            PathEntryError = null;
        }

        return navigated;
    }

    public bool NavigateToParent()
    {
        var parent = TryGetParentPath(ActivePath);
        if (parent is null)
        {
            return false;
        }

        return SubmitPath(parent).Accepted;
    }

    public void RefreshActiveTab()
    {
        PathEntryError = null;
        Workspace.RefreshActive();
    }

    public NavigationTab NewTab()
    {
        PathEntryError = null;
        return Workspace.OpenTab(_defaultLaunchPathProvider.GetDefaultLaunchPath());
    }

    public NavigationTab DuplicateActiveTab()
    {
        PathEntryError = null;
        return Workspace.DuplicateTab(Workspace.ActiveTab.Id);
    }

    public void CloseActiveTab()
    {
        PathEntryError = null;
        Workspace.CloseTab(Workspace.ActiveTab.Id);
    }

    public NavigationTab? ReopenClosedTab()
    {
        PathEntryError = null;
        return Workspace.ReopenClosedTab();
    }

    public void SwitchToTab(int index)
    {
        PathEntryError = null;
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
        PersistVisibilitySettings();
    }

    public void SetShowFileExtensions(bool show)
    {
        Visibility.SetShowFileExtensions(show);
        PersistVisibilitySettings();
    }

    public VisibilityChangeStatus SetShowProtectedOperatingSystemFiles(bool show, bool confirmed)
    {
        var status = Visibility.SetShowProtectedOperatingSystemFiles(show, confirmed);
        if (status is VisibilityChangeStatus.Applied)
        {
            PersistVisibilitySettings();
        }

        return status;
    }

    public void SetPreferredTerminalTargetId(string? targetId)
    {
        Visibility.SetPreferredTerminalTargetId(targetId);
        PersistVisibilitySettings();
    }

    public void StartFresh()
    {
        Workspace = NavigationWorkspace.Create(_defaultLaunchPathProvider.GetDefaultLaunchPath());
        CrashRecovery = CrashRecoveryState.None;
        PathEntryError = null;
        _entryPoints = CreateEntryPointService();
    }

    public void ClearPathEntryError()
    {
        PathEntryError = null;
    }

    private NavigationEntryPointService CreateEntryPointService()
    {
        return new NavigationEntryPointService(Workspace, Sidebar, _utcNow, _pathExistenceProbe.Exists);
    }

    private PathSubmissionResult ApplyNavigationAttempt(NavigationAttemptResult attempt)
    {
        if (attempt.Accepted)
        {
            PathEntryError = null;
            return PathSubmissionResult.AcceptedNavigation;
        }

        var error = new PathEntryError(
            attempt.SubmittedPath ?? string.Empty,
            attempt.ReasonCode ?? "unknown");
        PathEntryError = error;
        return PathSubmissionResult.Rejected(error);
    }

    private void PersistVisibilitySettings()
    {
        try
        {
            _settingsStateWriter.Write(Visibility.ToPayload());
        }
        catch (Exception ex)
        {
            TryWriteSettingsPersistenceDiagnostic(ex);
        }
    }

    private void TryWriteSettingsPersistenceDiagnostic(Exception exception)
    {
        if (_diagnostics is null)
        {
            return;
        }

        try
        {
            _diagnostics.Write(new DiagnosticEvent
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = "operation.failure",
                UtcTimestamp = _utcNow(),
                SequenceNumber = 0,
                Severity = "warning",
                Component = "persistence",
                OperationKind = "write",
                ResultState = "failed",
                ReasonCode = ExpectedFileSystemExceptions.IsExpected(exception)
                    ? ExpectedFileSystemExceptions.ReasonCode(exception)
                    : "unknown",
                DocumentType = DurableDocumentTypes.Settings
            });
        }
        catch
        {
            // Diagnostics are best-effort and must not break shell commands.
        }
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
