using Microsoft.VisualBasic.FileIO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using VeloFile.Core;
using VeloFile.Core.Listing;
using VeloFile.Core.Operations;
using VisualBasicFileSystem = Microsoft.VisualBasic.FileIO.FileSystem;

namespace VeloFile.Windows.Shell;

public enum WindowsShellFileOperationKind
{
    Copy,
    Move,
    CreateShortcut,
    Rename,
    Delete
}

public enum WindowsShellDeleteDisposition
{
    None,
    RecycleBin,
    Permanent
}

public enum WindowsShellConflictChoice
{
    None,
    Skip,
    Replace,
    KeepBoth
}

public sealed record WindowsShellFileOperationIntent(
    WindowsShellFileOperationKind Kind,
    IReadOnlyList<FileOperationTarget> Targets,
    string? TargetName,
    string? TargetDirectory,
    WindowsShellConflictChoice ConflictChoice,
    WindowsShellDeleteDisposition DeleteDisposition,
    bool AllowUndoBypassingDelete);

public enum RecycleBinCapability
{
    Recyclable,
    NotRecyclable,
    Unknown
}

public interface IRecycleBinCapabilityProbe
{
    RecycleBinCapability GetCapability(IReadOnlyList<FileOperationTarget> targets);
}

public sealed record FileOperationCollision(
    FileOperationTarget Target,
    string ExistingName);

public interface IFileOperationCollisionProbe
{
    FileOperationCollision? FindFirstCollision(WindowsShellFileOperationIntent intent);
}

public interface IWindowsShellFileOperationExecutor
{
    void Execute(
        WindowsShellFileOperationIntent intent,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken);
}

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
            FileOperationKind.Copy => MapCopyMove(request, WindowsShellFileOperationKind.Copy),
            FileOperationKind.Move => MapCopyMove(request, WindowsShellFileOperationKind.Move),
            FileOperationKind.CreateShortcut => MapCopyMove(request, WindowsShellFileOperationKind.CreateShortcut),
            FileOperationKind.Rename => MapRename(request),
            FileOperationKind.RecycleBinDelete => new WindowsShellFileOperationIntent(
                WindowsShellFileOperationKind.Delete,
                request.Items,
                TargetName: null,
                TargetDirectory: null,
                WindowsShellConflictChoice.None,
                WindowsShellDeleteDisposition.RecycleBin,
                AllowUndoBypassingDelete: false),
            FileOperationKind.PermanentDelete when request.ConfirmedPermanentDelete => new WindowsShellFileOperationIntent(
                WindowsShellFileOperationKind.Delete,
                request.Items,
                TargetName: null,
                TargetDirectory: null,
                WindowsShellConflictChoice.None,
                WindowsShellDeleteDisposition.Permanent,
                AllowUndoBypassingDelete: true),
            FileOperationKind.PermanentDelete => throw new InvalidOperationException("Permanent delete requires a confirmed permanent-delete request."),
            _ => throw new InvalidOperationException($"Unsupported file operation kind '{request.Kind}'.")
        };
    }

    private static WindowsShellFileOperationIntent MapCopyMove(
        FileOperationRequest request,
        WindowsShellFileOperationKind kind)
    {
        if (string.IsNullOrWhiteSpace(request.TargetDirectory))
        {
            throw new InvalidOperationException("Copy and move require a target directory.");
        }

        return new WindowsShellFileOperationIntent(
            kind,
            request.Items,
            TargetName: null,
            request.TargetDirectory,
            MapConflictChoice(request.ConflictChoice),
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);
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
            TargetDirectory: null,
            WindowsShellConflictChoice.None,
            WindowsShellDeleteDisposition.None,
            AllowUndoBypassingDelete: false);
    }

    private static WindowsShellConflictChoice MapConflictChoice(FileOperationConflictChoice? conflictChoice)
    {
        return conflictChoice switch
        {
            FileOperationConflictChoice.Skip => WindowsShellConflictChoice.Skip,
            FileOperationConflictChoice.Replace => WindowsShellConflictChoice.Replace,
            FileOperationConflictChoice.KeepBoth => WindowsShellConflictChoice.KeepBoth,
            _ => WindowsShellConflictChoice.None
        };
    }
}

public sealed class WindowsShellFileOperationAdapter : ICancellableFileOperationAdapter
{
    private readonly IRecycleBinCapabilityProbe _recycleBinCapabilityProbe;
    private readonly IFileOperationCollisionProbe _collisionProbe;
    private readonly IWindowsShellFileOperationExecutor _executor;

    public WindowsShellFileOperationAdapter(
        IRecycleBinCapabilityProbe? recycleBinCapabilityProbe = null,
        IWindowsShellFileOperationExecutor? executor = null,
        IFileOperationCollisionProbe? collisionProbe = null)
    {
        _recycleBinCapabilityProbe = recycleBinCapabilityProbe ?? DefaultRecycleBinCapabilityProbe.Instance;
        _collisionProbe = collisionProbe ?? DefaultFileOperationCollisionProbe.Instance;
        _executor = executor ?? VisualBasicShellFileOperationExecutor.Instance;
    }

    public bool CanCancel(FileOperationRequest request)
    {
        return true;
    }

    public Task<FileOperationAdapterResult> ExecuteAsync(
        FileOperationRequest request,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        return Task.Run(() => ExecuteCore(request, progress, cancellationToken), CancellationToken.None);
    }

    private FileOperationAdapterResult ExecuteCore(
        FileOperationRequest request,
        IProgress<FileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var intent = WindowsShellFileOperationRequestMapper.Map(request);
            if (IsRecycleBinDelete(intent)
                && _recycleBinCapabilityProbe.GetCapability(intent.Targets) is RecycleBinCapability.NotRecyclable)
            {
                return FileOperationAdapterResult.RecycleBinUnavailable("recycle-bin-unavailable");
            }

            var collision = _collisionProbe.FindFirstCollision(intent);
            if (collision is not null && intent.ConflictChoice is WindowsShellConflictChoice.None)
            {
                return FileOperationAdapterResult.ConflictRequired(
                    "name-conflict",
                    new FileOperationConflict(
                        request.Kind,
                        request.Items,
                        request.TargetDirectory!,
                        collision.ExistingName));
            }

            progress?.Report(new FileOperationProgress(request.Kind, 0, request.Items.Count, "Starting"));
            _executor.Execute(intent, progress, cancellationToken);
            return FileOperationAdapterResult.Completed(undoSupported: request.Kind is FileOperationKind.Move or FileOperationKind.Rename or FileOperationKind.RecycleBinDelete);
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

    private static bool IsRecycleBinDelete(WindowsShellFileOperationIntent intent)
    {
        return intent.Kind is WindowsShellFileOperationKind.Delete
            && intent.DeleteDisposition is WindowsShellDeleteDisposition.RecycleBin;
    }
}

public sealed class DefaultRecycleBinCapabilityProbe : IRecycleBinCapabilityProbe
{
    public static DefaultRecycleBinCapabilityProbe Instance { get; } = new();

    public RecycleBinCapability GetCapability(IReadOnlyList<FileOperationTarget> targets)
    {
        var sawUnknown = false;

        foreach (var target in targets)
        {
            var capability = GetTargetCapability(target.Path);
            if (capability is RecycleBinCapability.NotRecyclable)
            {
                return RecycleBinCapability.NotRecyclable;
            }

            sawUnknown |= capability is RecycleBinCapability.Unknown;
        }

        return sawUnknown ? RecycleBinCapability.Unknown : RecycleBinCapability.Recyclable;
    }

    private static RecycleBinCapability GetTargetCapability(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return RecycleBinCapability.Unknown;
        }

        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
        {
            return RecycleBinCapability.NotRecyclable;
        }

        if (path.StartsWith(@"\\", StringComparison.Ordinal)
            && !path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
        {
            return RecycleBinCapability.NotRecyclable;
        }

        try
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrWhiteSpace(root))
            {
                return RecycleBinCapability.Unknown;
            }

            var drive = new DriveInfo(root);
            return drive.DriveType is DriveType.Network
                ? RecycleBinCapability.NotRecyclable
                : RecycleBinCapability.Recyclable;
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return RecycleBinCapability.Unknown;
        }
        catch (ArgumentException)
        {
            return RecycleBinCapability.Unknown;
        }
        catch (NotSupportedException)
        {
            return RecycleBinCapability.Unknown;
        }
    }
}

public sealed class DefaultFileOperationCollisionProbe : IFileOperationCollisionProbe
{
    public static DefaultFileOperationCollisionProbe Instance { get; } = new();

    public FileOperationCollision? FindFirstCollision(WindowsShellFileOperationIntent intent)
    {
        if (intent.Kind is not (WindowsShellFileOperationKind.Copy or WindowsShellFileOperationKind.Move)
            || string.IsNullOrWhiteSpace(intent.TargetDirectory))
        {
            return null;
        }

        foreach (var target in intent.Targets)
        {
            var destination = Path.Combine(intent.TargetDirectory, target.Name);
            var exists = target.Kind is FileSystemEntryKind.Directory
                ? Directory.Exists(destination)
                : File.Exists(destination);

            if (exists)
            {
                return new FileOperationCollision(target, target.Name);
            }
        }

        return null;
    }
}

public sealed class VisualBasicShellFileOperationExecutor : IWindowsShellFileOperationExecutor
{
    public static VisualBasicShellFileOperationExecutor Instance { get; } = new();

    public void Execute(
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
                case WindowsShellFileOperationKind.Copy:
                    Copy(target, intent.TargetDirectory!, intent.ConflictChoice);
                    break;
                case WindowsShellFileOperationKind.Move:
                    Move(target, intent.TargetDirectory!, intent.ConflictChoice);
                    break;
                case WindowsShellFileOperationKind.CreateShortcut:
                    CreateShortcut(target, intent.TargetDirectory!);
                    break;
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

    private static void Copy(
        FileOperationTarget target,
        string targetDirectory,
        WindowsShellConflictChoice conflictChoice)
    {
        if (conflictChoice is WindowsShellConflictChoice.Skip
            && Exists(Path.Combine(targetDirectory, target.Name), target.Kind))
        {
            return;
        }

        var destination = DestinationPath(target, targetDirectory, conflictChoice);
        if (target.Kind is FileSystemEntryKind.Directory)
        {
            VisualBasicFileSystem.CopyDirectory(target.Path, destination, overwrite: conflictChoice is WindowsShellConflictChoice.Replace);
        }
        else
        {
            VisualBasicFileSystem.CopyFile(target.Path, destination, overwrite: conflictChoice is WindowsShellConflictChoice.Replace);
        }
    }

    private static void Move(
        FileOperationTarget target,
        string targetDirectory,
        WindowsShellConflictChoice conflictChoice)
    {
        if (conflictChoice is WindowsShellConflictChoice.Skip
            && Exists(Path.Combine(targetDirectory, target.Name), target.Kind))
        {
            return;
        }

        var destination = DestinationPath(target, targetDirectory, conflictChoice);
        if (target.Kind is FileSystemEntryKind.Directory)
        {
            VisualBasicFileSystem.MoveDirectory(target.Path, destination, overwrite: conflictChoice is WindowsShellConflictChoice.Replace);
        }
        else
        {
            VisualBasicFileSystem.MoveFile(target.Path, destination, overwrite: conflictChoice is WindowsShellConflictChoice.Replace);
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

    private static void CreateShortcut(FileOperationTarget target, string targetDirectory)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows shortcut creation requires Windows Shell COM.");
        }

        var shortcutPath = ShortcutDestinationPath(target, targetDirectory);
        WindowsShortcutFile.Create(shortcutPath, target.Path);
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

    private static string DestinationPath(
        FileOperationTarget target,
        string targetDirectory,
        WindowsShellConflictChoice conflictChoice)
    {
        var destination = Path.Combine(targetDirectory, target.Name);
        return conflictChoice is WindowsShellConflictChoice.KeepBoth
            ? KeepBothDestination(destination, target.Kind)
            : destination;
    }

    private static string ShortcutDestinationPath(FileOperationTarget target, string targetDirectory)
    {
        var baseName = target.Kind is FileSystemEntryKind.Directory
            ? target.Name
            : Path.GetFileNameWithoutExtension(target.Name);
        var initial = Path.Combine(targetDirectory, baseName + ".lnk");
        if (!File.Exists(initial) && !Directory.Exists(initial))
        {
            return initial;
        }

        for (var index = 2; index < 10_000; index++)
        {
            var candidate = Path.Combine(targetDirectory, $"{baseName} ({index}).lnk");
            if (!File.Exists(candidate) && !Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException("Could not find an available shortcut destination name.");
    }

    private static string KeepBothDestination(string destination, FileSystemEntryKind kind)
    {
        if (!Exists(destination, kind))
        {
            return destination;
        }

        var directory = Path.GetDirectoryName(destination) ?? "";
        var nameWithoutExtension = kind is FileSystemEntryKind.Directory
            ? Path.GetFileName(destination)
            : Path.GetFileNameWithoutExtension(destination);
        var extension = kind is FileSystemEntryKind.Directory ? "" : Path.GetExtension(destination);

        for (var index = 2; index < 10_000; index++)
        {
            var candidate = Path.Combine(directory, $"{nameWithoutExtension} ({index}){extension}");
            if (!Exists(candidate, kind))
            {
                return candidate;
            }
        }

        throw new IOException("Could not find an available keep-both destination name.");
    }

    private static bool Exists(string path, FileSystemEntryKind kind)
    {
        return kind is FileSystemEntryKind.Directory ? Directory.Exists(path) : File.Exists(path);
    }

    private static FileOperationKind ToCoreKind(WindowsShellFileOperationIntent intent)
    {
        return intent.Kind switch
        {
            WindowsShellFileOperationKind.Copy => FileOperationKind.Copy,
            WindowsShellFileOperationKind.Move => FileOperationKind.Move,
            WindowsShellFileOperationKind.CreateShortcut => FileOperationKind.CreateShortcut,
            WindowsShellFileOperationKind.Rename => FileOperationKind.Rename,
            WindowsShellFileOperationKind.Delete when intent.DeleteDisposition is WindowsShellDeleteDisposition.Permanent => FileOperationKind.PermanentDelete,
            WindowsShellFileOperationKind.Delete => FileOperationKind.RecycleBinDelete,
            _ => FileOperationKind.RecycleBinDelete
        };
    }
}

[SupportedOSPlatform("windows")]
public static class WindowsShortcutFile
{
    public static void Create(string shortcutPath, string targetPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);
        var shellLink = CreateShellLink();
        shellLink.SetPath(targetPath);
        ((IPersistFile)shellLink).Save(shortcutPath, remember: true);
    }

    public static string ReadTarget(string shortcutPath)
    {
        var shellLink = CreateShellLink();
        ((IPersistFile)shellLink).Load(shortcutPath, 0);
        var pathBuilder = new System.Text.StringBuilder(32_768);
        shellLink.GetPath(pathBuilder, pathBuilder.Capacity, IntPtr.Zero, 0);
        return pathBuilder.ToString();
    }

    private static IShellLinkW CreateShellLink()
    {
        var shellLinkType = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"))
            ?? throw new PlatformNotSupportedException("Shell link COM class is unavailable.");
        return (IShellLinkW)Activator.CreateInstance(shellLinkType)!;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath(
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile,
            int cchMaxPath,
            IntPtr pfd,
            uint fFlags);

        void GetIDList(out IntPtr ppidl);

        void SetIDList(IntPtr pidl);

        void GetDescription(
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName,
            int cchMaxName);

        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        void GetWorkingDirectory(
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir,
            int cchMaxPath);

        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        void GetArguments(
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs,
            int cchMaxPath);

        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        void GetHotkey(out short pwHotkey);

        void SetHotkey(short wHotkey);

        void GetShowCmd(out int piShowCmd);

        void SetShowCmd(int iShowCmd);

        void GetIconLocation(
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath,
            int cchIconPath,
            out int piIcon);

        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

        void Resolve(IntPtr hwnd, uint fFlags);

        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);

        void IsDirty();

        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool remember);

        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}
