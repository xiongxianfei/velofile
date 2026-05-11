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
    public void Empty_folder_fixture_contains_no_rows()
    {
        var fixture = UiFixtureRegistry.GetFixture("file-list-empty-folder");

        Assert.IsNotNull(fixture);
        Assert.HasCount(0, fixture.Rows);
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
}
