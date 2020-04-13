using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Tests
{
    public class ChannelIdSpecs
    {
        [Theory]
        [InlineData("UCEnBXANsKmyj2r9xVyKoDiQ")]
        [InlineData("UC46807r_RiRjH8IU-h_DrDQ")]
        public void I_can_specify_a_valid_channel_id(string channelId)
        {
            // Act
            var result = new ChannelId(channelId);

            // Assert
            result.Value.Should().Be(channelId);
        }

        [Theory]
        [InlineData("youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ", "UC3xnGqlcL3y-GXz5N3wiTJQ")]
        [InlineData("youtube.com/channel/UCkQO3QsgTpNTsOw6ujimT5Q", "UCkQO3QsgTpNTsOw6ujimT5Q")]
        [InlineData("youtube.com/channel/UCQtjJDOYluum87LA4sI6xcg", "UCQtjJDOYluum87LA4sI6xcg")]
        public void I_can_specify_a_valid_channel_url_in_place_of_an_id(string channelUrl, string expectedChannelId)
        {
            // Act
            var result = new ChannelId(channelUrl);

            // Assert
            result.Value.Should().Be(expectedChannelId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("UC3xnGqlcL3y-GXz5N3wiTJ")]
        [InlineData("UC3xnGqlcL y-GXz5N3wiTJQ")]
        public void I_cannot_specify_an_invalid_channel_id(string channelId)
        {
            // Act & assert
            Assert.Throws<ArgumentException>(() => new ChannelId(channelId));
        }

        [Theory]
        [InlineData("youtube.com/?channel=UCUC3xnGqlcL3y-GXz5N3wiTJQ")]
        [InlineData("youtube.com/channel/asd")]
        [InlineData("youtube.com/")]
        public void I_cannot_specify_an_invalid_channel_url_in_place_of_an_id(string channelUrl)
        {
            // Act & assert
            Assert.Throws<ArgumentException>(() => new ChannelId(channelUrl));
        }
    }
}