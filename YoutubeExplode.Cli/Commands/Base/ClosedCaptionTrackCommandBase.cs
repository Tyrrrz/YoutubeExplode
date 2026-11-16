using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Cli.Commands.Base;

// Commands that operate on a single closed caption track, identified either by URL or by JSON from stdin
public abstract partial class ClosedCaptionTrackCommandBase : CommandBase
{
    [CommandParameter(
        0,
        Name = "url",
        Description = "Closed caption track URL. If omitted, track info JSON is read from the standard input."
    )]
    public string? TrackUrl { get; set; }

    protected async ValueTask<ClosedCaptionTrackInfo> ResolveTrackInfoAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) =>
        !string.IsNullOrWhiteSpace(TrackUrl)
            ? new ClosedCaptionTrackInfo(TrackUrl, default, false)
            : await ReadJsonAsync<ClosedCaptionTrackInfo>(console, cancellationToken);
}
