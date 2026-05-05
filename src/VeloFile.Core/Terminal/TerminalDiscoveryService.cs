namespace VeloFile.Core.Terminal;

public sealed class TerminalDiscoveryService
{
    private static readonly IReadOnlyDictionary<TerminalTargetKind, int> DefaultOrder = new Dictionary<TerminalTargetKind, int>
    {
        [TerminalTargetKind.WindowsTerminal] = 0,
        [TerminalTargetKind.PowerShell7] = 1,
        [TerminalTargetKind.WindowsPowerShell] = 2,
        [TerminalTargetKind.CommandPrompt] = 3,
        [TerminalTargetKind.GitBash] = 4,
        [TerminalTargetKind.WslDistribution] = 5
    };

    private readonly ITerminalTargetSource _targetSource;

    public TerminalDiscoveryService(ITerminalTargetSource targetSource)
    {
        _targetSource = targetSource;
    }

    public async ValueTask<TerminalDiscoveryResult> DiscoverAsync(
        string? preferredTargetId = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TerminalTarget> targets;
        try
        {
            targets = await _targetSource.GetAvailableTargetsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            targets = [];
        }

        var orderedTargets = targets
            .OrderBy(target => DefaultOrder.TryGetValue(target.Kind, out var order) ? order : int.MaxValue)
            .ThenBy(target => target.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(target => target.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var preferred = string.IsNullOrWhiteSpace(preferredTargetId)
            ? null
            : orderedTargets.FirstOrDefault(target => string.Equals(target.Id, preferredTargetId, StringComparison.OrdinalIgnoreCase));

        return new TerminalDiscoveryResult(orderedTargets, preferred ?? orderedTargets.FirstOrDefault());
    }
}
