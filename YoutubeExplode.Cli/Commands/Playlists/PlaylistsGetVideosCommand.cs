using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Cli.Commands.Playlists;

[Command("playlists get-videos", Description = "Gets the videos included in a playlist.")]
public partial class PlaylistsGetVideosCommand : CommandBase
{
    [CommandParameter(0, Name = "id-or-url", Description = "Playlist ID or URL.")]
    public required string PlaylistIdOrUrl { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) =>
        WriteJson(
            console,
            await ToListAsync(
                Youtube.Playlists.GetVideosAsync(
                    PlaylistId.Parse(PlaylistIdOrUrl),
                    cancellationToken
                ),
                cancellationToken
            )
        );
}
