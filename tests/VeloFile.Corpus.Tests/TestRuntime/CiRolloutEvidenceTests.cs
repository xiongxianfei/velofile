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
    public void Post_merge_handoff_records_ruleset_required_check()
    {
        var handoff = ReadChangeFile(
            "2026-05-19-pr-ci-post-merge-handoff",
            "branch-protection-handoff.md");

        StringAssert.Contains(handoff, "Date recorded: 2026-05-19");
        StringAssert.Contains(handoff, "Protection mechanism: repository ruleset `protect`");
        StringAssert.Contains(handoff, "Ruleset enforcement: `active`");
        StringAssert.Contains(handoff, "Required status check: `ci-fast-required`");
        StringAssert.Contains(handoff, "required_status_checks");
        StringAssert.Contains(handoff, "Classic branch-protection result: GitHub returned `Branch not protected` (HTTP 404).");
        StringAssert.Contains(handoff, "Maintainer handoff recorded: ruleset now requires `ci-fast-required`");
        StringAssert.Contains(handoff, "Do not claim classic GitHub branch protection is configured");
        StringAssert.Contains(handoff, "M2 is unblocked");
        StringAssert.Contains(handoff, "broad closeout");
    }

    [TestMethod]
    public void Post_merge_hosted_confirmation_records_fast_only_pr_cycle()
    {
        var evidence = ReadChangeFile(
            "2026-05-19-pr-ci-post-merge-handoff",
            "shadow-run.md");

        StringAssert.Contains(evidence, "Pull request: https://github.com/xiongxianfei/velofile/pull/5");
        StringAssert.Contains(evidence, "Run: https://github.com/xiongxianfei/velofile/actions/runs/26086191007");
        StringAssert.Contains(evidence, "Commit: `b29fd249df61c370dcd069edde664a4c7281cec6`");
        StringAssert.Contains(evidence, "`ci-fast-required` | passed | 5m22s");
        StringAssert.Contains(evidence, "No broad `ci` job appeared in the accepted hosted PR run.");
        StringAssert.Contains(evidence, "Selected categories: `Fast|Contract`; `CorpusScript&Smoke`");
        StringAssert.Contains(evidence, "ReleaseEvidence: not run in this lane");
        StringAssert.Contains(evidence, "Full closeout: not run");
        StringAssert.Contains(evidence, "Broad closeout: not run on ordinary PR");
        StringAssert.Contains(evidence, "Ruleset required check: `ci-fast-required`");
        StringAssert.Contains(evidence, "Do not claim classic GitHub branch protection is configured");
    }

    [TestMethod]
    public void Rollout_guidance_keeps_release_readiness_and_rollback_explicit()
    {
        var repoRoot = TestRepo.FindRoot().FullName;
        var readme = File.ReadAllText(Path.Combine(repoRoot, "README.md"));
        var contributing = File.ReadAllText(Path.Combine(repoRoot, "CONTRIBUTING.md"));
        var projectMap = File.ReadAllText(Path.Combine(repoRoot, "docs", "project-map.md"));

        foreach (var document in new[] { readme, contributing, projectMap })
        {
            StringAssert.Contains(document, "ci-fast-required");
            StringAssert.Contains(document, "ci-release-evidence");
            StringAssert.Contains(document, "ci-full-closeout");
            StringAssert.Contains(document, "ReleaseEvidence: not run in this lane");
            StringAssert.Contains(document, "Full closeout");
            StringAssert.Contains(document, "release readiness");
            StringAssert.Contains(document, "rollback");
        }

        foreach (var document in new[] { readme, contributing, projectMap })
        {
            Assert.IsFalse(
                document.Contains("shadow-run the broad `ci` job", StringComparison.OrdinalIgnoreCase),
                "rollout-guidance-contract: docs must not describe broad ci as an ordinary PR shadow after handoff.");
            Assert.IsFalse(
                document.Contains("broad CI may still shadow ordinary PRs", StringComparison.OrdinalIgnoreCase),
                "rollout-guidance-contract: docs must not describe broad ci as an ordinary PR shadow after handoff.");
            Assert.IsFalse(
                document.Contains("temporary broad `ci` shadow job", StringComparison.OrdinalIgnoreCase),
                "rollout-guidance-contract: project map must not describe broad ci as an ordinary PR shadow after handoff.");
            Assert.IsFalse(
                document.Contains("Hosted PR CI still keeps the broad `ci` shadow job", StringComparison.OrdinalIgnoreCase),
                "rollout-guidance-contract: project map risk notes must not describe broad ci as an active ordinary PR shadow after handoff.");
        }
    }

    private static string ReadChangeFile(string fileName)
    {
        return ReadChangeFile("2026-05-18-pr-ci-validation-tiering", fileName);
    }

    private static string ReadChangeFile(string changeId, string fileName)
    {
        return File.ReadAllText(Path.Combine(
            TestRepo.FindRoot().FullName,
            "docs",
            "changes",
            changeId,
            fileName));
    }
}
