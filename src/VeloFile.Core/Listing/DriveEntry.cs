namespace VeloFile.Core.Listing;

public sealed record DriveEntry(
    string Name,
    string RootPath,
    DriveType DriveType,
    bool IsReady,
    long? AvailableFreeSpaceBytes,
    long? TotalSizeBytes,
    DriveHintStatus HintStatus = DriveHintStatus.NotRequested,
    string? VolumeLabel = null);

public interface IDriveEntrySource
{
    IReadOnlyList<DriveEntry> GetDrives();
}

public interface IDriveHintSource
{
    Task<DriveHint> GetHintAsync(string rootPath, CancellationToken cancellationToken);
}

public enum DriveHintStatus
{
    NotRequested,
    Loading,
    Available,
    Unavailable,
    TimedOut,
    AccessDenied,
    Cancelled
}

public sealed record DriveHint(
    DriveHintStatus Status,
    bool IsReady,
    string? VolumeLabel,
    long? AvailableFreeSpaceBytes,
    long? TotalSizeBytes)
{
    public static DriveHint Available(
        string? volumeLabel,
        long? availableFreeSpaceBytes,
        long? totalSizeBytes)
    {
        return new DriveHint(
            DriveHintStatus.Available,
            IsReady: true,
            volumeLabel,
            availableFreeSpaceBytes,
            totalSizeBytes);
    }

    public static DriveHint Unavailable()
    {
        return new DriveHint(DriveHintStatus.Unavailable, IsReady: false, VolumeLabel: null, AvailableFreeSpaceBytes: null, TotalSizeBytes: null);
    }

    public static DriveHint AccessDenied()
    {
        return new DriveHint(DriveHintStatus.AccessDenied, IsReady: false, VolumeLabel: null, AvailableFreeSpaceBytes: null, TotalSizeBytes: null);
    }

    public static DriveHint TimedOut()
    {
        return new DriveHint(DriveHintStatus.TimedOut, IsReady: false, VolumeLabel: null, AvailableFreeSpaceBytes: null, TotalSizeBytes: null);
    }

    public static DriveHint Cancelled()
    {
        return new DriveHint(DriveHintStatus.Cancelled, IsReady: false, VolumeLabel: null, AvailableFreeSpaceBytes: null, TotalSizeBytes: null);
    }
}
