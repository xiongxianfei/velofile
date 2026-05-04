using VeloFile.Core.Listing;

namespace VeloFile.App.Input;

public interface IFileListRowItem
{
    ListedFileItem FileItem { get; }
}

public static class FileListSelectionMapper
{
    public static IReadOnlyList<ListedFileItem> ToListedFileItems(IEnumerable<object?> selectedItems)
    {
        return selectedItems
            .Select(TryGetListedFileItem)
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();
    }

    public static ListedFileItem? TryGetListedFileItem(object? selectedItem)
    {
        if (selectedItem is null)
        {
            return null;
        }

        return selectedItem switch
        {
            ListedFileItem item => item,
            IFileListRowItem row => row.FileItem,
            _ => TryGetDataContextListedFileItem(selectedItem)
        };
    }

    private static ListedFileItem? TryGetDataContextListedFileItem(object selectedItem)
    {
        var dataContextProperty = selectedItem.GetType().GetProperty("DataContext");
        return dataContextProperty?.GetValue(selectedItem) as ListedFileItem;
    }
}
