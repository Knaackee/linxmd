namespace Linxmd.Services;

/// <summary>
/// Scans project memory source files (ADRs, CHANGELOG, KNOWN_ISSUES) and
/// populates a <see cref="MemoryDb"/> with their contents for FTS5 search.
///
/// Source files remain the canonical, human-readable source of truth.
/// The SQLite db is a derived index — safe to delete and rebuild.
/// </summary>
public static class MemoryIndexer
{
    /// <summary>
    /// Clears the existing index and re-indexes all memory source files
    /// found under <paramref name="projectRoot"/>.
    /// Returns the total number of entries indexed.
    /// </summary>
    public static int IndexProject(MemoryDb db, string projectRoot)
    {
        db.ClearAll();
        db.BeginTransaction();
        int count = 0;
        try
        {
            count += IndexDecisions(db, projectRoot);
            count += IndexChangelog(db, projectRoot);
            count += IndexKnownIssues(db, projectRoot);
            db.Commit();
        }
        catch
        {
            // Transaction auto-rolls-back when MemoryDb is disposed; rethrow so
            // the caller can surface the error.
            throw;
        }
        return count;
    }

    // -------------------------------------------------------------------------
    // ADRs: docs/decisions/*.md (excluding README.md)
    // -------------------------------------------------------------------------

    private static int IndexDecisions(MemoryDb db, string projectRoot)
    {
        var decisionsDir = Path.Combine(projectRoot, "docs", "decisions");
        if (!Directory.Exists(decisionsDir)) return 0;

        int count = 0;
        foreach (var file in Directory.GetFiles(decisionsDir, "*.md").OrderBy(f => f))
        {
            var fileName = Path.GetFileName(file);
            if (fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase)) continue;

            var content = File.ReadAllText(file);
            var title = ExtractH1(content) ?? Path.GetFileNameWithoutExtension(file);
            var tags = ExtractFrontMatterField(content, "tags");
            var created = File.GetLastWriteTimeUtc(file).ToString("yyyy-MM-dd");
            var relPath = Path.GetRelativePath(projectRoot, file).Replace('\\', '/');

            db.Insert("decision", title, content, relPath, tags, created);
            count++;
        }
        return count;
    }

    // -------------------------------------------------------------------------
    // CHANGELOG.md — one entry per version block
    // -------------------------------------------------------------------------

    private static int IndexChangelog(MemoryDb db, string projectRoot)
    {
        var path = Path.Combine(projectRoot, "CHANGELOG.md");
        if (!File.Exists(path)) return 0;

        int count = 0;
        var blocks = ParseChangelogBlocks(File.ReadAllText(path));
        foreach (var (version, body) in blocks)
        {
            if (string.IsNullOrWhiteSpace(body)) continue;
            db.Insert("changelog", version, body, "CHANGELOG.md", "", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            count++;
        }
        return count;
    }

    // -------------------------------------------------------------------------
    // KNOWN_ISSUES.md — one entry per table row
    // -------------------------------------------------------------------------

    private static int IndexKnownIssues(MemoryDb db, string projectRoot)
    {
        var path = Path.Combine(projectRoot, "KNOWN_ISSUES.md");
        if (!File.Exists(path)) return 0;

        int count = 0;
        var rows = ParseMarkdownTableRows(File.ReadAllText(path));
        foreach (var (title, body) in rows)
        {
            if (string.IsNullOrWhiteSpace(title)) continue;
            db.Insert("issue", title, body, "KNOWN_ISSUES.md", "", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            count++;
        }
        return count;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string? ExtractH1(string content)
    {
        // Skip YAML frontmatter if present
        var startIndex = 0;
        if (content.StartsWith("---"))
        {
            var fmEnd = content.IndexOf("\n---", 3);
            if (fmEnd >= 0) startIndex = fmEnd + 4;
        }

        foreach (var line in content[startIndex..].Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("# "))
                return trimmed[2..].Trim();
        }
        return null;
    }

    private static string ExtractFrontMatterField(string content, string field)
    {
        if (!content.StartsWith("---")) return "";
        var end = content.IndexOf("\n---", 3);
        if (end < 0) return "";
        var fm = content[3..end];
        var lines = fm.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (!trimmed.StartsWith(field + ":", StringComparison.OrdinalIgnoreCase)) continue;

            var inline = trimmed[(field.Length + 1)..].Trim();

            // Inline scalar: "tags: auth, security"
            if (!string.IsNullOrWhiteSpace(inline) && !inline.StartsWith('['))
                return inline;

            // Inline YAML sequence: "tags: [auth, security]"
            if (inline.StartsWith('['))
                return inline.Trim('[', ']').Replace("\"", "").Replace("'", "");

            // Multi-line YAML sequence:
            //   tags:
            //     - auth
            //     - security
            var items = new List<string>();
            for (var j = i + 1; j < lines.Length; j++)
            {
                var next = lines[j];
                if (!next.TrimStart().StartsWith("- ")) break;
                items.Add(next.TrimStart()[2..].Trim());
            }
            return string.Join(", ", items);
        }
        return "";
    }

    private static List<(string version, string body)> ParseChangelogBlocks(string content)
    {
        var result = new List<(string, string)>();
        string? currentVersion = null;
        var bodyLines = new List<string>();

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith("## "))
            {
                if (currentVersion is not null)
                    result.Add((currentVersion, string.Join('\n', bodyLines).Trim()));

                currentVersion = line[3..].Trim();
                bodyLines.Clear();
            }
            else if (currentVersion is not null)
            {
                bodyLines.Add(line);
            }
        }

        if (currentVersion is not null)
            result.Add((currentVersion, string.Join('\n', bodyLines).Trim()));

        return result;
    }

    /// <summary>
    /// Parses all markdown table rows (across all tables in the file).
    /// Returns (first-cell, remaining-cells-joined) per data row.
    /// </summary>
    private static List<(string title, string body)> ParseMarkdownTableRows(string content)
    {
        var result = new List<(string, string)>();
        bool pastSeparator = false;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Trim();

            if (!line.StartsWith("|"))
            {
                pastSeparator = false;
                continue;
            }

            // Separator row: |---|---|
            if (line.Replace("-", "").Replace("|", "").Replace(" ", "").Length == 0)
            {
                pastSeparator = true;
                continue;
            }

            if (!pastSeparator) continue; // header row

            var cells = line.Split('|', StringSplitOptions.TrimEntries)
                            .Where(c => !string.IsNullOrEmpty(c))
                            .ToArray();

            if (cells.Length == 0) continue;

            var title = cells[0];
            var body = cells.Length > 1 ? string.Join(" | ", cells[1..]) : "";
            result.Add((title, body));
        }

        return result;
    }
}
