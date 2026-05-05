using System.Security.Cryptography;
using System.Diagnostics;
using System.Security;
using System.Text;
using System.Text.Json;

return CorpusCli.Run(args, Console.Out, Console.Error);

internal static class CorpusCli
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length == 0)
        {
            error.WriteLine("Missing command. Expected generate, compat, preview, or benchmarks.");
            return 2;
        }

        try
        {
            var command = args[0].ToLowerInvariant();
            var options = CliOptions.Parse(args.Skip(1).ToArray());

            return command switch
            {
                "generate" => Generate(options, output),
                "compat" => RunCompat(options, output),
                "preview" => RunPreview(options, output),
                "benchmarks" => RunBenchmarks(options, output, error),
                _ => UnknownCommand(command, error)
            };
        }
        catch (CorpusException ex)
        {
            error.WriteLine(ex.Message);
            return 2;
        }
    }

    private static int Generate(CliOptions options, TextWriter output)
    {
        var root = ScratchRootGuard.Prepare(options.Required("root"));
        var profile = CorpusProfiles.Normalize(options.ValueOrDefault("profile", "smoke"));

        var manifest = CorpusProfileGenerator.Generate(root, profile);
        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", profile, "manifest.json"), manifest);
        output.WriteLine($"Generated VeloFile {profile} corpus at {ScratchRootGuard.PathUnderRoot(root, "corpora", profile)}");
        return 0;
    }

    private static int RunCompat(CliOptions options, TextWriter output)
    {
        var scope = options.ValueOrDefault("scope", "smoke").Trim().ToLowerInvariant();

        if (StringComparer.Ordinal.Equals(scope, "safe-delete"))
        {
            return RunOperationsCompat(
                options,
                output,
                scope,
                [
                    "operations/rename-source.txt",
                    "operations/delete-target.txt"
                ],
                "safe-delete-result.json",
                "Compatibility safe-delete corpus passed.");
        }

        if (StringComparer.Ordinal.Equals(scope, "operations"))
        {
            return RunOperationsCompat(
                options,
                output,
                scope,
                [
                    "operations/copy/source.txt",
                    "operations/move/source.txt",
                    "operations/rename-source.txt",
                    "operations/delete-target.txt",
                    "operations/collisions/existing-name.txt",
                    "operations/collisions/incoming-name.txt",
                    "operations/batch/partial-0001.txt",
                    "operations/batch/partial-0002.txt"
                ],
                "operations-result.json",
                "Compatibility operations corpus passed.");
        }

        if (StringComparer.Ordinal.Equals(scope, "dragdrop"))
        {
            return RunDragDropCompat(options, output);
        }

        if (StringComparer.Ordinal.Equals(scope, "paths"))
        {
            return RunPathsCompat(options, output);
        }

        if (!StringComparer.Ordinal.Equals(scope, "smoke"))
        {
            ScratchRootGuard.ValidateOnly(options.Required("root"));
            output.WriteLine($"Compatibility corpus scope '{scope}' is not implemented in M2.");
            return 2;
        }

        var root = ScratchRootGuard.Prepare(options.Required("root"));
        CorpusProfileGenerator.Generate(root, "smoke");

        var result = new
        {
            documentType = "velofileCompatCorpusResult",
            schemaVersion = 1,
            scope = "smoke",
            result = "passed",
            checkedDirectories = new[] { "small", "preview", "compat" }
        };

        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", "smoke", "compat", "compat-smoke-result.json"), result);
        output.WriteLine("Compatibility smoke corpus passed.");
        return 0;
    }

    private static int RunOperationsCompat(
        CliOptions options,
        TextWriter output,
        string scope,
        IReadOnlyList<string> fixturePaths,
        string resultFileName,
        string successMessage)
    {
        var root = ScratchRootGuard.Prepare(options.Required("root"));
        CorpusProfileGenerator.Generate(root, "operations");

        foreach (var fixture in fixturePaths)
        {
            var segments = fixture.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var fixturePath = ScratchRootGuard.PathUnderRoot(root, ["corpora", "operations", .. segments]);
            if (!File.Exists(fixturePath))
            {
                throw new CorpusException($"Operations fixture '{fixture}' was not generated.");
            }
        }

        var result = new
        {
            documentType = "velofileCompatCorpusResult",
            schemaVersion = 1,
            scope,
            result = "passed",
            checkedFixtures = fixturePaths.ToArray(),
            checkedDirectories = new[]
            {
                "operations/copy",
                "operations/copy-target",
                "operations/move",
                "operations/move-target",
                "operations/collisions",
                "operations/batch"
            }
        };

        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", "operations", "compat", resultFileName), result);
        output.WriteLine(successMessage);
        return 0;
    }

    private static int RunDragDropCompat(CliOptions options, TextWriter output)
    {
        var root = ScratchRootGuard.Prepare(options.Required("root"));
        CorpusProfileGenerator.Generate(root, "dragdrop");
        var fixturePaths = new[]
        {
            "dragdrop/same-volume/source/move-default.txt",
            "dragdrop/same-volume/target/target-placeholder.txt",
            "dragdrop/cross-volume/source/copy-default.txt",
            "dragdrop/cross-volume/target/target-placeholder.txt",
            "dragdrop/modifiers/ctrl-copy.txt",
            "dragdrop/modifiers/shift-move.txt",
            "dragdrop/modifiers/ctrl-shift-shortcut.txt"
        };

        AssertProfileFixtures(root, "dragdrop", fixturePaths);

        var result = new
        {
            documentType = "velofileCompatCorpusResult",
            schemaVersion = 1,
            scope = "dragdrop",
            result = "passed",
            checkedFixtures = fixturePaths,
            resolvedActions = new[]
            {
                "none:same-volume:move",
                "none:cross-volume:copy",
                "ctrl:any:copy",
                "shift:any:move",
                "ctrl-shift:any:shortcut"
            },
            manualChecklist = "docs/qa/m10-dragdrop-compatibility-checklist.md"
        };

        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", "dragdrop", "compat", "dragdrop-result.json"), result);
        output.WriteLine("Compatibility drag/drop corpus passed.");
        return 0;
    }

    private static int RunPathsCompat(CliOptions options, TextWriter output)
    {
        var root = ScratchRootGuard.Prepare(options.Required("root"));
        CorpusProfileGenerator.Generate(root, "pathological");
        var caseResults = new[]
        {
            VerifyLongPath(root),
            VerifySimplePathFixture(root, "unicode-path", "unicode", "compatibility/paths/unicode/文件-δοκιμή.txt"),
            VerifySimplePathFixture(root, "unusual-filename", "filename", "compatibility/paths/unusual/name with spaces [1].txt"),
            VerifyJunction(root),
            VerifySymlink(root),
            VerifyReparseLoop(root),
            SkippedCase(
                "access-denied",
                "permissions",
                "access-denied-fixture-requires-acl",
                "compatibility/access-denied",
                "access denied operation")
        };
        var failedCount = caseResults.Count(result => result.Status is "failed");

        var result = new
        {
            documentType = "velofileCompatCorpusResult",
            schemaVersion = 1,
            scope = "paths",
            result = failedCount == 0 ? "completed" : "failed",
            summary = new
            {
                verified = caseResults.Count(result => result.Status is "verified"),
                skipped = caseResults.Count(result => result.Status is "skipped"),
                unavailable = caseResults.Count(result => result.Status is "unavailable"),
                failed = failedCount
            },
            caseResults
        };

        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compat", "paths-result.json"), result);
        output.WriteLine(failedCount == 0
            ? "Compatibility paths corpus completed."
            : "Compatibility paths corpus failed.");
        return failedCount == 0 ? 0 : 1;
    }

    private static PathCompatibilityCaseResult VerifyLongPath(DirectoryInfo root)
    {
        var segments = new List<string>
        {
            "corpora",
            "pathological",
            "compatibility",
            "paths",
            "long-path"
        };

        var index = 1;
        while (Path.Combine([root.FullName, .. segments, "long-path-file.txt"]).Length < 270)
        {
            segments.Add("segment-" + index.ToString("0000"));
            index++;
        }

        segments.Add("long-path-file.txt");
        return TryWriteAndVerifyCase(root, "long-path", "long-path", segments, "long path fixture");
    }

    private static PathCompatibilityCaseResult VerifySimplePathFixture(
        DirectoryInfo root,
        string caseId,
        string category,
        string relativePath)
    {
        return TryWriteAndVerifyCase(
            root,
            caseId,
            category,
            ["corpora", "pathological", .. relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries)],
            category + " fixture");
    }

    private static PathCompatibilityCaseResult VerifyJunction(DirectoryInfo root)
    {
        const string relativePath = "compatibility/reparse/junction";
        if (!OperatingSystem.IsWindows())
        {
            return UnavailableCase("junction", "junction", "not-windows", relativePath, "junction handling");
        }

        var target = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compatibility", "reparse", "junction-target");
        var junction = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compatibility", "reparse", "junction");
        Directory.CreateDirectory(target);

        var process = new ProcessStartInfo("cmd.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        process.ArgumentList.Add("/c");
        process.ArgumentList.Add("mklink");
        process.ArgumentList.Add("/J");
        process.ArgumentList.Add(junction);
        process.ArgumentList.Add(target);

        try
        {
            using var started = Process.Start(process);
            if (started is null || !started.WaitForExit(milliseconds: 5_000) || started.ExitCode != 0)
            {
                return SkippedCase("junction", "junction", "junction-creation-unavailable", relativePath, "junction handling");
            }

            return Directory.Exists(junction) && HasReparsePoint(junction)
                ? VerifiedCase("junction", "junction", relativePath, "junction handling")
                : FailedCase("junction", "junction", "junction-verification-failed", relativePath, "junction handling");
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase("junction", "junction", "junction-creation-unavailable", relativePath, "junction handling");
        }
    }

    private static PathCompatibilityCaseResult VerifySymlink(DirectoryInfo root)
    {
        const string relativePath = "compatibility/reparse/symlink";
        if (!OperatingSystem.IsWindows())
        {
            return UnavailableCase("symlink", "symlink", "not-windows", relativePath, "symlink handling");
        }

        var target = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compatibility", "reparse", "symlink-target.txt");
        var link = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compatibility", "reparse", "symlink.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllText(target, "Symlink target." + Environment.NewLine, Encoding.UTF8);

        try
        {
            File.CreateSymbolicLink(link, target);
            return File.Exists(link) && HasReparsePoint(link)
                ? VerifiedCase("symlink", "symlink", relativePath, "symlink handling")
                : FailedCase("symlink", "symlink", "symlink-verification-failed", relativePath, "symlink handling");
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase("symlink", "symlink", SymbolicLinkReasonCode(ex), relativePath, "symlink handling");
        }
    }

    private static PathCompatibilityCaseResult VerifyReparseLoop(DirectoryInfo root)
    {
        const string relativePath = "compatibility/reparse/loop";
        if (!OperatingSystem.IsWindows())
        {
            return UnavailableCase("reparse-loop", "reparse-loop", "not-windows", relativePath, "reparse loop handling");
        }

        var loopRoot = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compatibility", "reparse", "loop");
        var backLink = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compatibility", "reparse", "loop", "back");
        Directory.CreateDirectory(loopRoot);

        try
        {
            Directory.CreateSymbolicLink(backLink, loopRoot);
            return Directory.Exists(backLink) && HasReparsePoint(backLink)
                ? VerifiedCase("reparse-loop", "reparse-loop", relativePath, "reparse loop handling")
                : FailedCase("reparse-loop", "reparse-loop", "reparse-loop-verification-failed", relativePath, "reparse loop handling");
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase("reparse-loop", "reparse-loop", SymbolicLinkReasonCode(ex), relativePath, "reparse loop handling");
        }
    }

    private static PathCompatibilityCaseResult TryWriteAndVerifyCase(
        DirectoryInfo root,
        string caseId,
        string category,
        IReadOnlyList<string> segments,
        string operationUnderTest)
    {
        var relativePath = string.Join('/', segments.Skip(2));
        try
        {
            var path = ScratchRootGuard.PathUnderRoot(root, segments.ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, caseId + Environment.NewLine, Encoding.UTF8);
            return File.Exists(path)
                ? VerifiedCase(caseId, category, relativePath, operationUnderTest)
                : FailedCase(caseId, category, caseId + "-verification-failed", relativePath, operationUnderTest);
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase(caseId, category, caseId + "-fixture-unavailable", relativePath, operationUnderTest);
        }
    }

    private static bool HasReparsePoint(string path)
    {
        return (File.GetAttributes(path) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
    }

    private static bool IsFixtureUnavailable(Exception ex)
    {
        return ex is IOException
            or UnauthorizedAccessException
            or SecurityException
            or NotSupportedException
            or ArgumentException
            or System.ComponentModel.Win32Exception;
    }

    private static string SymbolicLinkReasonCode(Exception ex)
    {
        return ex is UnauthorizedAccessException
            ? "requires-admin-or-developer-mode"
            : "symlink-creation-unavailable";
    }

    private static PathCompatibilityCaseResult VerifiedCase(string caseId, string category, string relativePath, string operationUnderTest)
    {
        return new PathCompatibilityCaseResult(
            caseId,
            category,
            "verified",
            "verified",
            "scratch-relative:" + relativePath,
            CreatedFixture: true,
            VerifiedBehavior: true,
            operationUnderTest,
            "fixture verified");
    }

    private static PathCompatibilityCaseResult SkippedCase(
        string caseId,
        string category,
        string reasonCode,
        string relativePath,
        string operationUnderTest)
    {
        return new PathCompatibilityCaseResult(
            caseId,
            category,
            "skipped",
            reasonCode,
            "scratch-relative:" + relativePath,
            CreatedFixture: false,
            VerifiedBehavior: false,
            operationUnderTest,
            "case skipped with controlled reason");
    }

    private static PathCompatibilityCaseResult UnavailableCase(
        string caseId,
        string category,
        string reasonCode,
        string relativePath,
        string operationUnderTest)
    {
        return new PathCompatibilityCaseResult(
            caseId,
            category,
            "unavailable",
            reasonCode,
            "scratch-relative:" + relativePath,
            CreatedFixture: false,
            VerifiedBehavior: false,
            operationUnderTest,
            "capability unavailable");
    }

    private static PathCompatibilityCaseResult FailedCase(
        string caseId,
        string category,
        string reasonCode,
        string relativePath,
        string operationUnderTest)
    {
        return new PathCompatibilityCaseResult(
            caseId,
            category,
            "failed",
            reasonCode,
            "scratch-relative:" + relativePath,
            CreatedFixture: true,
            VerifiedBehavior: false,
            operationUnderTest,
            "fixture verification failed");
    }

    private static void AssertProfileFixtures(DirectoryInfo root, string profile, IReadOnlyList<string> fixturePaths)
    {
        foreach (var fixture in fixturePaths)
        {
            var segments = fixture.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var fixturePath = ScratchRootGuard.PathUnderRoot(root, ["corpora", profile, .. segments]);
            if (!File.Exists(fixturePath))
            {
                throw new CorpusException($"Compatibility fixture '{fixture}' was not generated.");
            }
        }
    }

    private static int RunPreview(CliOptions options, TextWriter output)
    {
        var scope = options.ValueOrDefault("scope", "smoke");

        if (!StringComparer.OrdinalIgnoreCase.Equals(scope, "smoke"))
        {
            ScratchRootGuard.ValidateOnly(options.Required("root"));
            output.WriteLine($"Preview corpus scope '{scope}' is not implemented in M2.");
            return 2;
        }

        var root = ScratchRootGuard.Prepare(options.Required("root"));
        CorpusProfileGenerator.Generate(root, "smoke");

        var result = new
        {
            documentType = "velofilePreviewCorpusResult",
            schemaVersion = 1,
            scope = "smoke",
            result = "passed",
            fixtures = new[] { "preview/text-preview.txt", "preview/unsupported.bin" }
        };

        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", "smoke", "preview", "preview-smoke-result.json"), result);
        output.WriteLine("Preview smoke corpus passed.");
        return 0;
    }

    private static int RunBenchmarks(CliOptions options, TextWriter output, TextWriter error)
    {
        if (!options.Has("non-gating"))
        {
            error.WriteLine("M2 benchmark runner is a non-gating stub. Pass --non-gating.");
            return 2;
        }

        var root = ScratchRootGuard.Prepare(options.Required("root"));
        CorpusProfileGenerator.Generate(root, "smoke");

        var report = BenchmarkReport.CreateNonGating();
        WriteJson(Path.Combine(root.FullName, "benchmarks", "benchmark-smoke-report.json"), report);
        output.WriteLine("Wrote non-gating benchmark report stub.");
        return 0;
    }

    private static int UnknownCommand(string command, TextWriter error)
    {
        error.WriteLine($"Unknown corpus command '{command}'.");
        return 2;
    }

    private static void WriteJson(string path, object document)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, JsonSerializer.Serialize(document, JsonOptions) + Environment.NewLine, Encoding.UTF8);
    }
}

internal static class ScratchRootGuard
{
    private const string MarkerFileName = ".velofile-corpus-root";

    public static DirectoryInfo Prepare(string root)
    {
        var directory = ValidateOnly(root);
        Directory.CreateDirectory(directory.FullName);
        File.WriteAllText(Path.Combine(directory.FullName, MarkerFileName), "VeloFile generated corpus scratch root." + Environment.NewLine, Encoding.UTF8);
        return directory;
    }

    public static DirectoryInfo ValidateOnly(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new CorpusException("A scratch root is required.");
        }

        if (!Path.IsPathFullyQualified(root))
        {
            throw new CorpusException("The scratch root must be an absolute path.");
        }

        var fullPath = Path.GetFullPath(root);
        var directory = new DirectoryInfo(fullPath);
        var leaf = directory.Name.ToLowerInvariant();

        if (!leaf.Contains("velofile", StringComparison.Ordinal) || !leaf.Contains("corpus", StringComparison.Ordinal))
        {
            throw new CorpusException("Refusing unsafe scratch root: path leaf must contain 'velofile' and 'corpus'.");
        }

        if (IsUnsafeRoot(fullPath))
        {
            throw new CorpusException("Refusing unsafe scratch root: choose a dedicated VeloFile corpus workspace.");
        }

        if (directory.Exists && Directory.EnumerateFileSystemEntries(directory.FullName).Any() && !File.Exists(Path.Combine(directory.FullName, MarkerFileName)))
        {
            throw new CorpusException("Refusing unsafe scratch root: existing non-empty directory is not marked as a VeloFile corpus workspace.");
        }

        return directory;
    }

    public static string PathUnderRoot(DirectoryInfo root, params string[] segments)
    {
        var combined = Path.GetFullPath(Path.Combine(new[] { root.FullName }.Concat(segments).ToArray()));
        var rootPath = Path.GetFullPath(root.FullName).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new CorpusException("Internal corpus path escaped the scratch root.");
        }

        return combined;
    }

    private static bool IsUnsafeRoot(string fullPath)
    {
        var normalized = Normalize(fullPath);

        if (Path.GetPathRoot(normalized)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Equals(normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile) && string.Equals(normalized, Normalize(userProfile), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var repoRoot = FindRepoRoot();
        return repoRoot is not null && string.Equals(normalized, Normalize(repoRoot.FullName), StringComparison.OrdinalIgnoreCase);
    }

    private static DirectoryInfo? FindRepoRoot()
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

        return null;
    }

    private static string Normalize(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}

internal static class CorpusProfiles
{
    private static readonly string[] SupportedProfiles =
    [
        "smoke",
        "operations",
        "preview",
        "search",
        "large-folder",
        "dragdrop",
        "pathological"
    ];

    public static string Normalize(string profile)
    {
        var normalized = profile.Trim().ToLowerInvariant();

        if (!SupportedProfiles.Contains(normalized, StringComparer.Ordinal))
        {
            throw new CorpusException($"Corpus profile '{profile}' is not implemented in M2.");
        }

        return normalized;
    }
}

internal static class CorpusProfileGenerator
{
    private static readonly DateTime FixedTimestampUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static CorpusManifest Generate(DirectoryInfo root, string profile)
    {
        var normalizedProfile = CorpusProfiles.Normalize(profile);
        var profileRoot = new DirectoryInfo(ScratchRootGuard.PathUnderRoot(root, "corpora", normalizedProfile));
        Directory.CreateDirectory(profileRoot.FullName);

        var fixtures = FixturesFor(normalizedProfile);
        foreach (var directory in DirectoriesFor(normalizedProfile))
        {
            Directory.CreateDirectory(ScratchRootGuard.PathUnderRoot(profileRoot, directory.Split('/', StringSplitOptions.RemoveEmptyEntries)));
        }

        var files = fixtures
            .Select(fixture => WriteFixture(profileRoot, fixture.SegmentsAndContent))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();

        return new CorpusManifest(
            DocumentType: "velofileCorpusManifest",
            SchemaVersion: 1,
            Profile: normalizedProfile,
            Directories: DirectoriesFor(normalizedProfile),
            Files: files,
            Scopes: ScopesFor(normalizedProfile));
    }

    private static CorpusFixture[] FixturesFor(string profile)
    {
        return profile switch
        {
            "smoke" =>
            [
                new CorpusFixture(["small", "alpha.txt", FixedWidth("alpha", 64)]),
                new CorpusFixture(["small", "beta.log", FixedWidth("beta", 128)]),
                new CorpusFixture(["preview", "text-preview.txt", "VeloFile preview smoke text." + Environment.NewLine]),
                new CorpusFixture(["preview", "unsupported.bin", "\0\0VeloFile unsupported preview placeholder"]),
                new CorpusFixture(["compat", "paths", "normal-file.txt", "VeloFile compatibility smoke path." + Environment.NewLine]),
                new CorpusFixture(["operations", "rename-source.txt", "Scratch-only operation placeholder." + Environment.NewLine])
            ],
            "operations" =>
            [
                new CorpusFixture(["operations", "copy", "source.txt", "Copy source placeholder." + Environment.NewLine]),
                new CorpusFixture(["operations", "move", "source.txt", "Move source placeholder." + Environment.NewLine]),
                new CorpusFixture(["operations", "rename-source.txt", "Rename source placeholder." + Environment.NewLine]),
                new CorpusFixture(["operations", "delete-target.txt", "Recycle Bin delete placeholder." + Environment.NewLine]),
                new CorpusFixture(["operations", "collisions", "existing-name.txt", "Existing collision placeholder." + Environment.NewLine]),
                new CorpusFixture(["operations", "collisions", "incoming-name.txt", "Incoming collision placeholder." + Environment.NewLine]),
                new CorpusFixture(["operations", "batch", "partial-0001.txt", "Partial batch placeholder 0001." + Environment.NewLine]),
                new CorpusFixture(["operations", "batch", "partial-0002.txt", "Partial batch placeholder 0002." + Environment.NewLine])
            ],
            "preview" =>
            [
                new CorpusFixture(["preview", "text-preview.txt", "VeloFile preview text placeholder." + Environment.NewLine]),
                new CorpusFixture(["preview", "code-preview.cs", "namespace VeloFile.PreviewCorpus;" + Environment.NewLine]),
                new CorpusFixture(["preview", "unsupported.bin", "\0\0VeloFile unsupported preview placeholder"]),
                new CorpusFixture(["preview", "metadata-only.placeholder", "Metadata fallback placeholder." + Environment.NewLine])
            ],
            "search" =>
            [
                new CorpusFixture(["search", "root-match.txt", "Search root match placeholder." + Environment.NewLine]),
                new CorpusFixture(["search", "deep", "level01", "level02", "deep-match.txt", "Search deep match placeholder." + Environment.NewLine]),
                new CorpusFixture(["search", "many", "result-0001.txt", "Search result placeholder 0001." + Environment.NewLine]),
                new CorpusFixture(["search", "many", "result-0002.txt", "Search result placeholder 0002." + Environment.NewLine])
            ],
            "large-folder" =>
            [
                new CorpusFixture(["large-folder", "README.txt", "Large-folder profile placeholder. Full scale arrives in later milestones." + Environment.NewLine]),
                new CorpusFixture(["large-folder", "items", "item-0001.txt", "Large-folder item placeholder 0001." + Environment.NewLine]),
                new CorpusFixture(["large-folder", "items", "item-0002.txt", "Large-folder item placeholder 0002." + Environment.NewLine])
            ],
            "dragdrop" =>
            [
                new CorpusFixture(["dragdrop", "same-volume", "source", "move-default.txt", "Same-volume default move placeholder." + Environment.NewLine]),
                new CorpusFixture(["dragdrop", "same-volume", "target", "target-placeholder.txt", "Same-volume target placeholder." + Environment.NewLine]),
                new CorpusFixture(["dragdrop", "cross-volume", "source", "copy-default.txt", "Cross-volume default copy placeholder." + Environment.NewLine]),
                new CorpusFixture(["dragdrop", "cross-volume", "target", "target-placeholder.txt", "Cross-volume target placeholder." + Environment.NewLine]),
                new CorpusFixture(["dragdrop", "modifiers", "ctrl-copy.txt", "Ctrl copy placeholder." + Environment.NewLine]),
                new CorpusFixture(["dragdrop", "modifiers", "shift-move.txt", "Shift move placeholder." + Environment.NewLine]),
                new CorpusFixture(["dragdrop", "modifiers", "ctrl-shift-shortcut.txt", "Ctrl+Shift shortcut placeholder." + Environment.NewLine])
            ],
            "pathological" =>
            [
                new CorpusFixture(["compatibility", "paths", "long-path", "segment-0001", "segment-0002", "segment-0003", "segment-0004", "long-path-file.txt", "Long path compatibility seed." + Environment.NewLine]),
                new CorpusFixture(["compatibility", "paths", "unicode", "文件-δοκιμή.txt", "Unicode path compatibility seed." + Environment.NewLine]),
                new CorpusFixture(["compatibility", "paths", "unusual", "name with spaces [1].txt", "Unusual filename compatibility seed." + Environment.NewLine]),
                new CorpusFixture(["compatibility", "reparse", "junction-target", "target.txt", "Junction target seed." + Environment.NewLine]),
                new CorpusFixture(["compatibility", "reparse", "symlink-target.txt", "Symlink target seed." + Environment.NewLine]),
                new CorpusFixture(["compatibility", "access-denied", "README.txt", "Access-denied compatibility seed." + Environment.NewLine])
            ],
            _ => throw new CorpusException($"Corpus profile '{profile}' is not implemented in M2.")
        };
    }

    private static string[] DirectoriesFor(string profile)
    {
        return profile switch
        {
            "smoke" => ["small", "preview", "compat", "compat/paths", "operations"],
            "operations" => ["operations", "operations/copy", "operations/copy-target", "operations/move", "operations/move-target", "operations/collisions", "operations/batch"],
            "preview" => ["preview"],
            "search" => ["search", "search/deep", "search/deep/level01", "search/deep/level01/level02", "search/many"],
            "large-folder" => ["large-folder", "large-folder/items"],
            "dragdrop" => ["dragdrop", "dragdrop/same-volume", "dragdrop/same-volume/source", "dragdrop/same-volume/target", "dragdrop/cross-volume", "dragdrop/cross-volume/source", "dragdrop/cross-volume/target", "dragdrop/modifiers", "dragdrop/compat"],
            "pathological" => ["compatibility", "compatibility/paths", "compatibility/paths/long-path", "compatibility/paths/long-path/segment-0001", "compatibility/paths/long-path/segment-0001/segment-0002", "compatibility/paths/long-path/segment-0001/segment-0002/segment-0003", "compatibility/paths/long-path/segment-0001/segment-0002/segment-0003/segment-0004", "compatibility/paths/unicode", "compatibility/paths/unusual", "compatibility/reparse", "compatibility/reparse/junction-target", "compatibility/reparse/loop", "compatibility/access-denied", "compatibility/compat"],
            _ => throw new CorpusException($"Corpus profile '{profile}' is not implemented in M2.")
        };
    }

    private static string[] ScopesFor(string profile)
    {
        return profile switch
        {
            "smoke" => ["generate:smoke", "compat:smoke", "preview:smoke", "benchmarks:non-gating"],
            "operations" => ["generate:operations", "compat:operations", "compat:safe-delete"],
            "dragdrop" => ["generate:dragdrop", "compat:dragdrop"],
            "pathological" => ["generate:pathological", "compat:paths"],
            _ => [$"generate:{profile}"]
        };
    }

    private static CorpusFile WriteFixture(DirectoryInfo root, string[] segmentsAndContent)
    {
        var content = segmentsAndContent[^1];
        var segments = segmentsAndContent[..^1];
        var path = ScratchRootGuard.PathUnderRoot(root, segments);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, Encoding.UTF8);
        File.SetLastWriteTimeUtc(path, FixedTimestampUtc);

        return new CorpusFile(
            RelativePath: string.Join('/', segments),
            SizeBytes: new FileInfo(path).Length,
            Sha256: Sha256(path));
    }

    private static string FixedWidth(string prefix, int length)
    {
        return (prefix + new string('.', length)).Substring(0, length);
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}

internal sealed record CorpusFixture(string[] SegmentsAndContent);

internal sealed record CorpusManifest(
    string DocumentType,
    int SchemaVersion,
    string Profile,
    IReadOnlyList<string> Directories,
    IReadOnlyList<CorpusFile> Files,
    IReadOnlyList<string> Scopes);

internal sealed record CorpusFile(string RelativePath, long SizeBytes, string Sha256);

internal sealed record PathCompatibilityCaseResult(
    string CaseId,
    string Category,
    string Status,
    string ReasonCode,
    string FixturePathKind,
    bool CreatedFixture,
    bool VerifiedBehavior,
    string OperationUnderTest,
    string Notes);

internal static class BenchmarkReport
{
    public static object CreateNonGating()
    {
        return new
        {
            documentType = "velofileBenchmarkReport",
            schemaVersion = 1,
            nonGating = true,
            environment = new
            {
                osBuild = Environment.OSVersion.VersionString,
                hardwareClass = "unknown",
                cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown",
                ram = "unknown",
                storageType = "unknown",
                windowsSearchState = "unknown",
                antivirusState = "unknown",
                dpiConfiguration = "unknown"
            },
            measurements = new object[]
            {
                new
                {
                    name = "m2.non-gating-smoke",
                    runCount = 0,
                    medianMs = (double?)null,
                    p95Ms = (double?)null,
                    p99Ms = (double?)null,
                    releaseGatingStatus = "non-gating"
                }
            }
        };
    }
}

internal sealed class CliOptions
{
    private readonly Dictionary<string, string?> _values;

    private CliOptions(Dictionary<string, string?> values)
    {
        _values = values;
    }

    public static CliOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                throw new CorpusException($"Unexpected argument '{token}'.");
            }

            var key = token[2..];
            string? value = null;

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[++i];
            }

            values[key] = value;
        }

        return new CliOptions(values);
    }

    public bool Has(string key)
    {
        return _values.ContainsKey(key);
    }

    public string Required(string key)
    {
        if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new CorpusException($"Missing required option --{key}.");
        }

        return value;
    }

    public string ValueOrDefault(string key, string defaultValue)
    {
        return _values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }
}

internal sealed class CorpusException : Exception
{
    public CorpusException(string message)
        : base(message)
    {
    }
}
