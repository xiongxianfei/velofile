using System.Text.Json.Nodes;
using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class CorpusContractTests
{
    [TestMethod]
    public void Manifest_contract_runs_in_process_without_public_wrapper_artifacts()
    {
        using var scratch = ScratchWorkspace.Create();

        var result = CorpusToolHarness.RunInProcess("generate", "--profile", "smoke", "--root", scratch.Root);

        AssertCommandSucceeded(result);
        var manifestPath = Path.Combine(scratch.Root, "corpora", "smoke", "manifest.json");
        Assert.IsTrue(File.Exists(manifestPath), "Contract generation must write a manifest under the scratch root.");

        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        Assert.AreEqual("velofileCorpusManifest", (string?)manifest["documentType"]);
        Assert.AreEqual("smoke", (string?)manifest["profile"]);
        CollectionAssert.IsSubsetOf(
            new[] { "generate:smoke", "compat:smoke", "preview:smoke", "benchmarks:non-gating" },
            manifest["scopes"]!.AsArray().Select(value => (string)value!).ToArray());
        Assert.IsFalse(
            Directory.Exists(Path.Combine(scratch.Root, ".velofile-tools")),
            "In-process contract tests must not invoke the public wrapper scratch publish path.");
    }

    [TestMethod]
    public void Compatibility_contract_reports_scope_and_release_classification_in_process()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(CorpusToolHarness.RunInProcess("compat", "--scope", "operations", "--root", scratch.Root));

        var operationsResultPath = Path.Combine(scratch.Root, "corpora", "operations", "compat", "operations-result.json");
        Assert.IsTrue(File.Exists(operationsResultPath), "Operations contract must write its result under the scratch root.");
        var operationsResult = JsonNode.Parse(File.ReadAllText(operationsResultPath))!.AsObject();
        Assert.AreEqual("velofileCompatCorpusResult", (string?)operationsResult["documentType"]);
        Assert.AreEqual("operations", (string?)operationsResult["scope"]);
        CollectionAssert.IsSubsetOf(
            new[] { "operations/copy/source.txt", "operations/move/source.txt", "operations/delete-target.txt" },
            operationsResult["checkedFixtures"]!.AsArray().Select(value => (string)value!).ToArray());

        var release = CorpusToolHarness.RunInProcess("compat", "--scope", "release", "--root", scratch.Root);

        Assert.AreNotEqual(0, release.ExitCode, "Release compatibility remains blocked until required verifier evidence exists.");
        var releasePath = Path.Combine(scratch.Root, "corpora", "compatibility", "compat", "release-compat-result.json");
        Assert.IsTrue(File.Exists(releasePath), "Release classification must write a durable result document.");
        var releaseResult = JsonNode.Parse(File.ReadAllText(releasePath))!.AsObject();
        Assert.AreEqual("release", (string?)releaseResult["scope"]);
        Assert.AreEqual("incomplete", (string?)releaseResult["status"]);
        Assert.IsTrue((bool?)releaseResult["blocksReleaseEvidence"]);
        Assert.AreEqual(0, (int?)releaseResult["summary"]!["verifiedScopes"] ?? -1);
    }

    [TestMethod]
    public void Preview_and_diagnostics_contract_reports_are_redacted_and_scratch_local()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(CorpusToolHarness.RunInProcess("preview", "--scope", "contract", "--root", scratch.Root));
        AssertCommandSucceeded(CorpusToolHarness.RunInProcess("diagnostics", "--root", scratch.Root));

        var previewPath = Path.Combine(scratch.Root, "corpora", "preview", "preview", "preview-contract-result.json");
        var preview = JsonNode.Parse(File.ReadAllText(previewPath))!.AsObject();
        Assert.AreEqual("velofilePreviewCorpusResult", (string?)preview["documentType"]);
        Assert.AreEqual("contract", (string?)preview["scope"]);
        Assert.AreEqual("verified", (string?)preview["status"]);
        Assert.IsTrue((bool?)preview["verifiedBehavior"]);

        var diagnosticsPath = Path.Combine(scratch.Root, "diagnostics", "diagnostics-conformance-result.json");
        var exportPath = Path.Combine(scratch.Root, "diagnostics", "export", "diagnostics-redacted.jsonl");
        Assert.IsTrue(File.Exists(diagnosticsPath), "Diagnostics contract must write its result under the scratch root.");
        Assert.IsTrue(File.Exists(exportPath), "Diagnostics contract must write its redacted export under the scratch root.");

        var diagnostics = JsonNode.Parse(File.ReadAllText(diagnosticsPath))!.AsObject();
        Assert.AreEqual("velofileDiagnosticsConformanceResult", (string?)diagnostics["documentType"]);
        Assert.AreEqual("verified", (string?)diagnostics["status"]);
        Assert.IsTrue((bool?)diagnostics["localOnly"]);
        Assert.IsTrue((bool?)diagnostics["exportRedacted"]);
        Assert.IsFalse((bool?)diagnostics["prohibitedValuesFound"]);

        var exported = File.ReadAllText(exportPath);
        foreach (var prohibited in new[] { "alice", "secret-plan", "clipboard-secret", "preview text", "pwsh -NoProfile", "id_rsa" })
        {
            Assert.IsFalse(exported.Contains(prohibited, StringComparison.OrdinalIgnoreCase), $"Diagnostics export leaked '{prohibited}'.");
        }
    }

    private static void AssertCommandSucceeded(CorpusToolHarness.Result result)
    {
        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
    }
}
