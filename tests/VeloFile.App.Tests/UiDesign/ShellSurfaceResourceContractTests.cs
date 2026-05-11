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
            "x:Key=\"VfShellRootBackgroundBrush\"",
            "x:Key=\"VfShellChromeBackgroundBrush\"",
            "x:Key=\"VfShellSidebarBackgroundBrush\"",
            "x:Key=\"VfShellContentBackgroundBrush\"",
            "x:Key=\"VfShellElevatedBackgroundBrush\"",
            "x:Key=\"VfShellTextPrimaryBrush\"",
            "x:Key=\"VfShellTextSecondaryBrush\"",
            "x:Key=\"VfShellTextMutedBrush\"",
            "x:Key=\"VfShellTextDisabledBrush\"",
            "x:Key=\"VfShellControlBackgroundBrush\"",
            "x:Key=\"VfShellControlForegroundBrush\"",
            "x:Key=\"VfShellControlBorderBrush\"",
            "x:Key=\"VfShellControlHoverBackgroundBrush\"",
            "x:Key=\"VfShellControlDisabledBackgroundBrush\"",
            "x:Key=\"VfShellControlDisabledForegroundBrush\"",
            "x:Key=\"VfShellAppRootStyle\"",
            "x:Key=\"VfShellChromeStyle\"",
            "x:Key=\"VfShellSidebarStyle\"",
            "x:Key=\"VfShellContentStyle\"",
            "x:Key=\"VfShellCommandBandContainerStyle\"",
            "x:Key=\"VfShellStatusContainerStyle\"",
            "x:Key=\"VfShellPreviewContainerStyle\"",
            "x:Key=\"VfShellButtonStyle\"",
            "x:Key=\"VfShellTextBoxStyle\"",
            "x:Key=\"VfShellComboBoxStyle\"",
            "x:Key=\"VfShellToggleSwitchStyle\"",
            "x:Key=\"VfShellRadioButtonStyle\"",
            "x:Key=\"VfShellSeparatorBrush\"",
            "x:Key=\"VfShellFocusThickness\"",
            "VfBrushSurfaceApp",
            "VfBrushSurfaceChrome",
            "VfBrushSurfaceSidebar",
            "VfBrushSurfaceContent",
            "VfBrushSurfaceElevated",
            "VfBrushBorderSubtle",
            "VfBrushTextPrimary",
            "VfBrushTextSecondary",
            "VfBrushAccent",
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
    public void Shell_token_pairs_meet_readability_contrast_targets()
    {
        var colors = ReadTokenColors();

        AssertContrastAtLeast(colors, "VfColorTextPrimary", "VfColorSurfaceSidebar", 4.5);
        AssertContrastAtLeast(colors, "VfColorTextSecondary", "VfColorSurfaceSidebar", 4.5);
        AssertContrastAtLeast(colors, "VfColorTextPrimary", "VfColorSurfaceContent", 4.5);
        AssertContrastAtLeast(colors, "VfColorTextMuted", "VfColorSurfaceContent", 4.5);
        AssertContrastAtLeast(colors, "VfColorTextPrimary", "VfColorSurfaceInput", 4.5);
        AssertContrastAtLeast(colors, "VfColorTextFaint", "VfColorSurfaceInput", 3.0);
        AssertContrastAtLeast(colors, "VfColorAccent", "VfColorSurfaceContent", 3.0);
    }

    [TestMethod]
    public void Main_window_shell_controls_do_not_depend_on_raw_light_theme_defaults()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");

        foreach (var rawDefault in new[]
        {
            "SystemControlForegroundBaseHighBrush",
            "SystemControlForegroundBaseLowBrush",
            "ApplicationPageBackgroundThemeBrush"
        })
        {
            Assert.IsFalse(xaml.Contains(rawDefault, StringComparison.Ordinal), $"MainWindow must not use raw default theme brush '{rawDefault}' in M2 shell evidence.");
        }

        foreach (var requiredReference in new[]
        {
            "VfShellControlForegroundBrush",
            "VfShellButtonStyle",
            "VfShellTextBoxStyle",
            "VfShellComboBoxStyle",
            "VfShellToggleSwitchStyle",
            "VfShellRadioButtonStyle"
        })
        {
            StringAssert.Contains(xaml, requiredReference);
        }
    }

    [TestMethod]
    public void Sidebar_visibility_toggles_hide_visible_on_off_content_and_keep_accessible_names()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");

        foreach (var toggleName in new[]
        {
            "ShowHiddenFilesToggle",
            "ShowSystemFilesToggle",
            "ShowExtensionsToggle"
        })
        {
            var toggle = ExtractNamedElement(xaml, toggleName);
            StringAssert.Contains(toggle, "OnContent=\"\"");
            StringAssert.Contains(toggle, "OffContent=\"\"");
            StringAssert.Contains(toggle, "Style=\"{StaticResource VfShellToggleSwitchStyle}\"");
            StringAssert.Contains(toggle, "AutomationProperties.Name=");
            Assert.IsFalse(toggle.Contains("OnContent=\"On\"", StringComparison.Ordinal));
            Assert.IsFalse(toggle.Contains("OffContent=\"Off\"", StringComparison.Ordinal));
            Assert.IsFalse(toggle.Contains("开", StringComparison.Ordinal));
            Assert.IsFalse(toggle.Contains("关", StringComparison.Ordinal));
        }
    }

    [TestMethod]
    public void File_list_default_shell_template_does_not_render_placeholder_extension_chips()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.FileList.xaml");

        Assert.IsFalse(xaml.Contains("ThumbnailDisplayText", StringComparison.Ordinal), "M2 shell-default evidence must not render thumbnail text chips.");
        Assert.IsFalse(xaml.Contains("TextTrimming=\"CharacterEllipsis\"", StringComparison.Ordinal) && xaml.Contains("VfFileListIcon", StringComparison.Ordinal), "File-list icon treatment must not ellipsize text chips.");
        foreach (var placeholder in new[] { "P...", "D...", "T...", "B...", "DIR", "..." })
        {
            Assert.IsFalse(xaml.Contains($">{placeholder}<", StringComparison.Ordinal), $"File-list visual template must not include placeholder chip '{placeholder}'.");
            Assert.IsFalse(xaml.Contains($"Text=\"{placeholder}\"", StringComparison.Ordinal), $"File-list visual template must not include placeholder chip '{placeholder}'.");
        }

        StringAssert.Contains(xaml, "x:Key=\"VfFileListIconContainerStyle\"");
        StringAssert.Contains(xaml, "x:Key=\"VfFileListIconPathStyle\"");
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

    private static Dictionary<string, string> ReadTokenColors()
    {
        var tokenXaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Tokens", "VeloFile.Colors.xaml");
        var colors = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(tokenXaml, "<Color x:Key=\"(?<key>[^\"]+)\">(?<value>#[0-9A-Fa-f]{6,8})</Color>"))
        {
            colors.Add(match.Groups["key"].Value, match.Groups["value"].Value);
        }

        return colors;
    }

    private static void AssertContrastAtLeast(Dictionary<string, string> colors, string foregroundKey, string backgroundKey, double minimum)
    {
        Assert.IsTrue(colors.TryGetValue(foregroundKey, out var foreground), $"Missing color token '{foregroundKey}'.");
        Assert.IsTrue(colors.TryGetValue(backgroundKey, out var background), $"Missing color token '{backgroundKey}'.");
        var ratio = ContrastRatio(foreground, background);
        Assert.IsGreaterThanOrEqualTo(minimum, ratio, $"{foregroundKey} on {backgroundKey} contrast was {ratio:0.00}; expected at least {minimum:0.0}.");
    }

    private static double ContrastRatio(string foreground, string background)
    {
        static double Channel(int value)
        {
            var normalized = value / 255.0;
            return normalized <= 0.03928 ? normalized / 12.92 : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        static double Luminance(string hex)
        {
            var value = hex.TrimStart('#');
            if (value.Length == 8)
            {
                value = value[2..];
            }

            var r = Convert.ToInt32(value[..2], 16);
            var g = Convert.ToInt32(value.Substring(2, 2), 16);
            var b = Convert.ToInt32(value.Substring(4, 2), 16);
            return 0.2126 * Channel(r) + 0.7152 * Channel(g) + 0.0722 * Channel(b);
        }

        var foregroundLuminance = Luminance(foreground);
        var backgroundLuminance = Luminance(background);
        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static string ExtractNamedElement(string xaml, string name)
    {
        var nameIndex = xaml.IndexOf($"x:Name=\"{name}\"", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, nameIndex, $"Missing named element '{name}'.");

        var elementStart = xaml.LastIndexOf('<', nameIndex);
        Assert.IsGreaterThanOrEqualTo(0, elementStart, $"Could not find element start for '{name}'.");

        var elementEnd = xaml.IndexOf("/>", nameIndex, StringComparison.Ordinal);
        Assert.IsGreaterThan(nameIndex, elementEnd, $"Expected '{name}' to be a self-closing element.");

        return xaml.Substring(elementStart, elementEnd - elementStart + 2);
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
