using VeloFile.Core.Listing;

namespace VeloFile.Core.Navigation;

public enum NavigationTabLocationState
{
    Available,
    MissingLocation
}

public sealed record NavigationTab(
    string Id,
    string Path,
    IReadOnlyList<string> BackHistory,
    IReadOnlyList<string> ForwardHistory,
    string SortColumn,
    string SortDirection,
    FileListViewMode ViewMode,
    string? ScrollAnchorName,
    NavigationTabLocationState LocationState,
    string? MissingPath)
{
    public bool CloseTabActionAvailable => LocationState is NavigationTabLocationState.MissingLocation;

    public static NavigationTab Create(string id, string path)
    {
        return new NavigationTab(
            id,
            path,
            BackHistory: [],
            ForwardHistory: [],
            SortColumn: "name",
            SortDirection: "ascending",
            ViewMode: FileListViewMode.Details,
            ScrollAnchorName: null,
            LocationState: NavigationTabLocationState.Available,
            MissingPath: null);
    }

    public static NavigationTab Restored(
        string id,
        string path,
        IReadOnlyList<string> backHistory,
        IReadOnlyList<string> forwardHistory,
        string sortColumn,
        string sortDirection,
        FileListViewMode viewMode,
        string? scrollAnchorName,
        bool missingLocation)
    {
        return new NavigationTab(
            id,
            path,
            backHistory,
            forwardHistory,
            string.IsNullOrWhiteSpace(sortColumn) ? "name" : sortColumn,
            string.IsNullOrWhiteSpace(sortDirection) ? "ascending" : sortDirection,
            viewMode,
            scrollAnchorName,
            missingLocation ? NavigationTabLocationState.MissingLocation : NavigationTabLocationState.Available,
            missingLocation ? path : null);
    }
}
