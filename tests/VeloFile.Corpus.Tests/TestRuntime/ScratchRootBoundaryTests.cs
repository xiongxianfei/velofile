using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class ScratchRootBoundaryTests
{
    [TestMethod]
    public void In_process_contract_outputs_do_not_escape_assigned_scratch_root()
    {
        var repoRoot = TestRepo.FindRoot();
        var before = RepoOutputSnapshot.CaptureGeneratedOutputPaths(repoRoot);

        using var scratch = ScratchWorkspace.Create();

        Assert.AreEqual(0, CorpusToolHarness.RunInProcess("generate", "--profile", "smoke", "--root", scratch.Root).ExitCode);
        Assert.AreEqual(0, CorpusToolHarness.RunInProcess("compat", "--scope", "smoke", "--root", scratch.Root).ExitCode);
        Assert.AreEqual(0, CorpusToolHarness.RunInProcess("preview", "--scope", "contract", "--root", scratch.Root).ExitCode);

        var after = RepoOutputSnapshot.CaptureGeneratedOutputPaths(repoRoot);
        CollectionAssert.AreEqual(before, after, "Optimized corpus contract tests must not create repo-side generated outputs.");
        Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, "corpora", "smoke", "manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, "corpora", "smoke", "compat", "compat-smoke-result.json")));
        Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, "corpora", "preview", "preview", "preview-contract-result.json")));
    }
}
