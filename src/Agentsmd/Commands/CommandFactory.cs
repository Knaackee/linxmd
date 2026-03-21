using System.CommandLine;
using Agentsmd.Services;
using Agentsmd.Parsing;

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
            var json = await client.FetchIndexAsync();
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var results = IndexParser.Search(index, query);
            if (results.Count == 0) { Console.WriteLine("No results found."); return; }

            foreach (var a in results)
                Console.WriteLine($"  {a.Type,-10} {a.Name,-25} {a.Version,-10} {a.Description}");
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

            foreach (var a in installed.Artifacts)
                Console.WriteLine($"  {a.Type,-10} {a.Name,-25} {a.Version}");
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

            Console.WriteLine($"Synced: {result.GeneratedFiles.Count} wrapper(s), {result.CopiedSkills.Count} skill(s) copied.");
            foreach (var f in result.GeneratedFiles)
                Console.WriteLine($"  → {Path.GetRelativePath(project, f)}");
            foreach (var s in result.CopiedSkills)
                Console.WriteLine($"  → .claude/skills/{s}/");
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
            Console.WriteLine("agentsmd status");
            Console.WriteLine($"  Project: {project}");
            Console.WriteLine($"  Agents:    {installed.Artifacts.Count(a => a.Type == "agent")}");
            Console.WriteLine($"  Skills:    {installed.Artifacts.Count(a => a.Type == "skill")}");
            Console.WriteLine($"  Workflows: {installed.Artifacts.Count(a => a.Type == "workflow")}");

            var backlogDir = state.BacklogDir;
            var inProgressDir = state.InProgressDir;
            var backlogCount = Directory.Exists(backlogDir) ? Directory.GetFiles(backlogDir, "*.md").Length : 0;
            var inProgressCount = Directory.Exists(inProgressDir) ? Directory.GetDirectories(inProgressDir).Length : 0;
            Console.WriteLine($"  Backlog:       {backlogCount}");
            Console.WriteLine($"  In Progress:   {inProgressCount}");

            // tools
            var tools = new List<string>();
            if (Directory.Exists(Path.Combine(project, ".github", "agents"))) tools.Add("copilot");
            if (Directory.Exists(Path.Combine(project, ".claude", "agents"))) tools.Add("claude-code");
            if (Directory.Exists(Path.Combine(project, ".opencode", "agents"))) tools.Add("opencode");
            Console.WriteLine($"  Tools: {(tools.Count > 0 ? string.Join(", ", tools) : "none")}");
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

            state.EnsureDirectories();
            Console.WriteLine("Created .agentsmd/ structure.");

            // Fetch index and show available workflows
            var client = new GitHubLibClient(Http);
            var json = await client.FetchIndexAsync();
            if (json is not null)
            {
                var index = IndexParser.Parse(json);
                if (index is not null)
                {
                    var workflows = IndexParser.Search(index, "", filterType: "workflow");
                    if (workflows.Count > 0)
                    {
                        Console.WriteLine("\nAvailable workflows:");
                        foreach (var wf in workflows)
                            Console.WriteLine($"  - {wf.Name}: {wf.Description}");
                        Console.WriteLine($"\nInstall a workflow: agentsmd workflow install <name>");
                    }
                }
            }

            Console.WriteLine("\n✓ Initialized. Next steps:");
            Console.WriteLine("  1. agentsmd workflow install sdd-tdd");
            Console.WriteLine("  2. agentsmd sync");
            Console.WriteLine("  3. Start your AI agent with the kick-off prompt");
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
            var json = await client.FetchIndexAsync();
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var artifact = index.Artifacts.FirstOrDefault(a =>
                a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                a.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (artifact is null)
            {
                Console.Error.WriteLine($"{type} '{name}' not found in lib.");
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
            Console.WriteLine($"Installed {type} '{artifact.Name}' v{artifact.Version}.");

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
                    Console.WriteLine($"  Installed {depEntry.Type} '{depEntry.Name}' v{depEntry.Version}.");
                }
            }
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
            Console.WriteLine($"Uninstalled {type} '{name}'.");
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

            foreach (var a in installed)
                Console.WriteLine($"  {a.Name,-25} {a.Version}");
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
            var json = await client.FetchIndexAsync();
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var results = IndexParser.Search(index, query, filterType: type);
            if (results.Count == 0) { Console.WriteLine("No results found."); return; }

            foreach (var a in results)
                Console.WriteLine($"  {a.Name,-25} {a.Version,-10} {a.Description}");
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
            var json = await client.FetchIndexAsync();
            if (json is null) { Console.Error.WriteLine("Could not fetch lib index."); return; }

            var index = IndexParser.Parse(json);
            if (index is null) { Console.Error.WriteLine("Invalid index format."); return; }

            var artifact = index.Artifacts.FirstOrDefault(a =>
                a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                a.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (artifact is null)
            {
                Console.Error.WriteLine($"{type} '{name}' not found in lib.");
                return;
            }

            Console.WriteLine($"Name:        {artifact.Name}");
            Console.WriteLine($"Type:        {artifact.Type}");
            Console.WriteLine($"Version:     {artifact.Version}");
            Console.WriteLine($"Description: {artifact.Description}");
            Console.WriteLine($"Path:        {artifact.Path}");
            if (artifact.Deps.Count > 0)
            {
                Console.WriteLine("Dependencies:");
                foreach (var dep in artifact.Deps)
                    Console.WriteLine($"  - {dep}");
            }
            if (artifact.Tags.Count > 0)
                Console.WriteLine($"Tags:        {string.Join(", ", artifact.Tags)}");

            // Check if installed
            var state = new InstalledStateManager(project);
            if (state.IsInitialized)
            {
                var installed = state.GetArtifact(name, type);
                Console.WriteLine(installed is not null
                    ? $"Status:      installed (v{installed.Version})"
                    : "Status:      not installed");
            }
        }, nameArg, ProjectOption);
        return cmd;
    }

    // ──── Helpers ────

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
