namespace VeloFile.Core.Foundation;

public sealed class AppBootstrapper
{
    public InitialAppState CreateInitialState()
    {
        return new InitialAppState(
            WindowTitle: ProductIdentity.Name,
            AcceptsInput: true,
            RestoresExplorerReplacementMode: false);
    }
}
