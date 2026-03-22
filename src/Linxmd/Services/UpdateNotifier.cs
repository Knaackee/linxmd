using System.Reflection;
using System.Text.Json;

namespace Linxmd.Services;

public static class UpdateNotifier
{
    private sealed class UpdateCache
    {
        public DateTimeOffset CheckedAt { get; set; }
        public string LatestVersion { get; set; } = string.Empty;
    }

    public static async Task TryNotifyAsync()
    {
        if (Environment.GetEnvironmentVariable("LINXMD_NO_UPDATE_CHECK") == "1")
            return;

        try
        {
            var cacheFile = GetCacheFilePath();
            var cache = await LoadCacheAsync(cacheFile);
            if (cache is not null && DateTimeOffset.UtcNow - cache.CheckedAt < TimeSpan.FromHours(24))
            {
                NotifyIfNewer(cache.LatestVersion);
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(2000));
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("linxmd-cli/0.1");

            var url = "https://api.github.com/repos/Knaackee/linxmd/releases/latest";
            var response = await http.GetAsync(url, cts.Token);
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            var doc = JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString() ?? string.Empty;
            var latest = NormalizeVersion(tag);
            if (string.IsNullOrEmpty(latest)) return;

            await SaveCacheAsync(cacheFile, new UpdateCache
            {
                CheckedAt = DateTimeOffset.UtcNow,
                LatestVersion = latest
            });

            NotifyIfNewer(latest);
        }
        catch
        {
            // Update check is best-effort and must never block CLI usage.
        }
    }

    private static void NotifyIfNewer(string latest)
    {
        var current = GetCurrentVersion();
        if (!Version.TryParse(current, out var currentVersion)) return;
        if (!Version.TryParse(latest, out var latestVersion)) return;

        if (latestVersion > currentVersion)
        {
            Console.Error.WriteLine($"Update available: v{latest} (current v{current}).");
            Console.Error.WriteLine("Install latest release from https://github.com/Knaackee/linxmd/releases/latest");
        }
    }

    private static string GetCurrentVersion()
    {
        var informational = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
            return NormalizeVersion(informational) ?? "0.0.0";

        return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
    }

    private static string? NormalizeVersion(string raw)
    {
        var cleaned = raw.Trim();
        if (cleaned.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned[1..];

        var plus = cleaned.IndexOf('+');
        if (plus >= 0) cleaned = cleaned[..plus];
        var dash = cleaned.IndexOf('-');
        if (dash >= 0) cleaned = cleaned[..dash];

        return Version.TryParse(cleaned, out _) ? cleaned : null;
    }

    private static string GetCacheFilePath()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(baseDir))
            baseDir = Path.GetTempPath();

        var dir = Path.Combine(baseDir, "linxmd");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "update-check.json");
    }

    private static async Task<UpdateCache?> LoadCacheAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<UpdateCache>(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task SaveCacheAsync(string path, UpdateCache cache)
    {
        var json = JsonSerializer.Serialize(cache);
        await File.WriteAllTextAsync(path, json);
    }
}
