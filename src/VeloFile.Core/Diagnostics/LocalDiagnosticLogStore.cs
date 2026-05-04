namespace VeloFile.Core.Diagnostics;

public sealed record DiagnosticRetentionPolicy(
    TimeSpan MaxAge,
    long MaxTotalBytes,
    long MaxFileBytes,
    int MaxCrashMarkers)
{
    public static DiagnosticRetentionPolicy Default { get; } = new(
        MaxAge: TimeSpan.FromDays(30),
        MaxTotalBytes: 50L * 1024L * 1024L,
        MaxFileBytes: 5L * 1024L * 1024L,
        MaxCrashMarkers: 10);
}

public sealed class LocalDiagnosticLogStore : IDiagnosticSink
{
    private readonly string _root;
    private readonly string _logsDirectory;
    private readonly string _crashMarkersDirectory;
    private readonly string _lastActionMarkersDirectory;
    private readonly DiagnosticRetentionPolicy _retentionPolicy;

    public LocalDiagnosticLogStore(string root, DiagnosticRetentionPolicy retentionPolicy)
    {
        _root = root;
        _retentionPolicy = retentionPolicy;
        _logsDirectory = Path.Combine(root, "logs");
        _crashMarkersDirectory = Path.Combine(root, "crash-markers");
        _lastActionMarkersDirectory = Path.Combine(root, "last-action-markers");

        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_logsDirectory);
        Directory.CreateDirectory(_crashMarkersDirectory);
        Directory.CreateDirectory(_lastActionMarkersDirectory);
    }

    public void Write(DiagnosticEvent diagnosticEvent)
    {
        var line = DiagnosticJsonSerializer.Serialize(diagnosticEvent) + Environment.NewLine;
        var logPath = CurrentLogPath(line.Length);

        File.AppendAllText(logPath, line);
        ApplyLogRetention(DateTimeOffset.UtcNow);
    }

    public void RecordCrashMarker(string category, DateTimeOffset timestampUtc)
    {
        var marker = new DiagnosticEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            EventType = "crash.marker",
            UtcTimestamp = timestampUtc,
            SequenceNumber = 0,
            Severity = "error",
            Component = "Diagnostics",
            LastActionMarkerCategory = category,
            ResultState = "crashed"
        };

        var path = Path.Combine(_crashMarkersDirectory, $"{timestampUtc.UtcTicks:D20}-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, DiagnosticJsonSerializer.Serialize(marker) + Environment.NewLine);
        RetainLatestCrashMarkers();
    }

    public void RecordLastActionMarker(string category, string component, DateTimeOffset timestampUtc)
    {
        var marker = new DiagnosticEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            EventType = "last-action.marker",
            UtcTimestamp = timestampUtc,
            SequenceNumber = 0,
            Severity = "info",
            Component = component,
            LastActionMarkerCategory = category,
            ResultState = "recorded"
        };

        var path = Path.Combine(_lastActionMarkersDirectory, category + ".json");
        File.WriteAllText(path, DiagnosticJsonSerializer.Serialize(marker) + Environment.NewLine);
    }

    public bool HasRepeatedCrashMarkers(string category, int threshold)
    {
        if (threshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be positive.");
        }

        return Directory
            .EnumerateFiles(_crashMarkersDirectory, "*.json")
            .Select(File.ReadAllText)
            .Count(content => content.Contains($"\"lastActionMarkerCategory\":\"{category}\"", StringComparison.Ordinal)) >= threshold;
    }

    private string CurrentLogPath(int nextLineLength)
    {
        var latest = Directory
            .EnumerateFiles(_logsDirectory, "*.jsonl")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.CreationTimeUtc)
            .FirstOrDefault();

        if (latest is not null && latest.Length + nextLineLength <= _retentionPolicy.MaxFileBytes)
        {
            return latest.FullName;
        }

        return Path.Combine(_logsDirectory, $"diagnostics-{DateTimeOffset.UtcNow.UtcTicks:D20}-{Guid.NewGuid():N}.jsonl");
    }

    private void ApplyLogRetention(DateTimeOffset now)
    {
        var files = Directory
            .EnumerateFiles(_logsDirectory, "*.jsonl")
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.CreationTimeUtc)
            .ToList();

        foreach (var file in files.Where(file => now - file.CreationTimeUtc > _retentionPolicy.MaxAge))
        {
            file.Delete();
        }

        files = Directory
            .EnumerateFiles(_logsDirectory, "*.jsonl")
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.CreationTimeUtc)
            .ToList();

        while (files.Sum(file => file.Length) > _retentionPolicy.MaxTotalBytes && files.Count > 0)
        {
            files[0].Delete();
            files.RemoveAt(0);
        }
    }

    private void RetainLatestCrashMarkers()
    {
        var staleMarkers = Directory
            .EnumerateFiles(_crashMarkersDirectory, "*.json")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.Name)
            .Skip(_retentionPolicy.MaxCrashMarkers);

        foreach (var marker in staleMarkers)
        {
            marker.Delete();
        }
    }
}
