using System.Text.Json;
using TauWeb.Core.Models;

namespace TauWeb.Web.Services;

public sealed class JsonTauGameRepository(IWebHostEnvironment environment, IConfiguration configuration) : ITauGameRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<TauGame>? _games;
    private long _timestamp;

    public async Task<IReadOnlyList<TauGame>> GetGamesAsync(CancellationToken cancellationToken = default)
    {
        var path = GetPath();
        var timestamp = Timestamp(path);
        if (_games is not null && _timestamp == timestamp)
        {
            return _games;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            timestamp = Timestamp(path);
            if (_games is not null && _timestamp == timestamp)
            {
                return _games;
            }

            if (!File.Exists(path))
            {
                _games = [];
                _timestamp = 0;
                return _games;
            }

            await using var stream = File.OpenRead(path);
            _games = await JsonSerializer.DeserializeAsync<IReadOnlyList<TauGame>>(stream, JsonOptions, cancellationToken) ?? [];
            _timestamp = timestamp;
            return _games;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TauGame?> GetGameAsync(string slug, CancellationToken cancellationToken = default)
    {
        var games = await GetGamesAsync(cancellationToken);
        return games.FirstOrDefault(game => string.Equals(game.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task UpsertGamesAsync(IEnumerable<TauGame> games, CancellationToken cancellationToken = default)
    {
        var path = GetPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? environment.ContentRootPath);

        var existing = (await GetGamesAsync(cancellationToken)).ToDictionary(game => game.Slug, StringComparer.OrdinalIgnoreCase);
        foreach (var game in games)
        {
            existing[game.Slug] = game;
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, existing.Values.OrderBy(game => game.Name).ToArray(), JsonOptions, cancellationToken);
        _games = null;
        _timestamp = 0;
    }

    private string GetPath()
    {
        var options = configuration.GetSection("GameStore").Get<TauGameStoreOptions>() ?? new TauGameStoreOptions();
        return Path.Combine(environment.ContentRootPath, options.SeedPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static long Timestamp(string path)
    {
        return File.Exists(path) ? File.GetLastWriteTimeUtc(path).Ticks : 0;
    }
}
