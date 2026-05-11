using VeloFile.App.Testing;
using VeloFile.App.ViewModels;

namespace VeloFile.App.Tests.UiFixtures;

[TestClass]
[TestCategory("Fixture")]
[TestCategory("UiContracts")]
[TestCategory("Security")]
public sealed class UiFixtureRegistryTests
{
    [TestMethod]
    public void Fixture_registry_exposes_only_hardcoded_first_slice_names()
    {
        CollectionAssert.AreEqual(
            new[] { "file-list-v1", "file-list-empty-folder" },
            UiFixtureRegistry.AllowlistedFixtureNames.ToArray());
    }

    [TestMethod]
    public void File_list_v1_fixture_uses_synthetic_deterministic_rows_only()
    {
        var fixture = UiFixtureRegistry.GetFixture("file-list-v1");

        Assert.IsNotNull(fixture);
        Assert.AreEqual("file-list-v1", fixture.Name);
        Assert.AreEqual("dark", fixture.Theme);
        Assert.AreEqual("comfortable", fixture.Density);
        Assert.AreEqual("1440x900", fixture.Viewport);
        Assert.IsGreaterThanOrEqualTo(10, fixture.Rows.Count);

        foreach (var row in fixture.Rows)
        {
            StringAssert.StartsWith(row.FullPath, @"C:\VeloFileFixture\");
            Assert.IsFalse(row.FullPath.Contains("Users", StringComparison.OrdinalIgnoreCase), row.FullPath);
            Assert.IsFalse(row.FullPath.Contains("xiongxianfei", StringComparison.OrdinalIgnoreCase), row.FullPath);
            Assert.IsFalse(row.FullPath.Contains("Data\\20260428-velofile", StringComparison.OrdinalIgnoreCase), row.FullPath);
        }
    }

    [TestMethod]
    public void File_list_v1_fixture_contains_required_visual_states()
    {
        var fixture = UiFixtureRegistry.GetFixture("file-list-v1");

        Assert.IsNotNull(fixture);
        CollectionAssert.IsSubsetOf(
            new[]
            {
                UiFixtureRowState.Normal,
                UiFixtureRowState.Folder,
                UiFixtureRowState.Selected,
                UiFixtureRowState.Focused,
                UiFixtureRowState.SelectedFocused,
                UiFixtureRowState.MultiSelected,
                UiFixtureRowState.Hidden,
                UiFixtureRowState.ProtectedSystem,
                UiFixtureRowState.ThumbnailFallback,
                UiFixtureRowState.LongName,
                UiFixtureRowState.MetadataHeavy
            },
            fixture.Rows.Select(row => row.State).Distinct().ToArray());
    }

    [TestMethod]
    public void File_list_v1_fixture_exposes_explicit_presentation_targets()
    {
        var fixture = UiFixtureRegistry.GetFixture("file-list-v1");

        Assert.IsNotNull(fixture);
        var rowIds = fixture.Rows.Select(row => row.Id).ToHashSet(StringComparer.Ordinal);
        Assert.IsNotEmpty(fixture.PresentationState.SelectedRowIds);
        Assert.IsNotNull(fixture.PresentationState.FocusedRowId);
        Assert.IsNotNull(fixture.PresentationState.SelectedFocusedRowId);
        Assert.IsGreaterThan(1, fixture.PresentationState.MultiSelectedRowIds.Count);
        Assert.AreEqual(fixture.PresentationState.SelectedFocusedRowId, fixture.PresentationState.FocusedRowId);
        CollectionAssert.Contains(fixture.PresentationState.AllSelectedRowIds.ToArray(), fixture.PresentationState.SelectedFocusedRowId);

        foreach (var targetId in fixture.PresentationState.AllSelectedRowIds
            .Concat([fixture.PresentationState.FocusedRowId, fixture.PresentationState.SelectedFocusedRowId])
            .Where(id => id is not null)
            .Cast<string>())
        {
            CollectionAssert.Contains(rowIds.ToArray(), targetId);
            Assert.IsNotNull(fixture.PresentationState.GetFullPath(targetId), targetId);
        }
    }

    [TestMethod]
    public void Empty_folder_fixture_contains_no_rows()
    {
        var fixture = UiFixtureRegistry.GetFixture("file-list-empty-folder");

        Assert.IsNotNull(fixture);
        Assert.HasCount(0, fixture.Rows);
        Assert.IsFalse(fixture.PresentationState.HasTargets);
        Assert.AreEqual("file-list-empty-folder", fixture.Name);
    }

    [TestMethod]
    public async Task File_list_fixture_view_model_renders_fixture_rows_without_disk_backing()
    {
        var viewModel = UiFixtureRegistry.CreateFileListV1ViewModel();

        await WaitUntilAsync(() => viewModel.FileListRows.Count >= 10);

        Assert.AreEqual(@"C:\VeloFileFixture", viewModel.ActivePath);
        Assert.IsTrue(viewModel.FileListRows.Any(row => row.DisplayName == "Document.pdf"));
        Assert.IsTrue(viewModel.FileListRows.Any(row => row.VisibilityKind == FileListRowVisibilityKind.Hidden));
        Assert.IsTrue(viewModel.FileListRows.Any(row => row.VisibilityKind == FileListRowVisibilityKind.ProtectedSystem));
        Assert.IsTrue(viewModel.FileListRows.Any(row => row.ThumbnailDisplayText == "PDF"));
        Assert.IsTrue(viewModel.FileListRows.Any(row => row.DisplayName.Contains("Very long filename", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task Fixture_shell_state_preserves_presentation_targets_after_view_model_creation()
    {
        var shellState = UiFixtureRegistry.CreateFileListV1ShellState();

        await WaitUntilAsync(() => shellState.ViewModel.FileListRows.Count >= 10);

        var plan = UiFixturePresentationPlanner.Create(shellState.PresentationState, shellState.ViewModel.FileListRows);

        Assert.IsGreaterThanOrEqualTo(4, plan.SelectedRows.Count);
        Assert.IsNotNull(plan.FocusedRow);
        Assert.IsNotNull(plan.SelectedFocusedRow);
        Assert.AreSame(plan.FocusedRow, plan.SelectedFocusedRow);
        Assert.IsTrue(plan.SelectedRows.Contains(plan.SelectedFocusedRow));
        CollectionAssert.IsSubsetOf(
            new[] { "selected-report.docx", "selected-focused.xlsx", "multi-selected-a.txt", "multi-selected-b.txt" },
            plan.SelectedRows.Select(row => row.DisplayName).ToArray());
    }

    [TestMethod]
    public void App_startup_path_applies_fixture_presentation_to_file_list_surface()
    {
        var appSource = ReadRepoFile("src", "VeloFile.App", "App.xaml.cs");
        var mainWindowSource = ReadRepoFile("src", "VeloFile.App", "MainWindow.xaml.cs");

        StringAssert.Contains(appSource, "fixtureShellState?.PresentationState");
        StringAssert.Contains(mainWindowSource, "ApplyUiFixturePresentationState");
        StringAssert.Contains(mainWindowSource, "FileListSurface.SelectedItems.Add");
        StringAssert.Contains(mainWindowSource, "FileListSelectionMapper.ToListedFileItems");
        StringAssert.Contains(mainWindowSource, "ContainerFromItem");
        StringAssert.Contains(mainWindowSource, "Focus(FocusState.Keyboard)");
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        while (!condition())
        {
            if (cts.IsCancellationRequested)
            {
                Assert.Fail("Condition was not reached before timeout.");
            }

            await Task.Delay(20);
        }
    }

    private static string ReadRepoFile(params string[] relativePath)
    {
        return File.ReadAllText(FindRepoRoot().Combine(relativePath).FullName);
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
