using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace VeloFile.Corpus.Tests.TestRuntime;

internal sealed record CiWorkflowDocument(
    IReadOnlySet<string> Events,
    IReadOnlySet<string> PushBranches,
    IReadOnlySet<string> PushTags,
    IReadOnlyList<string> ScheduleCrons,
    IReadOnlyDictionary<string, string> Permissions,
    string? DefaultRunShell,
    IReadOnlyDictionary<string, CiWorkflowJob> Jobs)
{
    public CiWorkflowJob RequireJob(string id)
    {
        Assert.IsTrue(Jobs.TryGetValue(id, out var job), $"workflow-lane-contract: expected job '{id}' to exist.");
        return job;
    }
}

internal sealed record CiWorkflowJob(
    string Id,
    string? Name,
    IReadOnlyList<string> RunsOn,
    string? DefaultRunShell,
    IReadOnlyList<CiWorkflowStep> Steps)
{
    public IReadOnlyList<string> RunCommands => Steps
        .Where(step => !string.IsNullOrWhiteSpace(step.Run))
        .Select(step => step.Run!)
        .ToArray();
}

internal sealed record CiWorkflowStep(
    string? Name,
    string? Id,
    string? Uses,
    string? Run,
    string? Shell,
    bool ContinueOnError);

internal static class CiWorkflowModel
{
    public static CiWorkflowDocument LoadFile(string path)
    {
        return Parse(File.ReadAllText(path));
    }

    public static CiWorkflowDocument Parse(string content)
    {
        var yaml = new YamlStream();
        yaml.Load(new StringReader(content));
        var root = RequireMapping(yaml.Documents[0].RootNode, "workflow root");

        var onNode = GetValue(root, "on");
        var permissionsNode = GetValue(root, "permissions");
        var defaultsNode = GetValue(root, "defaults");
        var jobsNode = RequireMapping(GetValue(root, "jobs"), "jobs");

        return new CiWorkflowDocument(
            EventsFrom(onNode),
            PushBranchesFrom(onNode),
            PushTagsFrom(onNode),
            ScheduleCronsFrom(onNode),
            StringMapFrom(permissionsNode),
            DefaultsShellFrom(defaultsNode),
            JobsFrom(jobsNode));
    }

    private static IReadOnlyDictionary<string, CiWorkflowJob> JobsFrom(YamlMappingNode jobsNode)
    {
        var jobs = new Dictionary<string, CiWorkflowJob>(StringComparer.Ordinal);
        foreach (var child in jobsNode.Children)
        {
            var id = Scalar(child.Key);
            var jobNode = RequireMapping(child.Value, $"job '{id}'");
            var steps = Sequence(GetValue(jobNode, "steps"))
                .Select(step => StepFrom(RequireMapping(step, $"step in '{id}'")))
                .ToArray();

            jobs[id] = new CiWorkflowJob(
                id,
                ScalarOrNull(GetValue(jobNode, "name")),
                StringListFrom(GetValue(jobNode, "runs-on")),
                DefaultsShellFrom(GetValue(jobNode, "defaults")),
                steps);
        }

        return jobs;
    }

    private static CiWorkflowStep StepFrom(YamlMappingNode stepNode)
    {
        return new CiWorkflowStep(
            ScalarOrNull(GetValue(stepNode, "name")),
            ScalarOrNull(GetValue(stepNode, "id")),
            ScalarOrNull(GetValue(stepNode, "uses")),
            ScalarOrNull(GetValue(stepNode, "run")),
            ScalarOrNull(GetValue(stepNode, "shell")),
            BoolFrom(GetValue(stepNode, "continue-on-error")));
    }

    private static IReadOnlySet<string> EventsFrom(YamlNode? onNode)
    {
        if (onNode is null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        if (onNode is YamlMappingNode mapping)
        {
            return mapping.Children.Keys.Select(Scalar).ToHashSet(StringComparer.Ordinal);
        }

        return StringListFrom(onNode).ToHashSet(StringComparer.Ordinal);
    }

    private static IReadOnlySet<string> PushBranchesFrom(YamlNode? onNode)
    {
        if (onNode is not YamlMappingNode mapping)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var push = GetValue(mapping, "push");
        if (push is not YamlMappingNode pushMapping)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return StringListFrom(GetValue(pushMapping, "branches")).ToHashSet(StringComparer.Ordinal);
    }

    private static IReadOnlySet<string> PushTagsFrom(YamlNode? onNode)
    {
        if (onNode is not YamlMappingNode mapping)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var push = GetValue(mapping, "push");
        if (push is not YamlMappingNode pushMapping)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return StringListFrom(GetValue(pushMapping, "tags")).ToHashSet(StringComparer.Ordinal);
    }

    private static IReadOnlyList<string> ScheduleCronsFrom(YamlNode? onNode)
    {
        if (onNode is not YamlMappingNode mapping)
        {
            return Array.Empty<string>();
        }

        var schedule = GetValue(mapping, "schedule");
        return Sequence(schedule)
            .Select(node => node is YamlMappingNode scheduleMapping
                ? ScalarOrNull(GetValue(scheduleMapping, "cron"))
                : null)
            .Where(cron => !string.IsNullOrWhiteSpace(cron))
            .ToArray()!;
    }

    private static IReadOnlyDictionary<string, string> StringMapFrom(YamlNode? node)
    {
        if (node is not YamlMappingNode mapping)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return mapping.Children.ToDictionary(child => Scalar(child.Key), child => Scalar(child.Value), StringComparer.Ordinal);
    }

    private static string? DefaultsShellFrom(YamlNode? node)
    {
        if (node is not YamlMappingNode defaults)
        {
            return null;
        }

        if (GetValue(defaults, "run") is not YamlMappingNode run)
        {
            return null;
        }

        return ScalarOrNull(GetValue(run, "shell"));
    }

    private static IReadOnlyList<YamlNode> Sequence(YamlNode? node)
    {
        return node is YamlSequenceNode sequence ? sequence.Children.ToArray() : Array.Empty<YamlNode>();
    }

    private static IReadOnlyList<string> StringListFrom(YamlNode? node)
    {
        return node switch
        {
            YamlScalarNode scalar => string.IsNullOrWhiteSpace(scalar.Value) ? Array.Empty<string>() : [scalar.Value],
            YamlSequenceNode sequence => sequence.Children.Select(Scalar).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
            _ => Array.Empty<string>()
        };
    }

    private static YamlMappingNode RequireMapping(YamlNode? node, string context)
    {
        Assert.IsInstanceOfType<YamlMappingNode>(node, $"workflow-parser-contract: expected {context} to be a YAML mapping.");
        return (YamlMappingNode)node!;
    }

    private static YamlNode? GetValue(YamlMappingNode mapping, string key)
    {
        foreach (var child in mapping.Children)
        {
            if (StringComparer.Ordinal.Equals(Scalar(child.Key), key))
            {
                return child.Value;
            }
        }

        return null;
    }

    private static string Scalar(YamlNode node)
    {
        return node switch
        {
            YamlScalarNode scalar => scalar.Value ?? string.Empty,
            _ => node.ToString()
        };
    }

    private static string? ScalarOrNull(YamlNode? node)
    {
        if (node is null)
        {
            return null;
        }

        var value = Scalar(node);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool BoolFrom(YamlNode? node)
    {
        return node is YamlScalarNode scalar && bool.TryParse(scalar.Value, out var result) && result;
    }
}

internal static class CiWorkflowContractValidator
{
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    public static IReadOnlyList<string> ValidateFastLane(CiWorkflowDocument workflow, string jobId)
    {
        var diagnostics = ValidateHostedLane(workflow, jobId).ToList();

        if (!workflow.Jobs.TryGetValue(jobId, out var job))
        {
            return diagnostics;
        }

        ValidateFastLaneCommandSelection(job, diagnostics);

        return diagnostics;
    }

    public static IReadOnlyList<string> ValidateHostedLane(CiWorkflowDocument workflow, string jobId)
    {
        var diagnostics = new List<string>();

        if (!workflow.Jobs.TryGetValue(jobId, out var job))
        {
            diagnostics.Add($"workflow-lane-contract: missing required lane '{jobId}'.");
            return diagnostics;
        }

        ValidateWindowsRunner(job, diagnostics);
        ValidatePwshShell(workflow, job, diagnostics);
        ValidateSdkOrdering(job, diagnostics);

        return diagnostics;
    }

    private static void ValidateWindowsRunner(CiWorkflowJob job, List<string> diagnostics)
    {
        if (!job.RunsOn.Contains("windows-latest", StringComparer.Ordinal))
        {
            diagnostics.Add(
                $"workflow-runner-contract: {job.Id} must run on windows-latest or an approved Windows runner; found {string.Join(", ", job.RunsOn)}.");
        }
    }

    private static void ValidatePwshShell(CiWorkflowDocument workflow, CiWorkflowJob job, List<string> diagnostics)
    {
        foreach (var step in job.Steps.Where(step => !string.IsNullOrWhiteSpace(step.Run)))
        {
            var effectiveShell = step.Shell ?? job.DefaultRunShell ?? workflow.DefaultRunShell;
            if (!StringComparer.Ordinal.Equals(effectiveShell, "pwsh"))
            {
                diagnostics.Add(
                    $"workflow-shell-contract: {job.Id} step '{StepName(step)}' must use pwsh; found {effectiveShell ?? "none"}.");
            }
        }
    }

    private static void ValidateSdkOrdering(CiWorkflowJob job, List<string> diagnostics)
    {
        var setupIndex = job.Steps.ToList().FindIndex(step => step.Uses?.StartsWith("actions/setup-dotnet@", StringComparison.Ordinal) == true);
        if (setupIndex < 0)
        {
            diagnostics.Add($"workflow-sdk-contract: {job.Id} must set up the repository-approved .NET SDK before validation commands.");
            return;
        }

        for (var index = 0; index < setupIndex; index++)
        {
            var command = Normalize(job.Steps[index].Run);
            if (IsValidationCommand(command))
            {
                diagnostics.Add(
                    $"workflow-sdk-contract: {job.Id} runs validation before .NET SDK setup in step '{StepName(job.Steps[index])}'.");
            }
        }
    }

    private static void ValidateFastLaneCommandSelection(CiWorkflowJob job, List<string> diagnostics)
    {
        foreach (var command in job.RunCommands.Select(Normalize))
        {
            if (command.Contains("scripts/ci.ps1", StringComparison.OrdinalIgnoreCase)
                || command.Contains("scripts\\ci.ps1", StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add($"workflow-command-contract: {job.Id} must not call scripts/ci.ps1.");
            }

            if (command.Contains("dotnet test VeloFile.sln", StringComparison.Ordinal)
                && command.Contains("TestCategory=Fast|TestCategory=Contract", StringComparison.Ordinal))
            {
                diagnostics.Add($"workflow-filter-contract: {job.Id} must not apply Corpus category filters to the solution.");
            }

            if (command.Contains("TestCategory=ReleaseEvidence", StringComparison.Ordinal))
            {
                diagnostics.Add($"workflow-release-evidence-contract: {job.Id} must not run ReleaseEvidence by default.");
            }
        }
    }

    public static string Normalize(string? command)
    {
        return string.IsNullOrWhiteSpace(command) ? string.Empty : Whitespace.Replace(command, " ").Trim();
    }

    private static bool IsValidationCommand(string command)
    {
        return command.Contains("dotnet restore", StringComparison.Ordinal)
            || command.Contains("dotnet build", StringComparison.Ordinal)
            || command.Contains("dotnet test", StringComparison.Ordinal)
            || command.Contains("dotnet run --project tools", StringComparison.Ordinal)
            || command.Contains("scripts/ci.ps1", StringComparison.OrdinalIgnoreCase)
            || command.Contains("scripts\\ci.ps1", StringComparison.OrdinalIgnoreCase);
    }

    private static string StepName(CiWorkflowStep step)
    {
        return step.Name ?? step.Id ?? step.Run ?? step.Uses ?? "<unnamed>";
    }
}
