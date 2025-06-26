using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Converter.Tests.Utils;

namespace YoutubeExplode.Converter.Tests;

public class EnvironmentSpecs : IAsyncLifetime
{
    public async Task InitializeAsync() => await FFmpeg.InitializeAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task I_can_download_a_video_with_custom_environment_variables_passed_to_FFmpeg()
    {
        // Arrange
        var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        var logFilePath = Path.Combine(dir.Path, "ffreport.log");

        // Act
        await youtube.Videos.DownloadAsync(
            "9bZkp7q19f0",
            filePath,
            o => o
                .SetFFmpegPath(FFmpeg.FilePath)
                .SetEnvironmentVariable("FFREPORT", $"file={logFilePath}:level=32")
        );

        // Assert
        File.Exists(logFilePath).Should().BeTrue();
    }
}