using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TauWeb.Core.Models;
using TauWeb.Web.Services;

namespace TauWeb.Web.Pages.Games;

public sealed class IndexModel(ITauGameRepository games, IConfiguration configuration) : PageModel
{
    public bool ShowLibrary => configuration.GetValue<bool>("Games:ShowLibrary");
    public IReadOnlyList<TauGame> Games { get; private set; } = [];

    [BindProperty(SupportsGet = true, Name = "q")]
    public string Query { get; init; } = "";

    [BindProperty(SupportsGet = true, Name = "players")]
    public int? Players { get; init; }

    [BindProperty(SupportsGet = true, Name = "complexity")]
    public int? Complexity { get; init; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ShowLibrary) return;

        var results = await games.GetGamesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(Query))
        {
            results = results.Where(MatchesQuery).ToArray();
        }

        if (Players is not null)
        {
            results = results.Where(game => game.MinPlayers <= Players && game.MaxPlayers >= Players).ToArray();
        }

        if (Complexity is not null)
        {
            results = results.Where(game => game.Complexity >= Complexity && game.Complexity < Complexity + 1).ToArray();
        }

        Games = results;
    }

    private bool MatchesQuery(TauGame game)
    {
        var haystack = string.Join(" ", new[]
        {
            game.Name,
            game.Summary,
            game.Description,
            game.MapStrength,
            string.Join(" ", game.GameTypes),
            string.Join(" ", game.Components),
            string.Join(" ", game.CardZones.Select(zone => zone.Name))
        });

        return haystack.Contains(Query, StringComparison.OrdinalIgnoreCase);
    }
}
