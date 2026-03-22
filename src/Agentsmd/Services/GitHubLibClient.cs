namespace Agentsmd.Services;

public interface ILibClient
{
    Task<string?> FetchIndexAsync(CancellationToken ct = default);
    Task<string?> FetchFileAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListDirectoryAsync(string path, CancellationToken ct = default);
    Task<byte[]?> FetchBinaryAsync(string path, CancellationToken ct = default);
}

public sealed class GitHubLibClient : ILibClient
{
    private const string Owner = "Knaackee";
    private const string Repo = "agentsmd";
    private const string Branch = "main";
    private const string BaseUrl = $"https://raw.githubusercontent.com/{Owner}/{Repo}/{Branch}/lib";
    private const string ApiBase = $"https://api.github.com/repos/{Owner}/{Repo}/contents/lib";

    private readonly HttpClient _http;

    public GitHubLibClient(HttpClient http)
    {
        _http = http;
        if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("agentsmd-cli/0.1");

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
            var url = $"{BaseUrl}/{path}";
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
            var url = $"{BaseUrl}/{path}";
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
            var url = $"{ApiBase}/{path}";
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
}
