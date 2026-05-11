using System.Diagnostics;
using System.Text.Json.Nodes;

namespace VeloFile.Corpus.Tests.UiContracts;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("Visual")]
public sealed class ShellVisualCoherenceContractTests
{
    private static readonly string[] FollowOnScopeIds =
    [
        "shell-surface-foundation",
        "shell-command-band",
        "shell-sidebar",
        "shell-status-operations",
        "shell-preview-details",
        "fixture-icons",
        "full-shell-visual-evidence"
    ];

    private static readonly string[] BehaviorRows =
    [
        "navigation",
        "tabs-session-restore",
        "listing-virtualization",
        "selection",
        "filter-search",
        "context-menu",
        "file-operations",
        "drag-drop",
        "preview",
        "terminal-launch",
        "diagnostics",
        "persistence",
        "accessibility-routes"
    ];

    [TestMethod]
    public void Shell_visual_coherence_scope_contract_declares_follow_on_regions_and_behavior_matrix()
    {
        var repoRoot = FindRepoRoot();
        var scopesPath = Path.Combine(repoRoot.FullName, "docs", "ui", "ui-contract-scopes.v1.json");
        var scopes = JsonNode.Parse(File.ReadAllText(scopesPath))!.AsObject();

        Assert.AreEqual(1, (int?)scopes["version"]);

        var byId = scopes["scopes"]!.AsArray()
            .Select(scope => scope!.AsObject())
            .ToDictionary(scope => (string)scope["id"]!, StringComparer.Ordinal);

        foreach (var scopeId in FollowOnScopeIds)
        {
            Assert.IsTrue(byId.TryGetValue(scopeId, out var scope), $"Missing follow-on scope '{scopeId}'.");
            Assert.AreEqual("planned", (string?)scope["status"], scopeId);
            Assert.IsGreaterThan(0, scope["files"]!.AsArray().Count, scopeId);
            Assert.IsGreaterThan(0, scope["requiredResourceReferences"]!.AsArray().Count, scopeId);
            Assert.IsGreaterThan(0, scope["allowedResourceKeys"]!.AsArray().Count, scopeId);
            Assert.IsGreaterThan(0, scope["forbiddenLiteralRules"]!.AsArray().Count, scopeId);
        }

        var behaviorRows = scopes["behaviorPreservationMatrix"]!.AsArray()
            .Select(row => (string)row!.AsObject()["id"]!)
            .ToArray();

        CollectionAssert.IsSubsetOf(BehaviorRows, behaviorRows);
    }

    [TestMethod]
    public void Ui_contract_tool_rejects_forbidden_fixture_icon_resources()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.CreateFromValidFixtures(repoRoot);
        var iconsRoot = Path.Combine(scratch.Root, "Resources", "Icons");
        Directory.CreateDirectory(iconsRoot);
        File.WriteAllText(
            Path.Combine(iconsRoot, "VeloFile.FixtureIcons.xaml"),
            """
            <ResourceDictionary
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <SymbolIcon x:Key="VfFixtureBadSymbol" Symbol="Page" />
              <TextBlock x:Key="VfFixtureBadChip" Text="P..." />
            </ResourceDictionary>
            """);

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(scratch.Root, "Resources"));

        Assert.AreNotEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, "forbidden-icon");
        StringAssert.Contains(result.AllOutput, "SymbolIcon");
        StringAssert.Contains(result.AllOutput, "P...");
    }

    [TestMethod]
    public void Ui_contract_tool_rejects_fixture_icon_fill_color_literal()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.CreateFromValidFixtures(repoRoot);
        var iconsRoot = Path.Combine(scratch.Root, "Resources", "Icons");
        Directory.CreateDirectory(iconsRoot);
        File.Copy(
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "invalid", "fixture-icon-local-color", "Resources", "Icons", "VeloFile.FixtureIcons.xaml"),
            Path.Combine(iconsRoot, "VeloFile.FixtureIcons.xaml"),
            overwrite: true);

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(scratch.Root, "Resources"));

        Assert.AreNotEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, "forbidden-fixture-icon-color");
        StringAssert.Contains(result.AllOutput, "Fill");
        StringAssert.Contains(result.AllOutput, "#FFFFFF");
        StringAssert.Contains(result.AllOutput, "VeloFile.FixtureIcons.xaml");
    }

    [TestMethod]
    public void Ui_contract_tool_rejects_fixture_icon_solid_color_brush_literal()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.CreateFromValidFixtures(repoRoot);
        var iconsRoot = Path.Combine(scratch.Root, "Resources", "Icons");
        Directory.CreateDirectory(iconsRoot);
        File.WriteAllText(
            Path.Combine(iconsRoot, "VeloFile.FixtureIcons.xaml"),
            """
            <ResourceDictionary
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <SolidColorBrush Color="#FFFFFF" />
            </ResourceDictionary>
            """);

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(scratch.Root, "Resources"));

        Assert.AreNotEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, "forbidden-fixture-icon-color");
        StringAssert.Contains(result.AllOutput, "Color");
        StringAssert.Contains(result.AllOutput, "#FFFFFF");
    }

    [TestMethod]
    public void Ui_contract_tool_allows_fixture_icon_color_resource_reference()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.CreateFromValidFixtures(repoRoot);
        var iconsRoot = Path.Combine(scratch.Root, "Resources", "Icons");
        Directory.CreateDirectory(iconsRoot);
        File.WriteAllText(
            Path.Combine(iconsRoot, "VeloFile.FixtureIcons.xaml"),
            """
            <ResourceDictionary
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <Path Data="M 0,0 L 1,0 L 1,1 Z" Fill="{StaticResource VfIconForegroundBrush}" />
              <Path Data="M 0,0 L 1,0 L 1,1 Z" Stroke="{ThemeResource VfIconStrokeBrush}" />
            </ResourceDictionary>
            """);

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(scratch.Root, "Resources"));

        AssertCommandSucceeded(result);
        Assert.IsFalse(result.AllOutput.Contains("forbidden-fixture-icon-color", StringComparison.Ordinal), result.AllOutput);
    }

    [TestMethod]
    public void Ui_contract_tool_validates_full_shell_visual_sidecars()
    {
        var repoRoot = FindRepoRoot();
        using var scratch = ScratchWorkspace.CreateFromValidFixtures(repoRoot);
        var visualRoot = Path.Combine(scratch.Root, "tests", "visual", "baselines", "winui", "shell-standard-1440x900-100");
        Directory.CreateDirectory(visualRoot);
        File.WriteAllBytes(Path.Combine(visualRoot, "shell-default.png"), PngSignature());
        File.WriteAllText(
            Path.Combine(visualRoot, "shell-default.json"),
            """
            {
              "profile": "shell-standard-1440x900-100",
              "effectiveWindowSize": "1440x900",
              "scale": 1.0,
              "theme": "dark",
              "density": "comfortable",
              "fixture": "shell-default",
              "evidenceKind": "soft-review",
              "dynamicRegions": [],
              "reviewId": "review-123"
            }
            """);

        var valid = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(scratch.Root, "Resources"),
            "--visual-root",
            Path.Combine(scratch.Root, "tests", "visual", "baselines", "winui"));

        AssertCommandSucceeded(valid);

        File.WriteAllText(
            Path.Combine(visualRoot, "shell-search-active.json"),
            """
            {
              "profile": "shell-standard-1440x900-100",
              "effectiveWindowSize": "1440x900",
              "scale": 2.0,
              "theme": "dark",
              "density": "comfortable",
              "fixture": "shell-search-active",
              "evidenceKind": "soft-review",
              "dynamicRegions": [],
              "reviewId": "review-123",
              "path": "C:\\Users\\someone\\secret.txt"
            }
            """);

        var invalid = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(scratch.Root, "Resources"),
            "--visual-root",
            Path.Combine(scratch.Root, "tests", "visual", "baselines", "winui"));

        Assert.AreNotEqual(0, invalid.ExitCode, invalid.AllOutput);
        StringAssert.Contains(invalid.AllOutput, "sidecar");
        StringAssert.Contains(invalid.AllOutput, "scale");
        StringAssert.Contains(invalid.AllOutput, "raw local path");
    }

    private static CommandResult RunTool(DirectoryInfo repoRoot, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = repoRoot.FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(Path.Combine(repoRoot.FullName, "tools", "VeloFile.UiContracts", "VeloFile.UiContracts.csproj"));
        startInfo.ArgumentList.Add("--");

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dotnet.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new CommandResult(process.ExitCode, stdout.GetAwaiter().GetResult(), stderr.GetAwaiter().GetResult());
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

    private static byte[] PngSignature()
    {
        return [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
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

        public static ScratchWorkspace CreateFromValidFixtures(DirectoryInfo repoRoot)
        {
            var root = Path.Combine(Path.GetTempPath(), "velofile-ui-contracts", "shell-" + Guid.NewGuid().ToString("N"));
            CopyDirectory(Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "valid", "Resources"), Path.Combine(root, "Resources"));
            return new ScratchWorkspace(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directory.Replace(source, destination, StringComparison.Ordinal));
            }

            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(source, destination, StringComparison.Ordinal), overwrite: true);
            }
        }
    }
}
