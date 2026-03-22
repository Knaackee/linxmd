using Linxmd.Services;
using FluentAssertions;

namespace Linxmd.Tests.Services;

public class SyncEngineTests : IDisposable
{
    private readonly string _tempDir;
    private readonly InstalledStateManager _state;
    private readonly SyncEngine _engine;

    public SyncEngineTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-sync-" + Guid.NewGuid().ToString("N")[..8]);
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

    [Fact]
    public void Sync_NoTools_NoOutput()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\nContent.");

        var result = _engine.Sync(new SyncOptions { Copilot = false, ClaudeCode = false, OpenCode = false });

        result.GeneratedFiles.Should().BeEmpty();
        result.CopiedSkills.Should().BeEmpty();
    }

    [Fact]
    public void Sync_MultipleAgents_GeneratesAllWrappers()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\nWrite tests.");
        WriteAgent("implementer", "---\nname: implementer\ntype: agent\nversion: 1.0.0\n---\n\nImplement code.");
        WriteAgent("reviewer", "---\nname: reviewer\ntype: agent\nversion: 1.0.0\n---\n\nReview code.");

        var result = _engine.Sync(new SyncOptions { Copilot = true });

        result.GeneratedFiles.Should().HaveCount(3);
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "implementer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "reviewer.agent.md")).Should().BeTrue();
    }

    [Fact]
    public void Sync_MultipleSkills_CopiesAll()
    {
        WriteSkill("feature", "# Feature Skill");
        WriteSkill("debugging", "# Debugging Skill");

        var result = _engine.Sync(new SyncOptions { ClaudeCode = true });

        result.CopiedSkills.Should().HaveCount(2);
        result.CopiedSkills.Should().Contain("feature");
        result.CopiedSkills.Should().Contain("debugging");
    }

    [Fact]
    public void Sync_CopilotWrapper_HasCorrectFrontMatter()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\n# Test Writer\nWrite tests.");

        _engine.Sync(new SyncOptions { Copilot = true });

        var content = File.ReadAllText(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md"));
        content.Should().Contain("name: test-writer");
        content.Should().Contain("description: Agent test-writer");
        content.Should().Contain("# Test Writer");
        content.Should().Contain("Write tests.");
    }

    [Fact]
    public void Sync_ClaudeWrapper_HasCorrectFormat()
    {
        WriteAgent("implementer", "---\nname: implementer\ntype: agent\nversion: 1.0.0\n---\n\n# Implementer\nImplement code.");

        _engine.Sync(new SyncOptions { ClaudeCode = true });

        var content = File.ReadAllText(Path.Combine(_tempDir, ".claude", "agents", "implementer.md"));
        content.Should().Contain("# implementer");
        content.Should().Contain("# Implementer");
        content.Should().Contain("Implement code.");
    }

    [Fact]
    public void Sync_OpenCodeWrapper_HasCorrectFormat()
    {
        WriteAgent("reviewer", "---\nname: reviewer\ntype: agent\nversion: 1.0.0\n---\n\n# Reviewer\nReview code.");

        _engine.Sync(new SyncOptions { OpenCode = true });

        var content = File.ReadAllText(Path.Combine(_tempDir, ".opencode", "agents", "reviewer.md"));
        content.Should().Contain("# reviewer");
        content.Should().Contain("# Reviewer");
    }

    [Fact]
    public void Sync_RerunOverwritesPreviousOutput()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\nOld content.");
        _engine.Sync(new SyncOptions { Copilot = true });

        // Update agent and sync again
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 2.0.0\n---\n\nNew content.");
        var result = _engine.Sync(new SyncOptions { Copilot = true });

        result.GeneratedFiles.Should().HaveCount(1);
        var content = File.ReadAllText(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md"));
        content.Should().Contain("New content.");
        content.Should().NotContain("Old content.");
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

    [Fact]
    public void FromPlatforms_AllPlatforms_SetsAllTrue()
    {
        var options = SyncOptions.FromPlatforms(["copilot", "claude-code", "opencode"]);

        options.Copilot.Should().BeTrue();
        options.ClaudeCode.Should().BeTrue();
        options.OpenCode.Should().BeTrue();
    }

    [Fact]
    public void FromPlatforms_OnlyCopilot_SetsOnlyCopilot()
    {
        var options = SyncOptions.FromPlatforms(["copilot"]);

        options.Copilot.Should().BeTrue();
        options.ClaudeCode.Should().BeFalse();
        options.OpenCode.Should().BeFalse();
    }

    [Fact]
    public void FromPlatforms_Empty_SetsAllFalse()
    {
        var options = SyncOptions.FromPlatforms([]);

        options.Copilot.Should().BeFalse();
        options.ClaudeCode.Should().BeFalse();
        options.OpenCode.Should().BeFalse();
    }

    [Fact]
    public void FromPlatforms_CaseInsensitive()
    {
        var options = SyncOptions.FromPlatforms(["Copilot", "Claude-Code", "OpenCode"]);

        options.Copilot.Should().BeTrue();
        options.ClaudeCode.Should().BeTrue();
        options.OpenCode.Should().BeTrue();
    }

    [Fact]
    public void Sync_WithPlatformConfig_OnlySyncsSelected()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\nContent.");

        _state.SavePlatforms(["copilot"]);
        var options = SyncOptions.FromPlatforms(_state.GetPlatforms());
        var result = _engine.Sync(options);

        result.GeneratedFiles.Should().HaveCount(1);
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".claude", "agents")).Should().BeFalse();
        Directory.Exists(Path.Combine(_tempDir, ".opencode", "agents")).Should().BeFalse();
    }

    [Fact]
    public void Sync_WithTwoPlatforms_SyncsBoth()
    {
        WriteAgent("test-writer", "---\nname: test-writer\ntype: agent\nversion: 1.0.0\n---\n\nContent.");

        _state.SavePlatforms(["claude-code", "opencode"]);
        var options = SyncOptions.FromPlatforms(_state.GetPlatforms());
        var result = _engine.Sync(options);

        result.GeneratedFiles.Should().HaveCount(2);
        File.Exists(Path.Combine(_tempDir, ".claude", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".opencode", "agents", "test-writer.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".github", "agents")).Should().BeFalse();
    }
}
