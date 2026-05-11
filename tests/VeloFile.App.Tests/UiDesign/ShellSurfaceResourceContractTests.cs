namespace VeloFile.App.Tests.UiDesign;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("AppShellContract")]
[TestCategory("Accessibility")]
public sealed class ShellSurfaceResourceContractTests
{
    [TestMethod]
    public void App_resources_merge_shell_surface_dictionary_after_tokens()
    {
        var appXaml = ReadRepoFile("src", "VeloFile.App", "App.xaml");
        var shellDictionary = "Resources/Components/VeloFile.Shell.xaml";

        StringAssert.Contains(appXaml, $"Source=\"{shellDictionary}\"");
        Assert.IsTrue(RepoFileExists("src/VeloFile.App/" + shellDictionary), $"Missing merged resource dictionary '{shellDictionary}'.");

        var stateTokenIndex = appXaml.IndexOf("Resources/Tokens/VeloFile.State.xaml", StringComparison.Ordinal);
        var shellDictionaryIndex = appXaml.IndexOf(shellDictionary, StringComparison.Ordinal);
        var fileListDictionaryIndex = appXaml.IndexOf("Resources/Components/VeloFile.FileList.xaml", StringComparison.Ordinal);

        Assert.IsGreaterThan(-1, stateTokenIndex);
        Assert.IsGreaterThan(stateTokenIndex, shellDictionaryIndex, "Shell surface resources must load after token dictionaries.");
        Assert.IsGreaterThan(shellDictionaryIndex, fileListDictionaryIndex, "File-list resources must remain downstream of shell foundation resources.");
    }

    [TestMethod]
    public void Shell_surface_dictionary_exposes_tokenized_foundation_resources()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.Shell.xaml");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfShellAppRootStyle\"",
            "x:Key=\"VfShellChromeStyle\"",
            "x:Key=\"VfShellSidebarStyle\"",
            "x:Key=\"VfShellContentStyle\"",
            "x:Key=\"VfShellCommandBandContainerStyle\"",
            "x:Key=\"VfShellStatusContainerStyle\"",
            "x:Key=\"VfShellPreviewContainerStyle\"",
            "x:Key=\"VfShellSeparatorBrush\"",
            "VfBrushSurfaceApp",
            "VfBrushSurfaceChrome",
            "VfBrushSurfaceSidebar",
            "VfBrushSurfaceContent",
            "VfBrushSurfaceElevated",
            "VfBrushBorderSubtle",
            "VfBrushTextPrimary",
            "VfBrushTextSecondary",
            "VfFocusBrush",
            "VfBrushAccent",
            "VfBrushDanger",
            "VfBrushWarning",
            "VfBrushSuccess"
        })
        {
            StringAssert.Contains(xaml, requiredResource);
        }

        AssertNoRawHexColor(xaml, "VeloFile.Shell.xaml");
        Assert.IsFalse(xaml.Contains("ApplicationPageBackgroundThemeBrush", StringComparison.Ordinal));
        Assert.IsFalse(xaml.Contains("SystemControlForegroundBaseLowBrush", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Main_window_consumes_shell_surface_resources_in_governed_scope()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var region = ExtractScopeRegion(xaml, "shell-surface-foundation");

        foreach (var requiredReference in new[]
        {
            "VfShellAppRootStyle",
            "VfShellChromeStyle",
            "VfShellSidebarStyle",
            "VfShellContentStyle",
            "VfShellCommandBandContainerStyle",
            "VfShellStatusContainerStyle",
            "VfShellPreviewContainerStyle"
        })
        {
            StringAssert.Contains(region, requiredReference);
            StringAssert.Contains(xaml, requiredReference);
        }

        Assert.IsFalse(region.Contains("ApplicationPageBackgroundThemeBrush", StringComparison.Ordinal));
        Assert.IsFalse(region.Contains("SystemControlForegroundBaseLowBrush", StringComparison.Ordinal));
        AssertNoRawHexColor(region, "shell-surface-foundation scope");
    }

    [TestMethod]
    public void Shell_surface_foundation_preserves_existing_v1_routes()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");

        foreach (var route in new[]
        {
            "x:Name=\"TabList\"",
            "x:Name=\"NavigationButtons\"",
            "x:Name=\"BreadcrumbPathBar\"",
            "x:Name=\"RawPathBox\"",
            "x:Name=\"SidebarLocationsList\"",
            "x:Name=\"ShowHiddenFilesToggle\"",
            "x:Name=\"ShowSystemFilesToggle\"",
            "x:Name=\"ShowExtensionsToggle\"",
            "x:Name=\"TerminalTargetComboBox\"",
            "x:Name=\"FileListSurface\"",
            "x:Name=\"CurrentFolderFilterBox\"",
            "x:Name=\"RecursiveSearchBox\"",
            "x:Name=\"BuiltInFileContextMenu\"",
            "x:Name=\"CancelFileOperationButton\"",
            "x:Name=\"PreviewPane\"",
            "x:Name=\"PreviewMetadataList\""
        })
        {
            StringAssert.Contains(xaml, route);
        }

        foreach (var handler in new[]
        {
            "BackButton_Click",
            "ForwardButton_Click",
            "UpButton_Click",
            "RefreshButton_Click",
            "SidebarLocationsList_ItemClick",
            "FileListSurface_SelectionChanged",
            "FileListSurface_DoubleTapped",
            "CurrentFolderFilterBox_TextChanged",
            "RecursiveSearchButton_Click",
            "CancelSearchButton_Click",
            "OpenTerminalMenuItem_Click",
            "CancelFileOperationButton_Click",
            "PdfNextPageButton_Click"
        })
        {
            StringAssert.Contains(xaml, handler);
        }
    }

    private static void AssertNoRawHexColor(string xaml, string surface)
    {
        Assert.IsFalse(xaml.Contains("#", StringComparison.Ordinal), $"{surface} must use VeloFile resource references instead of raw hex colors.");
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
        Assert.IsGreaterThanOrEqualTo(0, startIndex, $"Missing scope start marker '{startMarker}'.");

        var endIndex = xaml.IndexOf(endMarker, startIndex, StringComparison.Ordinal);
        Assert.IsGreaterThan(startIndex, endIndex, $"Missing scope end marker '{endMarker}'.");

        return xaml.Substring(startIndex, endIndex - startIndex + endMarker.Length);
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
