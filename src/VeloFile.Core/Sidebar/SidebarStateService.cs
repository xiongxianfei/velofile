using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;

namespace VeloFile.Core.Sidebar;

public sealed record SidebarState(
    IReadOnlyList<PinnedLocationState> Favorites,
    IReadOnlyList<RecentLocationState> RecentLocations,
    IReadOnlyList<DriveEntry> Drives);

public sealed class SidebarStateService
{
    private readonly List<PinnedLocationState> _favorites;
    private readonly List<RecentLocationState> _recentLocations;
    private IReadOnlyList<DriveEntry> _drives;

    private SidebarStateService(
        IEnumerable<PinnedLocationState> favorites,
        IEnumerable<RecentLocationState> recentLocations,
        IReadOnlyList<DriveEntry> drives)
    {
        _favorites = favorites.ToList();
        _recentLocations = RecentLocationsStatePayload.Create(recentLocations).Entries.ToList();
        _drives = drives.ToArray();
    }

    public SidebarState State => new(
        Favorites: _favorites.ToArray(),
        RecentLocations: _recentLocations.ToArray(),
        Drives: _drives);

    public static SidebarStateService Create(
        FavoritesStatePayload favorites,
        RecentLocationsStatePayload recentLocations,
        IReadOnlyList<DriveEntry> drives)
    {
        return new SidebarStateService(favorites.PinnedLocations, recentLocations.Entries, drives);
    }

    public void AddFavorite(string displayName, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        RemoveFavorite(path);
        _favorites.Add(new PinnedLocationState(
            string.IsNullOrWhiteSpace(displayName) ? Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) : displayName.Trim(),
            path.Trim()));
    }

    public void RemoveFavorite(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _favorites.RemoveAll(location => string.Equals(location.Path, path.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void RecordRecent(string path, DateTimeOffset visitedUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var normalizedPath = path.Trim();
        _recentLocations.RemoveAll(location => string.Equals(location.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));
        _recentLocations.Add(new RecentLocationState(normalizedPath, visitedUtc));

        var capped = RecentLocationsStatePayload.Create(_recentLocations);
        _recentLocations.Clear();
        _recentLocations.AddRange(capped.Entries);
    }

    public void DismissRecent(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _recentLocations.RemoveAll(location => string.Equals(location.Path, path.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void SetDrives(IReadOnlyList<DriveEntry> drives)
    {
        _drives = drives.ToArray();
    }

    public FavoritesStatePayload ToFavoritesPayload()
    {
        return new FavoritesStatePayload(_favorites.ToArray());
    }

    public RecentLocationsStatePayload ToRecentLocationsPayload()
    {
        return RecentLocationsStatePayload.Create(_recentLocations);
    }
}
