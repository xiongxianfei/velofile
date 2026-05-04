using System.Diagnostics;
using System.Text.Json.Nodes;

namespace VeloFile.Corpus.Tests;

[TestClass]
public sealed class CorpusToolingSmokeTests
{
    [TestMethod]
    public void Generate_corpus_refuses_repository_root()
    {
        var repoRoot = FindRepoRoot();

        var result = RunScript("generate-corpus.ps1", "-Profile", "smoke", "-ScratchRoot", repoRoot.FullName);

        Assert.AreNotEqual(0, result.ExitCode);
        StringAssert.Contains(result.AllOutput, "unsafe scratch root");
        Assert.IsFalse(File.Exists(Path.Combine(repoRoot.FullName, ".velofile-corpus-root")));
    }

    [TestMethod]
    public void Generate_placeholder_profiles_are_deterministic()
    {
        var profiles = new[] { "smoke", "operations", "preview", "search", "large-folder" };

        foreach (var profile in profiles)
        {
            using var scratch = ScratchWorkspace.Create();

            var first = RunScript("generate-corpus.ps1", "-Profile", profile, "-ScratchRoot", scratch.Root);
            AssertCommandSucceeded(first);

            var manifestPath = Path.Combine(scratch.Root, "corpora", profile, "manifest.json");
            Assert.IsTrue(File.Exists(manifestPath), $"{profile} corpus generation must write a manifest.");

            var firstManifest = File.ReadAllText(manifestPath);
            var manifest = JsonNode.Parse(firstManifest)!.AsObject();

            Assert.AreEqual("velofileCorpusManifest", (string?)manifest["documentType"]);
            Assert.AreEqual(profile, (string?)manifest["profile"]);
            Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, ".velofile-corpus-root")));
            Assert.IsTrue(Directory.Exists(Path.Combine(scratch.Root, "corpora", profile)));

            var second = RunScript("generate-corpus.ps1", "-Profile", profile, "-ScratchRoot", scratch.Root);
            AssertCommandSucceeded(second);

            Assert.AreEqual(firstManifest, File.ReadAllText(manifestPath));
        }
    }

    [TestMethod]
    public void Compatibility_and_preview_runners_validate_scope()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(RunScript("generate-corpus.ps1", "-Profile", "smoke", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-compat-corpus.ps1", "-Scope", "smoke", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-ScratchRoot", scratch.Root));

        var unimplemented = RunScript("run-compat-corpus.ps1", "-Scope", "operations", "-ScratchRoot", scratch.Root);

        Assert.AreNotEqual(0, unimplemented.ExitCode);
        StringAssert.Contains(unimplemented.AllOutput, "not implemented");
    }

    [TestMethod]
    public void Benchmark_stub_emits_non_gating_report_shape()
    {
        using var scratch = ScratchWorkspace.Create();

        var result = RunScript("run-benchmarks.ps1", "-NonGating", "-ScratchRoot", scratch.Root);

        AssertCommandSucceeded(result);

        var reportPath = Path.Combine(scratch.Root, "benchmarks", "benchmark-smoke-report.json");
        Assert.IsTrue(File.Exists(reportPath), "Benchmark stub must write its report inside the scratch root.");

        var report = JsonNode.Parse(File.ReadAllText(reportPath))!.AsObject();
        var environment = report["environment"]!.AsObject();
        var measurement = report["measurements"]!.AsArray()[0]!.AsObject();

        Assert.AreEqual("velofileBenchmarkReport", (string?)report["documentType"]);
        Assert.IsTrue((bool?)report["nonGating"]);
        Assert.IsTrue(environment.ContainsKey("osBuild"));
        Assert.IsTrue(environment.ContainsKey("hardwareClass"));
        Assert.IsTrue(environment.ContainsKey("cpu"));
        Assert.IsTrue(environment.ContainsKey("ram"));
        Assert.IsTrue(environment.ContainsKey("storageType"));
        Assert.IsTrue(environment.ContainsKey("windowsSearchState"));
        Assert.IsTrue(environment.ContainsKey("antivirusState"));
        Assert.IsTrue(environment.ContainsKey("dpiConfiguration"));
        Assert.IsTrue(measurement.ContainsKey("runCount"));
        Assert.IsTrue(measurement.ContainsKey("medianMs"));
        Assert.IsTrue(measurement.ContainsKey("p95Ms"));
        Assert.IsTrue(measurement.ContainsKey("p99Ms"));
    }

    [TestMethod]
    public void Corpus_tool_wrapper_disables_dotnet_global_tools_path_mutation()
    {
        var repoRoot = FindRepoRoot();
        var wrapperPath = Path.Combine(repoRoot.FullName, "scripts", "Invoke-CorpusTool.ps1");
        var wrapper = File.ReadAllText(wrapperPath);

        StringAssert.Contains(wrapper, "\"DOTNET_ADD_GLOBAL_TOOLS_TO_PATH\"");
        StringAssert.Contains(wrapper, "Set-VeloFileEnvironmentValue -Name \"DOTNET_ADD_GLOBAL_TOOLS_TO_PATH\" -Value \"0\"");
    }

    [TestMethod]
    public void Corpus_scripts_do_not_add_scratch_dotnet_tools_to_user_path()
    {
        using var scratch = ScratchWorkspace.Create();
        var before = UserPathEntries().ToHashSet(StringComparer.OrdinalIgnoreCase);

        var result = RunScript("generate-corpus.ps1", "-Profile", "smoke", "-ScratchRoot", scratch.Root);

        AssertCommandSucceeded(result);
        var addedScratchPaths = UserPathEntries()
            .Where(entry => !before.Contains(entry))
            .Where(IsVeloFileScratchDotnetToolsPath)
            .ToArray();

        Assert.IsEmpty(addedScratchPaths, string.Join(Environment.NewLine, addedScratchPaths));
    }

    private static void AssertCommandSucceeded(CommandResult result)
    {
        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
    }

    private static CommandResult RunScript(string scriptName, params string[] arguments)
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot.FullName, "scripts", scriptName);

        var startInfo = new ProcessStartInfo("pwsh")
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

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start pwsh.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new CommandResult(process.ExitCode, stdout, stderr);
    }

    private static DirectoryInfo FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "VeloFile.sln")))
            {
                return current;
            }

            current = current.Parent;
        }

        Assert.Fail("Could not find repository root from test output directory.");
        throw new InvalidOperationException("Could not find repository root from test output directory.");
    }

    private static IReadOnlyList<string> UserPathEntries()
    {
        var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? string.Empty;
        return path.Split(
            Path.PathSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsVeloFileScratchDotnetToolsPath(string path)
    {
        return path.Contains("velofile-corpus-tests", StringComparison.OrdinalIgnoreCase)
            && path.Contains(".velofile-tools", StringComparison.OrdinalIgnoreCase)
            && path.Contains("dotnet-cli-home", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(Path.Combine(".dotnet", "tools"), StringComparison.OrdinalIgnoreCase);
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + StandardError;
    }

    private sealed class ScratchWorkspace : IDisposable
    {
        private ScratchWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static ScratchWorkspace Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "velofile-corpus-tests", "velofile-corpus-" + Guid.NewGuid().ToString("N"));
            return new ScratchWorkspace(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                for (var attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        Directory.Delete(Root, recursive: true);
                        return;
                    }
                    catch (IOException) when (attempt < 4)
                    {
                        Thread.Sleep(200);
                    }
                    catch (UnauthorizedAccessException) when (attempt < 4)
                    {
                        Thread.Sleep(200);
                    }
                }

                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
