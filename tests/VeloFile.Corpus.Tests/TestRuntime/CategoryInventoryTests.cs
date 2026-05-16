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
            new CorpusTestCategoryDescriptor("MissingCategoryTest", [], ReleaseEvidenceFastRationale: null)
        ]);

        AssertContainsSingleError(errors, "missing-category", "MissingCategoryTest");
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_unknown_category()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor("LegacyCategoryTest", ["UiContracts"], ReleaseEvidenceFastRationale: null)
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
                ReleaseEvidenceFastRationale: null)
        ]);

        AssertContainsSingleError(errors, "non-empty ReleaseEvidenceFastRationale", "ReleaseFastTest");
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_release_evidence_fast_with_empty_rationale()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ReleaseFastTest",
                [CorpusTestCategories.ReleaseEvidence, CorpusTestCategories.Fast],
                ReleaseEvidenceFastRationale: string.Empty)
        ]);

        AssertContainsSingleError(errors, "non-empty ReleaseEvidenceFastRationale", "ReleaseFastTest");
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_release_evidence_fast_with_whitespace_rationale()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ReleaseFastTest",
                [CorpusTestCategories.ReleaseEvidence, CorpusTestCategories.Fast],
                ReleaseEvidenceFastRationale: "   ")
        ]);

        AssertContainsSingleError(errors, "non-empty ReleaseEvidenceFastRationale", "ReleaseFastTest");
    }

    [TestMethod]
    public void Corpus_category_inventory_allows_release_evidence_fast_with_non_empty_rationale()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ReleaseFastTest",
                [CorpusTestCategories.ReleaseEvidence, CorpusTestCategories.Fast],
                ReleaseEvidenceFastRationale: "Static schema classification only; no corpus generation or script execution.")
        ]);

        Assert.IsFalse(errors.Any(), string.Join(Environment.NewLine, errors));
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_class_level_whitespace_rationale()
    {
        var descriptors = CorpusCategoryInventory.FromTypes(typeof(ClassLevelWhitespaceRationaleFixture));

        var errors = CorpusCategoryInventory.Validate(descriptors);

        AssertContainsSingleError(errors, "non-empty ReleaseEvidenceFastRationale", typeof(ClassLevelWhitespaceRationaleFixture).FullName!);
    }

    [TestMethod]
    public void Corpus_category_inventory_allows_method_level_non_empty_rationale()
    {
        var descriptors = CorpusCategoryInventory.FromTypes(typeof(MethodLevelRationaleFixture));

        var errors = CorpusCategoryInventory.Validate(descriptors);

        Assert.IsFalse(errors.Any(), string.Join(Environment.NewLine, errors));
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_method_level_empty_override_even_when_class_rationale_exists()
    {
        var descriptors = CorpusCategoryInventory.FromTypes(typeof(MethodOverrideWhitespaceRationaleFixture));

        var errors = CorpusCategoryInventory.Validate(descriptors);

        AssertContainsSingleError(errors, "non-empty ReleaseEvidenceFastRationale", nameof(MethodOverrideWhitespaceRationaleFixture.WhitespaceOverride));
    }

    [TestMethod]
    public void Corpus_category_inventory_rejects_corpus_script_without_smoke_or_release_evidence()
    {
        var errors = CorpusCategoryInventory.Validate(
        [
            new CorpusTestCategoryDescriptor(
                "ScriptOnlyTest",
                [CorpusTestCategories.CorpusScript],
                ReleaseEvidenceFastRationale: null)
        ]);

        AssertContainsSingleError(errors, "CorpusScript without Smoke or ReleaseEvidence", "ScriptOnlyTest");
    }

    private static void AssertContainsSingleError(IReadOnlyList<string> errors, string expectedKind, string expectedSubject)
    {
        Assert.HasCount(1, errors, string.Join(Environment.NewLine, errors));
        StringAssert.Contains(errors[0], expectedKind);
        StringAssert.Contains(errors[0], expectedSubject);
    }

#pragma warning disable MSTEST0003, MSTEST0030

    [TestCategory(CorpusTestCategories.ReleaseEvidence)]
    [TestCategory(CorpusTestCategories.Fast)]
    [ReleaseEvidenceFastRationale("   ")]
    private sealed class ClassLevelWhitespaceRationaleFixture
    {
        [TestMethod]
        public void UsesClassLevelRationale()
        {
        }
    }

    [TestCategory(CorpusTestCategories.ReleaseEvidence)]
    [TestCategory(CorpusTestCategories.Fast)]
    private sealed class MethodLevelRationaleFixture
    {
        [TestMethod]
        [ReleaseEvidenceFastRationale("Static schema classification only; no corpus generation or script execution.")]
        public void UsesMethodLevelRationale()
        {
        }
    }

    [TestCategory(CorpusTestCategories.ReleaseEvidence)]
    [TestCategory(CorpusTestCategories.Fast)]
    [ReleaseEvidenceFastRationale("Class-level rationale should not hide an empty method override.")]
    private sealed class MethodOverrideWhitespaceRationaleFixture
    {
        [TestMethod]
        [ReleaseEvidenceFastRationale(" ")]
        public void WhitespaceOverride()
        {
        }
    }

#pragma warning restore MSTEST0003, MSTEST0030
}
