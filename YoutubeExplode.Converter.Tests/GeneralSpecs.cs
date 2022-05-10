using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Gress;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Converter.Tests.Fixtures;
using YoutubeExplode.Converter.Tests.Utils;

namespace YoutubeExplode.Converter.Tests;

public class GeneralSpecs : IClassFixture<TempOutputFixture>, IClassFixture<FFmpegFixture>
{
    private readonly ITestOutputHelper _testOutput;
    private readonly TempOutputFixture _tempOutputFixture;
    private readonly FFmpegFixture _ffmpegFixture;

    public GeneralSpecs(
        ITestOutputHelper testOutput,
        TempOutputFixture tempOutputFixture,
        FFmpegFixture ffmpegFixture)
    {
        _testOutput = testOutput;
        _tempOutputFixture = tempOutputFixture;
        _ffmpegFixture = ffmpegFixture;
    }

    [Fact]
    public async Task User_can_download_a_video_by_merging_best_streams_into_a_single_mp4_file()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "mp4");

        // Act
        await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath);

        // Assert
        MediaFormat.IsMp4File(outputFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task User_can_download_a_video_by_merging_best_streams_into_a_single_webm_file()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "webm");

        // Act
        await youtube.Videos.DownloadAsync("5NmxuoNyDss", outputFilePath);

        // Assert
        MediaFormat.IsWebMFile(outputFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task User_can_download_a_video_by_merging_best_streams_into_a_single_mp3_file()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "mp3");

        // Act
        await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath);

        // Assert
        MediaFormat.IsMp3File(outputFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task User_can_download_a_video_by_merging_best_streams_into_a_single_ogg_file()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = Path.ChangeExtension(_tempOutputFixture.GetTempFilePath(), "ogg");

        // Act
        await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath);

        // Assert
        MediaFormat.IsOggFile(outputFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task User_can_download_a_video_with_custom_conversion_settings()
    {
        // Arrange
        var youtube = new YoutubeClient();
        var outputFilePath = _tempOutputFixture.GetTempFilePath();

        // Act
        await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath, o => o
            .SetFFmpegPath(_ffmpegFixture.FilePath)
            .SetContainer("mp4")
            .SetPreset(ConversionPreset.UltraFast)
        );

        // Assert
        MediaFormat.IsMp4File(outputFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task User_can_download_a_video_and_track_the_progress_of_the_operation()
    {
        // Arrange
        var progress = new ProgressCollector<double>();

        var youtube = new YoutubeClient();
        var outputFilePath = _tempOutputFixture.GetTempFilePath();

        // Act
        await youtube.Videos.DownloadAsync("AI7ULzgf8RU", outputFilePath, progress);

        // Assert
        var progressValues = progress.GetValues();

        progressValues.Should().NotBeEmpty();

        foreach (var value in progress.GetValues())
            _testOutput.WriteLine($"Progress: {value:P2}");
    }
}