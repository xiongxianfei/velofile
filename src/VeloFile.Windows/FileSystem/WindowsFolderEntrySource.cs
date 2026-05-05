using System.Runtime.CompilerServices;
using VeloFile.Core;
using VeloFile.Core.Listing;

namespace VeloFile.Windows.FileSystem;

public sealed class WindowsFolderEntrySource : IFolderEntrySource
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
            var kind = EntryKind(entry, attributes);

            return new FileSystemEntrySnapshot(
                FullPath: entry.FullName,
                Name: entry.Name,
                Kind: kind,
                Length: kind is FileSystemEntryKind.File && entry is FileInfo file ? file.Length : null,
                LastWriteTimeUtc: entry.LastWriteTimeUtc,
                Attributes: attributes,
                CreationTimeUtc: entry.CreationTimeUtc,
                LastAccessTimeUtc: entry.LastAccessTimeUtc);
        }
        catch (Exception ex) when (ExpectedFileSystemExceptions.IsExpected(ex))
        {
            return null;
        }
    }

    private static FileSystemEntryKind EntryKind(FileSystemInfo entry, FileAttributes attributes)
    {
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            return FileSystemEntryKind.Directory;
        }

        return entry is FileInfo ? FileSystemEntryKind.File : FileSystemEntryKind.Other;
    }
}
