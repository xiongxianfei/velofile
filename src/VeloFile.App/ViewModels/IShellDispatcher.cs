namespace VeloFile.App.ViewModels;

public interface IShellDispatcher
{
    void Post(Action action);
}

public sealed class ImmediateShellDispatcher : IShellDispatcher
{
    public static ImmediateShellDispatcher Instance { get; } = new();

    private ImmediateShellDispatcher()
    {
    }

    public void Post(Action action)
    {
        action();
    }
}
