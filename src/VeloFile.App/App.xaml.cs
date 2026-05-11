using Microsoft.UI.Xaml;
using VeloFile.App.Testing;
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
        var fixtureLaunch = UiFixtureLaunchGate.FromCurrentProcess(args.Arguments);
        if (fixtureLaunch.Status is UiFixtureLaunchStatus.Rejected)
        {
            Environment.Exit(fixtureLaunch.ExitCode);
            return;
        }

        var shellDispatcher = new WinUiShellDispatcher(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
        var viewModel = fixtureLaunch.ShouldLaunchFixture && fixtureLaunch.FixtureName is not null
            ? AppCompositionRoot.CreateFixtureShellViewModel(fixtureLaunch.FixtureName, shellDispatcher)
            : AppCompositionRoot.CreateShellViewModel(shellDispatcher);
        _window = new MainWindow(
            viewModel,
            AppCompositionRoot.CreateWindowPlacementApplier());
        _window.Activate();
    }
}
