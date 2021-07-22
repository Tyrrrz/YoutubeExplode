using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.TestData;

namespace YoutubeExplode.Tests
{
    public class VideoSpecs
    {
        private readonly ITestOutputHelper _testOutput;

        public VideoSpecs(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public async Task User_can_get_metadata_of_a_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var video = await youtube.Videos.GetAsync(VideoIds.ContainsDashManifest);

            // Assert
            video.Id.Value.Should().Be(VideoIds.ContainsDashManifest);
            video.Url.Should().NotBeNullOrWhiteSpace();
            video.Title.Should().Be("Aka no Ha [Another] +HDHR");
            video.Author.ChannelId.Value.Should().Be("UCEnBXANsKmyj2r9xVyKoDiQ");
            video.Author.Title.Should().Be("Tyrrrz");
            video.UploadDate.Date.Should().Be(new DateTime(2017, 09, 30));
            video.Description.Should().Contain("246pp");
            video.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(108), 1000);
            video.Thumbnails.Should().NotBeEmpty();
            video.Keywords.Should().BeEquivalentTo("osu", "mouse", "rhythm game");
            video.Engagement.ViewCount.Should().BeGreaterOrEqualTo(134);
            video.Engagement.LikeCount.Should().BeGreaterOrEqualTo(5);
            video.Engagement.DislikeCount.Should().BeGreaterOrEqualTo(0);
            video.Engagement.AverageRating.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public async Task User_cannot_get_metadata_of_a_private_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            var ex = await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.GetAsync(VideoIds.Private)
            );

            _testOutput.WriteLine(ex.Message);
        }

        [Fact]
        public async Task User_cannot_get_metadata_of_a_non_existing_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            var ex = await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.GetAsync(VideoIds.NonExisting)
            );

            _testOutput.WriteLine(ex.Message);
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

        [Fact]
        public async Task User_can_get_the_highest_resolution_thumbnail_from_a_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var video = await youtube.Videos.GetAsync(VideoIds.Normal);
            var thumbnail = video.Thumbnails.GetWithHighestResolution();

            // Assert
            thumbnail.Url.Should().NotBeNullOrWhiteSpace();
        }
    }
}