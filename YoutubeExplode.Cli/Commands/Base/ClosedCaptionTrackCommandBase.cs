using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single closed caption track, identified by track info JSON from stdin
public abstract class ClosedCaptionTrackCommandBase : CommandBase
{
    protected static async ValueTask<ClosedCaptionTrackInfo> ReadClosedCaptionTrackInfoAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) => await ReadJsonAsync<ClosedCaptionTrackInfo>(console, cancellationToken);
}
