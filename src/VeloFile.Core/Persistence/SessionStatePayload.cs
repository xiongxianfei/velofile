namespace VeloFile.Core.Persistence;

public sealed record SessionStatePayload(
    IReadOnlyList<SessionTabState> Tabs,
    int ActiveTabIndex,
    WindowPlacementState? WindowPlacement)
{
    public static SessionStatePayload Empty { get; } = new([], ActiveTabIndex: 0, WindowPlacement: null);

    public bool RestoresSelection => false;

    public bool RestoresFilterText => false;
}

public sealed record SessionTabState(
    string Path,
    string SortColumn,
    string SortDirection,
    string ViewMode,
    string? ScrollAnchorName,
    IReadOnlyList<string> BackHistory,
    IReadOnlyList<string> ForwardHistory);

public sealed record WindowPlacementState(
    int Left,
    int Top,
    int Width,
    int Height,
    string? MonitorDeviceName);
