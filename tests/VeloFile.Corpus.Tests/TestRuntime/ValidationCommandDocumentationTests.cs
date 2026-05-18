using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class ValidationCommandDocumentationTests
{
    [TestMethod]
    public void Contributor_validation_guidance_documents_required_tier_commands()
    {
        var repoRoot = FindRepoRoot();
        var readme = File.ReadAllText(Path.Combine(repoRoot.FullName, "README.md"));

        StringAssert.Contains(
            readme,
            "dotnet test VeloFile.sln -c Debug --no-build --filter \"TestCategory=Fast|TestCategory=Contract\"");
        StringAssert.Contains(
            readme,
            "dotnet test tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter \"TestCategory=Contract\"");
        StringAssert.Contains(
            readme,
            "dotnet test tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj -c Debug --filter \"TestCategory=CorpusScript&TestCategory=Smoke\"");
        StringAssert.Contains(
            readme,
            "dotnet test tests\\VeloFile.Corpus.Tests\\VeloFile.Corpus.Tests.csproj -c Debug --filter \"TestCategory=ReleaseEvidence\"");
        StringAssert.Contains(
            readme,
            "powershell -NoProfile -ExecutionPolicy Bypass -File scripts\\ci.ps1");
        StringAssert.Contains(
            readme,
            "`--no-build` commands assume the relevant projects have already been built.");
    }

    [TestMethod]
    public void Contributor_validation_guidance_distinguishes_hosted_ci_lanes()
    {
        var repoRoot = FindRepoRoot();
        var readme = File.ReadAllText(Path.Combine(repoRoot.FullName, "README.md"));
        var contributing = File.ReadAllText(Path.Combine(repoRoot.FullName, "CONTRIBUTING.md"));

        foreach (var document in new[] { readme, contributing })
        {
            StringAssert.Contains(document, "ci-fast-required");
            StringAssert.Contains(document, "ci-release-evidence");
            StringAssert.Contains(document, "ci-full-closeout");
            StringAssert.Contains(document, "ReleaseEvidence: not run in this lane");
            StringAssert.Contains(document, "CorpusScript Smoke: run");
            StringAssert.Contains(document, "Full closeout");
            StringAssert.Contains(document, "release readiness");
        }
    }

    [TestMethod]
    public void Fast_filter_excludes_expensive_only_corpus_tests()
    {
        var descriptors = CorpusCategoryInventory.FromAssembly(typeof(ValidationCommandDocumentationTests).Assembly);
        var expensiveOnlyTests = descriptors
            .Where(test => test.Categories.Count > 0)
            .Where(test => test.Categories.All(CorpusTestCategories.FastExcludedWhenOnlyCategoryPurpose.Contains))
            .ToArray();

        Assert.IsGreaterThan(0, expensiveOnlyTests.Length, "M1 should leave some expensive Corpus tests outside the fast loop.");

        foreach (var test in expensiveOnlyTests)
        {
            Assert.IsFalse(test.Categories.Contains(CorpusTestCategories.Fast), test.Name);
            Assert.IsFalse(test.Categories.Contains(CorpusTestCategories.Contract), test.Name);
        }
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
}
