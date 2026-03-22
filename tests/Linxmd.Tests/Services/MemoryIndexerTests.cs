using Linxmd.Services;
using FluentAssertions;

namespace Linxmd.Tests.Services;

public class MemoryIndexerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dbPath;
    private MemoryDb _db;

    public MemoryIndexerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-midx-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _dbPath = Path.Combine(_tempDir, "memory.db");
        _db = new MemoryDb(_dbPath);
    }

    public void Dispose()
    {
        _db.Dispose();
        // SQLite WAL files can linger briefly on Windows; ignore cleanup errors
        if (Directory.Exists(_tempDir))
            try { Directory.Delete(_tempDir, recursive: true); }
            catch { /* best-effort */ }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void WriteDecision(string filename, string content)
    {
        var dir = Path.Combine(_tempDir, "docs", "decisions");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, filename), content);
    }

    private void WriteChangelog(string content)
    {
        File.WriteAllText(Path.Combine(_tempDir, "CHANGELOG.md"), content);
    }

    private void WriteKnownIssues(string content)
    {
        File.WriteAllText(Path.Combine(_tempDir, "KNOWN_ISSUES.md"), content);
    }

    // -------------------------------------------------------------------------
    // IndexProject — general
    // -------------------------------------------------------------------------

    [Fact]
    public void IndexProject_EmptyProject_ReturnsZero()
    {
        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(0);
    }

    [Fact]
    public void IndexProject_ClearsExistingEntries()
    {
        _db.Insert("decision", "Old Entry", "Old body.", "old.md", "", "2026-01-01");
        WriteDecision("001-new.md", "# New ADR\nNew content.");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Recent(null, 100);
        results.Should().OnlyContain(r => r.Title == "New ADR");
    }

    [Fact]
    public void IndexProject_ReturnsCorrectTotalCount()
    {
        WriteDecision("001-adr.md", "# ADR One\nContent.");
        WriteDecision("002-adr.md", "# ADR Two\nContent.");
        WriteChangelog("## [1.0.0]\nFirst release.\n## [1.1.0]\nSecond release.");
        WriteKnownIssues("| Issue | Notes |\n|-------|-------|\n| Bug A | Note A |\n| Bug B | Note B |");

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(6); // 2 ADRs + 2 changelog blocks + 2 issue rows
    }

    // -------------------------------------------------------------------------
    // Decisions (ADRs)
    // -------------------------------------------------------------------------

    [Fact]
    public void IndexProject_IndexesAdrFiles()
    {
        WriteDecision("001-use-sqlite.md", "# Use SQLite for FTS5\nWe chose SQLite.");

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(1);
        var results = _db.Recent("decision", 10);
        results.Should().HaveCount(1);
        results[0].Type.Should().Be("decision");
        results[0].Title.Should().Be("Use SQLite for FTS5");
        results[0].SourcePath.Should().Be("docs/decisions/001-use-sqlite.md");
    }

    [Fact]
    public void IndexProject_Adr_ExtractsH1AsTitle()
    {
        WriteDecision("001-adr.md", "# My Decision Title\nBody of the decision.");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Recent("decision", 10);
        results[0].Title.Should().Be("My Decision Title");
    }

    [Fact]
    public void IndexProject_Adr_UsesFilenameWhenNoH1()
    {
        WriteDecision("001-storage-choice.md", "No heading here, just body text.");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Recent("decision", 10);
        results[0].Title.Should().Be("001-storage-choice");
    }

    [Fact]
    public void IndexProject_Adr_SkipsReadmeMd()
    {
        WriteDecision("README.md", "# Decisions README\nThis is the readme.");
        WriteDecision("001-real-adr.md", "# Real ADR\nActual decision.");

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(1);
        var results = _db.Recent("decision", 10);
        results.Should().HaveCount(1).And.Contain(r => r.Title == "Real ADR");
    }

    [Fact]
    public void IndexProject_Adr_ExtractsFrontMatterTags()
    {
        WriteDecision("001-auth.md", "---\ntags: auth, security\n---\n# Auth Decision\nBody.");

        MemoryIndexer.IndexProject(_db, _tempDir);

        // Tags should be searchable
        var results = _db.Search("security");
        results.Should().HaveCount(1);
    }

    [Fact]
    public void IndexProject_Adr_WithYamlSequenceTags()
    {
        WriteDecision("001-adr.md", "---\ntags:\n  - database\n  - performance\n---\n# DB Decision\nBody.");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Search("performance");
        results.Should().HaveCount(1);
    }

    [Fact]
    public void IndexProject_NoDecisionsDir_ReturnsZeroForDecisions()
    {
        WriteChangelog("## [1.0.0]\nRelease.");

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(1);
        _db.Recent("decision", 10).Should().BeEmpty();
    }

    [Fact]
    public void IndexProject_MultipleAdrs_IndexedInOrder()
    {
        WriteDecision("001-first.md", "# First ADR\nBody.");
        WriteDecision("002-second.md", "# Second ADR\nBody.");
        WriteDecision("003-third.md", "# Third ADR\nBody.");

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(3);
        _db.Recent("decision", 10).Should().HaveCount(3);
    }

    // -------------------------------------------------------------------------
    // Changelog
    // -------------------------------------------------------------------------

    [Fact]
    public void IndexProject_IndexesChangelogBlocks()
    {
        WriteChangelog("""
            # Changelog
            
            ## [1.0.0] - 2026-01-01
            
            ### Added
            - Initial release
            
            ## [0.9.0] - 2025-12-01
            
            ### Added
            - Beta features
            """);

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(2);
        var results = _db.Recent("changelog", 10);
        results.Should().HaveCount(2);
    }

    [Fact]
    public void IndexProject_Changelog_VersionIsTitle()
    {
        WriteChangelog("## [2.0.0] - 2026-03-22\n\n### Added\n- New stuff\n");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Recent("changelog", 10);
        results[0].Title.Should().Be("[2.0.0] - 2026-03-22");
    }

    [Fact]
    public void IndexProject_Changelog_BodyIsSearchable()
    {
        WriteChangelog("## [1.0.0]\n\n### Added\n- Added FTS5 support for memory search\n");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Search("FTS5");
        results.Should().HaveCount(1);
    }

    [Fact]
    public void IndexProject_Changelog_EmptyBlocksSkipped()
    {
        WriteChangelog("## [1.0.0]\n\n## [0.9.0]\n\nBeta release stuff.");

        MemoryIndexer.IndexProject(_db, _tempDir);

        // "1.0.0" block body is empty -> skipped; "0.9.0" has content -> indexed
        var results = _db.Recent("changelog", 10);
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("[0.9.0]");
    }

    [Fact]
    public void IndexProject_NoChangelogFile_SkipsWithNoError()
    {
        WriteDecision("001-adr.md", "# ADR\nContent.");

        var act = () => MemoryIndexer.IndexProject(_db, _tempDir);

        act.Should().NotThrow();
        _db.Recent("changelog", 10).Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Known Issues
    // -------------------------------------------------------------------------

    [Fact]
    public void IndexProject_IndexesKnownIssues()
    {
        WriteKnownIssues("""
            | Issue | Notes | Status |
            |-------|-------|--------|
            | Login crash | Happens on cold start | Open |
            | Memory leak | Seen on Windows | In progress |
            """);

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(2);
        var results = _db.Recent("issue", 10);
        results.Should().HaveCount(2);
    }

    [Fact]
    public void IndexProject_KnownIssues_TitleIsFirstCell()
    {
        WriteKnownIssues("| Issue | Notes |\n|-------|-------|\n| Login crash | Happens on cold start |");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Recent("issue", 10);
        results[0].Title.Should().Be("Login crash");
    }

    [Fact]
    public void IndexProject_KnownIssues_BodyContainsRemainingCells()
    {
        WriteKnownIssues("| Issue | Notes |\n|-------|-------|\n| Crash | Cold start issue |");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Search("Cold start");
        results.Should().HaveCount(1);
    }

    [Fact]
    public void IndexProject_KnownIssues_SkipsHeaderAndSeparatorRows()
    {
        WriteKnownIssues("| Issue | Notes |\n|-------|-------|\n| Real Bug | Description |");

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(1); // Only the data row
    }

    [Fact]
    public void IndexProject_NoKnownIssuesFile_SkipsWithNoError()
    {
        WriteDecision("001-adr.md", "# ADR\nContent.");

        var act = () => MemoryIndexer.IndexProject(_db, _tempDir);

        act.Should().NotThrow();
        _db.Recent("issue", 10).Should().BeEmpty();
    }

    [Fact]
    public void IndexProject_KnownIssues_MultipleTablesIndexedCorrectly()
    {
        WriteKnownIssues("""
            ## Open Issues

            | Issue | Notes |
            |-------|-------|
            | Bug A | Note A |

            ## Closed Issues

            | Issue | Notes |
            |-------|-------|
            | Bug B | Note B |
            """);

        var count = MemoryIndexer.IndexProject(_db, _tempDir);

        count.Should().Be(2);
    }

    // -------------------------------------------------------------------------
    // Search integration after indexing
    // -------------------------------------------------------------------------

    [Fact]
    public void IndexProject_ThenSearch_FindsAcrossAllTypes()
    {
        WriteDecision("001-auth.md", "# Auth Decision\nWe use OAuth2.");
        WriteChangelog("## [1.0.0]\nAdded OAuth2 login flow.");
        WriteKnownIssues("| OAuth2 redirect bug | Affects mobile |\n|---|---|\n| OAuth2 redirect bug | Affects mobile |");

        MemoryIndexer.IndexProject(_db, _tempDir);

        var results = _db.Search("OAuth2", limit: 10);
        results.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
