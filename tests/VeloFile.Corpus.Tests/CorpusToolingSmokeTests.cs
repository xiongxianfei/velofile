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
        var profiles = new[] { "smoke", "operations", "preview", "search", "large-folder", "dragdrop", "pathological" };

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
    [TestCategory("Benchmarks")]
    public void M15_reference_profiles_are_scaled_and_release_scoped()
    {
        var expectedMinimumFiles = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["small"] = 10,
            ["medium"] = 120,
            ["large"] = 1_000,
            ["deep"] = 1_050,
            ["preview"] = 8,
            ["pathological"] = 6
        };

        using var scratch = ScratchWorkspace.Create();

        foreach (var profile in expectedMinimumFiles.Keys)
        {
            AssertCommandSucceeded(RunScript("generate-corpus.ps1", "-Profile", profile, "-ScratchRoot", scratch.Root));

            var manifestPath = Path.Combine(scratch.Root, "corpora", profile, "manifest.json");
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            var files = manifest["files"]!.AsArray();
            var scopes = manifest["scopes"]!.AsArray().Select(value => (string)value!).ToArray();

            Assert.AreEqual(profile, (string?)manifest["profile"]);
            Assert.IsGreaterThanOrEqualTo(expectedMinimumFiles[profile], files.Count, $"{profile} corpus must have M15-scale fixtures.");
            Assert.IsTrue(scopes.Any(scope => scope.StartsWith("benchmark:", StringComparison.Ordinal)), $"{profile} corpus must advertise benchmark scope.");
            Assert.IsTrue(scopes.Any(scope => scope.StartsWith("compat:", StringComparison.Ordinal) || scope.StartsWith("preview:", StringComparison.Ordinal)), $"{profile} corpus must advertise validation scope.");
        }
    }

    [TestMethod]
    public void Compatibility_and_preview_runners_validate_scope()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(RunScript("generate-corpus.ps1", "-Profile", "smoke", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-compat-corpus.ps1", "-Scope", "smoke", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-compat-corpus.ps1", "-Scope", "safe-delete", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-compat-corpus.ps1", "-Scope", "operations", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-compat-corpus.ps1", "-Scope", "dragdrop", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-compat-corpus.ps1", "-Scope", "paths", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-Scope", "contract", "-ScratchRoot", scratch.Root));
        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-Scope", "providers", "-ScratchRoot", scratch.Root));

        var operationsResultPath = Path.Combine(scratch.Root, "corpora", "operations", "compat", "operations-result.json");
        Assert.IsTrue(File.Exists(operationsResultPath), "Operations compatibility runner must write a result document.");

        var operationsResult = JsonNode.Parse(File.ReadAllText(operationsResultPath))!.AsObject();
        Assert.AreEqual("velofileCompatCorpusResult", (string?)operationsResult["documentType"]);
        Assert.AreEqual("operations", (string?)operationsResult["scope"]);
        CollectionAssert.AreEquivalent(
            new[]
            {
                "operations/copy/source.txt",
                "operations/move/source.txt",
                "operations/rename-source.txt",
                "operations/delete-target.txt",
                "operations/collisions/existing-name.txt",
                "operations/collisions/incoming-name.txt",
                "operations/batch/partial-0001.txt",
                "operations/batch/partial-0002.txt"
            },
            operationsResult["checkedFixtures"]!.AsArray().Select(value => (string)value!).ToArray());

        var dragDropResultPath = Path.Combine(scratch.Root, "corpora", "dragdrop", "compat", "dragdrop-result.json");
        Assert.IsTrue(File.Exists(dragDropResultPath), "Drag/drop compatibility runner must write a result document.");
        var dragDropResult = JsonNode.Parse(File.ReadAllText(dragDropResultPath))!.AsObject();
        Assert.AreEqual("dragdrop", (string?)dragDropResult["scope"]);
        CollectionAssert.AreEquivalent(
            new[] { "none:same-volume:move", "none:cross-volume:copy", "ctrl:any:copy", "shift:any:move", "ctrl-shift:any:shortcut" },
            dragDropResult["resolvedActions"]!.AsArray().Select(value => (string)value!).ToArray());

        var pathsResultPath = Path.Combine(scratch.Root, "corpora", "pathological", "compat", "paths-result.json");
        Assert.IsTrue(File.Exists(pathsResultPath), "Path compatibility runner must write a result document.");
        var pathsResult = JsonNode.Parse(File.ReadAllText(pathsResultPath))!.AsObject();
        Assert.AreEqual("paths", (string?)pathsResult["scope"]);
        var pathCases = pathsResult["caseResults"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();
        Assert.IsGreaterThanOrEqualTo(6, pathCases.Length, "Path compatibility must report individual case outcomes.");
        Assert.IsFalse(pathCases.Any(result => ((string?)result["caseId"])?.Contains("placeholder", StringComparison.OrdinalIgnoreCase) == true));
        Assert.IsFalse(pathCases.Any(result => string.Equals((string?)result["status"], "passed", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(pathCases.Any(result => string.Equals((string?)result["status"], "verified", StringComparison.Ordinal)));
        Assert.IsTrue(pathCases.Any(result => string.Equals((string?)result["status"], "skipped", StringComparison.Ordinal)
            || string.Equals((string?)result["status"], "unavailable", StringComparison.Ordinal)));

        foreach (var pathCase in pathCases)
        {
            var status = (string?)pathCase["status"];
            Assert.IsTrue(
                status is "verified" or "skipped" or "unavailable" or "not-applicable" or "not-implemented" or "failed",
                $"Unexpected path compatibility status '{status}'.");

            if (status is "verified")
            {
                Assert.IsTrue((bool?)pathCase["fixtureCreated"], "Verified cases must create a fixture.");
                Assert.IsTrue((bool?)pathCase["fixtureVerified"], "Verified cases must verify the OS fixture.");
                Assert.IsTrue((bool?)pathCase["behaviorVerifierInvoked"], "Verified cases must invoke a behavior verifier.");
                Assert.IsTrue((bool?)pathCase["verifiedBehavior"], "Verified cases must check behavior.");
                Assert.AreNotEqual("fixture-only", (string?)pathCase["evidenceKind"], "Fixture creation alone is not compatibility behavior.");
                Assert.AreNotEqual(true, (bool?)pathCase["blocksReleaseEvidence"], "Verified cases must count as release evidence.");
            }
            else
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace((string?)pathCase["reasonCode"]), "Non-verified cases must include a reason code.");
                Assert.IsFalse((bool?)pathCase["verifiedBehavior"], "Non-verified cases must not claim behavior verification.");
            }

            StringAssert.Contains((string?)pathCase["fixturePathKind"] ?? "", "scratch-relative");
        }

        var unimplemented = RunScript("run-compat-corpus.ps1", "-Scope", "future-scope", "-ScratchRoot", scratch.Root);

        Assert.AreNotEqual(0, unimplemented.ExitCode);
        StringAssert.Contains(unimplemented.AllOutput, "not implemented");
    }

    [TestMethod]
    [TestCategory("Compatibility")]
    public void Compatibility_release_scope_consumes_real_scope_results_without_upgrading_missing_evidence()
    {
        using var scratch = ScratchWorkspace.Create();

        var command = RunScript("run-compat-corpus.ps1", "-Scope", "release", "-ScratchRoot", scratch.Root);
        Assert.AreNotEqual(0, command.ExitCode, "Release compatibility must block when required verifier evidence is missing.");

        var resultPath = Path.Combine(scratch.Root, "corpora", "compatibility", "compat", "release-compat-result.json");
        Assert.IsTrue(File.Exists(resultPath), "Release compatibility aggregation must write a durable result document.");
        var result = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        var scopes = result["scopeResults"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();

        Assert.AreEqual("release", (string?)result["scope"]);
        Assert.AreEqual("incomplete", (string?)result["status"]);
        CollectionAssert.AreEquivalent(
            new[] { "operations", "dragdrop", "paths", "associations", "dpi" },
            scopes.Select(value => (string)value["scope"]!).ToArray());
        Assert.IsTrue((bool?)result["blocksReleaseEvidence"], "Missing required verifier evidence must block release evidence.");
        Assert.AreEqual(0, (int?)result["summary"]!["verifiedScopes"] ?? -1, "Fixture-only and missing verifier scopes must not be counted as verified.");

        foreach (var scope in scopes)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace((string?)scope["evidenceKind"]), "Each aggregated compatibility scope must name evidence kind.");
            var status = (string?)scope["status"];
            Assert.IsTrue(
                status is "verified" or "skipped" or "unavailable" or "not-implemented" or "failed" or "fixture-only",
                $"Unexpected release compatibility status '{status}'.");

            if (status is "verified")
            {
                Assert.IsTrue((bool?)scope["behaviorVerifierInvoked"], "Verified release scopes must preserve source verifier proof.");
                Assert.IsTrue((bool?)scope["verifiedBehavior"], "Verified release scopes must preserve source behavior proof.");
                Assert.IsTrue((bool?)scope["releaseEvidence"], "Verified release scopes must count as release evidence.");
            }
            else
            {
                Assert.IsFalse((bool?)scope["verifiedBehavior"], $"Non-verified scope {scope["scope"]} must not claim verified behavior.");
                Assert.IsFalse((bool?)scope["releaseEvidence"], $"Non-verified scope {scope["scope"]} must not count as release evidence.");
                Assert.IsFalse(string.IsNullOrWhiteSpace((string?)scope["reasonCode"]), $"Non-verified scope {scope["scope"]} must explain why.");
            }
        }

        var association = scopes.Single(scope => (string?)scope["scope"] == "associations");
        Assert.AreEqual("not-implemented", (string?)association["status"]);
        Assert.IsFalse((bool?)association["behaviorVerifierInvoked"], "Association release evidence requires an actual verifier input.");

        var dpi = scopes.Single(scope => (string?)scope["scope"] == "dpi");
        Assert.AreEqual("not-implemented", (string?)dpi["status"]);
        Assert.IsFalse((bool?)dpi["behaviorVerifierInvoked"], "DPI release evidence requires an actual verifier or checklist input.");
    }

    [TestMethod]
    [TestCategory("PreviewContract")]
    public void PreviewContract_scope_records_contract_behavior_evidence()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-Scope", "contract", "-ScratchRoot", scratch.Root));

        var resultPath = Path.Combine(
            scratch.Root,
            "corpora",
            "preview",
            "preview",
            "preview-contract-result.json");
        Assert.IsTrue(File.Exists(resultPath), "Preview contract scope must write a durable result document.");
        var result = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        var cases = result["caseResults"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();

        Assert.AreEqual("contract", (string?)result["scope"]);
        Assert.AreEqual("verified", (string?)result["status"]);
        Assert.IsTrue((bool?)result["behaviorVerifierInvoked"]);
        Assert.IsTrue((bool?)result["verifiedBehavior"]);
        CollectionAssert.IsSubsetOf(
            new[] { "loading-delay", "timeout-policy", "timeout", "metadata-fallback", "stale-selection" },
            cases.Select(value => (string)value["caseId"]!).ToArray());
    }

    [TestMethod]
    [TestCategory("PreviewProviders")]
    public void PreviewProviders_scope_records_provider_behavior_evidence()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-Scope", "providers", "-ScratchRoot", scratch.Root));

        var resultPath = Path.Combine(
            scratch.Root,
            "corpora",
            "preview",
            "preview",
            "preview-providers-result.json");
        Assert.IsTrue(File.Exists(resultPath), "Preview providers scope must write a durable result document.");
        var result = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        var cases = result["caseResults"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();

        Assert.AreEqual("providers", (string?)result["scope"]);
        Assert.AreEqual("verified", (string?)result["status"]);
        Assert.IsTrue((bool?)result["behaviorVerifierInvoked"]);
        Assert.IsTrue((bool?)result["verifiedBehavior"]);
        CollectionAssert.IsSubsetOf(
            new[] { "image-success", "text-truncation", "pdf-first-page", "oversize-fallback", "source-non-mutation" },
            cases.Select(value => (string)value["caseId"]!).ToArray());
    }

    [TestMethod]
    [TestCategory("Thumbnails")]
    public void Thumbnails_scope_records_thumbnail_behavior_evidence()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(RunScript("run-preview-corpus.ps1", "-Scope", "thumbnails", "-ScratchRoot", scratch.Root));

        var resultPath = Path.Combine(
            scratch.Root,
            "corpora",
            "preview",
            "preview",
            "preview-thumbnails-result.json");
        Assert.IsTrue(File.Exists(resultPath), "Thumbnail preview scope must write a durable result document.");
        var result = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        var cases = result["caseResults"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();

        Assert.AreEqual("thumbnails", (string?)result["scope"]);
        Assert.AreEqual("verified", (string?)result["status"]);
        Assert.IsTrue((bool?)result["behaviorVerifierInvoked"]);
        Assert.IsTrue((bool?)result["verifiedBehavior"]);
        CollectionAssert.IsSubsetOf(
            new[] { "thumbnail-concurrency", "thumbnail-timeout", "generic-icon-fallback", "stale-thumbnail-ignore" },
            cases.Select(value => (string)value["caseId"]!).ToArray());
    }

    [TestMethod]
    public void Benchmark_harness_emits_measured_report_environment_and_release_status()
    {
        using var scratch = ScratchWorkspace.Create();
        var appExecutable = OperatingSystem.IsWindows() ? "cmd.exe" : "pwsh";
        var appArguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-NoProfile -Command exit 0";

        var result = RunScript(
            "run-benchmarks.ps1",
            "-NonGating",
            "-ScratchRoot",
            scratch.Root,
            "-RunCount",
            "3",
            "-AppExecutablePath",
            appExecutable,
            "-AppArguments",
            appArguments);

        AssertCommandSucceeded(result);

        var reportPath = Path.Combine(scratch.Root, "benchmarks", "benchmark-report.json");
        Assert.IsTrue(File.Exists(reportPath), "Benchmark harness must write its report inside the scratch root.");

        var report = JsonNode.Parse(File.ReadAllText(reportPath))!.AsObject();
        var environment = report["environment"]!.AsObject();
        var measurements = report["measurements"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();
        var scenarioCoverage = report["scenarioCoverage"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();

        Assert.AreEqual("velofileBenchmarkReport", (string?)report["documentType"]);
        Assert.IsTrue((bool?)report["nonGating"]);
        Assert.AreEqual("non-gating", (string?)report["releaseSummary"]!["status"]);
        Assert.IsTrue(environment.ContainsKey("osBuild"));
        Assert.IsTrue(environment.ContainsKey("hardwareClass"));
        Assert.IsTrue(environment.ContainsKey("cpu"));
        Assert.IsTrue(environment.ContainsKey("ramBytes"));
        Assert.IsTrue(environment.ContainsKey("storageType"));
        Assert.IsTrue(environment.ContainsKey("windowsSearchState"));
        Assert.IsTrue(environment.ContainsKey("antivirusState"));
        Assert.IsTrue(environment.ContainsKey("dpiConfiguration"));
        Assert.IsTrue(environment.ContainsKey("processorArchitecture"));
        CollectionAssert.IsSubsetOf(
            new[]
            {
                "app.process.launch",
                "folder.switch.small",
                "folder.switch.medium",
                "folder.switch.large",
                "filter.medium",
                "search.deep.firstResult",
                "search.deep.thousandResults",
                "contextMenu.open",
                "tab.switch",
                "session.restore.10tabs"
            },
            measurements.Select(value => (string)value["name"]!).ToArray());

        foreach (var measurement in measurements)
        {
            Assert.IsGreaterThan(0, (int?)measurement["runCount"] ?? 0, $"Measurement {measurement["name"]} must run at least once.");
            Assert.IsNotNull((double?)measurement["medianMs"], $"Measurement {measurement["name"]} must record median.");
            Assert.IsNotNull((double?)measurement["p95Ms"], $"Measurement {measurement["name"]} must record p95.");
            Assert.IsNotNull((double?)measurement["p99Ms"], $"Measurement {measurement["name"]} must record p99.");
            Assert.IsFalse(string.IsNullOrWhiteSpace((string?)measurement["releaseGatingStatus"]));
            Assert.AreEqual("infrastructure-only", (string?)measurement["measurementKind"], $"Measurement {measurement["name"]} must not be mislabeled as app-level release evidence.");
            Assert.IsFalse((bool?)measurement["releaseEvidence"], $"Measurement {measurement["name"]} must not count as P1-P13/AC15 release evidence without an app boundary driver.");
            Assert.IsFalse((bool?)measurement["appBoundaryDriven"], $"Measurement {measurement["name"]} must state that no app boundary was driven.");
            Assert.IsTrue((bool?)measurement["substituteMeasurement"], $"Measurement {measurement["name"]} must disclose that it is a substitute measurement.");
            Assert.IsFalse(string.IsNullOrWhiteSpace((string?)measurement["scenarioId"]), $"Measurement {measurement["name"]} must map to a scenario ID.");
            Assert.AreEqual("app-level-driver-not-implemented", (string?)measurement["reasonCode"], $"Measurement {measurement["name"]} must explain why it is not release evidence.");
        }

        CollectionAssert.AreEquivalent(
            new[]
            {
                "T039.launch",
                "T039.folder-switch",
                "T039.current-folder-filter",
                "T039.recursive-search",
                "T039.context-menu-open",
                "T039.tab-switch",
                "T039.session-restore",
                "T039.sustained-scroll",
                "T039.slow-tab-isolation",
                "T039.terminal-discovery-impact"
            },
            scenarioCoverage.Select(value => (string)value["scenarioId"]!).ToArray());
        foreach (var scenario in scenarioCoverage)
        {
            Assert.AreEqual("app-level", (string?)scenario["requiredMeasurementKind"]);
            Assert.IsFalse((bool?)scenario["appBoundaryDriven"], $"Scenario {scenario["scenarioId"]} must disclose that no app-level driver ran.");
            Assert.IsFalse((bool?)scenario["releaseEvidence"], $"Scenario {scenario["scenarioId"]} must not count as release evidence.");
            Assert.AreEqual("app-level-driver-not-implemented", (string?)scenario["reasonCode"]);
        }

        Assert.IsFalse((bool?)report["releaseSummary"]!["satisfiesAc15ReleaseEvidence"], "Infrastructure-only measurements must not satisfy AC15 release evidence.");
        Assert.IsFalse(measurements.Any(measurement => (bool?)measurement["releaseEvidence"] == true && (bool?)measurement["appBoundaryDriven"] != true));
    }

    [TestMethod]
    [TestCategory("Diagnostics")]
    public void Diagnostics_conformance_runner_writes_redacted_local_report_and_export()
    {
        using var scratch = ScratchWorkspace.Create();

        AssertCommandSucceeded(RunScript("run-diagnostics-conformance.ps1", "-ScratchRoot", scratch.Root));

        var reportPath = Path.Combine(scratch.Root, "diagnostics", "diagnostics-conformance-result.json");
        var exportPath = Path.Combine(scratch.Root, "diagnostics", "export", "diagnostics-redacted.jsonl");
        Assert.IsTrue(File.Exists(reportPath), "Diagnostics conformance must write a durable result document.");
        Assert.IsTrue(File.Exists(exportPath), "Diagnostics conformance must write a redacted export artifact.");

        var report = JsonNode.Parse(File.ReadAllText(reportPath))!.AsObject();
        Assert.AreEqual("velofileDiagnosticsConformanceResult", (string?)report["documentType"]);
        Assert.AreEqual("verified", (string?)report["status"]);
        Assert.IsTrue((bool?)report["localOnly"]);
        Assert.IsTrue((bool?)report["exportRedacted"]);
        Assert.IsFalse((bool?)report["prohibitedValuesFound"]);
        Assert.AreEqual(10, (int?)report["retention"]!["maxCrashMarkers"]);

        var workflows = report["workflowCoverage"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[] { "navigation", "preview", "file-operation", "search", "terminal", "session-restore" },
            workflows.Select(value => (string)value["workflow"]!).ToArray());
        foreach (var workflow in workflows)
        {
            Assert.IsTrue((bool?)workflow["covered"], $"Workflow {workflow["workflow"]} must be emitted.");
            Assert.IsTrue((bool?)workflow["serialized"], $"Workflow {workflow["workflow"]} must survive serialization.");
            Assert.IsTrue((bool?)workflow["redacted"], $"Workflow {workflow["workflow"]} must pass redaction.");
            Assert.IsFalse(string.IsNullOrWhiteSpace((string?)workflow["reasonCode"]), $"Workflow {workflow["workflow"]} must preserve a controlled reason code.");
        }

        var decisions = report["triageDecisions"]!.AsArray()
            .Select(value => value!.AsObject())
            .ToDictionary(value => (string)value["caseId"]!, StringComparer.Ordinal);
        Assert.AreEqual("promotion-allowed", (string?)decisions["below-threshold"]["decision"]);
        Assert.AreEqual("promotion-blocked", (string?)decisions["at-crash-threshold"]["decision"]);
        Assert.AreEqual("promotion-blocked", (string?)decisions["above-crash-threshold"]["decision"]);
        Assert.AreEqual("promotion-blocked", (string?)decisions["hang-threshold"]["decision"]);
        Assert.AreEqual("insufficient-evidence", (string?)decisions["missing-data"]["decision"]);
        Assert.AreEqual("promotion-blocked", (string?)decisions["redaction-failure"]["decision"]);
        Assert.AreEqual("promotion-blocked", (string?)decisions["retention-violation"]["decision"]);

        var diagnosticsOutput = string.Join(Environment.NewLine, Directory.GetFiles(Path.Combine(scratch.Root, "diagnostics"), "*", SearchOption.AllDirectories).Select(File.ReadAllText));
        foreach (var prohibited in new[] { "alice", "secret-plan", "clipboard-secret", "preview text", "pwsh -NoProfile", "id_rsa" })
        {
            Assert.IsFalse(diagnosticsOutput.Contains(prohibited, StringComparison.OrdinalIgnoreCase), $"Diagnostics conformance output leaked '{prohibited}'.");
        }
    }

    [TestMethod]
    [TestCategory("Release")]
    public void Preview_triage_policy_documents_blocking_thresholds_and_exception_path()
    {
        var repoRoot = FindRepoRoot();
        var policy = File.ReadAllText(Path.Combine(repoRoot.FullName, "docs", "release", "preview-triage.md"));

        StringAssert.Contains(policy, "Crash threshold");
        StringAssert.Contains(policy, "Hang threshold");
        StringAssert.Contains(policy, "blocks promotion");
        StringAssert.Contains(policy, "explicit exception");
        StringAssert.Contains(policy, "p95");
        StringAssert.Contains(policy, "25%");
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

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start pwsh.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new CommandResult(
            process.ExitCode,
            stdoutTask.GetAwaiter().GetResult(),
            stderrTask.GetAwaiter().GetResult());
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
