using System.Text.RegularExpressions;
using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class RuntimeReportTests
{
    private const string RuntimeReportRelativePath = "docs/changes/2026-05-16-test-runtime-optimization/runtime/m6-optimized-runtime.md";

    [TestMethod]
    public void Runtime_report_records_baseline_and_optimized_timings()
    {
        var report = ReadRuntimeReport();

        StringAssert.Contains(report, "## Baseline Corpus Runtime");
        StringAssert.Contains(report, "5 m 49 s");
        StringAssert.Contains(report, "## Optimized Runtime Measurements");
        StringAssert.Contains(report, "solution-fast-contract");
        StringAssert.Contains(report, "corpus-contract");
        StringAssert.Contains(report, "corpus-script-smoke");
        StringAssert.Contains(report, "full-ci");
        StringAssert.Contains(report, "## Top 10 Slowest Tests");
        StringAssert.Contains(report, "## Full CI Status");
        Assert.DoesNotContain("pending", report, StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void Runtime_report_metadata_and_privacy_are_controlled()
    {
        var report = ReadRuntimeReport();

        StringAssert.Contains(report, "Date recorded:");
        StringAssert.Contains(report, "Configuration:");
        StringAssert.Contains(report, "Filter:");
        StringAssert.Contains(report, "Local environment assumptions:");
        StringAssert.Contains(report, "not a universal runtime guarantee");
        Assert.DoesNotContain("C:\\Users\\", report);
        Assert.DoesNotContain("xiongxianfei", report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SECRET", report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TOKEN", report, StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void Runtime_report_uses_structured_slow_test_source_or_records_fallback()
    {
        var report = ReadRuntimeReport();

        StringAssert.Contains(report, "Structured slow-test source: TRX");
        StringAssert.Contains(report, "raw TRX not committed");

        var topSlowRows = Regex.Matches(report, @"^\|\s*\d+\s*\|", RegexOptions.Multiline);

        Assert.HasCount(10, topSlowRows, "The M6 report must include exactly the top 10 slow-test rows.");
    }

    [TestMethod]
    public void Runtime_report_records_missed_targets_without_deleting_coverage()
    {
        var report = ReadRuntimeReport();

        StringAssert.Contains(report, "## Runtime Target Outcomes");
        StringAssert.Contains(report, "R56");
        StringAssert.Contains(report, "R57");
        StringAssert.Contains(report, "R58");
        StringAssert.Contains(report, "R59");
        StringAssert.Contains(report, "R60");
        StringAssert.Contains(report, "Coverage preservation:");
        StringAssert.Contains(report, "No coverage was deleted");
    }

    private static string ReadRuntimeReport()
    {
        var reportPath = Path.Combine(TestRepo.FindRoot().FullName, RuntimeReportRelativePath);

        Assert.IsTrue(File.Exists(reportPath), $"Missing runtime report: {RuntimeReportRelativePath}");
        return File.ReadAllText(reportPath);
    }
}
