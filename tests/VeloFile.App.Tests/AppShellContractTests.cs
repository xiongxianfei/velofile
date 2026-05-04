namespace VeloFile.App.Tests;

[TestClass]
[TestCategory("Navigation")]
[TestCategory("Sidebar")]
[TestCategory("Session")]
public sealed class AppShellContractTests
{
    [TestMethod]
    public void Main_window_shell_exposes_navigation_sidebar_tabs_breadcrumb_and_file_view_regions()
    {
        var xaml = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);

        StringAssert.Contains(xaml, "x:Name=\"TabStrip\"");
        StringAssert.Contains(xaml, "x:Name=\"SidebarPane\"");
        StringAssert.Contains(xaml, "x:Name=\"BreadcrumbPathBar\"");
        StringAssert.Contains(xaml, "x:Name=\"RawPathBox\"");
        StringAssert.Contains(xaml, "x:Name=\"FileViewModeSelector\"");
        StringAssert.Contains(xaml, "x:Name=\"FileListSurface\"");
        StringAssert.Contains(xaml, "x:Name=\"MissingLocationState\"");
        StringAssert.Contains(xaml, "x:Name=\"VisibilityControls\"");
    }

    [TestMethod]
    public void Main_window_shell_declares_keyboard_paths_for_tabs_navigation_and_visibility()
    {
        var xaml = File.ReadAllText(FindRepoRoot().Combine("src", "VeloFile.App", "MainWindow.xaml").FullName);

        StringAssert.Contains(xaml, "<KeyboardAccelerator");
        StringAssert.Contains(xaml, "Key=\"T\"");
        StringAssert.Contains(xaml, "Key=\"W\"");
        StringAssert.Contains(xaml, "Key=\"Tab\"");
        StringAssert.Contains(xaml, "Key=\"L\"");
        StringAssert.Contains(xaml, "Key=\"P\"");
        StringAssert.Contains(xaml, "AccessKey=\"H\"");
        StringAssert.Contains(xaml, "AccessKey=\"S\"");
        StringAssert.Contains(xaml, "AccessKey=\"E\"");
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
