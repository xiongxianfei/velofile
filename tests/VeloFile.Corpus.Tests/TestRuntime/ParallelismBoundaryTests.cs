using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class ParallelismBoundaryTests
{
    [TestMethod]
    public void Assembly_wide_serialization_remains_and_shared_state_inventory_is_recorded()
    {
        var repoRoot = FindRepoRoot();
        var mstestSettings = File.ReadAllText(Path.Combine(repoRoot.FullName, "tests", "VeloFile.Corpus.Tests", "MSTestSettings.cs"));
        var inventoryPath = Path.Combine(
            repoRoot.FullName,
            "docs",
            "changes",
            "2026-05-16-test-runtime-optimization",
            "shared-state-inventory.md");

        StringAssert.Contains(mstestSettings, "[assembly: DoNotParallelize]");
        Assert.IsTrue(File.Exists(inventoryPath), "M1 must record shared-state constraints before any future parallelization slice.");

        var inventory = File.ReadAllText(inventoryPath);
        StringAssert.Contains(inventory, "CorpusToolingSmokeTests");
        StringAssert.Contains(inventory, "PowerShell");
        StringAssert.Contains(inventory, "scratch");
        StringAssert.Contains(inventory, "environment");
        StringAssert.Contains(inventory, "DoNotParallelize");
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
