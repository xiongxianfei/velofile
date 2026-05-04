using System.Security;

namespace VeloFile.Core;

public static class ExpectedFileSystemExceptions
{
    public static bool IsExpected(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException or SecurityException;
    }

    public static string ReasonCode(Exception exception)
    {
        return exception switch
        {
            FileNotFoundException => "missing",
            DirectoryNotFoundException => "missing",
            UnauthorizedAccessException => "access-denied",
            SecurityException => "security-denied",
            IOException => "io-error",
            _ => "unknown"
        };
    }
}
