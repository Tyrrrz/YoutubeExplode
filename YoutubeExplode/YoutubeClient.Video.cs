using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.CipherOperations;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Services;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<JToken> GetEmbedPageConfigAsync(string videoId)
        {
            // Get the embed video page
            var request = $"{YoutubeHost}/embed/{videoId}?disable_polymer=true&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Find and parse config
            var configRaw = response
                .SubstringAfter("yt.setConfig({'PLAYER_CONFIG': ")
                .SubstringUntil(",'EXPERIMENT_FLAGS'");

            return JToken.Parse(configRaw);
        }

        private async Task<JToken> GetWatchPageConfigAsync(string videoId)
        {
            // Get the embed video page
            var request = $"{YoutubeHost}/watch?v={videoId}&disable_polymer=true&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Find and parse config
            var configRaw = response
                .SubstringAfter("window[\"ytInitialData\"] = ")
                .SubstringUntil(";");

            return JToken.Parse(configRaw);
        }

        private async Task<PlayerContext> GetPlayerContextAsync(string videoId)
        {
            // Get config
            var configJson = await GetEmbedPageConfigAsync(videoId).ConfigureAwait(false);

            // Extract values
            var sourceUrl = configJson["assets"].Value<string>("js");
            var sts = configJson.Value<string>("sts");

            // Check if successful
            if (sourceUrl.IsBlank() || sts.IsBlank())
                throw new ParseException("Could not parse player context");

            // Append host to source url
            sourceUrl = YoutubeHost + sourceUrl;

            return new PlayerContext(sourceUrl, sts);
        }

        private async Task<PlayerSource> GetPlayerSourceAsync(string sourceUrl)
        {
            // Original code credit: Decipherer class of https://github.com/flagbug/YoutubeExtractor

            // Try to resolve from cache first
            var playerSource = _playerSourceCache.GetOrDefault(sourceUrl);
            if (playerSource != null)
                return playerSource;

            // Get player source code
            var response = await _httpService.GetStringAsync(sourceUrl).ConfigureAwait(false);

            // Find the name of the function that handles deciphering
            var entryPoint = Regex.Match(response, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
            if (entryPoint.IsBlank())
                throw new ParseException("Could not find the entry function for signature deciphering");

            // Find the body of the function
            var entryPointPattern = @"(?!h\.)" + Regex.Escape(entryPoint) + @"=function\(\w+\)\{(.*?)\}";
            var entryPointBody = Regex.Match(response, entryPointPattern, RegexOptions.Singleline).Groups[1].Value;
            if (entryPointBody.IsBlank())
                throw new ParseException("Could not find the signature decipherer function body");
            var entryPointLines = entryPointBody.Split(";").ToArray();

            // Identify cipher functions
            string reverseFuncName = null;
            string sliceFuncName = null;
            string charSwapFuncName = null;
            var operations = new List<ICipherOperation>();

            // Analyze the function body to determine the names of cipher functions
            foreach (var line in entryPointLines)
            {
                // Break when all functions are found
                if (reverseFuncName.IsNotBlank() && sliceFuncName.IsNotBlank() && charSwapFuncName.IsNotBlank())
                    break;

                // Get the function called on this line
                var calledFuncName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFuncName.IsBlank())
                    continue;

                // Find cipher function names
                if (Regex.IsMatch(response, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(response,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(response,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    charSwapFuncName = calledFuncName;
                }
            }

            // Analyze the function body again to determine the operation set and order
            foreach (var line in entryPointLines)
            {
                // Get the function called on this line
                var calledFuncName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFuncName.IsBlank())
                    continue;

                // Swap operation
                if (calledFuncName == charSwapFuncName)
                {
                    var index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }
                // Slice operation
                else if (calledFuncName == sliceFuncName)
                {
                    var index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }
                // Reverse operation
                else if (calledFuncName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return _playerSourceCache[sourceUrl] = new PlayerSource(operations);
        }

        private async Task<IReadOnlyDictionary<string, string>> GetVideoInfoAsync(string videoId, string sts = "",
            bool retry = true)
        {
            // If retry is enabled - query with different values of "el"
            var els = retry
                ? new[] {"embedded", "adunit"}
                : new[] {"embedded"};

            // Get best video info
            IReadOnlyDictionary<string, string> videoInfo = null;
            foreach (var el in els)
            {
                // Get video info
                var request = $"{YoutubeHost}/get_video_info?video_id={videoId}&el={el}&sts={sts}&hl=en";
                var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
                videoInfo = UrlHelper.GetDictionaryFromUrlQuery(response);

                // Check error code
                if (!videoInfo.ContainsKey("errorcode"))
                    break;
            }

            // This will never return null by implementation
            return videoInfo;
        }

        private void ThrowIfVideoInfoUnavailable(string videoId, IReadOnlyDictionary<string, string> videoInfo)
        {
            if (videoInfo.ContainsKey("errorcode"))
            {
                var errorCode = videoInfo.Get("errorcode").ParseInt();
                var errorReason = videoInfo.Get("reason");

                throw new VideoUnavailableException(videoId, errorCode, errorReason);
            }
        }

        private void ThrowIfVideoInfoRequiresPurchase(string videoId, IReadOnlyDictionary<string, string> videoInfo)
        {
            if (videoInfo.ContainsKey("ypc_vid"))
            {
                var previewVideoId = videoInfo.Get("ypc_vid");

                throw new VideoRequiresPurchaseException(videoId, previewVideoId);
            }
        }

        /// <summary>
        /// Gets video by ID
        /// </summary>
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get video info
            var videoInfo = await GetVideoInfoAsync(videoId).ConfigureAwait(false);
            ThrowIfVideoInfoUnavailable(videoId, videoInfo);
            ThrowIfVideoInfoRequiresPurchase(videoId, videoInfo);

            // Parse metadata
            var title = videoInfo.Get("title");
            var author = videoInfo.Get("author");
            var duration = TimeSpan.FromSeconds(videoInfo.Get("length_seconds").ParseDouble());
            var viewCount = videoInfo.Get("view_count").ParseLong();
            var keywords = videoInfo.Get("keywords").Split(",");

            // More metadata
            // Get the embed video page
            var request = $"{YoutubeHost}/watch?v={videoId}&disable_polymer=true&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            var html = await new HtmlParser().ParseAsync(response);
            
            var description = html.QuerySelector("p#eow-description").TextEx();
            var likeCount = html.QuerySelector("button.like-button-renderer-like-button").Text().StripNonDigit().ParseLong();
            var dislikeCount = html.QuerySelector("button.like-button-renderer-dislike-button").Text().StripNonDigit().ParseLong();

            // Concat metadata
            var thumbnails = new VideoThumbnails(videoId);
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Video(videoId, author, title, description, thumbnails, duration, keywords, statistics);
        }

        public async Task<Channel> GetVideoChannelAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get the embed config
            var configJson = await GetEmbedPageConfigAsync(videoId).ConfigureAwait(false);

            // Parse metadata
            var channelPath = configJson["args"].Value<string>("channel_path");
            var id = channelPath.SubstringAfter("channel/");
            var title = configJson["args"].Value<string>("author");
            var logoUrl = configJson["args"].Value<string>("profile_picture");

            return new Channel(id, title, logoUrl);
        }

        public async Task<MediaStreamInfoSet> GetMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get player context
            var playerContext = await GetPlayerContextAsync(videoId).ConfigureAwait(false);

            // Get video info
            var videoInfo = await GetVideoInfoAsync(videoId, playerContext.Sts).ConfigureAwait(false);
            ThrowIfVideoInfoUnavailable(videoId, videoInfo);
            ThrowIfVideoInfoRequiresPurchase(videoId, videoInfo);

            // Prepare stream info collections
            var muxedStreamInfos = new List<MuxedStreamInfo>();
            var audioStreamInfos = new List<AudioStreamInfo>();
            var videoStreamInfos = new List<VideoStreamInfo>();

            // Resolve muxed streams
            var muxedStreamInfosEncoded = videoInfo.GetOrDefault("url_encoded_fmt_stream_map");
            if (muxedStreamInfosEncoded.IsNotBlank())
            {
                foreach (var streamEncoded in muxedStreamInfosEncoded.Split(","))
                {
                    var streamInfoDic = UrlHelper.GetDictionaryFromUrlQuery(streamEncoded);

                    var itag = streamInfoDic.Get("itag").ParseInt();
                    var url = streamInfoDic.Get("url");
                    var sig = streamInfoDic.GetOrDefault("s");

#if RELEASE
                    if (!MediaStreamInfo.IsKnown(itag))
                        continue;
#endif

                    // Decipher signature if needed
                    if (sig.IsNotBlank())
                    {
                        var playerSource = await GetPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlHelper.SetUrlQueryParameter(url, "signature", sig);
                    }

                    // Probe stream and get content length
                    long contentLength;
                    using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                    using (var response = await _httpService.PerformRequestAsync(request).ConfigureAwait(false))
                    {
                        // Some muxed streams can be gone
                        if (response.StatusCode == HttpStatusCode.NotFound ||
                            response.StatusCode == HttpStatusCode.Gone)
                            continue;

                        // Ensure success
                        response.EnsureSuccessStatusCode();

                        // Extract content length
                        contentLength = response.Content.Headers.ContentLength ??
                                        throw new ParseException("Could not extract content length");
                    }

                    // Set rate bypass
                    url = UrlHelper.SetUrlQueryParameter(url, "ratebypass", "yes");

                    var streamInfo = new MuxedStreamInfo(itag, url, contentLength);
                    muxedStreamInfos.Add(streamInfo);
                }
            }

            // Resolve adaptive streams
            var adaptiveStreamInfosEncoded = videoInfo.GetOrDefault("adaptive_fmts");
            if (adaptiveStreamInfosEncoded.IsNotBlank())
            {
                foreach (var streamEncoded in adaptiveStreamInfosEncoded.Split(","))
                {
                    var streamInfoDic = UrlHelper.GetDictionaryFromUrlQuery(streamEncoded);

                    var itag = streamInfoDic.Get("itag").ParseInt();
                    var url = streamInfoDic.Get("url");
                    var sig = streamInfoDic.GetOrDefault("s");
                    var contentLength = streamInfoDic.Get("clen").ParseLong();
                    var bitrate = streamInfoDic.Get("bitrate").ParseLong();

#if RELEASE
                    if (!MediaStreamInfo.IsKnown(itag))
                        continue;
#endif

                    // Decipher signature if needed
                    if (sig.IsNotBlank())
                    {
                        var playerSource = await GetPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlHelper.SetUrlQueryParameter(url, "signature", sig);
                    }

                    // Set rate bypass
                    url = UrlHelper.SetUrlQueryParameter(url, "ratebypass", "yes");

                    // Check if audio
                    var isAudio = streamInfoDic.Get("type").Contains("audio/");

                    // If audio stream
                    if (isAudio)
                    {
                        var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreamInfos.Add(streamInfo);
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var size = streamInfoDic.Get("size");
                        var width = size.SubstringUntil("x").ParseInt();
                        var height = size.SubstringAfter("x").ParseInt();
                        var resolution = new VideoResolution(width, height);
                        var framerate = streamInfoDic.Get("fps").ParseInt();

                        var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreamInfos.Add(streamInfo);
                    }
                }
            }

            // Resolve dash streams
            var dashManifestUrl = videoInfo.GetOrDefault("dashmpd");
            if (dashManifestUrl.IsNotBlank())
            {
                // Parse signature
                var sig = Regex.Match(dashManifestUrl, @"/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature if needed
                if (sig.IsNotBlank())
                {
                    var playerSource = await GetPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    sig = playerSource.Decipher(sig);
                    dashManifestUrl = UrlHelper.SetUrlPathParameter(dashManifestUrl, "signature", sig);
                }

                // Get the manifest
                var response = await _httpService.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
                var dashManifestXml = XElement.Parse(response).StripNamespaces();
                var streamsXml = dashManifestXml.Descendants("Representation");

                // Filter out partial streams
                streamsXml = streamsXml.Where(x => !(x.Descendant("Initialization")
                                                         ?.Attribute("sourceURL")
                                                         ?.Value.Contains("sq/") ?? false));

                // Parse streams
                foreach (var streamXml in streamsXml)
                {
                    var itag = (int)streamXml.AttributeStrict("id");
                    var url = streamXml.ElementStrict("BaseURL").Value;
                    var bitrate = (long)streamXml.AttributeStrict("bandwidth");

#if RELEASE
                    if (!MediaStreamInfo.IsKnown(itag))
                        continue;
#endif

                    // Parse content length
                    var contentLength = Regex.Match(url, @"clen[/=](\d+)").Groups[1].Value.ParseLong();

                    // Set rate bypass
                    url = url.Contains("?")
                        ? UrlHelper.SetUrlQueryParameter(url, "ratebypass", "yes")
                        : UrlHelper.SetUrlPathParameter(url, "ratebypass", "yes");

                    // Check if audio stream
                    var isAudio = streamXml.Element("AudioChannelConfiguration") != null;

                    // If audio stream
                    if (isAudio)
                    {
                        var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreamInfos.Add(streamInfo);
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var width = (int)streamXml.AttributeStrict("width");
                        var height = (int)streamXml.AttributeStrict("height");
                        var resolution = new VideoResolution(width, height);
                        var framerate = (int)streamXml.AttributeStrict("frameRate");

                        var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreamInfos.Add(streamInfo);
                    }
                }
            }

            // Finalize stream info collections
            muxedStreamInfos = muxedStreamInfos.Distinct(s => s.Itag).OrderByDescending(s => s.VideoQuality).ToList();
            audioStreamInfos = audioStreamInfos.Distinct(s => s.Itag).OrderByDescending(s => s.Bitrate).ToList();
            videoStreamInfos = videoStreamInfos.Distinct(s => s.Itag).OrderByDescending(s => s.VideoQuality).ToList();

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos);
        }

        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get video info
            var videoInfo = await GetVideoInfoAsync(videoId).ConfigureAwait(false);
            ThrowIfVideoInfoUnavailable(videoId, videoInfo);
            ThrowIfVideoInfoRequiresPurchase(videoId, videoInfo);

            // Get captions
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            var playerResponseRaw = videoInfo.GetOrDefault("player_response");
            if (playerResponseRaw.IsBlank())
                return closedCaptionTrackInfos;

            // Parse closed caption tracks
            var playerResponseJson = JToken.Parse(playerResponseRaw);
            var captionsJson = playerResponseJson.SelectToken("$..captionTracks");
            foreach (var captionJson in captionsJson.EmptyIfNull())
            {
                var code = captionJson.Value<string>("languageCode");
                var name = captionJson["name"].Value<string>("simpleText");
                var url = captionJson.Value<string>("baseUrl");
                var isAuto = captionJson.Value<string>("vssId").StartsWith("a.");

                var language = new Language(code, name);
                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAuto);

                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}