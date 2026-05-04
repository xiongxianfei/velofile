using VeloFile.Core.Listing;
using VeloFile.Core.Persistence;
using VeloFile.Core.Sidebar;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Sidebar;

[TestClass]
[TestCategory("Sidebar")]
public sealed class SidebarStateTests
{
    [TestMethod]
    public void Favorites_are_mutable_and_recent_locations_are_capped_and_dismissible()
    {
        var sidebar = SidebarStateService.Create(
            FavoritesStatePayload.Empty,
            RecentLocationsStatePayload.Empty,
            drives: []);

        sidebar.AddFavorite("Projects", @"D:\projects");
        sidebar.AddFavorite("Downloads", @"C:\Users\Public\Downloads");
        sidebar.RemoveFavorite(@"C:\Users\Public\Downloads");

        Assert.AreEqual(1, sidebar.State.Favorites.Count);
        Assert.AreEqual("Projects", sidebar.State.Favorites.Single().DisplayName);
        Assert.AreEqual(@"D:\projects", sidebar.ToFavoritesPayload().PinnedLocations.Single().Path);

        var baseTime = DateTimeOffset.Parse("2026-05-04T00:00:00Z");
        for (var index = 0; index < 25; index++)
        {
            sidebar.RecordRecent($@"D:\recent\{index:D2}", baseTime.AddMinutes(index));
        }

        Assert.AreEqual(20, sidebar.State.RecentLocations.Count);
        Assert.AreEqual(@"D:\recent\24", sidebar.State.RecentLocations[0].Path);
        Assert.IsFalse(sidebar.State.RecentLocations.Any(item => item.Path == @"D:\recent\00"));

        sidebar.DismissRecent(@"D:\recent\24");

        Assert.AreEqual(19, sidebar.State.RecentLocations.Count);
        Assert.IsFalse(sidebar.State.RecentLocations.Any(item => item.Path == @"D:\recent\24"));
        Assert.AreEqual(19, sidebar.ToRecentLocationsPayload().Entries.Count);
    }

    [TestMethod]
    public void Drive_entries_are_exposed_without_requiring_space_hints()
    {
        var sidebar = SidebarStateService.Create(
            FavoritesStatePayload.Empty,
            RecentLocationsStatePayload.Empty,
            drives:
            [
                new DriveEntry(
                    Name: @"C:\",
                    RootPath: @"C:\",
                    DriveType: DriveType.Fixed,
                    IsReady: false,
                    AvailableFreeSpaceBytes: null,
                    TotalSizeBytes: null,
                    HintStatus: DriveHintStatus.Loading),
                new DriveEntry(
                    Name: @"Z:\",
                    RootPath: @"Z:\",
                    DriveType: DriveType.Network,
                    IsReady: false,
                    AvailableFreeSpaceBytes: null,
                    TotalSizeBytes: null,
                    HintStatus: DriveHintStatus.TimedOut)
            ]);

        Assert.AreEqual(2, sidebar.State.Drives.Count);
        Assert.IsTrue(sidebar.State.Drives.All(drive => drive.AvailableFreeSpaceBytes is null));
        Assert.AreEqual(DriveHintStatus.TimedOut, sidebar.State.Drives.Single(drive => drive.RootPath == @"Z:\").HintStatus);
    }
}
