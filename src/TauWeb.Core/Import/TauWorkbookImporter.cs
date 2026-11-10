using ClosedXML.Excel;
using System.Globalization;
using System.Text.RegularExpressions;
using TauWeb.Core.Models;

namespace TauWeb.Core.Import;

public sealed class TauWorkbookImporter
{
    private static readonly IReadOnlyDictionary<int, IconDefinition> IconMetadataOverrides = new Dictionary<int, IconDefinition>
    {
        [5] = new("Bridge", "Arched stone bridge over water."),
        [6] = new("Plough", "Plough cutting through soil."),
        [11] = new("Scythe", "Scythe among cut and standing grain."),
        [14] = new("Night", "Moonlit path under a crescent moon."),
        [17] = new("Horizon", "River or path leading toward the horizon."),
        [30] = new("Broken Rock", "Broken rock with conchoidal fractures."),
        [33] = new("Eclipse", "Eclipse over a distant settlement."),
        [40] = new("Upward Peak", "Path up to a high mountain peak."),
        [41] = new("Downward Arc", "Curving downward passage."),
        [49] = new("Storm", "Advancing storm."),
        [60] = new("Sapling", "Small sprout emerging from soil."),
        [62] = new("Scepter", "Austere throne with aligned rod. Simple and structured."),
        [71] = new("Landscape", "Cliff and canyon landscape.")
    };

    public TauSiteData Import(string workbookPath, bool useLegacyIconMetadataOverrides = true)
    {
        using var workbook = new XLWorkbook(workbookPath);
        var icons = ReadIconDefinitions(workbook.Worksheet("icons"), useLegacyIconMetadataOverrides);
        var cards = ReadCards(workbook.Worksheet("Table"), icons);
        var systems = BuildSystems(cards);

        return new TauSiteData
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            Cards = cards,
            Systems = systems,
            Games = []
        };
    }

    private static IReadOnlyList<TauCard> ReadCards(IXLWorksheet sheet, IReadOnlyDictionary<int, IconDefinition> icons)
    {
        var headers = sheet.Row(1).CellsUsed().ToDictionary(
            cell => cell.GetString().Trim(),
            cell => cell.Address.ColumnNumber,
            StringComparer.OrdinalIgnoreCase);

        var cards = new List<TauCard>();
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var sequence = Text(row, headers, "Sequence");
            if (string.IsNullOrWhiteSpace(sequence))
            {
                continue;
            }

            var id = cards.Count + 1;
            var word = Text(row, headers, "Word");
            var suitName = Text(row, headers, "Suit Name");
            var rank = Int(row, headers, "Rank");
            var name = string.IsNullOrWhiteSpace(word) ? $"{suitName} {rank}" : word;
            var slug = $"{sequence}-{Slug.Create(name)}";
            var sphereFullName = Text(row, headers, "Sphere");
            var sphere = NormalizeSphere(sphereFullName);
            var sphereNumber = SphereNumber(sphereFullName);
            var iconNumber = Int(row, headers, "Icon");
            icons.TryGetValue(iconNumber, out var icon);
            var totalBorderScore = Int(row, headers, "Segment Count");
            var borderTop = Int(row, headers, "Border Top");
            var borderRight = Int(row, headers, "Border Right");
            var borderBottom = Int(row, headers, "Border Bottom");
            var borderLeft = Int(row, headers, "Border Left");
            var linkWord1 = Text(row, headers, "LinkWord1");
            var linkWord2 = Text(row, headers, "LinkWord2");
            var zodiac = Text(row, headers, "Zodiac");

            var glyphImages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["iconHalf"] = WebGlyph(Text(row, headers, "IconHalfFileName")),
                ["suit"] = WebGlyph(Text(row, headers, "SuitFileName")),
                ["rank"] = WebGlyph(Text(row, headers, "RankIconFileName")),
                ["icon"] = WebGlyph(Text(row, headers, "IconFileName")),
                ["iconGroup"] = WebGlyph(Text(row, headers, "IconGroupFilledFileName")),
                ["sphere"] = WebGlyph(Text(row, headers, "SphereIconFileName")),
                ["mode"] = WebGlyph(Text(row, headers, "ModeIconFileName")),
                ["zodiac"] = WebGlyph(Text(row, headers, "ZodiacIconFileName")),
                ["letterScore"] = WebGlyph(Text(row, headers, "LetterScoreIconFileName")),
                ["direction"] = WebGlyph(Text(row, headers, "CompassIconFileName")),
                ["borderScore"] = WebGlyph(Text(row, headers, "BorderScoreIconFileName")),
                ["borderZodiac"] = WebGlyph(Text(row, headers, "BorderZodiacIconFileName"))
            };

            cards.Add(new TauCard
            {
                Id = id,
                Slug = slug,
                Sequence = sequence,
                Suit = Int(row, headers, "Suit"),
                Rank = rank,
                Name = name,
                SuitName = suitName,
                Color = Text(row, headers, "Color"),
                SuitTheme = Text(row, headers, "Suit Theme"),
                RankTheme = Text(row, headers, "Rank Theme"),
                IsTarot = Bool(row, headers, "IsTarot"),
                Word = word,
                FirstLetter = Text(row, headers, "First Letter"),
                Sphere = sphere,
                SphereNumber = sphereNumber,
                SphereFullName = sphereFullName,
                ModeNumber = Int(row, headers, "Mode (0-24)"),
                Mode = Text(row, headers, "Mode"),
                IconNumber = iconNumber,
                IconName = NormalizeTitle(icon?.Name ?? ""),
                IconDescription = NormalizeSentence(icon?.Description ?? ""),
                IconGroup = Int(row, headers, "Icon Group"),
                IconHalf = headers.ContainsKey("Icon Half") ? Int(row, headers, "Icon Half") : null,
                IsStandardDeck = headers.ContainsKey("IsStandardDeck") ? Bool(row, headers, "IsStandardDeck") : null,
                PartitionGroup = Int(row, headers, "Partition"),
                Zodiac = zodiac,
                SphereZodiac = zodiac,
                Direction = Text(row, headers, "Direction"),
                LetterScore = Int(row, headers, "Letter Score (1-6)"),
                BorderCount = Int(row, headers, "Border Count"),
                SegmentCount = totalBorderScore,
                TotalBorderScore = totalBorderScore,
                BorderZodiac = Text(row, headers, "BorderZodiac"),
                BorderTop = borderTop,
                BorderRight = borderRight,
                BorderBottom = borderBottom,
                BorderLeft = borderLeft,
                BorderArrangement = BorderArrangement(borderTop, borderRight, borderBottom, borderLeft),
                LinkWord1 = linkWord1,
                LinkWord2 = linkWord2,
                LinkWords = LinkWords(linkWord1, linkWord2),
                LinkInDegree = Int(row, headers, "LinkInDegree"),
                OriginalImage = $"/assets/cards/Card_{id}.png",
                DisplayImage = $"/generated/cards/card-{id:000}-display.webp",
                ThumbnailImage = $"/generated/cards/card-{id:000}-thumb.webp",
                GlyphImages = glyphImages
            });
        }

        return cards.OrderBy(card => card.Id).ToList();
    }

    private static IReadOnlyDictionary<int, IconDefinition> ReadIconDefinitions(IXLWorksheet sheet, bool useLegacyIconMetadataOverrides)
    {
        var headers = sheet.Row(1).CellsUsed().ToDictionary(
            cell => cell.GetString().Trim(),
            cell => cell.Address.ColumnNumber,
            StringComparer.OrdinalIgnoreCase);

        var icons = new Dictionary<int, IconDefinition>();
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var number = Int(row, headers, "#");
            if (number == 0)
            {
                continue;
            }

            var definition = new IconDefinition(
                Text(row, headers, "Icon / Drawing"),
                Text(row, headers, "Description"));

            icons[number] = useLegacyIconMetadataOverrides ? IconMetadataOverrides.GetValueOrDefault(number, definition) : definition;
        }

        return icons;
    }

    private static IReadOnlyList<TauSystem> BuildSystems(IReadOnlyList<TauCard> cards)
    {
        TauSystem[] systems =
        [
            BuildSystem("suits", "Suits", "The ten suits provide Tau's most familiar card identity: color, family, and classic deck feel.", cards, c => c.SuitName, c => "suit", "suitName", c => $"{c.Color}. {c.SuitTheme}", c => c.Suit),
            BuildSystem("ranks", "Ranks", "Ranks provide the standard 1-15 ordering, rank glyphs, themes, and the rank rail.", cards, c => c.Rank.ToString(CultureInfo.InvariantCulture), c => "rank", "rank", c => $"{c.RankTheme}. Rank rail: {c.Rank} / {SecondaryRankLabel(c.Rank)}.", c => c.Rank),
            BuildOverviewSystem("sequence-numbers", "Sequence Numbers", "Sequence numbers provide the canonical 001-150 order used for publishing, references, and complete-deck organization."),
            BuildSystem("spheres", "Spheres", "Spheres are six 25-card mega-suits: Order, Air, Water, Fire, Earth, and Chaos.", cards, c => c.Sphere, c => "sphere", "sphereNumber", c => c.SphereFullName, c => c.SphereNumber, filterValueSelector: c => c.SphereNumber.ToString(CultureInfo.InvariantCulture)),
            BuildSystem("modes", "Modes", "Modes are 25 recurring qualities that pair with spheres as a second suit/rank-like axis.", cards, c => c.Mode, c => "mode", "modeNumber", c => $"Mode {c.ModeNumber}", c => c.ModeNumber, filterValueSelector: c => c.ModeNumber.ToString(CultureInfo.InvariantCulture)),
            BuildSystem("sphere-zodiac", "Sphere Zodiac", "The sphere zodiac is a sparse overlay tied to elemental sphere assignments and higher-rank planets.", cards, c => c.SphereZodiac, c => "zodiac", "sphereZodiac", null, c => ZodiacSort(c.SphereZodiac), includeEmpty: true),
            BuildSystem("first-letters", "First Letters", "First letters make the word layer usable for alphabetic filtering and word-building games.", cards, c => c.FirstLetter, c => "", "firstLetter", null, c => LetterSort(c.FirstLetter)),
            BuildNumericSystem("letter-scores", "Letter Scores", "Letter scores are a compact 1-6 value attached to the word layer.", cards, c => c.LetterScore, "letterScore", "letterScore"),
            BuildSystem("directions", "Directions", "Directions create a near-four-way partition for movement, orientation, and spatial play.", cards, c => c.Direction, c => "direction", "direction", null, c => DirectionSort(c.Direction)),
            BuildNumericSystem("border-scores", "Border Scores", "Border scores sum the edge values into a 0-16 scale.", cards, c => c.TotalBorderScore, "totalBorderScore", "borderScore"),
            BuildSystem("border-arrangements", "Border Arrangements", "Border arrangements describe which card edges carry border marks.", cards, c => c.BorderArrangement, c => "", "borderArrangement", null, c => BorderArrangementSort(c.BorderArrangement)),
            BuildSystem("border-zodiac", "Border Zodiac", "Border zodiac balances the card face visually while mapping border shapes into a second zodiac overlay.", cards, c => c.BorderZodiac, c => "borderZodiac", "borderZodiac", null, c => ZodiacSort(c.BorderZodiac), includeEmpty: true),
            BuildWordSystem(cards),
            BuildLinkWordSystem(cards),
            BuildNumericSystem("in-link-counts", "In-Link Counts", "In-link count measures how many other cards point at a card through the word-link graph.", cards, c => c.LinkInDegree, "linkInDegree", ""),
            BuildNumericSystem("icon-groups", "Icon Groups", "Icon groups split the 75 icon pairs into three balanced visual subsets.", cards, c => c.IconGroup, "iconGroup", "iconGroup"),
            BuildNumericSystem("partitions", "Partitions", "Partitions are the 1-5 filled-symbol values on the lower rail.", cards, c => c.PartitionGroup, "partitionGroup", ""),
            BuildIconSystem(cards),
            BuildSystem("tarot", "Tarot", "Tarot marks identify the 25 cards that carry Tarot major-arcana roles.", cards, c => c.IsTarot ? "Yes" : "No", c => "", "isTarot", null, c => c.IsTarot ? 0 : 1, filterValueSelector: c => c.IsTarot ? "true" : "false")
        ];

        if (cards.Any(card => card.IconHalf is not null))
        {
            systems = [.. systems,
                BuildSystem("icon-halves", "Icon Halves", "Two 75-card halves each contain one card from every icon pair. The partition end bars identify the half.", cards, c => c.IconHalf?.ToString() ?? "", c => "iconHalf", "iconHalf"),
                BuildSystem("standard-deck", "Standard Deck", "The corner triangles mark the 52 cards used for a standard playing-card deck.", cards, c => c.IsStandardDeck == true ? "Yes" : "No", c => "", "isStandardDeck", filterValueSelector: c => c.IsStandardDeck == true ? "true" : "false")
            ];
        }

        return systems
            .Select((system, index) => system with { SortOrder = (index + 1) * 10 })
            .ToList();
    }

    private static TauSystem BuildOverviewSystem(string key, string name, string summary)
    {
        return new TauSystem
        {
            Key = key,
            Name = name,
            Summary = summary,
            Instances = []
        };
    }

    private static TauSystem BuildSystem(
        string key,
        string name,
        string summary,
        IReadOnlyList<TauCard> cards,
        Func<TauCard, string> valueSelector,
        Func<TauCard, string> imageKeySelector,
        string filterKey,
        Func<TauCard, string>? descriptionSelector = null,
        Func<TauCard, int>? sortSelector = null,
        bool includeEmpty = false,
        Func<TauCard, string>? filterValueSelector = null)
    {
        var instances = cards
            .Select(card => new { Card = card, Value = valueSelector(card).Trim() })
            .Where(item => includeEmpty || !string.IsNullOrWhiteSpace(item.Value))
            .GroupBy(item => item.Value, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First().Card;
                var value = string.IsNullOrWhiteSpace(group.Key) ? "None" : group.Key;
                var filterValue = filterValueSelector?.Invoke(first) ?? (string.IsNullOrWhiteSpace(group.Key) ? "__none__" : group.Key);
                var description = descriptionSelector?.Invoke(first);

                return new TauSystemInstance
                {
                    Slug = Slug.Create(value),
                    Name = key == "directions" ? value.ToUpperInvariant() : value,
                    Value = value,
                    Description = description ?? "",
                    Image = first.GlyphImages.GetValueOrDefault(imageKeySelector(first), ""),
                    SecondaryImage = key == "ranks" ? RankRailGlyph(first.Rank) : "",
                    Count = group.Count(),
                    SortOrder = sortSelector?.Invoke(first) ?? 0,
                    FilterKey = filterKey,
                    FilterValue = filterValue
                };
            })
            .OrderBy(instance => instance.SortOrder)
            .ThenBy(instance => instance.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TauSystem { Key = key, Name = name, Summary = summary, Instances = instances };
    }

    private static TauSystem BuildWordSystem(IReadOnlyList<TauCard> cards)
    {
        var instances = cards
            .Where(card => !string.IsNullOrWhiteSpace(card.Word))
            .OrderBy(card => card.Word, StringComparer.OrdinalIgnoreCase)
            .Select(card => new TauSystemInstance
            {
                Slug = Slug.Create(card.Word),
                Name = card.Word,
                Value = card.Word,
                Description = "",
                Image = card.GlyphImages.GetValueOrDefault("icon", ""),
                Count = 1,
                SortOrder = card.Id,
                FilterKey = "word",
                FilterValue = card.Word
            })
            .ToList();

        return new TauSystem
        {
            Key = "words",
            Name = "Words",
            Summary = "Words are the 150-card vocabulary layer for play, prompts, narrative, and future card-specific flavor.",
            Instances = instances
        };
    }

    private static TauSystem BuildNumericSystem(
        string key,
        string name,
        string summary,
        IReadOnlyList<TauCard> cards,
        Func<TauCard, int> valueSelector,
        string filterKey,
        string imageKey)
    {
        var instances = cards
            .Select(card => valueSelector(card))
            .Distinct()
            .Order()
            .Select(value => new TauSystemInstance
            {
                Slug = value.ToString(),
                Name = value.ToString(),
                Value = value.ToString(),
                Description = "",
                Image = imageKey.Length == 0
                    ? ""
                    : cards.First(card => valueSelector(card) == value).GlyphImages.GetValueOrDefault(imageKey, ""),
                Count = cards.Count(card => valueSelector(card) == value),
                SortOrder = value,
                FilterKey = filterKey,
                FilterValue = value.ToString()
            })
            .ToList();

        return new TauSystem { Key = key, Name = name, Summary = summary, Instances = instances };
    }

    private static TauSystem BuildIconSystem(IReadOnlyList<TauCard> cards)
    {
        var instances = cards
            .GroupBy(card => card.IconNumber)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var first = group.First();
                return new TauSystemInstance
                {
                    Slug = first.IconNumber.ToString(CultureInfo.InvariantCulture),
                    Name = $"{first.IconNumber:00} {first.IconName}",
                    Value = first.IconNumber.ToString(CultureInfo.InvariantCulture),
                    Description = first.IconDescription,
                    Image = first.GlyphImages.GetValueOrDefault("icon", ""),
                    Count = group.Count(),
                    SortOrder = first.IconNumber,
                    FilterKey = "iconNumber",
                    FilterValue = first.IconNumber.ToString(CultureInfo.InvariantCulture)
                };
            })
            .ToList();

        return new TauSystem
        {
            Key = "icons",
            Name = "Icons",
            Summary = "Icons are 75 paired scene glyphs: two cards share each icon, while avoiding shared rank and suit color.",
            Instances = instances
        };
    }

    private static TauSystem BuildLinkWordSystem(IReadOnlyList<TauCard> cards)
    {
        var cardsByWord = cards
            .Where(card => !string.IsNullOrWhiteSpace(card.Word))
            .GroupBy(card => card.Word.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var instances = cards
            .SelectMany(card => new[] { card.LinkWord1, card.LinkWord2 })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                cardsByWord.TryGetValue(group.Key, out var linkedCard);

                return new TauSystemInstance
                {
                    Slug = Slug.Create(group.Key),
                    Name = NormalizeTitle(group.Key),
                    Value = group.Key,
                    Description = "",
                    Image = linkedCard?.GlyphImages.GetValueOrDefault("icon", "") ?? "",
                    Count = group.Count(),
                    SortOrder = linkedCard?.Id ?? 0,
                    FilterKey = "linkWord",
                    FilterValue = group.Key
                };
            })
            .ToList();

        return new TauSystem
        {
            Key = "link-words",
            Name = "Link Words",
            Summary = "Link words turn the deck vocabulary into a directed graph of related terms.",
            Instances = instances
        };
    }

    private static int LetterSort(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? 999 : char.ToUpperInvariant(value[0]) - 'A';
    }

    private static int DirectionSort(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "N" or "NORTH" or "UP" => 0,
            "E" or "EAST" or "RIGHT" => 1,
            "S" or "SOUTH" or "DOWN" => 2,
            "W" or "WEST" or "LEFT" => 3,
            _ => 99
        };
    }

    private static int BorderArrangementSort(string value)
    {
        if (value.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        return value.Split('+', StringSplitOptions.RemoveEmptyEntries).Length * 10
            + (value.Contains("T", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            + (value.Contains("R", StringComparison.OrdinalIgnoreCase) ? 2 : 0)
            + (value.Contains("B", StringComparison.OrdinalIgnoreCase) ? 4 : 0)
            + (value.Contains("L", StringComparison.OrdinalIgnoreCase) ? 8 : 0);
    }

    private static int ZodiacSort(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 999;
        }

        var order = new[]
        {
            "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
            "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces",
            "Sun", "Moon", "Mercury", "Venus", "Mars", "Jupiter", "Saturn"
        };

        var index = Array.FindIndex(order, item => item.Equals(value, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? 500 : index;
    }

    private static string SecondaryRankLabel(int rank)
    {
        var secondary = 15 - rank;
        return secondary == 0 ? "0" : $"-{secondary}";
    }

    private static string Text(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
    {
        return headers.TryGetValue(header, out var column)
            ? row.Cell(column).GetFormattedString().Trim()
            : "";
    }

    private static int Int(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
    {
        var text = Text(row, headers, header);
        return int.TryParse(text, out var value) ? value : 0;
    }

    private static bool Bool(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
    {
        var text = Text(row, headers, header);
        return text.Equals("true", StringComparison.OrdinalIgnoreCase)
            || text.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || text == "1";
    }

    private static string WebGlyph(string fileName)
    {
        return string.IsNullOrWhiteSpace(fileName)
            ? ""
            : $"/generated/glyphs/{Path.GetFileNameWithoutExtension(fileName)}.webp";
    }

    private static string RankRailGlyph(int rank)
    {
        return $"/generated/glyphs/rankrail___{rank}.webp";
    }

    private static string NormalizeSphere(string sphere)
    {
        return sphere switch
        {
            "Order/Pattern" => "Order",
            "Air/Motion" => "Air",
            "Water/State" => "Water",
            "Fire/Force" => "Fire",
            "Earth/Form" => "Earth",
            "Chaos/Mystery" => "Chaos",
            _ => sphere
        };
    }

    private static int SphereNumber(string sphere)
    {
        return sphere switch
        {
            "Order/Pattern" => 0,
            "Air/Motion" => 1,
            "Water/State" => 2,
            "Fire/Force" => 3,
            "Earth/Form" => 4,
            "Chaos/Mystery" => 5,
            _ => -1
        };
    }

    private static string BorderArrangement(int top, int right, int bottom, int left)
    {
        var sides = new List<string>(4);
        if (top > 0)
        {
            sides.Add("T");
        }
        if (right > 0)
        {
            sides.Add("R");
        }
        if (bottom > 0)
        {
            sides.Add("B");
        }
        if (left > 0)
        {
            sides.Add("L");
        }

        return sides.Count == 0 ? "None" : string.Join("+", sides);
    }

    private static string LinkWords(string linkWord1, string linkWord2)
    {
        return string.Join("/", new[] { linkWord1, linkWord2 }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string NormalizeTitle(string value)
    {
        value = NormalizeWhitespace(value);
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
    }

    private static string NormalizeSentence(string value)
    {
        value = NormalizeWhitespace(value);
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        value = char.ToUpperInvariant(value[0]) + value[1..];
        return value.EndsWith('.') || value.EndsWith('?') || value.EndsWith('!')
            ? value
            : $"{value}.";
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    private sealed record IconDefinition(string Name, string Description);
}
