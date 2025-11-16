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
using YoutubeExplode.Converter.Tests.Utils.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Tests;

public class GeneralSpecs(ITestOutputHelper testOutput) : IAsyncLifetime
{
    public async Task InitializeAsync() => await FFmpeg.InitializeAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task I_can_download_a_video_as_a_single_mp4_file()
    {
        // Arrange
        using var youtube = new YoutubeClient();

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
        using var youtube = new YoutubeClient();

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
        using var youtube = new YoutubeClient();

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
        using var youtube = new YoutubeClient();

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
        using var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync("ngqcjXfggHQ");

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

        foreach (var streamInfo in audioStreamInfos)
        {
            if (streamInfo.AudioLanguage is not null)
            {
                File.ContainsBytes(
                        filePath,
                        Encoding.ASCII.GetBytes(streamInfo.AudioLanguage.Value.Name)
                    )
                    .Should()
                    .BeTrue();
            }
        }

        foreach (var streamInfo in videoStreamInfos)
        {
            File.ContainsBytes(filePath, Encoding.ASCII.GetBytes(streamInfo.VideoQuality.Label))
                .Should()
                .BeTrue();
        }
    }

    [Fact]
    public async Task I_can_download_a_video_as_a_single_webm_file_with_multiple_streams()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.webm");

        // Act
        var manifest = await youtube.Videos.Streams.GetManifestAsync("ngqcjXfggHQ");

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

        foreach (var streamInfo in audioStreamInfos)
        {
            if (streamInfo.AudioLanguage is not null)
            {
                File.ContainsBytes(
                        filePath,
                        Encoding.ASCII.GetBytes(streamInfo.AudioLanguage.Value.Name)
                    )
                    .Should()
                    .BeTrue();
            }
        }

        foreach (var streamInfo in videoStreamInfos)
        {
            File.ContainsBytes(filePath, Encoding.ASCII.GetBytes(streamInfo.VideoQuality.Label))
                .Should()
                .BeTrue();
        }
    }

    [Fact]
    public async Task I_can_download_a_video_with_custom_conversion_settings()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp3");

        // Act
        await youtube.Videos.DownloadAsync(
            "9bZkp7q19f0",
            filePath,
            o =>
                o.SetFFmpegPath(FFmpeg.FilePath)
                    .SetContainer("mp4")
                    .SetPreset(ConversionPreset.UltraFast)
        );

        // Assert
        MediaFormat.IsMp4File(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_try_to_download_a_video_and_get_an_error_if_the_conversion_settings_are_invalid()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        // Act & assert
        var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
            await youtube.Videos.DownloadAsync(
                "9bZkp7q19f0",
                filePath,
                o =>
                    o.SetFFmpegPath(FFmpeg.FilePath)
                        .SetContainer("invalid_format")
                        .SetPreset(ConversionPreset.UltraFast)
            )
        );

        Directory.EnumerateFiles(dir.Path, "*", SearchOption.AllDirectories).Should().BeEmpty();

        testOutput.WriteLine(ex.ToString());
    }

    [Fact]
    public async Task I_can_download_a_video_while_tracking_progress()
    {
        // Arrange
        using var youtube = new YoutubeClient();

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
            testOutput.WriteLine($"Progress: {value:P2}");
    }
}
