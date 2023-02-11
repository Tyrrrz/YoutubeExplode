using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Converter.Tests.Utils;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Tests;

public class SubtitleSpecs : IAsyncLifetime
{
    public async Task InitializeAsync() => await FFmpeg.InitializeAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task I_can_download_a_video_with_subtitles_as_a_single_mp4_file()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        var streamManifest = await youtube.Videos.Streams.GetManifestAsync("YltHGKX80Y8");
        var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync("YltHGKX80Y8");

        var streamInfos = new[]
        {
            streamManifest.GetVideoStreams().OrderBy(s => s.Size).First(s => s.Container == Container.Mp4)
        };

        var trackInfos = trackManifest.Tracks;

        // Act
        await youtube.Videos.DownloadAsync(
            streamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath).Build()
        );

        // Assert
        MediaFormat.IsMp4File(filePath).Should().BeTrue();

        foreach (var trackInfo in trackInfos)
            FileEx.ContainsBytes(filePath, Encoding.ASCII.GetBytes(trackInfo.Language.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_with_subtitles_as_a_single_webm_file()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.webm");

        var streamManifest = await youtube.Videos.Streams.GetManifestAsync("YltHGKX80Y8");
        var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync("YltHGKX80Y8");

        var streamInfos = new[]
        {
            streamManifest.GetVideoStreams().OrderBy(s => s.Size).First(s => s.Container == Container.WebM)
        };

        var trackInfos = trackManifest.Tracks;

        // Act
        await youtube.Videos.DownloadAsync(
            streamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath).Build()
        );

        // Assert
        MediaFormat.IsWebMFile(filePath).Should().BeTrue();

        foreach (var trackInfo in trackInfos)
            FileEx.ContainsBytes(filePath, Encoding.ASCII.GetBytes(trackInfo.Language.Name)).Should().BeTrue();
    }
}