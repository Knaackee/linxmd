namespace Linxmd.Services;

public interface ILibClient
{
    Task<string?> FetchIndexAsync(CancellationToken ct = default);
    Task<string?> FetchFileAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListDirectoryAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListFilesAsync(string path, CancellationToken ct = default);
    Task<byte[]?> FetchBinaryAsync(string path, CancellationToken ct = default);
}

public sealed class GitHubLibClient : ILibClient
{
    private readonly string _baseUrl;
    private readonly string _apiBase;

    private readonly HttpClient _http;

    public GitHubLibClient(HttpClient http, string owner = "Knaackee", string repo = "linxmd", string branch = "main", string basePath = "lib")
    {
        _http = http;
        _baseUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{basePath.Trim('/')}";
        _apiBase = $"https://api.github.com/repos/{owner}/{repo}/contents/{basePath.Trim('/')}";
        if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("linxmd-cli/0.1");

        // Use GITHUB_TOKEN if available (raises API rate limit from 60 to 1000+ req/hour)
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(token) && _http.DefaultRequestHeaders.Authorization is null)
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string?> FetchIndexAsync(CancellationToken ct = default)
    {
        return await FetchFileAsync("index.json", ct);
    }

    public async Task<string?> FetchFileAsync(string path, CancellationToken ct = default)
    {
        try
        {
            var url = $"{_baseUrl}/{path}";
            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> FetchBinaryAsync(string path, CancellationToken ct = default)
    {
        try
        {
            var url = $"{_baseUrl}/{path}";
            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<string>> ListDirectoryAsync(string path, CancellationToken ct = default)
    {
        try
        {
            var url = $"{_apiBase}/{path}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/vnd.github.v3+json");
            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return [];
            var json = await response.Content.ReadAsStringAsync(ct);
            var entries = System.Text.Json.JsonSerializer.Deserialize(json, AppJsonContext.Default.ListGitHubEntry);
            return entries?.Select(e => e.Name).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<string>> ListFilesAsync(string path, CancellationToken ct = default)
    {
        var results = new List<string>();
        await ListFilesRecursiveAsync(path.Trim('/'), string.Empty, results, ct);
        return results;
    }

    private async Task ListFilesRecursiveAsync(string path, string relativePrefix, List<string> results, CancellationToken ct)
    {
        try
        {
            var url = string.IsNullOrEmpty(path) ? _apiBase : $"{_apiBase}/{path}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/vnd.github.v3+json");
            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync(ct);
            var entries = System.Text.Json.JsonSerializer.Deserialize(json, AppJsonContext.Default.ListGitHubEntry);
            if (entries is null)
                return;

            foreach (var entry in entries)
            {
                var relativePath = string.IsNullOrEmpty(relativePrefix)
                    ? entry.Name
                    : $"{relativePrefix}/{entry.Name}";

                if (entry.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(relativePath);
                    continue;
                }

                if (entry.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
                    await ListFilesRecursiveAsync($"{path}/{entry.Name}".Trim('/'), relativePath, results, ct);
            }
        }
        catch
        {
        }
    }
}
