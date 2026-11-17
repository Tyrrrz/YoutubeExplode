using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Utils;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single closed caption track, identified by track info JSON from stdin
public abstract class ClosedCaptionTrackCommandBase : CommandBase
{
    protected static async ValueTask<ClosedCaptionTrackInfo> ReadClosedCaptionTrackInfoAsync(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var json = await ReadJsonAsync(console, cancellationToken);

        return json.Deserialize(JsonSerializerContext.Default.ClosedCaptionTrackInfo)
            ?? throw new JsonException("Closed caption track info is missing.");
    }
}
