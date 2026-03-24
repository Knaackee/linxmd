using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;

namespace Linxmd.Tests.E2E;

/// <summary>
/// E2E tests for lib v0.2.0 artifacts: new agents (router, planner),
/// new workflow (bug-fix), new skills (context-management, observability),
/// and version-bumped existing artifacts.
///
/// These tests use a local source pointing to the repo's lib/ directory so they
/// are independent of what is currently published on GitHub.
/// </summary>
public class LibV2E2ETests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _repoRoot;
    private readonly string _localLibPath;

    public LibV2E2ETests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-libv2-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _repoRoot = ResolveRepoRoot();
        _localLibPath = Path.Combine(_repoRoot, "lib");
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

    /// <summary>
    /// Overwrites the project's sources.json so that the default source reads from
    /// the local lib/ directory instead of GitHub. This allows tests to install new
    /// artifacts that have not been pushed to GitHub yet.
    /// </summary>
    private void UseLocalSource()
    {
        var sourcesPath = Path.Combine(_tempDir, ".linxmd", "sources.json");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcesPath)!);
        // Escape backslashes for JSON
        var escapedPath = _localLibPath.Replace("\\", "\\\\");
        var json = $$"""
            {
              "sources": [
                {
                  "id": "default",
                  "kind": "local",
                  "localPath": "{{escapedPath}}"
                }
              ]
            }
            """;
        File.WriteAllText(sourcesPath, json);
    }

    // ─── New agents ────────────────────────────────────────────────────────────

    [Fact]
    public void Add_Router_Agent_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add agent:router --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'router'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "router.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Planner_Agent_InstallsSuccessfully_WithTaskManagementDep()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add agent:planner --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'planner'");
        stdout.Should().Contain("Installed skill 'task-management'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "planner.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "task-management")).Should().BeTrue();
    }

    // ─── New workflow ───────────────────────────────────────────────────────────

    [Fact]
    public void Add_BugFix_Workflow_InstallsSuccessfully_WithAllDeps()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add workflow:bug-fix --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed workflow 'bug-fix'");
        stdout.Should().Contain("Installed agent 'implementer'");
        stdout.Should().Contain("Installed agent 'reviewer-quality'");
        stdout.Should().Contain("Installed agent 'docs-writer'");
        stdout.Should().Contain("Installed agent 'changelog-writer'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "bug-fix.md")).Should().BeTrue();
    }

    [Fact]
    public void Remove_BugFix_Workflow_UninstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add workflow:bug-fix --yes");

        var (code, stdout, stderr) = RunCli("remove workflow:bug-fix --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Uninstalled workflow 'bug-fix'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "bug-fix.md")).Should().BeFalse();
    }

    // ─── New skills ─────────────────────────────────────────────────────────────

    [Fact]
    public void Add_ContextManagement_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:context-management --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'context-management'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "context-management")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "context-management", "SKILL.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Observability_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:observability --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'observability'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "observability")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "observability", "SKILL.md")).Should().BeTrue();
    }

    // ─── Version-bumped existing artifacts ─────────────────────────────────────

    [Fact]
    public void Add_FeatureDevelopment_Workflow_IncludesPlannerDep()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add workflow:feature-development --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'planner'");
        stdout.Should().Contain("Installed workflow 'feature-development'");
    }

    [Fact]
    public void Add_Implementer_V2_IncludesDebuggingDep()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add agent:implementer --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'implementer'");
        stdout.Should().Contain("Installed skill 'debugging'");
    }

    // ─── Search and browse discovery ────────────────────────────────────────────

    [Fact]
    public void Add_Browse_ShowsAllNewV2Artifacts()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("router");
        stdout.Should().Contain("planner");
        stdout.Should().Contain("bug-fix");
        stdout.Should().Contain("context-management");
        stdout.Should().Contain("observability");
    }

    [Fact]
    public void Search_Router_FindsByNameAndTag()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add router");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("router");
    }

    [Fact]
    public void Search_BugFix_FindsByNameAndTag()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add bug-fix");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("bug-fix");
    }

    // ─── Dependency enforcement: remove blocked by v0.2 dependents ─────────────

    [Fact]
    public void Remove_Debugging_BlockedBy_Implementer_Agent()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:implementer --yes");

        var (_, stdout, stderr) = RunCli("remove skill:debugging --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("agent:implementer depends on skill:debugging");
    }

    [Fact]
    public void Remove_TaskManagement_BlockedBy_Planner_Agent()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:planner --yes");

        var (_, stdout, stderr) = RunCli("remove skill:task-management --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("agent:planner depends on skill:task-management");
    }

    // ─── Index JSON: all v0.2.0 artifacts appear in list --json ────────────────

    [Fact]
    public void IndexJson_ContainsAllV2Artifacts()
    {
        RunCli("init");
        UseLocalSource();
        // Install a few artifacts that pull in v0.2.0 deps
        RunCli("add agent:router --yes");
        RunCli("add agent:planner --yes");
        RunCli("add workflow:bug-fix --yes");
        RunCli("add skill:context-management --yes");
        RunCli("add skill:observability --yes");

        var (code, stdout, stderr) = RunCli("list --json");
        code.Should().Be(0);
        stderr.Should().BeEmpty();

        var doc = JsonDocument.Parse(stdout.Trim());
        var names = doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("router");
        names.Should().Contain("planner");
        names.Should().Contain("bug-fix");
        names.Should().Contain("context-management");
        names.Should().Contain("observability");
    }

    // ─── Sync: new artifacts are synced to target platforms ────────────────────

    [Fact]
    public void Sync_AfterInstallingV2Artifacts_CompletesSynced()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:router --yes");
        RunCli("add workflow:bug-fix --yes");

        var (code, stdout, stderr) = RunCli("sync");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Synced:");
    }

    // ─── Full lifecycle: install, update, remove for a new v0.2 artifact ───────

    [Fact]
    public void FullLifecycle_Router_InstallUpdateRemove()
    {
        RunCli("init");
        UseLocalSource();

        var (addCode, addOut, addErr) = RunCli("add agent:router --yes");
        addCode.Should().Be(0);
        addErr.Should().BeEmpty();
        addOut.Should().Contain("Installed agent 'router'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "router.md")).Should().BeTrue();

        var (updateCode, updateOut, updateErr) = RunCli("update --yes");
        updateCode.Should().Be(0);
        updateErr.Should().BeEmpty();
        updateOut.Should().ContainAny("All artifacts are up to date.", "Updated");

        var (removeCode, removeOut, removeErr) = RunCli("remove agent:router --yes");
        removeCode.Should().Be(0);
        removeErr.Should().BeEmpty();
        removeOut.Should().Contain("Uninstalled agent 'router'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "router.md")).Should().BeFalse();
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
