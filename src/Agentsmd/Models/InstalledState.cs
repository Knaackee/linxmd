namespace Agentsmd.Models;

public sealed class InstalledArtifact
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Version { get; init; }
    public DateTimeOffset InstalledAt { get; init; }
}

public sealed class InstalledState
{
    public List<InstalledArtifact> Artifacts { get; set; } = [];
}
