using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeloFile.Corpus.Tests.TestRuntime;

internal sealed record PreparedCorpusToolContext(
    string AllowedRoot,
    string SetupId,
    string Configuration,
    string TargetFramework)
{
    public static PreparedCorpusToolContext Create(string allowedRoot)
    {
        return new PreparedCorpusToolContext(
            Path.GetFullPath(allowedRoot),
            Guid.NewGuid().ToString("N"),
            "Debug",
            "net8.0-windows10.0.19041.0");
    }
}

internal sealed record PreparedCorpusToolSetup(string Root, string SourceRoot);

internal sealed record PreparedCorpusToolRunResult(
    bool ProcessStarted,
    int? ExitCode,
    string StandardOutput,
    string StandardError,
    string Diagnostic)
{
    public string AllOutput => Diagnostic + StandardOutput + StandardError;
}

internal sealed record PreparedCorpusToolManifest(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("toolKind")] string ToolKind,
    [property: JsonPropertyName("setupId")] string SetupId,
    [property: JsonPropertyName("configuration")] string Configuration,
    [property: JsonPropertyName("targetFramework")] string TargetFramework,
    [property: JsonPropertyName("entrypoint")] string Entrypoint,
    [property: JsonPropertyName("createdUtc")] string CreatedUtc);

internal static class PreparedCorpusToolHarness
{
    public const string ManifestFileName = ".velofile-prepared-tool.json";
    public const string ToolKind = "VeloFile.Corpus";
    public const string Entrypoint = "VeloFile.Corpus.dll";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static PreparedCorpusToolSetup Prepare(PreparedCorpusToolContext context)
    {
        Directory.CreateDirectory(context.AllowedRoot);
        var preparedRoot = Path.Combine(context.AllowedRoot, "prepared-tools", ToolKind);
        var sourceRoot = Path.Combine(context.AllowedRoot, "prepared-source");
        var toolSourceRoot = Path.Combine(sourceRoot, "tools", "VeloFile.Corpus");
        var coreSourceRoot = Path.Combine(sourceRoot, "src", "VeloFile.Core");
        var windowsSourceRoot = Path.Combine(sourceRoot, "src", "VeloFile.Windows");
        var dotnetHome = Path.Combine(context.AllowedRoot, "dotnet-home");
        var nugetPackages = Path.Combine(context.AllowedRoot, "nuget", "packages");
        var nugetHttpCache = Path.Combine(context.AllowedRoot, "nuget", "http-cache");
        var nugetPluginsCache = Path.Combine(context.AllowedRoot, "nuget", "plugins-cache");
        var tempRoot = Path.Combine(context.AllowedRoot, "temp");

        RecreateDirectory(preparedRoot);
        RecreateDirectory(sourceRoot);
        Directory.CreateDirectory(dotnetHome);
        Directory.CreateDirectory(nugetPackages);
        Directory.CreateDirectory(nugetHttpCache);
        Directory.CreateDirectory(nugetPluginsCache);
        Directory.CreateDirectory(tempRoot);

        var repoRoot = TestRepo.FindRoot();
        CopyDirectoryExcludingBuildOutputs(Path.Combine(repoRoot.FullName, "tools", "VeloFile.Corpus"), toolSourceRoot);
        CopyDirectoryExcludingBuildOutputs(Path.Combine(repoRoot.FullName, "src", "VeloFile.Core"), coreSourceRoot);
        CopyDirectoryExcludingBuildOutputs(Path.Combine(repoRoot.FullName, "src", "VeloFile.Windows"), windowsSourceRoot);
        CopyOptionalRootFile(repoRoot.FullName, sourceRoot, "Directory.Build.props");
        CopyOptionalRootFile(repoRoot.FullName, sourceRoot, "Directory.Build.targets");
        CopyOptionalRootFile(repoRoot.FullName, sourceRoot, "NuGet.config");
        CopyOptionalRootFile(repoRoot.FullName, sourceRoot, "global.json");

        var projectPath = Path.Combine(toolSourceRoot, "VeloFile.Corpus.csproj");
        var result = RunProcess(
            "dotnet",
            [
                "publish",
                projectPath,
                "-c",
                context.Configuration,
                "-f",
                context.TargetFramework,
                "-o",
                preparedRoot,
                "-p:RestorePackagesPath=" + nugetPackages,
                "-p:UseSharedCompilation=false",
                "--nologo"
            ],
            sourceRoot,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["DOTNET_CLI_HOME"] = dotnetHome,
                ["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0",
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
                ["DOTNET_NOLOGO"] = "1",
                ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
                ["NUGET_PACKAGES"] = nugetPackages,
                ["NUGET_HTTP_CACHE_PATH"] = nugetHttpCache,
                ["NUGET_PLUGINS_CACHE_PATH"] = nugetPluginsCache,
                ["TEMP"] = tempRoot,
                ["TMP"] = tempRoot
            });

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("Failed to prepare corpus tool." + Environment.NewLine + result.AllOutput);
        }

        WriteManifest(
            preparedRoot,
            new PreparedCorpusToolManifest(
                1,
                ToolKind,
                context.SetupId,
                context.Configuration,
                context.TargetFramework,
                Entrypoint,
                DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)));

        return new PreparedCorpusToolSetup(preparedRoot, sourceRoot);
    }

    public static PreparedCorpusToolRunResult Run(PreparedCorpusToolContext context, string preparedToolRoot, params string[] arguments)
    {
        var validation = Validate(context, preparedToolRoot);
        if (validation is not null)
        {
            return new PreparedCorpusToolRunResult(
                ProcessStarted: false,
                ExitCode: null,
                StandardOutput: string.Empty,
                StandardError: string.Empty,
                Diagnostic: validation);
        }

        var entrypointPath = Path.Combine(Path.GetFullPath(preparedToolRoot), Entrypoint);
        var result = RunProcess("dotnet", [entrypointPath, .. arguments], Path.GetFullPath(preparedToolRoot));

        return new PreparedCorpusToolRunResult(
            ProcessStarted: true,
            ExitCode: result.ExitCode,
            StandardOutput: result.StandardOutput,
            StandardError: result.StandardError,
            Diagnostic: string.Empty);
    }

    public static void WriteManifest(string preparedToolRoot, PreparedCorpusToolManifest manifest)
    {
        Directory.CreateDirectory(preparedToolRoot);
        File.WriteAllText(
            Path.Combine(preparedToolRoot, ManifestFileName),
            JsonSerializer.Serialize(manifest, JsonOptions));
    }

    private static string? Validate(PreparedCorpusToolContext context, string preparedToolRoot)
    {
        var normalizedAllowedRoot = Path.GetFullPath(context.AllowedRoot);
        var normalizedPreparedRoot = Path.GetFullPath(preparedToolRoot);

        if (!IsStrictChildPath(normalizedAllowedRoot, normalizedPreparedRoot))
        {
            return "prepared-tool-outside-root: prepared tool root must stay under the allowed scratch root.";
        }

        if (!Directory.Exists(normalizedPreparedRoot))
        {
            return "prepared-tool-root-missing: prepared tool root does not exist.";
        }

        var manifestPath = Path.Combine(normalizedPreparedRoot, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return "prepared-tool-manifest-missing: current-run prepared tool manifest is required.";
        }

        PreparedCorpusToolManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<PreparedCorpusToolManifest>(File.ReadAllText(manifestPath), JsonOptions);
        }
        catch (JsonException)
        {
            return "prepared-tool-manifest-invalid: manifest JSON is not readable.";
        }

        if (manifest is null)
        {
            return "prepared-tool-manifest-invalid: manifest JSON is empty.";
        }

        if (!StringEquals(manifest.SetupId, context.SetupId))
        {
            return "prepared-tool-setup-mismatch: manifest setupId does not match the current test setup invocation.";
        }

        if (!StringEquals(manifest.ToolKind, ToolKind))
        {
            return "prepared-tool-metadata-invalid: toolKind does not match the expected corpus tool.";
        }

        if (!StringEquals(manifest.Configuration, context.Configuration))
        {
            return "prepared-tool-metadata-invalid: configuration does not match the current test setup.";
        }

        if (!StringEquals(manifest.TargetFramework, context.TargetFramework))
        {
            return "prepared-tool-metadata-invalid: targetFramework does not match the current test setup.";
        }

        if (!StringEquals(manifest.Entrypoint, Entrypoint))
        {
            return "prepared-tool-metadata-invalid: entrypoint does not match the expected corpus tool artifact.";
        }

        if (!File.Exists(Path.Combine(normalizedPreparedRoot, Entrypoint)))
        {
            return "prepared-tool-artifact-missing: expected prepared corpus tool artifact is missing.";
        }

        return null;
    }

    private static bool IsStrictChildPath(string parent, string child)
    {
        var normalizedParent = EnsureTrailingSeparator(parent);
        return child.StartsWith(normalizedParent, OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }

    private static bool StringEquals(string? left, string? right)
    {
        return string.Equals(left, right, StringComparison.Ordinal);
    }

    private static void RecreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void CopyDirectoryExcludingBuildOutputs(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var directory in Directory.EnumerateDirectories(source))
        {
            var name = Path.GetFileName(directory);
            if (string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            CopyDirectoryExcludingBuildOutputs(directory, Path.Combine(destination, name));
        }

        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }
    }

    private static void CopyOptionalRootFile(string repoRoot, string sourceRoot, string fileName)
    {
        var source = Path.Combine(repoRoot, fileName);
        if (File.Exists(source))
        {
            File.Copy(source, Path.Combine(sourceRoot, fileName), overwrite: true);
        }
    }

    private static ProcessResult RunProcess(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string>? environment = null)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (environment is not null)
        {
            foreach (var (name, value) in environment)
            {
                startInfo.Environment[name] = value;
            }
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start process.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new ProcessResult(
            process.ExitCode,
            stdoutTask.GetAwaiter().GetResult(),
            stderrTask.GetAwaiter().GetResult());
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + StandardError;
    }
}
