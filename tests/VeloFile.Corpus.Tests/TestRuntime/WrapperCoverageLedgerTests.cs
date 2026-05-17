using System.Reflection;
using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class WrapperCoverageLedgerTests
{
    [TestMethod]
    public void M2_wrapper_coverage_ledger_preserves_existing_public_wrapper_claims()
    {
        var repoRoot = TestRepo.FindRoot();
        var plan = File.ReadAllText(Path.Combine(repoRoot.FullName, "docs", "plans", "2026-05-16-test-runtime-optimization.md"));

        foreach (var claim in new[]
        {
            "Generate corpus profile entrypoint works",
            "Compatibility runner entrypoint works",
            "Preview runner entrypoint works",
            "Benchmark wrapper entrypoint works",
            "Diagnostics conformance wrapper works",
            "Scratch publish does not write outside scratch root"
        })
        {
            StringAssert.Contains(plan, claim);
        }

        Assert.IsGreaterThanOrEqualTo(
            6,
            CountOccurrences(plan, "preserved in M2"),
            "M2 must preserve existing public wrapper coverage until M3 replacement smoke coverage closes.");
        Assert.IsFalse(
            plan.Contains("removed, replacement planned for M3", StringComparison.OrdinalIgnoreCase),
            "M2 must not remove public wrapper coverage before equivalent replacement coverage exists.");
    }

    [TestMethod]
    public void Existing_public_wrapper_tests_remain_script_categorized_during_M2()
    {
        var descriptors = CorpusCategoryInventory.FromTypes(typeof(CorpusToolingSmokeTests))
            .ToDictionary(test => test.Name, StringComparer.Ordinal);

        AssertScriptTest(descriptors, nameof(CorpusToolingSmokeTests.Generate_corpus_refuses_repository_root), CorpusTestCategories.Smoke);
        AssertScriptTest(descriptors, nameof(CorpusToolingSmokeTests.Generate_placeholder_profiles_are_deterministic), CorpusTestCategories.ReleaseEvidence);
        AssertScriptTest(descriptors, nameof(CorpusToolingSmokeTests.Compatibility_and_preview_runners_validate_scope), CorpusTestCategories.ReleaseEvidence);
        AssertScriptTest(descriptors, nameof(CorpusToolingSmokeTests.Benchmark_harness_emits_measured_report_environment_and_release_status), CorpusTestCategories.ReleaseEvidence);
        AssertScriptTest(descriptors, nameof(CorpusToolingSmokeTests.Diagnostics_conformance_runner_writes_redacted_local_report_and_export), CorpusTestCategories.ReleaseEvidence);

        var runScriptMethod = typeof(CorpusToolingSmokeTests).GetMethod("RunScript", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(runScriptMethod, "M2 must not remove existing public wrapper execution coverage before M3 smoke replacement exists.");
    }

    private static void AssertScriptTest(
        IReadOnlyDictionary<string, CorpusTestCategoryDescriptor> descriptors,
        string methodName,
        string expectedCompanionCategory)
    {
        var test = descriptors.Single(pair => pair.Key.EndsWith("." + methodName, StringComparison.Ordinal)).Value;

        CollectionAssert.Contains(test.Categories.ToArray(), CorpusTestCategories.CorpusScript, test.Name);
        CollectionAssert.Contains(test.Categories.ToArray(), expectedCompanionCategory, test.Name);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
