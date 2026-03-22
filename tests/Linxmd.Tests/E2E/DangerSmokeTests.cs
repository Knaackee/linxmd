using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;

namespace Linxmd.Tests.E2E;

public class DangerSmokeTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _repoRoot;

    public DangerSmokeTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-danger-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _repoRoot = ResolveRepoRoot();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private (int exitCode, string stdout, string stderr) RunCli(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {Path.Combine(_repoRoot, "src", "Linxmd", "Linxmd.csproj")} -- {args} --project \"{_tempDir}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _repoRoot
        };

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(120_000);
        return (process.ExitCode, stdout, stderr);
    }

    [Fact]
    public void FullLifecycle_TypedIds_Works()
    {
        RunCli("init");

        var (installCode, installOut, _) = RunCli("add workflow:echo-test --yes");
        installCode.Should().Be(0);
        installOut.Should().Contain("Installed workflow 'echo-test'");

        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "echo-test.md")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "echo-test.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "echo-test")).Should().BeTrue();

        var (removeCode, removeOut, _) = RunCli("remove workflow:echo-test --yes");
        removeCode.Should().Be(0);
        removeOut.Should().Contain("Uninstalled workflow 'echo-test'");
    }

    [Fact]
    public void Remove_Name_RemovesAllMatchingTypes()
    {
        RunCli("init");
        RunCli("add agent:echo-test --yes");
        RunCli("add skill:echo-test --yes");

        var (code, stdout, _) = RunCli("remove echo-test --yes");
        code.Should().Be(0);
        stdout.Should().Contain("Uninstalled agent 'echo-test'");
        stdout.Should().Contain("Uninstalled skill 'echo-test'");
    }

    [Fact]
    public void SourcesRegistry_Default_IsUsableForInstall()
    {
        RunCli("init");

        var (code, stdout, _) = RunCli("add agent:echo-test --yes --source default");
        code.Should().Be(0);
        stdout.Should().Contain("Installed agent 'echo-test'");

        var installed = File.ReadAllText(Path.Combine(_tempDir, ".linxmd", "installed.json"));
        installed.Should().Contain("\"sourceId\": \"default\"");
        installed.Should().Contain("\"sourcePath\"");
    }

    [Fact]
    public void Update_HandlesSourceBoundArtifacts()
    {
        RunCli("init");
        RunCli("add agent:test-writer --yes");

        var (code, stdout, _) = RunCli("update --yes");
        code.Should().Be(0);
        stdout.Should().ContainAny("up to date", "Updated");
    }

    [Fact]
    public void Commands_WithoutInit_FailFast()
    {
        var commands = new[]
        {
            "add agent:test-writer --yes",
            "remove agent:test-writer --yes",
            "list",
            "sync",
            "update --yes"
        };

        foreach (var cmd in commands)
        {
            var (_, stdout, stderr) = RunCli(cmd);
            (stdout + stderr).Should().Contain("Not initialized", $"command '{cmd}' should require init");
        }
    }

    [Fact]
    public void ListJson_RemainsValidAfterMultipleOps()
    {
        RunCli("init");
        RunCli("add agent:echo-test --yes");
        RunCli("add skill:debugging --yes");
        RunCli("remove agent:echo-test --yes");

        var (_, stdout, _) = RunCli("list --json");
        var action = () => JsonDocument.Parse(stdout.Trim());
        action.Should().NotThrow();
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
