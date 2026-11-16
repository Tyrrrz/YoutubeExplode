using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Cli.Commands.Videos.ClosedCaptions;

[Command(
    "videos closed-captions get-manifest",
    Description = "Gets the manifest of closed caption tracks available for a video."
)]
public partial class ClosedCaptionsGetManifestCommand : CommandBase
{
    [CommandParameter(0, Name = "id-or-url", Description = "Video ID or URL.")]
    public required string VideoIdOrUrl { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) =>
        WriteJson(
            console,
            (
                await Youtube.Videos.ClosedCaptions.GetManifestAsync(
                    VideoId.Parse(VideoIdOrUrl),
                    cancellationToken
                )
            ).Tracks
        );
}
