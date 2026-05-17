namespace VeloFile.App.Tests.UiDesign;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("AppShellContract")]
[TestCategory("Accessibility")]
[TestCategory("Status")]
[TestCategory("Operations")]
public sealed class StatusOperationResourceContractTests
{
    [TestMethod]
    public void App_resources_merge_status_and_operation_dictionaries()
    {
        var appXaml = ReadRepoFile("src", "VeloFile.App", "App.xaml");

        StringAssert.Contains(appXaml, "Source=\"Resources/Components/VeloFile.Status.xaml\"");
        StringAssert.Contains(appXaml, "Source=\"Resources/Components/VeloFile.Operations.xaml\"");
        Assert.IsTrue(
            RepoFileExists("src/VeloFile.App/Resources/Components/VeloFile.Status.xaml"),
            "Missing status component resource dictionary.");
        Assert.IsTrue(
            RepoFileExists("src/VeloFile.App/Resources/Components/VeloFile.Operations.xaml"),
            "Missing operation component resource dictionary.");
    }

    [TestMethod]
    public void Status_and_operation_dictionaries_expose_state_resources()
    {
        var statusXaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.Status.xaml");
        var operationsXaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.Operations.xaml");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfStatusSurfaceStyle\"",
            "x:Key=\"VfStatusTextStyle\"",
            "x:Key=\"VfStatusMutedTextStyle\"",
            "x:Key=\"VfStatusFailureTextStyle\""
        })
        {
            StringAssert.Contains(statusXaml, requiredResource);
        }

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfOperationSurfaceStyle\"",
            "x:Key=\"VfOperationProgressStyle\"",
            "x:Key=\"VfOperationCompletedStyle\"",
            "x:Key=\"VfOperationCancelledStyle\"",
            "x:Key=\"VfOperationFailureStyle\"",
            "x:Key=\"VfOperationConflictStyle\"",
            "x:Key=\"VfOperationConflictStatusTextStyle\"",
            "x:Key=\"VfDestructiveConfirmationStyle\"",
            "x:Key=\"VfDestructiveStatusTextStyle\"",
            "x:Key=\"VfDestructiveConfirmationTextStyle\"",
            "x:Key=\"VfDestructiveActionButtonStyle\"",
            "x:Key=\"VfOperationCancelButtonStyle\"",
            "x:Key=\"VfOperationSecondaryButtonStyle\""
        })
        {
            StringAssert.Contains(operationsXaml, requiredResource);
        }

        StringAssert.Contains(operationsXaml, "x:Key=\"VfOperationDangerBrush\"");
        StringAssert.Contains(operationsXaml, "ResourceKey=\"VfShellDangerBrush\"");
        StringAssert.Contains(operationsXaml, "x:Key=\"VfOperationFocusBrush\"");
        StringAssert.Contains(operationsXaml, "ResourceKey=\"VfShellFocusBrush\"");
        StringAssert.Contains(operationsXaml, "Value=\"{StaticResource VfOperationDangerBrush}\"");
    }

    [TestMethod]
    public void Main_window_status_operation_scope_consumes_resources_and_preserves_routes()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var codeBehind = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml.cs");
        var region = ExtractScopeRegion(xaml, "shell-status-operations");

        foreach (var requiredReference in new[]
        {
            "Style=\"{StaticResource VfOperationSurfaceStyle}\"",
            "Style=\"{StaticResource VfDestructiveConfirmationStyle}\"",
            "Style=\"{StaticResource VfDestructiveConfirmationTextStyle}\"",
            "Style=\"{StaticResource VfDestructiveActionButtonStyle}\"",
            "Style=\"{StaticResource VfOperationCancelButtonStyle}\"",
            "Style=\"{StaticResource VfOperationConflictStyle}\"",
            "Style=\"{StaticResource VfOperationSecondaryButtonStyle}\"",
            "Style=\"{StaticResource VfStatusSurfaceStyle}\"",
            "Style=\"{StaticResource VfStatusTextStyle}\"",
            "Style=\"{StaticResource VfStatusMutedTextStyle}\"",
            "Style=\"{StaticResource VfStatusFailureTextStyle}\"",
            "Style=\"{StaticResource VfOperationProgressStyle}\""
        })
        {
            StringAssert.Contains(region, requiredReference);
        }

        foreach (var route in new[]
        {
            "Click=\"ConfirmPermanentDeleteButton_Click\"",
            "Click=\"CancelPermanentDeleteButton_Click\"",
            "Click=\"SkipConflictButton_Click\"",
            "Click=\"ReplaceConflictButton_Click\"",
            "Click=\"KeepBothConflictButton_Click\"",
            "Click=\"CancelFileOperationButton_Click\""
        })
        {
            StringAssert.Contains(region, route);
        }

        StringAssert.Contains(codeBehind, "ApplyFileOperationStatusStyle();");
        StringAssert.Contains(codeBehind, "FileOperationStatus.Running or FileOperationStatus.Cancelling");
        StringAssert.Contains(codeBehind, "FileOperationStatus.WaitingForConflict");
        StringAssert.Contains(codeBehind, "FileOperationStatus.WaitingForConfirmation");
        StringAssert.Contains(codeBehind, "FileOperationStatus.Cancelled");
        StringAssert.Contains(codeBehind, "FileOperationStatus.Failed");
        StringAssert.Contains(codeBehind, "VfOperationProgressStyle");
        StringAssert.Contains(codeBehind, "VfOperationConflictStatusTextStyle");
        StringAssert.Contains(codeBehind, "VfDestructiveStatusTextStyle");
        StringAssert.Contains(codeBehind, "VfOperationCancelledStyle");
        StringAssert.Contains(codeBehind, "VfOperationFailureStyle");
    }

    [TestMethod]
    public void Status_operation_scope_contract_is_active_and_declares_required_resources()
    {
        var scopes = ReadRepoFile("docs", "ui", "ui-contract-scopes.v1.json");
        var scope = ExtractJsonObjectContaining(scopes, "\"id\": \"shell-status-operations\"");

        StringAssert.Contains(scope, "\"status\": \"active\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/MainWindow.xaml\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/Resources/Components/VeloFile.Status.xaml\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/Resources/Components/VeloFile.Operations.xaml\"");
        StringAssert.Contains(scope, "\"VfStatusSurfaceStyle\"");
        StringAssert.Contains(scope, "\"VfOperationProgressStyle\"");
        StringAssert.Contains(scope, "\"VfOperationConflictStyle\"");
        StringAssert.Contains(scope, "\"VfDestructiveConfirmationStyle\"");
        StringAssert.Contains(scope, "\"VfDestructiveActionButtonStyle\"");
    }

    private static string ReadRepoFile(params string[] relativePath)
    {
        return File.ReadAllText(FindRepoRoot().Combine(relativePath).FullName);
    }

    private static bool RepoFileExists(string relativePath)
    {
        return File.Exists(FindRepoRoot().Combine(relativePath.Split('/')).FullName);
    }

    private static string ExtractScopeRegion(string xaml, string scopeId)
    {
        var startMarker = $"<!-- ui-contract-scope:{scopeId}:start -->";
        var endMarker = $"<!-- ui-contract-scope:{scopeId}:end -->";
        var startIndex = xaml.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, startIndex, $"Missing scope marker '{startMarker}'.");
        var endIndex = xaml.IndexOf(endMarker, startIndex, StringComparison.Ordinal);
        Assert.IsGreaterThan(startIndex, endIndex, $"Missing scope end marker '{endMarker}'.");

        return xaml.Substring(startIndex, endIndex - startIndex + endMarker.Length);
    }

    private static string ExtractJsonObjectContaining(string json, string marker)
    {
        var markerIndex = json.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, markerIndex, $"Missing JSON marker '{marker}'.");

        var objectStart = json.LastIndexOf('{', markerIndex);
        Assert.IsGreaterThanOrEqualTo(0, objectStart, $"Could not find JSON object start for '{marker}'.");

        var depth = 0;
        for (var index = objectStart; index < json.Length; index++)
        {
            depth += json[index] switch
            {
                '{' => 1,
                '}' => -1,
                _ => 0
            };

            if (depth == 0)
            {
                return json.Substring(objectStart, index - objectStart + 1);
            }
        }

        Assert.Fail($"Could not find JSON object end for '{marker}'.");
        throw new InvalidOperationException($"Could not find JSON object end for '{marker}'.");
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
