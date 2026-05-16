using System.Diagnostics;

namespace VeloFile.Corpus.Tests.TestRuntime;

internal static class PublicCorpusScriptHarness
{
    public static CommandResult RunScript(string scriptName, params string[] arguments)
    {
        var repoRoot = TestRepo.FindRoot();
        var scriptPath = Path.Combine(repoRoot.FullName, "scripts", scriptName);

        var shell = OperatingSystem.IsWindows()
            ? "powershell.exe"
            : "pwsh";
        var startInfo = new ProcessStartInfo(shell)
        {
            WorkingDirectory = repoRoot.FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start PowerShell.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new CommandResult(
            process.ExitCode,
            stdoutTask.GetAwaiter().GetResult(),
            stderrTask.GetAwaiter().GetResult());
    }

    internal sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + StandardError;
    }
}
