namespace Linxmd.Services;

/// <summary>
/// Reads lib artifacts from the local filesystem. Used for development and testing.
/// </summary>
public sealed class LocalLibClient : ILibClient
{
    private readonly string _basePath;

    public LocalLibClient(string basePath)
    {
        _basePath = basePath;
    }

    public Task<string?> FetchIndexAsync(CancellationToken ct = default)
        => FetchFileAsync("index.json", ct);

    public Task<string?> FetchFileAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<string?>(null);
        return Task.FromResult<string?>(File.ReadAllText(fullPath));
    }

    public Task<byte[]?> FetchBinaryAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<byte[]?>(null);
        return Task.FromResult<byte[]?>(File.ReadAllBytes(fullPath));
    }

    public Task<IReadOnlyList<string>> ListDirectoryAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(fullPath))
            return Task.FromResult<IReadOnlyList<string>>([]);

        var files = Directory.GetFiles(fullPath)
            .Select(Path.GetFileName)
            .Where(f => f is not null)
            .Cast<string>()
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(files);
    }
}
