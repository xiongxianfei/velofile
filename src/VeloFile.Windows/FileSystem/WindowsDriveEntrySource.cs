using VeloFile.Core;
using VeloFile.Core.Listing;

namespace VeloFile.Windows.FileSystem;

public sealed class WindowsDriveEntrySource : IDriveEntrySource, IDriveHintSource
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

    public Task<DriveHint> GetHintAsync(string rootPath, CancellationToken cancellationToken)
    {
        return Task.Run(() => ReadHint(rootPath, cancellationToken), cancellationToken);
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
        return new DriveEntry(
            Name: drive.Name,
            RootPath: drive.Name,
            DriveType: drive.DriveType,
            IsReady: false,
            AvailableFreeSpaceBytes: null,
            TotalSizeBytes: null);
    }

    private static DriveHint ReadHint(string rootPath, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var drive = new DriveInfo(rootPath);
            if (!drive.IsReady)
            {
                return DriveHint.Unavailable();
            }

            var label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? null : drive.VolumeLabel;
            return DriveHint.Available(
                volumeLabel: label,
                availableFreeSpaceBytes: drive.AvailableFreeSpace,
                totalSizeBytes: drive.TotalSize);
        }
        catch (OperationCanceledException)
        {
            return DriveHint.Cancelled();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
        {
            return DriveHint.AccessDenied();
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return DriveHint.Unavailable();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            return DriveHint.Unavailable();
        }
    }
}
