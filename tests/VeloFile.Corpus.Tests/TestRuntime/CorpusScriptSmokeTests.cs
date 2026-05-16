using System.Text.Json.Nodes;
using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
public sealed class CorpusScriptSmokeTests
{
    private static readonly ScriptSmokeCase CompatSmoke = new(
        ScriptName: "run-compat-corpus.ps1",
        ArgumentsBeforeScratchRoot: ["-Scope", "smoke"],
        ExpectedOutput: "Compatibility smoke corpus passed.",
        ResultPathSegments: ["corpora", "smoke", "compat", "compat-smoke-result.json"],
        ExpectedDocumentType: "velofileCompatCorpusResult",
        ExpectedScope: "smoke");

    private static readonly ScriptSmokeCase PreviewSmoke = new(
        ScriptName: "run-preview-corpus.ps1",
        ArgumentsBeforeScratchRoot: ["-Scope", "smoke"],
        ExpectedOutput: "Preview smoke corpus passed.",
        ResultPathSegments: ["corpora", "smoke", "preview", "preview-smoke-result.json"],
        ExpectedDocumentType: "velofilePreviewCorpusResult",
        ExpectedScope: "smoke");

    private static readonly ScriptSmokeCase BenchmarkSmoke = new(
        ScriptName: "run-benchmarks.ps1",
        ArgumentsBeforeScratchRoot: BenchmarkSmokeArguments(),
        ExpectedOutput: "Wrote non-gating benchmark report.",
        ResultPathSegments: ["benchmarks", "benchmark-smoke-report.json"],
        ExpectedDocumentType: "velofileBenchmarkReport",
        ExpectedScope: null);

    private static readonly ScriptSmokeCase DiagnosticsSmoke = new(
        ScriptName: "run-diagnostics-conformance.ps1",
        ArgumentsBeforeScratchRoot: [],
        ExpectedOutput: "Diagnostics conformance verified.",
        ResultPathSegments: ["diagnostics", "diagnostics-conformance-result.json"],
        ExpectedDocumentType: "velofileDiagnosticsConformanceResult",
        ExpectedScope: null);

    [TestMethod]
    [TestCategory(CorpusTestCategories.CorpusScript)]
    [TestCategory(CorpusTestCategories.Smoke)]
    public void Compat_public_script_smoke_routes_and_writes_representative_output()
    {
        RunSmokeCase(CompatSmoke);
    }

    [TestMethod]
    [TestCategory(CorpusTestCategories.CorpusScript)]
    [TestCategory(CorpusTestCategories.Smoke)]
    public void Preview_public_script_smoke_routes_and_writes_representative_output()
    {
        RunSmokeCase(PreviewSmoke);
    }

    [TestMethod]
    [TestCategory(CorpusTestCategories.CorpusScript)]
    [TestCategory(CorpusTestCategories.Smoke)]
    public void Benchmark_public_script_smoke_routes_and_writes_representative_output()
    {
        RunSmokeCase(BenchmarkSmoke);
    }

    [TestMethod]
    [TestCategory(CorpusTestCategories.CorpusScript)]
    [TestCategory(CorpusTestCategories.Smoke)]
    public void Diagnostics_public_script_smoke_routes_and_writes_representative_output()
    {
        RunSmokeCase(DiagnosticsSmoke);
    }

    [TestMethod]
    [TestCategory(CorpusTestCategories.Contract)]
    public void Script_smoke_cases_use_minimal_scopes_not_release_matrices()
    {
        var cases = new[] { CompatSmoke, PreviewSmoke, BenchmarkSmoke, DiagnosticsSmoke };

        foreach (var smokeCase in cases)
        {
            var arguments = smokeCase.ArgumentsBeforeScratchRoot;
            Assert.IsFalse(arguments.Contains("release", StringComparer.OrdinalIgnoreCase), smokeCase.ScriptName);
            Assert.IsFalse(arguments.Contains("operations", StringComparer.OrdinalIgnoreCase), smokeCase.ScriptName);
            Assert.IsFalse(arguments.Contains("dragdrop", StringComparer.OrdinalIgnoreCase), smokeCase.ScriptName);
            Assert.IsFalse(arguments.Contains("paths", StringComparer.OrdinalIgnoreCase), smokeCase.ScriptName);
            Assert.IsFalse(arguments.Contains("providers", StringComparer.OrdinalIgnoreCase), smokeCase.ScriptName);
            Assert.IsFalse(arguments.Contains("thumbnails", StringComparer.OrdinalIgnoreCase), smokeCase.ScriptName);
        }

        CollectionAssert.Contains(BenchmarkSmoke.ArgumentsBeforeScratchRoot.ToArray(), "1", "Benchmark smoke must use the minimum run count.");
    }

    private static void RunSmokeCase(ScriptSmokeCase smokeCase)
    {
        using var scratch = ScratchWorkspace.Create();

        var result = PublicCorpusScriptHarness.RunScript(
            smokeCase.ScriptName,
            [.. smokeCase.ArgumentsBeforeScratchRoot, "-ScratchRoot", scratch.Root]);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, smokeCase.ExpectedOutput);

        var resultPath = Path.Combine([scratch.Root, .. smokeCase.ResultPathSegments]);
        Assert.IsTrue(File.Exists(resultPath), $"{smokeCase.ScriptName} must write representative smoke output.");
        var json = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        Assert.AreEqual(smokeCase.ExpectedDocumentType, (string?)json["documentType"]);
        if (smokeCase.ExpectedScope is not null)
        {
            Assert.AreEqual(smokeCase.ExpectedScope, (string?)json["scope"]);
        }
    }

    private static string[] BenchmarkSmokeArguments()
    {
        var appExecutable = OperatingSystem.IsWindows() ? "cmd.exe" : "pwsh";
        var appArguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-NoProfile -Command exit 0";

        return ["-NonGating", "-RunCount", "1", "-AppExecutablePath", appExecutable, "-AppArguments", appArguments];
    }

    private sealed record ScriptSmokeCase(
        string ScriptName,
        IReadOnlyList<string> ArgumentsBeforeScratchRoot,
        string ExpectedOutput,
        IReadOnlyList<string> ResultPathSegments,
        string ExpectedDocumentType,
        string? ExpectedScope);
}
