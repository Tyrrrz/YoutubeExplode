using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;

namespace YoutubeExplode.Cli.Commands.Videos.Streams;

[Command(
    "videos streams get",
    Description = "Writes the binary data of a stream to the standard output."
)]
public partial class StreamsGetCommand : StreamCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var streamInfo = await ResolveStreamInfoAsync(cancellationToken);

        // Bypass the text writer, since this is raw binary data
        await using var output = Console.OpenStandardOutput();
        await Youtube.Videos.Streams.CopyToAsync(
            streamInfo,
            output,
            cancellationToken: cancellationToken
        );
    }
}
