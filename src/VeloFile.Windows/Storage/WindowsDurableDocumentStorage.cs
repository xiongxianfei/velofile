using System.Text;
using System.Text.Json;
using VeloFile.Core.Persistence;

namespace VeloFile.Windows.Storage;

public sealed class WindowsDurableDocumentStorage : IDurableDocumentStorage
{
    public string BackupPath(string canonicalPath)
    {
        return canonicalPath + ".bak";
    }

    public bool TryReadText(string path, out string content)
    {
        if (!File.Exists(path))
        {
            content = string.Empty;
            return false;
        }

        content = File.ReadAllText(path, Encoding.UTF8);
        return true;
    }

    public void WriteAtomic(string canonicalPath, string content)
    {
        JsonDocument.Parse(content).Dispose();

        var canonicalFullPath = Path.GetFullPath(canonicalPath);
        var directory = Path.GetDirectoryName(canonicalFullPath) ?? throw new InvalidOperationException("Canonical path must have a directory.");
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $".{Path.GetFileName(canonicalFullPath)}.{Guid.NewGuid():N}.tmp");
        var backupPath = BackupPath(canonicalFullPath);

        try
        {
            WriteTempFile(tempPath, content);

            if (File.Exists(canonicalFullPath))
            {
                File.Replace(tempPath, canonicalFullPath, backupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, canonicalFullPath);
                File.Copy(canonicalFullPath, backupPath, overwrite: true);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static void WriteTempFile(string tempPath, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        using var stream = new FileStream(
            tempPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.WriteThrough);

        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }
}
