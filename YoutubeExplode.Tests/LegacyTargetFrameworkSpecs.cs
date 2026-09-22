#if NETFRAMEWORK
using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace YoutubeExplode.Tests;

public class LegacyTargetFrameworkSpecs
{
    [Fact]
    public void Netstandard2_package_uses_the_UWP_compatible_SystemTextJson_version()
    {
        var version = typeof(JsonSerializer).Assembly.GetName().Version;

        version.Should().NotBeNull();
        version!.Major.Should().Be(9);
    }
}
#endif
