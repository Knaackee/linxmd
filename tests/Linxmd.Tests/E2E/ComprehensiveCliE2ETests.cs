using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;

namespace Linxmd.Tests.E2E;

public class ComprehensiveCliE2ETests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _repoRoot;

    public ComprehensiveCliE2ETests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-e2e-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _repoRoot = ResolveRepoRoot();
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
            Arguments = $"run --project {Path.Combine(_repoRoot, "src", "Linxmd", "Linxmd.csproj")} -- {args} {projectArg}".Trim(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _repoRoot
        };

        psi.Environment["NO_COLOR"] = "1";

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(120_000);
        return (process.ExitCode, stdout, stderr);
    }

    [Fact]
    public void FullLifecycle_EndToEnd_Run_CoversAllMainCommands()
    {
        var (initCode, initOut, initErr) = RunCli("init");
        initCode.Should().Be(0);
        initErr.Should().BeEmpty();
        initOut.Should().Contain("Created .linxmd/ structure");
        initOut.Should().Contain("Created base skill 'linxmd-self-bootstrap'");
        initOut.Should().Contain("Base onboarding prompt:");

        Directory.Exists(Path.Combine(_tempDir, ".linxmd")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "sources.json")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "linxmd-self-bootstrap", "SKILL.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".claude", "skills", "linxmd-self-bootstrap", "SKILL.md")).Should().BeTrue();

        var (_, promptOut, promptErr) = RunCli("init-prompt");
        promptErr.Should().BeEmpty();
        promptOut.Should().Contain("Base onboarding prompt:");
        promptOut.Should().Contain("linxmd status");
        promptOut.Should().Contain("linxmd list");
        promptOut.Should().Contain("linxmd add --help");

        var (_, statusOut, statusErr) = RunCli("status");
        statusErr.Should().BeEmpty();
        statusOut.Should().Contain("linxmd status");

        var (_, addOut, addErr) = RunCli("add workflow:echo-test --yes");
        addErr.Should().BeEmpty();
        addOut.Should().Contain("Installed workflow 'echo-test'");
        addOut.Should().Contain("Installed agent 'echo-test'");
        addOut.Should().Contain("Installed skill 'echo-test'");
        addOut.Should().Contain("Synced:");

        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "echo-test.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "echo-test.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "echo-test")).Should().BeTrue();

        var (_, listOut, listErr) = RunCli("list");
        listErr.Should().BeEmpty();
        listOut.Should().Contain("workflow");
        listOut.Should().Contain("agent");
        listOut.Should().Contain("skill");
        listOut.Should().Contain("echo-test");

        var (_, jsonOut, jsonErr) = RunCli("list --json");
        jsonErr.Should().BeEmpty();
        var doc = JsonDocument.Parse(jsonOut.Trim());
        doc.RootElement.GetProperty("artifacts").GetArrayLength().Should().BeGreaterThan(1);

        var (_, syncOut, syncErr) = RunCli("sync");
        syncErr.Should().BeEmpty();
        syncOut.Should().Contain("Synced:");

        var (_, updateOut, updateErr) = RunCli("update --yes");
        updateErr.Should().BeEmpty();
        updateOut.Should().ContainAny("All artifacts are up to date.", "Updated");

        var (_, removeOut, removeErr) = RunCli("remove workflow:echo-test --yes");
        removeErr.Should().BeEmpty();
        removeOut.Should().Contain("Uninstalled workflow 'echo-test'");

        var (_, listAfterOut, listAfterErr) = RunCli("list");
        listAfterErr.Should().BeEmpty();
        listAfterOut.Should().Contain("echo-test");

        var (_, removeRemainingOut, removeRemainingErr) = RunCli("remove echo-test --yes");
        removeRemainingErr.Should().BeEmpty();
        removeRemainingOut.Should().Contain("Uninstalled agent 'echo-test'");
        removeRemainingOut.Should().Contain("Uninstalled skill 'echo-test'");

        var (_, finalListOut, finalListErr) = RunCli("list");
        finalListErr.Should().BeEmpty();
        finalListOut.Should().Contain("No artifacts installed");
    }

    [Fact]
    public void Help_ShowsAllUnifiedCommands()
    {
        var (code, stdout, stderr) = RunCli("--help", includeProject: false);

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("add");
        stdout.Should().Contain("remove");
        stdout.Should().Contain("list");
        stdout.Should().Contain("sync");
        stdout.Should().Contain("status");
        stdout.Should().Contain("init");
        stdout.Should().Contain("init-prompt");
        stdout.Should().Contain("update");
        stdout.Should().Contain("platform");
    }

    [Fact]
    public void CommandHelp_AllMainCommands_Available()
    {
        var commands = new[]
        {
            "init --help",
            "init-prompt --help",
            "add --help",
            "remove --help",
            "list --help",
            "sync --help",
            "status --help",
            "update --help",
            "platform --help"
        };

        foreach (var command in commands)
        {
            var (code, stdout, stderr) = RunCli(command, includeProject: false);
            code.Should().Be(0, $"help command '{command}' should work");
            stderr.Should().BeEmpty();
            stdout.Should().Contain("Usage:");
        }
    }

    [Fact]
    public void Commands_WithoutInit_FailFast_ExceptStatus()
    {
        var commands = new[]
        {
            "add agent:test-writer --yes",
            "remove agent:test-writer --yes",
            "list",
            "sync",
            "update --yes"
        };

        foreach (var command in commands)
        {
            var (_, stdout, stderr) = RunCli(command);
            (stdout + stderr).Should().Contain("Not initialized", $"'{command}' should require init");
        }

        var (_, statusOut, statusErr) = RunCli("status");
        statusErr.Should().BeEmpty();
        statusOut.Should().Contain("linxmd: not initialized");
        statusOut.Should().Contain("Run 'linxmd init' to set up.");
    }

    [Fact]
    public void Init_IsIdempotent_AndCanStillPrintPrompt()
    {
        RunCli("init");

        var (_, stdout, stderr) = RunCli("init --copy-prompt");
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Already initialized.");
        stdout.Should().Contain("Base onboarding prompt:");
    }

    [Fact]
    public void Add_WithoutQuery_ShowsBrowseResults_AfterInit()
    {
        RunCli("init");

        var (code, stdout, stderr) = RunCli("add");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("test-writer");
        stdout.Should().Contain("sdd-tdd");
    }

    [Fact]
    public void Add_Install_WithoutQuery_ShowsError()
    {
        RunCli("init");

        var (_, stdout, stderr) = RunCli("add --install");
        (stdout + stderr).Should().Contain("--install requires a query");
    }

    [Fact]
    public void Add_AmbiguousName_ShowsTypedHint()
    {
        RunCli("init");

        var (_, stdout, stderr) = RunCli("add echo-test --yes");
        (stdout + stderr).Should().Contain("Multiple matches");
        (stdout + stderr).Should().Contain("agent:echo-test");
    }

    [Fact]
    public void Add_BestMatch_Install_Works()
    {
        RunCli("init");

        var (code, stdout, stderr) = RunCli("add tdd --install --yes");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed ");
        stdout.Should().Contain("Synced:");
    }

    [Fact]
    public void Remove_NoQuery_NonInteractive_ListsInstalled()
    {
        RunCli("init");
        RunCli("add agent:test-writer --yes");

        var (code, stdout, stderr) = RunCli("remove");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("test-writer");
    }

    [Fact]
    public void Remove_Blocks_WhenDependencyExists()
    {
        RunCli("init");
        RunCli("add workflow:sdd-tdd --yes");

        var (_, stdout, stderr) = RunCli("remove skill:task-management --yes");
        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("workflow:sdd-tdd depends on skill:task-management");
    }

    [Fact]
    public void List_Filter_ByType_AndTypedId_Works()
    {
        RunCli("init");
        RunCli("add agent:test-writer --yes");
        RunCli("add skill:debugging --yes");

        var (_, typeOut, typeErr) = RunCli("list agent");
        typeErr.Should().BeEmpty();
        typeOut.Should().Contain("test-writer");
        typeOut.Should().NotContain("debugging");

        var (_, idOut, idErr) = RunCli("list skill:debugging");
        idErr.Should().BeEmpty();
        idOut.Should().Contain("debugging");
    }

    [Fact]
    public void Platform_WithoutInit_FailsFast()
    {
        var (_, stdout, stderr) = RunCli("platform");
        (stdout + stderr).Should().Contain("Not initialized");
    }

    [Fact]
    public void Platform_NonInteractive_NoPlatforms_ShowsMessage()
    {
        RunCli("init");

        var (code, stdout, stderr) = RunCli("platform");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("No platforms configured");
    }

    [Fact]
    public void Platform_Help_ShowsUsage()
    {
        var (code, stdout, stderr) = RunCli("platform --help", includeProject: false);
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Usage:");
    }

    [Fact]
    public void Help_ShowsPlatformCommand()
    {
        var (code, stdout, stderr) = RunCli("--help", includeProject: false);
        code.Should().Be(0);
        stdout.Should().Contain("platform");
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Linxmd.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root from test runtime directory.");
    }
}
