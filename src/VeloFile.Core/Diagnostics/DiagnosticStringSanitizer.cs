using System.Security.Cryptography;
using System.Text;

namespace VeloFile.Core.Diagnostics;

public static class DiagnosticStringSanitizer
{
    public static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value) || IsCodeLike(value))
        {
            return value;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return "redacted-" + Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }

    private static bool IsCodeLike(string value)
    {
        if (LooksLikeFileName(value))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (char.IsAsciiLetterOrDigit(character)
                || character is '.' or '_' or '-')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool LooksLikeFileName(string value)
    {
        if (value.StartsWith(".", StringComparison.Ordinal))
        {
            return false;
        }

        var extension = Path.GetExtension(value);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return extension is ".txt" or ".pdf" or ".exe" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx"
            or ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".tif" or ".tiff"
            or ".zip" or ".7z" or ".rar" or ".json" or ".xml" or ".csv" or ".md" or ".cs" or ".ps1";
    }
}
