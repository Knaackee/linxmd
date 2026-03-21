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

    public bool IsInitialized => Directory.Exists(AgentsmdDir);

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(AgentsDir);
        Directory.CreateDirectory(SkillsDir);
        Directory.CreateDirectory(WorkflowsDir);
        Directory.CreateDirectory(BacklogDir);
        Directory.CreateDirectory(InProgressDir);
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

    public void AddArtifact(string name, string type, string version)
    {
        var state = Load();
        state.Artifacts.RemoveAll(a => a.Name == name && a.Type == type);
        state.Artifacts.Add(new InstalledArtifact
        {
            Name = name,
            Type = type,
            Version = version,
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
