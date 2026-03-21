using Agentsmd.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Agentsmd.Parsing;

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

            return new FrontMatter
            {
                Name = raw.Name,
                Type = artifactType,
                Version = raw.Version,
                Description = raw.Description ?? string.Empty,
                Deps = raw.Deps ?? [],
                Tags = raw.Tags ?? []
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

    private sealed class RawFrontMatter
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public List<string>? Deps { get; set; }
        public List<string>? Tags { get; set; }
    }
}
