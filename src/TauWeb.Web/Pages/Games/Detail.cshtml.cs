using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Core.Models;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Games;

public sealed class DetailModel(ITauGameRepository games, TauContentStore tauContent, IConfiguration configuration) : PageModel
{
    public TauGame Game { get; private set; } = new();
    public IReadOnlyList<TauSuitHeader> TauSuits { get; private set; } = [];
    public IReadOnlyList<int> TauRanks { get; private set; } = [];
    public IReadOnlyDictionary<string, TauGameCardMapping> MappingByTauCell { get; private set; } =
        new Dictionary<string, TauGameCardMapping>();
    public IReadOnlyDictionary<string, TauGameCardZone> ZoneById { get; private set; } =
        new Dictionary<string, TauGameCardZone>();
    public IReadOnlyList<TauGameCardMapping> SourceMappings { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("Games:ShowLibrary")) return NotFound();

        var game = await games.GetGameAsync(slug, cancellationToken);
        if (game is null)
        {
            return NotFound();
        }

        Game = game;
        var tauData = await tauContent.GetAsync();
        var cardById = tauData.Cards.ToDictionary(card => card.Id);

        TauSuits = tauData.Cards
            .GroupBy(card => card.Suit)
            .Select(group => new TauSuitHeader(group.Key, group.First().SuitName))
            .OrderBy(suit => suit.Number)
            .ToArray();
        TauRanks = tauData.Cards
            .Select(card => card.Rank)
            .Distinct()
            .Order()
            .ToArray();
        MappingByTauCell = game.CardMappings
            .Select(mapping => ResolveTauMapping(mapping, cardById))
            .Where(mapping => mapping.TauSuit is not null && mapping.TauRank is not null)
            .GroupBy(mapping => CellKey(mapping.TauSuit!.Value, mapping.TauRank!.Value), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        ZoneById = game.CardZones.ToDictionary(zone => zone.Id, StringComparer.OrdinalIgnoreCase);
        SourceMappings = game.CardMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.SourceSuit) || !string.IsNullOrWhiteSpace(mapping.SourceRank))
            .ToArray();

        return Page();
    }

    public static string CellKey(int suit, int rank) => $"{suit}|{rank}";

    private static TauGameCardMapping ResolveTauMapping(TauGameCardMapping mapping, IReadOnlyDictionary<int, TauCard> cardById)
    {
        if (mapping.TauCardId is null || !cardById.TryGetValue(mapping.TauCardId.Value, out var card))
        {
            return mapping;
        }

        return mapping with
        {
            TauSuit = card.Suit,
            TauRank = card.Rank
        };
    }

    public sealed record TauSuitHeader(int Number, string Name);
}
