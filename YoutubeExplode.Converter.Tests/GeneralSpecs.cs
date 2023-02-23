using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Gress;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Converter.Tests.Utils;

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
    public async Task I_can_download_a_video_using_custom_conversion_settings()
    {
        // Arrange
        var youtube = new YoutubeClient();
        using var file = TempFile.Create();

        // Act
        await youtube.Videos.DownloadAsync("9bZkp7q19f0", file.Path, o => o
            .SetFFmpegPath(FFmpeg.FilePath)
            .SetContainer("mp4")
            .SetPreset(ConversionPreset.UltraFast)
        );

        // Assert
        MediaFormat.IsMp4File(file.Path).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_download_a_video_and_track_the_progress()
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

        foreach (var value in progress.GetValues())
            _testOutput.WriteLine($"Progress: {value:P2}");
    }
}