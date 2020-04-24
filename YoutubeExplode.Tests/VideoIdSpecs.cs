using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Tests
{
    public class VideoIdSpecs
    {
        [Theory]
        [InlineData("9bZkp7q19f0")]
        [InlineData("_kmeFXjjGfk")]
        [InlineData("AI7ULzgf8RU")]
        public void I_can_specify_a_valid_video_id(string videoId)
        {
            // Act
            var result = new VideoId(videoId);
            var maybeResult = VideoId.TryParse(videoId);

            // Assert
            result.Value.Should().Be(videoId);
            maybeResult.Should().Be(result);
        }

        [Theory]
        [InlineData("youtube.com/watch?v=yIVRs6YSbOM", "yIVRs6YSbOM")]
        [InlineData("youtu.be/yIVRs6YSbOM", "yIVRs6YSbOM")]
        [InlineData("youtube.com/embed/yIVRs6YSbOM", "yIVRs6YSbOM")]
        public void I_can_specify_a_valid_video_url_in_place_of_an_id(string videoUrl, string expectedVideoId)
        {
            // Act
            var result = new VideoId(videoUrl);
            var maybeResult = VideoId.TryParse(videoUrl);

            // Assert
            result.Value.Should().Be(expectedVideoId);
            maybeResult.Should().Be(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("pI2I2zqzeK")]
        [InlineData("pI2I2z zeKg")]
        public void I_cannot_specify_an_invalid_video_id(string videoId)
        {
            // Act & assert
            Assert.Throws<ArgumentException>(() => new VideoId(videoId));
            VideoId.TryParse(videoId).Should().BeNull();
        }

        [Theory]
        [InlineData("youtube.com/xxx?v=pI2I2zqzeKg")]
        [InlineData("youtu.be/watch?v=xxx")]
        [InlineData("youtube.com/embed/")]
        public void I_cannot_specify_an_invalid_video_url_in_place_of_an_id(string videoUrl)
        {
            // Act & assert
            Assert.Throws<ArgumentException>(() => new VideoId(videoUrl));
            VideoId.TryParse(videoUrl).Should().BeNull();
        }
    }
}