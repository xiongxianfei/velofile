using Microsoft.UI.Xaml;
using VeloFile.Core.Foundation;

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
        var initialState = new AppBootstrapper().CreateInitialState();

        _window = new MainWindow(initialState);
        _window.Activate();
    }
}
