using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class CategoryInventoryTests
{
    [TestMethod]
    public void Corpus_tests_use_only_accepted_categories_and_required_combinations()
    {
        var descriptors = CorpusCategoryInventory.FromAssembly(typeof(CategoryInventoryTests).Assembly);

        var errors = CorpusCategoryInventory.Validate(descriptors);

        Assert.IsFalse(errors.Any(), string.Join(Environment.NewLine, errors));
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_missing_category()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor("MissingCategoryTest", [], HasReleaseEvidenceFastRationale: false)
        ]);

        AssertContainsSingleError(errors, "missing-category", "MissingCategoryTest");
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_unknown_category()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor("LegacyCategoryTest", ["UiContracts"], HasReleaseEvidenceFastRationale: false)
        ]);

        AssertContainsSingleError(errors, "unknown-category", "UiContracts");
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_release_evidence_fast_without_rationale()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ReleaseFastTest",
                [CorpusTestCategories.ReleaseEvidence, CorpusTestCategories.Fast],
                HasReleaseEvidenceFastRationale: false)
        ]);

        AssertContainsSingleError(errors, "ReleaseEvidence and Fast", "ReleaseFastTest");
    }

    [TestMethod]
    public void Corpus_category_inventory_allows_release_evidence_fast_with_rationale()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ReleaseFastTest",
                [CorpusTestCategories.ReleaseEvidence, CorpusTestCategories.Fast],
                HasReleaseEvidenceFastRationale: true)
        ]);

        Assert.IsFalse(errors.Any(), string.Join(Environment.NewLine, errors));
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_corpus_script_without_smoke_or_release_evidence()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ScriptOnlyTest",
                [CorpusTestCategories.CorpusScript],
                HasReleaseEvidenceFastRationale: false)
        ]);

        AssertContainsSingleError(errors, "CorpusScript without Smoke or ReleaseEvidence", "ScriptOnlyTest");
    }

    private static void AssertContainsSingleError(IReadOnlyList<string> errors, string expectedKind, string expectedSubject)
    {
        Assert.HasCount(1, errors, string.Join(Environment.NewLine, errors));
        StringAssert.Contains(errors[0], expectedKind);
        StringAssert.Contains(errors[0], expectedSubject);
    }
}
