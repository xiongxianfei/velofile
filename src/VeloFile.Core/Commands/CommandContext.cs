using VeloFile.Core.Listing;

namespace VeloFile.Core.Commands;

public sealed record CommandContext(
    string CurrentFolderPath,
    IReadOnlyList<ListedFileItem> SelectedItems,
    bool CanPaste)
{
    public int SelectionCount => SelectedItems.Count;

    public static CommandContext ForSelection(
        string currentFolderPath,
        IReadOnlyList<ListedFileItem> selectedItems,
        bool canPaste)
    {
        return new CommandContext(currentFolderPath, selectedItems, canPaste);
    }
}
