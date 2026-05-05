namespace VeloFile.Core.FileAssociations;

public enum FileAssociationLaunchKind
{
    Open,
    OpenWith
}

public sealed record FileAssociationLaunchRequest(
    FileAssociationLaunchKind Kind,
    string Path,
    bool ModifySystemAssociations);

public enum FileAssociationLaunchStatus
{
    Succeeded,
    Failed
}

public sealed record FileAssociationLaunchResult(
    FileAssociationLaunchKind Kind,
    FileAssociationLaunchStatus Status,
    string? ReasonCode)
{
    public static FileAssociationLaunchResult Succeeded(FileAssociationLaunchKind kind)
    {
        return new FileAssociationLaunchResult(kind, FileAssociationLaunchStatus.Succeeded, ReasonCode: null);
    }

    public static FileAssociationLaunchResult Failed(
        FileAssociationLaunchKind kind,
        string reasonCode = "association-launch-failed")
    {
        return new FileAssociationLaunchResult(kind, FileAssociationLaunchStatus.Failed, reasonCode);
    }
}

public interface IFileAssociationLaunchAdapter
{
    Task<FileAssociationLaunchResult> LaunchAsync(
        FileAssociationLaunchRequest request,
        CancellationToken cancellationToken = default);
}
