namespace TauWeb.Core.Models;

public sealed record TauSiteData
{
    public string GeneratedAtUtc { get; init; } = "";
    public IReadOnlyList<TauCard> Cards { get; init; } = [];
    public IReadOnlyList<TauSystem> Systems { get; init; } = [];
    public IReadOnlyList<TauGame> Games { get; init; } = [];
}

public sealed record TauCard
{
    public int Id { get; init; }
    public string Slug { get; init; } = "";
    public string Sequence { get; init; } = "";
    public string SequenceDigit => Sequence.Length > 0 && char.IsAsciiDigit(Sequence[^1]) ? Sequence[^1..] : "";
    public int Suit { get; init; }
    public int Rank { get; init; }
    public string Name { get; init; } = "";
    public string SuitName { get; init; } = "";
    public string SuitShape => Path.GetFileName(GlyphImages.GetValueOrDefault("rank")) switch
    {
        string name when name.StartsWith("rankicon__square", StringComparison.OrdinalIgnoreCase) => "Hard",
        string name when name.StartsWith("rankicon__circle", StringComparison.OrdinalIgnoreCase) => "Soft",
        _ => ""
    };
    public string Color { get; init; } = "";
    public string SuitTheme { get; init; } = "";
    public string RankTheme { get; init; } = "";
    public bool IsTarot { get; init; }
    public string Word { get; init; } = "";
    public string FirstLetter { get; init; } = "";
    public string Sphere { get; init; } = "";
    public int SphereNumber { get; init; }
    public string SphereFullName { get; init; } = "";
    public string Mode { get; init; } = "";
    public int ModeNumber { get; init; }
    public int IconNumber { get; init; }
    public string IconName { get; init; } = "";
    public string IconDescription { get; init; } = "";
    public int IconGroup { get; init; }
    public int? IconHalf { get; init; }
    public bool? IsStandardDeck { get; init; }
    public int PartitionGroup { get; init; }
    public string Zodiac { get; init; } = "";
    public string SphereZodiac { get; init; } = "";
    public string Direction { get; init; } = "";
    public int LetterScore { get; init; }
    public int BorderCount { get; init; }
    public int SegmentCount { get; init; }
    public int TotalBorderScore { get; init; }
    public string BorderZodiac { get; init; } = "";
    public int BorderTop { get; init; }
    public int BorderRight { get; init; }
    public int BorderBottom { get; init; }
    public int BorderLeft { get; init; }
    public string BorderArrangement { get; init; } = "";
    public string LinkWord1 { get; init; } = "";
    public string LinkWord2 { get; init; } = "";
    public string LinkWords { get; init; } = "";
    public int LinkInDegree { get; init; }
    public string OriginalImage { get; init; } = "";
    public string DisplayImage { get; init; } = "";
    public string ThumbnailImage { get; init; } = "";
    public IReadOnlyDictionary<string, string> GlyphImages { get; init; } = new Dictionary<string, string>();
}

public sealed record TauSystem
{
    public string Key { get; init; } = "";
    public string Name { get; init; } = "";
    public int SortOrder { get; init; }
    public string Summary { get; init; } = "";
    public string WhyExists { get; init; } = "";
    public string ExistingGameMappings { get; init; } = "";
    public string NovelGameUses { get; init; } = "";
    public string UsesInPlay { get; init; } = "";
    public string AuthorsNote { get; init; } = "";
    public string Relationships { get; init; } = "";
    public IReadOnlyList<TauSystemInstance> Instances { get; init; } = [];
    public IReadOnlyList<int> InstanceBreakAfter { get; init; } = [];
    public IReadOnlyList<TauSystemGroup> Groups { get; init; } = [];
}

public sealed record TauSystemInstance
{
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string Value { get; init; } = "";
    public string Description { get; init; } = "";
    public string Image { get; init; } = "";
    public string SecondaryImage { get; init; } = "";
    public int Count { get; init; }
    public int SortOrder { get; init; }
    public string FilterKey { get; init; } = "";
    public string FilterValue { get; init; } = "";
}

public sealed record TauSystemGroup
{
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Image { get; init; } = "";
    public int Count { get; init; }
    public int SortOrder { get; init; }
    public string FilterKey { get; init; } = "";
    public string FilterValue { get; init; } = "";
    public TauFilterExpression Filter { get; init; } = new();
}

public sealed record TauFilterExpression
{
    public string Field { get; init; } = "";
    public string EqualTo { get; init; } = "";
    public IReadOnlyList<string> In { get; init; } = [];
    public string HasBorderSide { get; init; } = "";
}

public sealed record TauGame
{
    // Empty means compatible with all editions; list edition IDs for restricted games.
    public IReadOnlyList<string> CompatibleEditions { get; init; } = [];
    public string Id => Slug;
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string Summary { get; init; } = "";
    public string Description { get; init; } = "";
    public int MinPlayers { get; init; }
    public int MaxPlayers { get; init; }
    public int MinPlayMinutes { get; init; }
    public int MaxPlayMinutes { get; init; }
    public double Complexity { get; init; }
    public string MapStrength { get; init; } = "";
    public string Status { get; init; } = "";
    public string SourceName { get; init; } = "";
    public string SourceUrl { get; init; } = "";
    public string BggLink { get; init; } = "";
    public string RuleSource { get; init; } = "";
    public string AuthorUserId { get; init; } = "";
    public string Visibility { get; init; } = "public";
    public string ModerationStatus { get; init; } = "approved";
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public IReadOnlyList<string> GameTypes { get; init; } = [];
    public IReadOnlyList<string> Components { get; init; } = [];
    public TauGameRules Rules { get; init; } = new();
    public IReadOnlyList<TauGameCardZone> CardZones { get; init; } = [];
    public IReadOnlyList<TauGameCardMapping> CardMappings { get; init; } = [];

    public string PlayerCount => MinPlayers == MaxPlayers ? MinPlayers.ToString() : $"{MinPlayers}-{MaxPlayers}";
    public string PlayTime => MinPlayMinutes == MaxPlayMinutes ? $"{MinPlayMinutes}" : $"{MinPlayMinutes}-{MaxPlayMinutes}";
}

public sealed record TauGameRules
{
    public string Objective { get; init; } = "";
    public string Setup { get; init; } = "";
    public string Gameplay { get; init; } = "";
    public string MapDifferences { get; init; } = "";
}

public sealed record TauGameCardZone
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Keyword { get; init; } = "";
    public string Description { get; init; } = "";
    public IReadOnlyList<string> Components { get; init; } = [];
    public IReadOnlyList<string> Rules { get; init; } = [];
}

public sealed record TauGameCardMapping
{
    public string Id { get; init; } = "";
    public int? TauCardId { get; init; }
    public int? TauSuit { get; init; }
    public int? TauRank { get; init; }
    public string SourceSuit { get; init; } = "";
    public string SourceRank { get; init; } = "";
    public string Label { get; init; } = "";
    public IReadOnlyList<string> ZoneIds { get; init; } = [];
    public string Notes { get; init; } = "";
    public bool IsUsed { get; init; } = true;
}
