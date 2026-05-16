using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

internal static class CorpusToolHarness
{
    public static Result RunInProcess(params string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = global::CorpusCli.Run(args, output, error);

        return new Result(exitCode, output.ToString(), error.ToString());
    }

    internal sealed record Result(int ExitCode, string StandardOutput, string StandardError)
    {
        public string AllOutput => StandardOutput + StandardError;
    }
}

