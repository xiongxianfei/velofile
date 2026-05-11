using System.Diagnostics;
using System.Text.Json.Nodes;

namespace VeloFile.Corpus.Tests.Visual;

[TestClass]
[TestCategory("Visual")]
[TestCategory("UiContracts")]
public sealed class VisualBaselineInventoryTests
{
    private static readonly string[] RequiredScreens =
    [
        "file-list-normal",
        "file-list-selected-row",
        "file-list-focused-row",
        "file-list-selected-focused-row",
        "file-list-multi-selection",
        "file-list-hidden-protected",
        "file-list-thumbnail-fallback",
        "file-list-long-names",
        "file-list-empty-folder"
    ];

    [TestMethod]
    public void First_slice_visual_baselines_have_pngs_and_safe_sidecars()
    {
        var repoRoot = FindRepoRoot();
        var baselineRoot = Path.Combine(
            repoRoot.FullName,
            "tests",
            "visual",
            "baselines",
            "winui",
            "dark-comfortable-1440x900-100");

        Assert.IsTrue(Directory.Exists(baselineRoot), "The first WinUI visual baseline profile must exist.");

        foreach (var screen in RequiredScreens)
        {
            var pngPath = Path.Combine(baselineRoot, screen + ".png");
            var jsonPath = Path.Combine(baselineRoot, screen + ".json");

            Assert.IsTrue(File.Exists(pngPath), $"{screen}.png must exist.");
            Assert.IsTrue(File.Exists(jsonPath), $"{screen}.json must exist.");
            AssertPngSignature(pngPath);
            AssertPngDimensions(pngPath, 1440, 900);

            var sidecar = JsonNode.Parse(File.ReadAllText(jsonPath))!.AsObject();
            Assert.AreEqual("dark", (string?)sidecar["theme"], screen);
            Assert.AreEqual("comfortable", (string?)sidecar["density"], screen);
            Assert.AreEqual("1440x900", (string?)sidecar["viewport"], screen);
            Assert.AreEqual(1.0, (double?)sidecar["scale"], screen);
            Assert.AreEqual(screen, (string?)sidecar["screen"], screen);
            var expectedFixture = string.Equals(screen, "file-list-empty-folder", StringComparison.Ordinal)
                ? "file-list-empty-folder"
                : "file-list-v1";
            Assert.AreEqual(expectedFixture, (string?)sidecar["fixture"], screen);
            Assert.IsNotNull(sidecar["dynamicRegions"]?.AsArray(), $"{screen} must declare dynamicRegions.");
            AssertRequiredString(sidecar, "reviewId", screen);

            var serialized = sidecar.ToJsonString();
            Assert.IsFalse(serialized.Contains("C:\\Users", StringComparison.OrdinalIgnoreCase), screen);
            Assert.IsFalse(serialized.Contains("xiongxianfei", StringComparison.OrdinalIgnoreCase), screen);
            Assert.IsFalse(serialized.Contains("20260428-velofile", StringComparison.OrdinalIgnoreCase), screen);
        }
    }

    [TestMethod]
    public void Generated_visual_outputs_are_gitignored()
    {
        var repoRoot = FindRepoRoot();
        var gitignore = File.ReadAllText(Path.Combine(repoRoot.FullName, ".gitignore"));

        StringAssert.Contains(gitignore, "tests/visual/current/");
        StringAssert.Contains(gitignore, "tests/visual/diffs/");

        var ciScript = File.ReadAllText(Path.Combine(repoRoot.FullName, "scripts", "ci.ps1"));
        Assert.IsFalse(
            ciScript.Contains("update-ui-baselines", StringComparison.OrdinalIgnoreCase),
            "CI may compare or validate visual evidence, but it must not update reviewed baselines.");
    }

    [TestMethod]
    public void Update_ui_baselines_requires_review_id_and_current_screenshots()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.Create();

        var missingReviewId = RunScript(
            repoRoot,
            "-Suite",
            "winui",
            "-Profile",
            "dark-comfortable-1440x900-100",
            "-RepositoryRoot",
            scratch.Root);

        Assert.AreNotEqual(0, missingReviewId.ExitCode);
        StringAssert.Contains(missingReviewId.AllOutput, "ReviewId");

        var missingCurrent = RunScript(
            repoRoot,
            "-Suite",
            "winui",
            "-Profile",
            "dark-comfortable-1440x900-100",
            "-ReviewId",
            "review-123",
            "-RepositoryRoot",
            scratch.Root);

        Assert.AreNotEqual(0, missingCurrent.ExitCode);
        StringAssert.Contains(missingCurrent.AllOutput, "current screenshots");
    }

    [TestMethod]
    public void Update_ui_baselines_copies_reviewed_current_screenshots_and_sidecars()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.Create();
        var profile = "dark-comfortable-1440x900-100";
        var currentRoot = Path.Combine(scratch.Root, "tests", "visual", "current", "winui", profile);
        Directory.CreateDirectory(currentRoot);
        var sourcePng = Path.Combine(currentRoot, "file-list-normal.png");
        var sourceJson = Path.Combine(currentRoot, "file-list-normal.json");
        File.WriteAllBytes(sourcePng, PngSignature());
        File.WriteAllText(
            sourceJson,
            """
            {
              "theme": "dark",
              "density": "comfortable",
              "viewport": "1440x900",
              "scale": 1.0,
              "screen": "file-list-normal",
              "fixture": "file-list-v1",
              "dynamicRegions": []
            }
            """);

        var result = RunScript(
            repoRoot,
            "-Suite",
            "winui",
            "-Profile",
            profile,
            "-ReviewId",
            "review-123",
            "-RepositoryRoot",
            scratch.Root);

        AssertCommandSucceeded(result);

        var baselineRoot = Path.Combine(scratch.Root, "tests", "visual", "baselines", "winui", profile);
        var baselinePng = Path.Combine(baselineRoot, "file-list-normal.png");
        var baselineJson = Path.Combine(baselineRoot, "file-list-normal.json");
        Assert.IsTrue(File.Exists(baselinePng));
        Assert.IsTrue(File.Exists(baselineJson));
        AssertPngSignature(baselinePng);

        var sidecar = JsonNode.Parse(File.ReadAllText(baselineJson))!.AsObject();
        Assert.AreEqual("review-123", (string?)sidecar["reviewId"]);
        AssertRequiredString(sidecar, "updatedAtUtc", "file-list-normal");
    }

    private static void AssertRequiredString(JsonObject sidecar, string propertyName, string screen)
    {
        var value = (string?)sidecar[propertyName];
        Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"{screen} must include {propertyName}.");
    }

    private static void AssertPngSignature(string path)
    {
        CollectionAssert.AreEqual(PngSignature(), File.ReadAllBytes(path).Take(8).ToArray(), path);
    }

    private static void AssertPngDimensions(string path, int expectedWidth, int expectedHeight)
    {
        var bytes = File.ReadAllBytes(path);
        Assert.IsGreaterThanOrEqualTo(24, bytes.Length, path);
        var width = ReadBigEndianInt32(bytes, 16);
        var height = ReadBigEndianInt32(bytes, 20);

        Assert.AreEqual(expectedWidth, width, path);
        Assert.AreEqual(expectedHeight, height, path);
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24)
            | (bytes[offset + 1] << 16)
            | (bytes[offset + 2] << 8)
            | bytes[offset + 3];
    }

    private static byte[] PngSignature()
    {
        return [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    }

    private static CommandResult RunScript(DirectoryInfo repoRoot, params string[] arguments)
    {
        var scriptPath = Path.Combine(repoRoot.FullName, "scripts", "update-ui-baselines.ps1");
        var startInfo = new ProcessStartInfo("powershell")
        {
            WorkingDirectory = repoRoot.FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start powershell.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new CommandResult(
            process.ExitCode,
            stdoutTask.GetAwaiter().GetResult(),
            stderrTask.GetAwaiter().GetResult());
    }

    private static void AssertCommandSucceeded(CommandResult result)
    {
        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
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

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + StandardError;
    }

    private sealed class ScratchWorkspace : IDisposable
    {
        private ScratchWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static ScratchWorkspace Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "velofile-visual-tests", "visual-" + Guid.NewGuid().ToString("N"));
            return new ScratchWorkspace(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
