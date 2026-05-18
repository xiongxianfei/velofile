namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class CiWorkflowContractTests
{
    private const string FastLaneJobId = "ci-fast-required";
    private const string ReleaseEvidenceJobId = "ci-release-evidence";
    private const string CloseoutJobId = "ci-full-closeout";
    private const string CiWorkflowRelativePath = ".github/workflows/ci.yml";
    private const string ReleaseEvidenceWorkflowRelativePath = ".github/workflows/release-evidence.yml";
    private const string CloseoutWorkflowRelativePath = ".github/workflows/closeout.yml";

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

        AssertSummaryUsesNamedSplatting(summaryCommand);
        StringAssert.Contains(summaryCommand, "LaneName = \"ci-fast-required\"");
        StringAssert.Contains(summaryCommand, "SelectedCategory = \"Fast|Contract;CorpusScript&Smoke\"");
        StringAssert.Contains(summaryCommand, "ReleaseEvidenceStatus = \"not run in this lane\"");
        StringAssert.Contains(summaryCommand, "CorpusScriptSmokeStatus = \"run\"");
        StringAssert.Contains(summaryCommand, "FullCloseoutStatus = \"not run\"");
        StringAssert.Contains(summaryCommand, "steps.dotnet_info.outcome");
        StringAssert.Contains(summaryCommand, "TrxPath");
        Assert.IsTrue(
            summaryCommand.Contains("TestProjectDuration", StringComparison.Ordinal)
                || summaryCommand.Contains("TrxPath", StringComparison.Ordinal),
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

        var diagnostics = CiWorkflowContractValidator.ValidateReleaseEvidenceLane(workflow, ReleaseEvidenceJobId);
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

        AssertSummaryUsesNamedSplatting(summaryCommand);
        StringAssert.Contains(summaryCommand, "LaneName = \"ci-release-evidence\"");
        StringAssert.Contains(summaryCommand, "ReleaseEvidence;Benchmark=run;Visual=not selected in this lane;ManualEvidence=absent from current test inventory");
        StringAssert.Contains(summaryCommand, "ReleaseEvidenceStatus = \"run\"");
        StringAssert.Contains(summaryCommand, "CorpusScriptSmokeStatus = \"not selected in this lane\"");
        StringAssert.Contains(summaryCommand, "FullCloseoutStatus = \"not run\"");
        StringAssert.Contains(summaryCommand, "steps.test_release_evidence.outcome");
        StringAssert.Contains(summaryCommand, "TrxPath");

        var releaseEvidenceStep = releaseLane.Steps.Single(step => StringComparer.Ordinal.Equals(step.Id, "test_release_evidence"));
        Assert.IsFalse(
            releaseEvidenceStep.ContinueOnError,
            "workflow-step-violation: test_release_evidence step has ContinueOnError=true; workflow must fail on release-evidence validation failure.");

        var summaryStep = releaseLane.Steps.Single(step => step.Run?.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal) == true);
        Assert.AreEqual("pwsh", summaryStep.Shell ?? releaseLane.DefaultRunShell);
        Assert.AreEqual(
            "always()",
            summaryStep.StepIfCondition,
            "workflow-step-violation: release-evidence summary step does not have if=always(); must run even when validation fails.");

        var uploadStep = releaseLane.Steps.Single(step => step.Uses?.StartsWith("actions/upload-artifact@", StringComparison.Ordinal) == true);
        StringAssert.Contains(uploadStep.Name!, "release evidence test results");
    }

    [TestMethod]
    public void Release_evidence_lane_diagnostics_name_failure_semantics_violations()
    {
        var workflow = CiWorkflowModel.Parse("""
            name: invalid-release-evidence
            on:
              workflow_dispatch:
            jobs:
              ci-release-evidence:
                name: ci-release-evidence
                runs-on: windows-latest
                defaults:
                  run:
                    shell: pwsh
                steps:
                  - name: Set up .NET SDK
                    uses: actions/setup-dotnet@v4
                  - name: Test Corpus release evidence
                    id: test_release_evidence
                    continue-on-error: true
                    run: dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=ReleaseEvidence" --logger "trx;LogFileName=VeloFile.Corpus.ReleaseEvidence.trx" --results-directory artifacts\test-results\release-evidence
                  - name: Write release evidence runtime summary
                    run: ./scripts/Write-CiRuntimeSummary.ps1 -LaneName ci-release-evidence
            """);

        var diagnostics = CiWorkflowContractValidator.ValidateReleaseEvidenceLane(workflow, ReleaseEvidenceJobId);

        AssertContainsDiagnostic(diagnostics, "workflow-step-violation", "test_release_evidence", "ContinueOnError=true");
        AssertContainsDiagnostic(diagnostics, "workflow-step-violation", "release-evidence summary step", "if=always()");
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
    public void Full_closeout_lane_exists_with_manual_trigger_and_hosted_environment()
    {
        var workflow = LoadCloseoutWorkflow();
        var closeoutLane = workflow.RequireJob(CloseoutJobId);

        Assert.AreEqual(CloseoutJobId, closeoutLane.Name);
        CollectionAssert.Contains(workflow.Events.ToArray(), "workflow_dispatch");
        CollectionAssert.DoesNotContain(workflow.Events.ToArray(), "pull_request");
        CollectionAssert.DoesNotContain(workflow.Events.ToArray(), "push");
        CollectionAssert.Contains(closeoutLane.RunsOn.ToArray(), "windows-latest");
        Assert.AreEqual("pwsh", closeoutLane.DefaultRunShell);
        Assert.IsTrue(workflow.Permissions.TryGetValue("contents", out var contentsPermission));
        Assert.AreEqual("read", contentsPermission);

        var diagnostics = CiWorkflowContractValidator.ValidateFullCloseoutLane(workflow, CloseoutJobId);
        CollectionAssert.AreEqual(Array.Empty<string>(), diagnostics.ToArray(), string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public void Full_closeout_lane_invokes_broad_script_and_reports_summary()
    {
        var closeoutLane = LoadCloseoutWorkflow().RequireJob(CloseoutJobId);
        var closeoutStep = closeoutLane.Steps.Single(step => StringComparer.Ordinal.Equals(step.Id, "full_closeout"));
        var closeoutCommand = CiWorkflowContractValidator.Normalize(closeoutStep.Run);

        StringAssert.Contains(closeoutCommand, "./scripts/ci.ps1");
        Assert.IsFalse(
            closeoutStep.ContinueOnError,
            "workflow-step-violation: full_closeout step has ContinueOnError=true; workflow must fail when scripts/ci.ps1 fails.");

        var setupIndex = closeoutLane.Steps.ToList().FindIndex(step => step.Uses?.StartsWith("actions/setup-dotnet@", StringComparison.Ordinal) == true);
        var closeoutIndex = closeoutLane.Steps.ToList().FindIndex(step => StringComparer.Ordinal.Equals(step.Id, "full_closeout"));
        Assert.IsGreaterThanOrEqualTo(0, setupIndex, "workflow-sdk-contract: ci-full-closeout must set up .NET before closeout validation.");
        Assert.IsLessThan(closeoutIndex, setupIndex, "workflow-sdk-contract: ci-full-closeout must set up .NET before scripts/ci.ps1.");

        foreach (var command in closeoutLane.RunCommands.Select(CiWorkflowContractValidator.Normalize))
        {
            Assert.IsFalse(command.StartsWith("dotnet restore", StringComparison.Ordinal), command);
            Assert.IsFalse(command.StartsWith("dotnet build", StringComparison.Ordinal), command);
            Assert.IsFalse(command.StartsWith("dotnet test", StringComparison.Ordinal), command);
            Assert.IsFalse(command.Contains("TestCategory=Fast|TestCategory=Contract", StringComparison.Ordinal), command);
            Assert.IsFalse(command.Contains("TestCategory=CorpusScript&TestCategory=Smoke", StringComparison.Ordinal), command);
            Assert.IsFalse(command.Contains("TestCategory=ReleaseEvidence", StringComparison.Ordinal), command);
        }

        var summaryCommand = closeoutLane.RunCommands
            .Select(CiWorkflowContractValidator.Normalize)
            .Single(command => command.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal));

        AssertSummaryUsesNamedSplatting(summaryCommand);
        StringAssert.Contains(summaryCommand, "LaneName = \"ci-full-closeout\"");
        StringAssert.Contains(summaryCommand, "SelectedCategory = \"FullSolution\"");
        StringAssert.Contains(summaryCommand, "ReleaseEvidenceStatus = \"unknown; full closeout unfiltered\"");
        StringAssert.Contains(summaryCommand, "CorpusScriptSmokeStatus = \"unknown; full closeout unfiltered\"");
        StringAssert.Contains(summaryCommand, "FullCloseoutStatus = \"run\"");
        StringAssert.Contains(summaryCommand, "steps.full_closeout.outcome");
        StringAssert.Contains(summaryCommand, "FailedCommand");
        StringAssert.Contains(summaryCommand, "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1");
        StringAssert.Contains(summaryCommand, "TrxPath");

        var summaryStep = closeoutLane.Steps.Single(step => step.Run?.Contains("./scripts/Write-CiRuntimeSummary.ps1", StringComparison.Ordinal) == true);
        Assert.AreEqual("pwsh", summaryStep.Shell ?? closeoutLane.DefaultRunShell);
        Assert.AreEqual(
            "always()",
            summaryStep.StepIfCondition,
            "workflow-step-violation: full-closeout summary step does not have if=always(); must run even when closeout validation fails.");

        var uploadStep = closeoutLane.Steps.Single(step => step.Uses?.StartsWith("actions/upload-artifact@", StringComparison.Ordinal) == true);
        StringAssert.Contains(uploadStep.Name!, "full closeout test results");
    }

    [TestMethod]
    public void Full_closeout_lane_diagnostics_name_failure_semantics_violations()
    {
        var workflow = CiWorkflowModel.Parse("""
            name: invalid-full-closeout
            on:
              workflow_dispatch:
            jobs:
              ci-full-closeout:
                name: ci-full-closeout
                runs-on: windows-latest
                defaults:
                  run:
                    shell: pwsh
                steps:
                  - name: Set up .NET SDK
                    uses: actions/setup-dotnet@v4
                  - name: Run full closeout script
                    id: full_closeout
                    continue-on-error: true
                    run: ./scripts/ci.ps1
                  - name: Write full closeout runtime summary
                    run: ./scripts/Write-CiRuntimeSummary.ps1 -LaneName ci-full-closeout
            """);

        var diagnostics = CiWorkflowContractValidator.ValidateFullCloseoutLane(workflow, CloseoutJobId);

        AssertContainsDiagnostic(diagnostics, "workflow-step-violation", "full_closeout", "ContinueOnError=true");
        AssertContainsDiagnostic(diagnostics, "workflow-step-violation", "full-closeout summary step", "if=always()");
    }

    [TestMethod]
    public void Scripts_ci_remains_broad_closeout_command()
    {
        var script = CiWorkflowContractValidator.Normalize(File.ReadAllText(Path.Combine(TestRepo.FindRoot().FullName, "scripts", "ci.ps1")));

        StringAssert.Contains(script, "dotnet --info");
        StringAssert.Contains(script, "dotnet restore VeloFile.sln");
        StringAssert.Contains(script, "dotnet build VeloFile.sln -c Debug --no-restore");
        StringAssert.Contains(script, "dotnet run --project tools/VeloFile.UiContracts -- validate-tokens");
        StringAssert.Contains(script, "dotnet test VeloFile.sln -c Debug --no-build");
        Assert.IsFalse(script.Contains("--filter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("TestCategory=", StringComparison.Ordinal), script);
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

    private static CiWorkflowDocument LoadCloseoutWorkflow()
    {
        return CiWorkflowModel.LoadFile(Path.Combine(TestRepo.FindRoot().FullName, CloseoutWorkflowRelativePath));
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

    private static void AssertSummaryUsesNamedSplatting(string command)
    {
        StringAssert.Contains(command, "$summaryArgs = @{");
        StringAssert.Contains(command, "./scripts/Write-CiRuntimeSummary.ps1 @summaryArgs");
        Assert.IsFalse(
            command.Contains("$summaryArgs = @(", StringComparison.Ordinal),
            "workflow-runtime-summary-contract: summary helper calls must use named hashtable splatting; positional array splatting can bind optional parameters incorrectly on hosted pwsh.");
    }
}
