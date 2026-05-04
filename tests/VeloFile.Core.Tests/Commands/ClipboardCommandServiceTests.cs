using VeloFile.Core.Commands;
using VeloFile.Core.Listing;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Commands;

[TestClass]
[TestCategory("Commands")]
public sealed class ClipboardCommandServiceTests
{
    [TestMethod]
    public void Copy_path_writes_absolute_windows_paths_for_single_and_multiple_selection()
    {
        var clipboard = new CollectingClipboardWriter();
        var service = new ClipboardCommandService(clipboard);
        var items = new[]
        {
            Item(@"D:\folder\report one.txt", "report one.txt"),
            Item(@"D:\folder\weird & safe.ps1", "weird & safe.ps1")
        };

        var result = service.CopyPath(items);

        Assert.AreEqual(ClipboardCommandStatus.Written, result.Status);
        StringAssert.Contains(clipboard.Text!, @"D:\folder\report one.txt");
        StringAssert.Contains(clipboard.Text, @"D:\folder\weird & safe.ps1");
        Assert.IsFalse(clipboard.Text.Contains("cmd /c", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Copy_name_writes_only_names_for_single_and_multiple_selection()
    {
        var clipboard = new CollectingClipboardWriter();
        var service = new ClipboardCommandService(clipboard);

        var result = service.CopyName([
            Item(@"D:\folder\report one.txt", "report one.txt"),
            Item(@"D:\folder\client-a", "client-a")
        ]);

        Assert.AreEqual(ClipboardCommandStatus.Written, result.Status);
        StringAssert.Contains(clipboard.Text!, "report one.txt");
        StringAssert.Contains(clipboard.Text, "client-a");
        Assert.IsFalse(clipboard.Text.Contains(@"D:\folder", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Empty_selection_does_not_write_clipboard()
    {
        var clipboard = new CollectingClipboardWriter();
        var service = new ClipboardCommandService(clipboard);

        var result = service.CopyPath([]);

        Assert.AreEqual(ClipboardCommandStatus.NoSelection, result.Status);
        Assert.IsNull(clipboard.Text);
    }

    private static ListedFileItem Item(string fullPath, string name)
    {
        return new ListedFileItem(
            FullPath: fullPath,
            Name: name,
            DisplayName: name,
            Kind: FileSystemEntryKind.File,
            Length: 1,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            Attributes: FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private sealed class CollectingClipboardWriter : IClipboardTextWriter
    {
        public string? Text { get; private set; }

        public void SetText(string text)
        {
            Text = text;
        }
    }
}
