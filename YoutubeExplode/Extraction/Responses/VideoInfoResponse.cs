using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class VideoInfoResponse
    {
        private readonly IReadOnlyDictionary<string, string> _root;
        private readonly Memo _memo = new();

        public VideoInfoResponse(IReadOnlyDictionary<string, string> root) => _root = root;

        public string? TryGetStatus() => _memo.Wrap(() => _root.GetValueOrDefault("status"));

        public bool IsVideoAvailable() => _memo.Wrap(() =>
            !string.Equals(TryGetStatus(), "fail", StringComparison.OrdinalIgnoreCase)
        );

        public PlayerResponse? TryGetPlayerResponse() => _memo.Wrap(() =>
            _root
                .GetValueOrDefault("player_response")?
                .Pipe(Json.TryParse)?
                .Pipe(j => new PlayerResponse(j))
        );

        public IReadOnlyList<IStreamInfoResponse> GetStreams() => _memo.Wrap(() =>
        {
            var result = new List<IStreamInfoResponse>();

            var muxedStreams = _root
                .GetValueOrDefault("url_encoded_fmt_stream_map")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoResponse(d));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = _root
                .GetValueOrDefault("adaptive_fmts")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoResponse(d));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        });
    }
}