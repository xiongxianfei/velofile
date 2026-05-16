namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class PreparedToolHarnessTests
{
    [TestMethod]
    public void PreparedTool_current_run_executes_minimal_command()
    {
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var runRoot = Path.Combine(scratch.Root, "velofile-corpus-run");

        var prepared = PreparedCorpusToolHarness.Prepare(context);
        var result = PreparedCorpusToolHarness.Run(context, prepared.Root, "generate", "--profile", "smoke", "--root", runRoot);
        var compatResult = PreparedCorpusToolHarness.Run(context, prepared.Root, "compat", "--scope", "smoke", "--root", runRoot);

        Assert.IsTrue(prepared.Root.StartsWith(scratch.Root, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(File.Exists(Path.Combine(prepared.Root, PreparedCorpusToolHarness.ManifestFileName)));
        Assert.IsTrue(result.ProcessStarted, result.Diagnostic);
        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        Assert.IsTrue(compatResult.ProcessStarted, compatResult.Diagnostic);
        Assert.AreEqual(0, compatResult.ExitCode, compatResult.AllOutput);
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "corpora", "smoke", "manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "corpora", "smoke", "compat", "compat-smoke-result.json")));
    }

    [TestMethod]
    public void PreparedTool_missing_root_fails_before_invocation()
    {
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var missingRoot = Path.Combine(scratch.Root, "missing-prepared-tool");

        var result = PreparedCorpusToolHarness.Run(context, missingRoot, "generate", "--profile", "smoke", "--root", scratch.Root);

        AssertRejectedBeforeInvocation(result, "prepared-tool-root-missing");
    }

    [TestMethod]
    public void PreparedTool_outside_allowed_root_fails_before_invocation()
    {
        using var scratch = ScratchWorkspace.Create();
        using var outside = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        Directory.CreateDirectory(outside.Root);

        var result = PreparedCorpusToolHarness.Run(context, outside.Root, "generate", "--profile", "smoke", "--root", scratch.Root);

        AssertRejectedBeforeInvocation(result, "prepared-tool-outside-root");
    }

    [TestMethod]
    public void PreparedTool_missing_manifest_fails_before_invocation()
    {
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var preparedRoot = Path.Combine(scratch.Root, "prepared-tool");
        Directory.CreateDirectory(preparedRoot);

        var result = PreparedCorpusToolHarness.Run(context, preparedRoot, "generate", "--profile", "smoke", "--root", scratch.Root);

        AssertRejectedBeforeInvocation(result, "prepared-tool-manifest-missing");
    }

    [TestMethod]
    public void PreparedTool_previous_setup_manifest_fails_as_stale()
    {
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var preparedRoot = CreateManifestOnlyPreparedRoot(context, setupId: "previous-setup");

        var result = PreparedCorpusToolHarness.Run(context, preparedRoot, "generate", "--profile", "smoke", "--root", scratch.Root);

        AssertRejectedBeforeInvocation(result, "prepared-tool-setup-mismatch");
    }

    [TestMethod]
    public void PreparedTool_wrong_metadata_fails_before_invocation()
    {
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var cases = new[]
        {
            ("toolKind", new PreparedCorpusToolManifest(1, "Wrong.Tool", context.SetupId, context.Configuration, context.TargetFramework, PreparedCorpusToolHarness.Entrypoint, "2026-05-16T00:00:00Z")),
            ("configuration", new PreparedCorpusToolManifest(1, PreparedCorpusToolHarness.ToolKind, context.SetupId, "Release", context.TargetFramework, PreparedCorpusToolHarness.Entrypoint, "2026-05-16T00:00:00Z")),
            ("targetFramework", new PreparedCorpusToolManifest(1, PreparedCorpusToolHarness.ToolKind, context.SetupId, context.Configuration, "net8.0", PreparedCorpusToolHarness.Entrypoint, "2026-05-16T00:00:00Z")),
            ("entrypoint", new PreparedCorpusToolManifest(1, PreparedCorpusToolHarness.ToolKind, context.SetupId, context.Configuration, context.TargetFramework, "Other.Tool.dll", "2026-05-16T00:00:00Z"))
        };

        foreach (var (field, manifest) in cases)
        {
            var preparedRoot = Path.Combine(scratch.Root, "prepared-tool-" + field);
            Directory.CreateDirectory(preparedRoot);
            PreparedCorpusToolHarness.WriteManifest(preparedRoot, manifest);
            File.WriteAllText(Path.Combine(preparedRoot, PreparedCorpusToolHarness.Entrypoint), string.Empty);

            var result = PreparedCorpusToolHarness.Run(context, preparedRoot, "generate", "--profile", "smoke", "--root", scratch.Root);

            AssertRejectedBeforeInvocation(result, "prepared-tool-metadata-invalid");
            StringAssert.Contains(result.Diagnostic, field);
        }
    }

    [TestMethod]
    public void PreparedTool_missing_artifact_fails_before_invocation()
    {
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var preparedRoot = CreateManifestOnlyPreparedRoot(context, setupId: context.SetupId);

        var result = PreparedCorpusToolHarness.Run(context, preparedRoot, "generate", "--profile", "smoke", "--root", scratch.Root);

        AssertRejectedBeforeInvocation(result, "prepared-tool-artifact-missing");
    }

    [TestMethod]
    public void PreparedTool_execution_does_not_mutate_global_state_or_repo_outputs()
    {
        var repoRoot = TestRepo.FindRoot();
        using var scratch = ScratchWorkspace.Create();
        var context = PreparedCorpusToolContext.Create(scratch.Root);
        var runRoot = Path.Combine(scratch.Root, "velofile-corpus-run");
        var prepared = PreparedCorpusToolHarness.Prepare(context);
        var beforeRepoOutputs = RepoOutputSnapshot.CaptureGeneratedOutputPaths(repoRoot);
        var beforeUserPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
        var beforeDotnetTools = Environment.GetEnvironmentVariable("DOTNET_ADD_GLOBAL_TOOLS_TO_PATH", EnvironmentVariableTarget.User);
        var beforeDotnetCliHome = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME", EnvironmentVariableTarget.User);

        var result = PreparedCorpusToolHarness.Run(context, prepared.Root, "generate", "--profile", "smoke", "--root", runRoot);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        CollectionAssert.AreEqual(beforeRepoOutputs, RepoOutputSnapshot.CaptureGeneratedOutputPaths(repoRoot));
        Assert.AreEqual(beforeUserPath, Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User));
        Assert.AreEqual(beforeDotnetTools, Environment.GetEnvironmentVariable("DOTNET_ADD_GLOBAL_TOOLS_TO_PATH", EnvironmentVariableTarget.User));
        Assert.AreEqual(beforeDotnetCliHome, Environment.GetEnvironmentVariable("DOTNET_CLI_HOME", EnvironmentVariableTarget.User));
    }

    [TestMethod]
    public void Public_scripts_do_not_expose_prepared_tool_options()
    {
        var repoRoot = TestRepo.FindRoot();
        var scripts = Directory.EnumerateFiles(Path.Combine(repoRoot.FullName, "scripts"), "*.ps1");

        foreach (var script in scripts)
        {
            var content = File.ReadAllText(script);
            Assert.IsFalse(content.Contains("PreparedToolPath", StringComparison.Ordinal), script);
            Assert.IsFalse(content.Contains("UseExistingToolBuild", StringComparison.Ordinal), script);
        }
    }

    private static string CreateManifestOnlyPreparedRoot(PreparedCorpusToolContext context, string setupId)
    {
        var preparedRoot = Path.Combine(context.AllowedRoot, "manifest-only-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(preparedRoot);
        PreparedCorpusToolHarness.WriteManifest(
            preparedRoot,
            new PreparedCorpusToolManifest(
                1,
                PreparedCorpusToolHarness.ToolKind,
                setupId,
                context.Configuration,
                context.TargetFramework,
                PreparedCorpusToolHarness.Entrypoint,
                "2026-05-16T00:00:00Z"));
        return preparedRoot;
    }

    private static void AssertRejectedBeforeInvocation(PreparedCorpusToolRunResult result, string expectedDiagnostic)
    {
        Assert.IsFalse(result.ProcessStarted, result.AllOutput);
        Assert.IsNull(result.ExitCode);
        StringAssert.Contains(result.Diagnostic, expectedDiagnostic);

        var repoRoot = TestRepo.FindRoot().FullName;
        Assert.IsFalse(result.Diagnostic.Contains(repoRoot, StringComparison.OrdinalIgnoreCase), result.Diagnostic);

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            Assert.IsFalse(result.Diagnostic.Contains(userProfile, StringComparison.OrdinalIgnoreCase), result.Diagnostic);
        }
    }
}
