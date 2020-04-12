using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class VideoInfoResponse
    {
        private readonly IReadOnlyDictionary<string, string> _root;

        public VideoInfoResponse(IReadOnlyDictionary<string, string> root)
        {
            _root = root;
        }

        public string GetStatus() => _root["status"];

        public bool IsVideoAvailable() =>
            !string.Equals(GetStatus(), "fail", StringComparison.OrdinalIgnoreCase);

        public PlayerResponse GetPlayerResponse() => _root["player_response"]
            .Pipe(PlayerResponse.Parse);

        public IEnumerable<StreamInfo> GetMuxedStreams() => Fallback.ToEmpty(
            _root
                .GetValueOrDefault("url_encoded_fmt_stream_map")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new StreamInfo(d))
        );

        public IEnumerable<StreamInfo> GetAdaptiveStreams() => Fallback.ToEmpty(
            _root
                .GetValueOrDefault("adaptive_fmts")?
                .Split(",")
                .Select(Url.SplitQuery)
                .Select(d => new StreamInfo(d))
        );
    }

    internal partial class VideoInfoResponse
    {
        public class StreamInfo
        {
            private readonly IReadOnlyDictionary<string, string> _root;

            public StreamInfo(IReadOnlyDictionary<string, string> root)
            {
                _root = root;
            }

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

            public double GetBitrate() => _root["bitrate"]
                .ParseDouble();

            public string GetMimeType() => _root["type"];

            public bool IsMuxed() => GetMimeType()
                .SubstringAfter("codecs=\"")
                .SubstringUntil("\"")
                .Split(", ")
                .Length >= 2;

            public bool IsAudioOnly() => GetMimeType()
                .StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

            public string GetContainer() => GetMimeType()
                .SubstringUntil(";")
                .SubstringAfter("/");

            public string GetAudioCodec() => GetMimeType()
                .SubstringAfter("codecs=\"")
                .SubstringUntil("\"")
                .Split(", ")
                .Last();

            public string GetVideoCodec() => GetMimeType()
                .SubstringAfter("codecs=\"")
                .SubstringUntil("\"")
                .Split(", ")
                .First();

            public string GetVideoQualityLabel() => _root["quality_label"];

            public int GetVideoWidth() => _root["size"]
                .SubstringUntil("x")
                .ParseInt();

            public int GetVideoHeight() => _root["size"]
                .SubstringAfter("x")
                .ParseInt();

            public int GetFramerate() => _root["fps"]
                .ParseInt();
        }
    }

    internal partial class VideoInfoResponse
    {
        public static VideoInfoResponse Parse(string raw) => new VideoInfoResponse(Url.SplitQuery(raw));

        public static async Task<VideoInfoResponse> GetAsync(HttpClient httpClient, string videoId, string? sts = null) =>
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