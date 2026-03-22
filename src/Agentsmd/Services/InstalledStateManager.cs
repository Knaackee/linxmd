using System.Text.Json;
using Agentsmd.Models;

namespace Agentsmd.Services;

public sealed class InstalledStateManager
{
    private readonly string _projectRoot;

    public InstalledStateManager(string projectRoot)
    {
        _projectRoot = projectRoot;
    }

    public string AgentsmdDir => Path.Combine(_projectRoot, ".agentsmd");
    public string AgentsDir => Path.Combine(AgentsmdDir, "agents");
    public string SkillsDir => Path.Combine(AgentsmdDir, "skills");
    public string WorkflowsDir => Path.Combine(AgentsmdDir, "workflows");
    public string TasksDir => Path.Combine(AgentsmdDir, "tasks");
    public string BacklogDir => Path.Combine(TasksDir, "backlog");
    public string InProgressDir => Path.Combine(TasksDir, "in-progress");
    public string InstalledJsonPath => Path.Combine(AgentsmdDir, "installed.json");
    public string SourcesJsonPath => Path.Combine(AgentsmdDir, "sources.json");

    public bool IsInitialized =>
        Directory.Exists(AgentsDir) &&
        Directory.Exists(SkillsDir) &&
        Directory.Exists(WorkflowsDir) &&
        Directory.Exists(TasksDir);

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(AgentsDir);
        Directory.CreateDirectory(SkillsDir);
        Directory.CreateDirectory(WorkflowsDir);
        Directory.CreateDirectory(BacklogDir);
        Directory.CreateDirectory(InProgressDir);
        EnsureDefaultSources();
    }

    public void EnsureDefaultSources()
    {
        Directory.CreateDirectory(AgentsmdDir);
        if (File.Exists(SourcesJsonPath)) return;

        var registry = new SourceRegistry
        {
            Sources =
            [
                new LibSource
                {
                    Id = "default",
                    Kind = "github",
                    Owner = "Knaackee",
                    Repo = "linxmd",
                    Branch = "main",
                    BasePath = "lib"
                }
            ]
        };

        SaveSources(registry);
    }

    public SourceRegistry LoadSources()
    {
        EnsureDefaultSources();

        try
        {
            var json = File.ReadAllText(SourcesJsonPath);
            var parsed = JsonSerializer.Deserialize(json, AppJsonContext.Default.SourceRegistry);
            if (parsed is null || parsed.Sources.Count == 0)
                return new SourceRegistry
                {
                    Sources =
                    [
                        new LibSource
                        {
                            Id = "default",
                            Kind = "github",
                            Owner = "Knaackee",
                            Repo = "linxmd",
                            Branch = "main",
                            BasePath = "lib"
                        }
                    ]
                };
            return parsed;
        }
        catch
        {
            return new SourceRegistry
            {
                Sources =
                [
                    new LibSource
                    {
                        Id = "default",
                        Kind = "github",
                        Owner = "Knaackee",
                        Repo = "linxmd",
                        Branch = "main",
                        BasePath = "lib"
                    }
                ]
            };
        }
    }

    public void SaveSources(SourceRegistry registry)
    {
        var json = JsonSerializer.Serialize(registry, AppJsonContext.Default.SourceRegistry);
        File.WriteAllText(SourcesJsonPath, json);
    }

    public LibSource? GetSource(string id)
    {
        return LoadSources().Sources.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public InstalledState Load()
    {
        if (!File.Exists(InstalledJsonPath))
            return new InstalledState();

        try
        {
            var json = File.ReadAllText(InstalledJsonPath);
            return JsonSerializer.Deserialize(json, AppJsonContext.Default.InstalledState)
                   ?? new InstalledState();
        }
        catch
        {
            return new InstalledState();
        }
    }

    public void Save(InstalledState state)
    {
        var json = JsonSerializer.Serialize(state, AppJsonContext.Default.InstalledState);
        File.WriteAllText(InstalledJsonPath, json);
    }

    public void AddArtifact(string name, string type, string version, string sourceId = "default", string sourcePath = "", string? checksum = null)
    {
        var state = Load();
        state.Artifacts.RemoveAll(a => a.Name == name && a.Type == type);
        state.Artifacts.Add(new InstalledArtifact
        {
            Name = name,
            Type = type,
            Version = version,
            SourceId = sourceId,
            SourcePath = sourcePath,
            Checksum = checksum,
            InstalledAt = DateTimeOffset.UtcNow
        });
        Save(state);
    }

    public void RemoveArtifact(string name, string type)
    {
        var state = Load();
        state.Artifacts.RemoveAll(a => a.Name == name && a.Type == type);
        Save(state);
    }

    public InstalledArtifact? GetArtifact(string name, string type)
    {
        return Load().Artifacts.FirstOrDefault(a => a.Name == name && a.Type == type);
    }

    public string GetArtifactDir(string type) => type.ToLowerInvariant() switch
    {
        "agent" => AgentsDir,
        "skill" => SkillsDir,
        "workflow" => WorkflowsDir,
        _ => throw new ArgumentException($"Unknown type: {type}")
    };
}
