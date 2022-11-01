using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Tests;

public class ChannelHandleSpecs
{
    [Theory]
    [InlineData("BeauMiles")]
    [InlineData("a-z.0_9")]
    public void Channel_handle_can_be_parsed_from_a_handle_string(string channelHandle)
    {
        // Act
        var parsed = ChannelHandle.Parse(channelHandle);

        // Assert
        parsed.Value.Should().Be(channelHandle);
    }

    [Theory]
    [InlineData("youtube.com/@BeauMiles", "BeauMiles")]
    [InlineData("youtube.com/@a-z.0_9", "a-z.0_9")]
    public void Channel_handle_can_be_parsed_from_a_URL_string(string channelUrl, string expectedChannelHandle)
    {
        // Act
        var parsed = ChannelHandle.Parse(channelUrl);

        // Assert
        parsed.Value.Should().Be(expectedChannelHandle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("foo bar")]
    [InlineData("youtube.com/")]
    [InlineData("youtube.com@BeauMiles")]
    [InlineData("youtube.com/@=BeauMiles")]
    [InlineData("youtube.com/@BeauMile$")]
    [InlineData("youtube.com/@Beau+Miles")]
    [InlineData("youtube.com/?@BeauMiles")]
    public void Channel_handle_cannot_be_parsed_from_an_invalid_string(string channelHandleOrUrl)
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => ChannelHandle.Parse(channelHandleOrUrl));
    }
}