using Agentsmd.Services;
using FluentAssertions;

namespace Agentsmd.Tests.Services;

public class InstalledStateManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly InstalledStateManager _manager;

    public InstalledStateManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "agentsmd-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _manager = new InstalledStateManager(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void EnsureDirectories_CreatesAllFolders()
    {
        _manager.EnsureDirectories();

        Directory.Exists(_manager.AgentsDir).Should().BeTrue();
        Directory.Exists(_manager.SkillsDir).Should().BeTrue();
        Directory.Exists(_manager.WorkflowsDir).Should().BeTrue();
        Directory.Exists(_manager.BacklogDir).Should().BeTrue();
        Directory.Exists(_manager.InProgressDir).Should().BeTrue();
    }

    [Fact]
    public void IsInitialized_FalseBeforeInit()
    {
        _manager.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void IsInitialized_TrueAfterInit()
    {
        _manager.EnsureDirectories();

        _manager.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void Load_NoFile_ReturnsEmptyState()
    {
        _manager.EnsureDirectories();

        var state = _manager.Load();

        state.Artifacts.Should().BeEmpty();
    }

    [Fact]
    public void AddArtifact_PersistsToFile()
    {
        _manager.EnsureDirectories();

        _manager.AddArtifact("test-writer", "agent", "1.2.0");

        File.Exists(_manager.InstalledJsonPath).Should().BeTrue();
        var state = _manager.Load();
        state.Artifacts.Should().HaveCount(1);
        state.Artifacts[0].Name.Should().Be("test-writer");
        state.Artifacts[0].Type.Should().Be("agent");
        state.Artifacts[0].Version.Should().Be("1.2.0");
    }

    [Fact]
    public void AddArtifact_Duplicate_ReplacesExisting()
    {
        _manager.EnsureDirectories();
        _manager.AddArtifact("test-writer", "agent", "1.0.0");
        _manager.AddArtifact("test-writer", "agent", "1.2.0");

        var state = _manager.Load();
        state.Artifacts.Should().HaveCount(1);
        state.Artifacts[0].Version.Should().Be("1.2.0");
    }

    [Fact]
    public void RemoveArtifact_RemovesFromState()
    {
        _manager.EnsureDirectories();
        _manager.AddArtifact("test-writer", "agent", "1.0.0");

        _manager.RemoveArtifact("test-writer", "agent");

        var state = _manager.Load();
        state.Artifacts.Should().BeEmpty();
    }

    [Fact]
    public void GetArtifact_Exists_ReturnsIt()
    {
        _manager.EnsureDirectories();
        _manager.AddArtifact("feature", "skill", "1.0.0");

        var artifact = _manager.GetArtifact("feature", "skill");

        artifact.Should().NotBeNull();
        artifact!.Name.Should().Be("feature");
    }

    [Fact]
    public void GetArtifact_NotExists_ReturnsNull()
    {
        _manager.EnsureDirectories();

        var artifact = _manager.GetArtifact("nonexistent", "agent");

        artifact.Should().BeNull();
    }

    [Fact]
    public void GetArtifactDir_ReturnsCorrectPaths()
    {
        _manager.GetArtifactDir("agent").Should().EndWith("agents");
        _manager.GetArtifactDir("skill").Should().EndWith("skills");
        _manager.GetArtifactDir("workflow").Should().EndWith("workflows");
    }

    [Fact]
    public void GetArtifactDir_Unknown_Throws()
    {
        var act = () => _manager.GetArtifactDir("unknown");
        act.Should().Throw<ArgumentException>();
    }
}
