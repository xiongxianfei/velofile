using System.Xml.Linq;

namespace VeloFile.App.Tests;

[TestClass]
public sealed class AppProjectSmokeTests
{
    [TestMethod]
    public void App_project_is_winui_windows_executable()
    {
        var projectFile = FindRepoRoot().Combine("src", "VeloFile.App", "VeloFile.App.csproj");

        Assert.IsTrue(File.Exists(projectFile.FullName), "The M1 app project must exist.");

        var project = XDocument.Load(projectFile.FullName);

        Assert.AreEqual("WinExe", ValueOf(project, "OutputType"));
        Assert.AreEqual("net8.0-windows10.0.19041.0", ValueOf(project, "TargetFramework"));
        Assert.AreEqual("true", ValueOf(project, "UseWinUI"));
        Assert.AreEqual("app.manifest", ValueOf(project, "ApplicationManifest"));
    }

    [TestMethod]
    public void App_shell_contains_launchable_xaml_surface()
    {
        var appRoot = FindRepoRoot().Combine("src", "VeloFile.App");

        Assert.IsTrue(File.Exists(appRoot.Combine("App.xaml").FullName));
        Assert.IsTrue(File.Exists(appRoot.Combine("App.xaml.cs").FullName));
        Assert.IsTrue(File.Exists(appRoot.Combine("MainWindow.xaml").FullName));
        Assert.IsTrue(File.Exists(appRoot.Combine("MainWindow.xaml.cs").FullName));
    }

    private static string ValueOf(XContainer project, string elementName)
    {
        return project.Descendants(elementName).Single().Value;
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

internal static class DirectoryInfoExtensions
{
    public static DirectoryInfo Combine(this DirectoryInfo directory, params string[] segments)
    {
        return new DirectoryInfo(Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray()));
    }
}
