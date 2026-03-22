using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Linxmd.Models;
using Linxmd.Parsing;
using Linxmd.Services;
using Spectre.Console;

namespace Linxmd.Commands;

public static class CommandFactory
{
    private static readonly HttpClient Http = new();

    public static readonly Option<string> ProjectOption =
        new(["--project", "-p"], () => Directory.GetCurrentDirectory(), "Project root directory");

    public static Command CreateAddCommand()
    {
        var queryArg = new Argument<string>("query", () => "", "Artifact id (type:name) or search query");
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");
        var sourceOpt = new Option<string>("--source", () => "default", "Source id from .linxmd/sources.json");
        var cmd = new Command("add", "Browse, search and install artifacts") { queryArg };
        cmd.AddOption(yesOpt);
        cmd.AddOption(sourceOpt);

        cmd.SetHandler(async (string query, bool yes, string source, string project) =>
        {
            var state = new InstalledStateManager(project);
            state.EnsureDefaultSources();

            var selectedSource = state.GetSource(source);
            if (selectedSource is null)
            {
                Console.Error.WriteLine($"Unknown source '{source}'. Check .linxmd/sources.json.");
                return;
            }

            var client = CreateClient(selectedSource);
            var json = await Cli.SpinAsync("Fetching library index...", () => client.FetchIndexAsync());
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            bool interactive = !yes && AnsiConsole.Profile.Capabilities.Interactive;
            var parsedId = ParseArtifactId(query);
            string? filterType = parsedId?.type;
            string lookup = parsedId?.name ?? query;

            if (string.IsNullOrEmpty(lookup) && interactive)
            {
                lookup = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold]Search library:[/]")
                        .AllowEmpty());
            }

            var results = IndexParser.Search(index, lookup, filterType: filterType);
            if (results.Count == 0)
            {
                Console.WriteLine("No results found.");
                if (!string.IsNullOrEmpty(lookup))
                {
                    var suggestion = Cli.FindClosestMatch(lookup, index.Artifacts.Select(a => $"{a.Type}:{a.Name}"));
                    if (suggestion is not null)
                        AnsiConsole.MarkupLine($"[{Cli.Muted}]Did you mean [{Cli.Accent}]{Markup.Escape(suggestion)}[/]?[/]");
                }
                return;
            }

            ArtifactEntry? selected = null;
            var exact = results.Where(a =>
                    a.Name.Equals(lookup, StringComparison.OrdinalIgnoreCase) &&
                    (filterType is null || a.Type.Equals(filterType, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (parsedId is not null)
                exact = results.Where(a => a.Name.Equals(parsedId.Value.name, StringComparison.OrdinalIgnoreCase)
                                           && a.Type.Equals(parsedId.Value.type, StringComparison.OrdinalIgnoreCase)).ToList();

            if (exact.Count == 1)
            {
                selected = exact[0];
            }
            else if (exact.Count > 1)
            {
                if (yes)
                {
                    Console.Error.WriteLine($"Multiple matches for '{lookup}'. Use typed id, e.g. agent:{lookup}");
                    foreach (var m in exact)
                        Console.Error.WriteLine($"  {m.Type}:{m.Name}");
                    return;
                }

                if (interactive)
                {
                    selected = AnsiConsole.Prompt(
                        new SelectionPrompt<ArtifactEntry>()
                            .Title("Multiple matches found:")
                            .PageSize(10)
                            .UseConverter(a => $"{Cli.TypeIcon(a.Type)} {a.Type,-10} {a.Name,-20} {a.Version}")
                            .AddChoices(exact));
                }
                else
                {
                    Cli.WriteArtifactTable(exact.Select(a => (a.Type, a.Name, a.Version, a.Description)));
                    return;
                }
            }
            else if (interactive)
            {
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
                Cli.WriteArtifactTable(results.Select(a => (a.Type, a.Name, a.Version, a.Description)));
                return;
            }

            if (selected is null) return;

            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'linxmd init' first.");
                return;
            }

            ShowArtifactInfo(selected);

            if (interactive)
            {
                var depsMsg = selected.Deps.Count > 0
                    ? $" This will also install {selected.Deps.Count} dependencies."
                    : "";
                if (!AnsiConsole.Confirm($"Install [bold]{Markup.Escape(selected.Type + ":" + selected.Name)}[/]?{depsMsg}"))
                    return;
            }

            await InstallArtifactAsync(client, state, index, selected, selectedSource.Id);
            AutoSync(state, project);
        }, queryArg, yesOpt, sourceOpt, ProjectOption);

        return cmd;
    }

    public static Command CreateRemoveCommand()
    {
        var queryArg = new Argument<string>("query", () => "", "Artifact id (type:name) or name");
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");
        var cmd = new Command("remove", "Browse installed artifacts and uninstall") { queryArg };
        cmd.AddOption(yesOpt);

        cmd.SetHandler(async (string query, bool yes, string project) =>
        {
            var state = new InstalledStateManager(project);
            state.EnsureDefaultSources();
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'linxmd init' first.");
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
                var parsed = ParseArtifactId(query);
                toRemove = parsed is null
                    ? installed.Artifacts.Where(a => a.Name.Equals(query, StringComparison.OrdinalIgnoreCase)).ToList()
                    : installed.Artifacts.Where(a =>
                        a.Name.Equals(parsed.Value.name, StringComparison.OrdinalIgnoreCase) &&
                        a.Type.Equals(parsed.Value.type, StringComparison.OrdinalIgnoreCase)).ToList();

                if (toRemove.Count == 0)
                {
                    Console.Error.WriteLine($"'{query}' is not installed.");
                    return;
                }
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
                Console.Error.WriteLine("Specify an artifact id. Usage: linxmd remove <type:name> [--yes]");
                return;
            }

            if (toRemove.Count == 0) return;

            var blockers = await FindRemovalBlockersAsync(state, installed, toRemove);
            if (blockers.Count > 0)
            {
                Console.Error.WriteLine("Cannot uninstall due to active dependencies:");
                foreach (var (target, dependent) in blockers)
                    Console.Error.WriteLine($"  {dependent.Type}:{dependent.Name} depends on {target.Type}:{target.Name}");
                return;
            }

            if (interactive && !AnsiConsole.Confirm($"Remove {toRemove.Count} artifact(s)?"))
                return;

            foreach (var artifact in toRemove)
            {
                UninstallArtifact(state, artifact.Name, artifact.Type);
                Cli.WriteSuccess($"Uninstalled {artifact.Type} '{artifact.Name}'.");
            }

            AutoSync(state, project);
        }, queryArg, yesOpt, ProjectOption);

        return cmd;
    }

    public static Command CreateUpdateCommand()
    {
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");
        var cmd = new Command("update", "Update installed artifacts to latest versions");
        cmd.AddOption(yesOpt);

        cmd.SetHandler(async (bool yes, string project) =>
        {
            var state = new InstalledStateManager(project);
            state.EnsureDefaultSources();
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'linxmd init' first.");
                return;
            }

            var installed = state.Load();
            if (installed.Artifacts.Count == 0)
            {
                Console.WriteLine("No artifacts installed.");
                return;
            }

            var updatable = new List<(InstalledArtifact Installed, ArtifactEntry Latest, ILibClient Client)>();
            var sourceCache = new Dictionary<string, (ILibClient client, LibIndex index)>();

            foreach (var inst in installed.Artifacts)
            {
                var sourceId = string.IsNullOrWhiteSpace(inst.SourceId) ? "default" : inst.SourceId;
                if (!sourceCache.TryGetValue(sourceId, out var pair))
                {
                    var source = state.GetSource(sourceId);
                    if (source is null)
                    {
                        Console.Error.WriteLine($"Source '{sourceId}' for {inst.Type}:{inst.Name} is missing.");
                        continue;
                    }

                    var client = CreateClient(source);
                    var json = await client.FetchIndexAsync();
                    if (json is null) continue;
                    var index = IndexParser.Parse(json);
                    if (index is null) continue;
                    pair = (client, index);
                    sourceCache[sourceId] = pair;
                }

                var latest = pair.index.Artifacts.FirstOrDefault(a =>
                    a.Name.Equals(inst.Name, StringComparison.OrdinalIgnoreCase) &&
                    a.Type.Equals(inst.Type, StringComparison.OrdinalIgnoreCase));
                if (latest is not null && latest.Version != inst.Version)
                    updatable.Add((inst, latest, pair.client));
            }

            if (updatable.Count == 0)
            {
                Cli.WriteSuccess("All artifacts are up to date.");
                return;
            }

            var table = Cli.CreateTable("Type", "Name", "Installed", "Available", "Source");
            foreach (var (inst, latest, _) in updatable)
                table.AddRow(
                    $"{Cli.TypeIcon(inst.Type)} {Markup.Escape(inst.Type)}",
                    $"[{Cli.Primary}]{Markup.Escape(inst.Name)}[/]",
                    $"[{Cli.Muted}]{Markup.Escape(inst.Version)}[/]",
                    $"[{Cli.Success}]{Markup.Escape(latest.Version)}[/]",
                    $"[{Cli.Muted}]{Markup.Escape(inst.SourceId)}[/]");
            AnsiConsole.Write(table);

            bool interactive = !yes && AnsiConsole.Profile.Capabilities.Interactive;
            var toUpdate = updatable;

            if (interactive)
            {
                var labels = updatable.Select(u => $"{u.Installed.Type}:{u.Installed.Name}@{u.Installed.SourceId}").ToList();
                var selected = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Select artifacts to update:")
                        .PageSize(15)
                        .AddChoices(labels));

                toUpdate = updatable.Where(u => selected.Contains($"{u.Installed.Type}:{u.Installed.Name}@{u.Installed.SourceId}")).ToList();
                if (toUpdate.Count == 0) return;
                if (!AnsiConsole.Confirm($"Update {toUpdate.Count} artifact(s)?"))
                    return;
            }

            foreach (var (inst, latest, client) in toUpdate)
            {
                var sourceId = string.IsNullOrWhiteSpace(inst.SourceId) ? "default" : inst.SourceId;
                var sourceIndex = sourceCache[sourceId].index;
                await InstallArtifactAsync(client, state, sourceIndex, latest, sourceId);
            }

            AutoSync(state, project);
            Cli.WriteSuccess($"Updated {toUpdate.Count} artifact(s).");
        }, yesOpt, ProjectOption);

        return cmd;
    }

    public static Command CreateListCommand()
    {
        var filterArg = new Argument<string>("filter", () => "", "Optional type (agent|skill|workflow) or typed id (type:name)");
        var jsonOpt = new Option<bool>("--json", "Output as JSON");
        var cmd = new Command("list", "List installed artifacts") { filterArg };
        cmd.AddOption(jsonOpt);

        cmd.SetHandler((string filter, bool json, string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'linxmd init' first.");
                return;
            }

            var installed = state.Load();
            var artifacts = installed.Artifacts.AsEnumerable();
            var parsed = ParseArtifactId(filter);

            if (parsed is not null)
                artifacts = artifacts.Where(a => a.Type.Equals(parsed.Value.type, StringComparison.OrdinalIgnoreCase) &&
                                                 a.Name.Equals(parsed.Value.name, StringComparison.OrdinalIgnoreCase));
            else if (!string.IsNullOrWhiteSpace(filter))
                artifacts = artifacts.Where(a => a.Type.Equals(filter, StringComparison.OrdinalIgnoreCase));

            var list = artifacts.ToList();
            if (json)
            {
                var output = new InstalledState { Artifacts = list };
                Console.WriteLine(JsonSerializer.Serialize(output, AppJsonContext.Default.InstalledState));
                return;
            }

            if (list.Count == 0)
            {
                Console.WriteLine(string.IsNullOrWhiteSpace(filter) ? "No artifacts installed." : $"No entries for '{filter}'.");
                return;
            }

            Cli.WriteInstalledTable(list.Select(a => (a.Type, a.Name, a.Version)));
        }, filterArg, jsonOpt, ProjectOption);

        return cmd;
    }

    public static Command CreateSyncCommand()
    {
        var cmd = new Command("sync", "Generate tool wrappers and copy skills");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.Error.WriteLine("Not initialized. Run 'linxmd init' first.");
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

    public static Command CreateStatusCommand()
    {
        var cmd = new Command("status", "Show project overview");
        cmd.SetHandler((string project) =>
        {
            var state = new InstalledStateManager(project);
            if (!state.IsInitialized)
            {
                Console.WriteLine("linxmd: not initialized");
                Console.WriteLine("Run 'linxmd init' to set up.");
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

            Cli.WritePanel("linxmd status", sb.ToString());
        }, ProjectOption);
        return cmd;
    }

    public static Command CreateInitCommand()
    {
        var cmd = new Command("init", "Initialize linxmd in project");
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
            Cli.WriteSuccess("Created .linxmd/ structure.");

            var source = state.GetSource("default")!;
            var client = CreateClient(source);
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
                                await InstallArtifactAsync(client, state, index, workflow, "default");
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
                            AnsiConsole.MarkupLine($"\n[{Cli.Muted}]Install a workflow:[/] [{Cli.Accent}]linxmd add workflow:<name>[/]");
                        }
                    }
                }
            }

            AnsiConsole.WriteLine();
            Cli.WriteSuccess("Initialized. Next steps:");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]1.[/] linxmd add workflow:<name>");
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]2.[/] Start your AI agent");
        }, ProjectOption);
        return cmd;
    }

    private static void ShowArtifactInfo(ArtifactEntry artifact)
    {
        AnsiConsole.MarkupLine($"  {Cli.TypeIcon(artifact.Type)} [{Cli.Primary}]{Markup.Escape(artifact.Name)}[/] v{Markup.Escape(artifact.Version)}");
        if (!string.IsNullOrEmpty(artifact.Description))
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]{Markup.Escape(artifact.Description)}[/]");
        if (artifact.Deps.Count > 0)
            AnsiConsole.MarkupLine($"  [{Cli.Muted}]Dependencies: {Markup.Escape(string.Join(", ", artifact.Deps))}[/]");
    }

    private static ILibClient CreateClient(LibSource source)
    {
        if (!source.Kind.Equals("github", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Source kind '{source.Kind}' is not supported yet.");

        return new GitHubLibClient(Http, source.Owner, source.Repo, source.Branch, source.BasePath);
    }

    private static async Task InstallArtifactAsync(ILibClient client, InstalledStateManager state, LibIndex index, ArtifactEntry artifact, string sourceId)
    {
        string? checksum = null;
        if (artifact.Type == "skill")
        {
            await InstallSkillAsync(client, state, artifact);
        }
        else
        {
            var content = await client.FetchFileAsync(artifact.Path);
            if (content is null) { Console.Error.WriteLine($"Could not fetch {artifact.Path}."); return; }

            var targetDir = state.GetArtifactDir(artifact.Type);
            Directory.CreateDirectory(targetDir);
            var targetPath = Path.Combine(targetDir, Path.GetFileName(artifact.Path));
            await File.WriteAllTextAsync(targetPath, content);
            checksum = Hash(content);
        }

        state.AddArtifact(artifact.Name, artifact.Type, artifact.Version, sourceId, artifact.Path, checksum);
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

                string? depChecksum = null;
                if (depEntry.Type == "skill")
                {
                    await InstallSkillAsync(client, state, depEntry);
                }
                else
                {
                    var depContent = await client.FetchFileAsync(depEntry.Path);
                    if (depContent is null) continue;
                    var targetDir = state.GetArtifactDir(depEntry.Type);
                    Directory.CreateDirectory(targetDir);
                    await File.WriteAllTextAsync(Path.Combine(targetDir, Path.GetFileName(depEntry.Path)), depContent);
                    depChecksum = Hash(depContent);
                }

                state.AddArtifact(depEntry.Name, depEntry.Type, depEntry.Version, sourceId, depEntry.Path, depChecksum);
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

    private static async Task<List<(InstalledArtifact target, InstalledArtifact dependent)>> FindRemovalBlockersAsync(
        InstalledStateManager state,
        InstalledState installed,
        List<InstalledArtifact> toRemove)
    {
        var blockers = new List<(InstalledArtifact target, InstalledArtifact dependent)>();
        var removeSet = toRemove.Select(a => $"{a.Type}:{a.Name}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var indexCache = new Dictionary<string, LibIndex>(StringComparer.OrdinalIgnoreCase);

        foreach (var dependent in installed.Artifacts)
        {
            var dependentId = $"{dependent.Type}:{dependent.Name}";
            if (removeSet.Contains(dependentId))
                continue;

            var sourceId = string.IsNullOrWhiteSpace(dependent.SourceId) ? "default" : dependent.SourceId;
            if (!indexCache.TryGetValue(sourceId, out var sourceIndex))
            {
                var source = state.GetSource(sourceId);
                if (source is null)
                    continue;

                var client = CreateClient(source);
                var json = await client.FetchIndexAsync();
                if (json is null) continue;
                sourceIndex = IndexParser.Parse(json) ?? new LibIndex();
                indexCache[sourceId] = sourceIndex;
            }

            var depEntry = sourceIndex.Artifacts.FirstOrDefault(a =>
                a.Name.Equals(dependent.Name, StringComparison.OrdinalIgnoreCase) &&
                a.Type.Equals(dependent.Type, StringComparison.OrdinalIgnoreCase));

            if (depEntry is null) continue;

            var depIds = depEntry.Deps
                .Select(ParseDep)
                .Where(x => x is not null)
                .Select(x => $"{x!.Value.type}:{x.Value.name}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var target in toRemove)
            {
                var targetId = $"{target.Type}:{target.Name}";
                if (depIds.Contains(targetId))
                    blockers.Add((target, dependent));
            }
        }

        return blockers;
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

    private static (string type, string name)? ParseArtifactId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var colonIndex = value.IndexOf(':');
        if (colonIndex <= 0 || colonIndex >= value.Length - 1) return null;
        var type = value[..colonIndex].Trim().ToLowerInvariant();
        var name = value[(colonIndex + 1)..].Trim();
        if (string.IsNullOrEmpty(name)) return null;
        return (type, name);
    }

    private static string Hash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static SyncOptions DetectTools(string project) => new()
    {
        Copilot = Directory.Exists(Path.Combine(project, ".github")) || true,
        ClaudeCode = true,
        OpenCode = true
    };
}