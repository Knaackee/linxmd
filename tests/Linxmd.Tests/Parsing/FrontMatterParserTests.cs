using Linxmd.Models;
using Linxmd.Parsing;
using FluentAssertions;
using System.IO;
using System.Linq;

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

        [Fact]
        public void Parse_QuickActions_FileMatchAsList_ParsesAllFields()
        {
                var markdown = "---\n"
                        + "name: e2e-testing\n"
                        + "type: skill\n"
                        + "version: 2.1.0\n"
                        + "quickActions:\n"
                        + "  - id: write-e2e\n"
                        + "    icon: \"\ud83e\uddea\"\n"
                        + "    label: Generate E2E test\n"
                        + "    prompt: Write an e2e test for the current page.\n"
                        + "    trigger:\n"
                        + "      fileMatch: [src/pages/.*, src/routes/.*]\n"
                        + "      fileExclude: [spec/.*]\n"
                        + "      workspaceHas: [playwright.config.ts]\n"
                        + "      workspaceMissing: [PROJECT.md]\n"
                        + "      languageId: [typescript, typescriptreact]\n"
                        + "      contentMatch: [export default function]\n"
                        + "---\n\n"
                        + "# E2E Testing Skill\n";

                var result = FrontMatterParser.Parse(markdown);

                result.Should().NotBeNull();
                result!.QuickActions.Should().HaveCount(1);
                var quickAction = result.QuickActions[0];
                quickAction.Id.Should().Be("write-e2e");
                quickAction.Icon.Should().Be("\ud83e\uddea");
                quickAction.Label.Should().Be("Generate E2E test");
                quickAction.Prompt.Should().Be("Write an e2e test for the current page.");
                quickAction.Trigger.FileMatch.Should().BeEquivalentTo(["src/pages/.*", "src/routes/.*"]);
                quickAction.Trigger.FileExclude.Should().BeEquivalentTo(["spec/.*"]);
                quickAction.Trigger.WorkspaceHas.Should().BeEquivalentTo(["playwright.config.ts"]);
                quickAction.Trigger.WorkspaceMissing.Should().BeEquivalentTo(["PROJECT.md"]);
                quickAction.Trigger.LanguageId.Should().BeEquivalentTo(["typescript", "typescriptreact"]);
                quickAction.Trigger.ContentMatch.Should().BeEquivalentTo(["export default function"]);
        }

        [Fact]
        public void Parse_LifecycleHooks_ParsesAllLifecycleEvents()
        {
                var markdown = "---\n"
                        + "name: feature-development\n"
                        + "type: workflow\n"
                        + "version: 2.1.0\n"
                        + "lifecycle:\n"
                        + "  preInstall:\n"
                        + "    - id: check-prereq\n"
                        + "      label: Check prerequisites\n"
                        + "      prompt: Validate required tooling.\n"
                        + "      blocking: true\n"
                        + "      requiresConfirmation: true\n"
                        + "  postInstall:\n"
                        + "    - id: kickoff\n"
                        + "      label: Kickoff workflow\n"
                        + "      prompt: Start onboarding workflow.\n"
                        + "  preUninstall:\n"
                        + "    - id: deps-check\n"
                        + "      label: Check dependencies\n"
                        + "      prompt: Verify dependent artifacts.\n"
                        + "  postUninstall:\n"
                        + "    - id: cleanup\n"
                        + "      label: Cleanup hints\n"
                        + "      prompt: Suggest cleanup steps.\n"
                        + "  preUpdate:\n"
                        + "    - id: backup\n"
                        + "      label: Backup\n"
                        + "      prompt: Create a backup.\n"
                        + "  postUpdate:\n"
                        + "    - id: changelog\n"
                        + "      label: Read changelog\n"
                        + "      prompt: Summarize update changes.\n"
                        + "---\n\n"
                        + "# Feature Development Workflow\n";

                var result = FrontMatterParser.Parse(markdown);

                result.Should().NotBeNull();
                result!.Lifecycle.PreInstall.Should().HaveCount(1);
                result.Lifecycle.PostInstall.Should().HaveCount(1);
                result.Lifecycle.PreUninstall.Should().HaveCount(1);
                result.Lifecycle.PostUninstall.Should().HaveCount(1);
                result.Lifecycle.PreUpdate.Should().HaveCount(1);
                result.Lifecycle.PostUpdate.Should().HaveCount(1);

                var preInstall = result.Lifecycle.PreInstall[0];
                preInstall.Id.Should().Be("check-prereq");
                preInstall.Blocking.Should().BeTrue();
                preInstall.RequiresConfirmation.Should().BeTrue();
        }

            [Fact]
            public void Parse_QuickAction_MissingFileMatch_ReturnsNull()
            {
                var markdown = "---\n"
                    + "name: invalid-skill\n"
                    + "type: skill\n"
                    + "version: 2.1.0\n"
                    + "quickActions:\n"
                    + "  - id: invalid\n"
                    + "    label: Missing fileMatch\n"
                    + "    prompt: Should fail.\n"
                    + "    trigger:\n"
                    + "      workspaceHas: [playwright.config.ts]\n"
                    + "---\n\n"
                    + "# Invalid Skill\n";

                var result = FrontMatterParser.Parse(markdown);

                result.Should().BeNull();
            }

    [Fact]
    public void Parse_ExtendedLibArtifacts_QuickActionsAreValid()
    {
        var repoRoot = FindRepoRoot();
        var files = new[]
        {
            "lib/workflows/feature-development.md",
            "lib/workflows/bug-fix.md",
            "lib/workflows/project-start.md",
            "lib/workflows/research-spike.md",
            "lib/workflows/quality-baseline.md",
            "lib/workflows/release.md",
            "lib/workflows/consistency-sprint.md",
            "lib/workflows/content-review.md",
            "lib/agents/router.md",
            "lib/agents/spec-writer.md",
            "lib/agents/planner.md",
            "lib/agents/test-writer.md",
            "lib/agents/reviewer-spec.md",
            "lib/agents/reviewer-quality.md",
            "lib/agents/consistency-guardian.md",
            "lib/agents/docs-writer.md",
            "lib/agents/changelog-writer.md",
            "lib/agents/architect.md",
            "lib/agents/implementer.md",
            "lib/skills/task-management/SKILL.md",
            "lib/skills/debugging/SKILL.md",
            "lib/skills/refactoring/SKILL.md",
            "lib/skills/api-design/SKILL.md",
            "lib/skills/project-memory/SKILL.md",
            "lib/skills/consistency-check/SKILL.md",
            "lib/skills/conventional-commits/SKILL.md"
        };

        foreach (var relativePath in files)
        {
            var fullPath = Path.Combine(repoRoot, relativePath);
            var markdown = File.ReadAllText(fullPath);

            var result = FrontMatterParser.Parse(markdown);

            result.Should().NotBeNull($"{relativePath} should parse as valid frontmatter");
            result!.QuickActions.Should().NotBeEmpty($"{relativePath} should expose quickActions");
            result.QuickActions.Should().OnlyContain(action => action.Trigger.FileMatch.Count > 0,
                $"{relativePath} quickActions must have non-empty fileMatch");

            var actionIds = result.QuickActions
                .Where(action => !string.IsNullOrWhiteSpace(action.Id))
                .Select(action => action.Id);
            actionIds.Should().OnlyHaveUniqueItems($"{relativePath} quickAction ids should be unique");
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Linxmd.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root from test runtime directory.");
    }
}
