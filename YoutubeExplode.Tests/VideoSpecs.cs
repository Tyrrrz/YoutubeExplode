using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.Ids;

namespace YoutubeExplode.Tests
{
    public class VideoSpecs
    {
        [Fact]
        public async Task User_can_get_metadata_of_a_video()
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
            video.Duration.Should().BeCloseTo(new TimeSpan(00, 01, 48), 1000);
            video.Thumbnails.Should().NotBeEmpty();
            video.Keywords.Should().BeEquivalentTo("osu", "mouse", "rhythm game");
            video.Engagement.ViewCount.Should().BeGreaterOrEqualTo(134);
            video.Engagement.LikeCount.Should().BeGreaterOrEqualTo(5);
            video.Engagement.DislikeCount.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public async Task User_cannot_get_metadata_of_a_private_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=pb_hHv3fByo";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.GetAsync(videoUrl)
            );
        }

        [Fact]
        public async Task User_cannot_get_metadata_of_a_non_existing_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=qld9w0b-1ao";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.GetAsync(videoUrl)
            );
        }

        [Theory]
        [InlineData(VideoIds.Normal)]
        [InlineData(VideoIds.Unlisted)]
        [InlineData(VideoIds.EmbedRestrictedByAuthor)]
        [InlineData(VideoIds.EmbedRestrictedByYouTube)]
        [InlineData(VideoIds.AgeRestricted)]
        [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
        [InlineData(VideoIds.RatingDisabled)]
        public async Task User_can_get_metadata_of_any_available_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var video = await youtube.Videos.GetAsync(videoId);

            // Assert
            video.Id.Value.Should().Be(videoId);
        }
    }
}