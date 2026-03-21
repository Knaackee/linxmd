using System.Diagnostics;
using FluentAssertions;

namespace Agentsmd.Tests.E2E;

public class CliE2ETests : IDisposable
{
    private readonly string _tempDir;

    public CliE2ETests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "agentsmd-e2e-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private (int exitCode, string stdout, string stderr) RunCli(string args, bool includeProject = true)
    {
        var projectArg = includeProject ? $"--project \"{_tempDir}\"" : "";
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {Path.Combine(TestPaths.RepoRoot, "src", "Agentsmd", "Agentsmd.csproj")} -- {args} {projectArg}".Trim(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = TestPaths.RepoRoot
        };

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(60_000);
        return (process.ExitCode, stdout, stderr);
    }

    [Fact]
    public void Help_ShowsUsageInfo()
    {
        var (exitCode, stdout, _) = RunCli("--help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("agentsmd");
        stdout.Should().Contain("agent");
        stdout.Should().Contain("skill");
        stdout.Should().Contain("workflow");
        stdout.Should().Contain("init");
        stdout.Should().Contain("sync");
        stdout.Should().Contain("status");
    }

    [Fact]
    public void Init_CreatesAgentsmdDirectory()
    {
        var (exitCode, stdout, _) = RunCli("init");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Created .agentsmd/ structure");
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "agents")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "workflows")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "tasks", "backlog")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "tasks", "in-progress")).Should().BeTrue();
    }

    [Fact]
    public void Init_AlreadyInitialized_ShowsMessage()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("init");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Already initialized");
    }

    [Fact]
    public void Status_NotInitialized_ShowsMessage()
    {
        var (exitCode, stdout, _) = RunCli("status");

        exitCode.Should().Be(0);
        stdout.Should().Contain("not initialized");
    }

    [Fact]
    public void Status_Initialized_ShowsOverview()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("status");

        exitCode.Should().Be(0);
        stdout.Should().Contain("agentsmd status");
        stdout.Should().Contain("Agents:");
        stdout.Should().Contain("Skills:");
        stdout.Should().Contain("Workflows:");
        stdout.Should().Contain("Backlog:");
    }

    [Fact]
    public void List_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("list");

        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void List_Initialized_NoArtifacts()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("list");

        exitCode.Should().Be(0);
        stdout.Should().Contain("No artifacts installed");
    }

    [Fact]
    public void Sync_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("sync");

        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void AgentHelp_ShowsSubcommands()
    {
        var (exitCode, stdout, _) = RunCli("agent --help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("install");
        stdout.Should().Contain("uninstall");
        stdout.Should().Contain("list");
        stdout.Should().Contain("search");
        stdout.Should().Contain("info");
    }

    [Fact]
    public void SkillHelp_ShowsSubcommands()
    {
        var (exitCode, stdout, _) = RunCli("skill --help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("install");
        stdout.Should().Contain("uninstall");
        stdout.Should().Contain("list");
        stdout.Should().Contain("search");
        stdout.Should().Contain("info");
    }

    [Fact]
    public void WorkflowHelp_ShowsSubcommands()
    {
        var (exitCode, stdout, _) = RunCli("workflow --help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("install");
        stdout.Should().Contain("uninstall");
        stdout.Should().Contain("list");
        stdout.Should().Contain("search");
        stdout.Should().Contain("info");
    }

    [Fact]
    public void Version_ShowsVersion()
    {
        var (exitCode, stdout, _) = RunCli("--version", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Trim().Should().NotBeEmpty();
        stdout.Trim().Should().Contain("0.1.0");
    }

    // ──── Search ────

    [Fact]
    public void Search_NoQuery_ShowsAllArtifacts()
    {
        var (exitCode, stdout, _) = RunCli("search");

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("implementer");
        stdout.Should().Contain("feature");
        stdout.Should().Contain("sdd-tdd");
    }

    [Fact]
    public void Search_WithQuery_FiltersResults()
    {
        var (exitCode, stdout, _) = RunCli("search tdd");

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
    }

    [Fact]
    public void Search_NoMatch_ShowsMessage()
    {
        var (exitCode, stdout, _) = RunCli("search zzz-nonexistent-zzz");

        exitCode.Should().Be(0);
        stdout.Should().Contain("No results found");
    }

    // ──── Type-scoped search ────

    [Fact]
    public void AgentSearch_ReturnsOnlyAgents()
    {
        var (exitCode, stdout, _) = RunCli("agent search");

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().NotContain("sdd-tdd"); // workflow
    }

    [Fact]
    public void SkillSearch_ReturnsOnlySkills()
    {
        var (exitCode, stdout, _) = RunCli("skill search");

        exitCode.Should().Be(0);
        stdout.Should().Contain("feature");
        stdout.Should().NotContain("test-writer"); // agent
    }

    [Fact]
    public void WorkflowSearch_ReturnsOnlyWorkflows()
    {
        var (exitCode, stdout, _) = RunCli("workflow search");

        exitCode.Should().Be(0);
        stdout.Should().Contain("sdd-tdd");
        stdout.Should().NotContain("test-writer"); // agent
    }

    // ──── Info ────

    [Fact]
    public void AgentInfo_ShowsDetails()
    {
        var (exitCode, stdout, _) = RunCli("agent info test-writer");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Name:");
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("Version:");
        stdout.Should().Contain("1.0.0");
        stdout.Should().Contain("Description:");
    }

    [Fact]
    public void AgentInfo_NotFound_ShowsError()
    {
        var (_, _, stderr) = RunCli("agent info nonexistent-agent");

        stderr.Should().Contain("not found");
    }

    [Fact]
    public void WorkflowInfo_ShowsDeps()
    {
        var (exitCode, stdout, _) = RunCli("workflow info sdd-tdd");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Dependencies:");
        stdout.Should().Contain("agent:test-writer");
    }

    // ──── Install + Uninstall lifecycle ────

    [Fact]
    public void AgentInstall_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("agent install test-writer");

        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void AgentInstall_Success()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("agent install test-writer");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed agent 'test-writer'");
        stdout.Should().Contain("Synced:"); // auto-sync
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void AgentInstall_CreatesWrappers()
    {
        RunCli("init");
        RunCli("agent install test-writer");

        // Auto-sync should have created wrappers
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".claude", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".opencode", "agents", "test-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void AgentUninstall_Success()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        var (exitCode, stdout, _) = RunCli("agent uninstall test-writer");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Uninstalled agent 'test-writer'");
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeFalse();
    }

    [Fact]
    public void AgentUninstall_NotInstalled_ShowsError()
    {
        RunCli("init");
        var (_, _, stderr) = RunCli("agent uninstall nonexistent");

        stderr.Should().Contain("not installed");
    }

    [Fact]
    public void AgentInstall_NotFound_ShowsError()
    {
        RunCli("init");
        var (_, _, stderr) = RunCli("agent install zzz-nonexistent-agent");

        stderr.Should().Contain("not found");
    }

    // ──── Skill install ────

    [Fact]
    public void SkillInstall_CreatesFolder()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("skill install debugging");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed skill 'debugging'");
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "debugging")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "debugging", "SKILL.md")).Should().BeTrue();
    }

    // ──── Type list ────

    [Fact]
    public void AgentList_Empty_ShowsMessage()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("agent list");

        exitCode.Should().Be(0);
        stdout.Should().Contain("No agents installed");
    }

    [Fact]
    public void AgentList_WithInstalled_ShowsAgents()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        var (exitCode, stdout, _) = RunCli("agent list");

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
    }

    // ──── Workflow install with dependency resolution ────

    [Fact]
    public void WorkflowInstall_InstallsAllDeps()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("workflow install sdd-tdd");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed workflow 'sdd-tdd'");
        stdout.Should().Contain("Installing dependencies");
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("implementer");
        stdout.Should().Contain("reviewer-spec");
        stdout.Should().Contain("reviewer-quality");
        stdout.Should().Contain("docs-writer");
        stdout.Should().Contain("feature");
        stdout.Should().Contain("task-management");

        // Verify all files exist
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "workflows", "sdd-tdd.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "implementer.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "feature")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "task-management")).Should().BeTrue();
    }

    // ──── Sync after install ────

    [Fact]
    public void Sync_AfterInstall_GeneratesWrappers()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        RunCli("agent install implementer");

        var (exitCode, stdout, _) = RunCli("sync");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Synced:");
        stdout.Should().Contain("wrapper(s)");

        // Copilot
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "implementer.agent.md")).Should().BeTrue();

        // Claude
        File.Exists(Path.Combine(_tempDir, ".claude", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".claude", "agents", "implementer.md")).Should().BeTrue();

        // OpenCode
        File.Exists(Path.Combine(_tempDir, ".opencode", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".opencode", "agents", "implementer.md")).Should().BeTrue();
    }

    // ──── Full lifecycle ────

    [Fact]
    public void FullLifecycle_Init_Install_Sync_Status_List_Uninstall()
    {
        // 1. Init
        var (_, initOut, _) = RunCli("init");
        initOut.Should().Contain("Created .agentsmd/ structure");

        // 2. Install workflow with deps
        var (_, installOut, _) = RunCli("workflow install content-review");
        installOut.Should().Contain("Installed workflow 'content-review'");
        installOut.Should().Contain("task-management"); // dep

        // 3. Status shows counts
        var (_, statusOut, _) = RunCli("status");
        statusOut.Should().Contain("Workflows: 1");
        statusOut.Should().Contain("Skills:    1"); // task-management dep

        // 4. List shows all installed
        var (_, listOut, _) = RunCli("list");
        listOut.Should().Contain("content-review");
        listOut.Should().Contain("task-management");

        // 5. Uninstall workflow
        var (_, uninstallOut, _) = RunCli("workflow uninstall content-review");
        uninstallOut.Should().Contain("Uninstalled workflow 'content-review'");

        // 6. Status reflects change
        var (_, statusOut2, _) = RunCli("status");
        statusOut2.Should().Contain("Workflows: 0");
    }

    // ──── Init shows available workflows ────

    [Fact]
    public void Init_ShowsAvailableWorkflows()
    {
        var (exitCode, stdout, _) = RunCli("init");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Available workflows:");
        stdout.Should().Contain("sdd-tdd");
    }

    // ──── Status shows tools ────

    [Fact]
    public void Status_AfterSync_ShowsTools()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        // auto-sync creates .github/agents, .claude/agents, .opencode/agents

        var (_, stdout, _) = RunCli("status");
        stdout.Should().Contain("Tools: copilot, claude-code, opencode");
    }
}

internal static class TestPaths
{
    // Resolve repo root from test assembly location
    public static string RepoRoot
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Agentsmd.sln")))
                dir = Path.GetDirectoryName(dir);
            return dir ?? throw new InvalidOperationException("Could not find repo root (Agentsmd.sln)");
        }
    }
}
