using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
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
        private async Task<string> GetVideoEmbedPageRawAsync(string videoId)
        {
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        private async Task<JToken> GetVideoEmbedPageConfigAsync(string videoId)
        {
            // TODO: check if video is available

            var raw = await GetVideoEmbedPageRawAsync(videoId).ConfigureAwait(false);
            var part = raw.SubstringAfter("yt.setConfig({'PLAYER_CONFIG': ").SubstringUntil(",'");
            return JToken.Parse(part);
        }

        private async Task<string> GetVideoWatchPageRawAsync(string videoId)
        {
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&hl=en";
            return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        private async Task<IHtmlDocument> GetVideoWatchPageAsync(string videoId)
        {
            // TODO: check if video is available

            var raw = await GetVideoWatchPageRawAsync(videoId).ConfigureAwait(false);
            return await new HtmlParser().ParseAsync(raw).ConfigureAwait(false);
        }

        private async Task<string> GetVideoInfoRawAsync(string videoId, string el = "", string sts = "")
        {
            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el={el}&sts={sts}&hl=en";
            return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        private async Task<IReadOnlyDictionary<string, string>> GetVideoInfoAsync(string videoId, string sts = "")
        {
            // Get video info
            var raw = await GetVideoInfoRawAsync(videoId, "embedded", sts).ConfigureAwait(false);
            var videoInfo = UrlEx.SplitQuery(raw);

            // If can't be embedded - try another value of el
            if (videoInfo.ContainsKey("errorcode"))
            {
                var errorReason = videoInfo["reason"];
                if (errorReason.Contains("&feature=player_embedded"))
                {
                    raw = await GetVideoInfoRawAsync(videoId, "detailpage", sts).ConfigureAwait(false);
                    videoInfo = UrlEx.SplitQuery(raw);
                }
            }

            // Check error
            if (videoInfo.ContainsKey("errorcode"))
            {
                var errorCode = videoInfo["errorcode"].ParseInt();
                var errorReason = videoInfo["reason"];

                throw new VideoUnavailableException(videoId, errorCode, errorReason);
            }

            return videoInfo;
        }

        private async Task<PlayerContext> GetVideoPlayerContextAsync(string videoId)
        {
            // Get embed page config
            var configJson = await GetVideoEmbedPageConfigAsync(videoId).ConfigureAwait(false);

            // Extract values
            var sourceUrl = configJson["assets"]["js"].Value<string>();
            var sts = configJson["sts"].Value<string>();

            // Check if successful
            if (sourceUrl.IsBlank() || sts.IsBlank())
                throw new ParseException("Could not parse player context.");

            // Append host to source url
            sourceUrl = "https://www.youtube.com" + sourceUrl;

            return new PlayerContext(sourceUrl, sts);
        }

        private async Task<PlayerSource> GetVideoPlayerSourceAsync(string sourceUrl)
        {
            // Original code credit:
            // https://github.com/flagbug/YoutubeExtractor/blob/3106efa1063994fd19c0e967793315f6962b2d3c/YoutubeExtractor/YoutubeExtractor/Decipherer.cs
            // No copyright, MIT license

            // Try to resolve from cache first
            var playerSource = _playerSourceCache.GetOrDefault(sourceUrl);
            if (playerSource != null)
                return playerSource;

            // Get player source code
            var sourceRaw = await _httpClient.GetStringAsync(sourceUrl).ConfigureAwait(false);

            // Find the name of the function that handles deciphering
            var entryPoint = Regex.Match(sourceRaw, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
            if (entryPoint.IsBlank())
                throw new ParseException("Could not find the entry function for signature deciphering.");

            // Find the body of the function
            var entryPointPattern = @"(?!h\.)" + Regex.Escape(entryPoint) + @"=function\(\w+\)\{(.*?)\}";
            var entryPointBody = Regex.Match(sourceRaw, entryPointPattern, RegexOptions.Singleline).Groups[1].Value;
            if (entryPointBody.IsBlank())
                throw new ParseException("Could not find the signature decipherer function body.");
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
                if (Regex.IsMatch(sourceRaw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(sourceRaw,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(sourceRaw,
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

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info
            var videoInfo = await GetVideoInfoAsync(videoId).ConfigureAwait(false);

            // Extract values
            var title = videoInfo["title"];
            var author = videoInfo["author"];
            var duration = TimeSpan.FromSeconds(videoInfo["length_seconds"].ParseDouble());
            var viewCount = videoInfo["view_count"].ParseLong();
            var keywords = videoInfo["keywords"].Split(",");

            // Get video watch page
            var watchPage = await GetVideoWatchPageAsync(videoId).ConfigureAwait(false);

            // Extract values
            var uploadDate = watchPage.QuerySelector("meta[itemprop=\"datePublished\"]").GetAttribute("content")
                .ParseDateTime("yyyy-MM-dd");
            var description = watchPage.QuerySelector("p#eow-description").TextEx();
            var likeCount = watchPage.QuerySelector("button.like-button-renderer-like-button").Text()
                .StripNonDigit().ParseLongOrDefault();
            var dislikeCount = watchPage.QuerySelector("button.like-button-renderer-dislike-button").Text()
                .StripNonDigit().ParseLongOrDefault();
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            var thumbnails = new ThumbnailSet(videoId);
            return new Video(videoId, author, uploadDate, title, description, thumbnails, duration, keywords,
                statistics);
        }

        /// <inheritdoc />
        public async Task<Channel> GetVideoAuthorChannelAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info just to check error code
            await GetVideoInfoAsync(videoId).ConfigureAwait(false);

            // Get embed page config
            var configJson = await GetVideoEmbedPageConfigAsync(videoId).ConfigureAwait(false);

            // Extract values
            var channelPath = configJson["args"]["channel_path"].Value<string>();
            var id = channelPath.SubstringAfter("channel/");
            var title = configJson["args"]["author"].Value<string>();
            var logoUrl = configJson["args"]["profile_picture"].Value<string>();

            return new Channel(id, title, logoUrl);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get player context
            var playerContext = await GetVideoPlayerContextAsync(videoId).ConfigureAwait(false);

            // Get video info
            var videoInfo = await GetVideoInfoAsync(videoId, playerContext.Sts).ConfigureAwait(false);

            // Check if requires purchase
            if (videoInfo.ContainsKey("ypc_vid"))
            {
                var previewVideoId = videoInfo["ypc_vid"];
                throw new VideoRequiresPurchaseException(videoId, previewVideoId);
            }

            // Prepare stream info collections
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Resolve muxed streams
            var muxedStreamInfosEncoded = videoInfo.GetOrDefault("url_encoded_fmt_stream_map");
            if (muxedStreamInfosEncoded.IsNotBlank())
            {
                foreach (var streamEncoded in muxedStreamInfosEncoded.Split(","))
                {
                    var streamInfoDic = UrlEx.SplitQuery(streamEncoded);

                    // Extract values
                    var itag = streamInfoDic["itag"].ParseInt();
                    var url = streamInfoDic["url"];
                    var sig = streamInfoDic.GetOrDefault("s");

#if RELEASE
                    if (!MediaStreamInfo.IsKnown(itag))
                        continue;
#endif

                    // Decipher signature if needed
                    if (sig.IsNotBlank())
                    {
                        var playerSource =
                            await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlEx.SetQueryParameter(url, "signature", sig);
                    }

                    // Probe stream and get content length
                    long contentLength;
                    using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                    using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        // Some muxed streams can be gone
                        if (response.StatusCode == HttpStatusCode.NotFound ||
                            response.StatusCode == HttpStatusCode.Gone)
                            continue;

                        // Ensure success
                        response.EnsureSuccessStatusCode();

                        // Extract content length
                        contentLength = response.Content.Headers.ContentLength ??
                                        throw new ParseException("Could not extract content length of muxed stream.");
                    }

                    var streamInfo = new MuxedStreamInfo(itag, url, contentLength);
                    muxedStreamInfoMap[itag] = streamInfo;
                }
            }

            // Resolve adaptive streams
            var adaptiveStreamInfosEncoded = videoInfo.GetOrDefault("adaptive_fmts");
            if (adaptiveStreamInfosEncoded.IsNotBlank())
            {
                foreach (var streamEncoded in adaptiveStreamInfosEncoded.Split(","))
                {
                    var streamInfoDic = UrlEx.SplitQuery(streamEncoded);

                    // Extract values
                    var itag = streamInfoDic["itag"].ParseInt();
                    var url = streamInfoDic["url"];
                    var sig = streamInfoDic.GetOrDefault("s");
                    var contentLength = streamInfoDic["clen"].ParseLong();
                    var bitrate = streamInfoDic["bitrate"].ParseLong();

#if RELEASE
                    if (!MediaStreamInfo.IsKnown(itag))
                        continue;
#endif

                    // Decipher signature if needed
                    if (sig.IsNotBlank())
                    {
                        var playerSource =
                            await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlEx.SetQueryParameter(url, "signature", sig);
                    }

                    // Check if audio
                    var isAudio = streamInfoDic["type"].Contains("audio/");

                    // If audio stream
                    if (isAudio)
                    {
                        var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreamInfoMap[itag] = streamInfo;
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var size = streamInfoDic["size"];
                        var width = size.SubstringUntil("x").ParseInt();
                        var height = size.SubstringAfter("x").ParseInt();
                        var resolution = new VideoResolution(width, height);
                        var framerate = streamInfoDic["fps"].ParseInt();
                        var qualityLabel = streamInfoDic["quality_label"];

                        var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate,
                            qualityLabel);
                        videoStreamInfoMap[itag] = streamInfo;
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
                    var playerSource = await GetVideoPlayerSourceAsync(playerContext.SourceUrl).ConfigureAwait(false);
                    sig = playerSource.Decipher(sig);
                    dashManifestUrl = UrlEx.SetRouteParameter(dashManifestUrl, "signature", sig);
                }

                // Get the manifest
                var response = await _httpClient.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
                var dashManifestXml = XElement.Parse(response).StripNamespaces();
                var streamsXml = dashManifestXml.Descendants("Representation");

                // Parse streams
                foreach (var streamXml in streamsXml)
                {
                    // Skip partial streams
                    if (streamXml.Descendants("Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value
                            .Contains("sq/") == true)
                        continue;

                    // Extract values
                    var itag = (int) streamXml.Attribute("id");
                    var url = (string) streamXml.Element("BaseURL");
                    var bitrate = (long) streamXml.Attribute("bandwidth");

#if RELEASE
                    if (!MediaStreamInfo.IsKnown(itag))
                        continue;
#endif

                    // Parse content length
                    var contentLength = Regex.Match(url, @"clen[/=](\d+)").Groups[1].Value.ParseLong();

                    // Check if audio stream
                    var isAudio = streamXml.Element("AudioChannelConfiguration") != null;

                    // If audio stream
                    if (isAudio)
                    {
                        var streamInfo = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreamInfoMap[itag] = streamInfo;
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var width = (int) streamXml.Attribute("width");
                        var height = (int) streamXml.Attribute("height");
                        var resolution = new VideoResolution(width, height);
                        var framerate = (int) streamXml.Attribute("frameRate");

                        var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreamInfoMap[itag] = streamInfo;
                    }
                }
            }

            // Get the raw HLS stream playlist (*.m3u8)
            var hlsLiveStreamUrl = videoInfo.GetOrDefault("hlsvp");

            // Finalize stream info collections
            var muxedStreamInfos = muxedStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();
            var audioStreamInfos = audioStreamInfoMap.Values.OrderByDescending(s => s.Bitrate).ToArray();
            var videoStreamInfos = videoStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsLiveStreamUrl);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info
            var videoInfo = await GetVideoInfoAsync(videoId).ConfigureAwait(false);

            // Extract captions metadata
            var playerResponseRaw = videoInfo["player_response"];
            var playerResponseJson = JToken.Parse(playerResponseRaw);
            var captionsJson = playerResponseJson.SelectToken("$..captionTracks").EmptyIfNull();

            // Parse closed caption tracks
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var captionJson in captionsJson)
            {
                // Extract values
                var code = captionJson["languageCode"].Value<string>();
                var name = captionJson["name"]["simpleText"].Value<string>();
                var language = new Language(code, name);
                var isAuto = captionJson["vssId"].Value<string>().StartsWith("a.", StringComparison.OrdinalIgnoreCase);
                var url = captionJson["baseUrl"].Value<string>();

                // Enforce format
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAuto);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}