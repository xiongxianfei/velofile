using System.Text.Json;
using System.Text.Json.Nodes;

namespace VeloFile.Core.Persistence;

public sealed class SessionStateDocumentCodec : IDurableDocumentCodec<SessionStatePayload>
{
    public static SessionStateDocumentCodec Instance { get; } = new();

    private SessionStateDocumentCodec()
    {
    }

    public string Serialize(DurableDocumentEnvelope<SessionStatePayload> envelope)
    {
        var root = new JsonObject
        {
            ["documentType"] = envelope.DocumentType,
            ["schemaVersion"] = envelope.SchemaVersion,
            ["minimumReaderVersion"] = envelope.MinimumReaderVersion,
            ["appVersion"] = envelope.AppVersion,
            ["writtenAtUtc"] = envelope.WrittenAtUtc.ToString("O"),
            ["payload"] = SerializePayload(envelope.Payload)
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
    }

    public DocumentReadResult<SessionStatePayload> Read(string json)
    {
        try
        {
            var root = JsonNode.Parse(json)?.AsObject();
            if (root is null)
            {
                return Failure("Document root is missing.");
            }

            var documentType = ReadRequiredString(root, "documentType");
            var schemaVersion = ReadRequiredInt(root, "schemaVersion");
            var minimumReaderVersion = ReadRequiredInt(root, "minimumReaderVersion");
            var appVersion = ReadRequiredString(root, "appVersion");
            var writtenAtUtc = ReadRequiredTimestamp(root, "writtenAtUtc");
            var payloadObject = root["payload"]?.AsObject() ?? throw new JsonException("Payload is missing.");

            var fallbackEvents = new List<PersistenceFallbackEvent>();
            var corruptFieldCount = 0;
            var tabs = ReadTabs(payloadObject, fallbackEvents, ref corruptFieldCount);
            var activeTabIndex = ReadOptionalInt(payloadObject, "activeTabIndex", 0, fallbackEvents, ref corruptFieldCount);
            if (activeTabIndex < 0 || activeTabIndex >= Math.Max(tabs.Count, 1))
            {
                fallbackEvents.Add(new PersistenceFallbackEvent("activeTabIndex", "out-of-range"));
                corruptFieldCount++;
                activeTabIndex = 0;
            }

            var windowPlacement = ReadWindowPlacement(payloadObject, fallbackEvents, ref corruptFieldCount);

            var unknownFieldCount = CountUnknownFields(root, RootFields) + CountUnknownFields(payloadObject, PayloadFields);
            var payload = new SessionStatePayload(tabs, activeTabIndex, windowPlacement);
            var envelope = DurableDocumentEnvelope.Create(
                documentType,
                schemaVersion,
                minimumReaderVersion,
                appVersion,
                writtenAtUtc,
                payload);

            return new DocumentReadResult<SessionStatePayload>(
                Success: true,
                Document: envelope,
                Fallbacks: fallbackEvents,
                UnknownFieldCount: unknownFieldCount,
                CorruptFieldCount: corruptFieldCount,
                FailureReason: null);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return Failure(ex.Message);
        }
    }

    private static readonly HashSet<string> RootFields = new(StringComparer.Ordinal)
    {
        "documentType",
        "schemaVersion",
        "minimumReaderVersion",
        "appVersion",
        "writtenAtUtc",
        "payload"
    };

    private static readonly HashSet<string> PayloadFields = new(StringComparer.Ordinal)
    {
        "tabs",
        "activeTabIndex",
        "windowPlacement"
    };

    private static JsonObject SerializePayload(SessionStatePayload payload)
    {
        var tabs = new JsonArray();

        foreach (var tab in payload.Tabs)
        {
            tabs.Add(new JsonObject
            {
                ["path"] = tab.Path,
                ["sortColumn"] = tab.SortColumn,
                ["sortDirection"] = tab.SortDirection,
                ["viewMode"] = tab.ViewMode,
                ["scrollAnchorName"] = tab.ScrollAnchorName,
                ["backHistory"] = ToJsonArray(tab.BackHistory),
                ["forwardHistory"] = ToJsonArray(tab.ForwardHistory)
            });
        }

        var payloadObject = new JsonObject
        {
            ["tabs"] = tabs,
            ["activeTabIndex"] = payload.ActiveTabIndex,
        };

        if (payload.WindowPlacement is not null)
        {
            payloadObject["windowPlacement"] = new JsonObject
            {
                ["left"] = payload.WindowPlacement.Left,
                ["top"] = payload.WindowPlacement.Top,
                ["width"] = payload.WindowPlacement.Width,
                ["height"] = payload.WindowPlacement.Height,
                ["monitorDeviceName"] = payload.WindowPlacement.MonitorDeviceName
            };
        }

        return payloadObject;
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static IReadOnlyList<SessionTabState> ReadTabs(
        JsonObject payload,
        List<PersistenceFallbackEvent> fallbackEvents,
        ref int corruptFieldCount)
    {
        if (payload["tabs"] is not JsonArray tabsArray)
        {
            fallbackEvents.Add(new PersistenceFallbackEvent("tabs", "missing-or-malformed"));
            corruptFieldCount++;
            return [];
        }

        var tabs = new List<SessionTabState>();

        foreach (var tabNode in tabsArray)
        {
            if (tabNode is not JsonObject tabObject || tabObject["path"]?.GetValue<string>() is not { Length: > 0 } path)
            {
                fallbackEvents.Add(new PersistenceFallbackEvent("tabs[]", "malformed-tab"));
                corruptFieldCount++;
                continue;
            }

            tabs.Add(new SessionTabState(
                Path: path,
                SortColumn: ReadOptionalString(tabObject, "sortColumn", "name"),
                SortDirection: ReadOptionalString(tabObject, "sortDirection", "ascending"),
                ViewMode: ReadOptionalString(tabObject, "viewMode", "details"),
                ScrollAnchorName: ReadNullableString(tabObject, "scrollAnchorName"),
                BackHistory: ReadStringArray(tabObject, "backHistory"),
                ForwardHistory: ReadStringArray(tabObject, "forwardHistory")));
        }

        return tabs;
    }

    private static WindowPlacementState? ReadWindowPlacement(
        JsonObject payload,
        List<PersistenceFallbackEvent> fallbackEvents,
        ref int corruptFieldCount)
    {
        if (payload["windowPlacement"] is null)
        {
            return null;
        }

        try
        {
            var placement = payload["windowPlacement"]?.AsObject()
                ?? throw new JsonException("windowPlacement is malformed.");

            return new WindowPlacementState(
                Left: placement["left"]?.GetValue<int>() ?? throw new JsonException("windowPlacement.left is missing."),
                Top: placement["top"]?.GetValue<int>() ?? throw new JsonException("windowPlacement.top is missing."),
                Width: placement["width"]?.GetValue<int>() ?? throw new JsonException("windowPlacement.width is missing."),
                Height: placement["height"]?.GetValue<int>() ?? throw new JsonException("windowPlacement.height is missing."),
                MonitorDeviceName: ReadNullableString(placement, "monitorDeviceName"));
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            fallbackEvents.Add(new PersistenceFallbackEvent("windowPlacement", "malformed"));
            corruptFieldCount++;
            return null;
        }
    }

    private static string ReadRequiredString(JsonObject root, string fieldName)
    {
        return root[fieldName]?.GetValue<string>() ?? throw new JsonException($"{fieldName} is missing.");
    }

    private static int ReadRequiredInt(JsonObject root, string fieldName)
    {
        return root[fieldName]?.GetValue<int>() ?? throw new JsonException($"{fieldName} is missing.");
    }

    private static DateTimeOffset ReadRequiredTimestamp(JsonObject root, string fieldName)
    {
        var value = ReadRequiredString(root, fieldName);
        return DateTimeOffset.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private static int ReadOptionalInt(
        JsonObject root,
        string fieldName,
        int fallback,
        List<PersistenceFallbackEvent> fallbackEvents,
        ref int corruptFieldCount)
    {
        try
        {
            return root[fieldName]?.GetValue<int>() ?? fallback;
        }
        catch (InvalidOperationException)
        {
            fallbackEvents.Add(new PersistenceFallbackEvent(fieldName, "malformed"));
            corruptFieldCount++;
            return fallback;
        }
    }

    private static string ReadOptionalString(JsonObject root, string fieldName, string fallback)
    {
        try
        {
            return root[fieldName]?.GetValue<string>() ?? fallback;
        }
        catch (InvalidOperationException)
        {
            return fallback;
        }
    }

    private static string? ReadNullableString(JsonObject root, string fieldName)
    {
        try
        {
            return root[fieldName]?.GetValue<string>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> ReadStringArray(JsonObject root, string fieldName)
    {
        if (root[fieldName] is not JsonArray array)
        {
            return [];
        }

        return array
            .Select(node =>
            {
                try
                {
                    return node?.GetValue<string>();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            })
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
    }

    private static int CountUnknownFields(JsonObject root, HashSet<string> allowed)
    {
        return root.Count(property => !allowed.Contains(property.Key));
    }

    private static DocumentReadResult<SessionStatePayload> Failure(string reason)
    {
        return new DocumentReadResult<SessionStatePayload>(
            Success: false,
            Document: null,
            Fallbacks: [],
            UnknownFieldCount: 0,
            CorruptFieldCount: 1,
            FailureReason: reason);
    }
}
