namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class ReleaseEvidenceTierTests
{
    private static readonly string[] ExpectedReleaseEvidenceTests =
    [
        nameof(CorpusToolingSmokeTests.Generate_placeholder_profiles_are_deterministic),
        nameof(CorpusToolingSmokeTests.M15_reference_profiles_are_scaled_and_release_scoped),
        nameof(CorpusToolingSmokeTests.Compatibility_and_preview_runners_validate_scope),
        nameof(CorpusToolingSmokeTests.Compatibility_release_scope_consumes_real_scope_results_without_upgrading_missing_evidence),
        nameof(CorpusToolingSmokeTests.PreviewContract_scope_records_contract_behavior_evidence),
        nameof(CorpusToolingSmokeTests.PreviewProviders_scope_records_provider_behavior_evidence),
        nameof(CorpusToolingSmokeTests.Thumbnails_scope_records_thumbnail_behavior_evidence),
        nameof(CorpusToolingSmokeTests.Benchmark_harness_emits_measured_report_environment_and_release_status),
        nameof(CorpusToolingSmokeTests.Diagnostics_conformance_runner_writes_redacted_local_report_and_export),
        nameof(CorpusToolingSmokeTests.Preview_triage_policy_documents_blocking_thresholds_and_exception_path)
    ];

    private static readonly string[] FullMatrixReleaseEvidenceTests =
    [
        nameof(CorpusToolingSmokeTests.Generate_placeholder_profiles_are_deterministic),
        nameof(CorpusToolingSmokeTests.M15_reference_profiles_are_scaled_and_release_scoped),
        nameof(CorpusToolingSmokeTests.Compatibility_and_preview_runners_validate_scope),
        nameof(CorpusToolingSmokeTests.Compatibility_release_scope_consumes_real_scope_results_without_upgrading_missing_evidence),
        nameof(CorpusToolingSmokeTests.PreviewContract_scope_records_contract_behavior_evidence),
        nameof(CorpusToolingSmokeTests.PreviewProviders_scope_records_provider_behavior_evidence),
        nameof(CorpusToolingSmokeTests.Thumbnails_scope_records_thumbnail_behavior_evidence),
        nameof(CorpusToolingSmokeTests.Diagnostics_conformance_runner_writes_redacted_local_report_and_export)
    ];

    [TestMethod]
    public void ReleaseEvidence_command_is_documented_and_expected_tests_are_selectable()
    {
        var repoRoot = TestRepo.FindRoot();
        var readme = File.ReadAllText(Path.Combine(repoRoot.FullName, "README.md"));
        var descriptors = DescriptorsByMethodName();

        StringAssert.Contains(
            readme,
            "dotnet test tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj -c Debug --filter \"TestCategory=ReleaseEvidence\"");

        foreach (var testName in ExpectedReleaseEvidenceTests)
        {
            Assert.IsTrue(descriptors.TryGetValue(testName, out var test), testName);
            CollectionAssert.Contains(test.Categories.ToArray(), CorpusTestCategories.ReleaseEvidence, test.Name);
            Assert.IsFalse(test.Categories.Contains(CorpusTestCategories.Fast), test.Name);
        }
    }

    [TestMethod]
    public void Full_profile_and_scope_matrix_checks_are_release_evidence_not_smoke_only()
    {
        var descriptors = DescriptorsByMethodName();

        foreach (var testName in FullMatrixReleaseEvidenceTests)
        {
            Assert.IsTrue(descriptors.TryGetValue(testName, out var test), testName);
            CollectionAssert.Contains(test.Categories.ToArray(), CorpusTestCategories.ReleaseEvidence, test.Name);
            Assert.IsFalse(test.Categories.Contains(CorpusTestCategories.Smoke), test.Name);
            Assert.IsFalse(test.Categories.Contains(CorpusTestCategories.Fast), test.Name);
        }
    }

    [TestMethod]
    public void Benchmark_evidence_tests_use_benchmark_or_release_evidence_purpose_categories()
    {
        var descriptors = DescriptorsByMethodName();
        var evidenceTestNames = new[]
        {
            nameof(CorpusToolingSmokeTests.M15_reference_profiles_are_scaled_and_release_scoped),
            nameof(CorpusToolingSmokeTests.Benchmark_harness_emits_measured_report_environment_and_release_status)
        };

        foreach (var testName in evidenceTestNames)
        {
            Assert.IsTrue(descriptors.TryGetValue(testName, out var test), testName);
            Assert.IsTrue(
                test.Categories.Contains(CorpusTestCategories.Benchmark)
                    || test.Categories.Contains(CorpusTestCategories.ReleaseEvidence),
                test.Name);
            Assert.IsFalse(test.Categories.Contains(CorpusTestCategories.Fast), test.Name);
        }
    }

    [TestMethod]
    public void Visual_and_manual_evidence_fast_default_members_have_explicit_rationale()
    {
        var descriptors = CorpusCategoryInventory.FromAssembly(typeof(ReleaseEvidenceTierTests).Assembly);
        var evidenceTestsInFastDefaults = descriptors
            .Where(test => test.Categories.Contains(CorpusTestCategories.Visual)
                || test.Categories.Contains(CorpusTestCategories.ManualEvidence))
            .Where(test => test.Categories.Contains(CorpusTestCategories.Fast)
                || test.Categories.Contains(CorpusTestCategories.Contract))
            .ToArray();

        Assert.IsGreaterThan(0, evidenceTestsInFastDefaults.Length, "M5 should keep the visual evidence fast-default rationale rule exercised.");

        foreach (var test in evidenceTestsInFastDefaults)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(test.EvidenceFastPathRationale), test.Name);
        }
    }

    [TestMethod]
    public void Broad_closeout_ci_remains_unsplit_and_unfiltered()
    {
        var repoRoot = TestRepo.FindRoot();
        var ciScript = File.ReadAllText(Path.Combine(repoRoot.FullName, "scripts", "ci.ps1"));
        var workflow = File.ReadAllText(Path.Combine(repoRoot.FullName, ".github", "workflows", "ci.yml"));

        StringAssert.Contains(workflow, "./scripts/ci.ps1");
        StringAssert.Contains(ciScript, "dotnet test VeloFile.sln -c Debug --no-build");
        Assert.IsFalse(ciScript.Contains("--filter", StringComparison.OrdinalIgnoreCase), "Broad closeout CI must not be silently narrowed to a tier filter.");
        Assert.IsFalse(ciScript.Contains("TestCategory=", StringComparison.OrdinalIgnoreCase), "Broad closeout CI must still select the full solution test suite.");
    }

    private static IReadOnlyDictionary<string, CorpusTestCategoryDescriptor> DescriptorsByMethodName()
    {
        return CorpusCategoryInventory.FromAssembly(typeof(ReleaseEvidenceTierTests).Assembly)
            .ToDictionary(test => test.Name.Split('.').Last(), StringComparer.Ordinal);
    }
}
