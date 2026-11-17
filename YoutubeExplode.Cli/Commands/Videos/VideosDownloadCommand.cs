using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using PowerKit.Extensions;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Cli.Utils.Extensions;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Cli.Commands.Videos;

[Command(
    "videos download",
    Description = "Downloads a video by muxing its best streams using FFmpeg."
)]
public partial class VideosDownloadCommand : CommandBase
{
    [CommandParameter(0, Name = "id-or-url", Description = "Video ID or URL.")]
    public required string VideoIdOrUrl { get; set; }

    [CommandOption(
        "output",
        'o',
        Description = "Output file path. Defaults to '<video-id>.<container>'."
    )]
    public string? OutputFilePath { get; set; }

    [CommandOption(
        "container",
        'c',
        Description = "Output container (e.g. mp4, webm, mp3). Defaults to the output file extension, or mp4."
    )]
    public string? Container { get; set; }

    [CommandOption("preset", Description = "Encoder speed preset.")]
    public ConversionPreset Preset { get; set; } = ConversionPreset.Medium;

    [CommandOption(
        "ffmpeg-path",
        EnvironmentVariable = "FFMPEG_PATH",
        Description = "Path to the FFmpeg executable or to the directory containing it. "
            + "Defaults to the probe directory or PATH."
    )]
    public string? FFmpegPath { get; set; }

    private string ResolveContainer()
    {
        if (!string.IsNullOrWhiteSpace(Container))
            return Container.TrimStart('.');

        var extension = Path.GetExtension(OutputFilePath ?? "").TrimStart('.');
        return !string.IsNullOrWhiteSpace(extension) ? extension : "mp4";
    }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    )
    {
        var videoId = VideoId.Parse(VideoIdOrUrl);
        var container = ResolveContainer();
        var outputFilePath = OutputFilePath ?? $"{videoId}.{container}";
        Directory.CreateForFile(outputFilePath);

        try
        {
            await console.RunWithProgressAsync(
                $"Downloading {videoId}",
                async progress =>
                    await youtube.Videos.DownloadAsync(
                        videoId,
                        outputFilePath,
                        o =>
                        {
                            o.SetContainer(container).SetPreset(Preset);

                            if (!string.IsNullOrWhiteSpace(FFmpegPath))
                                o.SetFFmpegPath(FFmpegPath);
                        },
                        progress,
                        cancellationToken
                    )
            );
        }
        catch (FileNotFoundException ex)
        {
            throw new CommandException(
                $"""
                {ex.Message}

                FFmpeg is required for this command. Install it from https://ffmpeg.org/download.html
                and make sure it is available on the PATH, or specify its location using
                the --ffmpeg-path option or the FFMPEG_PATH environment variable.
                """,
                innerException: ex
            );
        }

        console.Error.WriteLine($"Video saved to '{outputFilePath}'.");
    }
}
