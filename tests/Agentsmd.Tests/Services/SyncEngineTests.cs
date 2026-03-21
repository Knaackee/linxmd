using Agentsmd.Services;
using FluentAssertions;

namespace Agentsmd.Tests.Services;

public class SyncEngineTests : IDisposable
{
    private readonly string _tempDir;
    private readonly InstalledStateManager _state;
    private readonly SyncEngine _engine;

    public SyncEngineTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "agentsmd-sync-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _state = new InstalledStateManager(_tempDir);
        _state.EnsureDirectories();
        _engine = new SyncEngine(_state, _tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Sync_Copilot_GeneratesAgentFiles()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\n# Test Writer\nWrite tests.");

        var result = _engine.Sync(new SyncOptions { Copilot = true });

        result.GeneratedFiles.Should().HaveCount(1);
        var target = Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md");
        File.Exists(target).Should().BeTrue();
        var content = File.ReadAllText(target);
        content.Should().Contain("name: test-writer");
        content.Should().Contain("Write tests.");
    }

    [Fact]
    public void Sync_ClaudeCode_GeneratesAgentsAndCopiesSkills()
    {
        WriteAgent("implementer", "---\nname: implementer\ntype: agent\nversion: 1.0.0\n---\n\n# Implementer\nImplement code.");
        WriteSkill("feature", "# Feature Skill\nDevelop features.");

        var result = _engine.Sync(new SyncOptions { ClaudeCode = true });

        // Agent wrapper
        var agentTarget = Path.Combine(_tempDir, ".claude", "agents", "implementer.md");
        File.Exists(agentTarget).Should().BeTrue();

        // Skill copy
        var skillTarget = Path.Combine(_tempDir, ".claude", "skills", "feature", "SKILL.md");
        File.Exists(skillTarget).Should().BeTrue();
        result.CopiedSkills.Should().Contain("feature");
    }

    [Fact]
    public void Sync_OpenCode_GeneratesAgentFiles()
    {
        WriteAgent("reviewer", "---\nname: reviewer\ntype: agent\nversion: 1.0.0\n---\n\n# Reviewer\nReview code.");

        var result = _engine.Sync(new SyncOptions { OpenCode = true });

        var target = Path.Combine(_tempDir, ".opencode", "agents", "reviewer.md");
        File.Exists(target).Should().BeTrue();
    }

    [Fact]
    public void Sync_AllTools_GeneratesForAll()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\nContent.");

        var result = _engine.Sync(new SyncOptions { Copilot = true, ClaudeCode = true, OpenCode = true });

        result.GeneratedFiles.Should().HaveCount(3);
    }

    [Fact]
    public void Sync_NoAgents_NoFilesGenerated()
    {
        var result = _engine.Sync(new SyncOptions { Copilot = true });

        result.GeneratedFiles.Should().BeEmpty();
    }

    [Fact]
    public void Sync_SkillWithSubdirectory_CopiesRecursively()
    {
        var skillDir = Path.Combine(_state.SkillsDir, "feature");
        Directory.CreateDirectory(Path.Combine(skillDir, "scripts"));
        File.WriteAllText(Path.Combine(skillDir, "SKILL.md"), "# Feature");
        File.WriteAllText(Path.Combine(skillDir, "scripts", "scaffold.sh"), "echo hello");

        var result = _engine.Sync(new SyncOptions { ClaudeCode = true });

        var targetScript = Path.Combine(_tempDir, ".claude", "skills", "feature", "scripts", "scaffold.sh");
        File.Exists(targetScript).Should().BeTrue();
        File.ReadAllText(targetScript).Should().Be("echo hello");
    }

    private void WriteAgent(string name, string content)
    {
        File.WriteAllText(Path.Combine(_state.AgentsDir, $"{name}.md"), content);
    }

    private void WriteSkill(string name, string content)
    {
        var dir = Path.Combine(_state.SkillsDir, name);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "SKILL.md"), content);
    }
}
