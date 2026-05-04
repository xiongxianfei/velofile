using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using VeloFile.App.ViewModels;
using VeloFile.App.Windowing;
using VeloFile.Core.Navigation;
using VeloFile.Core.Shell;
using Windows.System;

namespace VeloFile.App;

public sealed partial class MainWindow : Window
{
    private bool _isRefreshingShellBindings;

    public MainWindow(AppShellViewModel viewModel)
        : this(viewModel, new WinUiWindowPlacementApplier())
    {
    }

    public MainWindow(AppShellViewModel viewModel, IWindowPlacementApplier windowPlacementApplier)
    {
        InitializeComponent();
        ViewModel = viewModel;
        Title = ViewModel.WindowTitle;
        RootShell.DataContext = ViewModel;
        RefreshShellBindings();
        windowPlacementApplier.Apply(this, ViewModel.WindowPlacement);
    }

    public AppShellViewModel ViewModel { get; }

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
        args.Handled = true;
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
        }
        finally
        {
            _isRefreshingShellBindings = false;
        }
    }
}
