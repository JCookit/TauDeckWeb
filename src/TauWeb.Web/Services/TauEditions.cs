using Microsoft.AspNetCore.WebUtilities;

namespace TauWeb.Web.Services;

public sealed record TauEdition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tag { get; init; } = "";
}

public sealed class TauEditionCatalog
{
    public string Default { get; }
    public IReadOnlyList<TauEdition> Editions { get; }
    private readonly Dictionary<string, HashSet<string>> _systems;

    public TauEditionCatalog(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Default = configuration["Editions:Default"] ?? "v2";
        Editions = configuration.GetSection("Editions:Items").Get<TauEdition[]>() ?? [];
        if (Editions.Count == 0 || Editions.Select(e => e.Id).Distinct().Count() != Editions.Count
            || Editions.Any(e => !System.Text.RegularExpressions.Regex.IsMatch(e.Id, "^v[0-9]+$"))
            || !Editions.Any(e => e.Id == Default))
            throw new InvalidOperationException("Editions must have unique v-number IDs and a valid default.");
        _systems = Editions.ToDictionary(e => e.Id, e =>
        {
            using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(
                Path.Combine(environment.WebRootPath, "editions", e.Id, "content", "tau-data.json")));
            return json.RootElement.GetProperty("systems").EnumerateArray()
                .Select(s => s.GetProperty("key").GetString()!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        });
    }

    public bool HasSystem(string edition, string key) => _systems[edition].Contains(key);
}

public sealed class TauEditionContext(TauEditionCatalog catalog, IHttpContextAccessor accessor)
{
    public bool IsGlobal => accessor.HttpContext?.Request.Path.StartsWithSegments("/games") == true
        || accessor.HttpContext?.Request.Path.StartsWithSegments("/articles") == true;
    public string Id => IsGlobal ? catalog.Default : accessor.HttpContext?.Request.Query["edition"].FirstOrDefault() ?? catalog.Default;
    public TauEdition Current => catalog.Editions.First(e => e.Id == Id);
    public IReadOnlyList<TauEdition> Editions => catalog.Editions;
    public string Asset(string path) => path.StartsWith("/generated/", StringComparison.Ordinal)
        ? $"/editions/{Id}{path}" : path;

    public string Url(string url, string? edition = null)
    {
        var hashIndex = url.IndexOf('#');
        var fragment = hashIndex < 0 ? "" : url[hashIndex..];
        var bare = hashIndex < 0 ? url : url[..hashIndex];
        var queryIndex = bare.IndexOf('?');
        var path = queryIndex < 0 ? bare : bare[..queryIndex];
        if (!(path == "/" || path.Equals("/cards", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/cards/", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/systems", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/systems/", StringComparison.OrdinalIgnoreCase))) return url;
        var query = QueryHelpers.ParseQuery(queryIndex < 0 ? "" : bare[queryIndex..]);
        query["edition"] = edition ?? (query.TryGetValue("edition", out var existing) ? existing.ToString() : Id);
        return QueryHelpers.AddQueryString(path, query) + fragment;
    }

    public string SwitchUrl(string edition)
    {
        var request = accessor.HttpContext!.Request;
        // Newer systems have no equivalent in older editions; switch to the catalog.
        var path = request.Path.Value ?? "/";
        if (path.StartsWith("/systems/", StringComparison.OrdinalIgnoreCase) && !catalog.HasSystem(edition, path["/systems/".Length..])) path = "/systems";
        var query = QueryHelpers.ParseQuery(request.QueryString.Value);
        if (!catalog.HasSystem(edition, "icon-halves")) query.Remove("iconHalf");
        if (!catalog.HasSystem(edition, "standard-deck")) query.Remove("isStandardDeck");
        return Url(QueryHelpers.AddQueryString(path, query), edition);
    }
}
