using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using LtGt;
using LtGt.Models;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.CipherOperations;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private readonly Dictionary<string, IReadOnlyList<ICipherOperation>> _cipherOperationsCache =
            new Dictionary<string, IReadOnlyList<ICipherOperation>>();

        private async Task<IReadOnlyDictionary<string, string>> GetVideoInfoDicAsync(string videoId)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            // Execute request
            var url = $"https://youtube.com/get_video_info?video_id={videoId}&el=embedded&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            // Parse response as URL-encoded dictionary
            var result = Url.SplitQuery(raw);

            return result;
        }

        private async Task<HtmlDocument> GetVideoWatchPageHtmlAsync(string videoId)
        {
            var url = $"https://youtube.com/watch?v={videoId}&disable_polymer=true&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return HtmlParser.Default.ParseDocument(raw);
        }

        private async Task<HtmlDocument> GetVideoEmbedPageHtmlAsync(string videoId)
        {
            var url = $"https://youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return HtmlParser.Default.ParseDocument(raw);
        }

        private async Task<XElement> GetDashManifestXmlAsync(string url)
        {
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            return XElement.Parse(raw).StripNamespaces();
        }

        private async Task<PlayerConfiguration> GetPlayerConfigurationAsync(string videoId)
        {
            // Try to get from video info
            {
                // Get video embed page HTML
                var videoEmbedPageHtml = await GetVideoEmbedPageHtmlAsync(videoId).ConfigureAwait(false);

                // Get player config JSON
                var playerConfigRaw = videoEmbedPageHtml.GetElementsByTagName("script")
                    .Select(e => e.GetInnerText())
                    .Select(s => Regex.Match(s, @"yt\.setConfig\({'PLAYER_CONFIG':(.*)}\);").Groups[1].Value)
                    .First(s => !s.IsNullOrWhiteSpace());
                var playerConfigJson = JToken.Parse(playerConfigRaw);

                // Extract player source URL
                var playerSourceUrl = "https://youtube.com" + playerConfigJson.SelectToken("assets.js").Value<string>();

                // Get video info dictionary
                var requestedAt = DateTimeOffset.Now;
                var videoInfoDic = await GetVideoInfoDicAsync(videoId).ConfigureAwait(false);

                // Get player response JSON
                var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

                // If video is unavailable - throw
                if (string.Equals(playerResponseJson.SelectToken("playabilityStatus.status")?.Value<string>(), "error",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");
                }

                // If there is no error - extract info and return
                var errorReason = playerResponseJson.SelectToken("playabilityStatus.reason")?.Value<string>();
                if (errorReason.IsNullOrWhiteSpace())
                {
                    // Extract whether the video is a live stream
                    var isLiveStream = playerResponseJson.SelectToken("videoDetails.isLive")?.Value<bool>() == true;

                    // Extract valid until date
                    var expiresIn = TimeSpan.FromSeconds(playerResponseJson.SelectToken("streamingData.expiresInSeconds").Value<double>());
                    var validUntil = requestedAt + expiresIn;

                    // Extract stream info
                    var hlsManifestUrl =
                        isLiveStream ? playerResponseJson.SelectToken("streamingData.hlsManifestUrl")?.Value<string>() : null;
                    var dashManifestUrl =
                        !isLiveStream ? playerResponseJson.SelectToken("streamingData.dashManifestUrl")?.Value<string>() : null;
                    var muxedStreamInfosUrlEncoded =
                        !isLiveStream ? videoInfoDic.GetValueOrDefault("url_encoded_fmt_stream_map") : null;
                    var adaptiveStreamInfosUrlEncoded =
                        !isLiveStream ? videoInfoDic.GetValueOrDefault("adaptive_fmts") : null;

                    return new PlayerConfiguration(playerSourceUrl, dashManifestUrl, hlsManifestUrl, muxedStreamInfosUrlEncoded,
                        adaptiveStreamInfosUrlEncoded, validUntil);
                }

                // If the video requires purchase - throw (approach one)
                {
                    var previewVideoId = playerResponseJson
                        .SelectToken("playabilityStatus.errorScreen.playerLegacyDesktopYpcTrailerRenderer.trailerVideoId")?.Value<string>();
                    if (!previewVideoId.IsNullOrWhiteSpace())
                    {
                        throw new VideoRequiresPurchaseException(videoId, previewVideoId,
                            $"Video [{videoId}] is unplayable because it requires purchase.");
                    }
                }

                // If the video requires purchase - throw (approach two)
                {
                    var previewVideoInfoRaw = playerResponseJson.SelectToken("playabilityStatus.errorScreen.ypcTrailerRenderer.playerVars")
                        ?.Value<string>();
                    if (!previewVideoInfoRaw.IsNullOrWhiteSpace())
                    {
                        var previewVideoInfoDic = Url.SplitQuery(previewVideoInfoRaw);
                        var previewVideoId = previewVideoInfoDic.GetValueOrDefault("video_id");

                        throw new VideoRequiresPurchaseException(videoId, previewVideoId,
                            $"Video [{videoId}] is unplayable because it requires purchase.");
                    }
                }
            }

            // Try to get from video watch page
            {
                // Get video watch page HTML
                var requestedAt = DateTimeOffset.Now;
                var videoWatchPageHtml = await GetVideoWatchPageHtmlAsync(videoId).ConfigureAwait(false);

                // Extract player config
                var playerConfigRaw = videoWatchPageHtml.GetElementsByTagName("script")
                    .Select(e => e.GetInnerText())
                    .Select(s =>
                        Regex.Match(s,
                                @"ytplayer\.config = (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                            .Groups["Json"].Value)
                    .FirstOrDefault(s => !s.IsNullOrWhiteSpace());

                // If player config is not available - throw
                if (playerConfigRaw.IsNullOrWhiteSpace())
                {
                    var errorReason = videoWatchPageHtml.GetElementById("unavailable-message")?.GetInnerText().Trim();
                    throw new VideoUnplayableException(videoId, $"Video [{videoId}] is unplayable. Reason: {errorReason}");
                }

                // Get player config JSON
                var playerConfigJson = JToken.Parse(playerConfigRaw);

                // Extract player source URL
                var playerSourceUrl = "https://youtube.com" + playerConfigJson.SelectToken("assets.js").Value<string>();

                // Get player response JSON
                var playerResponseRaw = playerConfigJson.SelectToken("args.player_response").Value<string>();
                var playerResponseJson = JToken.Parse(playerResponseRaw);

                // Extract whether the video is a live stream
                var isLiveStream = playerResponseJson.SelectToken("videoDetails.isLive")?.Value<bool>() == true;

                // Extract valid until date
                var expiresIn = TimeSpan.FromSeconds(playerResponseJson.SelectToken("streamingData.expiresInSeconds").Value<double>());
                var validUntil = requestedAt + expiresIn;

                // Extract stream info
                var hlsManifestUrl =
                    isLiveStream ? playerResponseJson.SelectToken("streamingData.hlsManifestUrl")?.Value<string>() : null;
                var dashManifestUrl =
                    !isLiveStream ? playerResponseJson.SelectToken("streamingData.dashManifestUrl")?.Value<string>() : null;
                var muxedStreamInfosUrlEncoded =
                    !isLiveStream ? playerConfigJson.SelectToken("args.url_encoded_fmt_stream_map")?.Value<string>() : null;
                var adaptiveStreamInfosUrlEncoded =
                    !isLiveStream ? playerConfigJson.SelectToken("args.adaptive_fmts")?.Value<string>() : null;

                return new PlayerConfiguration(playerSourceUrl, dashManifestUrl, hlsManifestUrl, muxedStreamInfosUrlEncoded,
                    adaptiveStreamInfosUrlEncoded, validUntil);
            }
        }

        private async Task<IReadOnlyList<ICipherOperation>> GetCipherOperationsAsync(string playerSourceUrl)
        {
            // If already in cache - return
            if (_cipherOperationsCache.TryGetValue(playerSourceUrl, out var cached))
                return cached;

            // Get player source
            var raw = await _httpClient.GetStringAsync(playerSourceUrl).ConfigureAwait(false);

            // Find the name of the function that handles deciphering
            var deciphererFuncName = Regex.Match(raw,
                @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}").Groups[1].Value;

            if (deciphererFuncName.IsNullOrWhiteSpace())
            {
                throw new UnrecognizedStructureException(
                    "Could not find signature decipherer function name. Please report this issue on GitHub.");
            }

            // Find the body of the function
            var deciphererFuncBody = Regex.Match(raw,
                @"(?!h\.)" + Regex.Escape(deciphererFuncName) + @"=function\(\w+\)\{(.*?)\}", RegexOptions.Singleline).Groups[1].Value;

            if (deciphererFuncBody.IsNullOrWhiteSpace())
            {
                throw new UnrecognizedStructureException(
                    "Could not find signature decipherer function body. Please report this issue on GitHub.");
            }

            // Split the function body into statements
            var deciphererFuncBodyStatements = deciphererFuncBody.Split(";");

            // Find the name of block that defines functions used in decipherer
            var deciphererDefinitionName = Regex.Match(deciphererFuncBody, "(\\w+).\\w+\\(\\w+,\\d+\\);").Groups[1].Value;

            // Find the body of the function
            var deciphererDefinitionBody = Regex.Match(raw,
                @"var\s+" +
                Regex.Escape(deciphererDefinitionName) +
                @"=\{(\w+:function\(\w+(,\w+)?\)\{(.*?)\}),?\};", RegexOptions.Singleline).Groups[0].Value;

            // Identify cipher functions
            var operations = new List<ICipherOperation>();

            // Analyze statements to determine cipher function names
            foreach (var statement in deciphererFuncBodyStatements)
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsNullOrWhiteSpace())
                    continue;

                // Slice
                if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }

                // Swap
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }

                // Reverse
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return _cipherOperationsCache[playerSourceUrl] = operations;
        }

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info dictionary
            var videoInfoDic = await GetVideoInfoDicAsync(videoId).ConfigureAwait(false);

            // Get player response JSON
            var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

            // If video is unavailable - throw
            if (string.Equals(playerResponseJson.SelectToken("playabilityStatus.status")?.Value<string>(), "error",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");
            }

            // Extract video info
            var videoAuthor = playerResponseJson.SelectToken("videoDetails.author").Value<string>();
            var videoTitle = playerResponseJson.SelectToken("videoDetails.title").Value<string>();
            var videoDuration = TimeSpan.FromSeconds(playerResponseJson.SelectToken("videoDetails.lengthSeconds").Value<double>());
            var videoKeywords = playerResponseJson.SelectToken("videoDetails.keywords").EmptyIfNull().Values<string>().ToArray();
            var videoDescription = playerResponseJson.SelectToken("videoDetails.shortDescription").Value<string>();
            var videoViewCount = playerResponseJson.SelectToken("videoDetails.viewCount")?.Value<long>() ?? 0; // some videos have no views

            // Get video watch page HTML
            var videoWatchPageHtml = await GetVideoWatchPageHtmlAsync(videoId).ConfigureAwait(false);

            // Extract upload date
            var videoUploadDate = videoWatchPageHtml.GetElementsBySelector("meta[itemprop=\"datePublished\"]")
                .First().GetAttribute("content").Value.ParseDateTimeOffset("yyyy-MM-dd");

            // Extract like count
            var videoLikeCountRaw = videoWatchPageHtml.GetElementsByClassName("like-button-renderer-like-button")
                .FirstOrDefault()?.GetInnerText().StripNonDigit();

            var videoLikeCount = !videoLikeCountRaw.IsNullOrWhiteSpace() ? videoLikeCountRaw.ParseLong() : 0;

            // Extract dislike count
            var videoDislikeCountRaw = videoWatchPageHtml.GetElementsByClassName("like-button-renderer-dislike-button")
                .FirstOrDefault()?.GetInnerText().StripNonDigit();

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
            var videoInfoDic = await GetVideoInfoDicAsync(videoId).ConfigureAwait(false);

            // Get player response JSON
            var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

            // If video is unavailable - throw
            if (string.Equals(playerResponseJson.SelectToken("playabilityStatus.status")?.Value<string>(), "error",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");
            }

            // Extract channel ID
            var channelId = playerResponseJson.SelectToken("videoDetails.channelId").Value<string>();

            return await GetChannelAsync(channelId).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get player configuration
            var playerConfiguration = await GetPlayerConfigurationAsync(videoId).ConfigureAwait(false);

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Get muxed stream infos
            var muxedStreamInfoDics = playerConfiguration.MuxedStreamInfosUrlEncoded.EmptyIfNull().Split(",").Select(Url.SplitQuery);
            foreach (var streamInfoDic in muxedStreamInfoDics)
            {
                // Extract info
                var itag = streamInfoDic["itag"].ParseInt();
                var url = streamInfoDic["url"];

                // Decipher signature if needed
                var signature = streamInfoDic.GetValueOrDefault("s");
                if (!signature.IsNullOrWhiteSpace())
                {
                    // Get cipher operations (cached)
                    var cipherOperations = await GetCipherOperationsAsync(playerConfiguration.PlayerSourceUrl).ConfigureAwait(false);

                    // Decipher signature
                    signature = cipherOperations.Decipher(signature);

                    // Set the corresponding parameter in the URL
                    var signatureParameter = streamInfoDic.GetValueOrDefault("sp") ?? "signature";
                    url = Url.SetQueryParameter(url, signatureParameter, signature);
                }

                // Try to extract content length, otherwise get it manually
                var contentLength = Regex.Match(url, @"clen=(\d+)").Groups[1].Value.ParseLongOrDefault();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? 0;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Extract container
                var containerRaw = streamInfoDic["type"].SubstringUntil(";").SubstringAfter("/");
                var container = Heuristics.ContainerFromString(containerRaw);

                // Extract audio encoding
                var audioEncodingRaw = streamInfoDic["type"].SubstringAfter("codecs=\"").SubstringUntil("\"").Split(", ").Last();
                var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingRaw);

                // Extract video encoding
                var videoEncodingRaw = streamInfoDic["type"].SubstringAfter("codecs=\"").SubstringUntil("\"").Split(", ").First();
                var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingRaw);

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

            // Get adaptive stream infos
            var adaptiveStreamInfoDics = playerConfiguration.AdaptiveStreamInfosUrlEncoded.EmptyIfNull().Split(",").Select(Url.SplitQuery);
            foreach (var streamInfoDic in adaptiveStreamInfoDics)
            {
                // Extract info
                var itag = streamInfoDic["itag"].ParseInt();
                var url = streamInfoDic["url"];
                var bitrate = streamInfoDic["bitrate"].ParseLong();

                // Decipher signature if needed
                var signature = streamInfoDic.GetValueOrDefault("s");
                if (!signature.IsNullOrWhiteSpace())
                {
                    // Get cipher operations (cached)
                    var cipherOperations = await GetCipherOperationsAsync(playerConfiguration.PlayerSourceUrl).ConfigureAwait(false);

                    // Decipher signature
                    signature = cipherOperations.Decipher(signature);

                    // Set the corresponding parameter in the URL
                    var signatureParameter = streamInfoDic.GetValueOrDefault("sp") ?? "signature";
                    url = Url.SetQueryParameter(url, signatureParameter, signature);
                }

                // Try to extract content length, otherwise get it manually
                var contentLength = streamInfoDic.GetValueOrDefault("clen").ParseLongOrDefault();
                if (contentLength <= 0)
                {
                    // Send HEAD request and get content length
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? 0;

                    // If content length is still not available - stream is gone or faulty
                    if (contentLength <= 0)
                        continue;
                }

                // Extract container
                var containerRaw = streamInfoDic["type"].SubstringUntil(";").SubstringAfter("/");
                var container = Heuristics.ContainerFromString(containerRaw);

                // If audio-only
                if (streamInfoDic["type"].StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract audio encoding
                    var audioEncodingRaw = streamInfoDic["type"].SubstringAfter("codecs=\"").SubstringUntil("\"");
                    var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingRaw);

                    // Add stream
                    audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                }
                // If video-only
                else
                {
                    // Extract video encoding
                    var videoEncodingRaw = streamInfoDic["type"].SubstringAfter("codecs=\"").SubstringUntil("\"");
                    var videoEncoding = !videoEncodingRaw.Equals("unknown", StringComparison.OrdinalIgnoreCase)
                        ? Heuristics.VideoEncodingFromString(videoEncodingRaw)
                        : VideoEncoding.Av1; // HACK: issue 246

                    // Extract video quality label and video quality
                    var videoQualityLabel = streamInfoDic["quality_label"];
                    var videoQuality = Heuristics.VideoQualityFromLabel(videoQualityLabel);

                    // Extract resolution
                    var width = streamInfoDic["size"].SubstringUntil("x").ParseInt();
                    var height = streamInfoDic["size"].SubstringAfter("x").ParseInt();
                    var resolution = new VideoResolution(width, height);

                    // Extract framerate
                    var framerate = streamInfoDic["fps"].ParseInt();

                    // Add to list
                    videoStreamInfoMap[itag] = new VideoStreamInfo(itag, url, container, contentLength, bitrate, videoEncoding,
                        videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Get dash manifest
            var dashManifestUrl = playerConfiguration.DashManifestUrl;
            if (!dashManifestUrl.IsNullOrWhiteSpace())
            {
                // Extract signature
                var signature = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature if needed
                if (!signature.IsNullOrWhiteSpace())
                {
                    // Get cipher operations (cached)
                    var cipherOperations = await GetCipherOperationsAsync(playerConfiguration.PlayerSourceUrl).ConfigureAwait(false);

                    // Decipher signature
                    signature = cipherOperations.Decipher(signature);

                    // Set the corresponding parameter in the URL
                    dashManifestUrl = Url.SetRouteParameter(dashManifestUrl, "signature", signature);
                }

                // Get DASH manifest XML
                var dashManifestXml = await GetDashManifestXmlAsync(dashManifestUrl).ConfigureAwait(false);

                // Get representation nodes (skip partial streams)
                var streamInfoXmls = dashManifestXml.Descendants("Representation").Where(s =>
                    s.Descendants("Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value.Contains("sq/") != true);

                // Get DASH stream infos
                foreach (var streamInfoXml in streamInfoXmls)
                {
                    // Extract info
                    var itag = (int) streamInfoXml.Attribute("id");
                    var url = (string) streamInfoXml.Element("BaseURL");
                    var contentLength = Regex.Match(url, @"clen[/=](\d+)").Groups[1].Value.ParseLong();
                    var bitrate = (long) streamInfoXml.Attribute("bandwidth");

                    // Extract container
                    var containerRaw = Regex.Match(url, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value.UrlDecode();
                    var container = Heuristics.ContainerFromString(containerRaw);

                    // If audio-only
                    if (streamInfoXml.Element("AudioChannelConfiguration") != null)
                    {
                        // Extract audio encoding
                        var audioEncodingRaw = (string) streamInfoXml.Attribute("codecs");
                        var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingRaw);

                        // Add to list
                        audioStreamInfoMap[itag] = new AudioStreamInfo(itag, url, container, contentLength, bitrate, audioEncoding);
                    }
                    // If video-only
                    else
                    {
                        // Extract video encoding
                        var videoEncodingRaw = (string) streamInfoXml.Attribute("codecs");
                        var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingRaw);

                        // Extract resolution
                        var width = (int) streamInfoXml.Attribute("width");
                        var height = (int) streamInfoXml.Attribute("height");
                        var resolution = new VideoResolution(width, height);

                        // Extract framerate
                        var framerate = (int) streamInfoXml.Attribute("frameRate");

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

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos,
                playerConfiguration.HlsManifestUrl, playerConfiguration.ValidUntil);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info dictionary
            var videoInfoDic = await GetVideoInfoDicAsync(videoId).ConfigureAwait(false);

            // Get player response JSON
            var playerResponseJson = JToken.Parse(videoInfoDic["player_response"]);

            // If video is unavailable - throw
            if (string.Equals(playerResponseJson.SelectToken("playabilityStatus.status")?.Value<string>(), "error",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable.");
            }

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
