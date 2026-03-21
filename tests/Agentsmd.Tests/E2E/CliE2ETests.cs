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
