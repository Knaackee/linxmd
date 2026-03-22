using Microsoft.Data.Sqlite;

namespace Linxmd.Services;

/// <summary>
/// SQLite + FTS5 backing store for project memory (ADRs, changelog, known issues).
/// The .db file is a derived index — git-ignorable and safe to delete/rebuild at any time.
/// </summary>
public sealed class MemoryDb : IDisposable
{
    private readonly SqliteConnection _conn;
    private SqliteTransaction? _tx;

    public MemoryDb(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _conn = new SqliteConnection($"Data Source={dbPath}");
        _conn.Open();
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        // WAL mode: allows reads to proceed concurrently with a write
        Execute("PRAGMA journal_mode=WAL;");
        Execute("""
            CREATE TABLE IF NOT EXISTS memories (
                id          INTEGER PRIMARY KEY,
                type        TEXT NOT NULL,
                title       TEXT NOT NULL,
                body        TEXT NOT NULL,
                source_path TEXT NOT NULL,
                tags        TEXT NOT NULL DEFAULT '',
                created_at  TEXT NOT NULL
            );
            """);
        Execute("CREATE INDEX IF NOT EXISTS idx_memories_type ON memories(type);");
        Execute("""
            CREATE VIRTUAL TABLE IF NOT EXISTS memories_fts USING fts5(
                title,
                body,
                tags,
                content=memories,
                content_rowid=id
            );
            """);
    }

    /// <summary>
    /// Starts a write transaction. All subsequent Insert calls participate in it.
    /// Must be paired with <see cref="Commit"/>. Dramatically faster for bulk inserts.
    /// </summary>
    public void BeginTransaction() => _tx = _conn.BeginTransaction();

    /// <summary>Commits and disposes the active transaction.</summary>
    public void Commit()
    {
        _tx?.Commit();
        _tx?.Dispose();
        _tx = null;
    }

    private void Execute(string sql)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public void ClearAll()
    {
        Execute("DELETE FROM memories;");
        // Rebuild the FTS5 index from the (now empty) content table
        Execute("INSERT INTO memories_fts(memories_fts) VALUES('rebuild');");
    }

    public void Insert(string type, string title, string body, string sourcePath, string tags, string createdAt)
    {
        long rowid;
        using (var cmd = _conn.CreateCommand())
        {
            cmd.Transaction = _tx;
            cmd.CommandText = """
                INSERT INTO memories (type, title, body, source_path, tags, created_at)
                VALUES (@type, @title, @body, @source, @tags, @created);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@body", body);
            cmd.Parameters.AddWithValue("@source", sourcePath);
            cmd.Parameters.AddWithValue("@tags", tags);
            cmd.Parameters.AddWithValue("@created", createdAt);
            rowid = (long)cmd.ExecuteScalar()!;
        }
        using (var ftsCmd = _conn.CreateCommand())
        {
            ftsCmd.Transaction = _tx;
            ftsCmd.CommandText = "INSERT INTO memories_fts(rowid, title, body, tags) VALUES (@rowid, @title, @body, @tags);";
            ftsCmd.Parameters.AddWithValue("@rowid", rowid);
            ftsCmd.Parameters.AddWithValue("@title", title);
            ftsCmd.Parameters.AddWithValue("@body", body);
            ftsCmd.Parameters.AddWithValue("@tags", tags);
            ftsCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// FTS5 keyword search. Returns up to <paramref name="limit"/> results ordered by rank.
    /// The <see cref="MemoryResult.Snippet"/> field contains a highlighted excerpt from the body.
    /// </summary>
    public List<MemoryResult> Search(string query, int limit = 5)
    {
        var results = new List<MemoryResult>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            SELECT m.id, m.type, m.title, m.source_path, m.created_at,
                   snippet(memories_fts, 1, '>>>', '<<<', '...', 20) AS snip
            FROM memories_fts f
            JOIN memories m ON m.id = f.rowid
            WHERE memories_fts MATCH @query
            ORDER BY rank
            LIMIT @limit;
            """;
        cmd.Parameters.AddWithValue("@query", query);
        cmd.Parameters.AddWithValue("@limit", limit);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new MemoryResult(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)));
        }
        return results;
    }

    /// <summary>Returns entry count grouped by type.</summary>
    public Dictionary<string, int> Stats()
    {
        var stats = new Dictionary<string, int>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT type, COUNT(*) FROM memories GROUP BY type ORDER BY type;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            stats[reader.GetString(0)] = reader.GetInt32(1);
        return stats;
    }

    /// <summary>Returns the most recently indexed entries, newest first.</summary>
    public List<MemoryResult> Recent(string? type, int limit = 10)
    {
        var results = new List<MemoryResult>();
        using var cmd = _conn.CreateCommand();
        if (type is not null)
        {
            cmd.CommandText = "SELECT id, type, title, source_path, created_at, '' FROM memories WHERE type = @type ORDER BY id DESC LIMIT @limit;";
            cmd.Parameters.AddWithValue("@type", type);
        }
        else
        {
            cmd.CommandText = "SELECT id, type, title, source_path, created_at, '' FROM memories ORDER BY id DESC LIMIT @limit;";
        }
        cmd.Parameters.AddWithValue("@limit", limit);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new MemoryResult(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)));
        }
        return results;
    }

    public void Dispose()
    {
        _tx?.Dispose();
        _conn.Dispose();
    }
}

public record MemoryResult(
    long Id,
    string Type,
    string Title,
    string SourcePath,
    string CreatedAt,
    string Snippet);
