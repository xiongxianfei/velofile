using VeloFile.Corpus.Tests;

namespace VeloFile.Corpus.Tests.TestRuntime;

internal sealed class ScratchWorkspace : IDisposable
{
    private ScratchWorkspace(string root)
    {
        Root = root;
    }

    public string Root { get; }

    public static ScratchWorkspace Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "velofile-corpus-tests", "velofile-corpus-" + Guid.NewGuid().ToString("N"));
        return new ScratchWorkspace(root);
    }

    public void Dispose()
    {
        if (!Directory.Exists(Root))
        {
            return;
        }

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                Directory.Delete(Root, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                Thread.Sleep(200);
            }
            catch (UnauthorizedAccessException) when (attempt < 4)
            {
                Thread.Sleep(200);
            }
        }

        Directory.Delete(Root, recursive: true);
    }
}
