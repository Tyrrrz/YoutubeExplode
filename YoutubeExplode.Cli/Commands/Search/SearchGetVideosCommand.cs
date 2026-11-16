using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;

namespace YoutubeExplode.Cli.Commands.Search;

[Command("search get-videos", Description = "Searches for videos matching a query.")]
public partial class SearchGetVideosCommand : SearchCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        CancellationToken cancellationToken
    ) =>
        WriteJson(
            console,
            await ToListAsync(
                Youtube.Search.GetVideosAsync(Query, cancellationToken),
                cancellationToken
            )
        );
}
