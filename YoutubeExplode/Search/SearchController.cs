using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Search;

internal class SearchController : YoutubeControllerBase
{
    public SearchController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<SearchResultsExtractor> GetSearchResultsAsync(
        string searchQuery,
        SearchFilter searchFilter,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/search")
        {
            Content = Json.SerializeToHttpContent(new
            {
                query = searchQuery,
                @params = searchFilter switch
                {
                    SearchFilter.Video => "EgIQAQ%3D%3D",
                    SearchFilter.Playlist => "EgIQAw%3D%3D",
                    SearchFilter.Channel => "EgIQAg%3D%3D",
                    _ => null
                },
                continuation = continuationToken,
                context = new
                {
                    client = new
                    {
                        clientName = "WEB",
                        clientVersion = "2.20210408.08.00",
                        hl = "en",
                        gl = "US",
                        utcOffsetMinutes = 0
                    }
                }
            })
        };

        var raw = await GetStringAsync(request, cancellationToken);
        return SearchResultsExtractor.Create(raw);
    }
}