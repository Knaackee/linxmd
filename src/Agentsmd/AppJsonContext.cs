using System.Text.Json.Serialization;
using Agentsmd.Models;
using Agentsmd.Services;

namespace Agentsmd;

[JsonSerializable(typeof(LibIndex))]
[JsonSerializable(typeof(InstalledState))]
[JsonSerializable(typeof(List<GitHubEntry>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
internal partial class AppJsonContext : JsonSerializerContext;
