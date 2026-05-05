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
    bool IsTruncated,
    int? WidthPixels = null,
    int? HeightPixels = null,
    int? PageNumber = null)
{
    public static PreviewContent Text(string text, bool truncated)
    {
        return new PreviewContent(PreviewContentKind.Text, text, truncated);
    }

    public static PreviewContent Image(int widthPixels, int heightPixels)
    {
        return new PreviewContent(
            PreviewContentKind.Image,
            $"Image {widthPixels} x {heightPixels}",
            IsTruncated: false,
            WidthPixels: widthPixels,
            HeightPixels: heightPixels);
    }

    public static PreviewContent PdfFirstPage(string text)
    {
        return new PreviewContent(
            PreviewContentKind.Pdf,
            text,
            IsTruncated: false,
            PageNumber: 1);
    }
}

public enum PreviewOperation
{
    ImageDecode,
    TextReadAndEncodingDetection,
    PdfFirstPageRender,
    MetadataFallback,
    ThumbnailGeneration
}

public sealed record PreviewMetadata(
    string Name,
    long? SizeBytes,
    DateTimeOffset? CreationTimeUtc,
    DateTimeOffset? LastWriteTimeUtc,
    DateTimeOffset? LastAccessTimeUtc,
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
            new PreviewMetadataField("Created", CreationTimeUtc?.ToString("u") ?? "Unknown"),
            new PreviewMetadataField("Modified", LastWriteTimeUtc?.ToString("u") ?? "Unknown"),
            new PreviewMetadataField("Accessed", LastAccessTimeUtc?.ToString("u") ?? "Unknown"),
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

public sealed record PreviewProviderContext(
    PreviewOperation Operation,
    TimeSpan TimeoutBudget);

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
    PreviewOperation Operation { get; }

    bool CanPreview(PreviewRequest request);

    ValueTask<PreviewProviderResult> PreviewAsync(
        PreviewRequest request,
        PreviewProviderContext context,
        CancellationToken cancellationToken);
}

public sealed record PreviewControllerOptions(
    TimeSpan LoadingDelay,
    PreviewTimeoutPolicy TimeoutPolicy)
{
    public static PreviewControllerOptions Default { get; } = new(
        TimeSpan.FromMilliseconds(200),
        PreviewTimeoutPolicy.Default);
}

public sealed record PreviewTimeoutPolicy(
    TimeSpan ImageDecodeBudget,
    TimeSpan TextReadAndEncodingDetectionBudget,
    TimeSpan PdfFirstPageRenderBudget,
    TimeSpan MetadataFallbackBudget,
    TimeSpan ThumbnailGenerationBudget,
    int ThumbnailConcurrencyLimit)
{
    public static PreviewTimeoutPolicy Default { get; } = new(
        ImageDecodeBudget: TimeSpan.FromSeconds(2),
        TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
        PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
        MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
        ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(500),
        ThumbnailConcurrencyLimit: 4);

    public TimeSpan GetBudget(PreviewOperation operation)
    {
        return operation switch
        {
            PreviewOperation.ImageDecode => ImageDecodeBudget,
            PreviewOperation.TextReadAndEncodingDetection => TextReadAndEncodingDetectionBudget,
            PreviewOperation.PdfFirstPageRender => PdfFirstPageRenderBudget,
            PreviewOperation.MetadataFallback => MetadataFallbackBudget,
            PreviewOperation.ThumbnailGeneration => ThumbnailGenerationBudget,
            _ => MetadataFallbackBudget
        };
    }

    public static PreviewTimeoutPolicy ForTesting(TimeSpan budget)
    {
        return new PreviewTimeoutPolicy(
            ImageDecodeBudget: budget,
            TextReadAndEncodingDetectionBudget: budget,
            PdfFirstPageRenderBudget: budget,
            MetadataFallbackBudget: budget,
            ThumbnailGenerationBudget: budget,
            ThumbnailConcurrencyLimit: 4);
    }
}
