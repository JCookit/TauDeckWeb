using System.Globalization;

namespace TauWeb.Core.Models;

public sealed record TauSystemTextOverlay
{
    public Dictionary<string, TauSystemText> Systems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record TauSystemText
{
    public int? SortOrder { get; init; }
    public IReadOnlyList<string>? Summary { get; init; }
    public IReadOnlyList<string>? WhyExists { get; init; }
    public IReadOnlyList<string>? ExistingGameMappings { get; init; }
    public IReadOnlyList<string>? NovelGameUses { get; init; }
    public IReadOnlyList<string>? UsesInPlay { get; init; }
    public IReadOnlyList<string>? AuthorsNote { get; init; }
    public IReadOnlyList<string>? Relationships { get; init; }
    public IReadOnlyList<int>? InstanceBreakAfter { get; init; }
    public Dictionary<string, TauSystemInstanceText> Instances { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, TauSystemGroupText> Groups { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record TauSystemInstanceText
{
    public IReadOnlyList<string>? Description { get; init; }
    public int? SortOrder { get; init; }
}

public sealed record TauSystemGroupText
{
    public string? Name { get; init; }
    public IReadOnlyList<string>? Description { get; init; }
    public string? Image { get; init; }
    public int? SortOrder { get; init; }
    public string FilterKey { get; init; } = "";
    public TauFilterExpression Filter { get; init; } = new();
}

public static class TauSystemTextOverlayMerger
{
    public static TauSiteData Apply(TauSiteData data, TauSystemTextOverlay? overlay)
    {
        if (overlay is null || overlay.Systems.Count == 0)
        {
            return data;
        }

        return data with
        {
            Systems = data.Systems
                .Select((system, index) => system.SortOrder == 0
                    ? system with { SortOrder = (index + 1) * 10 }
                    : system)
                .Select(system => ApplySystem(system, overlay, data.Cards))
                .OrderBy(system => system.SortOrder)
                .ToList()
        };
    }

    private static TauSystem ApplySystem(TauSystem system, TauSystemTextOverlay overlay, IReadOnlyList<TauCard> cards)
    {
        if (!overlay.Systems.TryGetValue(system.Key, out var text))
        {
            return system;
        }

        return system with
        {
            SortOrder = text.SortOrder ?? system.SortOrder,
            Summary = TextOrDefault(text.Summary, system.Summary),
            WhyExists = TextOrDefault(text.WhyExists, system.WhyExists),
            ExistingGameMappings = TextOrDefault(text.ExistingGameMappings, system.ExistingGameMappings),
            NovelGameUses = TextOrDefault(text.NovelGameUses, system.NovelGameUses),
            UsesInPlay = TextOrDefault(text.UsesInPlay, system.UsesInPlay),
            AuthorsNote = TextOrDefault(text.AuthorsNote, system.AuthorsNote),
            Relationships = TextOrDefault(text.Relationships, system.Relationships),
            Instances = system.Instances
                .Select(UseNumericInstanceFilter)
                .Select(instance => ApplyInstance(instance, text))
                .OrderBy(instance => instance.SortOrder)
                .ToList(),
            InstanceBreakAfter = text.InstanceBreakAfter ?? system.InstanceBreakAfter,
            Groups = BuildGroups(system, text, cards)
        };
    }

    private static IReadOnlyList<TauSystemGroup> BuildGroups(TauSystem system, TauSystemText text, IReadOnlyList<TauCard> cards)
    {
        return text.Groups
            .Concat(DeckGroups(system.Key).Where(item => !text.Groups.ContainsKey(item.Key)))
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .Select((item, index) =>
            {
                var slug = item.Key.Trim();
                var group = item.Value;
                return new TauSystemGroup
                {
                    Slug = slug,
                    Name = string.IsNullOrWhiteSpace(group.Name) ? item.Key : group.Name,
                    Description = TextOrDefault(group.Description, ""),
                    Image = group.Image ?? "",
                    Count = cards.Count(card => Matches(card, group.Filter)),
                    SortOrder = group.SortOrder ?? (10_000 + index),
                    FilterKey = NumericFilterKey(group.FilterKey),
                    FilterValue = $"group:{system.Key}:{slug}",
                    Filter = group.Filter
                };
            })
            .OrderBy(group => group.SortOrder)
            .ThenBy(group => group.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static TauSystemInstance UseNumericInstanceFilter(TauSystemInstance instance)
    {
        var key = NumericFilterKey(instance.FilterKey);
        // Older edition snapshots use the original number as the sort order.
        return key == instance.FilterKey
            ? instance
            : instance with { FilterKey = key, FilterValue = instance.SortOrder.ToString(CultureInfo.InvariantCulture) };
    }

    private static string NumericFilterKey(string key) => key switch
    {
        "sphere" => "sphereNumber",
        "mode" => "modeNumber",
        _ => key
    };

    private static IEnumerable<KeyValuePair<string, TauSystemGroupText>> DeckGroups(string systemKey)
    {
        return systemKey switch
        {
            "sequence-numbers" => Enumerable.Range(0, 10).Select(digit =>
                DeckGroup($"ending-{digit}", $"Ends in {digit}", "sequenceDigit", digit.ToString(), digit)),
            "suits" =>
            [
                DeckGroup("hard", "Hard suits", "suitShape", "Hard", 105),
                DeckGroup("soft", "Soft suits", "suitShape", "Soft", 106)
            ],
            _ => []
        };
    }

    private static KeyValuePair<string, TauSystemGroupText> DeckGroup(string slug, string name, string field, string value, int order)
        => new(slug, new TauSystemGroupText
        {
            Name = name,
            SortOrder = order,
            FilterKey = field,
            Filter = new TauFilterExpression { Field = field, EqualTo = value }
        });

    private static TauSystemInstance ApplyInstance(TauSystemInstance instance, TauSystemText text)
    {
        var instanceText = InstanceText(instance, text);
        if (instanceText is null)
        {
            return instance;
        }

        return instance with
        {
            Description = TextOrDefault(instanceText.Description, instance.Description),
            SortOrder = instanceText.SortOrder ?? instance.SortOrder
        };
    }

    private static TauSystemInstanceText? InstanceText(TauSystemInstance instance, TauSystemText text)
    {
        if (text.Instances.TryGetValue(instance.Slug, out var instanceText)
            || text.Instances.TryGetValue(instance.Value, out instanceText)
            || text.Instances.TryGetValue(instance.Name, out instanceText))
        {
            return instanceText;
        }

        return null;
    }

    private static string TextOrDefault(IReadOnlyList<string>? paragraphs, string fallback)
    {
        return paragraphs is null ? fallback : string.Join("\n\n", paragraphs);
    }

    public static bool Matches(TauCard card, TauFilterExpression filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.HasBorderSide))
        {
            return filter.HasBorderSide.ToLowerInvariant() switch
            {
                "top" => card.BorderTop > 0,
                "right" => card.BorderRight > 0,
                "bottom" => card.BorderBottom > 0,
                "left" => card.BorderLeft > 0,
                _ => false
            };
        }

        var value = CardValue(card, filter.Field);
        if (!string.IsNullOrWhiteSpace(filter.EqualTo))
        {
            return value.Equals(filter.EqualTo, StringComparison.OrdinalIgnoreCase);
        }

        return filter.In.Count > 0 && filter.In.Any(item => value.Equals(item, StringComparison.OrdinalIgnoreCase));
    }

    private static string CardValue(TauCard card, string field)
    {
        return field switch
        {
            "sequence" => card.Sequence,
            "sequenceDigit" => card.SequenceDigit,
            "suit" => card.Suit.ToString(),
            "suitName" => card.SuitName,
            "suitShape" => card.SuitShape,
            "color" => card.Color,
            "rank" => card.Rank.ToString(),
            "sphere" => card.Sphere,
            "sphereNumber" => card.SphereNumber.ToString(),
            "mode" => card.Mode,
            "modeNumber" => card.ModeNumber.ToString(),
            "sphereZodiac" => string.IsNullOrWhiteSpace(card.SphereZodiac) ? "__none__" : card.SphereZodiac,
            "firstLetter" => card.FirstLetter,
            "letterScore" => card.LetterScore.ToString(),
            "direction" => card.Direction,
            "borderCount" => card.BorderCount.ToString(),
            "totalBorderScore" => card.TotalBorderScore.ToString(),
            "borderZodiac" => string.IsNullOrWhiteSpace(card.BorderZodiac) ? "__none__" : card.BorderZodiac,
            "borderArrangement" => card.BorderArrangement,
            "isTarot" => card.IsTarot ? "true" : "false",
            "iconGroup" => card.IconGroup.ToString(),
            "iconHalf" => card.IconHalf?.ToString() ?? "",
            "isStandardDeck" => card.IsStandardDeck?.ToString().ToLowerInvariant() ?? "",
            "partitionGroup" => card.PartitionGroup.ToString(),
            "iconNumber" => card.IconNumber.ToString(),
            "linkInDegree" => card.LinkInDegree.ToString(),
            _ => ""
        };
    }
}
