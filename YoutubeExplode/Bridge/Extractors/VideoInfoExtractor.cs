using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class VideoInfoExtractor
    {
        private readonly IReadOnlyDictionary<string, string> _content;
        private readonly Memo _memo = new();

        public VideoInfoExtractor(IReadOnlyDictionary<string, string> content) => _content = content;

        public string? TryGetStatus() => _memo.Wrap(() =>
            _content.GetValueOrDefault("status")
        );

        public bool IsVideoAvailable() => _memo.Wrap(() =>
            !string.Equals(TryGetStatus(), "fail", StringComparison.OrdinalIgnoreCase)
        );

        public PlayerResponseExtractor? TryGetPlayerResponse() => _memo.Wrap(() =>
            _content
                .GetValueOrDefault("player_response")?
                .Pipe(Json.TryParse)?
                .Pipe(j => new PlayerResponseExtractor(j))
        );

        public IReadOnlyList<IStreamInfoExtractor> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoExtractor>();

            var muxedStreams = _content
                .GetValueOrDefault("url_encoded_fmt_stream_map")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoExtractor(d));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = _content
                .GetValueOrDefault("adaptive_fmts")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoExtractor(d));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });
    }

    internal partial class VideoInfoExtractor
    {
        public static VideoInfoExtractor Create(string raw)
        {
            var content = Url.SplitQuery(raw);
            return new VideoInfoExtractor(content);
        }
    }
}