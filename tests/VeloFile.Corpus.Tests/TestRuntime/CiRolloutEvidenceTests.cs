namespace VeloFile.Corpus.Tests.TestRuntime;

[TestClass]
[TestCategory(CorpusTestCategories.Contract)]
public sealed class CiRolloutEvidenceTests
{
    [TestMethod]
    public void Shadow_run_evidence_records_hosted_fast_and_broad_pr_cycle()
    {
        var evidence = ReadChangeFile("shadow-run.md");

        StringAssert.Contains(evidence, "Pull request: https://github.com/xiongxianfei/velofile/pull/4");
        StringAssert.Contains(evidence, "Run: https://github.com/xiongxianfei/velofile/actions/runs/26062568345");
        StringAssert.Contains(evidence, "Commit: `28de2d60faaa7fc2fbf0f3eade53f8467c26ff1a`");
        StringAssert.Contains(evidence, "ci-fast-required");
        StringAssert.Contains(evidence, "7m20s");
        StringAssert.Contains(evidence, "Selected categories: `Fast|Contract`; `CorpusScript&Smoke`");
        StringAssert.Contains(evidence, "ReleaseEvidence: not run in this lane");
        StringAssert.Contains(evidence, "Full closeout: not run");
        StringAssert.Contains(evidence, "Broad check: passed");
        StringAssert.Contains(evidence, "No validation failures on accepted shadow run");
    }

    [TestMethod]
    public void Branch_protection_handoff_records_no_external_required_check_claim()
    {
        var handoff = ReadChangeFile("branch-protection-handoff.md");

        StringAssert.Contains(handoff, "Branch protection status: not configured");
        StringAssert.Contains(handoff, "HTTP 404");
        StringAssert.Contains(handoff, "No maintainer handoff recorded");
        StringAssert.Contains(handoff, "Intended ordinary required check: `ci-fast-required`");
        StringAssert.Contains(handoff, "Do not claim GitHub branch protection has changed");
    }

    [TestMethod]
    public void Rollout_guidance_keeps_release_readiness_and_rollback_explicit()
    {
        var repoRoot = TestRepo.FindRoot().FullName;
        var readme = File.ReadAllText(Path.Combine(repoRoot, "README.md"));
        var contributing = File.ReadAllText(Path.Combine(repoRoot, "CONTRIBUTING.md"));

        foreach (var document in new[] { readme, contributing })
        {
            StringAssert.Contains(document, "ci-fast-required");
            StringAssert.Contains(document, "ci-release-evidence");
            StringAssert.Contains(document, "ci-full-closeout");
            StringAssert.Contains(document, "ReleaseEvidence: not run in this lane");
            StringAssert.Contains(document, "Full closeout");
            StringAssert.Contains(document, "release readiness");
            StringAssert.Contains(document, "rollback");
        }
    }

    private static string ReadChangeFile(string fileName)
    {
        return File.ReadAllText(Path.Combine(
            TestRepo.FindRoot().FullName,
            "docs",
            "changes",
            "2026-05-18-pr-ci-validation-tiering",
            fileName));
    }
}
