using System.Text.Json;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class SearchResultsExtractor
    {
        private readonly JsonElement _content;
        private readonly Memo _memo = new();

        public SearchResultsExtractor(JsonElement content) => _content = content;
    }

    internal partial class SearchResultsExtractor
    {
        public static SearchResultsExtractor Create(string raw) => new(Json.Parse(raw));
    }
}