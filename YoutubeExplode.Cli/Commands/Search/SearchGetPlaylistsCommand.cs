using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Common;

namespace YoutubeExplode.Cli.Commands.Search;

[Command("search get-playlists", Description = "Searches for playlists matching a query.")]
public partial class SearchGetPlaylistsCommand : SearchCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Search.GetPlaylistsAsync(Query, cancellationToken)
        );
}
