namespace Linxmd.Models;

public sealed class FrontMatter
{
    public required string Name { get; init; }
    public required ArtifactType Type { get; init; }
    public required string Version { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<string> Deps { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> Supported { get; init; } = [];
    public List<QuickAction> QuickActions { get; init; } = [];
    public ArtifactLifecycle Lifecycle { get; init; } = new();
}

public sealed class QuickAction
{
    public string Id { get; init; } = string.Empty;
    public required string Label { get; init; }
    public required string Prompt { get; init; }
    public QuickActionTrigger Trigger { get; init; } = new();
}

public sealed class QuickActionTrigger
{
    public List<string> FileMatch { get; init; } = [];
    public List<string> FileExclude { get; init; } = [];
    public List<string> WorkspaceHas { get; init; } = [];
    public List<string> WorkspaceMissing { get; init; } = [];
    public List<string> LanguageId { get; init; } = [];
    public List<string> ContentMatch { get; init; } = [];
}

public sealed class ArtifactLifecycle
{
    public List<LifecycleHook> PreInstall { get; init; } = [];
    public List<LifecycleHook> PostInstall { get; init; } = [];
    public List<LifecycleHook> PreUninstall { get; init; } = [];
    public List<LifecycleHook> PostUninstall { get; init; } = [];
    public List<LifecycleHook> PreUpdate { get; init; } = [];
    public List<LifecycleHook> PostUpdate { get; init; } = [];
}

public sealed class LifecycleHook
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public bool Blocking { get; init; }
    public bool RequiresConfirmation { get; init; }
}
