namespace VeloFile.Core.Foundation;

public sealed record InitialAppState(
    string WindowTitle,
    bool AcceptsInput,
    bool RestoresExplorerReplacementMode);
