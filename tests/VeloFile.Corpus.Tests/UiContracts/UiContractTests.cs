using System.Diagnostics;
using System.Text.Json.Nodes;

namespace VeloFile.Corpus.Tests.UiContracts;

[TestClass]
[TestCategory("UiContracts")]
public sealed class UiContractTests
{
    [TestMethod]
    public void Token_contract_defines_first_slice_values()
    {
        var repoRoot = FindRepoRoot();
        var contractPath = Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json");

        Assert.IsTrue(File.Exists(contractPath), "docs/ui/tokens.v1.json must exist.");

        var contract = JsonNode.Parse(File.ReadAllText(contractPath))!.AsObject();
        Assert.AreEqual(1, (int?)contract["version"]);
        Assert.AreEqual("dark", (string?)contract["theme"]);
        Assert.AreEqual("comfortable", (string?)contract["density"]);

        var tokens = contract["tokens"]!.AsArray().Select(token => token!.AsObject()).ToArray();
        Assert.IsGreaterThanOrEqualTo(62, tokens.Length, "The first-slice contract must include all T1-T5 tokens.");

        foreach (var token in tokens)
        {
            AssertRequiredString(token, "id");
            AssertRequiredArray(token, "xamlKeys");
            AssertRequiredString(token, "type");
            Assert.IsTrue(token.ContainsKey("value"), $"Token {token["id"]} must declare value.");
            AssertRequiredString(token, "category");
            Assert.IsNotNull((bool?)token["requiredInFirstSlice"], $"Token {token["id"]} must declare requiredInFirstSlice.");
            Assert.IsFalse(((string)token["id"]!).Contains("hifi", StringComparison.OrdinalIgnoreCase));
        }

        var byId = tokens.ToDictionary(token => (string)token["id"]!, StringComparer.Ordinal);
        AssertToken(byId, "VfColor.Surface.Content", "#202329");
        AssertToken(byId, "VfColor.Accent.Line", "#9BE15D66");
        AssertToken(byId, "VfFont.Ui", "Segoe UI Variable, Segoe UI, system-ui, sans-serif");
        AssertToken(byId, "VfText.Size.Base", 12.5);
        AssertToken(byId, "VfSize.SidebarWidth", 240);
        AssertToken(byId, "VfDensity.Current", "comfortable");
        AssertToken(byId, "VfDensity.RowHeight", 30);
        AssertToken(byId, "VfFocus.Color", "VfColor.Accent.Line");
        AssertToken(byId, "VfState.HiddenOpacity", 0.68);
    }

    [TestMethod]
    public void Scope_and_deviation_contracts_define_first_slice_boundaries()
    {
        var repoRoot = FindRepoRoot();
        var scopesPath = Path.Combine(repoRoot.FullName, "docs", "ui", "ui-contract-scopes.v1.json");
        var deviationsPath = Path.Combine(repoRoot.FullName, "docs", "ui", "design-deviations.md");

        Assert.IsTrue(File.Exists(scopesPath), "docs/ui/ui-contract-scopes.v1.json must exist.");
        Assert.IsTrue(File.Exists(deviationsPath), "docs/ui/design-deviations.md must exist.");

        var scopes = JsonNode.Parse(File.ReadAllText(scopesPath))!.AsObject();
        Assert.AreEqual(1, (int?)scopes["version"]);
        var firstScope = scopes["scopes"]!.AsArray().Select(scope => scope!.AsObject())
            .Single(scope => string.Equals((string?)scope["id"], "file-list-first-slice", StringComparison.Ordinal));

        Assert.AreEqual("active", (string?)firstScope["status"]);
        CollectionAssert.IsSubsetOf(
            new[] { "src/VeloFile.App/MainWindow.xaml", "src/VeloFile.App/Resources/Components/VeloFile.FileList.xaml" },
            firstScope["files"]!.AsArray().Select(value => (string)value!).ToArray());
        CollectionAssert.IsSubsetOf(
            new[] { "VfFileListRowTemplate", "VfFileListItemContainerStyle", "VfFileListRowHeight" },
            firstScope["requiredResourceReferences"]!.AsArray().Select(value => (string)value!).ToArray());
        CollectionAssert.Contains(
            firstScope["forbiddenLiteralRules"]!.AsArray().Select(value => (string)value!).ToArray(),
            "inline-color");

        var deviations = File.ReadAllText(deviationsPath);
        StringAssert.Contains(deviations, "proposed");
        StringAssert.Contains(deviations, "accepted");
        StringAssert.Contains(deviations, "temporary");
        StringAssert.Contains(deviations, "rejected");
        StringAssert.Contains(deviations, "Reference pattern");
        StringAssert.Contains(deviations, "VeloFile decision");
        StringAssert.Contains(deviations, "Verification");
    }

    [TestMethod]
    public void Ui_contract_tool_is_in_solution_and_validates_static_fixtures()
    {
        var repoRoot = FindRepoRoot();
        var solution = File.ReadAllText(Path.Combine(repoRoot.FullName, "VeloFile.sln"));

        StringAssert.Contains(solution, "tools\\VeloFile.UiContracts\\VeloFile.UiContracts.csproj");

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "valid"));

        AssertCommandSucceeded(result);
        Assert.IsFalse(result.AllOutput.Contains("Microsoft.UI.Xaml", StringComparison.OrdinalIgnoreCase), "Tool output should not indicate a WinUI runtime dependency.");
    }

    [TestMethod]
    [DataRow("missing-key", "VfBrushSurfaceContent")]
    [DataRow("duplicate-key", "duplicate")]
    [DataRow("wrong-type", "expected type")]
    [DataRow("wrong-value", "expected value")]
    [DataRow("wrong-brush-reference", "brush")]
    public void Ui_contract_tool_rejects_invalid_token_fixtures(string fixtureName, string expectedOutput)
    {
        var repoRoot = FindRepoRoot();

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "invalid", fixtureName));

        Assert.AreNotEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, expectedOutput);
    }

    [TestMethod]
    [DataRow("extra-resource", "extra-resource", "VfFileListUnapprovedGap")]
    [DataRow("component-inline-color", "forbidden-literal", "#FF00FF")]
    [DataRow("component-inline-row-height", "forbidden-literal", "MinHeight")]
    [DataRow("component-inline-padding", "forbidden-literal", "Padding")]
    [DataRow("token-undocumented-color", "extra-resource", "VfColorRandomAccent")]
    public void Ui_contract_tool_rejects_governed_resource_drift_by_default(
        string fixtureName,
        string expectedRule,
        string expectedOutput)
    {
        var repoRoot = FindRepoRoot();

        var result = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "invalid", fixtureName));

        Assert.AreNotEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, expectedRule);
        StringAssert.Contains(result.AllOutput, expectedOutput);
        StringAssert.Contains(result.AllOutput, fixtureName);
    }

    [TestMethod]
    public void Ui_contract_tool_enforces_targeted_scope_literals_without_blocking_legacy_literals()
    {
        var repoRoot = FindRepoRoot();

        var valid = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "valid"),
            "--scopes",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "scopes.valid.json"),
            "--scope-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "valid"));

        AssertCommandSucceeded(valid);

        var invalid = RunTool(
            repoRoot,
            "validate-tokens",
            "--contract",
            Path.Combine(repoRoot.FullName, "docs", "ui", "tokens.v1.json"),
            "--xaml-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "valid"),
            "--scopes",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "scopes.forbidden-literal.json"),
            "--scope-root",
            Path.Combine(repoRoot.FullName, "tests", "fixtures", "ui-contracts", "invalid", "forbidden-literal"));

        Assert.AreNotEqual(0, invalid.ExitCode, invalid.AllOutput);
        StringAssert.Contains(invalid.AllOutput, "file-list-first-slice");
        StringAssert.Contains(invalid.AllOutput, "inline-color");
    }

    private static void AssertToken(IReadOnlyDictionary<string, JsonObject> tokens, string id, object expectedValue)
    {
        Assert.IsTrue(tokens.TryGetValue(id, out var token), $"Missing token {id}.");

        var actual = token["value"];
        switch (expectedValue)
        {
            case string expected:
                Assert.AreEqual(expected, (string?)actual, id);
                break;
            case int expected:
                Assert.AreEqual(expected, (int?)actual, id);
                break;
            case double expected:
                Assert.AreEqual(expected, (double?)actual ?? double.NaN, 0.0001, id);
                break;
            default:
                Assert.Fail($"Unsupported expected value type for {id}.");
                break;
        }
    }

    private static void AssertRequiredString(JsonObject node, string propertyName)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace((string?)node[propertyName]), $"Missing {propertyName}.");
    }

    private static void AssertRequiredArray(JsonObject node, string propertyName)
    {
        Assert.IsGreaterThan(0, node[propertyName]?.AsArray().Count ?? 0, $"Missing {propertyName}.");
    }

    private static void AssertCommandSucceeded(CommandResult result)
    {
        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
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
}
