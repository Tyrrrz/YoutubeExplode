using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Extractors;
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

        var payload = new Dictionary<string, object?>
        {
            ["query"] = searchQuery,
            ["params"] = searchFilter switch
            {
                SearchFilter.Video => "EgIQAQ%3D%3D",
                SearchFilter.Playlist => "EgIQAw%3D%3D",
                SearchFilter.Channel => "EgIQAg%3D%3D",
                _ => null
            },
            ["continuation"] = continuationToken,
            ["context"] = new Dictionary<string, object?>
            {
                ["client"] = new Dictionary<string, object?>
                {
                    ["clientName"] = "WEB",
                    ["clientVersion"] = "2.20210408.08.00",
                    ["hl"] = "en",
                    ["gl"] = "US",
                    ["utcOffsetMinutes"] = 0
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