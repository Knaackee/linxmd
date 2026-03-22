namespace Agentsmd.Models;

public sealed class SourceRegistry
{
    public List<LibSource> Sources { get; init; } = [];
}

public sealed class LibSource
{
    public required string Id { get; init; }
    public string Kind { get; init; } = "github";
    public string Owner { get; init; } = "Knaackee";
    public string Repo { get; init; } = "linxmd";
    public string Branch { get; init; } = "main";
    public string BasePath { get; init; } = "lib";
}
