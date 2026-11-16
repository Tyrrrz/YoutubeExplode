using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single stream, identified by stream info JSON from stdin
public abstract class StreamCommandBase : CommandBase
{
    protected static async ValueTask<IStreamInfo> ReadStreamInfoAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) => await ReadJsonAsync<IStreamInfo>(console, cancellationToken);
}
