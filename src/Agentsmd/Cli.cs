using Spectre.Console;

namespace Agentsmd;

/// <summary>
/// Centralized CLI rendering using Spectre.Console.
/// All output goes through here for consistent theming.
/// Falls back gracefully when stdout is redirected (non-TTY).
/// </summary>
public static class Cli
{
    // ──── Emoji / Icons ────
    public static string AgentIcon => "🤖";
    public static string SkillIcon => "📦";
    public static string WorkflowIcon => "🔄";
    public static string FolderIcon => "📁";
    public static string BacklogIcon => "📋";
    public static string InProgressIcon => "🔨";
    public static string ToolsIcon => "🔧";

    public static string TypeIcon(string type) => type.ToLowerInvariant() switch
    {
        "agent" => AgentIcon,
        "skill" => SkillIcon,
        "workflow" => WorkflowIcon,
        _ => "·"
    };

    // ──── Colors (markup strings) ────
    public static string Primary => "dodgerblue1";
    public static string Success => "green3";
    public static string Warning => "orange1";
    public static string Error => "red";
    public static string Muted => "grey";
    public static string Accent => "mediumpurple2";

    // ──── Logo ────
    public static void WriteLogo()
    {
        AnsiConsole.Write(new FigletText("agentsmd").Color(Color.DodgerBlue1));
        AnsiConsole.MarkupLine($"[{Muted}]AI Agent Workflow Manager[/]");
        AnsiConsole.WriteLine();
    }

    // ──── Tables ────
    public static Table CreateTable(params string[] columns)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        foreach (var col in columns)
            table.AddColumn(new TableColumn($"[bold]{Markup.Escape(col)}[/]"));

        return table;
    }

    public static void WriteArtifactTable(IEnumerable<(string Type, string Name, string Version, string Description)> items)
    {
        var table = CreateTable("Type", "Name", "Version", "Description");
        foreach (var (type, name, version, description) in items)
        {
            table.AddRow(
                $"{TypeIcon(type)} [bold]{Markup.Escape(type)}[/]",
                $"[{Primary}]{Markup.Escape(name)}[/]",
                $"[{Muted}]{Markup.Escape(version)}[/]",
                Markup.Escape(description));
        }
        AnsiConsole.Write(table);
    }

    public static void WriteInstalledTable(IEnumerable<(string Type, string Name, string Version)> items)
    {
        var table = CreateTable("Type", "Name", "Version");
        foreach (var (type, name, version) in items)
        {
            table.AddRow(
                $"{TypeIcon(type)} [bold]{Markup.Escape(type)}[/]",
                $"[{Primary}]{Markup.Escape(name)}[/]",
                $"[{Muted}]{Markup.Escape(version)}[/]");
        }
        AnsiConsole.Write(table);
    }

    // ──── Panels ────
    public static void WritePanel(string header, string content)
    {
        var panel = new Panel(content)
            .Header($"[bold {Primary}]{Markup.Escape(header)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(1, 0, 1, 0);
        AnsiConsole.Write(panel);
    }

    // ──── Status messages ────
    public static void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[{Success}]✓[/] {Markup.Escape(message)}");
    }

    public static void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[{Warning}]⚠[/] {Markup.Escape(message)}");
    }

    public static void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[{Error}]✗[/] {Markup.Escape(message)}");
    }

    public static void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine($"[{Muted}]{Markup.Escape(message)}[/]");
    }

    public static void WriteMuted(string message)
    {
        AnsiConsole.MarkupLine($"[{Muted}]{Markup.Escape(message)}[/]");
    }

    // ──── Spinner wrapper ────
    public static async Task<T> SpinAsync<T>(string message, Func<Task<T>> action)
    {
        T result = default!;
        await AnsiConsole.Status().StartAsync(message, async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse(Primary));
            result = await action();
        });
        return result;
    }

    public static async Task SpinAsync(string message, Func<Task> action)
    {
        await AnsiConsole.Status().StartAsync(message, async ctx =>
        {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse(Primary));
            await action();
        });
    }

    // ──── Tree ────
    public static void WriteTree(string root, IEnumerable<string> children)
    {
        var tree = new Tree($"[{Primary}]{Markup.Escape(root)}[/]");
        foreach (var child in children)
            tree.AddNode(Markup.Escape(child));
        AnsiConsole.Write(tree);
    }

    public static void WriteFileTree(string root, IEnumerable<(string folder, IEnumerable<string> files)> groups)
    {
        var tree = new Tree($"[bold]Synced[/]");
        foreach (var (folder, files) in groups)
        {
            var node = tree.AddNode($"{FolderIcon} [{Primary}]{Markup.Escape(folder)}[/]");
            foreach (var file in files)
                node.AddNode($"[{Muted}]{Markup.Escape(file)}[/]");
        }
        AnsiConsole.Write(tree);
    }

    // ──── Error panel ────
    public static void WriteErrorPanel(string title, string message, string? suggestion = null)
    {
        var content = Markup.Escape(message);
        if (suggestion is not null)
            content += $"\n\n[{Muted}]Hint:[/] [{Accent}]{Markup.Escape(suggestion)}[/]";

        var panel = new Panel(content)
            .Header($"[bold {Error}]{Markup.Escape(title)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red)
            .Padding(1, 0, 1, 0);
        AnsiConsole.Write(panel);
    }

    // ──── Fuzzy match (did-you-mean) ────
    public static string? FindClosestMatch(string input, IEnumerable<string> candidates, int maxDistance = 3)
    {
        string? best = null;
        var bestDist = int.MaxValue;

        foreach (var candidate in candidates)
        {
            var dist = LevenshteinDistance(input.ToLowerInvariant(), candidate.ToLowerInvariant());
            if (dist < bestDist && dist <= maxDistance)
            {
                bestDist = dist;
                best = candidate;
            }
        }
        return best;
    }

    private static int LevenshteinDistance(string s, string t)
    {
        var n = s.Length;
        var m = t.Length;
        var d = new int[n + 1, m + 1];

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
        for (var j = 1; j <= m; j++)
        {
            var cost = s[i - 1] == t[j - 1] ? 0 : 1;
            d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
        }

        return d[n, m];
    }
}
