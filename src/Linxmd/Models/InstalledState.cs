namespace Linxmd.Models;

public sealed class InstalledArtifact
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Version { get; init; }
    public string SourceId { get; init; } = "default";
    public string SourcePath { get; init; } = string.Empty;
    public string? Checksum { get; init; }
    public DateTimeOffset InstalledAt { get; init; }
}

public sealed class InstalledState
{
    public List<InstalledArtifact> Artifacts { get; set; } = [];
    public List<string> Platforms { get; set; } = [];
}
