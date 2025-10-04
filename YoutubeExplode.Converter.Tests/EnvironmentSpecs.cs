using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Converter.Tests.Utils;

namespace YoutubeExplode.Converter.Tests;

public class EnvironmentSpecs(ITestOutputHelper testOutput) : IAsyncLifetime
{
    public async Task InitializeAsync() => await FFmpeg.InitializeAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task I_can_download_a_video_with_custom_environment_variables_passed_to_FFmpeg()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "video.mp4");

        var logFilePath = Path.Combine(dir.Path, "ffreport.log");

        // Act
        await youtube.Videos.DownloadAsync(
            "9bZkp7q19f0",
            filePath,
            o =>
            {
                // FFREPORT file path must be relative to the current working directory
                var logFilePathFormatted = Path.GetRelativePath(
                        Directory.GetCurrentDirectory(),
                        logFilePath
                    )
                    .Replace('\\', '/');

                o.SetFFmpegPath(FFmpeg.FilePath)
                    .SetEnvironmentVariable("FFREPORT", $"file={logFilePathFormatted}:level=32");
            }
        );

        // Assert
        File.Exists(logFilePath).Should().BeTrue();

        testOutput.WriteLine(await File.ReadAllTextAsync(logFilePath));
    }
}
