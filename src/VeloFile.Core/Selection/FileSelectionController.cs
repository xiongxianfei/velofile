using VeloFile.Core.Listing;

namespace VeloFile.Core.Selection;

public sealed class FileSelectionController
{
    private readonly List<ListedFileItem> _items;
    private readonly SortedSet<int> _selectedIndices = [];
    private int _anchorIndex;

    public FileSelectionController(IReadOnlyList<ListedFileItem> items)
    {
        _items = items.ToList();
        FocusedIndex = _items.Count == 0 ? -1 : 0;
        _anchorIndex = FocusedIndex;
    }

    public int FocusedIndex { get; private set; }

    public IReadOnlyList<ListedFileItem> SelectedItems => _selectedIndices
        .Where(index => index >= 0 && index < _items.Count)
        .Select(index => _items[index])
        .ToArray();

    public IReadOnlyList<string> SelectedNames => SelectedItems
        .Select(item => item.Name)
        .ToArray();

    public void SelectSingle(int index)
    {
        if (!TryNormalizeIndex(index, out var normalized))
        {
            return;
        }

        FocusedIndex = normalized;
        _anchorIndex = normalized;
        _selectedIndices.Clear();
        _selectedIndices.Add(normalized);
    }

    public void ToggleSelection(int index)
    {
        if (!TryNormalizeIndex(index, out var normalized))
        {
            return;
        }

        FocusedIndex = normalized;
        _anchorIndex = normalized;
        if (!_selectedIndices.Add(normalized))
        {
            _selectedIndices.Remove(normalized);
        }
    }

    public void SelectRangeTo(int index)
    {
        if (!TryNormalizeIndex(index, out var normalized) || FocusedIndex < 0)
        {
            return;
        }

        FocusedIndex = normalized;
        _selectedIndices.Clear();
        foreach (var selectedIndex in RangeBetween(_anchorIndex, normalized))
        {
            _selectedIndices.Add(selectedIndex);
        }
    }

    public void MoveFocus(int delta, bool extendSelection, bool preserveSelection)
    {
        if (_items.Count == 0)
        {
            FocusedIndex = -1;
            _anchorIndex = -1;
            _selectedIndices.Clear();
            return;
        }

        var current = FocusedIndex < 0 ? 0 : FocusedIndex;
        var next = Math.Clamp(current + delta, 0, _items.Count - 1);
        FocusedIndex = next;

        if (extendSelection)
        {
            _selectedIndices.Clear();
            foreach (var selectedIndex in RangeBetween(_anchorIndex, next))
            {
                _selectedIndices.Add(selectedIndex);
            }

            return;
        }

        if (preserveSelection)
        {
            return;
        }

        _anchorIndex = next;
        _selectedIndices.Clear();
        _selectedIndices.Add(next);
    }

    public void SelectAll()
    {
        _selectedIndices.Clear();
        for (var index = 0; index < _items.Count; index++)
        {
            _selectedIndices.Add(index);
        }
    }

    public void ClearSelection()
    {
        _selectedIndices.Clear();
    }

    private bool TryNormalizeIndex(int index, out int normalized)
    {
        normalized = -1;
        if (_items.Count == 0)
        {
            FocusedIndex = -1;
            _anchorIndex = -1;
            return false;
        }

        normalized = Math.Clamp(index, 0, _items.Count - 1);
        return true;
    }

    private static IEnumerable<int> RangeBetween(int first, int second)
    {
        var start = Math.Min(first, second);
        var end = Math.Max(first, second);
        for (var index = start; index <= end; index++)
        {
            yield return index;
        }
    }
}
