using VeloFile.Core.Listing;

namespace VeloFile.Core.Preview;

public enum PreviewStatus
{
    Empty,
    Loading,
    Success,
    Unsupported,
    Failed
}

public enum PreviewContentKind
{
    None,
    Text,
    Image,
    Pdf
}

public sealed record PreviewContent(
    PreviewContentKind Kind,
    string? TextContent,
    bool IsTruncated)
{
    public static PreviewContent Text(string text, bool truncated)
    {
        return new PreviewContent(PreviewContentKind.Text, text, truncated);
    }
}

public sealed record PreviewMetadata(
    string Name,
    long? SizeBytes,
    DateTimeOffset? LastWriteTimeUtc,
    FileAttributes Attributes,
    string TypeDescription,
    string ExtensionClass)
{
    public IReadOnlyList<PreviewMetadataField> Fields()
    {
        return
        [
            new PreviewMetadataField("Name", Name),
            new PreviewMetadataField("Size", SizeBytes is null ? "Unknown" : $"{SizeBytes} bytes"),
            new PreviewMetadataField("Modified", LastWriteTimeUtc?.ToString("u") ?? "Unknown"),
            new PreviewMetadataField("Attributes", Attributes.ToString()),
            new PreviewMetadataField("Type", TypeDescription)
        ];
    }
}

public sealed record PreviewMetadataField(string Label, string Value);

public sealed record PreviewState(
    PreviewStatus Status,
    PreviewMetadata? Metadata,
    PreviewContent? Content,
    string? ReasonCode)
{
    public static PreviewState Empty { get; } = new(PreviewStatus.Empty, Metadata: null, Content: null, ReasonCode: null);

    public static PreviewState Loading(PreviewMetadata metadata)
    {
        return new PreviewState(PreviewStatus.Loading, metadata, Content: null, ReasonCode: null);
    }

    public static PreviewState Success(PreviewMetadata metadata, PreviewContent content)
    {
        return new PreviewState(PreviewStatus.Success, metadata, content, ReasonCode: null);
    }

    public static PreviewState Unsupported(PreviewMetadata metadata, string reasonCode)
    {
        return new PreviewState(PreviewStatus.Unsupported, metadata, Content: null, reasonCode);
    }

    public static PreviewState Failed(PreviewMetadata metadata, string reasonCode)
    {
        return new PreviewState(PreviewStatus.Failed, metadata, Content: null, reasonCode);
    }
}

public sealed record PreviewRequest(ListedFileItem Item, PreviewMetadata Metadata);

public enum PreviewProviderResultStatus
{
    Success,
    Unsupported,
    Failed
}

public sealed record PreviewProviderResult(
    PreviewProviderResultStatus Status,
    PreviewContent? Content,
    string? ReasonCode)
{
    public static PreviewProviderResult Success(PreviewContent content)
    {
        return new PreviewProviderResult(PreviewProviderResultStatus.Success, content, ReasonCode: null);
    }

    public static PreviewProviderResult Unsupported(string reasonCode)
    {
        return new PreviewProviderResult(PreviewProviderResultStatus.Unsupported, Content: null, reasonCode);
    }

    public static PreviewProviderResult Failed(string reasonCode)
    {
        return new PreviewProviderResult(PreviewProviderResultStatus.Failed, Content: null, reasonCode);
    }
}

public interface IPreviewProvider
{
    bool CanPreview(PreviewRequest request);

    ValueTask<PreviewProviderResult> PreviewAsync(PreviewRequest request, CancellationToken cancellationToken);
}

public sealed record PreviewControllerOptions(
    TimeSpan LoadingDelay,
    TimeSpan TimeoutBudget)
{
    public static PreviewControllerOptions Default { get; } = new(
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromSeconds(3));
}
