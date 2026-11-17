using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Cli.Commands.Videos;

[Command("video get", Description = "Gets the metadata of a video.")]
public partial class VideosGetCommand : CommandBase
{
    [CommandParameter(0, Name = "id-or-url", Description = "Video ID or URL.")]
    public required string VideoIdOrUrl { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Videos.GetAsync(VideoId.Parse(VideoIdOrUrl), cancellationToken)
        );
}
