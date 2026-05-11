using VeloFile.App.Testing;

namespace VeloFile.App.Tests.UiFixtures;

[TestClass]
[TestCategory("Fixture")]
[TestCategory("UiContracts")]
[TestCategory("Security")]
public sealed class UiFixtureLaunchTests
{
    [TestMethod]
    public void Fixture_launch_rejects_release_or_production_builds_before_rendering()
    {
        var request = UiFixtureLaunchParser.Parse([
            "--test-ui-fixture",
            "file-list-v1"
        ]);

        var result = UiFixtureLaunchGate.Evaluate(
            request,
            new UiFixtureLaunchContext(IsDebugOrTestBuild: false, EnableEnvironmentValue: "1"));

        Assert.AreEqual(UiFixtureLaunchStatus.Rejected, result.Status);
        Assert.AreEqual("fixture-not-available-in-production", result.ReasonCode);
        Assert.IsFalse(result.ShouldLaunchNormalApp);
        Assert.IsFalse(result.ShouldLaunchFixture);
    }

    [TestMethod]
    public void Fixture_launch_rejects_debug_build_without_environment_guard()
    {
        var request = UiFixtureLaunchParser.Parse([
            "--test-ui-fixture",
            "file-list-v1"
        ]);

        var result = UiFixtureLaunchGate.Evaluate(
            request,
            new UiFixtureLaunchContext(IsDebugOrTestBuild: true, EnableEnvironmentValue: null));

        Assert.AreEqual(UiFixtureLaunchStatus.Rejected, result.Status);
        Assert.AreEqual("fixture-env-guard-missing", result.ReasonCode);
        Assert.IsFalse(result.ShouldLaunchNormalApp);
        Assert.IsFalse(result.ShouldLaunchFixture);
    }

    [TestMethod]
    public void Fixture_launch_rejects_unknown_fixture_names()
    {
        var request = UiFixtureLaunchParser.Parse([
            "--test-ui-fixture",
            "not-real"
        ]);

        var result = UiFixtureLaunchGate.Evaluate(
            request,
            new UiFixtureLaunchContext(IsDebugOrTestBuild: true, EnableEnvironmentValue: "1"));

        Assert.AreEqual(UiFixtureLaunchStatus.Rejected, result.Status);
        Assert.AreEqual("fixture-not-allowlisted", result.ReasonCode);
        Assert.IsFalse(result.ShouldLaunchNormalApp);
        Assert.IsFalse(result.ShouldLaunchFixture);
    }

    [TestMethod]
    public void Fixture_launch_accepts_only_allowlisted_fixture_when_guarded()
    {
        var request = UiFixtureLaunchParser.Parse([
            "--test-ui-fixture",
            "file-list-v1",
            "--theme",
            "dark",
            "--density",
            "comfortable",
            "--viewport",
            "1440x900"
        ]);

        var result = UiFixtureLaunchGate.Evaluate(
            request,
            new UiFixtureLaunchContext(IsDebugOrTestBuild: true, EnableEnvironmentValue: "1"));

        Assert.AreEqual(UiFixtureLaunchStatus.Accepted, result.Status);
        Assert.AreEqual("file-list-v1", result.FixtureName);
        Assert.AreEqual("dark", result.Theme);
        Assert.AreEqual("comfortable", result.Density);
        Assert.AreEqual("1440x900", result.Viewport);
        Assert.IsTrue(result.ShouldLaunchFixture);
        Assert.IsFalse(result.ShouldLaunchNormalApp);
    }

    [TestMethod]
    public void Normal_launch_without_fixture_flag_still_launches_normal_app()
    {
        var request = UiFixtureLaunchParser.Parse([]);

        var result = UiFixtureLaunchGate.Evaluate(
            request,
            new UiFixtureLaunchContext(IsDebugOrTestBuild: false, EnableEnvironmentValue: null));

        Assert.AreEqual(UiFixtureLaunchStatus.NotRequested, result.Status);
        Assert.IsTrue(result.ShouldLaunchNormalApp);
        Assert.IsFalse(result.ShouldLaunchFixture);
    }

    [TestMethod]
    public void Fixture_parser_rejects_arbitrary_data_paths_and_unknown_arguments()
    {
        foreach (var args in new[]
        {
            new[] { "--test-ui-fixture", "file-list-v1", "--fixture-data", @"C:\Users\me\real-files" },
            ["--test-ui-fixture", "file-list-v1", @"C:\Users\me\real-files"],
            ["--test-ui-fixture", "file-list-v1", "--unknown", "value"]
        })
        {
            var request = UiFixtureLaunchParser.Parse(args);

            Assert.IsTrue(request.IsRequested);
            Assert.IsNotNull(request.ParseErrorReasonCode);
            StringAssert.Contains(request.ParseErrorReasonCode, "unsupported");
        }
    }

    [TestMethod]
    public void App_startup_wires_fixture_launch_guard_before_normal_window_creation()
    {
        var appSource = ReadRepoFile("src", "VeloFile.App", "App.xaml.cs");
        var compositionRoot = ReadRepoFile("src", "VeloFile.App", "AppCompositionRoot.cs");

        StringAssert.Contains(appSource, "UiFixtureLaunchGate.FromCurrentProcess");
        StringAssert.Contains(appSource, "Environment.Exit");
        StringAssert.Contains(appSource, "CreateFixtureShellViewModel");
        StringAssert.Contains(compositionRoot, "UiFixtureRegistry.CreateFileListV1ViewModel");
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

internal static class UiFixtureDirectoryInfoExtensions
{
    public static FileInfo Combine(this DirectoryInfo directory, params string[] paths)
    {
        return new FileInfo(Path.Combine(new[] { directory.FullName }.Concat(paths).ToArray()));
    }
}
