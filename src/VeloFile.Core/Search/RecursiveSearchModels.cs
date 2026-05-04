using VeloFile.Core.Listing;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Search;

public enum RecursiveSearchUpdateKind
{
    Result,
    SkippedLocation,
    ResultLimitReached,
    Completed,
    Cancelled
}

public enum RecursiveSearchStatus
{
    NotStarted,
    Running,
    Completed,
    Cancelled,
    ResultLimitReached,
    Failed
}

public sealed record RecursiveSearchOptions(
    int ResultLimit,
    VisibilitySettings VisibilitySettings)
{
    public const int DefaultResultLimit = 10_000;

    public static RecursiveSearchOptions Default { get; } = new(DefaultResultLimit);

    public RecursiveSearchOptions(int ResultLimit)
        : this(ResultLimit, VisibilitySettings.Default)
    {
    }

    public void Validate()
    {
        if (ResultLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ResultLimit), "Result limit must be positive.");
        }
    }
}

public sealed record RecursiveSearchSkippedLocation(
    string Path,
    string ReasonCode);

public sealed record RecursiveSearchUpdate(
    RecursiveSearchUpdateKind Kind,
    ListedFileItem? Result,
    RecursiveSearchSkippedLocation? SkippedLocation,
    int ResultCount)
{
    public static RecursiveSearchUpdate ResultFound(ListedFileItem result, int resultCount)
    {
        return new RecursiveSearchUpdate(RecursiveSearchUpdateKind.Result, result, SkippedLocation: null, resultCount);
    }

    public static RecursiveSearchUpdate Skipped(string path, string reasonCode, int resultCount)
    {
        return new RecursiveSearchUpdate(
            RecursiveSearchUpdateKind.SkippedLocation,
            Result: null,
            new RecursiveSearchSkippedLocation(path, reasonCode),
            resultCount);
    }

    public static RecursiveSearchUpdate LimitReached(int resultCount)
    {
        return new RecursiveSearchUpdate(RecursiveSearchUpdateKind.ResultLimitReached, Result: null, SkippedLocation: null, resultCount);
    }

    public static RecursiveSearchUpdate Completed(int resultCount)
    {
        return new RecursiveSearchUpdate(RecursiveSearchUpdateKind.Completed, Result: null, SkippedLocation: null, resultCount);
    }

    public static RecursiveSearchUpdate Cancelled(int resultCount)
    {
        return new RecursiveSearchUpdate(RecursiveSearchUpdateKind.Cancelled, Result: null, SkippedLocation: null, resultCount);
    }
}

public sealed record RecursiveSearchState(
    RecursiveSearchStatus Status,
    string? RootPath,
    string Query,
    IReadOnlyList<ListedFileItem> Results,
    IReadOnlyList<RecursiveSearchSkippedLocation> SkippedLocations,
    bool ResultLimitReached,
    bool CanCancel,
    int ResultLimit)
{
    public static RecursiveSearchState NotStarted { get; } = new(
        RecursiveSearchStatus.NotStarted,
        RootPath: null,
        Query: "",
        Results: [],
        SkippedLocations: [],
        ResultLimitReached: false,
        CanCancel: false,
        ResultLimit: RecursiveSearchOptions.DefaultResultLimit);

    public static RecursiveSearchState Running(string rootPath, string query, int resultLimit)
    {
        return new RecursiveSearchState(
            RecursiveSearchStatus.Running,
            rootPath,
            query,
            Results: [],
            SkippedLocations: [],
            ResultLimitReached: false,
            CanCancel: true,
            resultLimit);
    }

    public RecursiveSearchState Apply(RecursiveSearchUpdate update)
    {
        return update.Kind switch
        {
            RecursiveSearchUpdateKind.Result when update.Result is not null => this with
            {
                Results = Results.Concat([update.Result]).ToArray()
            },
            RecursiveSearchUpdateKind.SkippedLocation when update.SkippedLocation is not null => this with
            {
                SkippedLocations = SkippedLocations.Concat([update.SkippedLocation]).ToArray()
            },
            RecursiveSearchUpdateKind.ResultLimitReached => this with
            {
                Status = RecursiveSearchStatus.ResultLimitReached,
                ResultLimitReached = true,
                CanCancel = true
            },
            RecursiveSearchUpdateKind.Completed => this with
            {
                Status = RecursiveSearchStatus.Completed,
                CanCancel = false
            },
            RecursiveSearchUpdateKind.Cancelled => Cancelled(),
            _ => this
        };
    }

    public RecursiveSearchState Cancelled()
    {
        return this with
        {
            Status = RecursiveSearchStatus.Cancelled,
            CanCancel = false
        };
    }
}
