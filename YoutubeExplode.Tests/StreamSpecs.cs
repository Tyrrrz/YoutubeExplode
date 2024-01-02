using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.TestData;
using YoutubeExplode.Tests.Utils;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Tests;

public class StreamSpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task I_can_get_the_list_of_available_streams_of_a_video()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(
            VideoIds.WithHighQualityStreams
        );

        // Assert
        manifest.Streams.Should().NotBeEmpty();

        manifest
            .GetVideoStreams()
            .Should()
            .Contain(
                s =>
                    s.VideoQuality.MaxHeight == 2160
                    && s.VideoQuality.Framerate == 60
                    && s.VideoQuality.IsHighDefinition
            );

        manifest
            .GetVideoStreams()
            .Should()
            .Contain(
                s =>
                    s.VideoQuality.MaxHeight == 1080
                    && s.VideoQuality.Framerate == 60
                    && s.VideoQuality.IsHighDefinition
            );

        manifest
            .GetVideoStreams()
            .Should()
            .Contain(s => s.VideoQuality.MaxHeight == 720 && !s.VideoQuality.IsHighDefinition);

        manifest
            .GetVideoStreams()
            .Should()
            .Contain(s => s.VideoQuality.MaxHeight == 144 && !s.VideoQuality.IsHighDefinition);
    }

    [Theory]
    [InlineData(VideoIds.Normal)]
    [InlineData(VideoIds.Unlisted)]
    [InlineData(VideoIds.EmbedRestrictedByYouTube)]
    [InlineData(VideoIds.EmbedRestrictedByAuthor)]
    [InlineData(VideoIds.AgeRestrictedViolent)]
    [InlineData(VideoIds.AgeRestrictedSexual)]
    [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
    [InlineData(VideoIds.LiveStreamRecording)]
    [InlineData(VideoIds.WithBrokenStreams)]
    [InlineData(VideoIds.WithOmnidirectionalStreams)]
    [InlineData(VideoIds.WithHighDynamicRangeStreams)]
    public async Task I_can_get_the_list_of_available_streams_of_any_playable_video(string videoId)
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

        // Assert
        manifest.Streams.Should().NotBeEmpty();
    }

    [Fact(Skip = "Preview video ID is not always available")]
    public async Task I_can_try_to_get_the_list_of_available_streams_of_a_video_and_get_an_error_if_it_is_paid()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<VideoRequiresPurchaseException>(
            async () => await youtube.Videos.Streams.GetManifestAsync(VideoIds.RequiresPurchase)
        );

        ex.PreviewVideoId.Value.Should().NotBeNullOrWhiteSpace();

        testOutput.WriteLine(ex.ToString());
    }

    [Fact]
    public async Task I_can_try_to_get_the_list_of_available_streams_of_a_video_and_get_an_error_if_it_is_private()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<VideoUnavailableException>(
            async () => await youtube.Videos.Streams.GetManifestAsync(VideoIds.Private)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Fact]
    public async Task I_can_try_to_get_the_list_of_available_streams_of_a_video_and_get_an_error_if_it_does_not_exist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<VideoUnavailableException>(
            async () => await youtube.Videos.Streams.GetManifestAsync(VideoIds.Deleted)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Theory]
    [InlineData(VideoIds.Normal)]
    [InlineData(VideoIds.AgeRestrictedViolent)]
    [InlineData(VideoIds.AgeRestrictedSexual)]
    [InlineData(VideoIds.LiveStreamRecording)]
    [InlineData(VideoIds.WithBrokenStreams)]
    [InlineData(VideoIds.WithOmnidirectionalStreams)]
    public async Task I_can_get_a_specific_stream_of_a_video(string videoId)
    {
        // Arrange
        using var buffer = MemoryPool<byte>.Shared.Rent(1024);
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

        foreach (var streamInfo in manifest.Streams)
        {
            using var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            var bytesRead = await stream.ReadAsync(buffer.Memory);

            // Assert
            bytesRead.Should().BeGreaterThan(0);
        }
    }

    [Theory]
    [InlineData(VideoIds.Normal)]
    [InlineData(VideoIds.Unlisted)]
    [InlineData(VideoIds.EmbedRestrictedByYouTube)]
    [InlineData(VideoIds.EmbedRestrictedByAuthor)]
    [InlineData(VideoIds.AgeRestrictedViolent)]
    [InlineData(VideoIds.AgeRestrictedSexual)]
    [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
    [InlineData(VideoIds.LiveStreamRecording)]
    [InlineData(VideoIds.WithBrokenStreams)]
    [InlineData(VideoIds.WithOmnidirectionalStreams)]
    public async Task I_can_download_a_specific_stream_of_a_video(string videoId)
    {
        // Arrange
        using var file = TempFile.Create();
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var streamInfo = manifest.Streams.OrderBy(s => s.Size).First();

        await youtube.Videos.Streams.DownloadAsync(streamInfo, file.Path);

        // Assert
        var fileInfo = new FileInfo(file.Path);
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Length.Should().Be(streamInfo.Size.Bytes);
    }

    [Fact]
    public async Task I_can_download_the_highest_bitrate_stream_of_a_video()
    {
        // Arrange
        using var file = TempFile.Create();
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(VideoIds.Normal);
        var streamInfo = manifest.Streams.GetWithHighestBitrate();

        await youtube.Videos.Streams.DownloadAsync(streamInfo, file.Path);

        // Assert
        var fileInfo = new FileInfo(file.Path);
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Length.Should().Be(streamInfo.Size.Bytes);
    }

    [Fact]
    public async Task I_can_download_the_highest_quality_stream_of_a_video()
    {
        // Arrange
        using var file = TempFile.Create();
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(VideoIds.Normal);
        var streamInfo = manifest.GetVideoStreams().GetWithHighestVideoQuality();

        await youtube.Videos.Streams.DownloadAsync(streamInfo, file.Path);

        // Assert
        var fileInfo = new FileInfo(file.Path);
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Length.Should().Be(streamInfo.Size.Bytes);
    }

    [Fact]
    public async Task I_can_seek_to_a_specific_position_of_a_stream_from_a_video()
    {
        // Arrange
        using var buffer = new MemoryStream();
        var youtube = new YoutubeClient();

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync(VideoIds.Normal);
        var streamInfo = manifest.GetAudioStreams().OrderBy(s => s.Size).First();

        using var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
        stream.Seek(1000, SeekOrigin.Begin);
        await stream.CopyToAsync(buffer);

        // Assert
        buffer.Length.Should().Be(streamInfo.Size.Bytes - 1000);
    }

    [Fact]
    public async Task I_can_get_the_HTTP_live_stream_URL_for_a_video()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var url = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(VideoIds.LiveStream);

        // Assert
        url.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task I_can_try_to_get_the_HTTP_live_stream_URL_for_a_video_and_get_an_error_if_it_is_unplayable()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<VideoUnplayableException>(
            async () =>
                await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(VideoIds.RequiresPurchase)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Fact]
    public async Task I_can_try_to_get_the_HTTP_live_stream_URL_for_a_video_and_get_an_error_if_it_is_not_live()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<YoutubeExplodeException>(
            async () => await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(VideoIds.Normal)
        );

        testOutput.WriteLine(ex.ToString());
    }
}
