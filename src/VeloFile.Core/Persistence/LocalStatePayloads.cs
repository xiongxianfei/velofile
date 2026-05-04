namespace VeloFile.Core.Persistence;

public sealed record SettingsStatePayload(
    bool ShowHiddenFiles,
    bool ShowProtectedOperatingSystemFiles,
    bool ShowFileExtensions)
{
    public static SettingsStatePayload Default { get; } = new(
        ShowHiddenFiles: false,
        ShowProtectedOperatingSystemFiles: false,
        ShowFileExtensions: true);
}

public sealed record FavoritesStatePayload(IReadOnlyList<PinnedLocationState> PinnedLocations)
{
    public static FavoritesStatePayload Empty { get; } = new([]);
}

public sealed record PinnedLocationState(string DisplayName, string Path);

public sealed record RecentLocationsStatePayload(IReadOnlyList<RecentLocationState> Entries)
{
    public const int MaxEntries = 20;

    public static RecentLocationsStatePayload Empty { get; } = new([]);

    public static RecentLocationsStatePayload Create(IEnumerable<RecentLocationState> entries)
    {
        return new RecentLocationsStatePayload(
            entries
                .OrderByDescending(entry => entry.LastVisitedUtc)
                .ThenBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
                .Take(MaxEntries)
                .ToArray());
    }
}

public sealed record RecentLocationState(string Path, DateTimeOffset LastVisitedUtc);
