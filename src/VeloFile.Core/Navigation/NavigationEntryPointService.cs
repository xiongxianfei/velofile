using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;

namespace VeloFile.Core.Navigation;

public sealed record NavigationAttemptResult(
    bool Accepted,
    string? SubmittedPath,
    string? ReasonCode)
{
    public static NavigationAttemptResult AcceptedNavigation(string path)
    {
        return new NavigationAttemptResult(Accepted: true, SubmittedPath: path, ReasonCode: null);
    }

    public static NavigationAttemptResult Rejected(string? path, string reasonCode)
    {
        return new NavigationAttemptResult(Accepted: false, SubmittedPath: path, reasonCode);
    }
}

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

    public NavigationAttemptResult OpenTypedPath(string path)
    {
        return NavigateAndRecord(path);
    }

    public NavigationAttemptResult OpenPastedPath(string path)
    {
        return NavigateAndRecord(path);
    }

    public NavigationAttemptResult OpenBreadcrumbSegment(BreadcrumbSegment segment)
    {
        return NavigateAndRecord(segment.FullPath);
    }

    public NavigationAttemptResult OpenSidebarLocation(string path)
    {
        return NavigateAndRecord(path);
    }

    public NavigationAttemptResult OpenFavorite(PinnedLocationState favorite)
    {
        return NavigateAndRecord(favorite.Path);
    }

    public NavigationAttemptResult OpenRecent(RecentLocationState recentLocation)
    {
        return NavigateAndRecord(recentLocation.Path);
    }

    public NavigationAttemptResult OpenDrive(DriveEntry drive)
    {
        return NavigateAndRecord(drive.RootPath);
    }

    private NavigationAttemptResult NavigateAndRecord(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return NavigationAttemptResult.Rejected(path, "empty-path");
        }

        var normalizedPath = path.Trim();
        var validationReason = ValidatePathInput(normalizedPath);
        if (validationReason is not null)
        {
            return NavigationAttemptResult.Rejected(normalizedPath, validationReason);
        }

        if (_pathExists is not null && !_pathExists(normalizedPath))
        {
            return NavigationAttemptResult.Rejected(normalizedPath, "missing");
        }

        _workspace.NavigateActive(normalizedPath);
        _sidebar.RecordRecent(_workspace.ActiveTab.Path, _utcNow());
        return NavigationAttemptResult.AcceptedNavigation(normalizedPath);
    }

    private static string? ValidatePathInput(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && !uri.IsFile)
        {
            return "unsupported-path";
        }

        if (!Path.IsPathFullyQualified(path))
        {
            return "invalid-path";
        }

        try
        {
            _ = Path.GetFullPath(path);
            return null;
        }
        catch (ArgumentException)
        {
            return "invalid-path";
        }
        catch (NotSupportedException)
        {
            return "invalid-path";
        }
    }
}
