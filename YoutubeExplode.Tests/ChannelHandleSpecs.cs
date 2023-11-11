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
    public void I_can_parse_a_channel_handle_from_a_handle_string(string channelHandle)
    {
        // Act
        var parsed = ChannelHandle.Parse(channelHandle);

        // Assert
        parsed.Value.Should().Be(channelHandle);
    }

    [Theory]
    [InlineData("youtube.com/@BeauMiles", "BeauMiles")]
    [InlineData("youtube.com/@a-z.0_9", "a-z.0_9")]
    public void I_can_parse_a_channel_handle_from_a_URL_string(
        string channelUrl,
        string expectedChannelHandle
    )
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
    public void I_can_try_to_parse_a_channel_handle_and_get_an_error_if_the_input_string_is_invalid(
        string channelHandleOrUrl
    )
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => ChannelHandle.Parse(channelHandleOrUrl));
    }
}
