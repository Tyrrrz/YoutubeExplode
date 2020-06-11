using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Tests
{
    public class VideoSpecs
    {
        [Fact]
        public async Task I_can_get_metadata_of_a_YouTube_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=AI7ULzgf8RU";
            var youtube = new YoutubeClient();

            // Act
            var video = await youtube.Videos.GetAsync(videoUrl);

            // Assert
            video.Id.Value.Should().Be("AI7ULzgf8RU");
            video.Url.Should().Be(videoUrl);
            video.Title.Should().Be("Aka no Ha [Another] +HDHR");
            video.Author.Should().Be("Tyrrrz");
            video.ChannelId.Value.Should().Be("UCEnBXANsKmyj2r9xVyKoDiQ");
            video.UploadDate.Date.Should().Be(new DateTime(2017, 09, 30));
            video.Description.Should().Contain("246pp");
            video.Duration.Should().Be(new TimeSpan(00, 01, 48));
            video.Thumbnails.LowResUrl.Should().NotBeNullOrWhiteSpace();
            video.Thumbnails.MediumResUrl.Should().NotBeNullOrWhiteSpace();
            video.Thumbnails.HighResUrl.Should().NotBeNullOrWhiteSpace();
            video.Thumbnails.StandardResUrl.Should().NotBeNullOrWhiteSpace();
            video.Thumbnails.MaxResUrl.Should().NotBeNullOrWhiteSpace();
            video.Keywords.Should().BeEquivalentTo("osu", "mouse", "rhythm game");
            video.Engagement.ViewCount.Should().BeGreaterOrEqualTo(134);
            video.Engagement.LikeCount.Should().BeGreaterOrEqualTo(5);
            video.Engagement.DislikeCount.Should().BeGreaterOrEqualTo(0);
        }

        [Theory]
        [InlineData("9bZkp7q19f0")] // very popular
        [InlineData("SkRSXFQerZs")] // age-restricted
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("5qap5aO4i9A")] // ongoing live stream
        public async Task I_can_get_metadata_of_any_available_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var video = await youtube.Videos.GetAsync(videoId);

            // Assert
            video.Id.Value.Should().Be(videoId);
        }

        [Theory]
        [InlineData("qld9w0b-1ao")] // doesn't exist
        [InlineData("pb_hHv3fByo")] // private
        public async Task I_cannot_get_metadata_of_an_unavailable_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(() => youtube.Videos.GetAsync(videoId));
        }
    }
}