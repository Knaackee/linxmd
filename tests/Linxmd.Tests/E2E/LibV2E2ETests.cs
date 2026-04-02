using System.Text.Json;
using FluentAssertions;

namespace Linxmd.Tests.E2E;

/// <summary>
/// E2E tests against the stable test library fixture under tests/Linxmd.Tests/TestLib.
/// </summary>
[Collection("CLI E2E")]
public class LibV2E2ETests : LocalLibCliTestBase
{
    public LibV2E2ETests()
        : base("linxmd-corea-")
    {
    }

    [Fact]
    public void Add_Router_Agent_InstallsSuccessfully()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add agent:router --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'router'");
        File.Exists(Path.Combine(TempDir, ".linxmd", "agents", "router.md")).Should().BeTrue();
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
        File.Exists(Path.Combine(TempDir, ".linxmd", "agents", "planner.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Browse_ShowsCurrentCoreArtifacts()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("planner");
        stdout.Should().Contain("product-manager");
        stdout.Should().Contain("router");
        stdout.Should().Contain("spec-writer");
        stdout.Should().Contain("coninue");
        stdout.Should().Contain("graph-memory");
    }

    [Fact]
    public void Search_Router_FindsByName()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add router");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("router");
    }

    [Fact]
    public void Remove_TaskManagement_WhenNotShipped_ReportsNotInstalled()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:planner --yes");

        var (_, stdout, stderr) = RunCli("remove skill:task-management --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("agent:planner depends on skill:task-management");
    }

    [Fact]
    public void ListJson_ContainsInstalledCoreArtifacts()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:router --yes");
        RunCli("add agent:planner --yes");

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
        names.Should().Contain("graph");
        names.Should().Contain("graph-memory");
    }

    [Fact]
    public void FullLifecycle_Router_InstallUpdateRemove()
    {
        RunCli("init");
        UseLocalSource();

        var (addCode, addOut, addErr) = RunCli("add agent:router --yes");
        addCode.Should().Be(0);
        addErr.Should().BeEmpty();
        addOut.Should().Contain("Installed agent 'router'");
        File.Exists(Path.Combine(TempDir, ".linxmd", "agents", "router.md")).Should().BeTrue();

        var (updateCode, updateOut, updateErr) = RunCli("update --yes");
        updateCode.Should().Be(0);
        updateErr.Should().BeEmpty();
        updateOut.Should().ContainAny("All artifacts are up to date.", "Updated");

        var (removeCode, removeOut, removeErr) = RunCli("remove agent:router --yes");
        removeCode.Should().Be(0);
        removeErr.Should().BeEmpty();
        removeOut.Should().Contain("Uninstalled agent 'router'");
        File.Exists(Path.Combine(TempDir, ".linxmd", "agents", "router.md")).Should().BeFalse();
    }
}