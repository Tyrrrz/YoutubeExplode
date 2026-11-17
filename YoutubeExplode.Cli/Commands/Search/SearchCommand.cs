using System.Threading;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using YoutubeExplode.Cli.Commands.Base;
using YoutubeExplode.Common;

namespace YoutubeExplode.Cli.Commands.Search;

[Command("search", Description = "Searches YouTube for videos, playlists and channels.")]
public partial class SearchCommand : CommandBase
{
    [CommandParameter(0, Name = "query", Description = "Search query.")]
    public required string Query { get; set; }

    [CommandOption(
        "filter",
        'f',
        Description = "Restrict results to a specific kind (None, Videos, Playlists, Channels)."
    )]
    public SearchFilter Filter { get; set; } = SearchFilter.None;

    protected override async ValueTask ExecuteAsync(
        IConsole console,
        YoutubeClient youtube,
        CancellationToken cancellationToken
    )
    {
        switch (Filter)
        {
            case SearchFilter.Videos:
                await WriteJsonAsync(
                    console,
                    await youtube.Search.GetVideosAsync(Query, cancellationToken)
                );
                break;
            case SearchFilter.Playlists:
                await WriteJsonAsync(
                    console,
                    await youtube.Search.GetPlaylistsAsync(Query, cancellationToken)
                );
                break;
            case SearchFilter.Channels:
                await WriteJsonAsync(
                    console,
                    await youtube.Search.GetChannelsAsync(Query, cancellationToken)
                );
                break;
            default:
                await WriteJsonAsync(
                    console,
                    await youtube.Search.GetResultsAsync(Query, cancellationToken)
                );
                break;
        }
    }
}
