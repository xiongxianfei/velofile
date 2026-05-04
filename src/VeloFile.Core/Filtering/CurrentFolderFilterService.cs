using VeloFile.Core.Listing;

namespace VeloFile.Core.Filtering;

public sealed class CurrentFolderFilterService
{
    public IReadOnlyList<ListedFileItem> Apply(
        IReadOnlyList<ListedFileItem> visibleItems,
        string? filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return visibleItems.ToArray();
        }

        var literal = filterText.Trim();
        return visibleItems
            .Where(item => item.Name.Contains(literal, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
