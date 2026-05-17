namespace VeloFile.App.Tests.UiDesign;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("AppShellContract")]
[TestCategory("Accessibility")]
[TestCategory("Preview")]
[TestCategory("PreviewSurface")]
public sealed class PreviewResourceContractTests
{
    [TestMethod]
    public void App_resources_merge_preview_dictionary()
    {
        var appXaml = ReadRepoFile("src", "VeloFile.App", "App.xaml");

        StringAssert.Contains(appXaml, "Source=\"Resources/Components/VeloFile.Preview.xaml\"");
        Assert.IsTrue(
            RepoFileExists("src/VeloFile.App/Resources/Components/VeloFile.Preview.xaml"),
            "Missing preview component resource dictionary.");
    }

    [TestMethod]
    public void Preview_dictionary_exposes_state_and_metadata_resources()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.Preview.xaml");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfPreviewPaneStyle\"",
            "x:Key=\"VfPreviewLayoutStyle\"",
            "x:Key=\"VfPreviewHeaderStyle\"",
            "x:Key=\"VfPreviewStatusStyle\"",
            "x:Key=\"VfPreviewLoadingStyle\"",
            "x:Key=\"VfPreviewReadyStyle\"",
            "x:Key=\"VfPreviewUnsupportedStyle\"",
            "x:Key=\"VfPreviewFailedStyle\"",
            "x:Key=\"VfPreviewContentTextStyle\"",
            "x:Key=\"VfPreviewImageStyle\"",
            "x:Key=\"VfPreviewPdfNavigationStyle\"",
            "x:Key=\"VfPreviewPdfButtonStyle\"",
            "x:Key=\"VfPreviewPdfPageIndicatorStyle\"",
            "x:Key=\"VfPreviewMetadataStyle\"",
            "x:Key=\"VfPreviewMetadataRowStyle\"",
            "x:Key=\"VfPreviewMetadataLabelStyle\"",
            "x:Key=\"VfPreviewMetadataValueStyle\""
        })
        {
            StringAssert.Contains(xaml, requiredResource);
        }

        StringAssert.Contains(xaml, "ResourceKey=\"VfShellAccentBrush\"");
        StringAssert.Contains(xaml, "ResourceKey=\"VfShellSuccessBrush\"");
        StringAssert.Contains(xaml, "ResourceKey=\"VfShellWarningBrush\"");
        StringAssert.Contains(xaml, "ResourceKey=\"VfShellDangerBrush\"");
    }

    [TestMethod]
    public void Main_window_preview_scope_consumes_resources_and_preserves_routes()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var codeBehind = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml.cs");
        var region = ExtractScopeRegion(xaml, "shell-preview-details");

        foreach (var requiredReference in new[]
        {
            "Style=\"{StaticResource VfPreviewPaneStyle}\"",
            "Style=\"{StaticResource VfPreviewLayoutStyle}\"",
            "Style=\"{StaticResource VfPreviewHeaderStyle}\"",
            "Style=\"{StaticResource VfPreviewStatusStyle}\"",
            "Style=\"{StaticResource VfPreviewContentTextStyle}\"",
            "Style=\"{StaticResource VfPreviewPdfNavigationStyle}\"",
            "Style=\"{StaticResource VfPreviewPdfButtonStyle}\"",
            "Style=\"{StaticResource VfPreviewPdfPageIndicatorStyle}\"",
            "Style=\"{StaticResource VfPreviewImageStyle}\"",
            "Style=\"{StaticResource VfPreviewMetadataStyle}\"",
            "Style=\"{StaticResource VfPreviewMetadataRowStyle}\"",
            "Style=\"{StaticResource VfPreviewMetadataLabelStyle}\"",
            "Style=\"{StaticResource VfPreviewMetadataValueStyle}\""
        })
        {
            StringAssert.Contains(region, requiredReference);
        }

        foreach (var route in new[]
        {
            "x:Name=\"PreviewPane\"",
            "x:Name=\"PreviewStatusText\"",
            "x:Name=\"PreviewContentText\"",
            "x:Name=\"PreviewImageSurface\"",
            "x:Name=\"PreviewMetadataList\"",
            "x:Name=\"PdfPageNavigationPanel\"",
            "Click=\"PdfPreviousPageButton_Click\"",
            "Click=\"PdfNextPageButton_Click\"",
            "AutomationProperties.Name=\"Preview pane\"",
            "AutomationProperties.Name=\"Preview content\"",
            "AutomationProperties.Name=\"PDF page navigation\"",
            "AutomationProperties.Name=\"Preview metadata\""
        })
        {
            StringAssert.Contains(region, route);
        }

        StringAssert.Contains(codeBehind, "ApplyPreviewStatusStyle();");
        StringAssert.Contains(codeBehind, "PreviewStatus.Loading");
        StringAssert.Contains(codeBehind, "PreviewStatus.Success");
        StringAssert.Contains(codeBehind, "PreviewStatus.Unsupported");
        StringAssert.Contains(codeBehind, "PreviewStatus.Failed");
        StringAssert.Contains(codeBehind, "VfPreviewLoadingStyle");
        StringAssert.Contains(codeBehind, "VfPreviewReadyStyle");
        StringAssert.Contains(codeBehind, "VfPreviewUnsupportedStyle");
        StringAssert.Contains(codeBehind, "VfPreviewFailedStyle");
        StringAssert.Contains(codeBehind, "ViewModel.IsPreviewPaneOpen ? new GridLength(320) : new GridLength(0)");
    }

    [TestMethod]
    public void Preview_scope_contract_is_active_and_declares_required_resources()
    {
        var scopes = ReadRepoFile("docs", "ui", "ui-contract-scopes.v1.json");
        var scope = ExtractJsonObjectContaining(scopes, "\"id\": \"shell-preview-details\"");

        StringAssert.Contains(scope, "\"status\": \"active\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/MainWindow.xaml\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/Resources/Components/VeloFile.Preview.xaml\"");
        StringAssert.Contains(scope, "\"VfPreviewPaneStyle\"");
        StringAssert.Contains(scope, "\"VfPreviewHeaderStyle\"");
        StringAssert.Contains(scope, "\"VfPreviewMetadataStyle\"");
        StringAssert.Contains(scope, "\"VfPreviewLoadingStyle\"");
        StringAssert.Contains(scope, "\"VfPreviewFailedStyle\"");
        StringAssert.Contains(scope, "\"VfPreviewPdfNavigationStyle\"");
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
