using System.CommandLine;
using System.Text;
using Agentsmd.Services;
using Agentsmd.Parsing;
using Spectre.Console;

namespace Agentsmd.Commands;

public static class CommandFactory
{
    private static readonly HttpClient Http = new();

    public static readonly Option<string> ProjectOption =
        new(["--project", "-p"], () => Directory.GetCurrentDirectory(), "Project root directory");

    // ──── Agent ────
    public static Command CreateAgentCommand()
    {
        var cmd = new Command("agent", "Manage agents");
        cmd.AddCommand(CreateTypeInstallCommand("agent"));
        cmd.AddCommand(CreateTypeUninstallCommand("agent"));
        cmd.AddCommand(CreateTypeListCommand("agent"));
        cmd.AddCommand(CreateTypeSearchCommand("agent"));
        cmd.AddCommand(CreateTypeInfoCommand("agent"));
        return cmd;
    }

    // ──── Skill ────
    public static Command CreateSkillCommand()
    {
        var cmd = new Command("skill", "Manage skills");
        cmd.AddCommand(CreateTypeInstallCommand("skill"));
        cmd.AddCommand(CreateTypeUninstallCommand("skill"));
        cmd.AddCommand(CreateTypeListCommand("skill"));
        cmd.AddCommand(CreateTypeSearchCommand("skill"));
        cmd.AddCommand(CreateTypeInfoCommand("skill"));
        return cmd;
    }

    // ──── Workflow ────
    public static Command CreateWorkflowCommand()
    {
        var cmd = new Command("workflow", "Manage workflows");
        cmd.AddCommand(CreateTypeInstallCommand("workflow"));
        cmd.AddCommand(CreateTypeUninstallCommand("workflow"));
        cmd.AddCommand(CreateTypeListCommand("workflow"));
        cmd.AddCommand(CreateTypeSearchCommand("workflow"));
        cmd.AddCommand(CreateTypeInfoCommand("workflow"));
        return cmd;
    }

    // ──── Global search ────
    public static Command CreateSearchCommand()
    {
        var queryArg = new Argument<string>("query", () => "", "Search query");
        var cmd = new Command("search", "Search all artifacts in lib") { queryArg };
        cmd.SetHandler(async (string query, string project) =>
        {
            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var results = IndexParser.Search(index, query);
            if (results.Count == 0) { Console.WriteLine("No results found."); return; }

            Cli.WriteArtifactTable(results.Select(a => (a.Type, a.Name, a.Version, a.Description)));
        }, queryArg, ProjectOption);
        return cmd;
    }

    // ──── Global list ────
    public static Command CreateListCommand()
    {
        var cmd = new Command("list", "List all installed artifacts");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized) { Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first."); return; }

            var installed = state.Load();
            if (installed.Artifacts.Count == 0) { Console.WriteLine("No artifacts installed."); return; }

            Cli.WriteInstalledTable(installed.Artifacts.Select(a => (a.Type, a.Name, a.Version)));
        }, ProjectOption);
        return cmd;
    }

    // ──── Sync ────
    public static Command CreateSyncCommand()
    {
        var cmd = new Command("sync", "Generate tool wrappers and copy skills");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized) { Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first."); return; }

            var options = DetectTools(project);
            var engine = new SyncEngine(state, project);
            var result = engine.Sync(options);

            Cli.WriteSuccess($"Synced: {result.GeneratedFiles.Count} wrapper(s), {result.CopiedSkills.Count} skill(s) copied.");

            if (result.GeneratedFiles.Count > 0 || result.CopiedSkills.Count > 0)
            {
                var groups = new List<(string folder, IEnumerable<string> files)>();
                if (result.GeneratedFiles.Count > 0)
                    groups.Add(("Wrappers", result.GeneratedFiles.Select(f => Path.GetRelativePath(project, f))));
                if (result.CopiedSkills.Count > 0)
                    groups.Add(("Skills", result.CopiedSkills.Select(s => $".claude/skills/{s}/")));
                Cli.WriteFileTree("sync", groups);
            }
        }, ProjectOption);
        return cmd;
    }

    // ──── Status ────
    public static Command CreateStatusCommand()
    {
        var cmd = new Command("status", "Show project overview");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.WriteLine("agentsmd: not initialized");
                Console.WriteLine("Run 'agentsmd init' to set up.");
                return;
            }

            var installed = state.Load();

            var agentCount = installed.Artifacts.Count(a => a.Type == "agent");
            var skillCount = installed.Artifacts.Count(a => a.Type == "skill");
            var workflowCount = installed.Artifacts.Count(a => a.Type == "workflow");

            var backlogDir = state.BacklogDir;
            var inProgressDir = state.InProgressDir;
            var backlogCount = Directory.Exists(backlogDir) ? Directory.GetFiles(backlogDir, "*.md").Length : 0;
            var inProgressCount = Directory.Exists(inProgressDir) ? Directory.GetDirectories(inProgressDir).Length : 0;

            // tools
            var tools = new List<string>();
            if (Directory.Exists(Path.Combine(project, ".github", "agents"))) tools.Add("copilot");
            if (Directory.Exists(Path.Combine(project, ".claude", "agents"))) tools.Add("claude-code");
            if (Directory.Exists(Path.Combine(project, ".opencode", "agents"))) tools.Add("opencode");

            var sb = new StringBuilder();
            sb.AppendLine($"  Project:     {Markup.Escape(project)}");
            sb.AppendLine($"  {Cli.AgentIcon} Agents:    {agentCount}");
            sb.AppendLine($"  {Cli.SkillIcon} Skills:    {skillCount}");
            sb.AppendLine($"  {Cli.WorkflowIcon} Workflows: {workflowCount}");
            sb.AppendLine($"  {Cli.BacklogIcon} Backlog:       {backlogCount}");
            sb.AppendLine($"  {Cli.InProgressIcon} In Progress:   {inProgressCount}");
            sb.Append($"  {Cli.ToolsIcon} Tools: {(tools.Count > 0 ? string.Join(", ", tools) : "none")}");

            Cli.WritePanel("agentsmd status", sb.ToString());
        }, ProjectOption);
        return cmd;
    }

    // ──── Init ────
    public static Command CreateInitCommand()
    {
        var cmd = new Command("init", "Initialize agentsmd in project");
        cmd.SetHandler(async (string project) =>
        {
            var state = new InstalledStateManager(project);
            if (state.IsInitialized)
            {
                Console.WriteLine("Already initialized.");
                return;
            }

            Cli.WriteLogo();

            state.EnsureDirectories();
            Cli.WriteSuccess("Created .agentsmd/ structure.");

            // Fetch index and show available workflows
            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is not null)
            {
                var index = IndexParser.Parse(json);
                if (index is not null)
                {
                    var workflows = IndexParser.Search(index, "", filterType: "workflow");
                    if (workflows.Count > 0)
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold]Available workflows:[/]");
                        foreach (var wf in workflows)
                            AnsiConsole.MarkupLine($"  [{Cli.Muted}]-[/] [{Cli.Primary}]{Markup.Escape(wf.Name)}[/]: {Markup.Escape(wf.Description)}");
                        AnsiConsole.MarkupLine($"\n[{Cli.Muted}]Install a workflow:[/] [{Cli.Accent}]agentsmd workflow install <name>[/]");
                    }
                }
            }

            AnsiConsole.WriteLine();
            Cli.WriteSuccess("Initialized. Next steps:");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]1.[/] agentsmd workflow install sdd-tdd");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]2.[/] agentsmd sync");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]3.[/] Start your AI agent with the kick-off prompt");
        }, ProjectOption);
        return cmd;
    }

    // ──── Typed subcommands (shared pattern) ────

    private static Command CreateTypeInstallCommand(string type)
    {
        var nameArg = new Argument<string>("name", $"Name of the {type} to install");
        var cmd = new Command("install", $"Install a {type} from lib") { nameArg };
        cmd.SetHandler(async (string name, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized) { Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first."); return; }

            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var artifact = index.Artifacts.FirstOrDefault(a =>
                a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                a.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (artifact is null)
            {
                Console.Error.WriteLine($"{type} '{name}' not found in lib.");
                var candidates = index.Artifacts
                    .Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Name);
                var suggestion = Cli.FindClosestMatch(name, candidates);
                if (suggestion is not null)
                    AnsiConsole.MarkupLine($"[{Cli.Muted}]Did you mean [{Cli.Accent}]{Markup.Escape(suggestion)}[/]?[/]");
                return;
            }

            if (type == "skill")
            {
                await InstallSkillAsync(client, state, artifact);
            }
            else
            {
                var content = await client.FetchFileAsync(artifact.Path);
                if (content is null) { Console.Error.WriteLine($"Could not fetch {artifact.Path}."); return; }

                var targetDir = state.GetArtifactDir(type);
                var targetPath = Path.Combine(targetDir, Path.GetFileName(artifact.Path));
                await File.WriteAllTextAsync(targetPath, content);
            }

            state.AddArtifact(artifact.Name, artifact.Type, artifact.Version);
            Cli.WriteSuccess($"Installed {type} '{artifact.Name}' v{artifact.Version}.");

            // Install deps
            if (artifact.Deps.Count > 0)
            {
                Console.WriteLine("Installing dependencies...");
                foreach (var dep in artifact.Deps)
                {
                    var parsed = ParseDep(dep);
                    if (parsed is null) continue;

                    var depEntry = index.Artifacts.FirstOrDefault(a =>
                        a.Name.Equals(parsed.Value.name, StringComparison.OrdinalIgnoreCase) &&
                        a.Type.Equals(parsed.Value.type, StringComparison.OrdinalIgnoreCase));

                    if (depEntry is null)
                    {
                        Console.Error.WriteLine($"  Dependency '{dep}' not found in lib.");
                        continue;
                    }

                    // Skip if already installed
                    if (state.GetArtifact(depEntry.Name, depEntry.Type) is not null) continue;

                    if (depEntry.Type == "skill")
                    {
                        await InstallSkillAsync(client, state, depEntry);
                    }
                    else
                    {
                        var depContent = await client.FetchFileAsync(depEntry.Path);
                        if (depContent is null) continue;
                        var targetDir = state.GetArtifactDir(depEntry.Type);
                        await File.WriteAllTextAsync(Path.Combine(targetDir, Path.GetFileName(depEntry.Path)), depContent);
                    }

                    state.AddArtifact(depEntry.Name, depEntry.Type, depEntry.Version);
                    Cli.WriteSuccess($"  Installed {depEntry.Type} '{depEntry.Name}' v{depEntry.Version}.");
                }
            }

            // Auto-sync
            AutoSync(state, project);
        }, nameArg, ProjectOption);
        return cmd;
    }

    private static async Task InstallSkillAsync(ILibClient client, InstalledStateManager state, Models.ArtifactEntry artifact)
    {
        // Skills are folders — we need to fetch the directory listing and download all files
        var entries = await client.ListDirectoryAsync(artifact.Path.TrimEnd('/'));
        var targetDir = Path.Combine(state.SkillsDir, artifact.Name);
        Directory.CreateDirectory(targetDir);

        foreach (var entry in entries)
        {
            var content = await client.FetchFileAsync($"{artifact.Path.TrimEnd('/')}/{entry}");
            if (content is not null)
                await File.WriteAllTextAsync(Path.Combine(targetDir, entry), content);
        }
    }

    private static Command CreateTypeUninstallCommand(string type)
    {
        var nameArg = new Argument<string>("name", $"Name of the {type} to uninstall");
        var cmd = new Command("uninstall", $"Uninstall a {type}") { nameArg };
        cmd.SetHandler((string name, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized) { Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first."); return; }

            var existing = state.GetArtifact(name, type);
            if (existing is null)
            {
                Console.Error.WriteLine($"{type} '{name}' is not installed.");
                return;
            }

            // Remove files
            if (type == "skill")
            {
                var skillDir = Path.Combine(state.SkillsDir, name);
                if (Directory.Exists(skillDir))
                    Directory.Delete(skillDir, recursive: true);
            }
            else
            {
                var targetDir = state.GetArtifactDir(type);
                var targetPath = Path.Combine(targetDir, $"{name}.md");
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
            }

            state.RemoveArtifact(name, type);
            Cli.WriteSuccess($"Uninstalled {type} '{name}'.");

            // Auto-sync
            AutoSync(state, project);
        }, nameArg, ProjectOption);
        return cmd;
    }

    private static Command CreateTypeListCommand(string type)
    {
        var cmd = new Command("list", $"List installed {type}s");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized) { Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first."); return; }

            var installed = state.Load().Artifacts.Where(a => a.Type == type).ToList();
            if (installed.Count == 0) { Console.WriteLine($"No {type}s installed."); return; }

            var table = Cli.CreateTable("Name", "Version");
            foreach (var a in installed)
                table.AddRow(
                    $"[{Cli.Primary}]{Markup.Escape(a.Name)}[/]",
                    $"[{Cli.Muted}]{Markup.Escape(a.Version)}[/]");
            AnsiConsole.Write(table);
        }, ProjectOption);
        return cmd;
    }

    private static Command CreateTypeSearchCommand(string type)
    {
        var queryArg = new Argument<string>("query", () => "", "Search query");
        var cmd = new Command("search", $"Search {type}s in lib") { queryArg };
        cmd.SetHandler(async (string query, string project) =>
        {
            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var results = IndexParser.Search(index, query, filterType: type);
            if (results.Count == 0) { Console.WriteLine("No results found."); return; }

            var table = Cli.CreateTable("Name", "Version", "Description");
            foreach (var a in results)
                table.AddRow(
                    $"[{Cli.Primary}]{Markup.Escape(a.Name)}[/]",
                    $"[{Cli.Muted}]{Markup.Escape(a.Version)}[/]",
                    Markup.Escape(a.Description));
            AnsiConsole.Write(table);
        }, queryArg, ProjectOption);
        return cmd;
    }

    private static Command CreateTypeInfoCommand(string type)
    {
        var nameArg = new Argument<string>("name", $"Name of the {type}");
        var cmd = new Command("info", $"Show {type} details") { nameArg };
        cmd.SetHandler(async (string name, string project) =>
        {
            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var artifact = index.Artifacts.FirstOrDefault(a =>
                a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                a.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (artifact is null)
            {
                Console.Error.WriteLine($"{type} '{name}' not found in lib.");
                var candidates = index.Artifacts
                    .Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Name);
                var suggestion = Cli.FindClosestMatch(name, candidates);
                if (suggestion is not null)
                    AnsiConsole.MarkupLine($"[{Cli.Muted}]Did you mean [{Cli.Accent}]{Markup.Escape(suggestion)}[/]?[/]");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[bold]Name:[/]        [{Cli.Primary}]{Markup.Escape(artifact.Name)}[/]");
            sb.AppendLine($"[bold]Type:[/]        {Cli.TypeIcon(artifact.Type)} {Markup.Escape(artifact.Type)}");
            sb.AppendLine($"[bold]Version:[/]     [{Cli.Muted}]{Markup.Escape(artifact.Version)}[/]");
            sb.AppendLine($"[bold]Description:[/] {Markup.Escape(artifact.Description)}");
            sb.AppendLine($"[bold]Path:[/]        [{Cli.Muted}]{Markup.Escape(artifact.Path)}[/]");

            if (artifact.Deps.Count > 0)
            {
                sb.AppendLine("[bold]Dependencies:[/]");
                foreach (var dep in artifact.Deps)
                    sb.AppendLine($"  [{Cli.Accent}]- {Markup.Escape(dep)}[/]");
            }
            if (artifact.Tags.Count > 0)
                sb.AppendLine($"[bold]Tags:[/]        [{Cli.Muted}]{Markup.Escape(string.Join(", ", artifact.Tags))}[/]");
            if (artifact.Supported is { Count: > 0 })
                sb.AppendLine($"[bold]Supported:[/]   [{Cli.Muted}]{Markup.Escape(string.Join(", ", artifact.Supported))}[/]");

            // Check if installed
            var state = new InstalledStateManager(project);
            if (state.IsInitialized)
            {
                var installed = state.GetArtifact(name, type);
                sb.Append(installed is not null
                    ? $"[bold]Status:[/]      [{Cli.Success}]installed (v{Markup.Escape(installed.Version)})[/]"
                    : $"[bold]Status:[/]      [{Cli.Muted}]not installed[/]");
            }

            Cli.WritePanel($"{Cli.TypeIcon(type)} {artifact.Name}", sb.ToString());
        }, nameArg, ProjectOption);
        return cmd;
    }

    // ──── Helpers ────

    private static void AutoSync(InstalledStateManager state, string project)
    {
        var options = DetectTools(project);
        var engine = new SyncEngine(state, project);
        var result = engine.Sync(options);
        Cli.WriteSuccess($"Synced: {result.GeneratedFiles.Count} wrapper(s), {result.CopiedSkills.Count} skill(s) copied.");
    }

    private static (string type, string name)? ParseDep(string dep)
    {
        // Format: "type:name@version" or "type:name"
        var colonIndex = dep.IndexOf(':');
        if (colonIndex < 0) return null;

        var type = dep[..colonIndex];
        var rest = dep[(colonIndex + 1)..];
        var atIndex = rest.IndexOf('@');
        var name = atIndex >= 0 ? rest[..atIndex] : rest;

        return (type, name);
    }

    private static SyncOptions DetectTools(string project) => new()
    {
        Copilot = Directory.Exists(Path.Combine(project, ".github")) || true, // default on
        ClaudeCode = true, // default — generate for all 
        OpenCode = true
    };
}
