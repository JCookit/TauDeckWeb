namespace TauWeb.Core.Import;

public sealed record TauImportOptions
{
    public required string WorkbookPath { get; init; }
    public required string AssetsPath { get; init; }
    public required string OutputWebRootPath { get; init; }
    public double BleedCropPercent { get; init; } = 0.055;
    public int? BleedCropPixels { get; init; }
    public string? CardsPath { get; init; }
}
