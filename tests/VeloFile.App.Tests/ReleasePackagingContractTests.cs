using System.Diagnostics;
using System.Xml.Linq;

namespace VeloFile.App.Tests;

[TestClass]
[TestCategory("Release")]
public sealed class ReleasePackagingContractTests
{
    [TestMethod]
    public void M16_app_project_declares_msix_package_manifest_without_explorer_replacement_or_file_associations()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = repoRoot.Combine("src", "VeloFile.App", "VeloFile.App.csproj").FullName;
        var packageManifestPath = repoRoot.Combine("src", "VeloFile.App", "Package.appxmanifest").FullName;

        Assert.IsTrue(File.Exists(packageManifestPath), "M16 must add the MSIX package manifest.");

        var project = XDocument.Load(projectPath);
        Assert.AreEqual("true", ValueOf(project, "EnableMsixTooling"));
        Assert.AreEqual("Package.appxmanifest", ValueOf(project, "AppxManifest"));
        Assert.AreEqual("VeloFile", ValueOf(project, "PackageDisplayName"));
        Assert.AreEqual("VeloFile", ValueOf(project, "PackagePublisherDisplayName"));

        var manifest = XDocument.Load(packageManifestPath);
        var manifestText = File.ReadAllText(packageManifestPath);
        XNamespace packageNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
        XNamespace uapNs = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
        XNamespace desktopNs = "http://schemas.microsoft.com/appx/manifest/desktop/windows10";

        var identity = manifest.Root!.Element(packageNs + "Identity")!;
        Assert.AreEqual("VeloFile", (string?)identity.Attribute("Name"));
        Assert.AreEqual("0.1.0.0", (string?)identity.Attribute("Version"));
        StringAssert.StartsWith((string?)identity.Attribute("Publisher") ?? "", "CN=");

        var application = manifest.Root
            .Element(packageNs + "Applications")!
            .Element(packageNs + "Application")!;
        Assert.AreEqual("VeloFile", (string?)application.Attribute("Id"));
        Assert.AreEqual("$targetnametoken$.exe", (string?)application.Attribute("Executable"));
        Assert.AreEqual("$targetentrypoint$", (string?)application.Attribute("EntryPoint"));
        Assert.AreEqual("VeloFile", (string?)application.Element(uapNs + "VisualElements")!.Attribute("DisplayName"));

        Assert.IsNull(manifest.Descendants(uapNs + "FileTypeAssociation").FirstOrDefault(), "VeloFile V1 must not take ownership of system file associations.");
        Assert.IsNull(manifest.Descendants(desktopNs + "Extension").FirstOrDefault(extension =>
            string.Equals((string?)extension.Attribute("Category"), "windows.fileExplorerContextMenus", StringComparison.OrdinalIgnoreCase)),
            "VeloFile V1 must not register File Explorer context menu extensions.");
        Assert.IsFalse(manifestText.Contains("AppExecutionAlias", StringComparison.OrdinalIgnoreCase), "VeloFile V1 should not publish a global command alias in M16.");
    }

    [TestMethod]
    public void M16_release_scripts_and_workflow_run_product_specific_windows_release_checks()
    {
        var repoRoot = FindRepoRoot();
        var packageScript = File.ReadAllText(repoRoot.Combine("scripts", "package-msix.ps1").FullName);
        var releaseVerifyScript = File.ReadAllText(repoRoot.Combine("scripts", "release-verify.ps1").FullName);
        var releaseVerifyWrapper = File.ReadAllText(repoRoot.Combine("scripts", "release-verify.sh").FullName);
        var workflow = File.ReadAllText(repoRoot.Combine(".github", "workflows", "release.yml").FullName);

        StringAssert.Contains(packageScript, "Package.appxmanifest");
        StringAssert.Contains(packageScript, "dotnet publish");
        StringAssert.Contains(packageScript, "MSIX");
        StringAssert.Contains(packageScript, "makeappx.exe");
        StringAssert.Contains(packageScript, "signtool.exe");
        StringAssert.Contains(packageScript, "SigningThumbprint");
        StringAssert.Contains(packageScript, "Unsigned local packaging creates an unsigned MSIX");

        StringAssert.Contains(releaseVerifyScript, "Package.appxmanifest");
        StringAssert.Contains(releaseVerifyScript, "stable-update-channel.md");
        StringAssert.Contains(releaseVerifyScript, "install-rollback.md");
        StringAssert.Contains(releaseVerifyScript, "v1-release-notes.md");
        StringAssert.Contains(releaseVerifyScript, "differences-from-file-explorer.md");
        StringAssert.Contains(releaseVerifyScript, "release-checklist.md");
        StringAssert.Contains(releaseVerifyScript, "package-msix.ps1");
        Assert.IsFalse(releaseVerifyScript.Contains("Replace this script", StringComparison.OrdinalIgnoreCase));

        StringAssert.Contains(releaseVerifyWrapper, "release-verify.ps1");
        StringAssert.Contains(workflow, "windows-latest");
        StringAssert.Contains(workflow, "shell: pwsh");
        StringAssert.Contains(workflow, "./scripts/release-verify.ps1");
        StringAssert.Contains(workflow, "./scripts/package-msix.ps1");
        StringAssert.Contains(workflow, "artifacts/msix/*.msix");
        Assert.IsFalse(workflow.Contains("ubuntu-latest", StringComparison.OrdinalIgnoreCase), "Release workflow must run Windows packaging checks on Windows.");
    }

    [TestMethod]
    public void M16_release_verify_script_executes_documentation_and_packaging_contract_checks()
    {
        var repoRoot = FindRepoRoot();
        var result = RunPowerShellScript(repoRoot.Combine("scripts", "release-verify.ps1").FullName, "-SkipPublish");

        Assert.AreEqual(0, result.ExitCode, result.AllOutput);
        StringAssert.Contains(result.AllOutput, "Release verification passed.");
    }

    [TestMethod]
    public void M16_release_documentation_covers_extension_display_explorer_differences_and_rollback()
    {
        var repoRoot = FindRepoRoot();
        var releaseNotes = File.ReadAllText(repoRoot.Combine("docs", "release", "v1-release-notes.md").FullName);
        var stableChannel = File.ReadAllText(repoRoot.Combine("docs", "release", "stable-update-channel.md").FullName);
        var rollback = File.ReadAllText(repoRoot.Combine("docs", "release", "install-rollback.md").FullName);
        var checklist = File.ReadAllText(repoRoot.Combine("docs", "release", "release-checklist.md").FullName);
        var differences = File.ReadAllText(repoRoot.Combine("docs", "user", "differences-from-file-explorer.md").FullName);
        var readme = File.ReadAllText(repoRoot.Combine("README.md").FullName);

        foreach (var doc in new[] { releaseNotes, stableChannel, rollback, checklist, differences })
        {
            Assert.IsFalse(doc.Contains("TODO", StringComparison.OrdinalIgnoreCase), "M16 release docs must not leave TODO placeholders.");
        }

        StringAssert.Contains(releaseNotes, "file extensions are shown by default");
        StringAssert.Contains(releaseNotes, "invoice.pdf.exe");
        StringAssert.Contains(releaseNotes, "per-application");
        StringAssert.Contains(releaseNotes, "does not change File Explorer");

        StringAssert.Contains(differences, "Differences from File Explorer");
        StringAssert.Contains(differences, "file extensions");
        StringAssert.Contains(differences, "OS shell extension");
        StringAssert.Contains(differences, "built-in context menu");
        StringAssert.Contains(differences, "does not replace File Explorer");

        foreach (var required in new[] { "published release source", "signing identity", "versioning policy", "update cadence", "rollback", "uninstall" })
        {
            Assert.IsTrue(stableChannel.Contains(required, StringComparison.OrdinalIgnoreCase), $"Stable channel doc must mention {required}.");
        }

        foreach (var required in new[] { "Install", "Launch", "Update", "Rollback", "Uninstall", "Explorer", "file associations" })
        {
            Assert.IsTrue(checklist.Contains(required, StringComparison.OrdinalIgnoreCase), $"Release checklist must mention {required}.");
        }

        StringAssert.Contains(rollback, "Uninstalling VeloFile is the rollback path");
        StringAssert.Contains(rollback, "Explorer remains available");
        StringAssert.Contains(rollback, "system file associations remain owned by Windows and user defaults");

        StringAssert.Contains(readme, "docs/user/differences-from-file-explorer.md");
        StringAssert.Contains(readme, "docs/release/install-rollback.md");
    }

    private static string ValueOf(XContainer project, string elementName)
    {
        return project.Descendants(elementName).Single().Value;
    }

    private static CommandResult RunPowerShellScript(string scriptPath, params string[] arguments)
    {
        var shell = OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh";
        var startInfo = new ProcessStartInfo(shell)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start PowerShell.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new CommandResult(process.ExitCode, stdoutTask.GetAwaiter().GetResult(), stderrTask.GetAwaiter().GetResult());
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

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + StandardError;
    }
}
