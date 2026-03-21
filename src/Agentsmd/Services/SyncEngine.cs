namespace Agentsmd.Services;

public sealed class SyncEngine
{
    private readonly InstalledStateManager _state;
    private readonly string _projectRoot;

    public SyncEngine(InstalledStateManager state, string projectRoot)
    {
        _state = state;
        _projectRoot = projectRoot;
    }

    public SyncResult Sync(SyncOptions options)
    {
        var result = new SyncResult();

        if (options.Copilot)
            SyncAgentsTo(Path.Combine(_projectRoot, ".github", "agents"), "copilot", result);

        if (options.ClaudeCode)
        {
            SyncAgentsTo(Path.Combine(_projectRoot, ".claude", "agents"), "claude", result);
            SyncSkillsTo(Path.Combine(_projectRoot, ".claude", "skills"), result);
        }

        if (options.OpenCode)
            SyncAgentsTo(Path.Combine(_projectRoot, ".opencode", "agents"), "opencode", result);

        return result;
    }

    private void SyncAgentsTo(string targetDir, string tool, SyncResult result)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var agentFile in Directory.GetFiles(_state.AgentsDir, "*.md"))
        {
            var name = Path.GetFileNameWithoutExtension(agentFile);
            var content = File.ReadAllText(agentFile);
            var body = Parsing.FrontMatterParser.ExtractBody(content);

            var targetName = tool == "copilot" ? $"{name}.agent.md" : $"{name}.md";
            var targetPath = Path.Combine(targetDir, targetName);
            var wrapper = GenerateWrapper(name, body, tool);

            File.WriteAllText(targetPath, wrapper);
            result.GeneratedFiles.Add(targetPath);
        }
    }

    private void SyncSkillsTo(string targetDir, SyncResult result)
    {
        if (!Directory.Exists(_state.SkillsDir))
            return;

        foreach (var skillDir in Directory.GetDirectories(_state.SkillsDir))
        {
            var skillName = Path.GetFileName(skillDir);
            var targetSkillDir = Path.Combine(targetDir, skillName);

            CopyDirectory(skillDir, targetSkillDir);
            result.CopiedSkills.Add(skillName);
        }
    }

    private static string GenerateWrapper(string name, string body, string tool)
    {
        var frontMatter = tool switch
        {
            "copilot" => $"""
                ---
                name: {name}
                description: Agent {name}
                ---
                """,
            "claude" => $"# {name}\n",
            "opencode" => $"# {name}\n",
            _ => ""
        };

        return $"{frontMatter}\n{body}";
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);

        foreach (var file in Directory.GetFiles(source))
        {
            var destFile = Path.Combine(target, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            var destDir = Path.Combine(target, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }
}

public sealed class SyncOptions
{
    public bool Copilot { get; init; }
    public bool ClaudeCode { get; init; }
    public bool OpenCode { get; init; }
}

public sealed class SyncResult
{
    public List<string> GeneratedFiles { get; } = [];
    public List<string> CopiedSkills { get; } = [];
}
