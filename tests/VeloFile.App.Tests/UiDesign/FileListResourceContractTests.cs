using VeloFile.App.ViewModels;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;

namespace VeloFile.App.Tests.UiDesign;

[TestClass]
[TestCategory("UiContracts")]
[TestCategory("AppShellContract")]
[TestCategory("Accessibility")]
public sealed class FileListResourceContractTests
{
    [TestMethod]
    public void App_resources_merge_first_slice_token_and_file_list_dictionaries()
    {
        var appXaml = ReadRepoFile("src", "VeloFile.App", "App.xaml");

        foreach (var expectedSource in new[]
        {
            "Resources/Tokens/VeloFile.Colors.xaml",
            "Resources/Tokens/VeloFile.Typography.xaml",
            "Resources/Tokens/VeloFile.Spacing.xaml",
            "Resources/Tokens/VeloFile.Sizing.xaml",
            "Resources/Tokens/VeloFile.Radius.xaml",
            "Resources/Tokens/VeloFile.Focus.xaml",
            "Resources/Tokens/VeloFile.Density.xaml",
            "Resources/Tokens/VeloFile.State.xaml",
            "Resources/Tokens/VeloFile.Motion.xaml",
            "Resources/Components/VeloFile.FileList.xaml"
        })
        {
            StringAssert.Contains(appXaml, $"Source=\"{expectedSource}\"");
            Assert.IsTrue(RepoFileExists("src/VeloFile.App/" + expectedSource), $"Missing merged resource dictionary '{expectedSource}'.");
        }
    }

    [TestMethod]
    public void File_list_component_dictionary_exposes_named_row_resources()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.FileList.xaml");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfFileListRowTemplate\"",
            "x:Key=\"VfFileListItemContainerStyle\"",
            "x:Key=\"VfFileListRowNameTextStyle\"",
            "x:Key=\"VfFileListRowMetadataTextStyle\"",
            "VfFileListRowHeight",
            "VfFileListRowPadding",
            "VfSizeIconMd",
            "VfSizeIconSm",
            "VfBrushTextPrimary",
            "VfBrushTextMuted"
        })
        {
            StringAssert.Contains(xaml, requiredResource);
        }
    }

    [TestMethod]
    public void File_list_item_style_consumes_named_selection_and_focus_resources()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.FileList.xaml");
        var mainWindowXaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var fileListRegion = ExtractScopeRegion(mainWindowXaml, "file-list-first-slice");

        foreach (var requiredResource in new[]
        {
            "x:Key=\"VfFileListRowBackground\"",
            "x:Key=\"VfFileListRowHoverBackground\"",
            "x:Key=\"VfFileListRowSelectedBackground\"",
            "x:Key=\"VfFileListRowFocusedBorderBrush\"",
            "x:Key=\"VfFileListRowFocusedBorderThickness\"",
            "x:Key=\"VfFileListRowSelectedFocusedBorderBrush\"",
            "x:Key=\"VfFileListRowSelectedFocusedBorderThickness\"",
            "x:Key=\"VfFileListRowHiddenOpacity\"",
            "x:Key=\"VfFileListRowProtectedOpacity\""
        })
        {
            StringAssert.Contains(xaml, requiredResource);
        }

        StringAssert.Contains(fileListRegion, "x:Key=\"ListViewItemBackgroundSelected\"");
        StringAssert.Contains(fileListRegion, "Source={StaticResource VfFileListRowSelectedBackground}");
        StringAssert.Contains(xaml, "Color=\"{StaticResource VfColorSurfaceSelected}\"");
        StringAssert.Contains(xaml, "Color=\"{StaticResource VfColorAccentLine}\"");
        StringAssert.Contains(xaml, "Value=\"{StaticResource VfFileListRowFocusedBorderBrush}\"");
        StringAssert.Contains(xaml, "Value=\"{StaticResource VfFileListRowFocusedBorderThickness}\"");
    }

    [TestMethod]
    public void File_list_selected_and_focused_states_are_not_text_color_only()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.FileList.xaml");
        var mainWindowXaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var fileListRegion = ExtractScopeRegion(mainWindowXaml, "file-list-first-slice");

        StringAssert.Contains(fileListRegion, "ListViewItemBackgroundSelected");
        StringAssert.Contains(xaml, "VfFileListRowSelectedBackground");
        StringAssert.Contains(xaml, "FocusVisualPrimaryBrush");
        StringAssert.Contains(xaml, "VfFileListRowFocusedBorderBrush");
        StringAssert.Contains(xaml, "FocusVisualPrimaryThickness");
        StringAssert.Contains(xaml, "VfFileListRowFocusedBorderThickness");
        Assert.IsFalse(
            xaml.Contains("ForegroundSelected", StringComparison.Ordinal)
                && !xaml.Contains("BackgroundSelected", StringComparison.Ordinal),
            "Selected row distinction must not rely only on text color.");
    }

    [TestMethod]
    public void Main_window_file_list_consumes_named_row_resources_in_scoped_region()
    {
        var xaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var region = ExtractScopeRegion(xaml, "file-list-first-slice");

        StringAssert.Contains(region, "x:Name=\"FileListSurface\"");
        StringAssert.Contains(region, "ItemTemplate=\"{StaticResource VfFileListRowTemplate}\"");
        StringAssert.Contains(region, "ItemContainerStyle=\"{StaticResource VfFileListItemContainerStyle}\"");
        Assert.IsFalse(region.Contains("<ListView.ItemTemplate>", StringComparison.Ordinal));
        Assert.IsFalse(region.Contains("MinHeight=\"32\"", StringComparison.Ordinal));
        Assert.IsFalse(region.Contains("Padding=\"8,4\"", StringComparison.Ordinal));
        Assert.IsFalse(region.Contains("FontSize=\"10\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void File_list_row_template_preserves_existing_row_bindings_and_context_menu_route()
    {
        var mainWindowXaml = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml");
        var fileListXaml = ReadRepoFile("src", "VeloFile.App", "Resources", "Components", "VeloFile.FileList.xaml");

        foreach (var requiredBinding in new[]
        {
            "Opacity=\"{Binding RowOpacity}\"",
            "Text=\"{Binding ThumbnailDisplayText}\"",
            "Text=\"{Binding DisplayName}\"",
            "Text=\"{Binding Kind}\"",
            "Text=\"{Binding LastWriteTimeUtc}\""
        })
        {
            StringAssert.Contains(fileListXaml, requiredBinding);
        }

        StringAssert.Contains(mainWindowXaml, "<ListView.ContextFlyout>");
        StringAssert.Contains(mainWindowXaml, "x:Name=\"BuiltInFileContextMenu\"");
        StringAssert.Contains(mainWindowXaml, "SelectionMode=\"Extended\"");
        StringAssert.Contains(mainWindowXaml, "SelectionChanged=\"FileListSurface_SelectionChanged\"");
        StringAssert.Contains(mainWindowXaml, "DoubleTapped=\"FileListSurface_DoubleTapped\"");
    }

    [TestMethod]
    public void File_list_row_redesign_does_not_introduce_custom_row_control_or_behavior_model()
    {
        var repoRoot = FindRepoRoot();
        var appSource = Directory.GetFiles(repoRoot.Combine("src", "VeloFile.App").FullName, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase));

        foreach (var path in appSource)
        {
            var content = File.ReadAllText(path);
            Assert.IsFalse(content.Contains("VeloFileFileListRowControl", StringComparison.Ordinal), path);
            Assert.IsFalse(content.Contains("FileListRowControl", StringComparison.Ordinal), path);
            Assert.IsFalse(content.Contains("FileListRowBehavior", StringComparison.Ordinal), path);
        }
    }

    [TestMethod]
    public void File_list_row_view_model_keeps_dimmed_and_thumbnail_state_inputs_available()
    {
        var hidden = new FileListRowViewModel(
            CreateItem(isHidden: true, isProtectedOperatingSystemFile: false, isVisuallyDimmed: true),
            ThumbnailState.GenericIcon(ThumbnailArtifact.GenericIcon("TXT"), "fixture"));

        Assert.IsTrue(hidden.IsVisuallyDimmed);
        Assert.IsLessThan(1.0, hidden.RowOpacity);
        Assert.AreEqual("TXT", hidden.ThumbnailDisplayText);

        var directory = new FileListRowViewModel(
            CreateItem(isHidden: false, isProtectedOperatingSystemFile: false, isVisuallyDimmed: false, kind: FileSystemEntryKind.Directory),
            ThumbnailState.NotLoaded);

        Assert.AreEqual(1.0, directory.RowOpacity);
        Assert.AreEqual("DIR", directory.ThumbnailDisplayText);
    }

    private static ListedFileItem CreateItem(
        bool isHidden,
        bool isProtectedOperatingSystemFile,
        bool isVisuallyDimmed,
        FileSystemEntryKind kind = FileSystemEntryKind.File)
    {
        var attributes = FileAttributes.Normal;
        if (isHidden)
        {
            attributes |= FileAttributes.Hidden;
        }

        if (isProtectedOperatingSystemFile)
        {
            attributes |= FileAttributes.System;
        }

        return new ListedFileItem(
            FullPath: @"C:\fixture\Document.txt",
            Name: "Document.txt",
            DisplayName: "Document.txt",
            Kind: kind,
            Length: 128,
            LastWriteTimeUtc: new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            Attributes: attributes,
            IsHidden: isHidden,
            IsProtectedOperatingSystemFile: isProtectedOperatingSystemFile,
            IsVisuallyDimmed: isVisuallyDimmed);
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

internal static class UiDesignDirectoryInfoExtensions
{
    public static FileInfo Combine(this DirectoryInfo directory, params string[] paths)
    {
        return new FileInfo(Path.Combine(new[] { directory.FullName }.Concat(paths).ToArray()));
    }
}
