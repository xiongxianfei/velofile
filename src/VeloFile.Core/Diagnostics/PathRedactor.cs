using System.Security.Cryptography;
using System.Text;

namespace VeloFile.Core.Diagnostics;

public sealed class PathRedactor
{
    private readonly byte[] _localSalt;

    public PathRedactor(byte[] localSalt)
    {
        if (localSalt.Length < 16)
        {
            throw new ArgumentException("Path fingerprint salt must be at least 16 bytes.", nameof(localSalt));
        }

        _localSalt = localSalt.ToArray();
    }

    public PathRedaction Redact(string path)
    {
        var normalized = Path.GetFullPath(path);
        var extension = Path.GetExtension(normalized);

        return new PathRedaction(
            PathClassification: Classify(normalized),
            PathFingerprint: Fingerprint(normalized),
            ExtensionClass: string.IsNullOrWhiteSpace(extension) ? null : extension.ToLowerInvariant());
    }

    private static string Classify(string normalizedPath)
    {
        var classificationPath = StripExtendedLocalPrefix(normalizedPath);

        if (classificationPath.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)
            || classificationPath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return "network";
        }

        if (classificationPath.Contains(@"\Windows\", StringComparison.OrdinalIgnoreCase)
            || classificationPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase))
        {
            return "protected";
        }

        return Path.IsPathFullyQualified(classificationPath) ? "local" : "unknown";
    }

    private static string StripExtendedLocalPrefix(string normalizedPath)
    {
        return normalizedPath.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase)
            && !normalizedPath.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)
            ? normalizedPath[4..]
            : normalizedPath;
    }

    private string Fingerprint(string normalizedPath)
    {
        using var hmac = new HMACSHA256(_localSalt);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath.ToUpperInvariant()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed record PathRedaction(
    string PathClassification,
    string PathFingerprint,
    string? ExtensionClass);
