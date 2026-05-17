namespace VeloFile.App.Tests.UiDesign;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("AppShellContract")]
[TestCategory("Accessibility")]
public sealed class CommandBandResourceContractTests
{
    [TestMethod]
    public void App_resources_merge_command_band_dictionary()
    {
        var appXaml = ReadRepoFile("src", "VeloFile.App", "App.xaml");

        StringAssert.Contains(appXaml, "Source=\"Resources/Components/VeloFile.CommandBand.xaml\"");
        Assert.IsTrue(
            RepoFileExists("src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml"),
            "Missing command-band component resource dictionary.");
    }

    [TestMethod]
    public void Command_band_dictionary_exposes_tokenized_region_resources()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.CommandBand.xaml");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfCommandBandContainerStyle\"",
            "x:Key=\"VfCommandBandNavigationGroupStyle\"",
            "x:Key=\"VfCommandBandSearchGroupStyle\"",
            "x:Key=\"VfCommandBandButtonStyle\"",
            "x:Key=\"VfCommandBandIconButtonStyle\"",
            "x:Key=\"VfCommandBandInputStyle\"",
            "x:Key=\"VfCommandBandPathStyle\"",
            "x:Key=\"VfCommandBandBreadcrumbButtonStyle\"",
            "x:Key=\"VfCommandBandStatusTextStyle\"",
            "x:Key=\"VfCommandBandButtonHoverBackgroundBrush\"",
            "x:Key=\"VfCommandBandButtonPressedBackgroundBrush\"",
            "x:Key=\"VfCommandBandButtonDisabledBackgroundBrush\"",
            "x:Key=\"VfCommandBandInputBorderBrush\"",
            "x:Key=\"VfCommandBandInputFocusBorderBrush\"",
            "x:Key=\"VfCommandBandDisabledOpacity\""
        })
        {
            StringAssert.Contains(xaml, requiredResource);
        }

        foreach (var requiredSetter in new[]
        {
            "Property=\"MinHeight\" Value=\"{StaticResource VfCommandBandControlHeight}\"",
            "Property=\"CornerRadius\" Value=\"{StaticResource VfCommandBandControlRadius}\"",
            "Property=\"Padding\" Value=\"{StaticResource VfCommandBandControlPadding}\"",
            "Property=\"FontFamily\" Value=\"{StaticResource VfFontUi}\"",
            "Property=\"FontSize\" Value=\"{StaticResource VfTextSizeBase}\"",
            "Property=\"FocusVisualPrimaryBrush\" Value=\"{StaticResource VfCommandBandFocusBrush}\"",
            "Property=\"FocusVisualPrimaryThickness\" Value=\"{StaticResource VfCommandBandFocusThickness}\""
        })
        {
            StringAssert.Contains(xaml, requiredSetter);
        }
    }

    [TestMethod]
    public void Main_window_command_band_consumes_named_resources_and_preserves_routes()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var codeBehind = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml.cs");
        var region = ExtractScopeRegions(xaml, "shell-command-band");

        foreach (var requiredName in new[]
        {
            "x:Name=\"NavigationButtons\"",
            "x:Name=\"BreadcrumbPathBar\"",
            "x:Name=\"RawPathBox\"",
            "x:Name=\"FilterSearchSurface\"",
            "x:Name=\"CurrentFolderFilterBox\"",
            "x:Name=\"RecursiveSearchBox\"",
            "x:Name=\"RecursiveSearchButton\"",
            "x:Name=\"CancelSearchButton\"",
            "x:Name=\"ClearSearchButton\""
        })
        {
            StringAssert.Contains(region, requiredName);
        }

        foreach (var requiredStyle in new[]
        {
            "Style=\"{StaticResource VfCommandBandContainerStyle}\"",
            "Style=\"{StaticResource VfCommandBandNavigationGroupStyle}\"",
            "Style=\"{StaticResource VfCommandBandIconButtonStyle}\"",
            "Style=\"{StaticResource VfCommandBandPathListStyle}\"",
            "Style=\"{StaticResource VfCommandBandPathStyle}\"",
            "Style=\"{StaticResource VfCommandBandSearchContainerStyle}\"",
            "Style=\"{StaticResource VfCommandBandInputStyle}\"",
            "Style=\"{StaticResource VfCommandBandButtonStyle}\"",
            "Style=\"{StaticResource VfCommandBandStatusTextStyle}\""
        })
        {
            StringAssert.Contains(region, requiredStyle);
        }

        foreach (var requiredRoute in new[]
        {
            "Click=\"BackButton_Click\"",
            "Click=\"ForwardButton_Click\"",
            "Click=\"UpButton_Click\"",
            "Click=\"RefreshButton_Click\"",
            "ItemClick=\"BreadcrumbPathBar_ItemClick\"",
            "KeyDown=\"RawPathBox_KeyDown\"",
            "TextChanged=\"CurrentFolderFilterBox_TextChanged\"",
            "KeyDown=\"RecursiveSearchBox_KeyDown\"",
            "Click=\"RecursiveSearchButton_Click\"",
            "Click=\"CancelSearchButton_Click\"",
            "Click=\"ClearSearchButton_Click\"",
            "ViewModel.SetCurrentFolderFilter(CurrentFolderFilterBox.Text)",
            "ViewModel.StartRecursiveSearch(RecursiveSearchBox.Text)",
            "ViewModel.CancelRecursiveSearch()",
            "ViewModel.ClearRecursiveSearch()"
        })
        {
            StringAssert.Contains(requiredRoute.Contains("ViewModel.", StringComparison.Ordinal) ? codeBehind : region, requiredRoute);
        }

        foreach (var requiredAccessibleText in new[]
        {
            "ToolTipService.ToolTip=\"Back\"",
            "ToolTipService.ToolTip=\"Forward\"",
            "ToolTipService.ToolTip=\"Up\"",
            "ToolTipService.ToolTip=\"Refresh\"",
            "AutomationProperties.Name=\"Raw path\"",
            "AutomationProperties.Name=\"Current folder filter\"",
            "AutomationProperties.Name=\"Recursive search\"",
            "AutomationProperties.Name=\"Run recursive search\"",
            "AutomationProperties.Name=\"Cancel recursive search\"",
            "AutomationProperties.Name=\"Clear recursive search\""
        })
        {
            StringAssert.Contains(region, requiredAccessibleText);
        }

        Assert.IsFalse(region.Contains("Width=\"36\"", StringComparison.Ordinal), "Command icon buttons must use the command-band width resource.");
        Assert.IsFalse(region.Contains("Height=\"16\"", StringComparison.Ordinal), "Command icons must use the command-band icon size resource.");
        Assert.IsFalse(region.Contains("Padding=\"8,4\"", StringComparison.Ordinal), "Breadcrumb buttons must use command-band padding resources.");
        Assert.IsFalse(region.Contains("Opacity=\"0.72\"", StringComparison.Ordinal), "Command status text must use the command-band muted text style.");
    }

    [TestMethod]
    public void Command_band_scope_contract_is_active_and_declares_required_resources()
    {
        var scopes = ReadRepoFile("docs", "ui", "ui-contract-scopes.v1.json");

        StringAssert.Contains(scopes, "\"id\": \"shell-command-band\"");
        StringAssert.Contains(scopes, "\"status\": \"active\"");
        StringAssert.Contains(scopes, "\"src/VeloFile.App/Resources/Components/VeloFile.CommandBand.xaml\"");
        StringAssert.Contains(scopes, "\"VfCommandBandButtonStyle\"");
        StringAssert.Contains(scopes, "\"VfCommandBandInputStyle\"");
        StringAssert.Contains(scopes, "\"VfCommandBandPathStyle\"");
        StringAssert.Contains(scopes, "\"VfCommandBandStatusTextStyle\"");
    }

    private static string ReadRepoFile(params string[] relativePath)
    {
        return File.ReadAllText(FindRepoRoot().Combine(relativePath).FullName);
    }

    private static bool RepoFileExists(string relativePath)
    {
        return File.Exists(FindRepoRoot().Combine(relativePath.Split('/')).FullName);
    }

    private static string ExtractScopeRegions(string xaml, string scopeId)
    {
        var startMarker = $"<!-- ui-contract-scope:{scopeId}:start -->";
        var endMarker = $"<!-- ui-contract-scope:{scopeId}:end -->";
        var regions = new List<string>();
        var searchIndex = 0;

        while (searchIndex < xaml.Length)
        {
            var startIndex = xaml.IndexOf(startMarker, searchIndex, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                break;
            }

            var endIndex = xaml.IndexOf(endMarker, startIndex, StringComparison.Ordinal);
            Assert.IsGreaterThan(startIndex, endIndex, $"Missing scope end marker '{endMarker}'.");
            regions.Add(xaml.Substring(startIndex, endIndex - startIndex + endMarker.Length));
            searchIndex = endIndex + endMarker.Length;
        }

        Assert.IsNotEmpty(regions, $"Missing scope marker '{startMarker}'.");
        return string.Join(Environment.NewLine, regions);
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
