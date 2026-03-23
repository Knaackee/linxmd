using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;

namespace Linxmd.Tests.E2E;

/// <summary>
/// E2E tests for lib v0.3.0 artifacts:
///   - 6 new skills: code-translator, text-translator, i18n, design-tokens, project-memory, api-design
///   - 2 new agents: changelog-writer, architect
///   - 2 new workflows: quality-baseline, release
///   - updated workflow: bug-fix (v0.3.0 with test-writer + changelog-writer deps)
///   - 4 packs: fullstack-tdd, content-pipeline, quality-sprint, i18n-ready
///
/// All tests use a local source pointing to lib/ so they are independent of GitHub.
/// </summary>
public class LibV3E2ETests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _repoRoot;
    private readonly string _localLibPath;

    public LibV3E2ETests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-libv3-" + Guid.NewGuid().ToString("N")[..8]);
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

    private void UseLocalSource()
    {
        var sourcesPath = Path.Combine(_tempDir, ".linxmd", "sources.json");
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

    // ─── New skills ─────────────────────────────────────────────────────────────

    [Fact]
    public void Add_CodeTranslator_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:code-translator --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'code-translator'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "code-translator")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "code-translator", "SKILL.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_TextTranslator_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:text-translator --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'text-translator'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "text-translator")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "text-translator", "SKILL.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_I18n_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:i18n --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'i18n'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "i18n")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "i18n", "SKILL.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_DesignTokens_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:design-tokens --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'design-tokens'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "design-tokens")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "design-tokens", "SKILL.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_ProjectMemory_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:project-memory --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'project-memory'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "project-memory")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "project-memory", "SKILL.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_ApiDesign_Skill_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add skill:api-design --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'api-design'");
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "api-design")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "api-design", "SKILL.md")).Should().BeTrue();
    }

    // ─── New agents ─────────────────────────────────────────────────────────────

    [Fact]
    public void Add_ChangelogWriter_Agent_InstallsSuccessfully_WithConventionalCommitsDep()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add agent:changelog-writer --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'changelog-writer'");
        stdout.Should().Contain("Installed skill 'conventional-commits'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "changelog-writer.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "conventional-commits")).Should().BeTrue();
    }

    [Fact]
    public void Add_Architect_Agent_InstallsSuccessfully_WithApiDesignDep()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add agent:architect --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'architect'");
        stdout.Should().Contain("Installed skill 'api-design'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "agents", "architect.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(_tempDir, ".linxmd", "skills", "api-design")).Should().BeTrue();
    }

    // ─── New workflows ──────────────────────────────────────────────────────────

    [Fact]
    public void Add_QualityBaseline_Workflow_InstallsSuccessfully_WithAllDeps()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add workflow:quality-baseline --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed workflow 'quality-baseline'");
        stdout.Should().Contain("Installed agent 'reviewer-quality'");
        stdout.Should().Contain("Installed agent 'consistency-guardian'");
        stdout.Should().Contain("Installed agent 'performance-monitor'");
        stdout.Should().Contain("Installed agent 'fact-checker'");
        stdout.Should().Contain("Installed agent 'docs-writer'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "quality-baseline.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Release_Workflow_InstallsSuccessfully_WithAllDeps()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add workflow:release --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed workflow 'release'");
        stdout.Should().Contain("Installed agent 'changelog-writer'");
        stdout.Should().Contain("Installed agent 'docs-writer'");
        stdout.Should().Contain("Installed agent 'reviewer-quality'");
        stdout.Should().Contain("Installed agent 'performance-monitor'");
        stdout.Should().Contain("Installed agent 'implementer'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "release.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_BugFix_V3_Workflow_IncludesTestWriterAndChangelogDeps()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add workflow:bug-fix --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed workflow 'bug-fix'");
        stdout.Should().Contain("Installed agent 'test-writer'");
        stdout.Should().Contain("Installed agent 'changelog-writer'");
        stdout.Should().Contain("Installed agent 'implementer'");
        File.Exists(Path.Combine(_tempDir, ".linxmd", "workflows", "bug-fix.md")).Should().BeTrue();
    }

    // ─── Packs ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_Pack_FullstackTdd_InstallsAllMemberArtifacts()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add pack:fullstack-tdd --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        // workflow:feature-development and its transitive deps
        stdout.Should().Contain("Installed workflow 'feature-development'");
        // agent:router
        stdout.Should().Contain("Installed agent 'router'");
        // skill:context-management
        stdout.Should().Contain("Installed skill 'context-management'");

        // Pack itself must NOT appear in installed.json
        var (listCode, listOut, _) = RunCli("list --json");
        listCode.Should().Be(0);
        var doc = JsonDocument.Parse(listOut.Trim());
        var names = doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .ToList();
        names.Should().NotContain("fullstack-tdd");
        names.Should().Contain("router");
        names.Should().Contain("feature-development");
    }

    [Fact]
    public void Add_Pack_ContentPipeline_InstallsAllMemberArtifacts()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add pack:content-pipeline --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed workflow 'content-review'");
        // content-pipeline pack: drafter, fact-checker, editor, content-review
        stdout.Should().Contain("Installed agent 'drafter'");
        stdout.Should().Contain("Installed agent 'fact-checker'");
        stdout.Should().Contain("Installed agent 'editor'");

        // Pack must not be in installed list
        var (_, listOut, _) = RunCli("list --json");
        var doc = JsonDocument.Parse(listOut.Trim());
        doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .Should().NotContain("content-pipeline");
    }

    [Fact]
    public void Add_Pack_QualitySprint_InstallsAllMemberArtifacts()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add pack:quality-sprint --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed workflow 'quality-baseline'");
        stdout.Should().Contain("Installed skill 'project-memory'");
        // quality-sprint: quality-baseline + project-memory + router (no changelog-writer)
        stdout.Should().Contain("Installed agent 'router'");

        var (_, listOut, _) = RunCli("list --json");
        var doc = JsonDocument.Parse(listOut.Trim());
        doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .Should().NotContain("quality-sprint");
    }

    [Fact]
    public void Add_Pack_I18nReady_InstallsAllMemberArtifacts()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add pack:i18n-ready --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed skill 'i18n'");
        stdout.Should().Contain("Installed skill 'text-translator'");
        stdout.Should().Contain("Installed skill 'task-management'");

        var (_, listOut, _) = RunCli("list --json");
        var doc = JsonDocument.Parse(listOut.Trim());
        doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .Should().NotContain("i18n-ready");
    }

    [Fact]
    public void Add_Pack_SkipsAlreadyInstalledMembers()
    {
        RunCli("init");
        UseLocalSource();
        // Pre-install one member
        RunCli("add agent:router --yes");

        var (code, stdout, stderr) = RunCli("add pack:fullstack-tdd --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        // router was already installed — should not appear again in the output
        stdout.Should().NotContain("Installed agent 'router'");
        // other members still installed
        stdout.Should().Contain("Installed workflow 'feature-development'");
    }

    // ─── Dependency enforcement: v0.3.0 deps ────────────────────────────────────

    [Fact]
    public void Remove_ConventionalCommits_BlockedBy_ChangelogWriter_Agent()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:changelog-writer --yes");

        var (_, stdout, stderr) = RunCli("remove skill:conventional-commits --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("agent:changelog-writer depends on skill:conventional-commits");
    }

    [Fact]
    public void Remove_ApiDesign_BlockedBy_Architect_Agent()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:architect --yes");

        var (_, stdout, stderr) = RunCli("remove skill:api-design --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("agent:architect depends on skill:api-design");
    }

    [Fact]
    public void Remove_ChangelogWriter_BlockedBy_Release_Workflow()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add workflow:release --yes");

        var (_, stdout, stderr) = RunCli("remove agent:changelog-writer --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("workflow:release depends on agent:changelog-writer");
    }

    // ─── Browse and search ───────────────────────────────────────────────────────

    [Fact]
    public void Add_Browse_ShowsAllNewV3Artifacts()
    {
        RunCli("init");
        UseLocalSource();
        var (code, stdout, stderr) = RunCli("add");

        code.Should().Be(0);
        stderr.Should().BeEmpty();

        // New skills
        stdout.Should().Contain("code-translator");
        stdout.Should().Contain("text-translator");
        stdout.Should().Contain("i18n");
        stdout.Should().Contain("design-tokens");
        stdout.Should().Contain("project-memory");
        stdout.Should().Contain("api-design");

        // New agents
        stdout.Should().Contain("changelog-writer");
        stdout.Should().Contain("architect");

        // New workflows
        stdout.Should().Contain("quality-baseline");
        stdout.Should().Contain("release");

        // Packs
        stdout.Should().Contain("fullstack-tdd");
        stdout.Should().Contain("content-pipeline");
        stdout.Should().Contain("quality-sprint");
        stdout.Should().Contain("i18n-ready");
    }

    // ─── Index JSON: all v0.3.0 artifacts appear ────────────────────────────────

    [Fact]
    public void IndexJson_ContainsAllV3Artifacts()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add skill:code-translator --yes");
        RunCli("add skill:text-translator --yes");
        RunCli("add skill:i18n --yes");
        RunCli("add skill:design-tokens --yes");
        RunCli("add skill:project-memory --yes");
        RunCli("add skill:api-design --yes");
        RunCli("add agent:changelog-writer --yes");
        RunCli("add agent:architect --yes");
        RunCli("add workflow:quality-baseline --yes");
        RunCli("add workflow:release --yes");

        var (code, stdout, stderr) = RunCli("list --json");
        code.Should().Be(0);
        stderr.Should().BeEmpty();

        var doc = JsonDocument.Parse(stdout.Trim());
        var names = doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("code-translator");
        names.Should().Contain("text-translator");
        names.Should().Contain("i18n");
        names.Should().Contain("design-tokens");
        names.Should().Contain("project-memory");
        names.Should().Contain("api-design");
        names.Should().Contain("changelog-writer");
        names.Should().Contain("architect");
        names.Should().Contain("quality-baseline");
        names.Should().Contain("release");
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
