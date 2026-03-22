using System.Diagnostics;
using System.Text.Json;
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

    // ═══════════════════════════════════════════════════════════════
    // SECTION 1: Help & Version
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Help_ShowsNewCommands()
    {
        var (exitCode, stdout, _) = RunCli("--help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("agentsmd");
        stdout.Should().Contain("add");
        stdout.Should().Contain("remove");
        stdout.Should().Contain("list");
        stdout.Should().Contain("update");
        stdout.Should().Contain("init");
        stdout.Should().Contain("sync");
        stdout.Should().Contain("status");
    }

    [Fact]
    public void Help_StillShowsDeprecatedCommands()
    {
        var (exitCode, stdout, _) = RunCli("--help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("agent");
        stdout.Should().Contain("skill");
        stdout.Should().Contain("workflow");
    }

    [Fact]
    public void Help_DoesNotShowSearch()
    {
        var (_, stdout, _) = RunCli("--help");
        // search was removed (not a command by itself — old 'search' is gone)
        // but 'agent search' still exists as deprecated subcommand
        // The root --help should NOT list a standalone 'search' command
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var commandLines = lines.Where(l => l.TrimStart().StartsWith("search ")).ToList();
        commandLines.Should().BeEmpty("standalone 'search' command should be removed");
    }

    [Fact]
    public void Version_ShowsVersion()
    {
        var (exitCode, stdout, _) = RunCli("--version", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Trim().Should().NotBeEmpty();
        stdout.Trim().Should().Contain("0.2.0");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 2: Init
    // ═══════════════════════════════════════════════════════════════

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
    public void Init_ShowsAvailableWorkflows()
    {
        var (exitCode, stdout, _) = RunCli("init");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Available workflows:");
        stdout.Should().Contain("sdd-tdd");
    }

    [Fact]
    public void Init_ShowsNextStepsWithAdd()
    {
        var (exitCode, stdout, _) = RunCli("init");

        exitCode.Should().Be(0);
        stdout.Should().Contain("agentsmd add");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 3: Status
    // ═══════════════════════════════════════════════════════════════

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
    public void Status_AfterInstall_ShowsTools()
    {
        RunCli("init");
        RunCli("agent install test-writer");

        var (_, stdout, _) = RunCli("status");
        stdout.Should().Contain("Tools: copilot, claude-code, opencode");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 4: List (with --type and --json)
    // ═══════════════════════════════════════════════════════════════

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
    public void List_TypeFilter_Agent()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        RunCli("skill install debugging");

        var (exitCode, stdout, _) = RunCli("list --type agent");
        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().NotContain("debugging");
    }

    [Fact]
    public void List_TypeFilter_Skill()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        RunCli("skill install debugging");

        var (exitCode, stdout, _) = RunCli("list --type skill");
        exitCode.Should().Be(0);
        stdout.Should().Contain("debugging");
        stdout.Should().NotContain("test-writer");
    }

    [Fact]
    public void List_TypeFilter_Empty()
    {
        RunCli("init");
        RunCli("agent install test-writer");

        var (exitCode, stdout, _) = RunCli("list --type workflow");
        exitCode.Should().Be(0);
        stdout.Should().Contain("No workflows installed");
    }

    [Fact]
    public void List_Json_ReturnsValidJson()
    {
        RunCli("init");
        RunCli("agent install test-writer");

        var (exitCode, stdout, _) = RunCli("list --json");
        exitCode.Should().Be(0);

        var action = () => JsonDocument.Parse(stdout.Trim());
        action.Should().NotThrow("--json should output valid JSON");

        var doc = JsonDocument.Parse(stdout.Trim());
        doc.RootElement.GetProperty("artifacts").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public void List_Json_TypeFilter()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        RunCli("skill install debugging");

        var (exitCode, stdout, _) = RunCli("list --type agent --json");
        exitCode.Should().Be(0);

        var doc = JsonDocument.Parse(stdout.Trim());
        var artifacts = doc.RootElement.GetProperty("artifacts");
        artifacts.GetArrayLength().Should().Be(1);
        artifacts[0].GetProperty("name").GetString().Should().Be("test-writer");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 5: Add command (non-interactive browse / install)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Add_NoQuery_ShowsAllArtifacts()
    {
        // Non-interactive (redirected stdout) + no --yes → browse mode
        var (exitCode, stdout, _) = RunCli("add", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("implementer");
        stdout.Should().Contain("feature");
        stdout.Should().Contain("sdd-tdd");
    }

    [Fact]
    public void Add_WithQuery_FiltersResults()
    {
        var (exitCode, stdout, _) = RunCli("add tdd", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
    }

    [Fact]
    public void Add_NoMatch_ShowsMessage()
    {
        var (exitCode, stdout, _) = RunCli("add zzz-nonexistent-zzz", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Should().Contain("No results found");
    }

    [Fact]
    public void Add_TypeFilter_Agent()
    {
        var (exitCode, stdout, _) = RunCli("add --type agent", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().NotContain("sdd-tdd"); // workflow
    }

    [Fact]
    public void Add_TypeFilter_Skill()
    {
        var (exitCode, stdout, _) = RunCli("add --type skill", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Should().Contain("feature");
        stdout.Should().NotContain("test-writer"); // agent
    }

    [Fact]
    public void Add_TypeFilter_Workflow()
    {
        var (exitCode, stdout, _) = RunCli("add --type workflow", includeProject: false);

        exitCode.Should().Be(0);
        stdout.Should().Contain("sdd-tdd");
        stdout.Should().NotContain("test-writer"); // agent
    }

    [Fact]
    public void Add_Yes_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("add test-writer --yes");

        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void Add_Yes_InstallsAgent()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("add test-writer --yes");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed agent 'test-writer'");
        stdout.Should().Contain("Synced:");
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Yes_InstallsWorkflowWithDeps()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("add sdd-tdd --yes");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed workflow 'sdd-tdd'");
        stdout.Should().Contain("Installing dependencies");
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("implementer");

        File.Exists(Path.Combine(_tempDir, ".agentsmd", "workflows", "sdd-tdd.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Yes_TypeDisambiguation()
    {
        // echo-test exists as agent, skill, workflow — --type disambiguates
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("add echo-test --yes --type agent");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed agent 'echo-test'");
    }

    [Fact]
    public void Add_Yes_AmbiguousWithoutType_ShowsError()
    {
        // echo-test exists as multiple types — --yes without --type should fail
        RunCli("init");
        var (_, _, stderr) = RunCli("add echo-test --yes");

        stderr.Should().Contain("Multiple artifacts");
        stderr.Should().Contain("--type");
    }

    [Fact]
    public void Add_Yes_NotFound_ShowsError()
    {
        RunCli("init");
        var (_, _, stderr) = RunCli("add zzz-nonexistent-zzz --yes", includeProject: false);

        // No results → stdout says "No results found", not stderr
        var (_, stdout2, _) = RunCli("add zzz-nonexistent-zzz --yes", includeProject: false);
        stdout2.Should().Contain("No results found");
    }

    [Fact]
    public void Add_Yes_CreatesWrappers()
    {
        RunCli("init");
        RunCli("add test-writer --yes");

        File.Exists(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".claude", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".opencode", "agents", "test-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Yes_SkillCreatesFolder()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("add debugging --yes");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed skill 'debugging'");
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "debugging")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "debugging", "SKILL.md")).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 6: Remove command (non-interactive)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Remove_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("remove test-writer --yes");
        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void Remove_YesFlag_UninstallsArtifact()
    {
        RunCli("init");
        RunCli("add test-writer --yes");

        var (exitCode, stdout, _) = RunCli("remove test-writer --yes");
        exitCode.Should().Be(0);
        stdout.Should().Contain("Uninstalled agent 'test-writer'");
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeFalse();
    }

    [Fact]
    public void Remove_NotInstalled_ShowsError()
    {
        RunCli("init");
        RunCli("add test-writer --yes");
        var (_, _, stderr) = RunCli("remove nonexistent --yes");
        stderr.Should().Contain("not installed");
    }

    [Fact]
    public void Remove_NoQuery_NonInteractive_ShowsUsage()
    {
        RunCli("init");
        RunCli("add test-writer --yes");

        var (_, _, stderr) = RunCli("remove");
        stderr.Should().Contain("Specify an artifact name");
    }

    [Fact]
    public void Remove_EmptyProject_ShowsMessage()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("remove test-writer --yes");
        // No artifacts installed
        stdout.Should().Contain("No artifacts installed");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 7: Update command
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Update_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("update --yes");
        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void Update_NoArtifacts_ShowsMessage()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("update --yes");
        exitCode.Should().Be(0);
        stdout.Should().Contain("No artifacts installed");
    }

    [Fact]
    public void Update_AllUpToDate_ShowsMessage()
    {
        RunCli("init");
        RunCli("add test-writer --yes");
        var (exitCode, stdout, _) = RunCli("update --yes");
        exitCode.Should().Be(0);
        stdout.Should().Contain("up to date");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 8: Sync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Sync_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("sync");
        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void Sync_AfterInstall_GeneratesWrappers()
    {
        RunCli("init");
        RunCli("add test-writer --yes");
        RunCli("add implementer --yes");

        var (exitCode, stdout, _) = RunCli("sync");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Synced:");
        stdout.Should().Contain("wrapper(s)");

        File.Exists(Path.Combine(_tempDir, ".github", "agents", "test-writer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".github", "agents", "implementer.agent.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".claude", "agents", "test-writer.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".opencode", "agents", "test-writer.md")).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 9: Deprecated commands (still functional)
    // ═══════════════════════════════════════════════════════════════

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
    }

    [Fact]
    public void WorkflowHelp_ShowsSubcommands()
    {
        var (exitCode, stdout, _) = RunCli("workflow --help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("install");
    }

    [Fact]
    public void DeprecatedAgentInstall_StillWorks()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("agent install test-writer");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed agent 'test-writer'");
        stdout.Should().Contain("Tip:"); // deprecation hint
    }

    [Fact]
    public void DeprecatedAgentUninstall_StillWorks()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        var (exitCode, stdout, _) = RunCli("agent uninstall test-writer");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Uninstalled agent 'test-writer'");
        stdout.Should().Contain("Tip:");
    }

    [Fact]
    public void DeprecatedAgentSearch_StillWorks()
    {
        var (exitCode, stdout, _) = RunCli("agent search");

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("Tip:");
    }

    [Fact]
    public void DeprecatedAgentInfo_StillWorks()
    {
        var (exitCode, stdout, _) = RunCli("agent info test-writer");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Name:");
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("Version:");
        stdout.Should().Contain("1.0.0");
        stdout.Should().Contain("Description:");
        stdout.Should().Contain("Tip:");
    }

    [Fact]
    public void DeprecatedAgentInfo_NotFound_ShowsError()
    {
        var (_, _, stderr) = RunCli("agent info nonexistent-agent");
        stderr.Should().Contain("not found");
    }

    [Fact]
    public void DeprecatedWorkflowInfo_ShowsDeps()
    {
        var (exitCode, stdout, _) = RunCli("workflow info sdd-tdd");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Dependencies:");
        stdout.Should().Contain("agent:test-writer");
    }

    [Fact]
    public void DeprecatedAgentList_WithInstalled()
    {
        RunCli("init");
        RunCli("agent install test-writer");
        var (exitCode, stdout, _) = RunCli("agent list");

        exitCode.Should().Be(0);
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("Tip:");
    }

    [Fact]
    public void DeprecatedSkillInstall_CreatesFolder()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("skill install debugging");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed skill 'debugging'");
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "debugging")).Should().BeTrue();
    }

    [Fact]
    public void DeprecatedWorkflowInstall_InstallsAllDeps()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("workflow install sdd-tdd");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed workflow 'sdd-tdd'");
        stdout.Should().Contain("Installing dependencies");

        File.Exists(Path.Combine(_tempDir, ".agentsmd", "workflows", "sdd-tdd.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".agentsmd", "skills", "feature")).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 10: Full lifecycle with new commands
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void FullLifecycle_NewCommands()
    {
        // 1. Init
        var (_, initOut, _) = RunCli("init");
        initOut.Should().Contain("Created .agentsmd/ structure");

        // 2. Add workflow with deps
        var (_, addOut, _) = RunCli("add content-review --yes");
        addOut.Should().Contain("Installed workflow 'content-review'");
        addOut.Should().Contain("task-management"); // dep

        // 3. Status shows counts
        var (_, statusOut, _) = RunCli("status");
        statusOut.Should().Contain("Workflows: 1");
        statusOut.Should().Contain("Skills:    1"); // task-management dep

        // 4. List shows all installed
        var (_, listOut, _) = RunCli("list");
        listOut.Should().Contain("content-review");
        listOut.Should().Contain("task-management");

        // 5. List with type filter
        var (_, listAgentOut, _) = RunCli("list --type workflow");
        listAgentOut.Should().Contain("content-review");
        listAgentOut.Should().NotContain("task-management");

        // 6. Remove workflow
        var (_, removeOut, _) = RunCli("remove content-review --yes");
        removeOut.Should().Contain("Uninstalled workflow 'content-review'");

        // 7. Status reflects change
        var (_, statusOut2, _) = RunCli("status");
        statusOut2.Should().Contain("Workflows: 0");
    }

    [Fact]
    public void FullLifecycle_DeprecatedCommands()
    {
        // 1. Init
        var (_, initOut, _) = RunCli("init");
        initOut.Should().Contain("Created .agentsmd/ structure");

        // 2. Install via deprecated command
        var (_, installOut, _) = RunCli("workflow install content-review");
        installOut.Should().Contain("Installed workflow 'content-review'");

        // 3. List via new command
        var (_, listOut, _) = RunCli("list");
        listOut.Should().Contain("content-review");

        // 4. Uninstall via deprecated command
        var (_, uninstallOut, _) = RunCli("workflow uninstall content-review");
        uninstallOut.Should().Contain("Uninstalled workflow 'content-review'");

        // 5. Status
        var (_, statusOut, _) = RunCli("status");
        statusOut.Should().Contain("Workflows: 0");
    }

    // ═══════════════════════════════════════════════════════════════
    // SECTION 11: Error cases
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void AgentInstall_NotInitialized_ShowsError()
    {
        var (_, _, stderr) = RunCli("agent install test-writer");
        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void AgentInstall_NotFound_ShowsError()
    {
        RunCli("init");
        var (_, _, stderr) = RunCli("agent install zzz-nonexistent-agent");
        stderr.Should().Contain("not found");
    }

    [Fact]
    public void AgentUninstall_NotInstalled_ShowsError()
    {
        RunCli("init");
        var (_, _, stderr) = RunCli("agent uninstall nonexistent");
        stderr.Should().Contain("not installed");
    }
}

internal static class TestPaths
{
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
