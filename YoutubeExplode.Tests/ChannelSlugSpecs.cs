using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Tests;

public class ChannelSlugSpecs
{
    [Theory]
    [InlineData("Tyrrrz")]
    [InlineData("BlenderFoundation")]
    public void Channel_slug_can_be_parsed_from_a_slug_string(string channelSlug)
    {
        // Act
        var parsed = ChannelSlug.Parse(channelSlug);

        // Assert
        parsed.Value.Should().Be(channelSlug);
    }

    [Theory]
    [InlineData("youtube.com/c/Tyrrrz", "Tyrrrz")]
    [InlineData("youtube.com/c/BlenderFoundation", "BlenderFoundation")]
    [InlineData("youtube.com/c/%D0%9C%D0%B5%D0%BB%D0%B0%D0%BD%D1%96%D1%8F%D0%9F%D0%BE%D0%B4%D0%BE%D0%BB%D1%8F%D0%BA", "МеланіяПодоляк")]
    public void Channel_slug_can_be_parsed_from_a_URL_string(string channelUrl, string expectedChannelSlug)
    {
        // Act
        var parsed = ChannelSlug.Parse(channelUrl);

        // Assert
        parsed.Value.Should().Be(expectedChannelSlug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("foo bar")]
    [InlineData("youtube.com/?c=Tyrrrz")]
    [InlineData("youtube.com/channel/Tyrrrz")]
    [InlineData("youtube.com/")]
    public void Channel_slug_cannot_be_parsed_from_an_invalid_string(string channelSlugOrUrl)
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => ChannelSlug.Parse(channelSlugOrUrl));
    }
}