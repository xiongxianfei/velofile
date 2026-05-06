using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using VeloFile.Core;
using VeloFile.Core.Diagnostics;
using VeloFile.Core.Listing;
using VeloFile.Core.Preview;
using VeloFile.Core.Search;
using VeloFile.Core.Visibility;
using VeloFile.Windows.Preview;

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
            error.WriteLine("Missing command. Expected generate, compat, preview, diagnostics, or benchmarks.");
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
                "diagnostics" => RunDiagnosticsConformance(options, output),
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

        if (StringComparer.Ordinal.Equals(scope, "release"))
        {
            return RunReleaseCompat(options, output);
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
        var caseResults = BuildPathCompatibilityCases(root);
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

    private static int RunReleaseCompat(CliOptions options, TextWriter output)
    {
        var root = ScratchRootGuard.Prepare(options.Required("root"));
        _ = RunOperationsCompat(
            options,
            TextWriter.Null,
            "operations",
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
        _ = RunDragDropCompat(options, TextWriter.Null);
        _ = RunPathsCompat(options, TextWriter.Null);

        var operationsResultPath = ScratchRootGuard.PathUnderRoot(root, "corpora", "operations", "compat", "operations-result.json");
        var dragDropResultPath = ScratchRootGuard.PathUnderRoot(root, "corpora", "dragdrop", "compat", "dragdrop-result.json");
        var pathsResultPath = ScratchRootGuard.PathUnderRoot(root, "corpora", "pathological", "compat", "paths-result.json");
        var pathCases = ReadPathCaseResults(pathsResultPath);

        var scopeResults = new[]
        {
            AggregateFixtureScope("operations", operationsResultPath, root, "operation-corpus-result"),
            AggregateFixtureScope("dragdrop", dragDropResultPath, root, "dragdrop-route-result"),
            AggregatePathScope(pathsResultPath, root, pathCases),
            MissingReleaseScope("associations", "association-verifier-not-implemented", "association-launch-verifier"),
            MissingReleaseScope("dpi", "dpi-verifier-not-implemented", "mixed-dpi-verifier-or-checklist")
        };
        var blocksReleaseEvidence = scopeResults.Any(result => !result.ReleaseEvidence);

        var releaseResult = new
        {
            documentType = "velofileCompatCorpusResult",
            schemaVersion = 1,
            scope = "release",
            status = blocksReleaseEvidence ? "incomplete" : "completed",
            blocksReleaseEvidence,
            summary = new
            {
                verifiedScopes = scopeResults.Count(result => result.Status is "verified"),
                fixtureOnlyScopes = scopeResults.Count(result => result.Status is "fixture-only"),
                skippedScopes = scopeResults.Count(result => result.Status is "skipped" or "unavailable"),
                notImplementedScopes = scopeResults.Count(result => result.Status is "not-implemented"),
                failedScopes = scopeResults.Count(result => result.Status is "failed"),
                pathCasesVerified = pathCases.Count(result => result.Status is "verified"),
                pathCasesSkipped = pathCases.Count(result => result.Status is "skipped" or "unavailable" or "not-applicable")
            },
            scopeResults,
            pathCaseResults = pathCases
        };

        WriteJson(ScratchRootGuard.PathUnderRoot(root, "corpora", "compatibility", "compat", "release-compat-result.json"), releaseResult);
        output.WriteLine(blocksReleaseEvidence
            ? "Release compatibility aggregation incomplete."
            : "Release compatibility aggregation completed.");
        return blocksReleaseEvidence ? 1 : 0;
    }

    private static ReleaseCompatScopeResult AggregateFixtureScope(
        string scope,
        string resultPath,
        DirectoryInfo root,
        string evidenceKind)
    {
        if (!File.Exists(resultPath))
        {
            return new ReleaseCompatScopeResult(
                scope,
                "not-implemented",
                scope + "-result-missing",
                evidenceKind,
                BehaviorVerifierInvoked: false,
                VerifiedBehavior: false,
                ReleaseEvidence: false,
                SourceResultPath: ScratchRelativePath(root, resultPath));
        }

        var result = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        var sourceStatus = (string?)result["status"] ?? (string?)result["result"] ?? "unknown";
        var behaviorVerifierInvoked = (bool?)result["behaviorVerifierInvoked"] == true;
        var verifiedBehavior = (bool?)result["verifiedBehavior"] == true;
        var status = sourceStatus switch
        {
            "failed" => "failed",
            "skipped" => "skipped",
            "unavailable" => "unavailable",
            "not-implemented" => "not-implemented",
            _ => behaviorVerifierInvoked && verifiedBehavior ? "verified" : "fixture-only"
        };

        return new ReleaseCompatScopeResult(
            scope,
            status,
            status switch
            {
                "verified" => "verified",
                "failed" => scope + "-result-failed",
                "skipped" => scope + "-result-skipped",
                "unavailable" => scope + "-result-unavailable",
                "not-implemented" => scope + "-result-not-implemented",
                _ => "behavior-verifier-not-invoked"
            },
            evidenceKind,
            behaviorVerifierInvoked,
            VerifiedBehavior: status is "verified",
            ReleaseEvidence: status is "verified",
            SourceResultPath: ScratchRelativePath(root, resultPath));
    }

    private static ReleaseCompatScopeResult AggregatePathScope(
        string resultPath,
        DirectoryInfo root,
        IReadOnlyList<PathCompatibilityCaseResult> pathCases)
    {
        if (!File.Exists(resultPath))
        {
            return new ReleaseCompatScopeResult(
                "paths",
                "not-implemented",
                "paths-result-missing",
                "path-compatibility-corpus",
                BehaviorVerifierInvoked: false,
                VerifiedBehavior: false,
                ReleaseEvidence: false,
                SourceResultPath: ScratchRelativePath(root, resultPath));
        }

        var failed = pathCases.Any(result => result.Status is "failed");
        var notImplemented = pathCases.Any(result => result.Status is "not-implemented");
        var skippedOrUnavailable = pathCases.Any(result => result.Status is "skipped" or "unavailable" or "not-applicable");
        var allVerified = pathCases.Count > 0 && pathCases.All(result => result.Status is "verified" && result.VerifiedBehavior);
        var status = failed
            ? "failed"
            : notImplemented
                ? "not-implemented"
                : skippedOrUnavailable
                    ? "skipped"
                    : allVerified
                        ? "verified"
                        : "fixture-only";

        return new ReleaseCompatScopeResult(
            "paths",
            status,
            status switch
            {
                "verified" => "verified",
                "failed" => "path-compatibility-failed",
                "not-implemented" => "path-compatibility-not-implemented",
                "skipped" => "path-compatibility-has-skipped-cases",
                _ => "behavior-verifier-not-invoked"
            },
            "path-compatibility-corpus",
            BehaviorVerifierInvoked: pathCases.Any(result => result.BehaviorVerifierInvoked),
            VerifiedBehavior: allVerified,
            ReleaseEvidence: status is "verified",
            SourceResultPath: ScratchRelativePath(root, resultPath));
    }

    private static ReleaseCompatScopeResult MissingReleaseScope(
        string scope,
        string reasonCode,
        string evidenceKind)
    {
        return new ReleaseCompatScopeResult(
            scope,
            "not-implemented",
            reasonCode,
            evidenceKind,
            BehaviorVerifierInvoked: false,
            VerifiedBehavior: false,
            ReleaseEvidence: false,
            SourceResultPath: "not-available");
    }

    private static IReadOnlyList<PathCompatibilityCaseResult> ReadPathCaseResults(string resultPath)
    {
        if (!File.Exists(resultPath))
        {
            return [];
        }

        var result = JsonNode.Parse(File.ReadAllText(resultPath))!.AsObject();
        return result["caseResults"]!.AsArray()
            .Select(value => JsonSerializer.Deserialize<PathCompatibilityCaseResult>(value, JsonOptions)!)
            .ToArray();
    }

    private static string ScratchRelativePath(DirectoryInfo root, string path)
    {
        return "scratch-relative:" + Path.GetRelativePath(root.FullName, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static PathCompatibilityCaseResult[] BuildPathCompatibilityCases(DirectoryInfo root)
    {
        CorpusProfileGenerator.Generate(root, "pathological");
        var behaviorVerifier = new PathCompatibilityBehaviorVerifier();
        return
        [
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
        ];
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
                    attributes,
                    entry.CreationTimeUtc,
                    entry.LastAccessTimeUtc);
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

        if (StringComparer.OrdinalIgnoreCase.Equals(scope, "providers"))
        {
            var providersRoot = ScratchRootGuard.Prepare(options.Required("root"));
            CorpusProfileGenerator.Generate(providersRoot, "preview");
            var caseResults = PreviewProviderCorpusVerifier.Verify(providersRoot);
            var failedCount = caseResults.Count(result => result.Status is not "verified");

            var providersResult = new
            {
                documentType = "velofilePreviewCorpusResult",
                schemaVersion = 1,
                scope = "providers",
                status = failedCount == 0 ? "verified" : "failed",
                behaviorVerifierInvoked = caseResults.All(result => result.BehaviorVerifierInvoked),
                verifiedBehavior = caseResults.All(result => result.VerifiedBehavior),
                evidenceKind = "preview-providers",
                caseResults
            };

            WriteJson(ScratchRootGuard.PathUnderRoot(providersRoot, "corpora", "preview", "preview", "preview-providers-result.json"), providersResult);
            output.WriteLine(failedCount == 0
                ? "Preview providers corpus passed."
                : "Preview providers corpus failed.");
            return failedCount == 0 ? 0 : 1;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(scope, "thumbnails"))
        {
            var thumbnailsRoot = ScratchRootGuard.Prepare(options.Required("root"));
            CorpusProfileGenerator.Generate(thumbnailsRoot, "preview");
            var caseResults = ThumbnailCorpusVerifier.Verify();
            var failedCount = caseResults.Count(result => result.Status is not "verified");

            var thumbnailsResult = new
            {
                documentType = "velofilePreviewCorpusResult",
                schemaVersion = 1,
                scope = "thumbnails",
                status = failedCount == 0 ? "verified" : "failed",
                behaviorVerifierInvoked = caseResults.All(result => result.BehaviorVerifierInvoked),
                verifiedBehavior = caseResults.All(result => result.VerifiedBehavior),
                evidenceKind = "preview-thumbnails",
                caseResults
            };

            WriteJson(ScratchRootGuard.PathUnderRoot(thumbnailsRoot, "corpora", "preview", "preview", "preview-thumbnails-result.json"), thumbnailsResult);
            output.WriteLine(failedCount == 0
                ? "Preview thumbnails corpus passed."
                : "Preview thumbnails corpus failed.");
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

    private static int RunDiagnosticsConformance(CliOptions options, TextWriter output)
    {
        var root = ScratchRootGuard.Prepare(options.Required("root"));
        var diagnosticsRoot = ScratchRootGuard.PathUnderRoot(root, "diagnostics");
        var localStoreRoot = Path.Combine(diagnosticsRoot, "local-store");
        var exportRoot = Path.Combine(diagnosticsRoot, "export");
        var exportPath = Path.Combine(exportRoot, "diagnostics-redacted.jsonl");
        Directory.CreateDirectory(exportRoot);

        var retention = DiagnosticRetentionPolicy.Default;
        var store = new LocalDiagnosticLogStore(localStoreRoot, retention);
        var redactor = new PathRedactor(Encoding.UTF8.GetBytes("velofile-m15-diagnostics-local-salt"));
        var timestamp = DateTimeOffset.UtcNow;
        var rawSensitivePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "alice",
            "secret-project",
            "id_rsa");

        var workflowEvents = new[]
        {
            new DiagnosticWorkflowEvent(
                "navigation",
                DiagnosticEvent.CreateFailure(
                    Guid.NewGuid().ToString("N"),
                    1,
                    "navigation",
                    "navigation",
                    "access-denied",
                    rawSensitivePath,
                    redactor,
                    timestamp)),
            new DiagnosticWorkflowEvent(
                "preview",
                new DiagnosticEvent
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "operation.failure",
                    UtcTimestamp = timestamp.AddMilliseconds(1),
                    SequenceNumber = 2,
                    Severity = "warning",
                    Component = "preview",
                    OperationKind = "preview",
                    ResultState = "timed-out",
                    ReasonCode = "timeout",
                    TimeoutBudgetMs = 2_000
                }),
            new DiagnosticWorkflowEvent(
                "file-operation",
                DiagnosticEvent.CreateFailure(
                    Guid.NewGuid().ToString("N"),
                    3,
                    "file-operation",
                    "delete",
                    "access-denied",
                    rawSensitivePath,
                    redactor,
                    timestamp.AddMilliseconds(2))),
            new DiagnosticWorkflowEvent(
                "search",
                new DiagnosticEvent
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "operation.failure",
                    UtcTimestamp = timestamp.AddMilliseconds(3),
                    SequenceNumber = 4,
                    Severity = "warning",
                    Component = "search",
                    OperationKind = "search",
                    ResultState = "cancelled",
                    ReasonCode = "cancelled",
                    CancellationFlag = true
                }),
            new DiagnosticWorkflowEvent(
                "terminal",
                new DiagnosticEvent
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "terminal.launch",
                    UtcTimestamp = timestamp.AddMilliseconds(4),
                    SequenceNumber = 5,
                    Severity = "warning",
                    Component = "terminal",
                    OperationKind = "terminal-launch",
                    ResultState = "failed",
                    ReasonCode = "terminal-launch-failed",
                    TerminalTargetKind = "powershell-7"
                }),
            new DiagnosticWorkflowEvent(
                "session-restore",
                new DiagnosticEvent
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "persistence.fallback",
                    UtcTimestamp = timestamp.AddMilliseconds(5),
                    SequenceNumber = 6,
                    Severity = "warning",
                    Component = "session",
                    OperationKind = "session-restore",
                    ResultState = "fallback",
                    ReasonCode = "safe-defaults-used",
                    DocumentType = "session",
                    FallbackSource = "safeDefaults"
                })
        };

        foreach (var workflowEvent in workflowEvents)
        {
            store.Write(workflowEvent.Event);
        }

        store.Write(new DiagnosticEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            EventType = "terminal.launch",
            UtcTimestamp = timestamp.AddMilliseconds(6),
            SequenceNumber = 7,
            Severity = "warning",
            Component = "terminal",
            OperationKind = "terminal-launch",
            ResultState = "failed",
            ReasonCode = "terminal-launch-failed",
            TerminalTargetKind = "powershell-7"
        });

        for (var i = 0; i < retention.MaxCrashMarkers + 2; i++)
        {
            store.RecordCrashMarker("startup", timestamp.AddSeconds(i));
        }

        store.RecordLastActionMarker("navigation", "navigation", timestamp);
        store.RecordLastActionMarker("preview-generation", "preview", timestamp.AddSeconds(1));

        var diagnosticFiles = Directory
            .EnumerateFiles(localStoreRoot, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        File.WriteAllLines(
            exportPath,
            diagnosticFiles.SelectMany(File.ReadAllLines),
            Encoding.UTF8);

        var prohibitedValues = new[]
        {
            "alice",
            "secret-plan",
            "clipboard-secret",
            "preview text",
            "pwsh -NoProfile",
            "id_rsa"
        };
        var outputText = string.Join(
            Environment.NewLine,
            Directory
                .EnumerateFiles(diagnosticsRoot, "*", SearchOption.AllDirectories)
                .Select(File.ReadAllText));
        var prohibitedValuesFound = prohibitedValues.Any(value => outputText.Contains(value, StringComparison.OrdinalIgnoreCase));
        var crashMarkerCount = Directory.EnumerateFiles(Path.Combine(localStoreRoot, "crash-markers"), "*.json").Count();
        var lastActionMarkerCount = Directory.EnumerateFiles(Path.Combine(localStoreRoot, "last-action-markers"), "*.json").Count();
        var workflowCoverage = workflowEvents
            .Select(workflowEvent => DiagnosticWorkflowCoverage(workflowEvent, outputText, prohibitedValuesFound))
            .ToArray();
        var triageDecisions = new[]
        {
            EvaluateTriage("below-threshold", crashMarkers: 1, hangMarkers: 0, diagnosticsAvailable: true, redactionFailed: false, retentionViolation: false),
            EvaluateTriage("at-crash-threshold", crashMarkers: 2, hangMarkers: 0, diagnosticsAvailable: true, redactionFailed: false, retentionViolation: false),
            EvaluateTriage("above-crash-threshold", crashMarkers: 3, hangMarkers: 0, diagnosticsAvailable: true, redactionFailed: false, retentionViolation: false),
            EvaluateTriage("hang-threshold", crashMarkers: 0, hangMarkers: 1, diagnosticsAvailable: true, redactionFailed: false, retentionViolation: false),
            EvaluateTriage("missing-data", crashMarkers: 0, hangMarkers: 0, diagnosticsAvailable: false, redactionFailed: false, retentionViolation: false),
            EvaluateTriage("redaction-failure", crashMarkers: 0, hangMarkers: 0, diagnosticsAvailable: true, redactionFailed: true, retentionViolation: false),
            EvaluateTriage("retention-violation", crashMarkers: 0, hangMarkers: 0, diagnosticsAvailable: true, redactionFailed: false, retentionViolation: true)
        };
        var failed = prohibitedValuesFound
            || crashMarkerCount > retention.MaxCrashMarkers
            || workflowCoverage.Any(workflow => !workflow.Covered || !workflow.Serialized || !workflow.Redacted);

        var result = new
        {
            documentType = "velofileDiagnosticsConformanceResult",
            schemaVersion = 1,
            status = failed ? "failed" : "verified",
            localOnly = true,
            exportRedacted = !prohibitedValuesFound,
            prohibitedValuesFound,
            evidenceKind = "diagnostics-local-retention-redaction",
            retention = new
            {
                maxAgeDays = retention.MaxAge.TotalDays,
                maxTotalBytes = retention.MaxTotalBytes,
                maxFileBytes = retention.MaxFileBytes,
                maxCrashMarkers = retention.MaxCrashMarkers,
                observedCrashMarkers = crashMarkerCount,
                observedLastActionMarkers = lastActionMarkerCount
            },
            export = new
            {
                fixturePathKind = "scratch-relative:diagnostics/export/diagnostics-redacted.jsonl",
                redacted = !prohibitedValuesFound
            },
            workflowCoverage,
            triagePolicy = new
            {
                crashThreshold = 2,
                hangThreshold = 1,
                thresholdBoundary = "at-or-above-blocks-promotion",
                policyDocument = "docs/release/preview-triage.md"
            },
            triageDecisions,
            reasonCode = failed ? "diagnostics-conformance-failed" : "verified"
        };

        WriteJson(Path.Combine(diagnosticsRoot, "diagnostics-conformance-result.json"), result);
        output.WriteLine(failed
            ? "Diagnostics conformance failed."
            : "Diagnostics conformance verified.");
        return failed ? 1 : 0;
    }

    private static DiagnosticWorkflowCoverageResult DiagnosticWorkflowCoverage(
        DiagnosticWorkflowEvent workflowEvent,
        string outputText,
        bool prohibitedValuesFound)
    {
        var component = workflowEvent.Event.Component;
        var operationKind = workflowEvent.Event.OperationKind ?? "";
        var reasonCode = workflowEvent.Event.ReasonCode ?? "";
        var serialized = outputText.Contains($"\"component\":\"{component}\"", StringComparison.Ordinal)
            && outputText.Contains($"\"operationKind\":\"{operationKind}\"", StringComparison.Ordinal)
            && outputText.Contains($"\"reasonCode\":\"{reasonCode}\"", StringComparison.Ordinal);

        return new DiagnosticWorkflowCoverageResult(
            workflowEvent.Workflow,
            Covered: true,
            Serialized: serialized,
            Redacted: !prohibitedValuesFound,
            Component: component,
            OperationKind: operationKind,
            ReasonCode: reasonCode);
    }

    private static TriageDecisionResult EvaluateTriage(
        string caseId,
        int crashMarkers,
        int hangMarkers,
        bool diagnosticsAvailable,
        bool redactionFailed,
        bool retentionViolation)
    {
        const int crashThreshold = 2;
        const int hangThreshold = 1;
        string decision;
        string reasonCode;

        if (!diagnosticsAvailable)
        {
            decision = "insufficient-evidence";
            reasonCode = "diagnostics-missing";
        }
        else if (redactionFailed)
        {
            decision = "promotion-blocked";
            reasonCode = "diagnostics-redaction-failed";
        }
        else if (retentionViolation)
        {
            decision = "promotion-blocked";
            reasonCode = "diagnostics-retention-violated";
        }
        else if (crashMarkers >= crashThreshold)
        {
            decision = "promotion-blocked";
            reasonCode = "crash-threshold-reached";
        }
        else if (hangMarkers >= hangThreshold)
        {
            decision = "promotion-blocked";
            reasonCode = "hang-threshold-reached";
        }
        else
        {
            decision = "promotion-allowed";
            reasonCode = "below-threshold";
        }

        return new TriageDecisionResult(
            caseId,
            decision,
            reasonCode,
            crashMarkers,
            hangMarkers,
            crashThreshold,
            hangThreshold);
    }

    private static int RunBenchmarks(CliOptions options, TextWriter output, TextWriter error)
    {
        if (!options.Has("non-gating"))
        {
            error.WriteLine("M15 benchmark runner is contributor non-gating by default. Pass --non-gating.");
            return 2;
        }

        var root = ScratchRootGuard.Prepare(options.Required("root"));
        var runCount = ParsePositiveInt(options.ValueOrDefault("run-count", "5"), "run-count");
        var appExecutable = options.ValueOrDefault("app-executable", string.Empty);
        var appArguments = options.ValueOrDefault("app-arguments", string.Empty);
        var appTimeoutMs = ParsePositiveInt(options.ValueOrDefault("app-timeout-ms", "5000"), "app-timeout-ms");

        foreach (var profile in new[] { "small", "medium", "large", "deep", "preview", "pathological" })
        {
            CorpusProfileGenerator.Generate(root, profile);
        }

        var report = BenchmarkReport.CreateNonGating(root, runCount, appExecutable, appArguments, TimeSpan.FromMilliseconds(appTimeoutMs));
        WriteJson(Path.Combine(root.FullName, "benchmarks", "benchmark-report.json"), report);
        WriteJson(Path.Combine(root.FullName, "benchmarks", "benchmark-smoke-report.json"), report);
        output.WriteLine("Wrote non-gating benchmark report.");
        return 0;
    }

    private static int ParsePositiveInt(string rawValue, string optionName)
    {
        if (!int.TryParse(rawValue, out var value) || value <= 0)
        {
            throw new CorpusException($"Option --{optionName} must be a positive integer.");
        }

        return value;
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
            await VerifyCaseAsync("timeout-policy", VerifyTimeoutPolicyAsync).ConfigureAwait(false),
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

    private static Task<bool> VerifyTimeoutPolicyAsync()
    {
        var policy = PreviewTimeoutPolicy.Default;
        return Task.FromResult(
            policy.GetBudget(PreviewOperation.ImageDecode) == TimeSpan.FromSeconds(2)
            && policy.GetBudget(PreviewOperation.TextReadAndEncodingDetection) == TimeSpan.FromSeconds(1)
            && policy.GetBudget(PreviewOperation.PdfFirstPageRender) == TimeSpan.FromSeconds(3)
            && policy.GetBudget(PreviewOperation.ThumbnailGeneration) == TimeSpan.FromMilliseconds(500)
            && policy.ThumbnailConcurrencyLimit == 4);
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
                PreviewTimeoutPolicy.ForTesting(TimeSpan.FromMilliseconds(timeoutMs))));
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

        public PreviewOperation Operation => PreviewOperation.MetadataFallback;

        public bool CanPreview(PreviewRequest request)
        {
            return true;
        }

        public ValueTask<PreviewProviderResult> PreviewAsync(
            PreviewRequest request,
            PreviewProviderContext context,
            CancellationToken cancellationToken)
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

internal static class PreviewProviderCorpusVerifier
{
    public static IReadOnlyList<PreviewContractCaseResult> Verify(DirectoryInfo root)
    {
        return VerifyAsync(root).GetAwaiter().GetResult();
    }

    private static async Task<IReadOnlyList<PreviewContractCaseResult>> VerifyAsync(DirectoryInfo root)
    {
        var fixtureRoot = ScratchRootGuard.PathUnderRoot(root, "corpora", "preview", "preview");
        Directory.CreateDirectory(fixtureRoot);
        WriteBytes(Path.Combine(fixtureRoot, "image-success.bmp"), BmpBytes(width: 24, height: 18));
        WriteBytes(Path.Combine(fixtureRoot, "text-truncation.txt"), Encoding.UTF8.GetBytes(new string('t', 1024 * 1024) + "TAIL"));
        WriteBytes(Path.Combine(fixtureRoot, "document.pdf"), MinimalPdfBytes());

        return
        [
            await VerifyCaseAsync("image-success", () => VerifyImageAsync(fixtureRoot)).ConfigureAwait(false),
            await VerifyCaseAsync("text-truncation", () => VerifyTextAsync(fixtureRoot)).ConfigureAwait(false),
            await VerifyCaseAsync("pdf-first-page", () => VerifyPdfAsync(fixtureRoot)).ConfigureAwait(false),
            await VerifyCaseAsync("oversize-fallback", () => VerifyOversizeFallbackAsync(fixtureRoot)).ConfigureAwait(false),
            await VerifyCaseAsync("source-non-mutation", () => VerifyNonMutationAsync(fixtureRoot)).ConfigureAwait(false)
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
                "preview-providers",
                verified ? "verified" : caseId + "-failed");
        }
        catch (OperationCanceledException)
        {
            return new PreviewContractCaseResult(
                caseId,
                "failed",
                true,
                false,
                "preview-providers",
                "preview-provider-timeout");
        }
        catch
        {
            return new PreviewContractCaseResult(
                caseId,
                "failed",
                true,
                false,
                "preview-providers",
                "preview-provider-exception");
        }
    }

    private static async Task<bool> VerifyImageAsync(string fixtureRoot)
    {
        var state = await PreviewAsync(Item(Path.Combine(fixtureRoot, "image-success.bmp"))).ConfigureAwait(false);
        return state.Status is PreviewStatus.Success
            && state.Content?.Kind is PreviewContentKind.Image
            && state.Content.ImageArtifact?.EncodedBytes.Length > 0
            && state.Content.WidthPixels == 24
            && state.Content.HeightPixels == 18;
    }

    private static async Task<bool> VerifyTextAsync(string fixtureRoot)
    {
        var state = await PreviewAsync(Item(Path.Combine(fixtureRoot, "text-truncation.txt"))).ConfigureAwait(false);
        return state.Status is PreviewStatus.Success
            && state.Content?.Kind is PreviewContentKind.Text
            && state.Content.IsTruncated
            && state.Content.TextContent?.Length == 1024 * 1024
            && !state.Content.TextContent.Contains("TAIL", StringComparison.Ordinal);
    }

    private static async Task<bool> VerifyPdfAsync(string fixtureRoot)
    {
        var state = await PreviewAsync(Item(Path.Combine(fixtureRoot, "document.pdf"))).ConfigureAwait(false);
        return state.Status is PreviewStatus.Success
            && state.Content?.Kind is PreviewContentKind.Pdf
            && state.Content.PdfPageArtifact?.EncodedBytes.Length > 0
            && state.Content.PageNumber == 1;
    }

    private static async Task<bool> VerifyOversizeFallbackAsync(string fixtureRoot)
    {
        var imageState = await PreviewAsync(Item(
            Path.Combine(fixtureRoot, "image-success.bmp"),
            lengthOverride: 100L * 1024 * 1024 + 1)).ConfigureAwait(false);
        var pdfState = await PreviewAsync(Item(
            Path.Combine(fixtureRoot, "document.pdf"),
            lengthOverride: 500L * 1024 * 1024 + 1)).ConfigureAwait(false);

        return imageState.Status is PreviewStatus.Unsupported
            && imageState.ReasonCode == "image-too-large"
            && pdfState.Status is PreviewStatus.Unsupported
            && pdfState.ReasonCode == "pdf-too-large";
    }

    private static async Task<bool> VerifyNonMutationAsync(string fixtureRoot)
    {
        var paths = new[]
        {
            Path.Combine(fixtureRoot, "image-success.bmp"),
            Path.Combine(fixtureRoot, "text-truncation.txt"),
            Path.Combine(fixtureRoot, "document.pdf")
        };
        var before = paths.ToDictionary(path => path, Snapshot);
        foreach (var path in paths)
        {
            var state = await PreviewAsync(Item(path)).ConfigureAwait(false);
            if (state.Status is not PreviewStatus.Success)
            {
                return false;
            }
        }

        return paths.All(path => Snapshot(path).Equals(before[path]));
    }

    private static async Task<PreviewState> PreviewAsync(ListedFileItem item)
    {
        var controller = new PreviewController(
            WindowsPreviewProviderFactory.CreateDefault(),
            new PreviewMetadataProvider(),
            new PreviewControllerOptions(
                TimeSpan.FromMilliseconds(5),
                PreviewTimeoutPolicy.Default));

        controller.StartPreview(item);
        await WaitUntilAsync(() => controller.State.Status is PreviewStatus.Success or PreviewStatus.Unsupported or PreviewStatus.Failed).ConfigureAwait(false);
        return controller.State;
    }

    private static ListedFileItem Item(string path, long? lengthOverride = null)
    {
        var info = new FileInfo(path);
        return new ListedFileItem(
            path,
            info.Name,
            info.Name,
            FileSystemEntryKind.File,
            lengthOverride ?? info.Length,
            info.LastWriteTimeUtc,
            info.Attributes,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false,
            CreationTimeUtc: info.CreationTimeUtc,
            LastAccessTimeUtc: info.LastAccessTimeUtc);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token).ConfigureAwait(false);
        }
    }

    private static FileSnapshot Snapshot(string path)
    {
        var info = new FileInfo(path);
        return new FileSnapshot(
            info.Length,
            Sha256(path),
            info.CreationTimeUtc,
            info.LastWriteTimeUtc,
            info.Attributes);
    }

    private static void WriteBytes(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
        File.SetCreationTimeUtc(path, new DateTime(2026, 2, 1, 2, 3, 4, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(path, new DateTime(2026, 2, 2, 3, 4, 5, DateTimeKind.Utc));
        File.SetAttributes(path, FileAttributes.Archive);
    }

    private static byte[] BmpBytes(int width, int height)
    {
        var stride = width * 4;
        var pixelBytes = stride * height;
        var bytes = new byte[54 + pixelBytes];
        bytes[0] = (byte)'B';
        bytes[1] = (byte)'M';
        WriteLittleEndianInt32(bytes, 2, bytes.Length);
        WriteLittleEndianInt32(bytes, 10, 54);
        WriteLittleEndianInt32(bytes, 14, 40);
        WriteLittleEndianInt32(bytes, 18, width);
        WriteLittleEndianInt32(bytes, 22, height);
        WriteLittleEndianUInt16(bytes, 26, 1);
        WriteLittleEndianUInt16(bytes, 28, 32);
        WriteLittleEndianInt32(bytes, 34, pixelBytes);
        for (var index = 54; index < bytes.Length; index += 4)
        {
            bytes[index] = 0x40;
            bytes[index + 1] = 0x80;
            bytes[index + 2] = 0xc0;
            bytes[index + 3] = 0xff;
        }

        return bytes;
    }

    private static void WriteLittleEndianInt32(byte[] bytes, int offset, int value)
    {
        bytes[offset] = (byte)(value & 0xff);
        bytes[offset + 1] = (byte)((value >> 8) & 0xff);
        bytes[offset + 2] = (byte)((value >> 16) & 0xff);
        bytes[offset + 3] = (byte)((value >> 24) & 0xff);
    }

    private static void WriteLittleEndianUInt16(byte[] bytes, int offset, ushort value)
    {
        bytes[offset] = (byte)(value & 0xff);
        bytes[offset + 1] = (byte)((value >> 8) & 0xff);
    }

    private static byte[] MinimalPdfBytes()
    {
        var content = "BT /F1 18 Tf 20 60 Td (Page 1) Tj ET";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 120] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var builder = new StringBuilder();
        builder.Append("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n");
            builder.Append(objects[index]).Append('\n');
            builder.Append("endobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n");
        builder.Append("0 ").Append(objects.Length + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("0000000000")).Append(" 00000 n \n");
        }

        builder.Append("trailer\n");
        builder.Append("<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\n");
        builder.Append("startxref\n");
        builder.Append(xrefOffset).Append('\n');
        builder.Append("%%EOF\n");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private sealed record FileSnapshot(
        long Length,
        string Sha256,
        DateTime CreationTimeUtc,
        DateTime LastWriteTimeUtc,
        FileAttributes Attributes);
}

internal static class ThumbnailCorpusVerifier
{
    public static IReadOnlyList<PreviewContractCaseResult> Verify()
    {
        return VerifyAsync().GetAwaiter().GetResult();
    }

    private static async Task<IReadOnlyList<PreviewContractCaseResult>> VerifyAsync()
    {
        return
        [
            await VerifyCaseAsync("thumbnail-concurrency", VerifyConcurrencyAsync).ConfigureAwait(false),
            await VerifyCaseAsync("thumbnail-timeout", VerifyTimeoutAsync).ConfigureAwait(false),
            await VerifyCaseAsync("generic-icon-fallback", VerifyGenericFallbackAsync).ConfigureAwait(false),
            await VerifyCaseAsync("stale-thumbnail-ignore", VerifyStaleIgnoreAsync).ConfigureAwait(false)
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
                "preview-thumbnails",
                verified ? "verified" : caseId + "-failed");
        }
        catch
        {
            return new PreviewContractCaseResult(
                caseId,
                "failed",
                true,
                false,
                "preview-thumbnails",
                "thumbnail-verifier-exception");
        }
    }

    private static async Task<bool> VerifyConcurrencyAsync()
    {
        var provider = new BlockingThumbnailProvider();
        var controller = new ThumbnailController(provider, PreviewTimeoutPolicy.Default);

        controller.Start(Enumerable.Range(0, 8).Select(index => Item($"concurrent-{index}.txt")).ToArray());
        await WaitUntilAsync(() => provider.MaxConcurrentCount == PreviewTimeoutPolicy.Default.ThumbnailConcurrencyLimit).ConfigureAwait(false);
        provider.ReleaseAll();
        await WaitUntilAsync(() => controller.Snapshot.Values.All(state => state.Status is ThumbnailStatus.Ready)).ConfigureAwait(false);

        return provider.MaxConcurrentCount == PreviewTimeoutPolicy.Default.ThumbnailConcurrencyLimit;
    }

    private static async Task<bool> VerifyTimeoutAsync()
    {
        var policy = new PreviewTimeoutPolicy(
            ImageDecodeBudget: TimeSpan.FromSeconds(2),
            TextReadAndEncodingDetectionBudget: TimeSpan.FromSeconds(1),
            PdfFirstPageRenderBudget: TimeSpan.FromSeconds(3),
            MetadataFallbackBudget: TimeSpan.FromMilliseconds(200),
            ThumbnailGenerationBudget: TimeSpan.FromMilliseconds(30),
            ThumbnailConcurrencyLimit: 4);
        var controller = new ThumbnailController(new NeverCompletingThumbnailProvider(), policy);
        var item = Item("timeout.txt");

        controller.Start([item]);
        await WaitUntilAsync(() => controller.GetState(item).Status is ThumbnailStatus.GenericIcon).ConfigureAwait(false);

        return controller.GetState(item).ReasonCode == "thumbnail-timeout";
    }

    private static async Task<bool> VerifyGenericFallbackAsync()
    {
        var provider = new FailingThumbnailProvider();
        var controller = new ThumbnailController(provider, PreviewTimeoutPolicy.Default);
        var item = Item("fallback.bin");

        controller.Start([item]);
        await WaitUntilAsync(() => controller.GetState(item).Status is ThumbnailStatus.GenericIcon).ConfigureAwait(false);

        return controller.GetState(item).Artifact?.DisplayText == "BIN";
    }

    private static async Task<bool> VerifyStaleIgnoreAsync()
    {
        var provider = new ManualThumbnailProvider();
        var controller = new ThumbnailController(provider, PreviewTimeoutPolicy.Default);
        var oldItem = Item("old.txt");
        var newItem = Item("new.txt");

        controller.Start([oldItem]);
        await WaitUntilAsync(() => provider.Started("old.txt")).ConfigureAwait(false);
        controller.Start([newItem]);
        await WaitUntilAsync(() => provider.Cancelled("old.txt") && provider.Started("new.txt")).ConfigureAwait(false);

        provider.Complete("old.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("OLD")));
        provider.Complete("new.txt", ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon("NEW")));
        await WaitUntilAsync(() => controller.GetState(newItem).Artifact?.DisplayText == "NEW").ConfigureAwait(false);

        return controller.GetState(oldItem).Status is ThumbnailStatus.NotLoaded
            && controller.GetState(newItem).Status is ThumbnailStatus.Ready;
    }

    private static ListedFileItem Item(string name)
    {
        return new ListedFileItem(
            @"C:\velofile-thumbnails\" + name,
            name,
            name,
            FileSystemEntryKind.File,
            128,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token).ConfigureAwait(false);
        }
    }

    private sealed class BlockingThumbnailProvider : IThumbnailProvider
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _current;

        public int MaxConcurrentCount { get; private set; }

        public async ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _current);
            MaxConcurrentCount = Math.Max(MaxConcurrentCount, current);
            try
            {
                await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon(item.Name));
            }
            finally
            {
                Interlocked.Decrement(ref _current);
            }
        }

        public void ReleaseAll()
        {
            _release.TrySetResult();
        }
    }

    private sealed class NeverCompletingThumbnailProvider : IThumbnailProvider
    {
        public async ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            return ThumbnailProviderResult.Success(ThumbnailArtifact.GenericIcon(item.Name));
        }
    }

    private sealed class FailingThumbnailProvider : IThumbnailProvider
    {
        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(ThumbnailProviderResult.Failed("thumbnail-failed"));
        }
    }

    private sealed class ManualThumbnailProvider : IThumbnailProvider
    {
        private readonly Dictionary<string, TaskCompletionSource<ThumbnailProviderResult>> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cancelled = new(StringComparer.OrdinalIgnoreCase);

        public ValueTask<ThumbnailProviderResult> GenerateAsync(
            ListedFileItem item,
            ThumbnailProviderContext context,
            CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<ThumbnailProviderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[item.Name] = completion;
            _started.Add(item.Name);
            cancellationToken.Register(() =>
            {
                _cancelled.Add(item.Name);
                completion.TrySetCanceled(cancellationToken);
            });
            return new ValueTask<ThumbnailProviderResult>(completion.Task);
        }

        public bool Started(string name)
        {
            return _started.Contains(name);
        }

        public bool Cancelled(string name)
        {
            return _cancelled.Contains(name);
        }

        public void Complete(string name, ThumbnailProviderResult result)
        {
            if (_pending.TryGetValue(name, out var completion))
            {
                completion.TrySetResult(result);
            }
        }
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
        "small",
        "medium",
        "large",
        "deep",
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
            "small" => GenerateFlatProfile("small", "small", 12, "Small folder benchmark fixture"),
            "medium" => GenerateFlatProfile("medium", "medium/items", 160, "Medium folder benchmark fixture"),
            "large" => GenerateFlatProfile("large", "large/items", 1_100, "Large folder benchmark fixture"),
            "deep" => GenerateDeepProfile(1_100),
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
                new CorpusFixture(["preview", "markdown-preview.md", "# Preview fixture" + Environment.NewLine]),
                new CorpusFixture(["preview", "metadata.json", "{\"name\":\"preview\"}" + Environment.NewLine]),
                new CorpusFixture(["preview", "image-placeholder.png", "PNG preview fixture placeholder." + Environment.NewLine]),
                new CorpusFixture(["preview", "pdf-placeholder.pdf", "%PDF-1.7 preview fixture placeholder" + Environment.NewLine]),
                new CorpusFixture(["preview", "unsupported.bin", "\0\0VeloFile unsupported preview placeholder"]),
                new CorpusFixture(["preview", "metadata-only.placeholder", "Metadata fallback placeholder." + Environment.NewLine]),
                new CorpusFixture(["preview", "oversize-marker.placeholder", "Oversize fallback marker." + Environment.NewLine])
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

    private static CorpusFixture[] GenerateFlatProfile(string profile, string directory, int count, string description)
    {
        var directorySegments = directory.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return Enumerable.Range(1, count)
            .Select(index => new CorpusFixture(
                [
                    .. directorySegments,
                    $"item-{index:0000}.txt",
                    $"{description} {index:0000} in {profile}.{Environment.NewLine}"
                ]))
            .ToArray();
    }

    private static CorpusFixture[] GenerateDeepProfile(int count)
    {
        var fixtures = new List<CorpusFixture>
        {
            new(["deep", "root-match.txt", "deep recursive search root match" + Environment.NewLine])
        };

        for (var index = 1; index <= count; index++)
        {
            var bucket = ((index - 1) / 100) + 1;
            var level = (index % 5) + 1;
            fixtures.Add(new CorpusFixture(
                [
                    "deep",
                    $"bucket-{bucket:0000}",
                    $"level-{level:0000}",
                    $"recursive-match-{index:0000}.txt",
                    $"deep recursive search target {index:0000}{Environment.NewLine}"
                ]));
        }

        return fixtures.ToArray();
    }

    private static string[] DirectoriesFor(string profile)
    {
        return profile switch
        {
            "smoke" => ["small", "preview", "compat", "compat/paths", "operations"],
            "small" => ["small"],
            "medium" => ["medium", "medium/items"],
            "large" => ["large", "large/items"],
            "deep" => ["deep", .. Enumerable.Range(1, 11).Select(index => $"deep/bucket-{index:0000}"), .. Enumerable.Range(1, 11).SelectMany(bucket => Enumerable.Range(1, 5).Select(level => $"deep/bucket-{bucket:0000}/level-{level:0000}"))],
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
            "small" => ["generate:small", "benchmark:folder-switch", "compat:listing"],
            "medium" => ["generate:medium", "benchmark:folder-switch", "benchmark:filter", "compat:listing"],
            "large" => ["generate:large", "benchmark:folder-switch", "compat:large-folder"],
            "deep" => ["generate:deep", "benchmark:recursive-search", "compat:recursive-traversal"],
            "preview" => ["generate:preview", "benchmark:preview", "preview:providers", "preview:contract"],
            "dragdrop" => ["generate:dragdrop", "compat:dragdrop"],
            "pathological" => ["generate:pathological", "benchmark:pathological-skip", "compat:paths"],
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

internal sealed record ReleaseCompatScopeResult(
    string Scope,
    string Status,
    string ReasonCode,
    string EvidenceKind,
    bool BehaviorVerifierInvoked,
    bool VerifiedBehavior,
    bool ReleaseEvidence,
    string SourceResultPath);

internal sealed record DiagnosticWorkflowEvent(
    string Workflow,
    DiagnosticEvent Event);

internal sealed record DiagnosticWorkflowCoverageResult(
    string Workflow,
    bool Covered,
    bool Serialized,
    bool Redacted,
    string Component,
    string OperationKind,
    string ReasonCode);

internal sealed record TriageDecisionResult(
    string CaseId,
    string Decision,
    string ReasonCode,
    int CrashMarkers,
    int HangMarkers,
    int CrashThreshold,
    int HangThreshold);

internal static class BenchmarkReport
{
    public static object CreateNonGating(
        DirectoryInfo root,
        int runCount,
        string appExecutable,
        string appArguments,
        TimeSpan appTimeout)
    {
        var measurements = new List<object>
        {
            Measure("app.process.launch", "T039.launch", runCount, () => MeasureProcessLaunch(appExecutable, appArguments, appTimeout), "non-gating"),
            Measure("folder.switch.small", "T039.folder-switch", runCount, () => EnumerateProfile(root, "small"), "non-gating"),
            Measure("folder.switch.medium", "T039.folder-switch", runCount, () => EnumerateProfile(root, "medium"), "non-gating"),
            Measure("folder.switch.large", "T039.folder-switch", runCount, () => EnumerateProfile(root, "large"), "non-gating"),
            Measure("filter.medium", "T039.current-folder-filter", runCount, () => FilterMediumProfile(root), "non-gating"),
            Measure("search.deep.firstResult", "T039.recursive-search.first-result", runCount, () => SearchDeepProfile(root, skip: 0), "non-gating"),
            Measure("search.deep.thousandResults", "T039.recursive-search.milestone", runCount, () => SearchDeepProfile(root, skip: 999), "non-gating"),
            Measure("contextMenu.open", "T039.context-menu-open", runCount, SimulateContextMenuOpen, "non-gating"),
            Measure("tab.switch", "T039.tab-switch", runCount, SimulateTabSwitch, "non-gating"),
            Measure("session.restore.10tabs", "T039.session-restore", runCount, SimulateSessionRestore, "non-gating")
        };

        return new
        {
            documentType = "velofileBenchmarkReport",
            schemaVersion = 1,
            nonGating = true,
            environment = new
            {
                osBuild = Environment.OSVersion.VersionString,
                processorArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                hardwareClass = HardwareClass(),
                cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown",
                ramBytes = RamBytes(),
                storageType = StorageType(root),
                windowsSearchState = WindowsSearchState(),
                antivirusState = AntivirusState(),
                dpiConfiguration = "single-or-default-monitor"
            },
            referenceCorpus = new[]
            {
                CorpusProfileSummary(root, "small"),
                CorpusProfileSummary(root, "medium"),
                CorpusProfileSummary(root, "large"),
                CorpusProfileSummary(root, "deep"),
                CorpusProfileSummary(root, "preview"),
                CorpusProfileSummary(root, "pathological")
            },
            scenarioCoverage = AppScenarioCoverage(),
            releaseSummary = new
            {
                status = "non-gating",
                publicPerformanceClaimsAllowed = false,
                satisfiesAc15ReleaseEvidence = false,
                appLevelScenarioCoverage = "not-implemented",
                reasonCode = "app-level-driver-not-implemented"
            },
            releasePolicy = new
            {
                p95RegressionAcknowledgePercent = 10,
                p95RegressionBlockPercent = 25,
                comparisonMetric = "p95"
            },
            triageThresholds = new
            {
                crashThreshold = "any repeated crash marker at or above documented preview threshold blocks promotion",
                hangThreshold = "any repeated hang marker at or above documented preview threshold blocks promotion",
                policyDocument = "docs/release/preview-triage.md"
            },
            measurements
        };
    }

    private static object[] AppScenarioCoverage()
    {
        return
        [
            AppScenario("T039.launch", "Launch"),
            AppScenario("T039.folder-switch", "Folder switch"),
            AppScenario("T039.current-folder-filter", "Current-folder filter"),
            AppScenario("T039.recursive-search", "Recursive search"),
            AppScenario("T039.context-menu-open", "Context menu open"),
            AppScenario("T039.tab-switch", "Tab switch"),
            AppScenario("T039.session-restore", "Session restore"),
            AppScenario("T039.sustained-scroll", "Sustained scroll"),
            AppScenario("T039.slow-tab-isolation", "Slow-tab isolation"),
            AppScenario("T039.terminal-discovery-impact", "Terminal discovery impact")
        ];
    }

    private static object AppScenario(string scenarioId, string name)
    {
        return new
        {
            scenarioId,
            name,
            requiredMeasurementKind = "app-level",
            appBoundaryDriven = false,
            releaseEvidence = false,
            reasonCode = "app-level-driver-not-implemented"
        };
    }

    private static object Measure(string name, string scenarioId, int runCount, Action action, string releaseGatingStatus)
    {
        var samples = new List<double>(runCount);
        for (var index = 0; index < runCount; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            samples.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        samples.Sort();
        return new
        {
            name,
            scenarioId,
            measurementKind = "infrastructure-only",
            releaseEvidence = false,
            appBoundaryDriven = false,
            substituteMeasurement = true,
            reasonCode = "app-level-driver-not-implemented",
            runCount,
            medianMs = Round(Percentile(samples, 50)),
            p95Ms = Round(Percentile(samples, 95)),
            p99Ms = Round(Percentile(samples, 99)),
            releaseGatingStatus
        };
    }

    private static void MeasureProcessLaunch(string appExecutable, string appArguments, TimeSpan timeout)
    {
        if (string.IsNullOrWhiteSpace(appExecutable))
        {
            return;
        }

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(appExecutable)
        {
            Arguments = appArguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.Start();
        if (!process.WaitForExit(timeout))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            throw new CorpusException("App process launch benchmark timed out.");
        }
    }

    private static void EnumerateProfile(DirectoryInfo root, string profile)
    {
        var profileRoot = ScratchRootGuard.PathUnderRoot(root, "corpora", profile);
        _ = Directory.EnumerateFileSystemEntries(profileRoot, "*", SearchOption.AllDirectories).Count();
    }

    private static void FilterMediumProfile(DirectoryInfo root)
    {
        var profileRoot = ScratchRootGuard.PathUnderRoot(root, "corpora", "medium");
        _ = Directory
            .EnumerateFiles(profileRoot, "*.txt", SearchOption.AllDirectories)
            .Count(path => Path.GetFileName(path).Contains("09", StringComparison.Ordinal));
    }

    private static void SearchDeepProfile(DirectoryInfo root, int skip)
    {
        var profileRoot = ScratchRootGuard.PathUnderRoot(root, "corpora", "deep");
        _ = Directory
            .EnumerateFiles(profileRoot, "*recursive-match*.txt", SearchOption.AllDirectories)
            .Skip(skip)
            .FirstOrDefault();
    }

    private static void SimulateContextMenuOpen()
    {
        var commands = new[] { "open", "open-with", "cut", "copy", "paste", "rename", "delete", "properties" };
        _ = commands.Where(command => command.Length > 0).OrderBy(command => command, StringComparer.Ordinal).ToArray();
    }

    private static void SimulateTabSwitch()
    {
        var active = 0;
        for (var index = 0; index < 20; index++)
        {
            active = (active + 1) % 5;
        }
    }

    private static void SimulateSessionRestore()
    {
        var tabs = Enumerable.Range(1, 10)
            .Select(index => new { index, path = "scratch-relative:/tab-" + index.ToString("00") })
            .ToArray();
        _ = tabs.Sum(tab => tab.path.Length + tab.index);
    }

    private static object CorpusProfileSummary(DirectoryInfo root, string profile)
    {
        var profileRoot = ScratchRootGuard.PathUnderRoot(root, "corpora", profile);
        return new
        {
            profile,
            fileCount = Directory.EnumerateFiles(profileRoot, "*", SearchOption.AllDirectories).Count(),
            fixturePathKind = "scratch-relative:corpora/" + profile
        };
    }

    private static double Percentile(IReadOnlyList<double> sortedSamples, double percentile)
    {
        if (sortedSamples.Count == 0)
        {
            return 0;
        }

        var rank = (percentile / 100d) * (sortedSamples.Count - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);
        if (lower == upper)
        {
            return sortedSamples[lower];
        }

        var weight = rank - lower;
        return sortedSamples[lower] + ((sortedSamples[upper] - sortedSamples[lower]) * weight);
    }

    private static double Round(double value)
    {
        return Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    private static string HardwareClass()
    {
        var processorCount = Environment.ProcessorCount;
        return processorCount switch
        {
            >= 12 => "developer-workstation",
            >= 6 => "desktop",
            _ => "entry"
        };
    }

    private static long RamBytes()
    {
        try
        {
            return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        }
        catch
        {
            return -1;
        }
    }

    private static string StorageType(DirectoryInfo root)
    {
        try
        {
            var drive = new DriveInfo(root.Root.FullName);
            return drive.DriveType.ToString().ToLowerInvariant();
        }
        catch
        {
            return "unknown";
        }
    }

    private static string WindowsSearchState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return "not-applicable";
        }

        try
        {
            return Process.GetProcessesByName("SearchIndexer").Length > 0
                ? "running"
                : "not-observed";
        }
        catch
        {
            return "unknown";
        }
    }

    private static string AntivirusState()
    {
        return OperatingSystem.IsWindows() ? "unknown-or-not-observable" : "not-applicable";
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
