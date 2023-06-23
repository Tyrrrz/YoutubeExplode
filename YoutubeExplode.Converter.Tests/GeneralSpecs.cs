using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gress;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Converter.Tests.Utils;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Tests;

public class GeneralSpecs : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutput;

    public GeneralSpecs(ITestOutputHelper testOutput) =>
        _testOutput = testOutput;

    public async Task InitializeAsync() => await FFmpeg.InitializeAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task I_can_download_a_video_as_a_single_mp4_file()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", filePath);

        // Assert
        MediaFormat.IsMp4File(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_as_a_single_webm_file()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.webm");

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", filePath);

        // Assert
        MediaFormat.IsWebMFile(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_as_a_single_mp3_file()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp3");

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", filePath);

        // Assert
        MediaFormat.IsMp3File(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_as_a_single_ogg_file()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.ogg");

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", filePath);

        // Assert
        MediaFormat.IsOggFile(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_as_a_single_mp4_file_with_multiple_streams()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync("9bZkp7q19f0");

        var audioStreamInfos = manifest
            .GetAudioOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .OrderBy(s => s.Bitrate)
            .Take(3)
            .ToArray();

        var videoStreamInfos = manifest
            .GetVideoOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .OrderBy(s => s.VideoQuality)
            .DistinctBy(s => s.VideoQuality.Label, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToArray();

        await youtube.Videos.DownloadAsync(
            videoStreamInfos.Concat<IStreamInfo>(audioStreamInfos).ToArray(),
            new ConversionRequestBuilder(filePath).Build()
        );

        // Assert
        MediaFormat.IsMp4File(filePath).Should().BeTrue();

        foreach (var streamInfo in videoStreamInfos)
            FileEx.ContainsBytes(filePath, Encoding.ASCII.GetBytes(streamInfo.VideoQuality.Label)).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_as_a_single_webm_file_with_multiple_streams()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.webm");

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync("9bZkp7q19f0");

        var audioStreamInfos = manifest
            .GetAudioOnlyStreams()
            .Where(s => s.Container == Container.WebM)
            .OrderBy(s => s.Bitrate)
            .Take(3)
            .ToArray();

        var videoStreamInfos = manifest
            .GetVideoOnlyStreams()
            .Where(s => s.Container == Container.WebM)
            .OrderBy(s => s.VideoQuality)
            .DistinctBy(s => s.VideoQuality.Label, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToArray();

        await youtube.Videos.DownloadAsync(
            videoStreamInfos.Concat<IStreamInfo>(audioStreamInfos).ToArray(),
            new ConversionRequestBuilder(filePath).Build()
        );

        // Assert
        MediaFormat.IsWebMFile(filePath).Should().BeTrue();

        foreach (var streamInfo in videoStreamInfos)
            FileEx.ContainsBytes(filePath, Encoding.ASCII.GetBytes(streamInfo.VideoQuality.Label)).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_using_custom_conversion_settings()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp3");

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", filePath, o => o
            .SetFFmpegPath(FFmpeg.FilePath)
            .SetContainer("mp4")
            .SetPreset(ConversionPreset.UltraFast)
        );

        // Assert
        MediaFormat.IsMp4File(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_while_tracking_progress()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp3");

        var progress = new ProgressCollector<double>();

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", filePath, progress);

        // Assert
        var progressValues = progress.GetValues();
        progressValues.Should().NotBeEmpty();
        progressValues.Should().Contain(p => p >= 0.99);
        progressValues.Should().NotContain(p => p < 0 || p > 1);

        foreach (var value in progressValues)
            _testOutput.WriteLine($"Progress: {value:P2}");
    }
}