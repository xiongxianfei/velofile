using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    public void M16_release_workflow_verifies_signed_tag_when_stable_channel_requires_signed_git_tag()
    {
        var repoRoot = FindRepoRoot();
        var stableChannel = File.ReadAllText(repoRoot.Combine("docs", "release", "stable-update-channel.md").FullName);
        var workflow = File.ReadAllText(repoRoot.Combine(".github", "workflows", "release.yml").FullName);
        var verifyTagScriptPath = repoRoot.Combine("scripts", "verify-release-tag.ps1").FullName;

        StringAssert.Contains(stableChannel, "signed Git tag");
        Assert.IsTrue(File.Exists(verifyTagScriptPath), "Release workflow must use a dedicated trusted release-tag verifier.");

        var verifyTagScript = File.ReadAllText(verifyTagScriptPath);

        StringAssert.Contains(workflow, "scripts/verify-release-tag.ps1");
        Assert.IsFalse(workflow.Contains("VerifyStatusFile", StringComparison.OrdinalIgnoreCase), "Production release workflow must not use the verifier status-file test seam.");
        StringAssert.Contains(workflow, "VELOFILE_RELEASE_GPG_PUBLIC_KEYS");
        StringAssert.Contains(workflow, "VELOFILE_RELEASE_GPG_FINGERPRINTS");
        StringAssert.Contains(verifyTagScript, "GNUPGHOME");
        StringAssert.Contains(verifyTagScript, "RUNNER_TEMP");
        StringAssert.Contains(verifyTagScript, "VELOFILE_RELEASE_GPG_PUBLIC_KEYS");
        StringAssert.Contains(verifyTagScript, "gpg --batch --import");
        StringAssert.Contains(verifyTagScript, "git verify-tag --raw");
        StringAssert.Contains(verifyTagScript, "VALIDSIG");
        StringAssert.Contains(verifyTagScript, "VELOFILE_RELEASE_GPG_FINGERPRINTS");
        StringAssert.Contains(verifyTagScript, "not in the allowed release-key set");
        StringAssert.Contains(verifyTagScript, "^[0-9A-F]{40}$");

        var verifyTagIndex = workflow.IndexOf("scripts/verify-release-tag.ps1", StringComparison.OrdinalIgnoreCase);
        var packageIndex = workflow.IndexOf("./scripts/package-msix.ps1", StringComparison.OrdinalIgnoreCase);
        var releaseIndex = workflow.IndexOf("gh release create", StringComparison.OrdinalIgnoreCase);

        Assert.IsTrue(verifyTagIndex >= 0, "Release workflow must cryptographically verify the signed tag with the trusted release-key verifier.");
        Assert.IsTrue(packageIndex > verifyTagIndex, "Trusted signed tag verification must run before packaging.");
        Assert.IsTrue(releaseIndex > verifyTagIndex, "Trusted signed tag verification must run before creating the GitHub release.");
        Assert.IsTrue(workflow.Contains("fetch-depth: 0", StringComparison.OrdinalIgnoreCase), "Release workflow must fetch full tag history.");
    }

    [TestMethod]
    [DataRow("[GNUPG:] VALIDSIG AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA 2026-05-06 1746547200 0 4 0 1 10 00 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", true)]
    [DataRow("[GNUPG:] VALIDSIG BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB 2026-05-06 1746547200 0 4 0 1 10 00 BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", false)]
    [DataRow("[GNUPG:] NEWSIG\n[GNUPG:] GOODSIG AAAAAAAAAAAAAAAA VeloFile Release", false)]
    [DataRow("[GNUPG:] NO_PUBKEY AAAAAAAAAAAAAAAA\n[GNUPG:] ERRSIG AAAAAAAAAAAAAAAA 1 10 00 1746547200 9 AAAAAAAAAAAAAAAA", false)]
    [DataRow("", false)]
    public void M16_release_tag_verifier_accepts_only_validsig_from_allowed_full_fingerprint(
        string verifyStatus,
        bool expectedSuccess)
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = repoRoot.Combine("scripts", "verify-release-tag.ps1").FullName;
        var statusPath = Path.Combine(Path.GetTempPath(), "velofile-tag-status-" + Guid.NewGuid().ToString("N") + ".txt");

        try
        {
            File.WriteAllText(statusPath, verifyStatus);
            var result = RunPowerShellScript(
                scriptPath,
                new Dictionary<string, string>
                {
                    ["VELOFILE_RELEASE_GPG_FINGERPRINTS"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                },
                "-VerifyStatusFile",
                statusPath);

            if (expectedSuccess)
            {
                Assert.AreEqual(0, result.ExitCode, result.AllOutput);
                StringAssert.Contains(result.AllOutput, "Release tag signature verified with trusted fingerprint AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            }
            else
            {
                Assert.AreNotEqual(0, result.ExitCode, "Untrusted, unsigned, lightweight, and unverifiable tag status fixtures must fail.");
            }
        }
        finally
        {
            if (File.Exists(statusPath))
            {
                File.Delete(statusPath);
            }
        }
    }

    [TestMethod]
    public void M16_package_script_dry_run_maps_package_architecture_to_publish_runtime_identifier()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = "artifacts/msix-dry-run-" + Guid.NewGuid().ToString("N");
        var outputRootPath = repoRoot.Combine(outputRoot.Split('/')).FullName;

        try
        {
            foreach (var testCase in new[]
            {
                new PackageArchitectureCase("x64", "win-x64", "x64"),
                new PackageArchitectureCase("x86", "win-x86", "x86"),
                new PackageArchitectureCase("ARM64", "win-arm64", "arm64")
            })
            {
                var result = RunPowerShellScript(
                    repoRoot.Combine("scripts", "package-msix.ps1").FullName,
                    "-DryRun",
                    "-SkipPublish",
                    "-Platform",
                    testCase.Platform,
                    "-OutputRoot",
                    outputRoot);

                Assert.AreEqual(0, result.ExitCode, result.AllOutput);

                var metadataPath = Path.Combine(outputRootPath, "package-metadata.json");
                using var metadata = JsonDocument.Parse(File.ReadAllText(metadataPath));
                var root = metadata.RootElement;

                Assert.IsTrue(root.GetProperty("dryRun").GetBoolean());
                Assert.AreEqual(testCase.Platform, root.GetProperty("platform").GetString());
                Assert.AreEqual(testCase.RuntimeIdentifier, root.GetProperty("runtimeIdentifier").GetString());
                Assert.AreEqual(testCase.ManifestArchitecture, root.GetProperty("manifestArchitecture").GetString());
                StringAssert.Contains(root.GetProperty("packagePath").GetString()!, "_" + testCase.Platform + ".msix");
                StringAssert.Contains(root.GetProperty("publishPath").GetString()!, "publish-" + testCase.Platform);

                var publishCommand = root.GetProperty("dryRunCommands").EnumerateArray().Single(command =>
                    command.GetProperty("name").GetString() == "dotnet publish");
                Assert.IsTrue(publishCommand.GetProperty("arguments").EnumerateArray().Any(argument =>
                    argument.GetString() == "-r" || argument.GetString() == "--runtime"));
                Assert.IsTrue(publishCommand.GetProperty("arguments").EnumerateArray().Any(argument =>
                    argument.GetString() == testCase.RuntimeIdentifier));

                var generatedManifestPath = Path.Combine(outputRootPath, "publish-" + testCase.Platform, "AppxManifest.xml");
                var generatedManifest = XDocument.Load(generatedManifestPath);
                XNamespace packageNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                Assert.AreEqual(
                    testCase.ManifestArchitecture,
                    (string?)generatedManifest.Root!.Element(packageNs + "Identity")!.Attribute("ProcessorArchitecture"));
            }
        }
        finally
        {
            if (Directory.Exists(outputRootPath))
            {
                Directory.Delete(outputRootPath, recursive: true);
            }
        }
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

    [TestMethod]
    public void M16_change_metadata_links_resolve_to_tracked_docs()
    {
        var repoRoot = FindRepoRoot();
        var changePath = repoRoot.Combine("docs", "changes", "2026-05-06-m16-msix-release-docs", "change.yaml").FullName;
        var changeText = File.ReadAllText(changePath);

        foreach (var link in ReadYamlList(changeText, "architecture").Concat(ReadYamlList(changeText, "files")))
        {
            var parts = link.Split('#', 2);
            var relativePath = parts[0];
            var targetPath = repoRoot.Combine(relativePath.Split('/')).FullName;

            Assert.IsTrue(File.Exists(targetPath), $"Change metadata reference must exist: {link}");

            if (parts.Length == 2)
            {
                var anchors = MarkdownAnchors(File.ReadAllLines(targetPath));
                Assert.IsTrue(anchors.Contains(parts[1], StringComparer.OrdinalIgnoreCase), $"Change metadata anchor must exist: {link}");
            }
        }
    }

    private static string ValueOf(XContainer project, string elementName)
    {
        return project.Descendants(elementName).Single().Value;
    }

    private static IReadOnlyList<string> ReadYamlList(string yaml, string key)
    {
        var values = new List<string>();
        var inSection = false;

        foreach (var rawLine in yaml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            if (Regex.IsMatch(rawLine, @"^\S"))
            {
                inSection = rawLine.Equals(key + ":", StringComparison.Ordinal);
                continue;
            }

            if (!inSection)
            {
                continue;
            }

            var match = Regex.Match(rawLine, @"^\s+-\s+(.+?)\s*$");
            if (match.Success)
            {
                values.Add(match.Groups[1].Value);
            }
        }

        return values;
    }

    private static ISet<string> MarkdownAnchors(IEnumerable<string> lines)
    {
        var anchors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"^#{1,6}\s+(.+?)\s*$");
            if (!match.Success)
            {
                continue;
            }

            var heading = match.Groups[1].Value.Trim().ToLowerInvariant();
            var anchor = Regex.Replace(heading, @"[^\w\s-]", "");
            anchor = Regex.Replace(anchor, @"\s+", "-").Trim('-');
            if (anchor.Length > 0)
            {
                anchors.Add(anchor);
            }
        }

        return anchors;
    }

    private static CommandResult RunPowerShellScript(string scriptPath, params string[] arguments)
    {
        return RunPowerShellScript(scriptPath, null, arguments);
    }

    private static CommandResult RunPowerShellScript(
        string scriptPath,
        IReadOnlyDictionary<string, string>? environment,
        params string[] arguments)
    {
        var shell = OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh";
        var startInfo = new ProcessStartInfo(shell)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        if (environment is not null)
        {
            foreach (var (key, value) in environment)
            {
                startInfo.Environment[key] = value;
            }
        }

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

    private sealed record PackageArchitectureCase(string Platform, string RuntimeIdentifier, string ManifestArchitecture);
}
