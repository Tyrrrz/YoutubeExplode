using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Converter.Tests.Fixtures;
using YoutubeExplode.Converter.Tests.Utils;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Tests;

public class SubtitleSpecs : IClassFixture<TempOutputFixture>, IClassFixture<FFmpegFixture>
{
    private readonly TempOutputFixture _tempOutputFixture;
    private readonly FFmpegFixture _ffmpegFixture;

    public SubtitleSpecs(
        TempOutputFixture tempOutputFixture,
        FFmpegFixture ffmpegFixture)
    {
        _tempOutputFixture = tempOutputFixture;
        _ffmpegFixture = ffmpegFixture;
    }

    [Fact]
    public async Task I_can_download_a_video_with_subtitles_into_a_single_mp4_file()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = _tempOutputFixture.GetTempFilePath();

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
            new ConversionRequestBuilder(outputFilePath)
                .SetFFmpegPath(_ffmpegFixture.FilePath)
                .SetContainer("mp4")
                .Build()
        );

        // Assert
        MediaFormat.IsMp4File(outputFilePath).Should().BeTrue();

        foreach (var trackInfo in trackInfos)
            FileEx.ContainsBytes(outputFilePath, Encoding.ASCII.GetBytes(trackInfo.Language.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_with_subtitles_into_a_single_webm_file()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = _tempOutputFixture.GetTempFilePath();

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
            new ConversionRequestBuilder(outputFilePath)
                .SetFFmpegPath(_ffmpegFixture.FilePath)
                .SetContainer("webm")
                .Build()
        );

        // Assert
        MediaFormat.IsWebMFile(outputFilePath).Should().BeTrue();

        foreach (var trackInfo in trackInfos)
            FileEx.ContainsBytes(outputFilePath, Encoding.ASCII.GetBytes(trackInfo.Language.Name)).Should().BeTrue();
    }
}