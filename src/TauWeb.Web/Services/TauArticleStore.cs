using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Markdig;

namespace TauWeb.Web.Services;

public sealed record TauArticle(string Slug, string Title, int Order, string Status, bool Visible, string Markdown, string Source)
{
    public string Url => "/articles/" + Slug;
    public bool IsLive => Visible && Status != "pending";
}

/// <summary>Markdown is authoritative. Read on each request so local edits need only a refresh.</summary>
public sealed class TauArticleStore(IWebHostEnvironment environment)
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables().UseAutoIdentifiers().UseTaskLists().UseCustomContainers().DisableHtml().Build();

    public static string Render(string markdown) => Markdig.Markdown.ToHtml(markdown, Pipeline);

    public IReadOnlyList<TauArticle> GetAll()
    {
        var path = Path.Combine(environment.ContentRootPath, "Content", "Articles");
        var articles = Directory.EnumerateFiles(path, "*.md").Select(file => Parse(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file)))
            .Where(article => article.Visible).OrderBy(article => article.Order).ThenBy(article => article.Slug, StringComparer.Ordinal).ToArray();
        if (articles.GroupBy(a => a.Slug, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
            throw new InvalidDataException("Article filenames must have unique slugs.");
        return articles;
    }

    public static TauArticle Parse(string slug, string source)
    {
        if (!Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new InvalidDataException($"Invalid article filename: {slug}");
        var normalized = source.ReplaceLineEndings("\n").TrimStart('\uFEFF');
        var match = Regex.Match(normalized, "\\A---\\n(?<header>.*?)\\n---(?:\\n|$)(?<body>.*)\\z", RegexOptions.Singleline);
        if (!match.Success) throw new InvalidDataException($"Missing article metadata: {slug}");
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in match.Groups["header"].Value.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
            var split = line.IndexOf(':');
            if (split < 1 || !fields.TryAdd(line[..split].Trim(), line[(split + 1)..].Trim()))
                throw new InvalidDataException($"Invalid or duplicate metadata in {slug}: {line}");
        }
        if (fields.Keys.Except(["title", "order", "status", "visible"]).Any())
            throw new InvalidDataException($"Unknown metadata field in {slug}.");
        string Field(string name, string fallback = "") => fields.GetValueOrDefault(name, fallback);
        var title = Field("title");
        // Quoted titles use JSON string escaping, a portable subset of YAML.
        if (title.StartsWith('"')) title = JsonSerializer.Deserialize<string>(title) ?? "";
        if (string.IsNullOrWhiteSpace(title) || !int.TryParse(Field("order"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var order)
            || !bool.TryParse(Field("visible", "true"), out var visible) || Field("status", "draft") is not ("pending" or "draft" or "published"))
            throw new InvalidDataException($"Invalid title, order, visible or status in {slug}.");
        return new(slug, title, order, Field("status", "draft"), visible, match.Groups["body"].Value.Trim(), source);
    }
}
