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
        const string url = $"https://www.youtube.com/youtubei/v1/search?key={ApiKey}";

        var payload = new
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
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = Json.SerializeToHttpContent(payload)
        };

        var raw = await SendHttpRequestAsync(request, cancellationToken);
        return SearchResultsExtractor.Create(raw);
    }
}