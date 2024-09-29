using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.TestData;

namespace YoutubeExplode.Tests;

public class VideoSpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task I_can_get_the_metadata_of_a_video()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var video = await youtube.Videos.GetAsync(VideoIds.Normal);

        // Assert
        video.Id.Value.Should().Be(VideoIds.Normal);
        video.Url.Should().NotBeNullOrWhiteSpace();
        video.Title.Should().Be("PSY - GANGNAM STYLE(강남스타일) M/V");
        video.Author.ChannelId.Value.Should().Be("UCrDkAvwZum-UTjHmzDI2iIw");
        video.Author.ChannelUrl.Should().NotBeNullOrWhiteSpace();
        video.Author.ChannelTitle.Should().Be("officialpsy");
        video.UploadDate.Date.Should().Be(new DateTime(2012, 07, 15));
        video.Description.Should().Contain("More about PSY@");
        video.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(252), TimeSpan.FromSeconds(1));
        video.Thumbnails.Should().NotBeEmpty();
        video
            .Keywords.Should()
            .BeEquivalentTo(
                "PSY",
                "싸이",
                "강남스타일",
                "뮤직비디오",
                "Music Video",
                "Gangnam Style",
                "KOREAN SINGER",
                "KPOP",
                "KOERAN WAVE",
                "PSY 6甲",
                "6th Studio Album",
                "싸이6집",
                "육갑",
                "Psy Gangnam Style"
            );
        video.Engagement.ViewCount.Should().BeGreaterOrEqualTo(4_650_000_000);
        video.Engagement.LikeCount.Should().BeGreaterOrEqualTo(24_000_000);
        video.Engagement.DislikeCount.Should().BeGreaterOrEqualTo(0);
        video.Engagement.AverageRating.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task I_can_try_to_get_the_metadata_of_a_video_and_get_an_error_if_it_is_private()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<VideoUnavailableException>(
            async () => await youtube.Videos.GetAsync(VideoIds.Private)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Fact]
    public async Task I_can_try_to_get_the_metadata_of_a_video_and_get_an_error_if_it_does_not_exist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<VideoUnavailableException>(
            async () => await youtube.Videos.GetAsync(VideoIds.Deleted)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Theory]
    [InlineData(VideoIds.Normal)]
    [InlineData(VideoIds.Unlisted)]
    [InlineData(VideoIds.RequiresPurchaseDistributed)]
    [InlineData(VideoIds.EmbedRestrictedByYouTube)]
    [InlineData(VideoIds.EmbedRestrictedByAuthor)]
    [InlineData(VideoIds.ContentCheckViolent)]
    [InlineData(VideoIds.WithBrokenTitle)]
    public async Task I_can_get_the_metadata_of_any_available_video(string videoId)
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var video = await youtube.Videos.GetAsync(videoId);

        // Assert
        video.Id.Value.Should().Be(videoId);
        video.Url.Should().NotBeNullOrWhiteSpace();
        video.Title.Should().NotBeNull(); // empty titles are allowed
        video.Author.ChannelId.Value.Should().NotBeNullOrWhiteSpace();
        video.Author.ChannelUrl.Should().NotBeNullOrWhiteSpace();
        video.Author.ChannelTitle.Should().NotBeNullOrWhiteSpace();
        video.UploadDate.Date.Should().NotBe(default);
        video.Description.Should().NotBeNull();
        video.Duration.Should().NotBe(default);
        video.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_the_highest_resolution_thumbnail_from_a_video()
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
