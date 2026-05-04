using System.Runtime.CompilerServices;
using VeloFile.Core.Listing;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Tests.Listing;

[TestClass]
[TestCategory("Listing")]
public sealed class FolderListingCoordinatorTests
{
    private static readonly FolderListingOptions Options = new(25, VisibilitySettings.Default);

    [TestMethod]
    public async Task Slow_tab_listing_does_not_block_healthy_tab_listing()
    {
        var slowSource = new GatedFolderEntrySource();
        var source = new RoutedFolderEntrySource(
            ("slow://tab-a", slowSource),
            ("local://tab-b", new StaticFolderEntrySource("healthy.txt")));
        var coordinator = new FolderListingCoordinator(new FolderListingService(source));

        var slowOperation = coordinator.StartLoad("tab-a", "slow://tab-a", Options);
        await slowSource.Started.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.AreEqual(FolderListingStatus.Pending, coordinator.GetState("tab-a")?.Status);

        var healthyOperation = coordinator.StartLoad("tab-b", "local://tab-b", Options);
        var healthyResult = await healthyOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(healthyResult.Applied);
        Assert.AreEqual(FolderListingStatus.Ready, healthyResult.State.Status);
        Assert.AreEqual("healthy.txt", coordinator.GetState("tab-b")?.FirstViewport.Single().Name);
        Assert.AreEqual(FolderListingStatus.Pending, coordinator.GetState("tab-a")?.Status);
        Assert.IsFalse(slowOperation.Completion.IsCompleted, "The slow tab should still be pending while the healthy tab completes.");

        slowSource.Release("slow.txt");
        var slowResult = await slowOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(slowResult.Applied);
        Assert.AreEqual("slow.txt", coordinator.GetState("tab-a")?.FirstViewport.Single().Name);
        Assert.AreEqual("healthy.txt", coordinator.GetState("tab-b")?.FirstViewport.Single().Name);
    }

    [TestMethod]
    public async Task Stale_slow_listing_result_cannot_overwrite_newer_tab_state()
    {
        var oldSource = new GatedFolderEntrySource();
        var source = new RoutedFolderEntrySource(
            ("slow://old", oldSource),
            ("local://new", new StaticFolderEntrySource("new.txt")));
        var coordinator = new FolderListingCoordinator(new FolderListingService(source));

        var oldOperation = coordinator.StartLoad("tab-a", "slow://old", Options);
        await oldSource.Started.WaitAsync(TimeSpan.FromSeconds(2));

        var newOperation = coordinator.StartLoad("tab-a", "local://new", Options);
        var newResult = await newOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(newResult.Applied);
        Assert.AreEqual("local://new", coordinator.GetState("tab-a")?.Path);
        Assert.AreEqual("new.txt", coordinator.GetState("tab-a")?.FirstViewport.Single().Name);

        oldSource.Release("old.txt");
        var oldResult = await oldOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(oldResult.Applied);
        Assert.AreEqual("local://new", coordinator.GetState("tab-a")?.Path);
        Assert.AreEqual("new.txt", coordinator.GetState("tab-a")?.FirstViewport.Single().Name);
    }

    [TestMethod]
    public async Task Cancellation_ignoring_old_listing_result_cannot_overwrite_newer_tab_state()
    {
        var oldSource = new CancellationIgnoringFolderEntrySource();
        var source = new RoutedFolderEntrySource(
            ("slow://old", oldSource),
            ("local://new", new StaticFolderEntrySource("new-file.txt")));
        var coordinator = new FolderListingCoordinator(new FolderListingService(source));

        var oldOperation = coordinator.StartLoad("tab-a", "slow://old", Options);
        await oldSource.Started.WaitAsync(TimeSpan.FromSeconds(2));

        var newOperation = coordinator.StartLoad("tab-a", "local://new", Options);
        var newResult = await newOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(newResult.Applied);
        Assert.AreEqual("local://new", coordinator.GetState("tab-a")?.Path);
        Assert.AreEqual("new-file.txt", coordinator.GetState("tab-a")?.FirstViewport.Single().Name);
        await oldSource.CancellationRequested.WaitAsync(TimeSpan.FromSeconds(2));

        oldSource.Release("old-file.txt");
        var oldResult = await oldOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(oldResult.Applied);
        Assert.AreEqual("local://new", coordinator.GetState("tab-a")?.Path);
        Assert.AreEqual("new-file.txt", coordinator.GetState("tab-a")?.FirstViewport.Single().Name);
        Assert.IsFalse(coordinator.GetState("tab-a")!.FirstViewport.Any(entry => entry.Name == "old-file.txt"));
    }

    [TestMethod]
    public async Task Cancellation_ignoring_old_listing_failure_cannot_overwrite_newer_tab_state()
    {
        var oldSource = new CancellationIgnoringThrowingFolderEntrySource(new UnauthorizedAccessException("late-denied"));
        var source = new RoutedFolderEntrySource(
            ("slow://old", oldSource),
            ("local://new", new StaticFolderEntrySource("new-file.txt")));
        var coordinator = new FolderListingCoordinator(new FolderListingService(source));

        var oldOperation = coordinator.StartLoad("tab-a", "slow://old", Options);
        await oldSource.Started.WaitAsync(TimeSpan.FromSeconds(2));

        var newOperation = coordinator.StartLoad("tab-a", "local://new", Options);
        var newResult = await newOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsTrue(newResult.Applied);
        await oldSource.CancellationRequested.WaitAsync(TimeSpan.FromSeconds(2));

        oldSource.Release();
        var oldResult = await oldOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(oldResult.Applied);
        Assert.AreEqual(FolderListingStatus.Ready, coordinator.GetState("tab-a")?.Status);
        Assert.AreEqual("local://new", coordinator.GetState("tab-a")?.Path);
        Assert.AreEqual("new-file.txt", coordinator.GetState("tab-a")?.FirstViewport.Single().Name);
    }

    [TestMethod]
    public async Task Closing_tab_cancels_slow_listing_without_updating_state()
    {
        var slowSource = new GatedFolderEntrySource();
        var source = new RoutedFolderEntrySource(
            ("slow://tab-a", slowSource),
            ("local://tab-b", new StaticFolderEntrySource("healthy.txt")));
        var coordinator = new FolderListingCoordinator(new FolderListingService(source));

        var slowOperation = coordinator.StartLoad("tab-a", "slow://tab-a", Options);
        await slowSource.Started.WaitAsync(TimeSpan.FromSeconds(2));

        var healthyOperation = coordinator.StartLoad("tab-b", "local://tab-b", Options);
        await healthyOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        coordinator.CloseTab("tab-a");

        await slowSource.CancellationObserved.WaitAsync(TimeSpan.FromSeconds(2));
        var slowResult = await slowOperation.Completion.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.IsFalse(slowResult.Applied);
        Assert.IsNull(coordinator.GetState("tab-a"));
        Assert.AreEqual("healthy.txt", coordinator.GetState("tab-b")?.FirstViewport.Single().Name);
    }

    private sealed class RoutedFolderEntrySource : IFolderEntrySource
    {
        private readonly Dictionary<string, IFolderEntrySource> _routes;

        public RoutedFolderEntrySource(params (string Path, IFolderEntrySource Source)[] routes)
        {
            _routes = routes.ToDictionary(route => route.Path, route => route.Source, StringComparer.Ordinal);
        }

        public IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(string path, CancellationToken cancellationToken)
        {
            return _routes[path].EnumerateAsync(path, cancellationToken);
        }
    }

    private sealed class StaticFolderEntrySource : IFolderEntrySource
    {
        private readonly IReadOnlyList<string> _names;

        public StaticFolderEntrySource(params string[] names)
        {
            _names = names;
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();

            foreach (var name in _names)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return Entry(path, name);
            }
        }
    }

    private sealed class GatedFolderEntrySource : IFolderEntrySource
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<IReadOnlyList<string>> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _cancellationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Started => _started.Task;

        public Task CancellationObserved => _cancellationObserved.Task;

        public void Release(params string[] names)
        {
            _release.TrySetResult(names);
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _started.TrySetResult();
            using var registration = cancellationToken.Register(() => _cancellationObserved.TrySetResult());

            IReadOnlyList<string> names;
            try
            {
                names = await _release.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _cancellationObserved.TrySetResult();
                throw;
            }

            foreach (var name in names)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return Entry(path, name);
            }
        }
    }

    private sealed class CancellationIgnoringFolderEntrySource : IFolderEntrySource
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _cancellationRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<IReadOnlyList<string>> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Started => _started.Task;

        public Task CancellationRequested => _cancellationRequested.Task;

        public void Release(params string[] names)
        {
            _release.TrySetResult(names);
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _started.TrySetResult();
            using var registration = cancellationToken.Register(() => _cancellationRequested.TrySetResult());

            var names = await _release.Task;
            foreach (var name in names)
            {
                yield return Entry(path, name);
            }
        }
    }

    private sealed class CancellationIgnoringThrowingFolderEntrySource : IFolderEntrySource
    {
        private readonly Exception _exception;
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _cancellationRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CancellationIgnoringThrowingFolderEntrySource(Exception exception)
        {
            _exception = exception;
        }

        public Task Started => _started.Task;

        public Task CancellationRequested => _cancellationRequested.Task;

        public void Release()
        {
            _release.TrySetResult();
        }

        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _started.TrySetResult();
            using var registration = cancellationToken.Register(() => _cancellationRequested.TrySetResult());

            await _release.Task;
            throw _exception;
            #pragma warning disable CS0162
            yield break;
            #pragma warning restore CS0162
        }
    }

    private static FileSystemEntrySnapshot Entry(string path, string name)
    {
        return new FileSystemEntrySnapshot(
            FullPath: $"{path}/{name}",
            Name: name,
            Kind: FileSystemEntryKind.File,
            Length: 1,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            Attributes: FileAttributes.Archive);
    }
}
