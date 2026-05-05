using Microsoft.VisualBasic.FileIO;
using VeloFile.Core;
using VeloFile.Core.Listing;
using VeloFile.Core.Operations;
using VisualBasicFileSystem = Microsoft.VisualBasic.FileIO.FileSystem;

namespace VeloFile.Windows.Shell;

public enum WindowsShellFileOperationKind
{
    Rename,
    Delete
}

public enum WindowsShellDeleteDisposition
{
    None,
    RecycleBin,
    Permanent
}

public sealed record WindowsShellFileOperationIntent(
    WindowsShellFileOperationKind Kind,
    IReadOnlyList<FileOperationTarget> Targets,
    string? TargetName,
    WindowsShellDeleteDisposition DeleteDisposition,
    bool AllowUndoBypassingDelete);

public static class WindowsShellFileOperationRequestMapper
{
    public static WindowsShellFileOperationIntent Map(FileOperationRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("A shell file operation requires at least one target.");
        }

        return request.Kind switch
        {
            FileOperationKind.Rename => MapRename(request),
            FileOperationKind.RecycleBinDelete => new WindowsShellFileOperationIntent(
                WindowsShellFileOperationKind.Delete,
                request.Items,
                TargetName: null,
                WindowsShellDeleteDisposition.RecycleBin,
                AllowUndoBypassingDelete: false),
            FileOperationKind.PermanentDelete when request.ConfirmedPermanentDelete => new WindowsShellFileOperationIntent(
                WindowsShellFileOperationKind.Delete,
                request.Items,
                TargetName: null,
                WindowsShellDeleteDisposition.Permanent,
                AllowUndoBypassingDelete: true),
            FileOperationKind.PermanentDelete => throw new InvalidOperationException("Permanent delete requires a confirmed permanent-delete request."),
            _ => throw new InvalidOperationException($"Unsupported file operation kind '{request.Kind}'.")
        };
    }

    private static WindowsShellFileOperationIntent MapRename(FileOperationRequest request)
    {
        if (request.Items.Count != 1)
        {
            throw new InvalidOperationException("Rename requires exactly one target.");
        }

        if (string.IsNullOrWhiteSpace(request.TargetName))
        {
            throw new InvalidOperationException("Rename requires a target name.");
        }

        return new WindowsShellFileOperationIntent(
            WindowsShellFileOperationKind.Rename,
            request.Items,
            request.TargetName,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);
    }
}

public sealed class WindowsShellFileOperationAdapter : IFileOperationAdapter
{
    public Task<FileOperationAdapterResult> ExecuteAsync(
        FileOperationRequest request,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        return Task.Run(() => ExecuteCore(request, progress, cancellationToken), CancellationToken.None);
    }

    private static FileOperationAdapterResult ExecuteCore(
        FileOperationRequest request,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var intent = WindowsShellFileOperationRequestMapper.Map(request);
            progress?.Report(new FileOperationProgress(request.Kind, 0, request.Items.Count, "Starting"));
            ExecuteIntent(intent, progress, cancellationToken);
            return FileOperationAdapterResult.Completed(undoSupported: request.Kind is not FileOperationKind.PermanentDelete);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FileOperationAdapterResult.Cancelled();
        }
        catch (Exception ex)
        {
            return FileOperationAdapterResult.Failed(ExpectedFileSystemExceptions.ReasonCode(ex));
        }
    }

    private static void ExecuteIntent(
        WindowsShellFileOperationIntent intent,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < intent.Targets.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var target = intent.Targets[index];

            switch (intent.Kind)
            {
                case WindowsShellFileOperationKind.Rename:
                    Rename(target, intent.TargetName!);
                    break;
                case WindowsShellFileOperationKind.Delete:
                    Delete(target, intent.DeleteDisposition);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported shell operation kind '{intent.Kind}'.");
            }

            progress?.Report(new FileOperationProgress(ToCoreKind(intent), index + 1, intent.Targets.Count, "Completed"));
        }
    }

    private static void Rename(FileOperationTarget target, string targetName)
    {
        if (target.Kind is FileSystemEntryKind.Directory)
        {
            VisualBasicFileSystem.RenameDirectory(target.Path, targetName);
        }
        else
        {
            VisualBasicFileSystem.RenameFile(target.Path, targetName);
        }
    }

    private static void Delete(FileOperationTarget target, WindowsShellDeleteDisposition disposition)
    {
        var recycleOption = disposition is WindowsShellDeleteDisposition.RecycleBin
            ? RecycleOption.SendToRecycleBin
            : RecycleOption.DeletePermanently;

        if (target.Kind is FileSystemEntryKind.Directory)
        {
            VisualBasicFileSystem.DeleteDirectory(target.Path, UIOption.OnlyErrorDialogs, recycleOption);
        }
        else
        {
            VisualBasicFileSystem.DeleteFile(target.Path, UIOption.OnlyErrorDialogs, recycleOption);
        }
    }

    private static FileOperationKind ToCoreKind(WindowsShellFileOperationIntent intent)
    {
        return intent.Kind switch
        {
            WindowsShellFileOperationKind.Rename => FileOperationKind.Rename,
            WindowsShellFileOperationKind.Delete when intent.DeleteDisposition is WindowsShellDeleteDisposition.Permanent => FileOperationKind.PermanentDelete,
            WindowsShellFileOperationKind.Delete => FileOperationKind.RecycleBinDelete,
            _ => FileOperationKind.RecycleBinDelete
        };
    }
}
