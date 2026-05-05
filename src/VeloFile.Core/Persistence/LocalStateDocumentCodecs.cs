using System.Text.Json;
using System.Text.Json.Nodes;

namespace VeloFile.Core.Persistence;

public sealed class SettingsStateDocumentCodec : IDurableDocumentCodec<SettingsStatePayload>
{
    public static SettingsStateDocumentCodec Instance { get; } = new();

    private SettingsStateDocumentCodec()
    {
    }

    public string Serialize(DurableDocumentEnvelope<SettingsStatePayload> envelope)
    {
        return DurableDocumentCodecJson.SerializeEnvelope(envelope, new JsonObject
        {
            ["showHiddenFiles"] = envelope.Payload.ShowHiddenFiles,
            ["showProtectedOperatingSystemFiles"] = envelope.Payload.ShowProtectedOperatingSystemFiles,
            ["showFileExtensions"] = envelope.Payload.ShowFileExtensions,
            ["preferredTerminalTargetId"] = envelope.Payload.PreferredTerminalTargetId
        });
    }

    public DocumentReadResult<SettingsStatePayload> Read(string json)
    {
        try
        {
            var root = DurableDocumentCodecJson.ParseRoot(json);
            var header = DurableDocumentCodecJson.ReadHeader(root);
            var payload = DurableDocumentCodecJson.ReadPayload(root);
            var fallbacks = new List<PersistenceFallbackEvent>();
            var corruptFields = 0;

            var defaults = SettingsStatePayload.Default;
            var settings = new SettingsStatePayload(
                ShowHiddenFiles: ReadOptionalBool(payload, "showHiddenFiles", defaults.ShowHiddenFiles, fallbacks, ref corruptFields),
                ShowProtectedOperatingSystemFiles: ReadOptionalBool(payload, "showProtectedOperatingSystemFiles", defaults.ShowProtectedOperatingSystemFiles, fallbacks, ref corruptFields),
                ShowFileExtensions: ReadOptionalBool(payload, "showFileExtensions", defaults.ShowFileExtensions, fallbacks, ref corruptFields),
                PreferredTerminalTargetId: ReadOptionalString(payload, "preferredTerminalTargetId", defaults.PreferredTerminalTargetId, fallbacks, ref corruptFields));

            var envelope = DurableDocumentEnvelope.Create(
                header.DocumentType,
                header.SchemaVersion,
                header.MinimumReaderVersion,
                header.AppVersion,
                header.WrittenAtUtc,
                settings);

            return DurableDocumentCodecJson.Success(envelope, fallbacks, DurableDocumentCodecJson.CountUnknown(root, RootFields) + DurableDocumentCodecJson.CountUnknown(payload, SettingsFields), corruptFields);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return DurableDocumentCodecJson.Failure<SettingsStatePayload>(ex.Message);
        }
    }

    private static bool ReadOptionalBool(JsonObject payload, string fieldName, bool fallback, List<PersistenceFallbackEvent> fallbacks, ref int corruptFields)
    {
        try
        {
            return payload[fieldName]?.GetValue<bool>() ?? fallback;
        }
        catch (InvalidOperationException)
        {
            fallbacks.Add(new PersistenceFallbackEvent(fieldName, "malformed"));
            corruptFields++;
            return fallback;
        }
    }

    private static string? ReadOptionalString(
        JsonObject payload,
        string fieldName,
        string? fallback,
        List<PersistenceFallbackEvent> fallbacks,
        ref int corruptFields)
    {
        try
        {
            return payload[fieldName]?.GetValue<string>() ?? fallback;
        }
        catch (InvalidOperationException)
        {
            fallbacks.Add(new PersistenceFallbackEvent(fieldName, "malformed"));
            corruptFields++;
            return fallback;
        }
    }

    private static readonly HashSet<string> RootFields = DurableDocumentCodecJson.RootFields;

    private static readonly HashSet<string> SettingsFields = new(StringComparer.Ordinal)
    {
        "showHiddenFiles",
        "showProtectedOperatingSystemFiles",
        "showFileExtensions",
        "preferredTerminalTargetId"
    };
}

public sealed class FavoritesStateDocumentCodec : IDurableDocumentCodec<FavoritesStatePayload>
{
    public static FavoritesStateDocumentCodec Instance { get; } = new();

    private FavoritesStateDocumentCodec()
    {
    }

    public string Serialize(DurableDocumentEnvelope<FavoritesStatePayload> envelope)
    {
        var pinned = new JsonArray();
        foreach (var location in envelope.Payload.PinnedLocations)
        {
            pinned.Add(new JsonObject
            {
                ["displayName"] = location.DisplayName,
                ["path"] = location.Path
            });
        }

        return DurableDocumentCodecJson.SerializeEnvelope(envelope, new JsonObject
        {
            ["pinnedLocations"] = pinned
        });
    }

    public DocumentReadResult<FavoritesStatePayload> Read(string json)
    {
        try
        {
            var root = DurableDocumentCodecJson.ParseRoot(json);
            var header = DurableDocumentCodecJson.ReadHeader(root);
            var payload = DurableDocumentCodecJson.ReadPayload(root);
            var fallbacks = new List<PersistenceFallbackEvent>();
            var corruptFields = 0;
            var pinned = new List<PinnedLocationState>();

            if (payload["pinnedLocations"] is JsonArray locations)
            {
                foreach (var node in locations)
                {
                    try
                    {
                        if (node is not JsonObject item)
                        {
                            throw new JsonException("Favorite item is malformed.");
                        }

                        var displayName = item["displayName"]?.GetValue<string>() ?? throw new JsonException("displayName is missing.");
                        var path = item["path"]?.GetValue<string>() ?? throw new JsonException("path is missing.");
                        pinned.Add(new PinnedLocationState(displayName, path));
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidOperationException)
                    {
                        fallbacks.Add(new PersistenceFallbackEvent("pinnedLocations[]", "malformed"));
                        corruptFields++;
                    }
                }
            }
            else
            {
                fallbacks.Add(new PersistenceFallbackEvent("pinnedLocations", "missing-or-malformed"));
                corruptFields++;
            }

            var envelope = DurableDocumentEnvelope.Create(
                header.DocumentType,
                header.SchemaVersion,
                header.MinimumReaderVersion,
                header.AppVersion,
                header.WrittenAtUtc,
                new FavoritesStatePayload(pinned));

            return DurableDocumentCodecJson.Success(envelope, fallbacks, DurableDocumentCodecJson.CountUnknown(root, RootFields) + DurableDocumentCodecJson.CountUnknown(payload, FavoritesFields), corruptFields);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return DurableDocumentCodecJson.Failure<FavoritesStatePayload>(ex.Message);
        }
    }

    private static readonly HashSet<string> RootFields = DurableDocumentCodecJson.RootFields;

    private static readonly HashSet<string> FavoritesFields = new(StringComparer.Ordinal)
    {
        "pinnedLocations"
    };
}

public sealed class RecentLocationsStateDocumentCodec : IDurableDocumentCodec<RecentLocationsStatePayload>
{
    public static RecentLocationsStateDocumentCodec Instance { get; } = new();

    private RecentLocationsStateDocumentCodec()
    {
    }

    public string Serialize(DurableDocumentEnvelope<RecentLocationsStatePayload> envelope)
    {
        var entries = new JsonArray();
        foreach (var entry in envelope.Payload.Entries)
        {
            entries.Add(new JsonObject
            {
                ["path"] = entry.Path,
                ["lastVisitedUtc"] = entry.LastVisitedUtc.ToString("O")
            });
        }

        return DurableDocumentCodecJson.SerializeEnvelope(envelope, new JsonObject
        {
            ["entries"] = entries
        });
    }

    public DocumentReadResult<RecentLocationsStatePayload> Read(string json)
    {
        try
        {
            var root = DurableDocumentCodecJson.ParseRoot(json);
            var header = DurableDocumentCodecJson.ReadHeader(root);
            var payload = DurableDocumentCodecJson.ReadPayload(root);
            var fallbacks = new List<PersistenceFallbackEvent>();
            var corruptFields = 0;
            var entries = new List<RecentLocationState>();

            if (payload["entries"] is JsonArray items)
            {
                foreach (var node in items)
                {
                    try
                    {
                        if (node is not JsonObject item)
                        {
                            throw new JsonException("Recent location item is malformed.");
                        }

                        var path = item["path"]?.GetValue<string>() ?? throw new JsonException("path is missing.");
                        var lastVisited = DateTimeOffset.Parse(
                            item["lastVisitedUtc"]?.GetValue<string>() ?? throw new JsonException("lastVisitedUtc is missing."),
                            null,
                            System.Globalization.DateTimeStyles.RoundtripKind);
                        entries.Add(new RecentLocationState(path, lastVisited));
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
                    {
                        fallbacks.Add(new PersistenceFallbackEvent("entries[]", "malformed"));
                        corruptFields++;
                    }
                }
            }
            else
            {
                fallbacks.Add(new PersistenceFallbackEvent("entries", "missing-or-malformed"));
                corruptFields++;
            }

            var envelope = DurableDocumentEnvelope.Create(
                header.DocumentType,
                header.SchemaVersion,
                header.MinimumReaderVersion,
                header.AppVersion,
                header.WrittenAtUtc,
                RecentLocationsStatePayload.Create(entries));

            return DurableDocumentCodecJson.Success(envelope, fallbacks, DurableDocumentCodecJson.CountUnknown(root, RootFields) + DurableDocumentCodecJson.CountUnknown(payload, RecentFields), corruptFields);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return DurableDocumentCodecJson.Failure<RecentLocationsStatePayload>(ex.Message);
        }
    }

    private static readonly HashSet<string> RootFields = DurableDocumentCodecJson.RootFields;

    private static readonly HashSet<string> RecentFields = new(StringComparer.Ordinal)
    {
        "entries"
    };
}

internal static class DurableDocumentCodecJson
{
    public static readonly HashSet<string> RootFields = new(StringComparer.Ordinal)
    {
        "documentType",
        "schemaVersion",
        "minimumReaderVersion",
        "appVersion",
        "writtenAtUtc",
        "payload"
    };

    public static string SerializeEnvelope<TPayload>(DurableDocumentEnvelope<TPayload> envelope, JsonObject payload)
    {
        var root = new JsonObject
        {
            ["documentType"] = envelope.DocumentType,
            ["schemaVersion"] = envelope.SchemaVersion,
            ["minimumReaderVersion"] = envelope.MinimumReaderVersion,
            ["appVersion"] = envelope.AppVersion,
            ["writtenAtUtc"] = envelope.WrittenAtUtc.ToString("O"),
            ["payload"] = payload
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
    }

    public static JsonObject ParseRoot(string json)
    {
        return JsonNode.Parse(json)?.AsObject() ?? throw new JsonException("Document root is missing.");
    }

    public static DurableDocumentHeader ReadHeader(JsonObject root)
    {
        return new DurableDocumentHeader(
            DocumentType: root["documentType"]?.GetValue<string>() ?? throw new JsonException("documentType is missing."),
            SchemaVersion: root["schemaVersion"]?.GetValue<int>() ?? throw new JsonException("schemaVersion is missing."),
            MinimumReaderVersion: root["minimumReaderVersion"]?.GetValue<int>() ?? throw new JsonException("minimumReaderVersion is missing."),
            AppVersion: root["appVersion"]?.GetValue<string>() ?? throw new JsonException("appVersion is missing."),
            WrittenAtUtc: DateTimeOffset.Parse(
                root["writtenAtUtc"]?.GetValue<string>() ?? throw new JsonException("writtenAtUtc is missing."),
                null,
                System.Globalization.DateTimeStyles.RoundtripKind));
    }

    public static JsonObject ReadPayload(JsonObject root)
    {
        return root["payload"]?.AsObject() ?? throw new JsonException("payload is missing.");
    }

    public static DocumentReadResult<TPayload> Success<TPayload>(
        DurableDocumentEnvelope<TPayload> envelope,
        IReadOnlyList<PersistenceFallbackEvent> fallbacks,
        int unknownFieldCount,
        int corruptFieldCount)
    {
        return new DocumentReadResult<TPayload>(
            Success: true,
            Document: envelope,
            Fallbacks: fallbacks,
            UnknownFieldCount: unknownFieldCount,
            CorruptFieldCount: corruptFieldCount,
            FailureReason: null);
    }

    public static DocumentReadResult<TPayload> Failure<TPayload>(string reason)
    {
        return new DocumentReadResult<TPayload>(
            Success: false,
            Document: null,
            Fallbacks: [],
            UnknownFieldCount: 0,
            CorruptFieldCount: 1,
            FailureReason: reason);
    }

    public static int CountUnknown(JsonObject root, HashSet<string> allowed)
    {
        return root.Count(property => !allowed.Contains(property.Key));
    }
}

internal sealed record DurableDocumentHeader(
    string DocumentType,
    int SchemaVersion,
    int MinimumReaderVersion,
    string AppVersion,
    DateTimeOffset WrittenAtUtc);
