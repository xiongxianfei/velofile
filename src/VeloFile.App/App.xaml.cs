using Microsoft.UI.Xaml;

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
        _window = new MainWindow(AppCompositionRoot.CreateShellViewModel());
        _window.Activate();
    }
}
