using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Cli.Commands.Playlists;

[Command("playlist get", Description = "Gets the metadata of a playlist.")]
public partial class PlaylistsGetCommand : CommandBase
{
    [CommandParameter(0, Name = "id-or-url", Description = "Playlist ID or URL.")]
    public required string PlaylistIdOrUrl { get; set; }

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Playlists.GetAsync(PlaylistId.Parse(PlaylistIdOrUrl), cancellationToken)
        );
}
