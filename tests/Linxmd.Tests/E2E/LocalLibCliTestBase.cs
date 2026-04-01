using System.Diagnostics;

namespace Linxmd.Tests.E2E;

public abstract class LocalLibCliTestBase : IDisposable
{
    protected readonly string TempDir;
    protected readonly string RepoRoot;
    private readonly string _localLibPath;

    protected LocalLibCliTestBase(string tempPrefix)
    {
        TempDir = Path.Combine(Path.GetTempPath(), tempPrefix + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(TempDir);
        RepoRoot = ResolveRepoRoot();
        _localLibPath = Path.Combine(RepoRoot, "tests", "Linxmd.Tests", "TestLib");
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
            Directory.Delete(TempDir, recursive: true);
    }

    protected (int exitCode, string stdout, string stderr) RunCli(string args, bool includeProject = true)
    {
        var projectArg = includeProject ? $"--project \"{TempDir}\"" : "";
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {Path.Combine(RepoRoot, "src", "Linxmd", "Linxmd.csproj")} -- {args} {projectArg}".Trim(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = RepoRoot
        };
        psi.Environment["NO_COLOR"] = "1";

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(120_000);
        return (process.ExitCode, stdout, stderr);
    }

    protected void UseLocalSource()
    {
        var sourcesPath = Path.Combine(TempDir, ".linxmd", "sources.json");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcesPath)!);
        var escapedPath = _localLibPath.Replace("\\", "\\\\");
        var json = $$"""
            {
              "sources": [
                {
                  "id": "default",
                  "kind": "local",
                  "localPath": "{{escapedPath}}"
                }
              ]
            }
            """;
        File.WriteAllText(sourcesPath, json);
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Linxmd.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root from test runtime directory.");
    }
}