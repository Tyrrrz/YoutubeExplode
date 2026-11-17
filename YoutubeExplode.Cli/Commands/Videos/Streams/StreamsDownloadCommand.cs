using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using PowerKit.Extensions;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Cli.Utils.Extensions;

namespace YoutubeExplode.Cli.Commands.Videos.Streams;

[Command("videos streams download", Description = "Downloads a stream to a file.")]
public partial class StreamsDownloadCommand : StreamCommandBase
{
    [CommandOption("output", 'o', Description = "Output file path.")]
    public required string OutputFilePath { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    )
    {
        var streamInfo = await ReadStreamInfoAsync(console, cancellationToken);
        Directory.CreateForFile(OutputFilePath);

        await console.RunWithProgressAsync(
            "Downloading stream",
            async progress =>
                await youtube.Videos.Streams.DownloadAsync(
                    streamInfo,
                    OutputFilePath,
                    progress,
                    cancellationToken
                )
        );

        console.Error.WriteLine($"Stream saved to '{OutputFilePath}'.");
    }
}
