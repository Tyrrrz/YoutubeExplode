using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Cli.Commands.Videos.ClosedCaptions;

[Command(
    "video cc manifest",
    Description = "Gets the manifest of closed caption tracks available for a video."
)]
public partial class ClosedCaptionManifestCommand : CommandBase
{
    [CommandParameter(0, Description = "Video ID or URL.")]
    public required VideoId VideoId { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId, cancellationToken)
        );
}
