using System.Text.Json;
using TauWeb.Core.Models;

namespace TauWeb.Core.Import;

public static class EverdeckGameImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static TauGame Import(string slug, string gameJson, string rulesJson, IEnumerable<string> cardZoneJsonDocuments)
    {
        using var gameDocument = JsonDocument.Parse(gameJson, new JsonDocumentOptions { AllowTrailingCommas = true });
        using var rulesDocument = JsonDocument.Parse(rulesJson, new JsonDocumentOptions { AllowTrailingCommas = true });
        var gameRoot = gameDocument.RootElement;
        var rulesRoot = rulesDocument.RootElement;

        var zones = cardZoneJsonDocuments
            .Select(ImportZone)
            .OrderBy(zone => zone.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var zoneBySourceId = cardZoneJsonDocuments
            .Select(json =>
            {
                using var document = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true });
                var root = document.RootElement;
                return new
                {
                    SourceId = MongoString(root.GetProperty("_id")),
                    Zone = ImportZone(root)
                };
            })
            .ToDictionary(item => item.SourceId, item => item.Zone.Id, StringComparer.OrdinalIgnoreCase);

        return new TauGame
        {
            Slug = slug,
            Name = StringValue(gameRoot, "title"),
            Summary = StringValue(gameRoot, "shDescription"),
            Description = StringValue(rulesRoot, "summary"),
            MinPlayers = IntValue(gameRoot.GetProperty("plCount"), "plCountMin"),
            MaxPlayers = IntValue(gameRoot.GetProperty("plCount"), "plCountMax"),
            MinPlayMinutes = IntValue(gameRoot.GetProperty("plTime"), "plTimeMin"),
            MaxPlayMinutes = IntValue(gameRoot.GetProperty("plTime"), "plTimeMax"),
            Complexity = DoubleValue(gameRoot, "complexity"),
            MapStrength = StringValue(gameRoot, "mapStrength"),
            Status = "Imported source mapping",
            SourceName = "Everdeck Library",
            SourceUrl = "https://everdeck-library.vercel.app/",
            BggLink = StringValue(rulesRoot, "bggLink"),
            RuleSource = StringValue(rulesRoot.GetProperty("rules"), "ruleSource"),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            GameTypes = StringArray(gameRoot, "gameType"),
            Components = StringArray(rulesRoot, "extComponents").Concat(StringArray(gameRoot, "extComponents")).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            Rules = new TauGameRules
            {
                Objective = StringValue(rulesRoot.GetProperty("rules"), "objective"),
                Setup = StringValue(rulesRoot.GetProperty("rules"), "setup"),
                Gameplay = StringValue(rulesRoot.GetProperty("rules"), "gameplay"),
                MapDifferences = StringValue(rulesRoot.GetProperty("rules"), "mapDiff")
            },
            CardZones = zones,
            CardMappings = ImportMappings(rulesRoot, zoneBySourceId)
        };
    }

    private static TauGameCardZone ImportZone(string json)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true });
        return ImportZone(document.RootElement);
    }

    private static TauGameCardZone ImportZone(JsonElement root)
    {
        var name = StringValue(root, "name");
        return new TauGameCardZone
        {
            Id = Slug.Create(name),
            Name = name,
            Keyword = StringValue(root, "keyword"),
            Components = StringArray(root, "cardComponents"),
            Rules = root.TryGetProperty("extRules", out var rules) && rules.ValueKind == JsonValueKind.Array
                ? rules.EnumerateArray().Select(rule => rule.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray()
                : []
        };
    }

    private static IReadOnlyList<TauGameCardMapping> ImportMappings(JsonElement rulesRoot, IReadOnlyDictionary<string, string> zoneBySourceId)
    {
        if (!rulesRoot.TryGetProperty("cardTable", out var table) || table.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return table.EnumerateArray()
            .Select((cell, index) =>
            {
                var sourceSuit = StringValue(cell, "suit");
                var sourceRank = StringValue(cell, "rank");
                return new TauGameCardMapping
                {
                    Id = $"{sourceSuit}-{sourceRank}-{index}",
                    SourceSuit = sourceSuit,
                    SourceRank = sourceRank,
                    Label = StringValue(cell, "label"),
                    ZoneIds = ZoneIds(cell, zoneBySourceId)
                };
            })
            .ToArray();
    }

    private static IReadOnlyList<string> ZoneIds(JsonElement cell, IReadOnlyDictionary<string, string> zoneBySourceId)
    {
        if (!cell.TryGetProperty("cardZone", out var zones) || zones.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return zones.EnumerateArray()
            .Select(MongoString)
            .Where(sourceId => zoneBySourceId.ContainsKey(sourceId))
            .Select(sourceId => zoneBySourceId[sourceId])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string StringValue(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value))
        {
            return "";
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? "";
        }

        return MongoString(value);
    }

    private static int IntValue(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value))
        {
            return 0;
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.GetInt32();
        }

        if (int.TryParse(MongoString(value), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static double DoubleValue(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value))
        {
            return 0;
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDouble();
        }

        if (double.TryParse(MongoString(value), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static IReadOnlyList<string> StringArray(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? "" : item.ToString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }

    private static string MongoString(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in new[] { "$oid", "$numberInt", "$numberDouble" })
            {
                if (value.TryGetProperty(name, out var wrapped))
                {
                    return wrapped.GetString() ?? "";
                }
            }
        }

        return value.ToString();
    }
}
