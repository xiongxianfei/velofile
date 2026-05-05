using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.Core.Tests.Preview;

[TestClass]
[TestCategory("Thumbnails")]
public sealed class ThumbnailControllerTests
{
    [TestMethod]
    public async Task Thumbnails_controller_limits_concurrency_and_times_out_each_item_to_generic_icon()
    {
        var provider = new BlockingThumbnailProvider();
        var policy = new PreviewTimeoutPolicy(
            ImageDecodeBudget: TimeSpan.FromSeconds(2),
            TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
            PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
            MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
            ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(40),
            ThumbnailConcurrencyLimit: 4);
        var controller = new ThumbnailController(provider, policy);

        controller.Start(Enumerable.Range(0, 8).Select(index => Item($"file-{index}.txt")).ToArray());

        await WaitUntilAsync(() => provider.MaxConcurrentCount == 4);
        await WaitUntilAsync(() => controller.Snapshot.Count == 8
            && controller.Snapshot.Values.All(state => state.Status is ThumbnailStatus.GenericIcon));

        Assert.AreEqual(4, provider.MaxConcurrentCount);
        Assert.IsTrue(controller.Snapshot.Values.All(state => state.ReasonCode == "thumbnail-timeout"));
    }

    [TestMethod]
    public async Task Thumbnails_controller_ignores_stale_results_after_new_generation()
    {
        var provider = new ManualThumbnailProvider();
        var controller = new ThumbnailController(
            provider,
            PreviewTimeoutPolicy.ForTesting(TimeSpan.FromSeconds(5)));
        var oldItem = Item("old.txt");
        var newItem = Item("new.txt");

        controller.Start([oldItem]);
        await WaitUntilAsync(() => provider.Started("old.txt"));

        controller.Start([newItem]);
        await WaitUntilAsync(() => provider.Cancelled("old.txt") && provider.Started("new.txt"));

        provider.Complete("old.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("OLD")));
        provider.Complete("new.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("NEW")));
        await WaitUntilAsync(() => controller.GetState(newItem).Artifact?.DisplayText == "NEW");

        Assert.AreEqual(ThumbnailStatus.Ready, controller.GetState(newItem).Status);
        Assert.AreEqual(ThumbnailStatus.NotLoaded, controller.GetState(oldItem).Status);
        Assert.IsFalse(controller.Snapshot.ContainsKey(oldItem.FullPath));
    }

    private static ListedFileItem Item(string name)
    {
        return new ListedFileItem(
            @"C:\thumbs\" + name,
            name,
            name,
            FileSystemEntryKind.File,
            128,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class BlockingThumbnailProvider : IThumbnailProvider
    {
        private int _current;

        public int MaxConcurrentCount { get; private set; }

        public async ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _current);
            MaxConcurrentCount = Math.Max(MaxConcurrentCount, current);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon(item.Name));
            }
            finally
            {
                Interlocked.Decrement(ref _current);
            }
        }
    }

    private sealed class ManualThumbnailProvider : IThumbnailProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<ThumbnailProviderResult>> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<ThumbnailProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[item.Name] = completion;
            _started.Add(item.Name);
            cancellationToken.Register(() =>
            {
                _cancelled.Add(item.Name);
                completion.TrySetCanceled(cancellationToken);
            });
            return new ValueTask<ThumbnailProviderResult>(completion.Task);
        }

        public bool Started(string name)
        {
            return _started.Contains(name);
        }

        public bool Cancelled(string name)
        {
            return _cancelled.Contains(name);
        }

        public void Complete(string name, ThumbnailProviderResult result)
        {
            if (_pending.TryGetValue(name, out var completion))
            {
                completion.TrySetResult(result);
            }
        }
    }
}
