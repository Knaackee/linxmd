using System.Text.Json.Serialization;
using Linxmd.Models;
using Linxmd.Services;

namespace Linxmd;

[JsonSerializable(typeof(LibIndex))]
[JsonSerializable(typeof(InstalledState))]
[JsonSerializable(typeof(SourceRegistry))]
[JsonSerializable(typeof(List<LibSource>))]
[JsonSerializable(typeof(List<GitHubEntry>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
internal partial class AppJsonContext : JsonSerializerContext;
