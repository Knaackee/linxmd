namespace Linxmd.Models;

public sealed class LibIndex
{
    public int Version { get; init; } = 1;
    public DateTimeOffset Generated { get; init; }
    public List<ArtifactEntry> Artifacts { get; init; } = [];
}

public sealed class ArtifactEntry
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Version { get; init; }
    public string Description { get; init; } = string.Empty;
    public required string Path { get; init; }
    public List<string> Deps { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> Supported { get; init; } = [];
    public List<string> Artifacts { get; init; } = [];
}
