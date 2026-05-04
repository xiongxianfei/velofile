using VeloFile.Core;
using VeloFile.Core.Listing;

namespace VeloFile.Windows.FileSystem;

public sealed class WindowsDriveEntrySource : IDriveEntrySource
{
    public IReadOnlyList<DriveEntry> GetDrives()
    {
        var drives = new List<DriveEntry>();

        foreach (var drive in GetDriveInfos())
        {
            drives.Add(CreateDriveEntry(drive));
        }

        return drives;
    }

    private static DriveInfo[] GetDriveInfos()
    {
        try
        {
            return DriveInfo.GetDrives();
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return [];
        }
    }

    private static DriveEntry CreateDriveEntry(DriveInfo drive)
    {
        var isReady = IsReady(drive);
        var name = isReady ? TryReadName(drive) : drive.Name;
        var availableFreeSpace = isReady ? TryReadSpace(drive, static item => item.AvailableFreeSpace) : null;
        var totalSize = isReady ? TryReadSpace(drive, static item => item.TotalSize) : null;

        return new DriveEntry(
            Name: name,
            RootPath: drive.Name,
            DriveType: drive.DriveType,
            IsReady: isReady,
            AvailableFreeSpaceBytes: availableFreeSpace,
            TotalSizeBytes: totalSize);
    }

    private static bool IsReady(DriveInfo drive)
    {
        try
        {
            return drive.IsReady;
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return false;
        }
    }

    private static string TryReadName(DriveInfo drive)
    {
        try
        {
            return string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel;
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return drive.Name;
        }
    }

    private static long? TryReadSpace(DriveInfo drive, Func<DriveInfo, long> read)
    {
        try
        {
            return read(drive);
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return null;
        }
    }
}
