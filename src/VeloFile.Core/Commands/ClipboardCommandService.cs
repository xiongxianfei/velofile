using VeloFile.Core.Listing;

namespace VeloFile.Core.Commands;

public interface IClipboardTextWriter
{
    void SetText(string text);
}

public enum ClipboardCommandStatus
{
    Written,
    NoSelection
}

public sealed record ClipboardCommandResult(ClipboardCommandStatus Status);

public sealed class ClipboardCommandService
{
    private readonly IClipboardTextWriter _clipboard;

    public ClipboardCommandService(IClipboardTextWriter clipboard)
    {
        _clipboard = clipboard;
    }

    public ClipboardCommandResult CopyPath(IReadOnlyList<ListedFileItem> items)
    {
        return WriteSelectedValues(items, item => item.FullPath);
    }

    public ClipboardCommandResult CopyName(IReadOnlyList<ListedFileItem> items)
    {
        return WriteSelectedValues(items, item => item.Name);
    }

    private ClipboardCommandResult WriteSelectedValues(
        IReadOnlyList<ListedFileItem> items,
        Func<ListedFileItem, string> selector)
    {
        if (items.Count == 0)
        {
            return new ClipboardCommandResult(ClipboardCommandStatus.NoSelection);
        }

        _clipboard.SetText(string.Join(Environment.NewLine, items.Select(selector)));
        return new ClipboardCommandResult(ClipboardCommandStatus.Written);
    }
}
