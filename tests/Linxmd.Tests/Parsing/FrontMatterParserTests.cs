using Linxmd.Models;
using Linxmd.Parsing;
using FluentAssertions;

namespace Linxmd.Tests.Parsing;

public class FrontMatterParserTests
{
    [Fact]
    public void Parse_ValidAgentFrontMatter_ReturnsCorrectModel()
    {
        var markdown = """
            ---
            name: test-writer
            type: agent
            version: 1.2.0
            description: Schreibt Tests aus Spezifikationen
            deps:
              - agent:implementer@>=1.0
              - skill:feature@>=1.0
            tags:
              - testing
              - tdd
            ---

            # Test-Writer Agent
            Some content here.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Name.Should().Be("test-writer");
        result.Type.Should().Be(ArtifactType.Agent);
        result.Version.Should().Be("1.2.0");
        result.Description.Should().Be("Schreibt Tests aus Spezifikationen");
        result.Deps.Should().BeEquivalentTo(["agent:implementer@>=1.0", "skill:feature@>=1.0"]);
        result.Tags.Should().BeEquivalentTo(["testing", "tdd"]);
    }

    [Fact]
    public void Parse_WithSupportedField_ParsesPlatforms()
    {
        var markdown = """
            ---
            name: build-tool
            type: skill
            version: 1.0.0
            description: Build tool skill
            supported:
              - windows
              - linux
            tags:
              - build
            ---

            # Build Tool
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Supported.Should().BeEquivalentTo(["windows", "linux"]);
    }

    [Fact]
    public void Parse_WithoutSupportedField_ReturnsEmptyList()
    {
        var markdown = """
            ---
            name: simple
            type: agent
            version: 1.0.0
            description: No platform restriction
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Supported.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ValidSkillFrontMatter_ParsesTypeCorrectly()
    {
        var markdown = """
            ---
            name: feature
            type: skill
            version: 1.0.0
            description: Feature-Entwicklung mit SDD
            tags:
              - feature
            ---

            # Feature Skill
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Type.Should().Be(ArtifactType.Skill);
    }

    [Fact]
    public void Parse_ValidWorkflowFrontMatter_ParsesTypeCorrectly()
    {
        var markdown = """
            ---
            name: sdd-tdd
            type: workflow
            version: 2.0.0
            description: Spec-Driven Development
            deps:
              - agent:test-writer@>=1.0
            tags:
              - development
            ---

            # SDD+TDD Workflow
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Type.Should().Be(ArtifactType.Workflow);
        result.Deps.Should().Contain("agent:test-writer@>=1.0");
    }

    [Fact]
    public void Parse_NoDeps_ReturnsEmptyList()
    {
        var markdown = """
            ---
            name: simple
            type: agent
            version: 1.0.0
            description: Simple agent
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Deps.Should().BeEmpty();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NoFrontMatter_ReturnsNull()
    {
        var markdown = "# Just a heading\nSome content.";

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        var result = FrontMatterParser.Parse(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_MalformedYaml_ReturnsNull()
    {
        var markdown = """
            ---
            name: [invalid
            type: agent
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_MissingRequiredField_Name_ReturnsNull()
    {
        var markdown = """
            ---
            type: agent
            version: 1.0.0
            description: No name
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_MissingRequiredField_Version_ReturnsNull()
    {
        var markdown = """
            ---
            name: test
            type: agent
            description: No version
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_InvalidType_ReturnsNull()
    {
        var markdown = """
            ---
            name: test
            type: unknown
            version: 1.0.0
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractBody_ReturnsContentAfterFrontMatter()
    {
        var markdown = """
            ---
            name: test
            type: agent
            version: 1.0.0
            ---

            # Agent Content
            This is the body.
            """;

        var body = FrontMatterParser.ExtractBody(markdown);

        body.Should().Contain("# Agent Content");
        body.Should().Contain("This is the body.");
    }

    [Fact]
    public void ExtractBody_NoFrontMatter_ReturnsFullContent()
    {
        var markdown = "# Just content\nNo front matter.";

        var body = FrontMatterParser.ExtractBody(markdown);

        body.Should().Be(markdown);
    }

    [Fact]
    public void Parse_OnlyOpeningDelimiter_ReturnsNull()
    {
        var markdown = "---\nname: test\ntype: agent\nversion: 1.0.0\n";

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_NullInput_ReturnsNull()
    {
        var result = FrontMatterParser.Parse(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WhitespaceOnlyInput_ReturnsNull()
    {
        var result = FrontMatterParser.Parse("   \n  \n  ");

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_MissingType_ReturnsNull()
    {
        var markdown = """
            ---
            name: test
            version: 1.0.0
            description: Missing type field
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyDescription_UsesEmptyString()
    {
        var markdown = """
            ---
            name: minimal
            type: agent
            version: 1.0.0
            ---

            Content.
            """;

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Description.Should().BeEmpty();
    }

    [Fact]
    public void Parse_AllArtifactTypes_HandledCorrectly()
    {
        var types = new[] { ("agent", ArtifactType.Agent), ("skill", ArtifactType.Skill), ("workflow", ArtifactType.Workflow) };

        foreach (var (typeStr, expected) in types)
        {
            var markdown = $"---\nname: test\ntype: {typeStr}\nversion: 1.0.0\n---\nContent.";
            var result = FrontMatterParser.Parse(markdown);
            result.Should().NotBeNull($"type '{typeStr}' should parse");
            result!.Type.Should().Be(expected);
        }
    }

    [Fact]
    public void ExtractBody_EmptyBody_ReturnsEmpty()
    {
        var markdown = "---\nname: test\ntype: agent\nversion: 1.0.0\n---\n";

        var body = FrontMatterParser.ExtractBody(markdown);

        body.Should().BeEmpty();
    }

    [Fact]
    public void Parse_LeadingWhitespace_StillParses()
    {
        var markdown = "   \n---\nname: test\ntype: agent\nversion: 1.0.0\n---\nContent.";

        var result = FrontMatterParser.Parse(markdown);

        result.Should().NotBeNull();
        result!.Name.Should().Be("test");
    }
}
