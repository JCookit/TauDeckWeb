namespace TauWeb.Web.Services;

public sealed class TauGameStoreOptions
{
    public string Provider { get; init; } = "Json";
    public string SeedPath { get; init; } = "Content/games.seed.json";
    public string Endpoint { get; init; } = "";
    public string Key { get; init; } = "";
    public string DatabaseName { get; init; } = "tauweb";
    public string ContainerName { get; init; } = "games";
    public bool SeedOnStartup { get; init; } = true;
}
