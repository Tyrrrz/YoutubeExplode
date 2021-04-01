using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.ReverseEngineering
{
    internal partial class VideoInfoResponse
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public VideoInfoResponse(IReadOnlyDictionary<string, string> root) => _root = root;

        private string GetStatus() => _root["status"];

        public bool IsVideoAvailable() =>
            !string.Equals(GetStatus(), "fail", StringComparison.OrdinalIgnoreCase);

        public PlayerResponse GetPlayerResponse() => _root["player_response"]
            .Pipe(PlayerResponse.Parse);

        private IEnumerable<StreamInfo> GetMuxedStreams() => Fallback.ToEmpty(
            _root
                .GetValueOrDefault("url_encoded_fmt_stream_map")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new StreamInfo(d))
        );

        private IEnumerable<StreamInfo> GetAdaptiveStreams() => Fallback.ToEmpty(
            _root
                .GetValueOrDefault("adaptive_fmts")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new StreamInfo(d))
        );

        public IEnumerable<StreamInfo> GetStreams() => GetMuxedStreams().Concat(GetAdaptiveStreams());
    }

    internal partial class VideoInfoResponse
    {
        public class StreamInfo : IStreamInfoProvider
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public StreamInfo(IReadOnlyDictionary<string, string> root) => _root = root;

            public int GetTag() => _root["itag"].ParseInt();

            public string GetUrl() => _root["url"];

            public string? TryGetSignature() => _root.GetValueOrDefault("s");

            public string? TryGetSignatureParameter() => _root.GetValueOrDefault("sp");

            public long? TryGetContentLength() =>
                _root
                    .GetValueOrDefault("clen")?
                    .ParseLong() ??
                GetUrl()
                    .Pipe(s => Regex.Match(s, @"clen=(\d+)").Groups[1].Value)
                    .NullIfWhiteSpace()?
                    .ParseLong();

            public long GetBitrate() => _root["bitrate"]
                .ParseLong();

            private string GetMimeType() => _root["type"];

            public string GetContainer() => GetMimeType()
                .SubstringUntil(";")
                .SubstringAfter("/");

            private bool IsAudioOnly() => GetMimeType()
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            private string GetCodecs() => GetMimeType()
                .SubstringAfter("codecs=\"")
                .SubstringUntil("\"");

            public string? TryGetAudioCodec() =>
                IsAudioOnly()
                    ? GetCodecs()
                    : GetCodecs().SubstringAfter(", ").NullIfWhiteSpace();

            public string? TryGetVideoCodec() =>
                IsAudioOnly()
                    ? null
                    : GetCodecs().SubstringUntil(", ").NullIfWhiteSpace();

            public string? TryGetVideoQualityLabel() => _root
                .GetValueOrDefault("quality_label");

            public int? TryGetVideoWidth() => _root
                .GetValueOrDefault("size")?
                .SubstringUntil("x")
                .NullIfWhiteSpace()?
                .ParseInt();

            public int? TryGetVideoHeight() => _root
                .GetValueOrDefault("size")?
                .SubstringAfter("x")
                .NullIfWhiteSpace()?
                .ParseInt();

            public int? TryGetFramerate() => _root
                .GetValueOrDefault("fps")?
                .ParseInt();
        }
    }

    internal partial class VideoInfoResponse
    {
        public static VideoInfoResponse Parse(string raw) => new(Url.SplitQuery(raw));

        public static async Task<VideoInfoResponse> GetAsync(YoutubeHttpClient httpClient, string videoId, string? sts = null) =>
            await Retry.WrapAsync(async () =>
            {
                var eurl = WebUtility.HtmlEncode($"https://youtube.googleapis.com/v/{videoId}");

                var url = $"https://youtube.com/get_video_info?video_id={videoId}&el=embedded&eurl={eurl}&hl=en&sts={sts}";
                var raw = await httpClient.GetStringAsync(url);

                var result = Parse(raw);

                if (!result.IsVideoAvailable() || !result.GetPlayerResponse().IsVideoAvailable())
                    throw VideoUnavailableException.Unavailable(videoId);

                return result;
            });
    }
}