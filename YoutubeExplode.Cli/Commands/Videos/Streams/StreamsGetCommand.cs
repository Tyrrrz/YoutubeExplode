using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;

namespace YoutubeExplode.Cli.Commands.Videos.Streams;

[Command(
    "video stream get",
    Description = "Writes the binary data of a stream to the standard output."
)]
public partial class StreamsGetCommand : StreamCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    )
    {
        var streamInfo = await ReadStreamInfoAsync(console, cancellationToken);

        // Bypass the text writer, since this is raw binary data
        await youtube.Videos.Streams.CopyToAsync(
            streamInfo,
            console.Output.BaseStream,
            cancellationToken: cancellationToken
        );
    }
}
