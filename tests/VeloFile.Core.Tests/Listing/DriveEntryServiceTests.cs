using VeloFile.Core.Listing;

namespace VeloFile.Core.Tests.Listing;

[TestClass]
[TestCategory("Listing")]
public sealed class DriveEntryServiceTests
{
    [TestMethod]
    public async Task Drive_entries_are_returned_before_slow_hints()
    {
        var hintSource = new ControlledDriveHintSource();
        hintSource.SetHint(@"C:\", DriveHint.Available(volumeLabel: "System", availableFreeSpaceBytes: 10, totalSizeBytes: 100));
        hintSource.SetPending(@"Z:\");
        var service = new DriveEntryService(
            new StaticDriveEntrySource(FastEntry(@"C:\"), FastEntry(@"Z:\")),
            hintSource);

        var refresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromMilliseconds(50), MaxConcurrentHintOperations: 2));

        Assert.HasCount(2, refresh.InitialEntries, "Both drive entries must be returned before hint enrichment completes.");
        Assert.IsTrue(refresh.InitialEntries.All(entry => entry.AvailableFreeSpaceBytes is null));
        Assert.IsTrue(refresh.InitialEntries.All(entry => entry.TotalSizeBytes is null));
        Assert.AreEqual(DriveHintStatus.Loading, refresh.InitialEntries.Single(entry => entry.RootPath == @"Z:\").HintStatus);

        var result = await refresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(result.Applied);
        Assert.AreEqual(DriveHintStatus.Available, result.Entries.Single(entry => entry.RootPath == @"C:\").HintStatus);
        Assert.AreEqual(10, result.Entries.Single(entry => entry.RootPath == @"C:\").AvailableFreeSpaceBytes);
        Assert.AreEqual(DriveHintStatus.TimedOut, result.Entries.Single(entry => entry.RootPath == @"Z:\").HintStatus);
        Assert.IsNull(result.Entries.Single(entry => entry.RootPath == @"Z:\").AvailableFreeSpaceBytes);
        Assert.IsFalse(hintSource.PendingHintCompleted(@"Z:\"));
    }

    [TestMethod]
    public async Task Timed_out_non_cancelling_hints_continue_counting_against_concurrency_cap()
    {
        var hintSource = new ControlledDriveHintSource();
        foreach (var rootPath in new[] { @"A:\", @"B:\", @"C:\", @"D:\", @"E:\" })
        {
            hintSource.SetPending(rootPath, observeCancellation: false);
        }

        var service = new DriveEntryService(
            new StaticDriveEntrySource(
                FastEntry(@"A:\"),
                FastEntry(@"B:\"),
                FastEntry(@"C:\"),
                FastEntry(@"D:\"),
                FastEntry(@"E:\")),
            hintSource);

        var refresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromMilliseconds(50), MaxConcurrentHintOperations: 2));
        var result = await refresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(result.Applied);
        Assert.HasCount(5, result.Entries);
        Assert.IsLessThanOrEqualTo(hintSource.MaxActiveCount, 2, $"Expected at most 2 live hint reads, saw {hintSource.MaxActiveCount}.");
        Assert.AreEqual(DriveHintStatus.TimedOut, result.Entries.Single(entry => entry.RootPath == @"A:\").HintStatus);
        Assert.AreEqual(DriveHintStatus.TimedOut, result.Entries.Single(entry => entry.RootPath == @"B:\").HintStatus);
        Assert.AreEqual(DriveHintStatus.Unavailable, result.Entries.Single(entry => entry.RootPath == @"C:\").HintStatus);
        Assert.AreEqual(DriveHintStatus.Unavailable, result.Entries.Single(entry => entry.RootPath == @"D:\").HintStatus);
        Assert.AreEqual(DriveHintStatus.Unavailable, result.Entries.Single(entry => entry.RootPath == @"E:\").HintStatus);
        CollectionAssert.AreEquivalent(new[] { @"A:\", @"B:\" }, hintSource.StartedRoots.Distinct().ToArray());

        var secondRefresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromMilliseconds(50), MaxConcurrentHintOperations: 2));
        var secondResult = await secondRefresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(secondResult.Applied);
        Assert.IsLessThanOrEqualTo(hintSource.MaxActiveCount, 2, $"Expected at most 2 live hint reads, saw {hintSource.MaxActiveCount}.");
        CollectionAssert.AreEquivalent(new[] { @"A:\", @"B:\" }, hintSource.StartedRoots.Distinct().ToArray());

        hintSource.Release(@"A:\", DriveHint.Available(volumeLabel: "Released", availableFreeSpaceBytes: 1, totalSizeBytes: 2));
        await WaitUntilAsync(
            async () =>
            {
                var thirdRefresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromMilliseconds(50), MaxConcurrentHintOperations: 2));
                var thirdResult = await thirdRefresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));
                return thirdResult.Applied && hintSource.StartedRoots.Contains(@"C:\");
            },
            TimeSpan.FromSeconds(2));

        CollectionAssert.AreEquivalent(new[] { @"A:\", @"B:\", @"C:\" }, hintSource.StartedRoots.Distinct().ToArray());
        Assert.IsLessThanOrEqualTo(hintSource.MaxActiveCount, 2, $"Expected at most 2 live hint reads, saw {hintSource.MaxActiveCount}.");
    }

    [TestMethod]
    public async Task Slow_hint_completion_updates_only_matching_generation()
    {
        var entrySource = new MutableDriveEntrySource(FastEntry(@"C:\"));
        var hintSource = new ControlledDriveHintSource();
        hintSource.SetPending(@"C:\");
        hintSource.SetHint(@"D:\", DriveHint.Available(volumeLabel: "Data", availableFreeSpaceBytes: 20, totalSizeBytes: 200));
        var service = new DriveEntryService(entrySource, hintSource);

        var oldRefresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromSeconds(5), MaxConcurrentHintOperations: 1));
        entrySource.SetEntries(FastEntry(@"D:\"));

        var newRefresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromSeconds(1), MaxConcurrentHintOperations: 1));
        var newResult = await newRefresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(newResult.Applied);
        Assert.AreEqual(@"D:\", service.CurrentEntries.Single().RootPath);
        Assert.AreEqual(DriveHintStatus.Available, service.CurrentEntries.Single().HintStatus);

        hintSource.Release(@"C:\", DriveHint.Available(volumeLabel: "Old", availableFreeSpaceBytes: 1, totalSizeBytes: 2));
        var oldResult = await oldRefresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(oldResult.Applied);
        Assert.AreEqual(@"D:\", service.CurrentEntries.Single().RootPath);
        Assert.AreEqual(20, service.CurrentEntries.Single().AvailableFreeSpaceBytes);
    }

    [TestMethod]
    [DataRow("access-denied", DriveHintStatus.AccessDenied)]
    [DataRow("io-error", DriveHintStatus.Unavailable)]
    public async Task Hint_failure_keeps_drive_entry_visible(string failureKind, DriveHintStatus expectedStatus)
    {
        var hintSource = new ControlledDriveHintSource();
        hintSource.SetFailure(
            @"E:\",
            failureKind == "access-denied"
                ? new UnauthorizedAccessException(@"E:\private")
                : new IOException(@"E:\offline"));
        var service = new DriveEntryService(new StaticDriveEntrySource(FastEntry(@"E:\")), hintSource);

        var refresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromSeconds(1), MaxConcurrentHintOperations: 1));
        var result = await refresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        var entry = result.Entries.Single();
        Assert.IsTrue(result.Applied);
        Assert.AreEqual(@"E:\", entry.RootPath);
        Assert.AreEqual(expectedStatus, entry.HintStatus);
        Assert.IsNull(entry.AvailableFreeSpaceBytes);
        Assert.IsNull(entry.TotalSizeBytes);
    }

    [TestMethod]
    public async Task Cancelling_hint_refresh_abandons_late_updates()
    {
        var hintSource = new ControlledDriveHintSource();
        hintSource.SetPending(@"F:\");
        var service = new DriveEntryService(new StaticDriveEntrySource(FastEntry(@"F:\")), hintSource);

        var refresh = service.StartRefresh(new DriveHintRefreshOptions(TimeSpan.FromSeconds(5), MaxConcurrentHintOperations: 1));

        service.CancelRefresh();
        var result = await refresh.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(result.Applied);
        Assert.AreEqual(DriveHintStatus.Cancelled, service.CurrentEntries.Single().HintStatus);

        hintSource.Release(@"F:\", DriveHint.Available(volumeLabel: "Late", availableFreeSpaceBytes: 1, totalSizeBytes: 2));

        Assert.AreEqual(DriveHintStatus.Cancelled, service.CurrentEntries.Single().HintStatus);
        Assert.IsNull(service.CurrentEntries.Single().AvailableFreeSpaceBytes);
    }

    private static DriveEntry FastEntry(string rootPath)
    {
        return new DriveEntry(
            Name: rootPath,
            RootPath: rootPath,
            DriveType: DriveType.Fixed,
            IsReady: false,
            AvailableFreeSpaceBytes: null,
            TotalSizeBytes: null);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(20);
        }

        Assert.Fail("Condition was not met before the timeout.");
    }

    private sealed class StaticDriveEntrySource : IDriveEntrySource
    {
        private readonly IReadOnlyList<DriveEntry> _entries;

        public StaticDriveEntrySource(params DriveEntry[] entries)
        {
            _entries = entries;
        }

        public IReadOnlyList<DriveEntry> GetDrives()
        {
            return _entries;
        }
    }

    private sealed class MutableDriveEntrySource : IDriveEntrySource
    {
        private IReadOnlyList<DriveEntry> _entries;

        public MutableDriveEntrySource(params DriveEntry[] entries)
        {
            _entries = entries;
        }

        public void SetEntries(params DriveEntry[] entries)
        {
            _entries = entries;
        }

        public IReadOnlyList<DriveEntry> GetDrives()
        {
            return _entries;
        }
    }

    private sealed class ControlledDriveHintSource : IDriveHintSource
    {
        private readonly Dictionary<string, TaskCompletionSource<DriveHint>> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<CancellationToken, Task<DriveHint>>> _routes = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new();
        private int _activeCount;
        private int _maxActiveCount;

        public void SetHint(string rootPath, DriveHint hint)
        {
            _routes[rootPath] = _ => Task.FromResult(hint);
        }

        public IReadOnlyList<string> StartedRoots
        {
            get
            {
                lock (_sync)
                {
                    return _startedRoots.ToArray();
                }
            }
        }

        public int MaxActiveCount
        {
            get
            {
                lock (_sync)
                {
                    return _maxActiveCount;
                }
            }
        }

        public void SetPending(string rootPath, bool observeCancellation = true)
        {
            var pending = new TaskCompletionSource<DriveHint>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[rootPath] = pending;
            _routes[rootPath] = async cancellationToken =>
            {
                OnStarted(rootPath);
                try
                {
                    return observeCancellation
                        ? await pending.Task.WaitAsync(cancellationToken)
                        : await pending.Task;
                }
                finally
                {
                    OnCompleted();
                }
            };
        }

        public void SetFailure(string rootPath, Exception exception)
        {
            _routes[rootPath] = _ => Task.FromException<DriveHint>(exception);
        }

        public void Release(string rootPath, DriveHint hint)
        {
            _pending[rootPath].TrySetResult(hint);
        }

        public bool PendingHintCompleted(string rootPath)
        {
            return _pending.TryGetValue(rootPath, out var pending) && pending.Task.IsCompleted;
        }

        public Task<DriveHint> GetHintAsync(string rootPath, CancellationToken cancellationToken)
        {
            return _routes[rootPath](cancellationToken);
        }

        private readonly List<string> _startedRoots = [];

        private void OnStarted(string rootPath)
        {
            lock (_sync)
            {
                _startedRoots.Add(rootPath);
                _activeCount++;
                _maxActiveCount = Math.Max(_maxActiveCount, _activeCount);
            }
        }

        private void OnCompleted()
        {
            lock (_sync)
            {
                _activeCount--;
            }
        }
    }
}
