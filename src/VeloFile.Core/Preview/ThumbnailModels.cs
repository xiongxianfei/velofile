using VeloFile.Core.Listing;

namespace VeloFile.Core.Preview;

public enum ThumbnailStatus
{
    NotLoaded,
    Loading,
    Ready,
    GenericIcon,
    Failed
}

public enum ThumbnailProviderResultStatus
{
    Success,
    GenericIcon,
    Failed
}

public sealed record ThumbnailArtifact(
    string DisplayText,
    string? EncodedFormat = null,
    byte[]? EncodedBytes = null,
    bool IsGenericIcon = false)
{
    public static ThumbnailArtifact GenericIcon(string displayText)
    {
        return new ThumbnailArtifact(displayText, IsGenericIcon: true);
    }
}

public sealed record ThumbnailProviderContext(TimeSpan TimeoutBudget);

public sealed record ThumbnailProviderResult(
    ThumbnailProviderResultStatus Status,
    ThumbnailArtifact? Artifact,
    string? ReasonCode)
{
    public static ThumbnailProviderResult Success(ThumbnailArtifact artifact)
    {
        return new ThumbnailProviderResult(ThumbnailProviderResultStatus.Success, artifact, ReasonCode: null);
    }

    public static ThumbnailProviderResult GenericIcon(ThumbnailArtifact artifact, string reasonCode)
    {
        return new ThumbnailProviderResult(ThumbnailProviderResultStatus.GenericIcon, artifact, reasonCode);
    }

    public static ThumbnailProviderResult Failed(string reasonCode)
    {
        return new ThumbnailProviderResult(ThumbnailProviderResultStatus.Failed, Artifact: null, reasonCode);
    }
}

public sealed record ThumbnailState(
    ThumbnailStatus Status,
    ThumbnailArtifact? Artifact,
    string? ReasonCode)
{
    public static ThumbnailState NotLoaded { get; } = new(ThumbnailStatus.NotLoaded, Artifact: null, ReasonCode: null);

    public static ThumbnailState Loading { get; } = new(ThumbnailStatus.Loading, Artifact: null, ReasonCode: null);

    public static ThumbnailState Ready(ThumbnailArtifact artifact)
    {
        return new ThumbnailState(ThumbnailStatus.Ready, artifact, ReasonCode: null);
    }

    public static ThumbnailState GenericIcon(ThumbnailArtifact artifact, string reasonCode)
    {
        return new ThumbnailState(ThumbnailStatus.GenericIcon, artifact, reasonCode);
    }

    public static ThumbnailState Failed(string reasonCode)
    {
        return new ThumbnailState(ThumbnailStatus.Failed, Artifact: null, reasonCode);
    }
}

public interface IThumbnailProvider
{
    ValueTask<ThumbnailProviderResult> GenerateAsync(
        ListedFileItem item,
        ThumbnailProviderContext context,
        CancellationToken cancellationToken);
}
