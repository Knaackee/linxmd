using System.Text.Json;
using System.Text.Json.Serialization;
using Linxmd.Models;

namespace Linxmd.Parsing;

public static class IndexParser
{
    public static LibIndex? Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize(json, AppJsonContext.Default.LibIndex);
        }
        catch
        {
            return null;
        }
    }

    public static string Serialize(LibIndex index)
    {
        return JsonSerializer.Serialize(index, AppJsonContext.Default.LibIndex);
    }

    public static List<ArtifactEntry> Search(LibIndex index, string query, string? filterType = null, bool includeInternal = false)
    {
        var results = index.Artifacts.AsEnumerable();

        if (!includeInternal)
            results = results.Where(a => !a.Internal);

        if (!string.IsNullOrWhiteSpace(filterType))
            results = results.Where(a => a.Type.Equals(filterType, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(query))
            return results.ToList();

        return results.Where(a =>
            a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            a.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            a.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }
}
