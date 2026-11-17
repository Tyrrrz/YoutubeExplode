using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Utils;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single stream, identified by stream info JSON from stdin
public abstract class StreamCommandBase : CommandBase
{
    protected static async ValueTask<IStreamInfo> ReadStreamInfoAsync(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var json = await ReadJsonAsync(console, cancellationToken);
        return (
            json.Deserialize(JsonSerializerContext.Default.StreamInfoDto)
            ?? throw new JsonException("Stream info is missing.")
        ).ToStreamInfo();
    }
}
