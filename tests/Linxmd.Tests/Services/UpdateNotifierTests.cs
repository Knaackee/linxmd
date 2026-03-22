using System.Reflection;
using Linxmd.Services;
using FluentAssertions;

namespace Linxmd.Tests.Services;

/// <summary>
/// Tests for UpdateNotifier static helper logic.
/// Network-dependent methods (TryNotifyAsync) are not tested here.
/// NormalizeVersion is private, accessed via reflection.
/// </summary>
public class UpdateNotifierTests
{
    private static string? InvokeNormalizeVersion(string raw)
    {
        var method = typeof(UpdateNotifier).GetMethod(
            "NormalizeVersion",
            BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("NormalizeVersion must exist as a private static method");
        return (string?)method!.Invoke(null, [raw]);
    }

    [Theory]
    [InlineData("v1.2.3", "1.2.3")]
    [InlineData("V1.2.3", "1.2.3")]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("0.6.0", "0.6.0")]
    public void NormalizeVersion_ValidVersions_ReturnsClean(string input, string expected)
    {
        var result = InvokeNormalizeVersion(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("v1.2.3+build.1", "1.2.3")]
    [InlineData("1.2.3+meta", "1.2.3")]
    public void NormalizeVersion_WithBuildMetadata_StripsMeta(string input, string expected)
    {
        var result = InvokeNormalizeVersion(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("v1.2.3-beta.1", "1.2.3")]
    [InlineData("1.0.0-alpha", "1.0.0")]
    [InlineData("2.0.0-rc.1", "2.0.0")]
    public void NormalizeVersion_WithPrereleaseSuffix_StripsSuffix(string input, string expected)
    {
        var result = InvokeNormalizeVersion(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("not-a-version")]
    [InlineData("abc")]
    [InlineData("")]
    public void NormalizeVersion_InvalidInput_ReturnsNull(string input)
    {
        var result = InvokeNormalizeVersion(input);
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeVersion_WhitespaceInput_ReturnsNull()
    {
        var result = InvokeNormalizeVersion("   ");
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeVersion_LeadingTrailingSpaces_AreStripped()
    {
        var result = InvokeNormalizeVersion("  v1.0.0  ");
        result.Should().Be("1.0.0");
    }

    [Fact]
    public void TryNotifyAsync_WithNoUpdateCheckEnvVar_ReturnsWithoutError()
    {
        // Ensure the env var suppression works (no network call, no exception)
        Environment.SetEnvironmentVariable("LINXMD_NO_UPDATE_CHECK", "1");
        try
        {
            var act = async () => await UpdateNotifier.TryNotifyAsync();
            act.Should().NotThrowAsync().GetAwaiter().GetResult();
        }
        finally
        {
            Environment.SetEnvironmentVariable("LINXMD_NO_UPDATE_CHECK", null);
        }
    }
}
