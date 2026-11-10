using TauWeb.Core.Models;

namespace TauWeb.Web.Services;

public interface ITauGameRepository
{
    Task<IReadOnlyList<TauGame>> GetGamesAsync(CancellationToken cancellationToken = default);
    Task<TauGame?> GetGameAsync(string slug, CancellationToken cancellationToken = default);
    Task UpsertGamesAsync(IEnumerable<TauGame> games, CancellationToken cancellationToken = default);
}
