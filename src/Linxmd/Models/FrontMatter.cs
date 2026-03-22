namespace Linxmd.Models;

public sealed class FrontMatter
{
    public required string Name { get; init; }
    public required ArtifactType Type { get; init; }
    public required string Version { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<string> Deps { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> Supported { get; init; } = [];
}
