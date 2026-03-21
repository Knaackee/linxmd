using Agentsmd.Models;
using Agentsmd.Parsing;
using FluentAssertions;

namespace Agentsmd.Tests.Parsing;

public class IndexParserTests
{
    [Fact]
    public void Parse_ValidIndex_ReturnsAllArtifacts()
    {
        var json = """
            {
              "version": 1,
              "generated": "2026-03-21T12:00:00Z",
              "artifacts": [
                {
                  "name": "test-writer",
                  "type": "agent",
                  "version": "1.2.0",
                  "description": "Schreibt Tests",
                  "path": "agents/test-writer.md",
                  "deps": [],
                  "tags": ["testing", "tdd"]
                },
                {
                  "name": "feature",
                  "type": "skill",
                  "version": "1.0.0",
                  "description": "Feature-Entwicklung",
                  "path": "skills/feature/",
                  "deps": [],
                  "tags": ["feature"]
                }
              ]
            }
            """;

        var index = IndexParser.Parse(json);

        index.Should().NotBeNull();
        index!.Version.Should().Be(1);
        index.Artifacts.Should().HaveCount(2);
        index.Artifacts[0].Name.Should().Be("test-writer");
        index.Artifacts[1].Type.Should().Be("skill");
    }

    [Fact]
    public void Search_ByName_FindsMatch()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "test-writer");

        results.Should().HaveCount(1);
        results[0].Name.Should().Be("test-writer");
    }

    [Fact]
    public void Search_ByTag_FindsMatch()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "tdd");

        results.Should().HaveCount(2); // test-writer + sdd-tdd
    }

    [Fact]
    public void Search_ByDescription_FindsMatch()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "Tests");

        results.Should().Contain(a => a.Name == "test-writer");
    }

    [Fact]
    public void Search_CaseInsensitive()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "TEST-WRITER");

        results.Should().HaveCount(1);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "nonexistent");

        results.Should().BeEmpty();
    }

    [Fact]
    public void Search_EmptyQuery_ReturnsAll()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "");

        results.Should().HaveCount(index.Artifacts.Count);
    }

    [Fact]
    public void Search_ByType_FiltersCorrectly()
    {
        var index = CreateTestIndex();

        var results = IndexParser.Search(index, "", filterType: "agent");

        results.Should().OnlyContain(a => a.Type == "agent");
    }

    [Fact]
    public void Parse_EmptyJson_ReturnsNull()
    {
        var result = IndexParser.Parse("");

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_InvalidJson_ReturnsNull()
    {
        var result = IndexParser.Parse("{invalid}");

        result.Should().BeNull();
    }

    private static LibIndex CreateTestIndex() => new()
    {
        Version = 1,
        Generated = DateTimeOffset.UtcNow,
        Artifacts =
        [
            new ArtifactEntry
            {
                Name = "test-writer",
                Type = "agent",
                Version = "1.2.0",
                Description = "Schreibt Tests aus Spezifikationen",
                Path = "agents/test-writer.md",
                Tags = ["testing", "tdd"]
            },
            new ArtifactEntry
            {
                Name = "implementer",
                Type = "agent",
                Version = "1.0.0",
                Description = "Implementiert Code",
                Path = "agents/implementer.md",
                Tags = ["implementation"]
            },
            new ArtifactEntry
            {
                Name = "feature",
                Type = "skill",
                Version = "1.0.0",
                Description = "Feature-Entwicklung",
                Path = "skills/feature/",
                Tags = ["feature", "sdd"]
            },
            new ArtifactEntry
            {
                Name = "sdd-tdd",
                Type = "workflow",
                Version = "2.0.0",
                Description = "Spec-Driven Development",
                Path = "workflows/sdd-tdd.md",
                Deps = ["agent:test-writer@>=1.0", "skill:feature@>=1.0"],
                Tags = ["development", "tdd"]
            }
        ]
    };
}
