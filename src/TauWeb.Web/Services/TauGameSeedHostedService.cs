using System.Text.Json;
using TauWeb.Core.Models;

namespace TauWeb.Web.Services;

public sealed class TauGameSeedHostedService(
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ITauGameRepository repository,
    ILogger<TauGameSeedHostedService> logger) : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = configuration.GetSection("GameStore").Get<TauGameStoreOptions>() ?? new TauGameStoreOptions();
        if (!options.SeedOnStartup || !string.Equals(options.Provider, "Cosmos", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var existing = await repository.GetGamesAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        var path = Path.Combine(environment.ContentRootPath, options.SeedPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            logger.LogWarning("Game seed file was not found at {Path}", path);
            return;
        }

        await using var stream = File.OpenRead(path);
        var games = await JsonSerializer.DeserializeAsync<IReadOnlyList<TauGame>>(stream, JsonOptions, cancellationToken) ?? [];
        await repository.UpsertGamesAsync(games, cancellationToken);
        logger.LogInformation("Seeded {Count} games into Cosmos DB.", games.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
