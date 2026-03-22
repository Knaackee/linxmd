using System.CommandLine;
using System.Text;
using System.Text.Json;
using Agentsmd.Models;
using Agentsmd.Services;
using Agentsmd.Parsing;
using Spectre.Console;

namespace Agentsmd.Commands;

public static class CommandFactory
{
    private static readonly HttpClient Http = new();

    public static readonly Option<string> ProjectOption =
        new(["--project", "-p"], () => Directory.GetCurrentDirectory(), "Project root directory");

    // ═══════════════════════════════════════════════════════════════
    // agentsmd add [query] [--type] [--yes]
    // ═══════════════════════════════════════════════════════════════

    public static Command CreateAddCommand()
    {
        var queryArg = new Argument<string>("query", () => "", "Search query or artifact name");
        var typeOpt = new Option<string>(["--type", "-t"], () => "", "Filter by type (agent, skill, workflow)");
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");
        var cmd = new Command("add", "Browse, search and install artifacts") { queryArg };
        cmd.AddOption(typeOpt);
        cmd.AddOption(yesOpt);

        cmd.SetHandler(async (string query, string type, bool yes, string project) =>
        {
            var state = new InstalledStateManager(project);
            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            bool interactive = !yes && AnsiConsole.Profile.Capabilities.Interactive;
            string? filterType = string.IsNullOrEmpty(type) ? null : type;

            // If no query, prompt interactively or leave empty for all
            if (string.IsNullOrEmpty(query) && interactive)
            {
                query = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold]Search library:[/]")
                        .AllowEmpty());
            }

            var results = IndexParser.Search(index, query, filterType: filterType);
            if (results.Count == 0)
            {
                Console.WriteLine("No results found.");
                if (!string.IsNullOrEmpty(query))
                {
                    var suggestion = Cli.FindClosestMatch(query, index.Artifacts.Select(a => a.Name));
                    if (suggestion is not null)
                        AnsiConsole.MarkupLine($"[{Cli.Muted}]Did you mean [{Cli.Accent}]{Markup.Escape(suggestion)}[/]?[/]");
                }
                return;
            }

            ArtifactEntry? selected = null;

            // Try exact name match(es)
            var exact = results
                .Where(a => a.Name.Equals(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (exact.Count == 1)
            {
                selected = exact[0];
            }
            else if (exact.Count > 1)
            {
                // Same name, different types — disambiguate
                if (filterType is not null)
                {
                    selected = exact.FirstOrDefault(a => a.Type.Equals(filterType, StringComparison.OrdinalIgnoreCase));
                }
                else if (interactive)
                {
                    selected = AnsiConsole.Prompt(
                        new SelectionPrompt<ArtifactEntry>()
                            .Title("Multiple matches found:")
                            .PageSize(10)
                            .UseConverter(a => $"{Cli.TypeIcon(a.Type)} {a.Type,-10} {a.Name,-20} {a.Version}")
                            .AddChoices(exact));
                }
                else if (yes)
                {
                    Console.Error.WriteLine($"Multiple artifacts named '{query}'. Use --type to disambiguate.");
                    foreach (var m in exact)
                        Console.Error.WriteLine($"  {m.Type}: {m.Name}");
                    return;
                }
                else
                {
                    // Non-interactive browse — show all matches
                    Cli.WriteArtifactTable(exact.Select(a => (a.Type, a.Name, a.Version, a.Description)));
                    return;
                }
            }
            else if (interactive)
            {
                // No exact match — pick from results
                selected = AnsiConsole.Prompt(
                    new SelectionPrompt<ArtifactEntry>()
                        .Title("Select an artifact to install:")
                        .PageSize(15)
                        .UseConverter(a => $"{Cli.TypeIcon(a.Type)} {a.Type,-10} {a.Name,-20} {a.Version,-8} {a.Description}")
                        .AddChoices(results));
            }
            else if (yes && results.Count == 1)
            {
                selected = results[0];
            }
            else
            {
                // Non-interactive browse — show results table
                Cli.WriteArtifactTable(results.Select(a => (a.Type, a.Name, a.Version, a.Description)));
                return;
            }

            if (selected is null) return;

            // Check init before installing
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

            // Show info
            ShowArtifactInfo(selected);

            // Confirm
            if (interactive)
            {
                var depsMsg = selected.Deps.Count > 0
                    ? $" This will also install {selected.Deps.Count} dependencies."
                    : "";
                if (!AnsiConsole.Confirm($"Install [bold]{Markup.Escape(selected.Name)}[/]?{depsMsg}"))
                    return;
            }

            await InstallArtifactAsync(client, state, index, selected);
            AutoSync(state, project);
        }, queryArg, typeOpt, yesOpt, ProjectOption);

        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // agentsmd remove [query] [--yes]
    // ═══════════════════════════════════════════════════════════════

    public static Command CreateRemoveCommand()
    {
        var queryArg = new Argument<string>("query", () => "", "Artifact name to remove");
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");
        var cmd = new Command("remove", "Browse installed artifacts and uninstall") { queryArg };
        cmd.AddOption(yesOpt);

        cmd.SetHandler((string query, bool yes, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

            var installed = state.Load();
            if (installed.Artifacts.Count == 0)
            {
                Console.WriteLine("No artifacts installed.");
                return;
            }

            bool interactive = !yes && AnsiConsole.Profile.Capabilities.Interactive;
            List<InstalledArtifact> toRemove;

            if (!string.IsNullOrEmpty(query))
            {
                var matches = installed.Artifacts
                    .Where(a => a.Name.Equals(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matches.Count == 0)
                {
                    Console.Error.WriteLine($"'{query}' is not installed.");
                    return;
                }
                toRemove = matches;
            }
            else if (interactive)
            {
                toRemove = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<InstalledArtifact>()
                        .Title("Select artifacts to remove:")
                        .PageSize(15)
                        .UseConverter(a => $"{Cli.TypeIcon(a.Type)} {a.Type,-10} {a.Name,-20} {a.Version}")
                        .AddChoices(installed.Artifacts));
            }
            else
            {
                Console.Error.WriteLine("Specify an artifact name. Usage: agentsmd remove <name> [--yes]");
                return;
            }

            if (toRemove.Count == 0) return;

            if (interactive)
            {
                if (!AnsiConsole.Confirm($"Remove {toRemove.Count} artifact(s)?"))
                    return;
            }

            foreach (var artifact in toRemove)
            {
                UninstallArtifact(state, artifact.Name, artifact.Type);
                Cli.WriteSuccess($"Uninstalled {artifact.Type} '{artifact.Name}'.");
            }

            AutoSync(state, project);
        }, queryArg, yesOpt, ProjectOption);

        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // agentsmd update [--yes]
    // ═══════════════════════════════════════════════════════════════

    public static Command CreateUpdateCommand()
    {
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");
        var cmd = new Command("update", "Update installed artifacts to latest versions");
        cmd.AddOption(yesOpt);

        cmd.SetHandler(async (bool yes, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

            var installed = state.Load();
            if (installed.Artifacts.Count == 0)
            {
                Console.WriteLine("No artifacts installed.");
                return;
            }

            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var updatable = new List<(InstalledArtifact Installed, ArtifactEntry Latest)>();
            foreach (var inst in installed.Artifacts)
            {
                var latest = index.Artifacts.FirstOrDefault(a =>
                    a.Name.Equals(inst.Name, StringComparison.OrdinalIgnoreCase) &&
                    a.Type.Equals(inst.Type, StringComparison.OrdinalIgnoreCase));
                if (latest is not null && latest.Version != inst.Version)
                    updatable.Add((inst, latest));
            }

            if (updatable.Count == 0)
            {
                Cli.WriteSuccess("All artifacts are up to date.");
                return;
            }

            var table = Cli.CreateTable("Type", "Name", "Installed", "Available");
            foreach (var (inst, latest) in updatable)
                table.AddRow(
                    $"{Cli.TypeIcon(inst.Type)} {Markup.Escape(inst.Type)}",
                    $"[{Cli.Primary}]{Markup.Escape(inst.Name)}[/]",
                    $"[{Cli.Muted}]{Markup.Escape(inst.Version)}[/]",
                    $"[{Cli.Success}]{Markup.Escape(latest.Version)}[/]");
            AnsiConsole.Write(table);

            bool interactive = !yes && AnsiConsole.Profile.Capabilities.Interactive;
            List<(InstalledArtifact Installed, ArtifactEntry Latest)> toUpdate;

            if (interactive)
            {
                var labels = updatable.Select(u => $"{u.Latest.Type}: {u.Latest.Name}").ToList();
                var selected = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Select artifacts to update:")
                        .PageSize(15)
                        .AddChoices(labels));
                toUpdate = updatable.Where(u => selected.Contains($"{u.Latest.Type}: {u.Latest.Name}")).ToList();

                if (toUpdate.Count == 0) return;
                if (!AnsiConsole.Confirm($"Update {toUpdate.Count} artifact(s)?"))
                    return;
            }
            else
            {
                toUpdate = updatable;
            }

            foreach (var (_, latest) in toUpdate)
                await InstallArtifactAsync(client, state, index, latest);

            AutoSync(state, project);
            Cli.WriteSuccess($"Updated {toUpdate.Count} artifact(s).");
        }, yesOpt, ProjectOption);

        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // agentsmd list [--type] [--json]
    // ═══════════════════════════════════════════════════════════════

    public static Command CreateListCommand()
    {
        var typeOpt = new Option<string>(["--type", "-t"], () => "", "Filter by type (agent, skill, workflow)");
        var jsonOpt = new Option<bool>("--json", "Output as JSON");
        var cmd = new Command("list", "List installed artifacts");
        cmd.AddOption(typeOpt);
        cmd.AddOption(jsonOpt);

        cmd.SetHandler((string type, bool json, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

            var installed = state.Load();
            var artifacts = installed.Artifacts.AsEnumerable();

            if (!string.IsNullOrEmpty(type))
                artifacts = artifacts.Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            var list = artifacts.ToList();

            if (json)
            {
                var output = new InstalledState { Artifacts = list };
                Console.WriteLine(JsonSerializer.Serialize(output, AppJsonContext.Default.InstalledState));
                return;
            }

            if (list.Count == 0)
            {
                Console.WriteLine(!string.IsNullOrEmpty(type) ? $"No {type}s installed." : "No artifacts installed.");
                return;
            }

            Cli.WriteInstalledTable(list.Select(a => (a.Type, a.Name, a.Version)));
        }, typeOpt, jsonOpt, ProjectOption);

        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // agentsmd sync (unchanged)
    // ═══════════════════════════════════════════════════════════════

    public static Command CreateSyncCommand()
    {
        var cmd = new Command("sync", "Generate tool wrappers and copy skills");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

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

    // ═══════════════════════════════════════════════════════════════
    // agentsmd status (unchanged)
    // ═══════════════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════════════
    // agentsmd init (interactive wizard)
    // ═══════════════════════════════════════════════════════════════

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

            var client = new GitHubLibClient(Http);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            bool interactive = AnsiConsole.Profile.Capabilities.Interactive;

            if (json is not null)
            {
                var index = IndexParser.Parse(json);
                if (index is not null)
                {
                    var workflows = IndexParser.Search(index, "", filterType: "workflow");
                    if (workflows.Count > 0)
                    {
                        if (interactive)
                        {
                            AnsiConsole.WriteLine();
                            var choices = workflows.Select(w => w.Name).Append("(skip)").ToList();
                            var selected = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Select a workflow to start with:")
                                    .PageSize(10)
                                    .AddChoices(choices));

                            if (selected != "(skip)")
                            {
                                var workflow = workflows.First(w => w.Name == selected);
                                await InstallArtifactAsync(client, state, index, workflow);
                                AutoSync(state, project);
                                return;
                            }
                        }
                        else
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLine("[bold]Available workflows:[/]");
                            foreach (var wf in workflows)
                                AnsiConsole.MarkupLine($"  [{Cli.Muted}]-[/] [{Cli.Primary}]{Markup.Escape(wf.Name)}[/]: {Markup.Escape(wf.Description)}");
                            AnsiConsole.MarkupLine($"\n[{Cli.Muted}]Install a workflow:[/] [{Cli.Accent}]agentsmd add <name>[/]");
                        }
                    }
                }
            }

            AnsiConsole.WriteLine();
            Cli.WriteSuccess("Initialized. Next steps:");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]1.[/] agentsmd add <workflow>");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]2.[/] Start your AI agent");
        }, ProjectOption);
        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // DEPRECATED: agent / skill / workflow subcommands
    // Still functional but print deprecation hint
    // ═══════════════════════════════════════════════════════════════

    public static Command CreateAgentCommand()
    {
        var cmd = new Command("agent", "Manage agents (use 'add'/'remove' instead)");
        cmd.AddCommand(CreateTypeInstallCommand("agent"));
        cmd.AddCommand(CreateTypeUninstallCommand("agent"));
        cmd.AddCommand(CreateTypeListCommand("agent"));
        cmd.AddCommand(CreateTypeSearchCommand("agent"));
        cmd.AddCommand(CreateTypeInfoCommand("agent"));
        return cmd;
    }

    public static Command CreateSkillCommand()
    {
        var cmd = new Command("skill", "Manage skills (use 'add'/'remove' instead)");
        cmd.AddCommand(CreateTypeInstallCommand("skill"));
        cmd.AddCommand(CreateTypeUninstallCommand("skill"));
        cmd.AddCommand(CreateTypeListCommand("skill"));
        cmd.AddCommand(CreateTypeSearchCommand("skill"));
        cmd.AddCommand(CreateTypeInfoCommand("skill"));
        return cmd;
    }

    public static Command CreateWorkflowCommand()
    {
        var cmd = new Command("workflow", "Manage workflows (use 'add'/'remove' instead)");
        cmd.AddCommand(CreateTypeInstallCommand("workflow"));
        cmd.AddCommand(CreateTypeUninstallCommand("workflow"));
        cmd.AddCommand(CreateTypeListCommand("workflow"));
        cmd.AddCommand(CreateTypeSearchCommand("workflow"));
        cmd.AddCommand(CreateTypeInfoCommand("workflow"));
        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // Shared: Install, Uninstall, Info display
    // ═══════════════════════════════════════════════════════════════

    private static void ShowArtifactInfo(ArtifactEntry artifact)
    {
        AnsiConsole.MarkupLine($"  {Cli.TypeIcon(artifact.Type)} [{Cli.Primary}]{Markup.Escape(artifact.Name)}[/] v{Markup.Escape(artifact.Version)}");
        if (!string.IsNullOrEmpty(artifact.Description))
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]{Markup.Escape(artifact.Description)}[/]");
        if (artifact.Deps.Count > 0)
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]Dependencies: {Markup.Escape(string.Join(", ", artifact.Deps))}[/]");
    }

    private static async Task InstallArtifactAsync(ILibClient client, InstalledStateManager state, LibIndex index, ArtifactEntry artifact)
    {
        if (artifact.Type == "skill")
        {
            await InstallSkillAsync(client, state, artifact);
        }
        else
        {
            var content = await client.FetchFileAsync(artifact.Path);
            if (content is null) { Console.Error.WriteLine($"Could not fetch {artifact.Path}."); return; }

            var targetDir = state.GetArtifactDir(artifact.Type);
            var targetPath = Path.Combine(targetDir, Path.GetFileName(artifact.Path));
            await File.WriteAllTextAsync(targetPath, content);
        }

        state.AddArtifact(artifact.Name, artifact.Type, artifact.Version);
        Cli.WriteSuccess($"Installed {artifact.Type} '{artifact.Name}' v{artifact.Version}.");

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
    }

    private static async Task InstallSkillAsync(ILibClient client, InstalledStateManager state, ArtifactEntry artifact)
    {
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

    private static void UninstallArtifact(InstalledStateManager state, string name, string type)
    {
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
    }

    // ═══════════════════════════════════════════════════════════════
    // Deprecated typed subcommands (still functional)
    // ═══════════════════════════════════════════════════════════════

    private static Command CreateTypeInstallCommand(string type)
    {
        var nameArg = new Argument<string>("name", $"Name of the {type} to install");
        var cmd = new Command("install", $"Install a {type} from lib") { nameArg };
        cmd.SetHandler(async (string name, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

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

            await InstallArtifactAsync(client, state, index, artifact);
            AutoSync(state, project);

            WriteDeprecationHint("agentsmd add");
        }, nameArg, ProjectOption);
        return cmd;
    }

    private static Command CreateTypeUninstallCommand(string type)
    {
        var nameArg = new Argument<string>("name", $"Name of the {type} to uninstall");
        var cmd = new Command("uninstall", $"Uninstall a {type}") { nameArg };
        cmd.SetHandler((string name, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

            var existing = state.GetArtifact(name, type);
            if (existing is null)
            {
                Console.Error.WriteLine($"{type} '{name}' is not installed.");
                return;
            }

            UninstallArtifact(state, name, type);
            Cli.WriteSuccess($"Uninstalled {type} '{name}'.");
            AutoSync(state, project);

            WriteDeprecationHint("agentsmd remove");
        }, nameArg, ProjectOption);
        return cmd;
    }

    private static Command CreateTypeListCommand(string type)
    {
        var cmd = new Command("list", $"List installed {type}s");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'agentsmd init' first.");
                return;
            }

            var installed = state.Load().Artifacts.Where(a => a.Type == type).ToList();
            if (installed.Count == 0) { Console.WriteLine($"No {type}s installed."); return; }

            var table = Cli.CreateTable("Name", "Version");
            foreach (var a in installed)
                table.AddRow(
                    $"[{Cli.Primary}]{Markup.Escape(a.Name)}[/]",
                    $"[{Cli.Muted}]{Markup.Escape(a.Version)}[/]");
            AnsiConsole.Write(table);

            WriteDeprecationHint("agentsmd list --type " + type);
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

            WriteDeprecationHint("agentsmd add --type " + type);
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

            var state = new InstalledStateManager(project);
            if (state.IsInitialized)
            {
                var installed = state.GetArtifact(name, type);
                sb.Append(installed is not null
                    ? $"[bold]Status:[/]      [{Cli.Success}]installed (v{Markup.Escape(installed.Version)})[/]"
                    : $"[bold]Status:[/]      [{Cli.Muted}]not installed[/]");
            }

            Cli.WritePanel($"{Cli.TypeIcon(type)} {artifact.Name}", sb.ToString());

            WriteDeprecationHint("agentsmd add");
        }, nameArg, ProjectOption);
        return cmd;
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static void AutoSync(InstalledStateManager state, string project)
    {
        var options = DetectTools(project);
        var engine = new SyncEngine(state, project);
        var result = engine.Sync(options);
        Cli.WriteSuccess($"Synced: {result.GeneratedFiles.Count} wrapper(s), {result.CopiedSkills.Count} skill(s) copied.");
    }

    private static (string type, string name)? ParseDep(string dep)
    {
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
        Copilot = Directory.Exists(Path.Combine(project, ".github")) || true,
        ClaudeCode = true,
        OpenCode = true
    };

    private static void WriteDeprecationHint(string alternative)
    {
        AnsiConsole.MarkupLine($"\n[{Cli.Muted}]💡 Tip: Use '{Markup.Escape(alternative)}' for an interactive experience.[/]");
    }
}
