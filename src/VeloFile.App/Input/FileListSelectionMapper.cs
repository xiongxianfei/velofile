using VeloFile.Core.Listing;

namespace VeloFile.App.Input;

public interface IFileListRowItem
{
    ListedFileItem FileItem { get; }
}

public static class FileListSelectionMapper
{
    public static IReadOnlyList<ListedFileItem> ToListedFileItems(
        IEnumerable<object?> selectedItems,
        IReadOnlyList<ListedFileItem> visibleItems)
    {
        var selectedPaths = selectedItems
            .Select(TryGetListedFileItem)
            .Where(item => item is not null)
            .Select(item => item!.FullPath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return visibleItems
            .Where(item => selectedPaths.Contains(item.FullPath))
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
        var dataContext = dataContextProperty?.GetValue(selectedItem);
        return ReferenceEquals(dataContext, selectedItem) ? null : TryGetListedFileItem(dataContext);
    }
}
