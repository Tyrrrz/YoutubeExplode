using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;

namespace YoutubeExplode.Cli.Commands.Videos.ClosedCaptions;

[Command(
    "videos closed-captions get",
    Description = "Gets the contents of a closed caption track."
)]
public partial class ClosedCaptionsGetCommand : ClosedCaptionTrackCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) =>
        WriteJson(
            console,
            await Youtube.Videos.ClosedCaptions.GetAsync(
                await ResolveTrackInfoAsync(cancellationToken),
                cancellationToken
            )
        );
}
