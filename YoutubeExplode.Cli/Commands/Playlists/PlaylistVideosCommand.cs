using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Cli.Commands.Playlists;

[Command("playlist videos", Description = "Gets the videos included in a playlist.")]
public partial class PlaylistVideosCommand : CommandBase
{
    [CommandParameter(0, Name = "id-or-url", Description = "Playlist ID or URL.")]
    public required PlaylistId PlaylistId { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Playlists.GetVideosAsync(PlaylistId, cancellationToken)
        );
}
