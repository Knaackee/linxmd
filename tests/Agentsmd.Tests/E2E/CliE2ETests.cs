using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;

namespace Agentsmd.Tests.E2E;

public class CliE2ETests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _repoRoot;

    public CliE2ETests()
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
            Arguments = $"run --project {Path.Combine(_repoRoot, "src", "Agentsmd", "Agentsmd.csproj")} -- {args} {projectArg}".Trim(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _repoRoot
        };

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(90_000);
        return (process.ExitCode, stdout, stderr);
    }

    [Fact]
    public void Help_ShowsUnifiedCommands_AndNoDeprecatedTrees()
    {
        var (exitCode, stdout, _) = RunCli("--help");

        exitCode.Should().Be(0);
        stdout.Should().Contain("add");
        stdout.Should().Contain("remove");
        stdout.Should().Contain("list");
        stdout.Should().Contain("sync");
        stdout.Should().Contain("status");
        stdout.Should().Contain("init");
        stdout.Should().Contain("update");
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .ToList();
        lines.Should().NotContain(l => l.StartsWith("agent "));
        lines.Should().NotContain(l => l.StartsWith("skill "));
        lines.Should().NotContain(l => l.StartsWith("workflow "));
    }

    [Fact]
    public void Init_CreatesStructure_AndSourcesRegistry()
    {
        var (exitCode, stdout, _) = RunCli("init");
        exitCode.Should().Be(0);
        stdout.Should().Contain("Created .agentsmd/ structure");

        File.Exists(Path.Combine(_tempDir, ".agentsmd", "sources.json")).Should().BeTrue();
        var sourcesJson = File.ReadAllText(Path.Combine(_tempDir, ".agentsmd", "sources.json"));
        sourcesJson.Should().Contain("default");
        sourcesJson.Should().Contain("linxmd");
    }

    [Fact]
    public void Add_Browse_WorksWithoutInit()
    {
        var (exitCode, stdout, _) = RunCli("add echo-test", includeProject: false);
        exitCode.Should().Be(0);
        stdout.Should().Contain("echo-test");
    }

    [Fact]
    public void Add_Yes_WithoutInit_Fails()
    {
        var (_, _, stderr) = RunCli("add agent:test-writer --yes");
        stderr.Should().Contain("Not initialized");
    }

    [Fact]
    public void Add_TypedId_InstallsAgent()
    {
        RunCli("init");
        var (exitCode, stdout, _) = RunCli("add agent:test-writer --yes");

        exitCode.Should().Be(0);
        stdout.Should().Contain("Installed agent 'test-writer'");
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_AmbiguousWithoutType_ShowsTypedHint()
    {
        RunCli("init");
        var (_, _, stderr) = RunCli("add echo-test --yes");

        stderr.Should().Contain("Multiple matches");
        stderr.Should().Contain("agent:echo-test");
    }

    [Fact]
    public void List_FilterAndJson_Work()
    {
        RunCli("init");
        RunCli("add agent:test-writer --yes");
        RunCli("add skill:debugging --yes");

        var (_, typeOut, _) = RunCli("list agent");
        typeOut.Should().Contain("test-writer");
        typeOut.Should().NotContain("debugging");

        var (_, idOut, _) = RunCli("list skill:debugging");
        idOut.Should().Contain("debugging");

        var (_, jsonOut, _) = RunCli("list --json");
        var doc = JsonDocument.Parse(jsonOut.Trim());
        doc.RootElement.GetProperty("artifacts").GetArrayLength().Should().BeGreaterThan(1);
    }

    [Fact]
    public void Remove_Typed_Uninstalls()
    {
        RunCli("init");
        RunCli("add agent:test-writer --yes");

        var (exitCode, stdout, _) = RunCli("remove agent:test-writer --yes");
        exitCode.Should().Be(0);
        stdout.Should().Contain("Uninstalled agent 'test-writer'");
        File.Exists(Path.Combine(_tempDir, ".agentsmd", "agents", "test-writer.md")).Should().BeFalse();
    }

    [Fact]
    public void Remove_Blocks_WhenDependencyExists()
    {
        RunCli("init");
        RunCli("add workflow:sdd-tdd --yes");

        var (_, _, stderr) = RunCli("remove skill:task-management --yes");
        stderr.Should().Contain("Cannot uninstall due to active dependencies");
        stderr.Should().Contain("workflow:sdd-tdd depends on skill:task-management");
    }

    [Fact]
    public void Update_UsesSourceMetadata()
    {
        RunCli("init");
        RunCli("add agent:echo-test --yes");

        var stateJson = File.ReadAllText(Path.Combine(_tempDir, ".agentsmd", "installed.json"));
        stateJson.Should().Contain("sourceId");
        stateJson.Should().Contain("sourcePath");

        var (exitCode, stdout, _) = RunCli("update --yes");
        exitCode.Should().Be(0);
        stdout.Should().ContainAny("up to date", "Updated");
    }

    [Fact]
    public void RemovedTrees_AreNotAvailable()
    {
        var (_, stdout, _) = RunCli("--help");
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .ToList();
        lines.Should().NotContain(l => l.StartsWith("agent "));
        lines.Should().NotContain(l => l.StartsWith("skill "));
        lines.Should().NotContain(l => l.StartsWith("workflow "));
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Agentsmd.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root from test runtime directory.");
    }
}
