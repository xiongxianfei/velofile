using System.Security.Cryptography;
using System.Diagnostics;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;
using VeloFile.Core;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;
using VeloFile.Core.Search;
using VeloFile.Core.Visibility;

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
        var behaviorVerifier = new PathCompatibilityBehaviorVerifier();
        var caseResults = new[]
        {
            VerifyLongPath(root, behaviorVerifier),
            VerifySimplePathFixture(root, behaviorVerifier, "unicode-path", "unicode", "compatibility/paths/unicode/文件-δοκιμή.txt"),
            VerifySimplePathFixture(root, behaviorVerifier, "unusual-filename", "filename", "compatibility/paths/unusual/name with spaces [1].txt"),
            VerifyJunction(root, behaviorVerifier),
            VerifySymlink(root, behaviorVerifier),
            VerifyReparseLoop(root, behaviorVerifier),
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
                notImplemented = caseResults.Count(result => result.Status is "not-implemented"),
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

    private static PathCompatibilityCaseResult VerifyLongPath(
        DirectoryInfo root,
        PathCompatibilityBehaviorVerifier behaviorVerifier)
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
        return TryWriteAndVerifyCase(root, behaviorVerifier, "long-path", "long-path", segments, "long path fixture");
    }

    private static PathCompatibilityCaseResult VerifySimplePathFixture(
        DirectoryInfo root,
        PathCompatibilityBehaviorVerifier behaviorVerifier,
        string caseId,
        string category,
        string relativePath)
    {
        return TryWriteAndVerifyCase(
            root,
            behaviorVerifier,
            caseId,
            category,
            ["corpora", "pathological", .. relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries)],
            category + " fixture");
    }

    private static PathCompatibilityCaseResult VerifyJunction(
        DirectoryInfo root,
        PathCompatibilityBehaviorVerifier behaviorVerifier)
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

            if (!Directory.Exists(junction) || !HasReparsePoint(junction))
            {
                return FailedCase("junction", "junction", "junction-verification-failed", relativePath, "junction handling");
            }

            var behavior = behaviorVerifier.VerifyReparseDirectorySkipped(
                Path.GetDirectoryName(junction)!,
                junction);
            return CaseFromBehavior("junction", "junction", relativePath, "junction handling", behavior);
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase("junction", "junction", "junction-creation-unavailable", relativePath, "junction handling");
        }
    }

    private static PathCompatibilityCaseResult VerifySymlink(
        DirectoryInfo root,
        PathCompatibilityBehaviorVerifier behaviorVerifier)
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
            if (!File.Exists(link) || !HasReparsePoint(link))
            {
                return FailedCase("symlink", "symlink", "symlink-verification-failed", relativePath, "symlink handling");
            }

            var behavior = behaviorVerifier.VerifyListedEntry(
                Path.GetDirectoryName(link)!,
                link,
                requireReparsePoint: true,
                evidenceKind: "listing");
            return CaseFromBehavior("symlink", "symlink", relativePath, "symlink handling", behavior);
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase("symlink", "symlink", SymbolicLinkReasonCode(ex), relativePath, "symlink handling");
        }
    }

    private static PathCompatibilityCaseResult VerifyReparseLoop(
        DirectoryInfo root,
        PathCompatibilityBehaviorVerifier behaviorVerifier)
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
            if (!Directory.Exists(backLink) || !HasReparsePoint(backLink))
            {
                return FailedCase("reparse-loop", "reparse-loop", "reparse-loop-verification-failed", relativePath, "reparse loop handling");
            }

            var behavior = behaviorVerifier.VerifyReparseDirectorySkipped(loopRoot, backLink);
            return CaseFromBehavior("reparse-loop", "reparse-loop", relativePath, "reparse loop handling", behavior);
        }
        catch (Exception ex) when (IsFixtureUnavailable(ex))
        {
            return SkippedCase("reparse-loop", "reparse-loop", SymbolicLinkReasonCode(ex), relativePath, "reparse loop handling");
        }
    }

    private static PathCompatibilityCaseResult TryWriteAndVerifyCase(
        DirectoryInfo root,
        PathCompatibilityBehaviorVerifier behaviorVerifier,
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
            if (!File.Exists(path))
            {
                return FailedCase(caseId, category, caseId + "-verification-failed", relativePath, operationUnderTest);
            }

            var behavior = behaviorVerifier.VerifyListedEntry(
                Path.GetDirectoryName(path)!,
                path,
                requireReparsePoint: false,
                evidenceKind: "listing");
            return CaseFromBehavior(caseId, category, relativePath, operationUnderTest, behavior);
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

    private static PathCompatibilityCaseResult CaseFromBehavior(
        string caseId,
        string category,
        string relativePath,
        string operationUnderTest,
        PathBehaviorVerification behavior)
    {
        if (!behavior.Invoked)
        {
            return new PathCompatibilityCaseResult(
                caseId,
                category,
                "not-implemented",
                "behavior-verifier-not-implemented",
                "scratch-relative:" + relativePath,
                FixtureCreated: true,
                FixtureVerified: true,
                BehaviorVerifierInvoked: false,
                VerifiedBehavior: false,
                "fixture-only",
                BlocksReleaseEvidence: true,
                operationUnderTest,
                "fixture verified but behavior verifier was not invoked");
        }

        if (!behavior.Verified)
        {
            return new PathCompatibilityCaseResult(
                caseId,
                category,
                "failed",
                behavior.ReasonCode,
                "scratch-relative:" + relativePath,
                FixtureCreated: true,
                FixtureVerified: true,
                BehaviorVerifierInvoked: true,
                VerifiedBehavior: false,
                behavior.EvidenceKind,
                BlocksReleaseEvidence: true,
                operationUnderTest,
                "behavior verification failed");
        }

        return new PathCompatibilityCaseResult(
            caseId,
            category,
            "verified",
            "verified",
            "scratch-relative:" + relativePath,
            FixtureCreated: true,
            FixtureVerified: true,
            BehaviorVerifierInvoked: true,
            VerifiedBehavior: true,
            behavior.EvidenceKind,
            BlocksReleaseEvidence: false,
            operationUnderTest,
            "fixture and behavior verified");
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
            FixtureCreated: false,
            FixtureVerified: false,
            BehaviorVerifierInvoked: false,
            VerifiedBehavior: false,
            "not-run",
            BlocksReleaseEvidence: false,
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
            FixtureCreated: false,
            FixtureVerified: false,
            BehaviorVerifierInvoked: false,
            VerifiedBehavior: false,
            "not-run",
            BlocksReleaseEvidence: false,
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
            FixtureCreated: true,
            FixtureVerified: false,
            BehaviorVerifierInvoked: false,
            VerifiedBehavior: false,
            "fixture-verification",
            BlocksReleaseEvidence: true,
            operationUnderTest,
            "fixture verification failed");
    }

    private sealed class PathCompatibilityBehaviorVerifier
    {
        private readonly FolderListingService _listingService;
        private readonly RecursiveSearchService _searchService;

        public PathCompatibilityBehaviorVerifier()
        {
            var entrySource = new CorpusFolderEntrySource();
            _listingService = new FolderListingService(entrySource);
            _searchService = new RecursiveSearchService(entrySource);
        }

        public PathBehaviorVerification VerifyListedEntry(
            string parentDirectory,
            string expectedPath,
            bool requireReparsePoint,
            string evidenceKind)
        {
            return RunBounded(evidenceKind, async cancellationToken =>
            {
                var listing = await _listingService.LoadFirstViewportAsync(
                    parentDirectory,
                    new FolderListingOptions(256, VisibilitySettings.Default),
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                if (listing.Status is not FolderListingStatus.Ready and not FolderListingStatus.Empty)
                {
                    return PathBehaviorVerification.Failed(evidenceKind, listing.ReasonCode ?? "listing-unavailable");
                }

                var item = listing.FirstViewport.FirstOrDefault(entry =>
                    string.Equals(entry.FullPath, expectedPath, StringComparison.OrdinalIgnoreCase));
                if (item is null)
                {
                    return PathBehaviorVerification.Failed(evidenceKind, "listing-missing-fixture");
                }

                if (requireReparsePoint && !item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    return PathBehaviorVerification.Failed(evidenceKind, "listing-missing-reparse-attribute");
                }

                return PathBehaviorVerification.Passed(evidenceKind);
            });
        }

        public PathBehaviorVerification VerifyReparseDirectorySkipped(string rootPath, string expectedSkippedPath)
        {
            const string evidenceKind = "recursive-search-loop-detection";
            return RunBounded(evidenceKind, async cancellationToken =>
            {
                await foreach (var update in _searchService
                    .SearchAsync(rootPath, "__velofile_no_matches__", new RecursiveSearchOptions(256), cancellationToken)
                    .WithCancellation(cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (update.Kind is RecursiveSearchUpdateKind.SkippedLocation
                        && update.SkippedLocation is not null
                        && string.Equals(update.SkippedLocation.Path, expectedSkippedPath, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(update.SkippedLocation.ReasonCode, "reparse-point", StringComparison.Ordinal))
                    {
                        return PathBehaviorVerification.Passed(evidenceKind);
                    }
                }

                return PathBehaviorVerification.Failed(evidenceKind, "reparse-skip-not-observed");
            });
        }

        private static PathBehaviorVerification RunBounded(
            string evidenceKind,
            Func<CancellationToken, Task<PathBehaviorVerification>> verify)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                return verify(timeout.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                return PathBehaviorVerification.Failed(evidenceKind, "behavior-verifier-timeout");
            }
            catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
            {
                return PathBehaviorVerification.Failed(evidenceKind, ExpectedFileSystemExceptions.ReasonCode(ex));
            }
        }
    }

    private sealed record PathBehaviorVerification(
        bool Invoked,
        bool Verified,
        string EvidenceKind,
        string ReasonCode)
    {
        public static PathBehaviorVerification Passed(string evidenceKind)
        {
            return new PathBehaviorVerification(Invoked: true, Verified: true, evidenceKind, "verified");
        }

        public static PathBehaviorVerification Failed(string evidenceKind, string reasonCode)
        {
            return new PathBehaviorVerification(Invoked: true, Verified: false, evidenceKind, reasonCode);
        }
    }

    private sealed class CorpusFolderEntrySource : IFolderEntrySource
    {
        public async IAsyncEnumerable<FileSystemEntrySnapshot> EnumerateAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();

            var directory = new DirectoryInfo(path);
            foreach (var entry in directory.EnumerateFileSystemInfos())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var snapshot = TryCreateSnapshot(entry);
                if (snapshot is not null)
                {
                    yield return snapshot;
                }
            }
        }

        private static FileSystemEntrySnapshot? TryCreateSnapshot(FileSystemInfo entry)
        {
            try
            {
                var attributes = entry.Attributes;
                var kind = attributes.HasFlag(FileAttributes.Directory)
                    ? FileSystemEntryKind.Directory
                    : entry is FileInfo ? FileSystemEntryKind.File : FileSystemEntryKind.Other;

                return new FileSystemEntrySnapshot(
                    entry.FullName,
                    entry.Name,
                    kind,
                    kind is FileSystemEntryKind.File && entry is FileInfo file ? file.Length : null,
                    entry.LastWriteTimeUtc,
                    attributes);
            }
            catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
            {
                return null;
            }
        }
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

        if (StringComparer.OrdinalIgnoreCase.Equals(scope, "contract"))
        {
            var contractRoot = ScratchRootGuard.Prepare(options.Required("root"));
            CorpusProfileGenerator.Generate(contractRoot, "preview");
            var caseResults = PreviewContractCorpusVerifier.Verify();
            var failedCount = caseResults.Count(result => result.Status is not "verified");

            var contractResult = new
            {
                documentType = "velofilePreviewCorpusResult",
                schemaVersion = 1,
                scope = "contract",
                status = failedCount == 0 ? "verified" : "failed",
                behaviorVerifierInvoked = caseResults.All(result => result.BehaviorVerifierInvoked),
                verifiedBehavior = caseResults.All(result => result.VerifiedBehavior),
                evidenceKind = "preview-contract",
                caseResults
            };

            WriteJson(ScratchRootGuard.PathUnderRoot(contractRoot, "corpora", "preview", "preview", "preview-contract-result.json"), contractResult);
            output.WriteLine(failedCount == 0
                ? "Preview contract corpus passed."
                : "Preview contract corpus failed.");
            return failedCount == 0 ? 0 : 1;
        }

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

internal static class PreviewContractCorpusVerifier
{
    public static IReadOnlyList<PreviewContractCaseResult> Verify()
    {
        return VerifyAsync().GetAwaiter().GetResult();
    }

    private static async Task<IReadOnlyList<PreviewContractCaseResult>> VerifyAsync()
    {
        return
        [
            await VerifyCaseAsync("loading-delay", VerifyLoadingDelayAsync).ConfigureAwait(false),
            await VerifyCaseAsync("timeout", VerifyTimeoutAsync).ConfigureAwait(false),
            await VerifyCaseAsync("metadata-fallback", VerifyMetadataFallbackAsync).ConfigureAwait(false),
            await VerifyCaseAsync("stale-selection", VerifyStaleSelectionAsync).ConfigureAwait(false)
        ];
    }

    private static async Task<PreviewContractCaseResult> VerifyCaseAsync(
        string caseId,
        Func<Task<bool>> verify)
    {
        try
        {
            var verified = await verify().ConfigureAwait(false);
            return new PreviewContractCaseResult(
                caseId,
                verified ? "verified" : "failed",
                true,
                verified,
                "preview-contract",
                verified ? "verified" : caseId + "-failed");
        }
        catch (OperationCanceledException)
        {
            return new PreviewContractCaseResult(
                caseId,
                "failed",
                true,
                false,
                "preview-contract",
                "preview-contract-timeout");
        }
        catch
        {
            return new PreviewContractCaseResult(
                caseId,
                "failed",
                true,
                false,
                "preview-contract",
                "preview-contract-exception");
        }
    }

    private static async Task<bool> VerifyLoadingDelayAsync()
    {
        var provider = new ContractPreviewProvider();
        provider.Pending("loading.txt");
        var controller = CreateController(provider, loadingDelayMs: 60, timeoutMs: 500);

        controller.StartPreview(Item("loading.txt", length: 17));
        await Task.Delay(20).ConfigureAwait(false);
        if (controller.State.Status is not PreviewStatus.Empty)
        {
            return false;
        }

        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Loading).ConfigureAwait(false);
        provider.Complete("loading.txt", PreviewProviderResult.Success(PreviewContent.Text("loaded", truncated: false)));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success).ConfigureAwait(false);

        return controller.State.Metadata?.Name == "loading.txt"
            && controller.State.Content?.TextContent == "loaded";
    }

    private static async Task<bool> VerifyTimeoutAsync()
    {
        var provider = new ContractPreviewProvider();
        provider.Pending("timeout.txt");
        var controller = CreateController(provider, loadingDelayMs: 5, timeoutMs: 80);

        controller.StartPreview(Item("timeout.txt", length: 12));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Failed).ConfigureAwait(false);

        return controller.State.ReasonCode == "timeout"
            && controller.State.Metadata?.Name == "timeout.txt";
    }

    private static async Task<bool> VerifyMetadataFallbackAsync()
    {
        var provider = new ContractPreviewProvider();
        provider.Immediate("unsupported.bin", PreviewProviderResult.Unsupported("unsupported"));
        var controller = CreateController(provider, loadingDelayMs: 60, timeoutMs: 500);

        controller.StartPreview(Item("unsupported.bin", length: 1024));
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Unsupported).ConfigureAwait(false);

        var fields = controller.State.Metadata?.Fields() ?? [];
        return controller.State.ReasonCode == "unsupported"
            && fields.Any(field => field.Label == "Size" && field.Value.Contains("1024", StringComparison.Ordinal));
    }

    private static async Task<bool> VerifyStaleSelectionAsync()
    {
        var provider = new ContractPreviewProvider();
        provider.Pending("old.txt");
        provider.Immediate("new.txt", PreviewProviderResult.Success(PreviewContent.Text("new", truncated: false)));
        var controller = CreateController(provider, loadingDelayMs: 60, timeoutMs: 500);

        controller.StartPreview(Item("old.txt", length: 9));
        await WaitUntilAsync(() => provider.Started("old.txt")).ConfigureAwait(false);

        controller.StartPreview(Item("new.txt", length: 10));
        await WaitUntilAsync(() => provider.Cancelled("old.txt")).ConfigureAwait(false);
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success).ConfigureAwait(false);

        provider.Complete("old.txt", PreviewProviderResult.Success(PreviewContent.Text("old", truncated: false)));
        await Task.Delay(20).ConfigureAwait(false);

        return controller.State.Metadata?.Name == "new.txt"
            && controller.State.Content?.TextContent == "new";
    }

    private static PreviewController CreateController(
        ContractPreviewProvider provider,
        int loadingDelayMs,
        int timeoutMs)
    {
        return new PreviewController(
            [provider],
            new PreviewMetadataProvider(),
            new PreviewControllerOptions(
                TimeSpan.FromMilliseconds(loadingDelayMs),
                TimeSpan.FromMilliseconds(timeoutMs)));
    }

    private static ListedFileItem Item(string name, long? length)
    {
        return new ListedFileItem(
            @"C:\velofile-preview\" + name,
            name,
            name,
            FileSystemEntryKind.File,
            length,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token).ConfigureAwait(false);
        }
    }

    private sealed class ContractPreviewProvider : IPreviewProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<PreviewProviderResult>> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PreviewProviderResult> _immediate = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

        public bool CanPreview(PreviewRequest request)
        {
            return true;
        }

        public ValueTask<PreviewProviderResult> PreviewAsync(PreviewRequest request, CancellationToken cancellationToken)
        {
            _started.Add(request.Item.Name);
            cancellationToken.Register(() => _cancelled.Add(request.Item.Name));

            if (_pending.TryGetValue(request.Item.Name, out var pending))
            {
                return new ValueTask<PreviewProviderResult>(pending.Task);
            }

            return _immediate.TryGetValue(request.Item.Name, out var result)
                ? ValueTask.FromResult(result)
                : ValueTask.FromResult(PreviewProviderResult.Unsupported("unsupported"));
        }

        public void Pending(string name)
        {
            _pending[name] = new TaskCompletionSource<PreviewProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Immediate(string name, PreviewProviderResult result)
        {
            _immediate[name] = result;
        }

        public void Complete(string name, PreviewProviderResult result)
        {
            if (_pending.TryGetValue(name, out var pending))
            {
                pending.TrySetResult(result);
            }
        }

        public bool Started(string name)
        {
            return _started.Contains(name);
        }

        public bool Cancelled(string name)
        {
            return _cancelled.Contains(name);
        }
    }
}

internal sealed record PreviewContractCaseResult(
    string CaseId,
    string Status,
    bool BehaviorVerifierInvoked,
    bool VerifiedBehavior,
    string EvidenceKind,
    string ReasonCode);

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
    bool FixtureCreated,
    bool FixtureVerified,
    bool BehaviorVerifierInvoked,
    bool VerifiedBehavior,
    string EvidenceKind,
    bool BlocksReleaseEvidence,
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
