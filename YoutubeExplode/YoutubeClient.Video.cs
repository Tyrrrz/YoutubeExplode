using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.CipherOperations;
using YoutubeExplode.Internal.Parsers;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private readonly Dictionary<string, IReadOnlyList<ICipherOperation>> _cipherOperationsCache =
            new Dictionary<string, IReadOnlyList<ICipherOperation>>();

        private async Task<IReadOnlyDictionary<string, string>> GetVideoInfoDicAsync(string videoId, string sts = null)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            // Execute request
            var url = $"https://youtube.com/get_video_info?video_id={videoId}&el=embedded&sts={sts}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            // Parse response as a URL-encoded dictionary
            var result = Url.SplitQuery(raw);

            // If video ID is not set - throw
            if (result.GetValueOrDefault("video_id").IsNullOrWhiteSpace())
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");

            return result;
        }

        private async Task<IHtmlDocument> GetVideoWatchPageHtmlAsync(string videoId)
        {
            var url = $"https://youtube.com/watch?v={videoId}&disable_polymer=true&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return new HtmlParser().Parse(raw);
        }

        private async Task<IHtmlDocument> GetVideoEmbedPageHtmlAsync(string videoId)
        {
            var url = $"https://youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return new HtmlParser().Parse(raw);
        }

        private async Task<IReadOnlyList<ICipherOperation>> GetCipherOperationsAsync(string playerSourceUrl)
        {
            // If already in cache - return
            if (_cipherOperationsCache.TryGetValue(playerSourceUrl, out var cached))
                return cached;

            // Get player source
            var raw = await _httpClient.GetStringAsync(playerSourceUrl);

            // Find the name of the function that handles deciphering
            var deciphererFuncName = Regex.Match(raw,
                @"\bc\s*&&\s*d\.set\([^,]+,\s*(?:encodeURIComponent\s*\()?\s*([\w$]+)\(").Groups[1].Value;

            if (deciphererFuncName.IsNullOrWhiteSpace())
                throw new ParserException("Could not find signature decipherer function name.");

            // Find the body of the function
            var deciphererFuncBody = Regex.Match(raw,
                @"(?!h\.)" + Regex.Escape(deciphererFuncName) + @"=function\(\w+\)\{(.*?)\}", RegexOptions.Singleline).Groups[1].Value;

            if (deciphererFuncBody.IsNullOrWhiteSpace())
                throw new ParserException("Could not find signature decipherer function body.");

            // Split the function body into statements
            var deciphererFuncBodyStatements = deciphererFuncBody.Split(";");

            // Identify cipher functions
            var operations = new List<ICipherOperation>();
            var reverseFuncName = "";
            var sliceFuncName = "";
            var swapFuncName = "";

            // Analyze statements to determine cipher function names
            foreach (var statement in deciphererFuncBodyStatements)
            {
                // Break when all functions are found
                if (!reverseFuncName.IsNullOrWhiteSpace() &&
                    !sliceFuncName.IsNullOrWhiteSpace() &&
                    !swapFuncName.IsNullOrWhiteSpace())
                    break;

                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsNullOrWhiteSpace())
                    continue;

                // Determine cipher function names by signature
                if (Regex.IsMatch(raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    swapFuncName = calledFuncName;
                }
            }

            // Analyze cipher function calls to determine their order and parameters
            foreach (var statement in deciphererFuncBodyStatements)
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsNullOrWhiteSpace())
                    continue;

                // Reverse operation
                if (calledFuncName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
                // Slice operation
                else if (calledFuncName == sliceFuncName)
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }
                // Swap operation
                else if (calledFuncName == swapFuncName)
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }
            }

            return operations;
        }

        private async Task<PlayerConfiguration> GetPlayerConfigurationAsync(string videoId)
        {
            // Try to get from video info
            {
                // Get video embed page HTML
                var videoEmbedPageHtml = await GetVideoEmbedPageHtmlAsync(videoId);

                // Extract player config JSON
                var playerConfigRaw = Regex.Match(videoEmbedPageHtml.Source.Text,
                        @"yt\.setConfig\({'PLAYER_CONFIG': (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                    .Groups["Json"].Value;
                var playerConfigJson = JToken.Parse(playerConfigRaw);

                // Extract STS
                var sts = playerConfigJson.SelectToken("sts").Value<string>();

                // Extract player source URL
                var playerSourceUrl = "https://youtube.com" + playerConfigJson.SelectToken("assets.js").Value<string>();

                // Get video info dictionary
                var requestedAt = DateTimeOffset.Now;
                var videoInfoDic = await GetVideoInfoDicAsync(videoId, sts);

                // Get player response JSON
                var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

                // If there is no error - return
                var errorReason = playerResponseJson.SelectToken("playabilityStatus.reason")?.Value<string>();
                if (errorReason.IsNullOrWhiteSpace())
                {
                    // Extract whether the video is a live stream
                    var isLiveStream = playerResponseJson.SelectToken("videoDetails.isLive")?.Value<bool>() == true;

                    // Get cipher operations (TODO: do it only when needed)
                    var cipherOperations = await GetCipherOperationsAsync(playerSourceUrl);

                    // Extract valid until date
                    var expiresIn = TimeSpan.FromSeconds(playerResponseJson.SelectToken("streamingData.expiresInSeconds").Value<double>());
                    var validUntil = requestedAt + expiresIn;

                    // Extract stream info
                    var hlsManifestUrl =
                        isLiveStream ? playerResponseJson.SelectToken("streamingData.hlsManifestUrl").Value<string>() : null;
                    var dashManifestUrl =
                        !isLiveStream ? playerResponseJson.SelectToken("streamingData.dashManifestUrl").Value<string>() : null;
                    var muxedStreamInfosUrlEncoded =
                        !isLiveStream ? videoInfoDic.GetValueOrDefault("url_encoded_fmt_stream_map") : null;
                    var adaptiveStreamInfosUrlEncoded =
                        !isLiveStream ? videoInfoDic.GetValueOrDefault("adaptive_fmts") : null;

                    return new PlayerConfiguration(cipherOperations, dashManifestUrl, hlsManifestUrl, muxedStreamInfosUrlEncoded,
                        adaptiveStreamInfosUrlEncoded, validUntil);
                }

                // If the video requires purchase - throw
                var previewVideoId = playerResponseJson
                    .SelectToken("playabilityStatus.errorScreen.playerLegacyDesktopYpcTrailerRenderer.trailerVideoId")?.Value<string>();
                if (!previewVideoId.IsNullOrWhiteSpace())
                {
                    throw new VideoRequiresPurchaseException(videoId, previewVideoId,
                        $"Video [{videoId}] is unplayable because it requires purchase.");
                }
            }

            // Try to get from video watch page
            {
                // Get video watch page HTML
                var requestedAt = DateTimeOffset.Now;
                var videoWatchPageHtml = await GetVideoWatchPageHtmlAsync(videoId);

                // Get player config JSON
                var playerConfigRaw = Regex.Match(videoWatchPageHtml.Source.Text,
                        @"ytplayer\.config = (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                    .Groups["Json"].Value;
                var playerConfigJson = JToken.Parse(playerConfigRaw);

                // Get player source URL
                var playerSourceUrl = "https://youtube.com" + playerConfigJson.SelectToken("assets.js").Value<string>();

                // Get player response JSON
                var playerResponseRaw = playerConfigJson.SelectToken("args.player_response").Value<string>();
                var playerResponseJson = JToken.Parse(playerResponseRaw);

                // If there is no error - return
                var errorReason = playerResponseJson.SelectToken("playabilityStatus.reason")?.Value<string>();
                if (errorReason.IsNullOrWhiteSpace())
                {
                    // Extract whether the video is a live stream
                    var isLiveStream = playerResponseJson.SelectToken("videoDetails.isLive")?.Value<bool>() == true;

                    // Get cipher operations (TODO: do it only when needed)
                    var cipherOperations = await GetCipherOperationsAsync(playerSourceUrl);

                    // Extract valid until date
                    var expiresIn = TimeSpan.FromSeconds(playerResponseJson.SelectToken("streamingData.expiresInSeconds").Value<double>());
                    var validUntil = requestedAt + expiresIn;

                    // Extract stream info
                    var hlsManifestUrl =
                        isLiveStream ? playerResponseJson.SelectToken("streamingData.hlsManifestUrl").Value<string>() : null;
                    var dashManifestUrl =
                        !isLiveStream ? playerResponseJson.SelectToken("streamingData.dashManifestUrl").Value<string>() : null;
                    var muxedStreamInfosUrlEncoded =
                        !isLiveStream ? playerConfigJson.SelectToken("args.url_encoded_fmt_stream_map").Value<string>() : null;
                    var adaptiveStreamInfosUrlEncoded =
                        !isLiveStream ? playerConfigJson.SelectToken("args.adaptive_fmts").Value<string>() : null;

                    return new PlayerConfiguration(cipherOperations, dashManifestUrl, hlsManifestUrl, muxedStreamInfosUrlEncoded,
                        adaptiveStreamInfosUrlEncoded, validUntil);
                }

                // If the video is still unplayable - throw
                throw new VideoUnplayableException(videoId, $"Video [{videoId}] is unplayable. Reason: {errorReason}");
            }

        }

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info dictionary
            var videoInfoDic = await GetVideoInfoDicAsync(videoId);

            // Get player response JSON
            var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

            // Extract video info
            var videoAuthor = playerResponseJson.SelectToken("videoDetails.author").Value<string>();
            var videoTitle = playerResponseJson.SelectToken("videoDetails.title").Value<string>();
            var videoDuration = TimeSpan.FromSeconds(playerResponseJson.SelectToken("videoDetails.lengthSeconds").Value<double>());
            var videoKeywords = playerResponseJson.SelectToken("videoDetails.keywords").EmptyIfNull().Values<string>().ToArray();
            var videoDescription = playerResponseJson.SelectToken("videoDetails.shortDescription").Value<string>();
            var videoViewCount = playerResponseJson.SelectToken("videoDetails.viewCount")?.Value<long>() ?? 0; // some videos have no views

            // Get video watch page HTML
            var videoWatchPageHtml = await GetVideoWatchPageHtmlAsync(videoId);

            // Extract upload date
            var videoUploadDate = videoWatchPageHtml.QuerySelector("meta[itemprop=\"datePublished\"]").GetAttribute("content")
                .ParseDateTimeOffset("yyyy-MM-dd");

            // Extract like count
            var videoLikeCountRaw =
                videoWatchPageHtml.QuerySelector("button.like-button-renderer-like-button")?.Text().StripNonDigit();
            var videoLikeCount = !videoLikeCountRaw.IsNullOrWhiteSpace() ? videoLikeCountRaw.ParseLong() : 0;

            // Extract dislike count
            var videoDislikeCountRaw =
                videoWatchPageHtml.QuerySelector("button.like-button-renderer-dislike-button")?.Text().StripNonDigit();
            var videoDislikeCount = !videoDislikeCountRaw.IsNullOrWhiteSpace() ? videoDislikeCountRaw.ParseLong() : 0;

            // Create statistics and thumbnails
            var statistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);
            var thumbnails = new ThumbnailSet(videoId);

            return new Video(videoId, videoAuthor, videoUploadDate, videoTitle, videoDescription,
                thumbnails, videoDuration, videoKeywords, statistics);
        }

        /// <inheritdoc />
        public async Task<Channel> GetVideoAuthorChannelAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info dictionary
            var videoInfoDic = await GetVideoInfoDicAsync(videoId);

            // Get player response JSON
            var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

            // Extract channel ID
            var channelId = playerResponseJson.SelectToken("videoDetails.channelId").Value<string>();

            return await GetChannelAsync(channelId);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get player configuration
            var playerConfiguration = await GetPlayerConfigurationAsync(videoId);

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Extract muxed stream infos
            foreach (var streamInfoParser in playerConfiguration.MuxedStreamInfosUrlEncoded?.Split(",").EmptyIfNull())
            {
                // Extract info
                var itag = streamInfoParser.GetItag();
                var url = streamInfoParser.GetUrl();

                // Decipher signature if needed
                var signature = streamInfoParser.TryGetSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(playerSourceUrl, signature);
                    var signatureParameterName = streamInfoParser.TryGetSignatureParameterName() ?? "signature";
                    url = Url.SetQueryParameter(url, signatureParameterName, signature);
                }

                // Try to extract content length, otherwise get it manually
                var contentLength = streamInfoParser.TryGetContentLength() ?? -1;
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Extract container
                var containerStr = streamInfoParser.GetContainer();
                var container = Heuristics.ContainerFromString(containerStr);

                // Extract audio encoding
                var audioEncodingStr = streamInfoParser.GetAudioEncoding();
                var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                // Extract video encoding
                var videoEncodingStr = streamInfoParser.GetVideoEncoding();
                var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                // Determine video quality from itag
                var videoQuality = Heuristics.VideoQualityFromItag(itag);

                // Determine video quality label from video quality
                var videoQualityLabel = Heuristics.VideoQualityToLabel(videoQuality);

                // Determine video resolution from video quality
                var resolution = Heuristics.VideoQualityToResolution(videoQuality);

                // Add to list
                muxedStreamInfoMap[itag] = new MuxedStreamInfo(itag, url, container, contentLength, audioEncoding, videoEncoding,
                    videoQualityLabel, videoQuality, resolution);
            }

            // Extract adaptive stream infos
            foreach (var streamInfoParser in adaptiveStreamInfoParsers)
            {
                // Extract info
                var itag = streamInfoParser.GetItag();
                var url = streamInfoParser.GetUrl();
                var bitrate = streamInfoParser.GetBitrate();

                // Decipher signature if needed
                var signature = streamInfoParser.TryGetSignature();
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(playerSourceUrl, signature);
                    var signatureParameterName = streamInfoParser.TryGetSignatureParameterName() ?? "signature";
                    url = Url.SetQueryParameter(url, signatureParameterName, signature);
                }

                // Try to extract content length, otherwise get it manually
                var contentLength = streamInfoParser.TryGetContentLength() ?? -1;
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false) ?? -1;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Extract container
                var containerStr = streamInfoParser.GetContainer();
                var container = Heuristics.ContainerFromString(containerStr);

                // If audio-only
                if (streamInfoParser.GetIsAudioOnly())
                {
                    // Extract audio encoding
                    var audioEncodingStr = streamInfoParser.GetAudioEncoding();
                    var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                    // Add stream
                    audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                }
                // If video-only
                else
                {
                    // Extract video encoding
                    var videoEncodingStr = streamInfoParser.GetVideoEncoding();
                    var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                    // Extract video quality label and video quality
                    var videoQualityLabel = streamInfoParser.GetVideoQualityLabel();
                    var videoQuality = Heuristics.VideoQualityFromLabel(videoQualityLabel);

                    // Extract resolution
                    var width = streamInfoParser.GetWidth();
                    var height = streamInfoParser.GetHeight();
                    var resolution = new VideoResolution(width, height);

                    // Extract framerate
                    var framerate = streamInfoParser.GetFramerate();

                    // Add to list
                    videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                        videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Extract dash manifest
            if (!dashManifestUrl.IsNullOrWhiteSpace())
            {
                // Extract signature
                var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature if needed
                if (!signature.IsNullOrWhiteSpace())
                {
                    signature = await DecipherSignatureAsync(playerSourceUrl, signature);
                    dashManifestUrl = Url.SetRouteParameter(dashManifestUrl, "signature", signature);
                }

                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestParserAsync(dashManifestUrl);

                // Extract dash stream infos
                foreach (var streamInfoParser in dashManifestParser.GetStreamInfos())
                {
                    // Extract info
                    var itag = streamInfoParser.GetItag();
                    var url = streamInfoParser.GetUrl();
                    var contentLength = streamInfoParser.GetContentLength();
                    var bitrate = streamInfoParser.GetBitrate();

                    // Extract container
                    var containerStr = streamInfoParser.GetContainer();
                    var container = Heuristics.ContainerFromString(containerStr);

                    // If audio-only
                    if (streamInfoParser.GetIsAudioOnly())
                    {
                        // Extract audio encoding
                        var audioEncodingStr = streamInfoParser.GetEncoding();
                        var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingStr);

                        // Add to list
                        audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                    }
                    // If video-only
                    else
                    {
                        // Extract video encoding
                        var videoEncodingStr = streamInfoParser.GetEncoding();
                        var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingStr);

                        // Extract resolution
                        var width = streamInfoParser.GetWidth();
                        var height = streamInfoParser.GetHeight();
                        var resolution = new VideoResolution(width, height);

                        // Extract framerate
                        var framerate = streamInfoParser.GetFramerate();

                        // Determine video quality from itag
                        var videoQuality = Heuristics.VideoQualityFromItag(itag);

                        // Determine video quality label from video quality and framerate
                        var videoQualityLabel = Heuristics.VideoQualityToLabel(videoQuality, framerate);

                        // Add to list
                        videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                            videoQualityLabel, videoQuality, resolution, framerate);
                    }
                }
            }

            // Finalize stream info collections
            var muxedStreamInfos = muxedStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();
            var audioStreamInfos = audioStreamInfoMap.Values.OrderByDescending(s => s.Bitrate).ToArray();
            var videoStreamInfos = videoStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsManifestUrl, validUntil);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info dictionary
            var videoInfoDic = await GetVideoInfoDicAsync(videoId);

            // Get player response JSON
            var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

            // Get closed caption track infos
            var trackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var trackJson in playerResponseJson.SelectToken("..captionTracks").EmptyIfNull())
            {
                // Get URL
                var url = trackJson.SelectToken("baseUrl").Value<string>();

                // Set format to the one we know how to deal with
                url = Url.SetQueryParameter(url, "format", "3");

                // Get language
                var languageCode = trackJson.SelectToken("languageCode").Value<string>();
                var languageName = trackJson.SelectToken("name.simpleText").Value<string>();
                var language = new Language(languageCode, languageName);

                // Get whether the track is autogenerated
                var isAutoGenerated = trackJson.SelectToken("vssId").Value<string>()
                    .StartsWith("a.", StringComparison.OrdinalIgnoreCase);

                // Add to list
                trackInfos.Add(new ClosedCaptionTrackInfo(url, language, isAutoGenerated));
            }

            return trackInfos;
        }
    }
}