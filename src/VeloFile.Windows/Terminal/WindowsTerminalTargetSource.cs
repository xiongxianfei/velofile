using System.Diagnostics;
using VeloFile.Core.Terminal;

namespace VeloFile.Windows.Terminal;

public interface IWindowsTerminalProbe
{
    string? FindExecutable(TerminalTargetKind kind);

    IReadOnlyList<string> GetWslDistributions(CancellationToken cancellationToken = default);
}

public sealed class WindowsTerminalTargetSource : ITerminalTargetSource
{
    private readonly IWindowsTerminalProbe _probe;

    public WindowsTerminalTargetSource(IWindowsTerminalProbe? probe = null)
    {
        _probe = probe ?? WindowsTerminalProbe.Instance;
    }

    public async ValueTask<IReadOnlyList<TerminalTarget>> GetAvailableTargetsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GetAvailableTargets(cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    private IReadOnlyList<TerminalTarget> GetAvailableTargets(CancellationToken cancellationToken)
    {
        var targets = new List<TerminalTarget>();
        AddTarget(targets, TerminalTargetKind.WindowsTerminal, "Windows Terminal", "windows-terminal");
        AddTarget(targets, TerminalTargetKind.PowerShell7, "PowerShell 7", "powershell-7");
        AddTarget(targets, TerminalTargetKind.WindowsPowerShell, "Windows PowerShell", "windows-powershell");
        AddTarget(targets, TerminalTargetKind.CommandPrompt, "Command Prompt", "command-prompt");
        AddTarget(targets, TerminalTargetKind.GitBash, "Git Bash", "git-bash");

        var wslPath = _probe.FindExecutable(TerminalTargetKind.WslDistribution);
        if (!string.IsNullOrWhiteSpace(wslPath))
        {
            foreach (var distribution in _probe.GetWslDistributions(cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(distribution))
                {
                    continue;
                }

                targets.Add(new TerminalTarget(
                    $"wsl:{distribution}",
                    TerminalTargetKind.WslDistribution,
                    $"WSL - {distribution}",
                    wslPath,
                    distribution));
            }
        }

        return targets;
    }

    private void AddTarget(
        List<TerminalTarget> targets,
        TerminalTargetKind kind,
        string displayName,
        string id)
    {
        var path = _probe.FindExecutable(kind);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        targets.Add(new TerminalTarget(id, kind, displayName, path));
    }
}

public sealed class WindowsTerminalProbe : IWindowsTerminalProbe
{
    public static WindowsTerminalProbe Instance { get; } = new();

    private WindowsTerminalProbe()
    {
    }

    public string? FindExecutable(TerminalTargetKind kind)
    {
        return kind switch
        {
            TerminalTargetKind.WindowsTerminal => FindOnPath("wt.exe"),
            TerminalTargetKind.PowerShell7 => FindOnPath("pwsh.exe") ?? FindKnownPowerShell7(),
            TerminalTargetKind.WindowsPowerShell => FindKnownWindowsPowerShell() ?? FindOnPath("powershell.exe"),
            TerminalTargetKind.CommandPrompt => Environment.GetEnvironmentVariable("ComSpec") ?? FindKnownCommandPrompt(),
            TerminalTargetKind.GitBash => FindOnPath("git-bash.exe") ?? FindKnownGitBash(),
            TerminalTargetKind.WslDistribution => FindOnPath("wsl.exe") ?? FindKnownWsl(),
            _ => null
        };
    }

    public IReadOnlyList<string> GetWslDistributions(CancellationToken cancellationToken = default)
    {
        var wsl = FindExecutable(TerminalTargetKind.WslDistribution);
        if (string.IsNullOrWhiteSpace(wsl))
        {
            return [];
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = wsl,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add("--list");
            process.StartInfo.ArgumentList.Add("--quiet");
            process.Start();
            if (!process.WaitForExit(milliseconds: 500))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best-effort cleanup for a nonessential discovery probe.
                }

                return [];
            }

            return process.StandardOutput
                .ReadToEnd()
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static string? FindKnownPowerShell7()
    {
        return FirstExisting(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7", "pwsh.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PowerShell", "7", "pwsh.exe"));
    }

    private static string? FindKnownWindowsPowerShell()
    {
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return FirstExisting(Path.Combine(systemRoot, "System32", "WindowsPowerShell", "v1.0", "powershell.exe"));
    }

    private static string? FindKnownCommandPrompt()
    {
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return FirstExisting(Path.Combine(systemRoot, "System32", "cmd.exe"));
    }

    private static string? FindKnownGitBash()
    {
        return FirstExisting(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "git-bash.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "git-bash.exe"));
    }

    private static string? FindKnownWsl()
    {
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return FirstExisting(Path.Combine(systemRoot, "System32", "wsl.exe"));
    }

    private static string? FindOnPath(string executableName)
    {
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? FirstExisting(params string[] paths)
    {
        return paths.FirstOrDefault(File.Exists);
    }
}
