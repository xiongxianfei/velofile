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

internal sealed record PreparedCorpusToolSetup(string Root);

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
        Directory.CreateDirectory(preparedRoot);

        var repoRoot = TestRepo.FindRoot();
        var projectPath = Path.Combine(repoRoot.FullName, "tools", "VeloFile.Corpus", "VeloFile.Corpus.csproj");
        var result = RunProcess(
            "dotnet",
            [
                "publish",
                projectPath,
                "-c",
                context.Configuration,
                "-f",
                context.TargetFramework,
                "--no-restore",
                "-o",
                preparedRoot
            ],
            repoRoot.FullName);

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

        return new PreparedCorpusToolSetup(preparedRoot);
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

    private static ProcessResult RunProcess(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
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
