using VeloFile.Core.Persistence;

namespace VeloFile.Core.Session;

public sealed class FileSystemPathExistenceProbe : IPathExistenceProbe
{
    public bool Exists(string path)
    {
        try
        {
            return Directory.Exists(path) || File.Exists(path);
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return false;
        }
    }
}

public sealed class FileSystemScrollAnchorResolver : IScrollAnchorResolver
{
    public bool Exists(string path, string anchorName)
    {
        try
        {
            return File.Exists(Path.Combine(path, anchorName)) || Directory.Exists(Path.Combine(path, anchorName));
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return false;
        }
    }
}

public sealed class PassThroughMonitorPlacementResolver : IMonitorPlacementResolver
{
    public bool IsAvailable(string? monitorDeviceName)
    {
        return true;
    }

    public WindowPlacementState? Fallback(WindowPlacementState? requestedPlacement)
    {
        return requestedPlacement;
    }
}

public sealed class CrashRecoverySignal : ICrashRecoverySignal
{
    private readonly Func<bool> _shouldOfferStartFresh;

    public CrashRecoverySignal(Func<bool> shouldOfferStartFresh)
    {
        _shouldOfferStartFresh = shouldOfferStartFresh;
    }

    public bool ShouldOfferStartFresh => _shouldOfferStartFresh();
}
