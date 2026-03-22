using FluentAssertions;

namespace Agentsmd.Tests;

public class CliHelperTests
{
    // ═══════════════════════════════════════════════════════════════
    // FindClosestMatch (Levenshtein-based fuzzy matching)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void FindClosestMatch_ExactMatch_ReturnsIt()
    {
        var result = Cli.FindClosestMatch("test-writer", ["test-writer", "implementer", "docs-writer"]);
        result.Should().Be("test-writer");
    }

    [Fact]
    public void FindClosestMatch_Typo_FindsClosest()
    {
        var result = Cli.FindClosestMatch("test-wrter", ["test-writer", "implementer", "docs-writer"]);
        result.Should().Be("test-writer");
    }

    [Fact]
    public void FindClosestMatch_CaseInsensitive()
    {
        var result = Cli.FindClosestMatch("TEST-WRITER", ["test-writer", "implementer"]);
        result.Should().Be("test-writer");
    }

    [Fact]
    public void FindClosestMatch_NoClose_ReturnsNull()
    {
        var result = Cli.FindClosestMatch("zzzzzzzzzzzzz", ["test-writer", "implementer"]);
        result.Should().BeNull();
    }

    [Fact]
    public void FindClosestMatch_EmptyInput_ReturnsNull()
    {
        var result = Cli.FindClosestMatch("", ["test-writer", "implementer"]);
        result.Should().BeNull();
    }

    [Fact]
    public void FindClosestMatch_EmptyCandidates_ReturnsNull()
    {
        var result = Cli.FindClosestMatch("test-writer", []);
        result.Should().BeNull();
    }

    [Fact]
    public void FindClosestMatch_SingleCharTypo()
    {
        var result = Cli.FindClosestMatch("featur", ["feature", "debugging", "refactoring"]);
        result.Should().Be("feature");
    }

    [Fact]
    public void FindClosestMatch_SwappedChars()
    {
        var result = Cli.FindClosestMatch("featrue", ["feature", "debugging"]);
        result.Should().Be("feature");
    }

    [Theory]
    [InlineData("sdd-tdd", "sdd-tdd")]
    [InlineData("sd-tdd", "sdd-tdd")]
    [InlineData("sdd-td", "sdd-tdd")]
    [InlineData("content-reviw", "content-review")]
    public void FindClosestMatch_Various(string input, string expected)
    {
        var candidates = new[] { "sdd-tdd", "content-review", "echo-test" };
        var result = Cli.FindClosestMatch(input, candidates);
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // TypeIcon
    // ═══════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("agent", "🤖")]
    [InlineData("skill", "📦")]
    [InlineData("workflow", "🔄")]
    [InlineData("Agent", "🤖")]
    [InlineData("SKILL", "📦")]
    [InlineData("unknown", "·")]
    public void TypeIcon_ReturnsCorrectEmoji(string type, string expected)
    {
        Cli.TypeIcon(type).Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // Static property accessors (colors, icons)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ColorProperties_AreNotNull()
    {
        Cli.Primary.Should().NotBeNullOrEmpty();
        Cli.Success.Should().NotBeNullOrEmpty();
        Cli.Warning.Should().NotBeNullOrEmpty();
        Cli.Error.Should().NotBeNullOrEmpty();
        Cli.Muted.Should().NotBeNullOrEmpty();
        Cli.Accent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IconProperties_AreNotNull()
    {
        Cli.AgentIcon.Should().NotBeNullOrEmpty();
        Cli.SkillIcon.Should().NotBeNullOrEmpty();
        Cli.WorkflowIcon.Should().NotBeNullOrEmpty();
        Cli.FolderIcon.Should().NotBeNullOrEmpty();
        Cli.BacklogIcon.Should().NotBeNullOrEmpty();
        Cli.InProgressIcon.Should().NotBeNullOrEmpty();
        Cli.ToolsIcon.Should().NotBeNullOrEmpty();
    }
}
