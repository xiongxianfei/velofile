using System.Diagnostics;

namespace VeloFile.Windows.Processes;

public sealed record WindowsProcessStartRequest(
    string FileName,
    string? WorkingDirectory,
    bool UseShellExecute,
    IReadOnlyList<string> ArgumentList,
    string? Verb,
    string? CommandText,
    bool ModifySystemAssociations);

public interface IWindowsProcessStarter
{
    void Start(WindowsProcessStartRequest request);
}

public sealed class WindowsProcessStarter : IWindowsProcessStarter
{
    public static WindowsProcessStarter Instance { get; } = new();

    private WindowsProcessStarter()
    {
    }

    public void Start(WindowsProcessStartRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            UseShellExecute = request.UseShellExecute
        };

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            startInfo.WorkingDirectory = request.WorkingDirectory;
        }

        if (!string.IsNullOrWhiteSpace(request.Verb))
        {
            startInfo.Verb = request.Verb;
        }

        foreach (var argument in request.ArgumentList)
        {
            startInfo.ArgumentList.Add(argument);
        }

        Process.Start(startInfo);
    }
}
