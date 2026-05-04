using System.Runtime.CompilerServices;
using VeloFile.Core.Listing;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Search;

public interface IRecursiveSearchService
{
    IAsyncEnumerable<RecursiveSearchUpdate> SearchAsync(
        string rootPath,
        string query,
        RecursiveSearchOptions options,
        CancellationToken cancellationToken = default);
}

public sealed class RecursiveSearchService : IRecursiveSearchService
{
    private readonly IFolderEntrySource _entrySource;

    public RecursiveSearchService(IFolderEntrySource entrySource)
    {
        _entrySource = entrySource;
    }

    public async IAsyncEnumerable<RecursiveSearchUpdate> SearchAsync(
        string rootPath,
        string query,
        RecursiveSearchOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options.Validate();

        if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(query))
        {
            yield return RecursiveSearchUpdate.Completed(resultCount: 0);
            yield break;
        }

        var literalQuery = query.Trim();
        var resultCount = 0;
        var pending = new Queue<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        pending.Enqueue(rootPath);

        while (pending.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield return RecursiveSearchUpdate.Cancelled(resultCount);
                yield break;
            }

            var currentPath = pending.Dequeue();
            if (!visited.Add(currentPath))
            {
                continue;
            }

            await using var enumerator = _entrySource
                .EnumerateAsync(currentPath, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);

            while (true)
            {
                FileSystemEntrySnapshot? entry = null;
                RecursiveSearchUpdate? terminalUpdate = null;
                var hasEntry = false;

                try
                {
                    hasEntry = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    if (hasEntry)
                    {
                        entry = enumerator.Current;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    terminalUpdate = RecursiveSearchUpdate.Cancelled(resultCount);
                }
                catch (Exception ex)
                {
                    terminalUpdate = RecursiveSearchUpdate.Skipped(
                        currentPath,
                        ExpectedFileSystemExceptions.ReasonCode(ex),
                        resultCount);
                }

                if (terminalUpdate is not null)
                {
                    yield return terminalUpdate;
                    if (terminalUpdate.Kind is RecursiveSearchUpdateKind.Cancelled)
                    {
                        yield break;
                    }

                    break;
                }

                if (!hasEntry)
                {
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    yield return RecursiveSearchUpdate.Cancelled(resultCount);
                    yield break;
                }

                if (entry is null)
                {
                    continue;
                }

                if (entry.Kind is FileSystemEntryKind.Directory)
                {
                    if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        yield return RecursiveSearchUpdate.Skipped(entry.FullPath, "reparse-point", resultCount);
                    }
                    else if (!visited.Contains(entry.FullPath))
                    {
                        pending.Enqueue(entry.FullPath);
                    }
                }

                var projected = FileVisibilityProjector.Project(entry, options.VisibilitySettings);
                if (projected is null || !Matches(projected, literalQuery))
                {
                    continue;
                }

                resultCount++;
                yield return RecursiveSearchUpdate.ResultFound(projected, resultCount);
                if (resultCount == options.ResultLimit)
                {
                    yield return RecursiveSearchUpdate.LimitReached(resultCount);
                    yield break;
                }
            }
        }

        yield return RecursiveSearchUpdate.Completed(resultCount);
    }

    private static bool Matches(ListedFileItem item, string literalQuery)
    {
        return item.Name.Contains(literalQuery, StringComparison.OrdinalIgnoreCase);
    }
}
