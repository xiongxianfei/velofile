namespace VeloFile.App.Testing;

public sealed record UiFixtureLaunchContext(
    bool IsDebugOrTestBuild,
    string? EnableEnvironmentValue);

public sealed record UiFixtureLaunchRequest(
    bool IsRequested,
    string? FixtureName,
    string? Theme,
    string? Density,
    string? Viewport,
    string? ParseErrorReasonCode);

public enum UiFixtureLaunchStatus
{
    NotRequested,
    Accepted,
    Rejected
}

public sealed record UiFixtureLaunchResult(
    UiFixtureLaunchStatus Status,
    string? FixtureName,
    string? Theme,
    string? Density,
    string? Viewport,
    string? ReasonCode)
{
    public bool ShouldLaunchNormalApp => Status is UiFixtureLaunchStatus.NotRequested;

    public bool ShouldLaunchFixture => Status is UiFixtureLaunchStatus.Accepted;

    public int ExitCode => Status is UiFixtureLaunchStatus.Rejected ? 2 : 0;
}

public static class UiFixtureLaunchParser
{
    public static UiFixtureLaunchRequest Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            return new UiFixtureLaunchRequest(false, null, null, null, null, null);
        }

        string? fixtureName = null;
        string? theme = null;
        string? density = null;
        string? viewport = null;
        var requested = false;

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "--test-ui-fixture":
                    requested = true;
                    if (!TryReadOptionValue(args, ref index, out fixtureName))
                    {
                        return RequestedParseError("missing-fixture-name");
                    }

                    break;
                case "--theme":
                    if (!TryReadOptionValue(args, ref index, out theme))
                    {
                        return RequestedParseError("missing-theme");
                    }

                    break;
                case "--density":
                    if (!TryReadOptionValue(args, ref index, out density))
                    {
                        return RequestedParseError("missing-density");
                    }

                    break;
                case "--viewport":
                    if (!TryReadOptionValue(args, ref index, out viewport))
                    {
                        return RequestedParseError("missing-viewport");
                    }

                    break;
                default:
                    return new UiFixtureLaunchRequest(
                        requested || args.Any(value => string.Equals(value, "--test-ui-fixture", StringComparison.Ordinal)),
                        fixtureName,
                        theme,
                        density,
                        viewport,
                        arg.StartsWith("--", StringComparison.Ordinal)
                            ? "unsupported-fixture-option"
                            : "unsupported-fixture-argument");
            }
        }

        if (!requested)
        {
            return new UiFixtureLaunchRequest(false, null, null, null, null, null);
        }

        return new UiFixtureLaunchRequest(
            true,
            fixtureName,
            theme ?? "dark",
            density ?? "comfortable",
            viewport ?? "1440x900",
            ParseErrorReasonCode: null);
    }

    public static UiFixtureLaunchRequest ParseCommandLine(string? arguments)
    {
        return Parse(SplitCommandLine(arguments));
    }

    private static UiFixtureLaunchRequest RequestedParseError(string reasonCode)
    {
        return new UiFixtureLaunchRequest(true, null, null, null, null, reasonCode);
    }

    private static bool TryReadOptionValue(IReadOnlyList<string> args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            return false;
        }

        index++;
        value = args[index];
        return true;
    }

    private static IReadOnlyList<string> SplitCommandLine(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return [];
        }

        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in arguments)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                AddCurrent(values, current);
                continue;
            }

            current.Append(ch);
        }

        AddCurrent(values, current);
        return values;
    }

    private static void AddCurrent(List<string> values, System.Text.StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        values.Add(current.ToString());
        current.Clear();
    }
}

public static class UiFixtureLaunchGate
{
    public const string EnableEnvironmentVariable = "VELOFILE_ENABLE_TEST_UI_FIXTURES";

    public static UiFixtureLaunchResult Evaluate(
        UiFixtureLaunchRequest request,
        UiFixtureLaunchContext context)
    {
        if (!request.IsRequested)
        {
            return new UiFixtureLaunchResult(
                UiFixtureLaunchStatus.NotRequested,
                null,
                null,
                null,
                null,
                null);
        }

        if (request.ParseErrorReasonCode is not null)
        {
            return Reject(request, request.ParseErrorReasonCode);
        }

        if (!context.IsDebugOrTestBuild)
        {
            return Reject(request, "fixture-not-available-in-production");
        }

        if (!string.Equals(context.EnableEnvironmentValue, "1", StringComparison.Ordinal))
        {
            return Reject(request, "fixture-env-guard-missing");
        }

        if (request.FixtureName is null || !UiFixtureRegistry.IsAllowlisted(request.FixtureName))
        {
            return Reject(request, "fixture-not-allowlisted");
        }

        return new UiFixtureLaunchResult(
            UiFixtureLaunchStatus.Accepted,
            request.FixtureName,
            request.Theme,
            request.Density,
            request.Viewport,
            null);
    }

    public static UiFixtureLaunchResult FromCurrentProcess(string? arguments)
    {
        return Evaluate(
            UiFixtureLaunchParser.ParseCommandLine(arguments),
            new UiFixtureLaunchContext(IsDebugOrTestBuild: IsDebugOrTestBuild(), Environment.GetEnvironmentVariable(EnableEnvironmentVariable)));
    }

    private static UiFixtureLaunchResult Reject(UiFixtureLaunchRequest request, string reasonCode)
    {
        return new UiFixtureLaunchResult(
            UiFixtureLaunchStatus.Rejected,
            request.FixtureName,
            request.Theme,
            request.Density,
            request.Viewport,
            reasonCode);
    }

    private static bool IsDebugOrTestBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
