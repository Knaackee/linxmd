using Linxmd.Services;
using FluentAssertions;

namespace Linxmd.Tests.Services;

public class MemoryDbTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dbPath;
    private readonly MemoryDb _db;

    public MemoryDbTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "linxmd-memdb-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _dbPath = Path.Combine(_tempDir, "test-memory.db");
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

    [Fact]
    public void Constructor_CreatesDbFile()
    {
        File.Exists(_dbPath).Should().BeTrue();
    }

    [Fact]
    public void Insert_SingleEntry_PersistsToDb()
    {
        _db.Insert("decision", "Use SQLite", "We chose SQLite for FTS5 support.", "docs/decisions/001.md", "db, storage", "2026-01-01");

        var results = _db.Recent(null, 10);
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Use SQLite");
        results[0].Type.Should().Be("decision");
        results[0].SourcePath.Should().Be("docs/decisions/001.md");
    }

    [Fact]
    public void Insert_WithTransaction_PersistsOnCommit()
    {
        _db.BeginTransaction();
        _db.Insert("changelog", "v1.0.0", "Initial release.", "CHANGELOG.md", "", "2026-01-01");
        _db.Insert("changelog", "v1.1.0", "Added features.", "CHANGELOG.md", "", "2026-01-02");
        _db.Commit();

        var results = _db.Recent(null, 10);
        results.Should().HaveCount(2);
    }

    [Fact]
    public void ClearAll_RemovesAllEntries()
    {
        _db.Insert("decision", "ADR 001", "Body text.", "docs/decisions/001.md", "db", "2026-01-01");
        _db.Insert("changelog", "v1.0.0", "Release.", "CHANGELOG.md", "", "2026-01-01");

        _db.ClearAll();

        var results = _db.Recent(null, 10);
        results.Should().BeEmpty();
    }

    [Fact]
    public void Search_FindsMatchingEntry()
    {
        _db.Insert("decision", "Use PostgreSQL", "Chosen for relational integrity.", "docs/decisions/001.md", "", "2026-01-01");
        _db.Insert("changelog", "v1.0.0", "Initial release with SQLite.", "CHANGELOG.md", "", "2026-01-01");

        var results = _db.Search("PostgreSQL");

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Use PostgreSQL");
    }

    [Fact]
    public void Search_CaseInsensitive_FindsMatch()
    {
        _db.Insert("decision", "Use Redis for caching", "Fast key-value store.", "docs/001.md", "cache", "2026-01-01");

        var results = _db.Search("redis");

        results.Should().HaveCount(1);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
    {
        _db.Insert("decision", "Use SQLite", "Fast local DB.", "docs/001.md", "", "2026-01-01");

        var results = _db.Search("postgresql");

        results.Should().BeEmpty();
    }

    [Fact]
    public void Search_RespectsLimit()
    {
        _db.BeginTransaction();
        for (int i = 1; i <= 10; i++)
            _db.Insert("decision", $"ADR {i:00}", "Content about architecture.", $"docs/{i:00}.md", "", "2026-01-01");
        _db.Commit();

        var results = _db.Search("architecture", limit: 3);

        results.Should().HaveCount(3);
    }

    [Fact]
    public void Search_ByTags_FindsMatch()
    {
        _db.Insert("decision", "Auth ADR", "We use JWT.", "docs/001.md", "auth security", "2026-01-01");
        _db.Insert("decision", "Storage ADR", "We use S3.", "docs/002.md", "storage cloud", "2026-01-01");

        var results = _db.Search("security");

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Auth ADR");
    }

    [Fact]
    public void Stats_ReturnsGroupedCounts()
    {
        _db.BeginTransaction();
        _db.Insert("decision", "ADR 1", "Body.", "d/001.md", "", "2026-01-01");
        _db.Insert("decision", "ADR 2", "Body.", "d/002.md", "", "2026-01-01");
        _db.Insert("changelog", "v1.0.0", "Release.", "CHANGELOG.md", "", "2026-01-01");
        _db.Insert("issue", "Bug #1", "Description.", "KNOWN_ISSUES.md", "", "2026-01-01");
        _db.Commit();

        var stats = _db.Stats();

        stats.Should().ContainKey("decision").WhoseValue.Should().Be(2);
        stats.Should().ContainKey("changelog").WhoseValue.Should().Be(1);
        stats.Should().ContainKey("issue").WhoseValue.Should().Be(1);
    }

    [Fact]
    public void Stats_EmptyDb_ReturnsEmptyDictionary()
    {
        var stats = _db.Stats();

        stats.Should().BeEmpty();
    }

    [Fact]
    public void Recent_NoType_ReturnsAllEntriesNewestFirst()
    {
        _db.Insert("decision", "First", "Body.", "d/001.md", "", "2026-01-01");
        _db.Insert("changelog", "Second", "Body.", "CHANGELOG.md", "", "2026-01-01");

        var results = _db.Recent(null, 10);

        results.Should().HaveCount(2);
        // Newest first (by insert order / id)
        results[0].Title.Should().Be("Second");
        results[1].Title.Should().Be("First");
    }

    [Fact]
    public void Recent_WithType_FiltersToType()
    {
        _db.BeginTransaction();
        _db.Insert("decision", "ADR 1", "Body.", "d/001.md", "", "2026-01-01");
        _db.Insert("changelog", "v1.0.0", "Release.", "CHANGELOG.md", "", "2026-01-01");
        _db.Insert("decision", "ADR 2", "Body.", "d/002.md", "", "2026-01-01");
        _db.Commit();

        var results = _db.Recent("decision", 10);

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.Type == "decision");
    }

    [Fact]
    public void Recent_RespectsLimit()
    {
        _db.BeginTransaction();
        for (int i = 1; i <= 10; i++)
            _db.Insert("decision", $"ADR {i:00}", "Body.", $"d/{i:00}.md", "", "2026-01-01");
        _db.Commit();

        var results = _db.Recent(null, 3);

        results.Should().HaveCount(3);
    }

    [Fact]
    public void ClearAll_ThenInsert_Works()
    {
        _db.Insert("decision", "Old Entry", "Old body.", "old.md", "", "2026-01-01");
        _db.ClearAll();
        _db.Insert("decision", "New Entry", "New body.", "new.md", "", "2026-01-02");

        var results = _db.Recent(null, 10);
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("New Entry");
    }

    [Fact]
    public void Search_AfterClearAll_ReturnsEmpty()
    {
        _db.Insert("decision", "Important ADR", "About auth.", "docs/001.md", "auth", "2026-01-01");
        _db.ClearAll();

        var results = _db.Search("auth");

        results.Should().BeEmpty();
    }

    [Fact]
    public void MemoryResult_HasAllFields()
    {
        _db.Insert("decision", "Test Title", "Test body content.", "docs/001.md", "test", "2026-03-22");

        var recent = _db.Recent(null, 1);

        recent.Should().HaveCount(1);
        var r = recent[0];
        r.Id.Should().BeGreaterThan(0);
        r.Type.Should().Be("decision");
        r.Title.Should().Be("Test Title");
        r.SourcePath.Should().Be("docs/001.md");
        r.CreatedAt.Should().Be("2026-03-22");
    }
}
