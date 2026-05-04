using System.Runtime.CompilerServices;
using VeloFile.Core.Listing;
using VeloFile.Core.Search;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Search;

[TestClass]
[TestCategory("Search")]
public sealed class RecursiveSearchServiceTests
{
    [TestMethod]
    public async Task Search_streams_results_and_stops_adding_after_result_cap()
    {
        var source = new FakeFolderEntrySource();
        source.SetEntries(@"D:\root",
            File(@"D:\root\match-1.txt", "match-1.txt"),
            File(@"D:\root\match-2.txt", "match-2.txt"),
            File(@"D:\root\match-3.txt", "match-3.txt"),
            File(@"D:\root\match-4.txt", "match-4.txt"));
        var service = new RecursiveSearchService(source);

        var updates = await CollectAsync(service.SearchAsync(
            @"D:\root",
            "match",
            new RecursiveSearchOptions(ResultLimit: 3)));

        var results = updates.Where(update => update.Kind is RecursiveSearchUpdateKind.Result).ToArray();
        Assert.AreEqual(3, results.Length);
        CollectionAssert.AreEqual(
            new[] { "match-1.txt", "match-2.txt", "match-3.txt" },
            results.Select(update => update.Result!.Name).ToArray());
        Assert.IsTrue(updates.Any(update => update.Kind is RecursiveSearchUpdateKind.ResultLimitReached));
        Assert.IsFalse(updates.Any(update => update.Result?.Name == "match-4.txt"));
    }

    [TestMethod]
    public async Task Search_can_be_cancelled_before_cap()
    {
        var source = new GateFolderEntrySource();
        source.SetEntries(@"D:\root",
            File(@"D:\root\match-1.txt", "match-1.txt"),
            File(@"D:\root\match-2.txt", "match-2.txt"));
        var service = new RecursiveSearchService(source);
        using var cts = new CancellationTokenSource();
        var updates = new List<RecursiveSearchUpdate>();

        await foreach (var update in service.SearchAsync(@"D:\root", "match", RecursiveSearchOptions.Default, cts.Token))
        {
            updates.Add(update);
            if (update.Kind is RecursiveSearchUpdateKind.Result)
            {
                cts.Cancel();
                source.Release();
            }
        }

        Assert.AreEqual(1, updates.Count(update => update.Kind is RecursiveSearchUpdateKind.Result));
        Assert.IsTrue(updates.Any(update => update.Kind is RecursiveSearchUpdateKind.Cancelled));
    }

    [TestMethod]
    public async Task Search_reports_access_denied_branch_and_continues()
    {
        var source = new FakeFolderEntrySource();
        source.SetEntries(@"D:\root",
            Directory(@"D:\root\denied", "denied"),
            Directory(@"D:\root\ok", "ok"));
        source.SetException(@"D:\root\denied", new UnauthorizedAccessException());
        source.SetEntries(@"D:\root\ok", File(@"D:\root\ok\match.txt", "match.txt"));
        var service = new RecursiveSearchService(source);

        var updates = await CollectAsync(service.SearchAsync(@"D:\root", "match", RecursiveSearchOptions.Default));

        Assert.IsTrue(updates.Any(update => update.Kind is RecursiveSearchUpdateKind.SkippedLocation
            && update.SkippedLocation?.Path == @"D:\root\denied"
            && update.SkippedLocation.ReasonCode == "access-denied"));
        Assert.IsTrue(updates.Any(update => update.Result?.FullPath == @"D:\root\ok\match.txt"));
    }

    [TestMethod]
    public async Task Search_skips_reparse_point_directories_without_following_loops()
    {
        var source = new FakeFolderEntrySource();
        source.SetEntries(@"D:\root", Directory(@"D:\root\loop", "loop", FileAttributes.Directory | FileAttributes.ReparsePoint));
        source.SetException(@"D:\root\loop", new InvalidOperationException("Loop directory should not be enumerated."));
        var service = new RecursiveSearchService(source);

        var updates = await CollectAsync(service.SearchAsync(@"D:\root", "anything", RecursiveSearchOptions.Default));

        Assert.IsTrue(updates.Any(update => update.Kind is RecursiveSearchUpdateKind.SkippedLocation
            && update.SkippedLocation?.Path == @"D:\root\loop"
            && update.SkippedLocation.ReasonCode == "reparse-point"));
        Assert.IsFalse(source.EnumeratedPaths.Contains(@"D:\root\loop", StringComparer.OrdinalIgnoreCase));
    }

    private static async Task<IReadOnlyList<RecursiveSearchUpdate>> CollectAsync(
        IAsyncEnumerable<RecursiveSearchUpdate> updates)
    {
        var collected = new List<RecursiveSearchUpdate>();
        await foreach (var update in updates)
        {
            collected.Add(update);
        }

        return collected;
    }

    private static FileSystemEntrySnapshot File(string fullPath, string name)
    {
        return Entry(fullPath, name, FileSystemEntryKind.File, FileAttributes.Archive);
    }

    private static FileSystemEntrySnapshot Directory(
        string fullPath,
        string name,
        FileAttributes attributes = FileAttributes.Directory)
    {
        return Entry(fullPath, name, FileSystemEntryKind.Directory, attributes);
    }

    private static FileSystemEntrySnapshot Entry(
        string fullPath,
        string name,
        FileSystemEntryKind kind,
        FileAttributes attributes)
    {
        return new FileSystemEntrySnapshot(
            FullPath: fullPath,
            Name: name,
            Kind: kind,
            Length: kind is FileSystemEntryKind.File ? 10 : null,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            Attributes: attributes);
    }

    private class FakeFolderEntrySource : IFolderEntrySource
    {
        private readonly Dictionary<string, IReadOnlyList<FileSystemEntrySnapshot>> _entries = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Exception> _exceptions = new(StringComparer.OrdinalIgnoreCase);

        public List<string> EnumeratedPaths { get; } = [];

        public void SetEntries(string path, params FileSystemEntrySnapshot[] entries)
        {
            _entries[path] = entries;
        }

        public void SetException(string path, Exception exception)
        {
            _exceptions[path] = exception;
        }

        public virtual async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            EnumeratedPaths.Add(path);

            if (_exceptions.TryGetValue(path, out var exception))
            {
                throw exception;
            }

            if (!_entries.TryGetValue(path, out var entries))
            {
                yield break;
            }

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }

    private sealed class GateFolderEntrySource : FakeFolderEntrySource
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _yieldedFirst;

        public void Release()
        {
            _gate.TrySetResult();
        }

        public override async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var entry in base.EnumerateAsync(path, cancellationToken))
            {
                if (_yieldedFirst)
                {
                    await _gate.Task.ConfigureAwait(false);
                }

                _yieldedFirst = true;
                yield return entry;
            }
        }
    }
}
