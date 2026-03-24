using Linxmd.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Linxmd.Parsing;

public static class FrontMatterParser
{
    private const string Delimiter = "---";

    public static FrontMatter? Parse(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return null;

        var yaml = ExtractYaml(markdown);
        if (yaml is null)
            return null;

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var raw = deserializer.Deserialize<RawFrontMatter>(yaml);

            if (string.IsNullOrWhiteSpace(raw?.Name) ||
                string.IsNullOrWhiteSpace(raw.Version) ||
                string.IsNullOrWhiteSpace(raw.Type))
                return null;

            if (!Enum.TryParse<ArtifactType>(raw.Type, ignoreCase: true, out var artifactType))
                return null;

            if (!HasValidQuickActions(raw.QuickActions))
                return null;

            return new FrontMatter
            {
                Name = raw.Name,
                Type = artifactType,
                Version = raw.Version,
                Description = raw.Description ?? string.Empty,
                Deps = raw.Deps ?? [],
                Tags = raw.Tags ?? [],
                Supported = raw.Supported ?? [],
                QuickActions = raw.QuickActions?.Select(MapQuickAction).ToList() ?? [],
                Lifecycle = MapLifecycle(raw.Lifecycle)
            };
        }
        catch
        {
            return null;
        }
    }

    public static string ExtractBody(string markdown)
    {
        var trimmed = markdown.TrimStart();
        if (!trimmed.StartsWith(Delimiter))
            return markdown;

        var endIndex = trimmed.IndexOf(Delimiter, Delimiter.Length, StringComparison.Ordinal);
        if (endIndex < 0)
            return markdown;

        return trimmed[(endIndex + Delimiter.Length)..].TrimStart('\r', '\n');
    }

    private static string? ExtractYaml(string markdown)
    {
        var trimmed = markdown.TrimStart();
        if (!trimmed.StartsWith(Delimiter))
            return null;

        var startIndex = Delimiter.Length;
        var endIndex = trimmed.IndexOf(Delimiter, startIndex, StringComparison.Ordinal);
        if (endIndex < 0)
            return null;

        return trimmed[startIndex..endIndex].Trim();
    }

    private static QuickAction MapQuickAction(RawQuickAction raw)
    {
        return new QuickAction
        {
            Id = raw.Id ?? string.Empty,
            Label = raw.Label!,
            Prompt = raw.Prompt!,
            Trigger = new QuickActionTrigger
            {
                FileMatch = raw.Trigger?.FileMatch ?? [],
                FileExclude = raw.Trigger?.FileExclude ?? [],
                WorkspaceHas = raw.Trigger?.WorkspaceHas ?? [],
                WorkspaceMissing = raw.Trigger?.WorkspaceMissing ?? [],
                LanguageId = raw.Trigger?.LanguageId ?? [],
                ContentMatch = raw.Trigger?.ContentMatch ?? []
            }
        };
    }

    private static bool HasValidQuickActions(List<RawQuickAction>? quickActions)
    {
        if (quickActions is null)
            return true;

        return quickActions.All(q =>
            !string.IsNullOrWhiteSpace(q.Label) &&
            !string.IsNullOrWhiteSpace(q.Prompt) &&
            q.Trigger?.FileMatch is not null &&
            q.Trigger.FileMatch.Any(m => !string.IsNullOrWhiteSpace(m)));
    }

    private static ArtifactLifecycle MapLifecycle(RawArtifactLifecycle? raw)
    {
        if (raw is null)
            return new ArtifactLifecycle();

        return new ArtifactLifecycle
        {
            PreInstall = MapHooks(raw.PreInstall),
            PostInstall = MapHooks(raw.PostInstall),
            PreUninstall = MapHooks(raw.PreUninstall),
            PostUninstall = MapHooks(raw.PostUninstall),
            PreUpdate = MapHooks(raw.PreUpdate),
            PostUpdate = MapHooks(raw.PostUpdate)
        };
    }

    private static List<LifecycleHook> MapHooks(List<RawLifecycleHook>? raw)
    {
        if (raw is null)
            return [];

        return raw
            .Where(h => !string.IsNullOrWhiteSpace(h.Prompt) || !string.IsNullOrWhiteSpace(h.Label) || !string.IsNullOrWhiteSpace(h.Id))
            .Select(h => new LifecycleHook
            {
                Id = h.Id ?? string.Empty,
                Label = h.Label ?? string.Empty,
                Prompt = h.Prompt ?? string.Empty,
                Blocking = h.Blocking,
                RequiresConfirmation = h.RequiresConfirmation
            })
            .ToList();
    }

    private sealed class RawFrontMatter
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public List<string>? Deps { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Supported { get; set; }
        public List<RawQuickAction>? QuickActions { get; set; }
        public RawArtifactLifecycle? Lifecycle { get; set; }
    }

    private sealed class RawQuickAction
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Prompt { get; set; }
        public RawQuickActionTrigger? Trigger { get; set; }
    }

    private sealed class RawQuickActionTrigger
    {
        public List<string>? FileMatch { get; set; }
        public List<string>? FileExclude { get; set; }
        public List<string>? WorkspaceHas { get; set; }
        public List<string>? WorkspaceMissing { get; set; }
        public List<string>? LanguageId { get; set; }
        public List<string>? ContentMatch { get; set; }
    }

    private sealed class RawArtifactLifecycle
    {
        public List<RawLifecycleHook>? PreInstall { get; set; }
        public List<RawLifecycleHook>? PostInstall { get; set; }
        public List<RawLifecycleHook>? PreUninstall { get; set; }
        public List<RawLifecycleHook>? PostUninstall { get; set; }
        public List<RawLifecycleHook>? PreUpdate { get; set; }
        public List<RawLifecycleHook>? PostUpdate { get; set; }
    }

    private sealed class RawLifecycleHook
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Prompt { get; set; }
        public bool Blocking { get; set; }
        public bool RequiresConfirmation { get; set; }
    }
}
