using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.TestData;

namespace YoutubeExplode.Tests;

public class VideoSpecs
{
    private readonly ITestOutputHelper _testOutput;

    public VideoSpecs(ITestOutputHelper testOutput) =>
        _testOutput = testOutput;

    [Fact]
    public async Task I_can_get_metadata_of_a_video()
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
        video.Keywords.Should().BeEquivalentTo(
            "PSY", "싸이", "강남스타일", "뮤직비디오",
            "Music Video", "Gangnam Style", "KOREAN SINGER", "KPOP", "KOERAN WAVE",
            "PSY 6甲", "6th Studio Album", "싸이6집", "육갑"
        );
        video.Engagement.ViewCount.Should().BeGreaterOrEqualTo(4_650_000_000);
        video.Engagement.LikeCount.Should().BeGreaterOrEqualTo(24_000_000);
        video.Engagement.DislikeCount.Should().BeGreaterOrEqualTo(0);
        video.Engagement.AverageRating.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task I_cannot_get_metadata_of_a_private_video()
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
    public async Task I_cannot_get_metadata_of_a_non_existing_video()
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
    [InlineData(VideoIds.AgeRestrictedViolent)]
    [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
    public async Task I_can_get_metadata_of_any_available_video(string videoId)
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var video = await youtube.Videos.GetAsync(videoId);

        // Assert
        video.Id.Value.Should().Be(videoId);
        video.Url.Should().NotBeNullOrWhiteSpace();
        video.Title.Should().NotBeNullOrWhiteSpace();
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