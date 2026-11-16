using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
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
        CancellationToken cancellationToken
    )
    {
        var streamInfo = await ResolveStreamInfoAsync(cancellationToken);
        EnsureDirectoryExists(OutputFilePath);

        await console.RunWithProgressAsync(
            "Downloading stream",
            async progress =>
                await Youtube.Videos.Streams.DownloadAsync(
                    streamInfo,
                    OutputFilePath,
                    progress,
                    cancellationToken
                )
        );

        console.Error.WriteLine($"Stream saved to '{OutputFilePath}'.");
    }
}
