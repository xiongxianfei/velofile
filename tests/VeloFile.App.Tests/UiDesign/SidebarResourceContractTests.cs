namespace VeloFile.App.Tests.UiDesign;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("AppShellContract")]
[TestCategory("Accessibility")]
[TestCategory("Sidebar")]
public sealed class SidebarResourceContractTests
{
    [TestMethod]
    public void App_resources_merge_sidebar_dictionary()
    {
        var appXaml = ReadRepoFile("src", "VeloFile.App", "App.xaml");

        StringAssert.Contains(appXaml, "Source=\"Resources/Components/VeloFile.Sidebar.xaml\"");
        Assert.IsTrue(
            RepoFileExists("src/VeloFile.App/Resources/Components/VeloFile.Sidebar.xaml"),
            "Missing sidebar component resource dictionary.");
    }

    [TestMethod]
    public void Sidebar_dictionary_exposes_navigation_first_resources()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.Sidebar.xaml");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfSidebarSurfaceBrush\"",
            "x:Key=\"VfSidebarSectionHeaderStyle\"",
            "x:Key=\"VfSidebarNavigationListStyle\"",
            "x:Key=\"VfSidebarItemContainerStyle\"",
            "x:Key=\"VfSidebarItemTextStyle\"",
            "x:Key=\"VfSidebarSelectedItemBackgroundBrush\"",
            "x:Key=\"VfSidebarHoverItemBackgroundBrush\"",
            "x:Key=\"VfSidebarFocusBrush\"",
            "x:Key=\"VfSidebarDisabledOpacity\"",
            "x:Key=\"VfSidebarSecondarySectionStyle\"",
            "x:Key=\"VfSidebarSecondaryControlRowStyle\"",
            "x:Key=\"VfSidebarToggleStyle\"",
            "x:Key=\"VfSidebarTerminalPickerStyle\""
        })
        {
            StringAssert.Contains(xaml, requiredResource);
        }
    }

    [TestMethod]
    public void Main_window_sidebar_presents_navigation_before_secondary_controls_and_preserves_routes()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var codeBehind = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml.cs");
        var region = ExtractScopeRegion(xaml, "shell-sidebar");

        var locationsIndex = region.IndexOf("x:Name=\"SidebarLocationsList\"", StringComparison.Ordinal);
        var visibilityIndex = region.IndexOf("x:Name=\"VisibilityControls\"", StringComparison.Ordinal);
        var terminalIndex = region.IndexOf("x:Name=\"TerminalTargetComboBox\"", StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, locationsIndex, "Sidebar navigation list is missing.");
        Assert.IsGreaterThanOrEqualTo(0, visibilityIndex, "Sidebar visibility controls are missing.");
        Assert.IsGreaterThanOrEqualTo(0, terminalIndex, "Sidebar terminal selector is missing.");
        Assert.IsLessThan(visibilityIndex, locationsIndex, "Navigation locations must appear before visibility/settings controls.");
        Assert.IsLessThan(terminalIndex, locationsIndex, "Navigation locations must appear before terminal controls.");

        foreach (var requiredReference in new[]
        {
            "Style=\"{StaticResource VfSidebarNavigationListStyle}\"",
            "ItemContainerStyle=\"{StaticResource VfSidebarItemContainerStyle}\"",
            "Style=\"{StaticResource VfSidebarSectionHeaderStyle}\"",
            "Style=\"{StaticResource VfSidebarSecondarySectionStyle}\"",
            "Style=\"{StaticResource VfSidebarSecondaryControlRowStyle}\"",
            "Style=\"{StaticResource VfSidebarToggleStyle}\"",
            "Style=\"{StaticResource VfSidebarTerminalPickerStyle}\""
        })
        {
            StringAssert.Contains(region, requiredReference);
        }

        foreach (var accessibleName in new[]
        {
            "AutomationProperties.Name=\"Navigation locations\"",
            "AutomationProperties.Name=\"Visibility controls\"",
            "AutomationProperties.Name=\"Show hidden files\"",
            "AutomationProperties.Name=\"Show protected operating system files\"",
            "AutomationProperties.Name=\"Show file extensions\"",
            "AutomationProperties.Name=\"Terminal controls\"",
            "AutomationProperties.Name=\"Terminal target\""
        })
        {
            StringAssert.Contains(region, accessibleName);
        }

        foreach (var route in new[]
        {
            "ItemClick=\"SidebarLocationsList_ItemClick\"",
            "Toggled=\"ShowHiddenFilesToggle_Toggled\"",
            "Toggled=\"ShowSystemFilesToggle_Toggled\"",
            "Toggled=\"ShowExtensionsToggle_Toggled\"",
            "DropDownOpened=\"TerminalTargetComboBox_DropDownOpened\"",
            "SelectionChanged=\"TerminalTargetComboBox_SelectionChanged\""
        })
        {
            StringAssert.Contains(region, route);
        }

        StringAssert.Contains(codeBehind, "ViewModel.ActivateSidebarTarget");
        StringAssert.Contains(codeBehind, "ViewModel.SetShowHiddenFiles");
        StringAssert.Contains(codeBehind, "ViewModel.SetShowProtectedOperatingSystemFiles");
        StringAssert.Contains(codeBehind, "ViewModel.SetShowFileExtensions");
        StringAssert.Contains(codeBehind, "ViewModel.SelectTerminalTarget");
    }

    [TestMethod]
    public void Sidebar_scope_contract_is_active_and_declares_required_resources()
    {
        var scopes = ReadRepoFile("docs", "ui", "ui-contract-scopes.v1.json");
        var scope = ExtractJsonObjectContaining(scopes, "\"id\": \"shell-sidebar\"");

        StringAssert.Contains(scope, "\"status\": \"active\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/MainWindow.xaml\"");
        StringAssert.Contains(scope, "\"src/VeloFile.App/Resources/Components/VeloFile.Sidebar.xaml\"");
        StringAssert.Contains(scope, "\"VfSidebarSectionHeaderStyle\"");
        StringAssert.Contains(scope, "\"VfSidebarNavigationListStyle\"");
        StringAssert.Contains(scope, "\"VfSidebarItemContainerStyle\"");
        StringAssert.Contains(scope, "\"VfSidebarToggleStyle\"");
        StringAssert.Contains(scope, "\"VfSidebarTerminalPickerStyle\"");
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
