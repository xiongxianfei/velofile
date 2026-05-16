using Microsoft.UI.Input;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using VeloFile.App.Input;
using VeloFile.App.Testing;
using VeloFile.App.ViewModels;
using VeloFile.App.Windowing;
using VeloFile.Core.Commands;
using VeloFile.Core.DragDrop;
using VeloFile.Core.Navigation;
using VeloFile.Core.Operations;
using VeloFile.Core.Preview;
using VeloFile.Core.Session;
using VeloFile.Core.Shell;
using VeloFile.Windows.DragDrop;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;

namespace VeloFile.App;

public sealed partial class MainWindow : Window
{
    private readonly IKeyboardFocusContextProvider _keyboardFocusContextProvider;
    private readonly AppFileCommandAcceleratorRouter _fileCommandAcceleratorRouter;
    private readonly AppDragDropRoute _dragDropRoute;
    private readonly UiFixturePresentationState? _fixturePresentationState;
    private bool _isRefreshingShellBindings;
    private bool _isApplyingFixturePresentation;
    private bool _fixturePresentationApplied;
    private int _fixturePresentationAttempts;
    private int _previewArtifactVersion;
    private const int MaxFixturePresentationAttempts = 30;

    public MainWindow(AppShellViewModel viewModel)
        : this(viewModel, new WinUiWindowPlacementApplier(), fixturePresentationState: null)
    {
    }

    public MainWindow(
        AppShellViewModel viewModel,
        IWindowPlacementApplier windowPlacementApplier,
        UiFixturePresentationState? fixturePresentationState = null)
    {
        InitializeComponent();
        ViewModel = viewModel;
        _fixturePresentationState = fixturePresentationState;
        ViewModel.SetShellDispatcher(new WinUiShellDispatcher(DispatcherQueue));
        ConfigureDarkTitleBar();
        _keyboardFocusContextProvider = new WinUiKeyboardFocusContextProvider(RootShell, FileListSurface);
        _fileCommandAcceleratorRouter = new AppFileCommandAcceleratorRouter(ViewModel, _keyboardFocusContextProvider);
        _dragDropRoute = new AppDragDropRoute(ViewModel, new WinUiFileDropPayloadExtractor());
        ViewModel.ShellStateChanged += ViewModel_ShellStateChanged;
        Title = ViewModel.WindowTitle;
        RootShell.DataContext = ViewModel;
        RootShell.MinWidth = WindowPlacementPolicy.Default.MinimumRestorableWidth;
        RootShell.MinHeight = WindowPlacementPolicy.Default.MinimumRestorableHeight;
        RefreshShellBindings();
        windowPlacementApplier.Apply(this, ViewModel.WindowPlacementResolution);
    }

    public AppShellViewModel ViewModel { get; }

    private void ConfigureDarkTitleBar()
    {
        try
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            var titleBar = AppWindow.GetFromWindowId(windowId).TitleBar;
            titleBar.BackgroundColor = ResolveTitleBarColor("VfTitleBarBackgroundColor");
            titleBar.ForegroundColor = ResolveTitleBarColor("VfTitleBarForegroundColor");
            titleBar.InactiveBackgroundColor = ResolveTitleBarColor("VfTitleBarInactiveBackgroundColor");
            titleBar.InactiveForegroundColor = ResolveTitleBarColor("VfTitleBarInactiveForegroundColor");
            titleBar.ButtonBackgroundColor = ResolveTitleBarColor("VfTitleBarButtonBackgroundColor");
            titleBar.ButtonForegroundColor = ResolveTitleBarColor("VfTitleBarButtonForegroundColor");
            titleBar.ButtonHoverBackgroundColor = ResolveTitleBarColor("VfTitleBarButtonHoverBackgroundColor");
            titleBar.ButtonHoverForegroundColor = ResolveTitleBarColor("VfTitleBarButtonHoverForegroundColor");
            titleBar.ButtonPressedBackgroundColor = ResolveTitleBarColor("VfTitleBarButtonPressedBackgroundColor");
            titleBar.ButtonPressedForegroundColor = ResolveTitleBarColor("VfTitleBarButtonPressedForegroundColor");
            titleBar.ButtonInactiveBackgroundColor = ResolveTitleBarColor("VfTitleBarButtonInactiveBackgroundColor");
            titleBar.ButtonInactiveForegroundColor = ResolveTitleBarColor("VfTitleBarButtonInactiveForegroundColor");
        }
        catch
        {
            // Title bar color is visual polish; startup must remain usable if the platform rejects it.
        }
    }

    private static global::Windows.UI.Color ResolveTitleBarColor(string resourceKey)
    {
        var resource = Application.Current.Resources[resourceKey];
        if (resource is global::Windows.UI.Color color)
        {
            return color;
        }

        throw new InvalidOperationException($"Titlebar resource '{resourceKey}' must be a Color.");
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateBack();
        RefreshShellBindings();
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateForward();
        RefreshShellBindings();
    }

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToParent();
        RefreshShellBindings();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshActiveTab();
        RefreshShellBindings();
    }

    private void NewTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NewTab();
        RefreshShellBindings();
    }

    private void DuplicateTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.DuplicateActiveTab();
        RefreshShellBindings();
    }

    private void CloseTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseActiveTab();
        RefreshShellBindings();
    }

    private void ReopenClosedTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ReopenClosedTab();
        RefreshShellBindings();
    }

    private void PreviousTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SwitchPreviousTab();
        RefreshShellBindings();
    }

    private void NextTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SwitchNextTab();
        RefreshShellBindings();
    }

    private void RawPathBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is not VirtualKey.Enter)
        {
            return;
        }

        ViewModel.SubmitPath(RawPathBox.Text);
        e.Handled = true;
        RefreshShellBindings();
    }

    private void SidebarLocationsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not ShellNavigationTarget target)
        {
            return;
        }

        ViewModel.ActivateSidebarTarget(target);
        RefreshShellBindings();
    }

    private void BreadcrumbPathBar_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not BreadcrumbSegment segment)
        {
            return;
        }

        ViewModel.OpenBreadcrumbSegment(segment);
        RefreshShellBindings();
    }

    private void TabList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabList.SelectedIndex < 0 || TabList.SelectedIndex == ViewModel.ActiveTabIndex)
        {
            return;
        }

        ViewModel.SwitchToTab(TabList.SelectedIndex);
        RefreshShellBindings();
    }

    private void FileListSurface_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isApplyingFixturePresentation)
        {
            return;
        }

        ViewModel.SetSelectedFileItems(FileListSelectionMapper.ToListedFileItems(FileListSurface.SelectedItems, ViewModel.VisibleItems));
        RefreshShellBindings();
    }

    private void FileListSurface_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Open);
    }

    private void CurrentFolderFilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshingShellBindings)
        {
            return;
        }

        ViewModel.SetCurrentFolderFilter(CurrentFolderFilterBox.Text);
        RefreshShellBindings();
    }

    private void RecursiveSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.StartRecursiveSearch(RecursiveSearchBox.Text);
        RefreshShellBindings();
    }

    private void RecursiveSearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter)
        {
            ViewModel.StartRecursiveSearch(RecursiveSearchBox.Text);
            e.Handled = true;
            RefreshShellBindings();
        }
    }

    private void CancelSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelRecursiveSearch();
        RefreshShellBindings();
    }

    private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearRecursiveSearch();
        RefreshShellBindings();
    }

    private async void ConfirmPermanentDeleteButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ConfirmPermanentDeleteAsync(confirm: true);
        RefreshShellBindings();
    }

    private async void CancelPermanentDeleteButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ConfirmPermanentDeleteAsync(confirm: false);
        RefreshShellBindings();
    }

    private async void RenameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter)
        {
            ViewModel.SetPendingRenameText(RenameTextBox.Text);
            await ViewModel.CommitPendingRenameAsync();
            e.Handled = true;
            RefreshShellBindings();
        }
        else if (e.Key is VirtualKey.Escape)
        {
            ViewModel.CancelPendingRename();
            e.Handled = true;
            RefreshShellBindings();
        }
    }

    private void RenameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshingShellBindings)
        {
            return;
        }

        ViewModel.SetPendingRenameText(RenameTextBox.Text);
        RefreshShellBindings();
    }

    private async void CommitRenameButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetPendingRenameText(RenameTextBox.Text);
        await ViewModel.CommitPendingRenameAsync();
        RefreshShellBindings();
    }

    private void CancelRenameButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelPendingRename();
        RefreshShellBindings();
    }

    private void CancelFileOperationButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelFileOperation();
        RefreshShellBindings();
    }

    private async void SkipConflictButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Skip);
        RefreshShellBindings();
    }

    private async void ReplaceConflictButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.Replace);
        RefreshShellBindings();
    }

    private async void KeepBothConflictButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResolveFileOperationConflictAsync(FileOperationConflictChoice.KeepBoth);
        RefreshShellBindings();
    }

    private void ViewModel_ShellStateChanged(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(RefreshShellBindings);
    }

    private void ShowHiddenFilesToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isRefreshingShellBindings)
        {
            return;
        }

        ViewModel.SetShowHiddenFiles(ShowHiddenFilesToggle.IsOn);
    }

    private void ShowSystemFilesToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isRefreshingShellBindings)
        {
            return;
        }

        ViewModel.SetShowProtectedOperatingSystemFiles(ShowSystemFilesToggle.IsOn, confirmed: true);
    }

    private void ShowExtensionsToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isRefreshingShellBindings)
        {
            return;
        }

        ViewModel.SetShowFileExtensions(ShowExtensionsToggle.IsOn);
    }

    private async void TerminalTargetComboBox_DropDownOpened(object sender, object e)
    {
        await ViewModel.LoadTerminalTargetsAsync();
        RefreshShellBindings();
    }

    private void TerminalTargetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshingShellBindings)
        {
            return;
        }

        ViewModel.SelectTerminalTarget(TerminalTargetComboBox.SelectedItem as VeloFile.Core.Terminal.TerminalTarget);
        RefreshShellBindings();
    }

    private void CloseMissingLocationTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseActiveTab();
        RefreshShellBindings();
    }

    private void StartFreshButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.StartFresh();
        RefreshShellBindings();
    }

    private void ClearPathEntryErrorButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearPathEntryError();
        RefreshShellBindings();
    }

    private void BackAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.NavigateBack();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void ForwardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.NavigateForward();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void UpAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.NavigateToParent();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void NewTabAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.NewTab();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void CloseTabAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.CloseActiveTab();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void ReopenClosedTabAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.ReopenClosedTab();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void NextTabAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.SwitchNextTab();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void PreviousTabAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.SwitchPreviousTab();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void FocusPathAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        RawPathBox.Focus(FocusState.Keyboard);
        RawPathBox.SelectAll();
        args.Handled = true;
    }

    private void RefreshAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.RefreshActiveTab();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void TogglePreviewAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.TogglePreviewPane();
        args.Handled = true;
        RefreshShellBindings();
    }

    private void PdfPreviousPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.RequestPreviousPdfPage())
        {
            RefreshShellBindings();
        }
    }

    private void PdfNextPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.RequestNextPdfPage())
        {
            RefreshShellBindings();
        }
    }

    private void BuiltInFileContextMenu_Opening(object sender, object e)
    {
        RefreshFileContextMenuAvailability();
    }

    private void SelectAllAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var route = InvokeFileListShortcut(KeyGesture.Ctrl("A"));
        if (route.Status is KeyboardRouteStatus.Routed)
        {
            FileListSurface.SelectAll();
            args.Handled = true;
        }
    }

    private void ClearSelectionAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var route = InvokeFileListShortcut(KeyGesture.Escape());
        if (route.Status is KeyboardRouteStatus.Routed)
        {
            FileListSurface.SelectedItems.Clear();
            args.Handled = true;
        }
    }

    private void OpenAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        HandleFileListShortcutAccelerator(args, KeyGesture.Enter());
    }

    private void RenameAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        HandleFileListShortcutAccelerator(args, KeyGesture.F2());
    }

    private void DeleteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        HandleFileListShortcutAccelerator(args, KeyGesture.Delete());
    }

    private void PermanentDeleteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        HandleFileListShortcutAccelerator(args, KeyGesture.Delete(shift: true));
    }

    private void ParentFolderAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (HandleFileListShortcutAccelerator(args, KeyGesture.Backspace()))
        {
            RefreshShellBindings();
        }
    }

    private void CopyPathAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        HandleFileListShortcutAccelerator(args, KeyGesture.CtrlShift("C"));
    }

    private void CopyNameAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        HandleFileListShortcutAccelerator(args, KeyGesture.CtrlShift("N"));
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Open);
    }

    private void OpenWithMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.OpenWith);
    }

    private void CutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Cut);
    }

    private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Copy);
    }

    private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Paste);
    }

    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Rename);
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Delete);
    }

    private void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.Properties);
    }

    private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.CopyPath);
    }

    private void CopyNameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.CopyName);
    }

    private void OpenTerminalMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAvailableBuiltInCommand(VeloFileCommandId.OpenTerminalHere);
    }

    private KeyboardRouteResult InvokeFileListShortcut(KeyGesture gesture)
    {
        return _fileCommandAcceleratorRouter.Route(gesture);
    }

    private bool HandleFileListShortcutAccelerator(KeyboardAcceleratorInvokedEventArgs args, KeyGesture gesture)
    {
        var route = InvokeFileListShortcut(gesture);
        if (route.Status is not KeyboardRouteStatus.Routed)
        {
            return false;
        }

        args.Handled = true;
        RefreshShellBindings();
        return true;
    }

    private async void FileListSurface_DragOver(object sender, DragEventArgs e)
    {
        var deferral = e.GetDeferral();
        try
        {
            var result = await _dragDropRoute.DragOverAsync(e.DataView, CurrentDragDropModifiers());
            e.AcceptedOperation = ToDataPackageOperation(result.AcceptedOperation);
            RefreshShellBindings();
        }
        catch
        {
            _dragDropRoute.ReportFailure("drop-route-failed");
            e.AcceptedOperation = DataPackageOperation.None;
            RefreshShellBindings();
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void FileListSurface_DragLeave(object sender, DragEventArgs e)
    {
        _dragDropRoute.DragLeave();
        e.AcceptedOperation = DataPackageOperation.None;
        RefreshShellBindings();
    }

    private async void FileListSurface_Drop(object sender, DragEventArgs e)
    {
        var deferral = e.GetDeferral();
        try
        {
            var result = await _dragDropRoute.DropAsync(e.DataView, CurrentDragDropModifiers());
            e.AcceptedOperation = ToDataPackageOperation(result.AcceptedOperation);
            RefreshShellBindings();
        }
        catch
        {
            _dragDropRoute.ReportFailure("drop-route-failed");
            e.AcceptedOperation = DataPackageOperation.None;
            RefreshShellBindings();
        }
        finally
        {
            deferral.Complete();
        }
    }

    private static DataPackageOperation ToDataPackageOperation(AppDropAcceptedOperation operation)
    {
        return operation switch
        {
            AppDropAcceptedOperation.Copy => DataPackageOperation.Copy,
            AppDropAcceptedOperation.Move => DataPackageOperation.Move,
            AppDropAcceptedOperation.Link => DataPackageOperation.Link,
            _ => DataPackageOperation.None
        };
    }

    private static DragDropKeyModifiers CurrentDragDropModifiers()
    {
        var modifiers = DragDropKeyModifiers.None;
        if (IsKeyDown(VirtualKey.Control))
        {
            modifiers |= DragDropKeyModifiers.Control;
        }

        if (IsKeyDown(VirtualKey.Shift))
        {
            modifiers |= DragDropKeyModifiers.Shift;
        }

        if (IsKeyDown(VirtualKey.Menu))
        {
            modifiers |= DragDropKeyModifiers.Alt;
        }

        return modifiers;
    }

    private static bool IsKeyDown(VirtualKey key)
    {
        return (InputKeyboardSource.GetKeyStateForCurrentThread(key) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }

    private void ExecuteAvailableBuiltInCommand(VeloFileCommandId commandId)
    {
        if (ViewModel.IsBuiltInCommandAvailable(commandId, CanPasteFromClipboard()))
        {
            ViewModel.ExecuteBuiltInCommand(commandId);
            RefreshShellBindings();
        }
    }

    private void RefreshFileContextMenuAvailability()
    {
        OpenMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Open, CanPasteFromClipboard());
        OpenWithMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.OpenWith, CanPasteFromClipboard());
        CutMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Cut, CanPasteFromClipboard());
        CopyMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Copy, CanPasteFromClipboard());
        PasteMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Paste, CanPasteFromClipboard());
        RenameMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Rename, CanPasteFromClipboard());
        DeleteMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Delete, CanPasteFromClipboard());
        PropertiesMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.Properties, CanPasteFromClipboard());
        CopyPathMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.CopyPath, CanPasteFromClipboard());
        CopyNameMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.CopyName, CanPasteFromClipboard());
        OpenTerminalMenuItem.IsEnabled = ViewModel.IsBuiltInCommandAvailable(VeloFileCommandId.OpenTerminalHere, CanPasteFromClipboard());
    }

    private static bool CanPasteFromClipboard()
    {
        return false;
    }

    private void RefreshShellBindings()
    {
        _isRefreshingShellBindings = true;
        try
        {
            TabList.ItemsSource = ViewModel.Tabs;
            TabList.SelectedIndex = ViewModel.ActiveTabIndex;
            SidebarLocationsList.ItemsSource = ViewModel.SidebarNavigationTargets;
            BreadcrumbPathBar.ItemsSource = ViewModel.BreadcrumbSegments;
            FileListSurface.ItemsSource = ViewModel.FileListRows;
            ApplyUiFixturePresentationState();
            CurrentFolderFilterBox.Text = ViewModel.CurrentFolderFilterText;
            CancelSearchButton.IsEnabled = ViewModel.RecursiveSearch.CanCancel;
            ClearSearchButton.IsEnabled = ViewModel.IsRecursiveSearchDisplayActive;
            RecursiveSearchStatusText.Text = ViewModel.RecursiveSearchStatusText;
            SkippedLocationsList.ItemsSource = ViewModel.SearchSkippedLocations;
            SkippedLocationsList.Visibility = ViewModel.SearchSkippedLocationsVisible ? Visibility.Visible : Visibility.Collapsed;
            FileOperationStatusText.Text = ViewModel.FileOperationStatusText;
            LaunchStatusText.Text = ViewModel.LaunchStatusText;
            DropActionIndicatorText.Text = ViewModel.DropActionIndicatorText;
            DropActionIndicatorText.Visibility = ViewModel.DropActionIndicatorVisible ? Visibility.Visible : Visibility.Collapsed;
            PreviewColumn.Width = ViewModel.IsPreviewPaneOpen ? new GridLength(320) : new GridLength(0);
            PreviewPane.Visibility = ViewModel.IsPreviewPaneOpen ? Visibility.Visible : Visibility.Collapsed;
            AutomationProperties.SetName(PreviewPane, ViewModel.PreviewAccessibilityName);
            PreviewStatusText.Text = ViewModel.PreviewStatusText;
            PreviewContentText.Text = ViewModel.PreviewContentText;
            PdfPageNavigationPanel.Visibility = ViewModel.CanNavigatePdfPages ? Visibility.Visible : Visibility.Collapsed;
            PdfPageIndicatorText.Text = ViewModel.PreviewContentText;
            PdfPreviousPageButton.IsEnabled = ViewModel.CanRequestPreviousPdfPage;
            PdfNextPageButton.IsEnabled = ViewModel.CanRequestNextPdfPage;
            _ = SetPreviewArtifactAsync(ViewModel.PreviewDisplayContent, ++_previewArtifactVersion);
            PreviewMetadataList.ItemsSource = ViewModel.DetailsMetadataFields;
            CancelFileOperationButton.IsEnabled = ViewModel.CanCancelFileOperation;
            CancelFileOperationButton.Visibility = ViewModel.CanCancelFileOperation ? Visibility.Visible : Visibility.Collapsed;
            RenamePanel.Visibility = ViewModel.IsRenameActive ? Visibility.Visible : Visibility.Collapsed;
            RenameTextBox.Text = ViewModel.PendingRenameText;
            CommitRenameButton.IsEnabled = ViewModel.CanCommitRename;
            RenameErrorText.Text = ViewModel.RenameError ?? "";
            PermanentDeleteConfirmationPanel.Visibility = ViewModel.PendingPermanentDeleteConfirmation is null ? Visibility.Collapsed : Visibility.Visible;
            PermanentDeleteConfirmationText.Text = ViewModel.PendingPermanentDeleteConfirmation is null
                ? ""
                : PermanentDeleteConfirmationMessage(ViewModel.PendingPermanentDeleteConfirmation);
            FileOperationConflictPanel.Visibility = ViewModel.PendingFileOperationConflict is null ? Visibility.Collapsed : Visibility.Visible;
            FileOperationConflictText.Text = ViewModel.PendingFileOperationConflict is null
                ? ""
                : FileOperationConflictMessage(ViewModel.PendingFileOperationConflict);
            RawPathBox.Text = ViewModel.PathEntryError?.SubmittedPath ?? ViewModel.ActivePath;
            MissingLocationState.IsOpen = ViewModel.MissingLocationVisible;
            MissingLocationState.Message = ViewModel.MissingLocationPath is null
                ? "The restored path is no longer available."
                : $"The restored path is no longer available: {ViewModel.MissingLocationPath}";
            PathEntryFailureState.IsOpen = ViewModel.PathEntryErrorVisible;
            PathEntryFailureState.Message = ViewModel.PathEntryError is null
                ? "The submitted path could not be opened."
                : $"The submitted path could not be opened: {ViewModel.PathEntryError.SubmittedPath}";
            CrashRecoveryState.IsOpen = ViewModel.CrashRecovery.StartFreshOffered;
            ShowHiddenFilesToggle.IsOn = ViewModel.VisibilitySettings.ShowHiddenFiles;
            ShowSystemFilesToggle.IsOn = ViewModel.VisibilitySettings.ShowProtectedOperatingSystemFiles;
            ShowExtensionsToggle.IsOn = ViewModel.VisibilitySettings.ShowFileExtensions;
            TerminalTargetComboBox.ItemsSource = ViewModel.TerminalTargets;
            TerminalTargetComboBox.SelectedItem = ViewModel.SelectedTerminalTarget;
        }
        finally
        {
            _isRefreshingShellBindings = false;
        }
    }

    private void ApplyUiFixturePresentationState()
    {
        if (_fixturePresentationState is null || _fixturePresentationApplied)
        {
            return;
        }

        if (!_fixturePresentationState.HasTargets)
        {
            _fixturePresentationApplied = true;
            return;
        }

        var plan = UiFixturePresentationPlanner.Create(_fixturePresentationState, ViewModel.FileListRows);
        if (plan.SelectedRows.Count == 0 && plan.FocusedRow is null)
        {
            RetryOrFailFixturePresentation("Fixture presentation targets were not available in the file-list rows.");
            return;
        }

        _isApplyingFixturePresentation = true;
        try
        {
            FileListSurface.SelectedItems.Clear();
            foreach (var row in plan.SelectedRows)
            {
                FileListSurface.SelectedItems.Add(row);
            }
        }
        finally
        {
            _isApplyingFixturePresentation = false;
        }

        if (plan.SelectedRows.Count > 0)
        {
            ViewModel.SetSelectedFileItems(FileListSelectionMapper.ToListedFileItems(FileListSurface.SelectedItems, ViewModel.VisibleItems));
        }

        if (plan.FocusedRow is null)
        {
            _fixturePresentationApplied = true;
            return;
        }

        FileListSurface.ScrollIntoView(plan.FocusedRow);
        FileListSurface.UpdateLayout();
        if (FileListSurface.ContainerFromItem(plan.FocusedRow) is ListViewItem focusedContainer
            && focusedContainer.Focus(FocusState.Keyboard))
        {
            _fixturePresentationApplied = true;
            return;
        }

        RetryOrFailFixturePresentation("Fixture focused row container was not available.");
    }

    private void RetryOrFailFixturePresentation(string reason)
    {
        if (_fixturePresentationAttempts++ < MaxFixturePresentationAttempts)
        {
            var retryTimer = DispatcherQueue.CreateTimer();
            retryTimer.Interval = TimeSpan.FromMilliseconds(100);
            retryTimer.Tick += (_, _) =>
            {
                retryTimer.Stop();
                ApplyUiFixturePresentationState();
            };
            retryTimer.Start();
            return;
        }

        throw new InvalidOperationException(reason);
    }

    private async Task SetPreviewArtifactAsync(PreviewContent? content, int version)
    {
        var bytes = content?.ImageArtifact?.EncodedBytes
            ?? content?.PdfPageArtifact?.EncodedBytes;
        if (bytes is null || bytes.Length == 0)
        {
            if (version == _previewArtifactVersion)
            {
                PreviewImageSurface.Source = null;
                PreviewImageSurface.Visibility = Visibility.Collapsed;
            }

            return;
        }

        try
        {
            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream))
            {
                writer.WriteBytes(bytes);
                await writer.StoreAsync();
                writer.DetachStream();
            }

            stream.Seek(0);
            var image = new BitmapImage();
            await image.SetSourceAsync(stream);
            if (version != _previewArtifactVersion)
            {
                return;
            }

            PreviewImageSurface.Source = image;
            PreviewImageSurface.Visibility = Visibility.Visible;
        }
        catch
        {
            if (version == _previewArtifactVersion)
            {
                PreviewImageSurface.Source = null;
                PreviewImageSurface.Visibility = Visibility.Collapsed;
            }
        }
    }

    private static string PermanentDeleteConfirmationMessage(PermanentDeleteConfirmationRequest confirmation)
    {
        var prefix = confirmation.Reason is PermanentDeleteReason.RecycleBinUnavailable
            ? "Recycle Bin delete is unavailable. "
            : "";
        return $"{prefix}Permanently delete {confirmation.Items.Count} selected item(s)?";
    }

    private static string FileOperationConflictMessage(FileOperationConflict conflict)
    {
        return $"Name conflict for {conflict.ExistingName}. Choose how to continue.";
    }

    private sealed class WinUiFileDropPayloadExtractor : IAppDragDropPayloadExtractor
    {
        private readonly WindowsOleDragDropDataAdapter _adapter = new();

        public async ValueTask<AppDragDropPayload> ExtractAsync(object? data, CancellationToken cancellationToken = default)
        {
            if (data is not DataPackageView dataView)
            {
                return AppDragDropPayload.Unsupported("ole-drop-unsupported-payload");
            }

            if (!dataView.Contains(StandardDataFormats.StorageItems))
            {
                return AppDragDropPayload.Unsupported("ole-drop-unsupported-payload");
            }

            IReadOnlyList<IStorageItem> storageItems;
            try
            {
                storageItems = await dataView.GetStorageItemsAsync();
            }
            catch
            {
                return AppDragDropPayload.Unsupported("drop-storageitem-unavailable");
            }

            IReadOnlyList<string?> fileDropPaths;
            try
            {
                fileDropPaths = storageItems.Select(item => item.Path).ToArray();
            }
            catch
            {
                return AppDragDropPayload.Unsupported("drop-storageitem-path-unavailable");
            }

            return WinUiStorageItemDropPayloadProjection.ProjectPaths(fileDropPaths, paths =>
            {
                var result = _adapter.ExtractFileDrop(paths);

                return result.CanDrop
                    ? AppDragDropPayload.Supported(result.Items)
                    : AppDragDropPayload.Unsupported(result.ReasonCode ?? "ole-drop-no-supported-files");
            });
        }
    }

}
