using Microsoft.Azure.Cosmos;
using TauWeb.Core.Models;

namespace TauWeb.Web.Services;

public sealed class CosmosTauGameRepository : ITauGameRepository, IAsyncDisposable
{
    private readonly CosmosClient _client;
    private readonly Container _container;

    public CosmosTauGameRepository(IConfiguration configuration)
    {
        var options = configuration.GetSection("GameStore").Get<TauGameStoreOptions>() ?? new TauGameStoreOptions();
        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("GameStore:Endpoint and GameStore:Key are required when GameStore:Provider is Cosmos.");
        }

        _client = new CosmosClient(options.Endpoint, options.Key, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        });
        _container = _client.GetContainer(options.DatabaseName, options.ContainerName);
    }

    public async Task<IReadOnlyList<TauGame>> GetGamesAsync(CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<TauGame>(
            new QueryDefinition("SELECT * FROM c WHERE c.visibility = @visibility ORDER BY c.name")
                .WithParameter("@visibility", "public"));
        var games = new List<TauGame>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            games.AddRange(response);
        }

        return games;
    }

    public async Task<TauGame?> GetGameAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<TauGame>(slug, new PartitionKey(slug), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpsertGamesAsync(IEnumerable<TauGame> games, CancellationToken cancellationToken = default)
    {
        foreach (var game in games)
        {
            await _container.UpsertItemAsync(game, new PartitionKey(game.Slug), cancellationToken: cancellationToken);
        }
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
