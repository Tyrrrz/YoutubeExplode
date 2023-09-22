using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Tests;

public class VideoIdSpecs
{
    [Theory]
    [InlineData("9bZkp7q19f0")]
    [InlineData("_kmeFXjjGfk")]
    [InlineData("AI7ULzgf8RU")]
    public void I_can_parse_a_video_ID_from_an_ID_string(string videoId)
    {
        // Act
        var parsed = VideoId.Parse(videoId);

        // Assert
        parsed.Value.Should().Be(videoId);
    }

    [Theory]
    [InlineData("youtube.com/watch?v=yIVRs6YSbOM", "yIVRs6YSbOM")]
    [InlineData("youtu.be/yIVRs6YSbOM", "yIVRs6YSbOM")]
    [InlineData("youtube.com/embed/yIVRs6YSbOM", "yIVRs6YSbOM")]
    [InlineData("youtube.com/shorts/sKL1vjP0tIo", "sKL1vjP0tIo")]
    [InlineData("youtube.com/live/jfKfPfyJRdk", "jfKfPfyJRdk")]
    public void I_can_parse_a_video_ID_from_a_URL_string(string videoUrl, string expectedVideoId)
    {
        // Act
        var parsed = VideoId.Parse(videoUrl);

        // Assert
        parsed.Value.Should().Be(expectedVideoId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("pI2I2zqzeK")]
    [InlineData("pI2I2z zeKg")]
    [InlineData("youtube.com/xxx?v=pI2I2zqzeKg")]
    [InlineData("youtu.be/watch?v=xxx")]
    [InlineData("youtube.com/embed/")]
    [InlineData("youtube.com/live/")]
    public void I_cannot_parse_a_video_ID_from_an_invalid_string(string videoId)
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => VideoId.Parse(videoId));
    }
}
