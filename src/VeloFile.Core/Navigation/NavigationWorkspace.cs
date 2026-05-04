using VeloFile.Core.Listing;

namespace VeloFile.Core.Navigation;

public sealed class NavigationWorkspace
{
    private readonly List<NavigationTab> _tabs;
    private readonly Stack<NavigationTab> _closedTabs = new();
    private readonly string _defaultPath;
    private int _nextTabNumber;

    private NavigationWorkspace(IEnumerable<NavigationTab> tabs, int activeTabIndex, int nextTabNumber, string defaultPath)
    {
        _defaultPath = NormalizePathInput(defaultPath);
        _tabs = tabs.ToList();
        if (_tabs.Count == 0)
        {
            _tabs.Add(NavigationTab.Create("tab-0001", _defaultPath));
        }

        ActiveTabIndex = ClampActiveTabIndex(activeTabIndex, _tabs.Count);
        _nextTabNumber = Math.Max(nextTabNumber, _tabs.Count + 1);
    }

    public IReadOnlyList<NavigationTab> Tabs => _tabs;

    public int ActiveTabIndex { get; private set; }

    public NavigationTab ActiveTab => _tabs[ActiveTabIndex];

    public static NavigationWorkspace Create(string initialPath)
    {
        var normalizedInitialPath = NormalizePathInput(initialPath);
        return new NavigationWorkspace([NavigationTab.Create("tab-0001", normalizedInitialPath)], activeTabIndex: 0, nextTabNumber: 2, normalizedInitialPath);
    }

    public static NavigationWorkspace FromRestoredTabs(IEnumerable<NavigationTab> tabs, int activeTabIndex, string defaultPath)
    {
        var restoredTabs = tabs.ToArray();
        return new NavigationWorkspace(restoredTabs, activeTabIndex, restoredTabs.Length + 1, defaultPath);
    }

    public NavigationTab OpenTab(string path)
    {
        var tab = NavigationTab.Create(NextTabId(), NormalizePathInput(path));
        _tabs.Add(tab);
        ActiveTabIndex = _tabs.Count - 1;
        return tab;
    }

    public NavigationTab DuplicateTab(string tabId)
    {
        var index = IndexOf(tabId);
        var source = _tabs[index];
        var duplicate = source with { Id = NextTabId() };
        _tabs.Insert(index + 1, duplicate);
        ActiveTabIndex = index + 1;
        return duplicate;
    }

    public void CloseTab(string tabId)
    {
        var index = IndexOf(tabId);
        var closed = _tabs[index];
        _tabs.RemoveAt(index);
        _closedTabs.Push(closed);

        if (_tabs.Count == 0)
        {
            _tabs.Add(NavigationTab.Create(NextTabId(), _defaultPath));
            ActiveTabIndex = 0;
            return;
        }

        if (index < ActiveTabIndex)
        {
            ActiveTabIndex--;
        }
        else if (index == ActiveTabIndex)
        {
            ActiveTabIndex = Math.Min(index, _tabs.Count - 1);
        }
    }

    public NavigationTab? ReopenClosedTab()
    {
        if (_closedTabs.Count == 0)
        {
            return null;
        }

        var closed = _closedTabs.Pop();
        var reopened = closed with { Id = NextTabId() };
        _tabs.Add(reopened);
        ActiveTabIndex = _tabs.Count - 1;
        return reopened;
    }

    public void ReorderTab(string tabId, int newIndex)
    {
        var activeId = ActiveTab.Id;
        var oldIndex = IndexOf(tabId);
        var tab = _tabs[oldIndex];
        _tabs.RemoveAt(oldIndex);
        var targetIndex = Math.Clamp(newIndex, 0, _tabs.Count);
        _tabs.Insert(targetIndex, tab);
        ActiveTabIndex = IndexOf(activeId);
    }

    public void SwitchToTab(int index)
    {
        ActiveTabIndex = ClampActiveTabIndex(index, _tabs.Count);
    }

    public void SwitchToTab(string tabId)
    {
        ActiveTabIndex = IndexOf(tabId);
    }

    public void SwitchNextTab()
    {
        if (_tabs.Count == 0)
        {
            return;
        }

        ActiveTabIndex = (ActiveTabIndex + 1) % _tabs.Count;
    }

    public void SwitchPreviousTab()
    {
        if (_tabs.Count == 0)
        {
            return;
        }

        ActiveTabIndex = (ActiveTabIndex - 1 + _tabs.Count) % _tabs.Count;
    }

    public void NavigateActive(string path)
    {
        NavigateActive(path, missingLocation: false);
    }

    public void NavigateActive(string path, bool missingLocation)
    {
        EnsureHasActiveTab();
        var tab = ActiveTab;
        var nextPath = NormalizePathInput(path);
        _tabs[ActiveTabIndex] = tab with
        {
            Path = nextPath,
            BackHistory = tab.BackHistory.Concat([tab.Path]).ToArray(),
            ForwardHistory = [],
            ScrollAnchorName = null,
            LocationState = missingLocation ? NavigationTabLocationState.MissingLocation : NavigationTabLocationState.Available,
            MissingPath = missingLocation ? nextPath : null
        };
    }

    public void NavigateFromRawPathInput(string rawPathInput)
    {
        NavigateActive(NormalizePathInput(rawPathInput));
    }

    public bool NavigateBack()
    {
        EnsureHasActiveTab();
        var tab = ActiveTab;
        if (tab.BackHistory.Count == 0)
        {
            return false;
        }

        var back = tab.BackHistory.ToList();
        var previousPath = back[^1];
        back.RemoveAt(back.Count - 1);
        _tabs[ActiveTabIndex] = tab with
        {
            Path = previousPath,
            BackHistory = back,
            ForwardHistory = new[] { tab.Path }.Concat(tab.ForwardHistory).ToArray(),
            ScrollAnchorName = null,
            LocationState = NavigationTabLocationState.Available,
            MissingPath = null
        };
        return true;
    }

    public bool NavigateForward()
    {
        EnsureHasActiveTab();
        var tab = ActiveTab;
        if (tab.ForwardHistory.Count == 0)
        {
            return false;
        }

        var forward = tab.ForwardHistory.ToList();
        var nextPath = forward[0];
        forward.RemoveAt(0);
        _tabs[ActiveTabIndex] = tab with
        {
            Path = nextPath,
            BackHistory = tab.BackHistory.Concat([tab.Path]).ToArray(),
            ForwardHistory = forward,
            ScrollAnchorName = null,
            LocationState = NavigationTabLocationState.Available,
            MissingPath = null
        };
        return true;
    }

    public void RefreshActive()
    {
        EnsureHasActiveTab();
        _tabs[ActiveTabIndex] = ActiveTab with { ReloadVersion = ActiveTab.ReloadVersion + 1 };
    }

    public void SetActiveViewMode(FileListViewMode viewMode)
    {
        EnsureHasActiveTab();
        _tabs[ActiveTabIndex] = ActiveTab with { ViewMode = viewMode };
    }

    public void SetActiveSort(string sortColumn, string sortDirection)
    {
        EnsureHasActiveTab();
        _tabs[ActiveTabIndex] = ActiveTab with
        {
            SortColumn = string.IsNullOrWhiteSpace(sortColumn) ? "name" : sortColumn,
            SortDirection = string.IsNullOrWhiteSpace(sortDirection) ? "ascending" : sortDirection
        };
    }

    public void SetActiveScrollAnchor(string? firstVisibleItemName)
    {
        EnsureHasActiveTab();
        _tabs[ActiveTabIndex] = ActiveTab with { ScrollAnchorName = firstVisibleItemName };
    }

    private string NextTabId()
    {
        return $"tab-{_nextTabNumber++:D4}";
    }

    private int IndexOf(string tabId)
    {
        var index = _tabs.FindIndex(tab => string.Equals(tab.Id, tabId, StringComparison.Ordinal));
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tabId), "Tab id does not exist in the workspace.");
        }

        return index;
    }

    private void EnsureHasActiveTab()
    {
        if (_tabs.Count == 0)
        {
            throw new InvalidOperationException("The workspace has no active tab.");
        }
    }

    private static int ClampActiveTabIndex(int index, int tabCount)
    {
        if (tabCount == 0)
        {
            return 0;
        }

        return Math.Clamp(index, 0, tabCount - 1);
    }

    private static string NormalizePathInput(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return path.Trim();
    }
}
