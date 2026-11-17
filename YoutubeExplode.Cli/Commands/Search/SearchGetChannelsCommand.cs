using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Common;

namespace YoutubeExplode.Cli.Commands.Search;

[Command("search get-channels", Description = "Searches for channels matching a query.")]
public partial class SearchGetChannelsCommand : SearchCommandBase
{
    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    ) =>
        await WriteJsonAsync(
            console,
            await youtube.Search.GetChannelsAsync(Query, cancellationToken)
        );
}
