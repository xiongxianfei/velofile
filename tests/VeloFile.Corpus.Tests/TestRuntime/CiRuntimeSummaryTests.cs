using System.Diagnostics;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class CiRuntimeSummaryTests
{
    private const string SummaryScriptRelativePath = "scripts/Write-CiRuntimeSummary.ps1";

    [TestMethod]
    public void Runtime_summary_records_lane_tiers_durations_and_slowest_trx_tests()
    {
        using var workspace = TempWorkspace.Create();
        var summaryPath = Path.Combine(workspace.Root, "summary.md");
        var trxPath = Path.Combine(workspace.Root, "ci-fast.trx");
        File.WriteAllText(trxPath, SampleTrx());

        var result = RunSummary(
            "-LaneName", "ci-fast-required",
            "-Trigger", "pull_request",
            "-SelectedCategory", "Fast|Contract;CorpusScript&Smoke",
            "-ReleaseEvidenceStatus", "not run in this lane",
            "-CorpusScriptSmokeStatus", "run",
            "-FullCloseoutStatus", "not run",
            "-TotalDurationSeconds", "75.5",
            "-BuildDurationSeconds", "12.25",
            "-TestProjectDuration", "VeloFile.Core.Tests=00:00:01.200;VeloFile.Corpus.Tests=00:00:06.500",
            "-TrxPath", trxPath,
            "-SummaryPath", summaryPath);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        var summary = File.ReadAllText(summaryPath);

        StringAssert.Contains(summary, "# CI Runtime Summary");
        StringAssert.Contains(summary, "Lane: ci-fast-required");
        StringAssert.Contains(summary, "Trigger: pull_request");
        StringAssert.Contains(summary, "Selected categories: Fast|Contract, CorpusScript&Smoke");
        StringAssert.Contains(summary, "ReleaseEvidence: not run in this lane");
        StringAssert.Contains(summary, "CorpusScript Smoke: run");
        StringAssert.Contains(summary, "Full closeout: not run");
        StringAssert.Contains(summary, "Total job duration: 00:01:15.500");
        StringAssert.Contains(summary, "Build duration: 00:00:12.250");
        StringAssert.Contains(summary, "VeloFile.Core.Tests");
        StringAssert.Contains(summary, "VeloFile.Corpus.Tests");
        StringAssert.Contains(summary, "SlowContractTest");
        StringAssert.Contains(summary, "00:00:05.100");
        StringAssert.Contains(summary, "FastSmokeTest");
    }

    [TestMethod]
    public void Runtime_summary_reports_missing_structured_output_without_fabricating_slow_tests()
    {
        using var workspace = TempWorkspace.Create();
        var summaryPath = Path.Combine(workspace.Root, "summary.md");
        var missingTrxPath = Path.Combine(workspace.Root, "missing.trx");

        var result = RunSummary(
            "-LaneName", "ci",
            "-Trigger", "pull_request",
            "-SelectedCategory", "FullSolution",
            "-ReleaseEvidenceStatus", "unknown; broad closeout unfiltered",
            "-CorpusScriptSmokeStatus", "unknown; broad closeout unfiltered",
            "-FullCloseoutStatus", "run",
            "-FailedCommand", "dotnet build VeloFile.sln -c Debug --no-restore",
            "-FailedOutcome", "failure",
            "-TrxPath", missingTrxPath,
            "-SummaryPath", summaryPath);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        var summary = File.ReadAllText(summaryPath);

        StringAssert.Contains(summary, "Failed command: dotnet build VeloFile.sln -c Debug --no-restore");
        StringAssert.Contains(summary, "Command outcome: failure");
        StringAssert.Contains(summary, "Structured test output: unavailable");
        StringAssert.Contains(summary, "missing.trx");
        StringAssert.Contains(summary, "No slow-test details available.");
        Assert.DoesNotContain("| 1 |", summary, "Missing TRX must not produce fabricated slow-test rows.");
    }

    [TestMethod]
    public void Runtime_summary_derives_test_project_durations_from_trx_when_explicit_durations_are_absent()
    {
        using var workspace = TempWorkspace.Create();
        var summaryPath = Path.Combine(workspace.Root, "summary.md");
        var trxPath = Path.Combine(workspace.Root, "VeloFile.Core.Tests.trx");
        File.WriteAllText(trxPath, SampleTrx("VeloFile.Core.Tests.dll"));

        var result = RunSummary(
            "-LaneName", "ci-fast-required",
            "-Trigger", "pull_request",
            "-SelectedCategory", "Fast|Contract;CorpusScript&Smoke",
            "-ReleaseEvidenceStatus", "not run in this lane",
            "-CorpusScriptSmokeStatus", "run",
            "-FullCloseoutStatus", "not run",
            "-TrxPath", trxPath,
            "-SummaryPath", summaryPath);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        var summary = File.ReadAllText(summaryPath);

        StringAssert.Contains(summary, "| Project | Duration |");
        StringAssert.Contains(summary, "| VeloFile.Core.Tests | 00:00:05.350 |");
        Assert.DoesNotContain("No per test project duration data available.", summary, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Runtime_summary_records_release_evidence_category_statuses()
    {
        using var workspace = TempWorkspace.Create();
        var summaryPath = Path.Combine(workspace.Root, "summary.md");

        var result = RunSummary(
            "-LaneName", "ci-release-evidence",
            "-Trigger", "schedule",
            "-SelectedCategory", "ReleaseEvidence;Benchmark=run;Visual=not selected in this lane;ManualEvidence=absent from current test inventory",
            "-ReleaseEvidenceStatus", "run",
            "-CorpusScriptSmokeStatus", "not selected in this lane",
            "-FullCloseoutStatus", "not run",
            "-SummaryPath", summaryPath);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        var summary = File.ReadAllText(summaryPath);

        StringAssert.Contains(summary, "Lane: ci-release-evidence");
        StringAssert.Contains(summary, "Selected categories: ReleaseEvidence, Benchmark=run, Visual=not selected in this lane, ManualEvidence=absent from current test inventory");
        StringAssert.Contains(summary, "ReleaseEvidence: run");
        StringAssert.Contains(summary, "CorpusScript Smoke: not selected in this lane");
        StringAssert.Contains(summary, "Full closeout: not run");
    }

    [TestMethod]
    public void Runtime_summary_redacts_secrets_tokens_credentials_and_private_profile_paths()
    {
        using var workspace = TempWorkspace.Create();
        var summaryPath = Path.Combine(workspace.Root, "summary.md");

        var result = RunSummary(
            "-LaneName", "ci-fast-required",
            "-Trigger", "pull_request",
            "-SelectedCategory", "TOKEN=abc123",
            "-ReleaseEvidenceStatus", "secret=release-key",
            "-CorpusScriptSmokeStatus", "credential=script-key",
            "-FullCloseoutStatus", "signing material=certificate",
            "-FailedCommand", @"dotnet test C:\Users\private-user\repo --password=letmein",
            "-FailedOutcome", "token=abc123",
            "-SummaryPath", summaryPath);

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        var summary = File.ReadAllText(summaryPath);

        Assert.DoesNotContain("abc123", summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("release-key", summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("script-key", summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("certificate", summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("letmein", summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\Users\", summary, StringComparison.OrdinalIgnoreCase);
        StringAssert.Contains(summary, "[redacted-sensitive]");
        StringAssert.Contains(summary, "[redacted-user-profile]");
    }

    [TestMethod]
    public void Full_closeout_workflow_writes_runtime_summary_after_closeout_step()
    {
        var workflowPath = Path.Combine(TestRepo.FindRoot().FullName, ".github", "workflows", "closeout.yml");
        var workflow = File.ReadAllText(workflowPath);

        StringAssert.Contains(workflow, "permissions:");
        StringAssert.Contains(workflow, "contents: read");
        StringAssert.Contains(workflow, "Run full closeout script");
        StringAssert.Contains(workflow, "id: full_closeout");
        StringAssert.Contains(workflow, "run: ./scripts/ci.ps1");
        StringAssert.Contains(workflow, "Write full closeout runtime summary");
        StringAssert.Contains(workflow, "if: always()");
        StringAssert.Contains(workflow, "./scripts/Write-CiRuntimeSummary.ps1");
        StringAssert.Contains(workflow, "steps.full_closeout.outcome");
        StringAssert.Contains(workflow, "$closeoutOutcome -ne \"success\"");
        StringAssert.Contains(workflow, "FailedCommand");
        StringAssert.Contains(workflow, "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1");
        StringAssert.Contains(workflow, "$summaryArgs = @{");
        StringAssert.Contains(workflow, "FullCloseoutStatus = \"run\"");
        Assert.DoesNotContain("$summaryArgs = @(", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("continue-on-error", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secrets.", workflow, StringComparison.OrdinalIgnoreCase);
    }

    private static SummaryResult RunSummary(params string[] arguments)
    {
        var scriptPath = Path.Combine(TestRepo.FindRoot().FullName, SummaryScriptRelativePath);
        var startInfo = new ProcessStartInfo("pwsh")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true
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
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new SummaryResult(process.ExitCode, standardOutput, standardError);
    }

    private static string SampleTrx(string testAssemblyName = "VeloFile.Corpus.Tests.dll")
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <TestRun id="sample" name="sample" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
              <TestDefinitions>
                <UnitTest name="FastSmokeTest" id="fast-smoke-test">
                  <TestMethod codeBase="D:\a\VeloFile\__TEST_ASSEMBLY__" className="VeloFile.Corpus.Tests.FastSmokeTest" name="FastSmokeTest" />
                </UnitTest>
              </TestDefinitions>
              <Results>
                <UnitTestResult testName="FastSmokeTest" outcome="Passed" duration="00:00:00.250" />
                <UnitTestResult testName="SlowContractTest" outcome="Passed" duration="00:00:05.100" />
              </Results>
            </TestRun>
            """.Replace("__TEST_ASSEMBLY__", testAssemblyName, StringComparison.Ordinal);
    }

    private sealed record SummaryResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + Environment.NewLine + StandardError;
    }

    private sealed class TempWorkspace : IDisposable
    {
        private TempWorkspace(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempWorkspace Create()
        {
            return new TempWorkspace(Path.Combine(Path.GetTempPath(), "VeloFile-CiRuntimeSummary-" + Guid.NewGuid().ToString("N")));
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
