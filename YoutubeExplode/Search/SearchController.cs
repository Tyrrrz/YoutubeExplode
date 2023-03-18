using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Search;

internal class SearchController
{
    private readonly HttpClient _http;

    public SearchController(HttpClient http) => _http = http;

    public async ValueTask<SearchResponse> GetSearchResponseAsync(
        string searchQuery,
        SearchFilter searchFilter,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/search")
        {
            // ReSharper disable VariableHidesOuterVariable
            Content = new StringContent(Json.Create(x => x.Object(x => x
                .Property("query", x => x.String(searchQuery))
                .Property("params", x => x.String(searchFilter switch
                {
                    SearchFilter.Video => "EgIQAQ%3D%3D",
                    SearchFilter.Playlist => "EgIQAw%3D%3D",
                    SearchFilter.Channel => "EgIQAg%3D%3D",
                    _ => null
                }))
                .Property("continuation", x => x.String(continuationToken))
                .Property("context", x => x.Object(x => x
                    .Property("client", x => x.Object(x => x
                        .Property("clientName", x => x.String("WEB"))
                        .Property("clientVersion", x => x.String("2.20210408.08.00"))
                        .Property("hl", x => x.String("en"))
                        .Property("gl", x => x.String("US"))
                        .Property("utcOffsetMinutes", x => x.Number(0))
                    ))
                ))
            )))
            // ReSharper restore VariableHidesOuterVariable
        };

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return SearchResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );
    }
}