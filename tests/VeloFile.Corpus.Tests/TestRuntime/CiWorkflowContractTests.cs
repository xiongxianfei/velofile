namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class CiWorkflowContractTests
{
    private const string FastLaneJobId = "ci-fast-required";
    private const string ReleaseEvidenceJobId = "ci-release-evidence";
    private const string CiWorkflowRelativePath = ".github/workflows/ci.yml";
    private const string ReleaseEvidenceWorkflowRelativePath = ".github/workflows/release-evidence.yml";

    [TestMethod]
    public void Workflow_model_parses_committed_ci_workflow()
    {
        var workflow = LoadCiWorkflow();

        CollectionAssert.Contains(workflow.Events.ToArray(), "pull_request");
        CollectionAssert.Contains(workflow.Events.ToArray(), "push");
        CollectionAssert.Contains(workflow.PushBranches.ToArray(), "main");
        Assert.IsTrue(workflow.Permissions.TryGetValue("contents", out var contentsPermission));
        Assert.AreEqual("read", contentsPermission);
        Assert.IsTrue(workflow.Jobs.ContainsKey("ci"));
    }

    [TestMethod]
    public void Fast_required_lane_exists_with_pr_triggers_and_hosted_environment()
    {
        var workflow = LoadCiWorkflow();
        var fastLane = workflow.RequireJob(FastLaneJobId);

        Assert.AreEqual(FastLaneJobId, fastLane.Name);
        CollectionAssert.Contains(workflow.Events.ToArray(), "pull_request");
        CollectionAssert.Contains(workflow.PushBranches.ToArray(), "main");
        CollectionAssert.Contains(fastLane.RunsOn.ToArray(), "windows-latest");
        Assert.AreEqual("pwsh", fastLane.DefaultRunShell);

        var diagnostics = CiWorkflowContractValidator.ValidateFastLane(workflow, FastLaneJobId);
        CollectionAssert.AreEqual(Array.Empty<string>(), diagnostics.ToArray(), string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public void Fast_required_lane_runs_required_commands_in_order()
    {
        var fastLane = LoadCiWorkflow().RequireJob(FastLaneJobId);

        var dotnetInfoIndex = IndexOfRunContaining(fastLane, "dotnet --info");
        var restoreIndex = IndexOfRunContaining(fastLane, "dotnet restore VeloFile.sln");
        var buildIndex = IndexOfRunContaining(fastLane, "dotnet build VeloFile.sln -c Debug --no-restore");
        var firstNoBuildTestIndex = IndexOfRunContaining(fastLane, "--no-build");

        Assert.IsLessThan(restoreIndex, dotnetInfoIndex, "workflow-command-order: dotnet --info must run before restore.");
        Assert.IsLessThan(buildIndex, restoreIndex, "workflow-command-order: restore must run before build.");
        Assert.IsLessThan(firstNoBuildTestIndex, buildIndex, "workflow-command-order: build must run before --no-build tests.");

        AssertCommandContains(fastLane, "dotnet run --project tools\\VeloFile.UiContracts -- validate-tokens");
        AssertCommandContains(fastLane, "--contract docs\\ui\\tokens.v1.json");
        AssertCommandContains(fastLane, "--xaml-root src\\VeloFile.App\\Resources");
        AssertCommandContains(fastLane, "--scopes docs\\ui\\ui-contract-scopes.v1.json");
        AssertCommandContains(fastLane, "--scope-root .");

        AssertProductTestCommand(fastLane, "tests\\VeloFile.Core.Tests\\VeloFile.Core.Tests.csproj");
        AssertProductTestCommand(fastLane, "tests\\VeloFile.App.Tests\\VeloFile.App.Tests.csproj");
        AssertProductTestCommand(fastLane, "tests\\VeloFile.Windows.Tests\\VeloFile.Windows.Tests.csproj");

        AssertDotnetTestCommand(fastLane, "tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj", "TestCategory=Fast|TestCategory=Contract");
        AssertDotnetTestCommand(fastLane, "tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj", "TestCategory=CorpusScript&TestCategory=Smoke");
        AssertAllDotnetTestsWriteTrx(fastLane);

        foreach (var command in fastLane.RunCommands.Select(CiWorkflowContractValidator.Normalize))
        {
            Assert.IsFalse(command.Contains("scripts/ci.ps1", StringComparison.OrdinalIgnoreCase), command);
            Assert.IsFalse(command.Contains("TestCategory=ReleaseEvidence", StringComparison.Ordinal), command);
            Assert.IsFalse(command.Contains("dotnet test VeloFile.sln", StringComparison.Ordinal), command);
        }
    }

    [TestMethod]
    public void Fast_required_lane_reports_fast_confidence_summary_and_artifacts()
    {
        var fastLane = LoadCiWorkflow().RequireJob(FastLaneJobId);
        var runCommands = fastLane.RunCommands.Select(CiWorkflowContractValidator.Normalize).ToArray();
        var summaryCommand = runCommands.Single(command => command.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal));

        StringAssert.Contains(summaryCommand, "\"-LaneName\", \"ci-fast-required\"");
        StringAssert.Contains(summaryCommand, "\"-SelectedCategory\", \"Fast|Contract;CorpusScript&Smoke\"");
        StringAssert.Contains(summaryCommand, "\"-ReleaseEvidenceStatus\", \"not run in this lane\"");
        StringAssert.Contains(summaryCommand, "\"-CorpusScriptSmokeStatus\", \"run\"");
        StringAssert.Contains(summaryCommand, "\"-FullCloseoutStatus\", \"not run\"");
        StringAssert.Contains(summaryCommand, "steps.dotnet_info.outcome");
        StringAssert.Contains(summaryCommand, "-TrxPath");
        Assert.IsTrue(
            summaryCommand.Contains("-TestProjectDuration", StringComparison.Ordinal)
                || summaryCommand.Contains("-TrxPath", StringComparison.Ordinal),
            "workflow-runtime-summary-contract: ci-fast-required must pass either explicit project durations or TRX paths that the summary helper derives project durations from.");

        var summaryStep = fastLane.Steps.Single(step => step.Run?.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal) == true);
        Assert.AreEqual("pwsh", summaryStep.Shell ?? fastLane.DefaultRunShell);

        var uploadStep = fastLane.Steps.Single(step => step.Uses?.StartsWith("actions/upload-artifact@", StringComparison.Ordinal) == true);
        StringAssert.Contains(uploadStep.Name!, "fast PR test results");
    }

    [TestMethod]
    public void Fast_required_lane_inventory_selects_required_corpus_tests()
    {
        var descriptors = CorpusCategoryInventory.FromAssembly(typeof(CiWorkflowContractTests).Assembly);
        var fastOrContract = descriptors
            .Where(test => test.Categories.Contains(CorpusTestCategories.Fast, StringComparer.Ordinal)
                || test.Categories.Contains(CorpusTestCategories.Contract, StringComparer.Ordinal))
            .ToArray();
        var corpusScriptSmoke = descriptors
            .Where(test => test.Categories.Contains(CorpusTestCategories.CorpusScript, StringComparer.Ordinal)
                && test.Categories.Contains(CorpusTestCategories.Smoke, StringComparer.Ordinal))
            .ToArray();

        Assert.IsNotEmpty(fastOrContract, "category-selection-contract: Fast|Contract must select at least one Corpus test.");
        Assert.IsNotEmpty(corpusScriptSmoke, "category-selection-contract: CorpusScript&Smoke must select at least one Corpus public-wrapper smoke test.");
    }

    [TestMethod]
    public void Release_evidence_lane_exists_with_release_readiness_triggers_and_environment()
    {
        var workflow = LoadReleaseEvidenceWorkflow();
        var releaseLane = workflow.RequireJob(ReleaseEvidenceJobId);

        Assert.AreEqual(ReleaseEvidenceJobId, releaseLane.Name);
        CollectionAssert.Contains(workflow.Events.ToArray(), "workflow_dispatch");
        CollectionAssert.Contains(workflow.Events.ToArray(), "schedule");
        CollectionAssert.Contains(workflow.Events.ToArray(), "push");
        CollectionAssert.Contains(workflow.Events.ToArray(), "merge_group");
        CollectionAssert.DoesNotContain(workflow.Events.ToArray(), "pull_request");
        CollectionAssert.Contains(workflow.PushBranches.ToArray(), "release/**");
        CollectionAssert.Contains(workflow.PushTags.ToArray(), "v*");
        CollectionAssert.Contains(workflow.PushTags.ToArray(), "v*-rc*");
        Assert.IsTrue(
            workflow.ScheduleCrons.Any(cron => !cron.StartsWith("0 ", StringComparison.Ordinal)),
            "workflow-trigger-contract: release evidence schedule must use a non-top-of-hour cron.");
        CollectionAssert.Contains(releaseLane.RunsOn.ToArray(), "windows-latest");
        Assert.AreEqual("pwsh", releaseLane.DefaultRunShell);
        Assert.IsTrue(workflow.Permissions.TryGetValue("contents", out var contentsPermission));
        Assert.AreEqual("read", contentsPermission);

        var diagnostics = CiWorkflowContractValidator.ValidateHostedLane(workflow, ReleaseEvidenceJobId);
        CollectionAssert.AreEqual(Array.Empty<string>(), diagnostics.ToArray(), string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public void Release_evidence_lane_runs_release_evidence_validation_and_reports_expensive_categories()
    {
        var releaseLane = LoadReleaseEvidenceWorkflow().RequireJob(ReleaseEvidenceJobId);
        var restoreIndex = IndexOfRunContaining(releaseLane, "dotnet restore VeloFile.sln");
        var buildIndex = IndexOfRunContaining(releaseLane, "dotnet build VeloFile.sln -c Debug --no-restore");
        var releaseEvidenceIndex = IndexOfRunContaining(releaseLane, "TestCategory=ReleaseEvidence");

        Assert.IsLessThan(buildIndex, restoreIndex, "workflow-command-order: release-evidence restore must run before build.");
        Assert.IsLessThan(releaseEvidenceIndex, buildIndex, "workflow-command-order: release-evidence build must run before --no-build ReleaseEvidence tests.");

        AssertDotnetTestCommand(releaseLane, "tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj", "TestCategory=ReleaseEvidence");
        AssertAllDotnetTestsWriteTrx(releaseLane);

        foreach (var command in releaseLane.RunCommands.Select(CiWorkflowContractValidator.Normalize))
        {
            Assert.IsFalse(command.Contains("scripts/ci.ps1", StringComparison.OrdinalIgnoreCase), command);
        }

        var summaryCommand = releaseLane.RunCommands
            .Select(CiWorkflowContractValidator.Normalize)
            .Single(command => command.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal));

        StringAssert.Contains(summaryCommand, "\"-LaneName\", \"ci-release-evidence\"");
        StringAssert.Contains(summaryCommand, "ReleaseEvidence;Benchmark=run;Visual=not selected in this lane;ManualEvidence=absent from current test inventory");
        StringAssert.Contains(summaryCommand, "\"-ReleaseEvidenceStatus\", \"run\"");
        StringAssert.Contains(summaryCommand, "\"-CorpusScriptSmokeStatus\", \"not selected in this lane\"");
        StringAssert.Contains(summaryCommand, "\"-FullCloseoutStatus\", \"not run\"");
        StringAssert.Contains(summaryCommand, "steps.test_release_evidence.outcome");
        StringAssert.Contains(summaryCommand, "-TrxPath");

        var summaryStep = releaseLane.Steps.Single(step => step.Run?.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal) == true);
        Assert.AreEqual("pwsh", summaryStep.Shell ?? releaseLane.DefaultRunShell);

        var uploadStep = releaseLane.Steps.Single(step => step.Uses?.StartsWith("actions/upload-artifact@", StringComparison.Ordinal) == true);
        StringAssert.Contains(uploadStep.Name!, "release evidence test results");
    }

    [TestMethod]
    public void Release_evidence_lane_inventory_matches_expensive_category_summary_statuses()
    {
        var descriptors = CorpusCategoryInventory.FromAssembly(typeof(CiWorkflowContractTests).Assembly);
        var releaseEvidence = descriptors
            .Where(test => test.Categories.Contains(CorpusTestCategories.ReleaseEvidence, StringComparer.Ordinal))
            .ToArray();
        var benchmarkReleaseEvidence = releaseEvidence
            .Where(test => test.Categories.Contains(CorpusTestCategories.Benchmark, StringComparer.Ordinal))
            .ToArray();
        var visualReleaseEvidence = releaseEvidence
            .Where(test => test.Categories.Contains(CorpusTestCategories.Visual, StringComparer.Ordinal))
            .ToArray();
        var manualReleaseEvidence = releaseEvidence
            .Where(test => test.Categories.Contains(CorpusTestCategories.ManualEvidence, StringComparer.Ordinal))
            .ToArray();

        Assert.IsNotEmpty(releaseEvidence, "category-selection-contract: ReleaseEvidence must select at least one Corpus test.");
        Assert.IsNotEmpty(benchmarkReleaseEvidence, "category-selection-contract: Benchmark release evidence must be selected by the ReleaseEvidence lane.");
        Assert.IsEmpty(visualReleaseEvidence, "category-selection-contract: update the release-evidence summary if Visual tests become selected.");
        Assert.IsEmpty(manualReleaseEvidence, "category-selection-contract: update the release-evidence summary if ManualEvidence tests become selected.");
    }

    [TestMethod]
    public void Workflow_contract_diagnostics_name_environment_and_command_violations()
    {
        var workflow = CiWorkflowModel.Parse("""
            name: invalid
            on:
              pull_request:
            jobs:
              ci-fast-required:
                name: ci-fast-required
                runs-on: ubuntu-latest
                defaults:
                  run:
                    shell: powershell
                steps:
                  - name: Test before setup
                    run: dotnet test VeloFile.sln -c Debug
                  - name: Set up .NET SDK
                    uses: actions/setup-dotnet@v4
                  - name: Bad closeout
                    run: ./scripts/ci.ps1
                  - name: Bad solution filter
                    run: dotnet test VeloFile.sln -c Debug --filter "TestCategory=Fast|TestCategory=Contract"
                  - name: Bad release evidence
                    run: dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"
            """);

        var diagnostics = CiWorkflowContractValidator.ValidateFastLane(workflow, FastLaneJobId);
        AssertContainsDiagnostic(diagnostics, "workflow-runner-contract", FastLaneJobId, "ubuntu-latest");
        AssertContainsDiagnostic(diagnostics, "workflow-shell-contract", FastLaneJobId, "Test before setup");
        AssertContainsDiagnostic(diagnostics, "workflow-sdk-contract", FastLaneJobId, "Test before setup");
        AssertContainsDiagnostic(diagnostics, "workflow-command-contract", FastLaneJobId, "scripts/ci.ps1");
        AssertContainsDiagnostic(diagnostics, "workflow-filter-contract", FastLaneJobId);
        AssertContainsDiagnostic(diagnostics, "workflow-release-evidence-contract", FastLaneJobId);
    }

    private static CiWorkflowDocument LoadCiWorkflow()
    {
        return CiWorkflowModel.LoadFile(Path.Combine(TestRepo.FindRoot().FullName, CiWorkflowRelativePath));
    }

    private static CiWorkflowDocument LoadReleaseEvidenceWorkflow()
    {
        return CiWorkflowModel.LoadFile(Path.Combine(TestRepo.FindRoot().FullName, ReleaseEvidenceWorkflowRelativePath));
    }

    private static int IndexOfRunContaining(CiWorkflowJob job, string expected)
    {
        var index = job.Steps.ToList().FindIndex(step => CiWorkflowContractValidator.Normalize(step.Run).Contains(expected, StringComparison.Ordinal));
        Assert.IsGreaterThanOrEqualTo(0, index, $"workflow-command-contract: {job.Id} must contain '{expected}'.");
        return index;
    }

    private static void AssertCommandContains(CiWorkflowJob job, params string[] expectedParts)
    {
        var command = job.RunCommands
            .Select(CiWorkflowContractValidator.Normalize)
            .FirstOrDefault(command => expectedParts.All(part => command.Contains(part, StringComparison.Ordinal)));
        Assert.IsNotNull(command, $"workflow-command-contract: {job.Id} must contain command parts: {string.Join(", ", expectedParts)}.");
    }

    private static void AssertDotnetTestCommand(CiWorkflowJob job, string project, string filter)
    {
        var command = job.RunCommands
            .Select(CiWorkflowContractValidator.Normalize)
            .SingleOrDefault(command => command.StartsWith($"dotnet test {project}", StringComparison.Ordinal)
                && command.Contains(filter, StringComparison.Ordinal));

        Assert.IsNotNull(command, $"workflow-corpus-test-contract: {job.Id} must run {project} with {filter}.");
        StringAssert.Contains(command, "--no-build");
        StringAssert.Contains(command, "--logger");
        StringAssert.Contains(command, "trx;");
    }

    private static void AssertProductTestCommand(CiWorkflowJob job, string project)
    {
        var command = job.RunCommands
            .Select(CiWorkflowContractValidator.Normalize)
            .SingleOrDefault(command => command.StartsWith($"dotnet test {project}", StringComparison.Ordinal));

        Assert.IsNotNull(command, $"workflow-product-test-contract: {job.Id} must run {project} directly.");
        StringAssert.Contains(command, "--no-build");
        Assert.IsFalse(command.Contains("--filter", StringComparison.Ordinal), command);
        Assert.IsFalse(command.Contains("TestCategory=", StringComparison.Ordinal), command);
    }

    private static void AssertAllDotnetTestsWriteTrx(CiWorkflowJob job)
    {
        foreach (var command in job.RunCommands.Select(CiWorkflowContractValidator.Normalize).Where(command => command.StartsWith("dotnet test", StringComparison.Ordinal)))
        {
            StringAssert.Contains(command, "--logger");
            StringAssert.Contains(command, "trx;");
            StringAssert.Contains(command, "--results-directory artifacts\\test-results");
        }
    }

    private static void AssertContainsDiagnostic(IReadOnlyCollection<string> diagnostics, params string[] expectedParts)
    {
        var diagnostic = diagnostics.SingleOrDefault(value => expectedParts.All(part => value.Contains(part, StringComparison.Ordinal)));
        Assert.IsNotNull(diagnostic, $"Expected diagnostic containing: {string.Join(", ", expectedParts)}. Actual:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics)}");
    }
}
