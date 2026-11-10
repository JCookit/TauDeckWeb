using System.Text.Json;
using TauWeb.Core.Models;

namespace TauWeb.Web.Services;

public sealed class TauContentStore(IWebHostEnvironment environment, TauEditionContext edition)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private TauSiteData? _data;
    private long _dataTimestamp;
    private long _textTimestamp;

    public async Task<TauSiteData> GetAsync()
    {
        var contentRoot = Path.Combine(environment.WebRootPath, "editions", edition.Id, "content");
        var dataPath = Path.Combine(contentRoot, "tau-data.json");
        var textPath = Path.Combine(contentRoot, "system-text.json");
        var dataTimestamp = Timestamp(dataPath);
        var textTimestamp = Timestamp(textPath);

        if (_data is not null && _dataTimestamp == dataTimestamp && _textTimestamp == textTimestamp)
        {
            return _data;
        }

        await _lock.WaitAsync();
        try
        {
            dataTimestamp = Timestamp(dataPath);
            textTimestamp = Timestamp(textPath);
            if (_data is not null && _dataTimestamp == dataTimestamp && _textTimestamp == textTimestamp)
            {
                return _data;
            }

            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException($"Missing data for edition {edition.Id}.", dataPath);
            }

            var json = await File.ReadAllTextAsync(dataPath);
            json = json.Replace("/generated/", $"/editions/{edition.Id}/generated/", StringComparison.Ordinal);
            var data = JsonSerializer.Deserialize<TauSiteData>(json, JsonOptions) ?? new TauSiteData();
            var overlay = await LoadOverlayAsync(textPath);
            _data = TauSystemTextOverlayMerger.Apply(data, overlay);
            _dataTimestamp = dataTimestamp;
            _textTimestamp = textTimestamp;
            return _data;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<TauSystemTextOverlay?> LoadOverlayAsync(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var json = (await File.ReadAllTextAsync(path)).Replace("/generated/", $"/editions/{edition.Id}/generated/", StringComparison.Ordinal);
        return JsonSerializer.Deserialize<TauSystemTextOverlay>(json, JsonOptions);
    }

    private static long Timestamp(string path)
    {
        return File.Exists(path) ? File.GetLastWriteTimeUtc(path).Ticks : 0;
    }
}
