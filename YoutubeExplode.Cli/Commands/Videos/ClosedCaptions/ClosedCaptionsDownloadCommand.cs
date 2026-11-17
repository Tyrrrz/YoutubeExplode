using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using PowerKit.Extensions;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Cli.Utils.Extensions;

namespace YoutubeExplode.Cli.Commands.Videos.ClosedCaptions;

[Command(
    "videos closed-captions download",
    Description = "Downloads a closed caption track to an SRT file."
)]
public partial class ClosedCaptionsDownloadCommand : ClosedCaptionTrackCommandBase
{
    [CommandOption("output", 'o', Description = "Output file path.")]
    public required string OutputFilePath { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    )
    {
        var trackInfo = await ReadClosedCaptionTrackInfoAsync(console, cancellationToken);
        Directory.CreateForFile(OutputFilePath);

        await console.RunWithProgressAsync(
            "Downloading closed captions",
            async progress =>
                await youtube.Videos.ClosedCaptions.DownloadAsync(
                    trackInfo,
                    OutputFilePath,
                    progress,
                    cancellationToken
                )
        );

        console.Error.WriteLine($"Closed captions saved to '{OutputFilePath}'.");
    }
}
