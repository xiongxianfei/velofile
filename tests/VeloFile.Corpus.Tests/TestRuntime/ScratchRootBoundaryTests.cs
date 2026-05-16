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
        var forbiddenRepoOutputs = new[]
        {
            Path.Combine(repoRoot.FullName, ".velofile-corpus-root"),
            Path.Combine(repoRoot.FullName, ".velofile-tools"),
            Path.Combine(repoRoot.FullName, "corpora"),
            Path.Combine(repoRoot.FullName, "diagnostics"),
            Path.Combine(repoRoot.FullName, "benchmarks")
        };
        var before = RepoOutputSnapshot.Capture(forbiddenRepoOutputs);

        using var scratch = ScratchWorkspace.Create();

        Assert.AreEqual(0, CorpusToolHarness.RunInProcess("generate", "--profile", "smoke", "--root", scratch.Root).ExitCode);
        Assert.AreEqual(0, CorpusToolHarness.RunInProcess("compat", "--scope", "smoke", "--root", scratch.Root).ExitCode);
        Assert.AreEqual(0, CorpusToolHarness.RunInProcess("preview", "--scope", "contract", "--root", scratch.Root).ExitCode);

        var after = RepoOutputSnapshot.Capture(forbiddenRepoOutputs);
        CollectionAssert.AreEqual(before, after, "Optimized corpus contract tests must not create repo-side generated outputs.");
        Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, "corpora", "smoke", "manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, "corpora", "smoke", "compat", "compat-smoke-result.json")));
        Assert.IsTrue(File.Exists(Path.Combine(scratch.Root, "corpora", "preview", "preview", "preview-contract-result.json")));
    }

    private static class RepoOutputSnapshot
    {
        public static string[] Capture(IEnumerable<string> paths)
        {
            return paths
                .SelectMany(CapturePath)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static IEnumerable<string> CapturePath(string path)
        {
            if (File.Exists(path))
            {
                yield return "file:" + path;
                yield break;
            }

            if (!Directory.Exists(path))
            {
                yield return "missing:" + path;
                yield break;
            }

            yield return "dir:" + path;
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                yield return "child:" + Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, '/');
            }
        }
    }
}

