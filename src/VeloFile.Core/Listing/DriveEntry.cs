namespace VeloFile.Core.Listing;

public sealed record DriveEntry(
    string Name,
    string RootPath,
    DriveType DriveType,
    bool IsReady,
    long? AvailableFreeSpaceBytes,
    long? TotalSizeBytes);

public interface IDriveEntrySource
{
    IReadOnlyList<DriveEntry> GetDrives();
}
