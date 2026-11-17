using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using Gress;
using PowerKit.Extensions;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Cli.Utils.Extensions;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Cli.Commands.Videos;

[Command(
    "video download",
    Description = "Downloads a video by muxing its best streams using FFmpeg."
)]
public partial class VideoDownloadCommand : CommandBase
{
    [CommandParameter(0, Description = "Video ID or URL.")]
    public required VideoId VideoId { get; set; }

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

    private string ResolveContainer() =>
        Container?.NullIfWhiteSpace()?.TrimStart('.')
        ?? Path.GetExtension(OutputFilePath ?? "").NullIfWhiteSpace()?.TrimStart('.')
        ?? "mp4";

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    )
    {
        var container = ResolveContainer();
        var outputFilePath = OutputFilePath ?? $"{VideoId}.{container}";
        Directory.CreateForFile(outputFilePath);

        try
        {
            await console.RunWithProgressAsync(
                $"Downloading {VideoId}",
                async progress =>
                    await youtube.Videos.DownloadAsync(
                        VideoId,
                        outputFilePath,
                        o =>
                        {
                            o.SetContainer(container).SetPreset(Preset);

                            if (!string.IsNullOrWhiteSpace(FFmpegPath))
                                o.SetFFmpegPath(FFmpegPath);
                        },
                        progress.ToDoubleBased(),
                        cancellationToken
                    )
            );
        }
        catch (Win32Exception ex)
        {
            throw new CommandException(
                $"""
                FFmpeg could not be started: {ex.Message}

                FFmpeg is required for this command. To install it, use one of the following:
                  - Windows: winget install Gyan.FFmpeg (or: choco install ffmpeg, scoop install ffmpeg)
                  - macOS: brew install ffmpeg
                  - Debian/Ubuntu: sudo apt install ffmpeg
                  - Fedora: sudo dnf install ffmpeg-free
                  - Arch Linux: sudo pacman -S ffmpeg
                  - NixOS: nix-shell -p ffmpeg

                Alternatively, you can download FFmpeg manually from here:
                  - https://ffmpeg.org/download.html
                  - https://github.com/Tyrrrz/FFmpegBin

                Make sure FFmpeg is available on the PATH, or specify its location using
                the --ffmpeg-path option or the FFMPEG_PATH environment variable.
                """,
                innerException: ex
            );
        }

        console.Error.WriteLine($"Video saved to '{outputFilePath}'.");
    }
}
