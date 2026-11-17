using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;

namespace YoutubeExplode.Cli.Commands.Videos.ClosedCaptions;

[Command("video cc get", Description = "Gets the contents of a closed caption track.")]
public partial class ClosedCaptionGetCommand : ClosedCaptionTrackCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Videos.ClosedCaptions.GetAsync(
                await ReadClosedCaptionTrackInfoAsync(console, cancellationToken),
                cancellationToken
            )
        );
}
