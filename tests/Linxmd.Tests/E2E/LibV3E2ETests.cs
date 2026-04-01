using System.Text.Json;
using FluentAssertions;

namespace Linxmd.Tests.E2E;

/// <summary>
/// E2E tests against the stable test library fixture under tests/Linxmd.Tests/TestLib.
/// </summary>
public class LibV3E2ETests : LocalLibCliTestBase
{
    public LibV3E2ETests()
        : base("linxmd-coreb-")
    {
    }

    [Fact]
    public void Add_ProductManager_Agent_InstallsSuccessfully_WithResearchDep()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add agent:product-manager --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'product-manager'");
        File.Exists(Path.Combine(TempDir, ".linxmd", "agents", "product-manager.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_SpecWriter_Agent_InstallsSuccessfully_WithContextManagementDep()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add agent:spec-writer --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'spec-writer'");
        File.Exists(Path.Combine(TempDir, ".linxmd", "agents", "spec-writer.md")).Should().BeTrue();
    }

    [Fact]
    public void Add_Pack_Coninue_InstallsAllMemberArtifacts()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add pack:coninue --yes");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("Installed agent 'product-manager'");
        stdout.Should().Contain("Installed agent 'router'");
        stdout.Should().Contain("Installed agent 'spec-writer'");
        stdout.Should().Contain("Installed agent 'planner'");

        var (listCode, listOut, listErr) = RunCli("list --json");
        listCode.Should().Be(0);
        listErr.Should().BeEmpty();

        var doc = JsonDocument.Parse(listOut.Trim());
        var names = doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("product-manager");
        names.Should().Contain("router");
        names.Should().Contain("spec-writer");
        names.Should().Contain("planner");
        names.Should().Contain("graph");
        names.Should().Contain("graph-memory");
        names.Should().NotContain("coninue");
    }

    [Fact]
    public void Remove_GraphMemory_BlockedBy_ProductManager_Agent()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add agent:product-manager --yes");

        var (_, stdout, stderr) = RunCli("remove skill:graph-memory --yes");

        (stdout + stderr).Should().Contain("Cannot uninstall due to active dependencies");
        (stdout + stderr).Should().Contain("agent:product-manager depends on skill:graph-memory");
    }

    [Fact]
    public void Add_Browse_ShowsCurrentBootstrapArtifacts()
    {
        RunCli("init");
        UseLocalSource();

        var (code, stdout, stderr) = RunCli("add");

        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Should().Contain("product-manager");
        stdout.Should().Contain("spec-writer");
        stdout.Should().Contain("graph");
        stdout.Should().Contain("graph-memory");
        stdout.Should().Contain("coninue");
    }

    [Fact]
    public void ListJson_ContainsInstalledBootstrapArtifacts()
    {
        RunCli("init");
        UseLocalSource();
        RunCli("add pack:coninue --yes");

        var (code, stdout, stderr) = RunCli("list --json");
        code.Should().Be(0);
        stderr.Should().BeEmpty();

        var doc = JsonDocument.Parse(stdout.Trim());
        var names = doc.RootElement.GetProperty("artifacts")
            .EnumerateArray()
            .Select(a => a.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("product-manager");
        names.Should().Contain("router");
        names.Should().Contain("spec-writer");
        names.Should().Contain("planner");
        names.Should().Contain("graph");
        names.Should().Contain("graph-memory");
    }

}