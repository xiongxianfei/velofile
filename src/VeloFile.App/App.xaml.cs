using Microsoft.UI.Xaml;
using VeloFile.App.ViewModels;

namespace VeloFile.App;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var shellDispatcher = new WinUiShellDispatcher(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
        _window = new MainWindow(
            AppCompositionRoot.CreateShellViewModel(shellDispatcher),
            AppCompositionRoot.CreateWindowPlacementApplier());
        _window.Activate();
    }
}
