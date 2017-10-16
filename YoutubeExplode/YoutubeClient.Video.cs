using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        private async Task<PlayerContext> GetPlayerContextAsync(string videoId)
        {
            // Get the embed video page
            var request = $"{YoutubeHost}/embed/{videoId}";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Extract values
            var sourceUrl = Regex.Match(response, @"""js""\s*:\s*""(.*?)""").Groups[1].Value.Replace("\\", "");
            var sts = Regex.Match(response, @"""sts""\s*:\s*(\d+)").Groups[1].Value;

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

        private async Task ResolveMuxedStreamInfosAsync(PlayerContext context, string encodedData,
            ICollection<MuxedStreamInfo> streamInfos)
        {
            foreach (var streamEncoded in encodedData.Split(","))
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
                    var playerSource = await GetPlayerSourceAsync(context.SourceUrl).ConfigureAwait(false);
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
                streamInfos.Add(streamInfo);
            }
        }

        private async Task ResolveAdaptiveStreamInfosAsync(PlayerContext context, string encodedData,
            ICollection<AudioStreamInfo> audioStreamInfos, ICollection<VideoStreamInfo> videoStreamInfos)
        {
            foreach (var streamEncoded in encodedData.Split(","))
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
                    var playerSource = await GetPlayerSourceAsync(context.SourceUrl).ConfigureAwait(false);
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

        private async Task ResolveDashStreamInfosAsync(PlayerContext context, string dashManifestUrl,
            ICollection<AudioStreamInfo> audioStreamInfos, ICollection<VideoStreamInfo> videoStreamInfos)
        {
            // Parse signature
            var sig = Regex.Match(dashManifestUrl, @"/s/(.*?)(?:/|$)").Groups[1].Value;

            // Decipher signature if needed
            if (sig.IsNotBlank())
            {
                var playerSource = await GetPlayerSourceAsync(context.SourceUrl).ConfigureAwait(false);
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
                var itag = (int) streamXml.AttributeStrict("id");
                var url = streamXml.ElementStrict("BaseURL").Value;
                var bitrate = (long) streamXml.AttributeStrict("bandwidth");

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
                    var width = (int) streamXml.AttributeStrict("width");
                    var height = (int) streamXml.AttributeStrict("height");
                    var resolution = new VideoResolution(width, height);
                    var framerate = (int) streamXml.AttributeStrict("frameRate");

                    var streamInfo = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                    videoStreamInfos.Add(streamInfo);
                }
            }
        }

        private void ParseClosedCaptionTrackInfos(string encodedData,
            ICollection<ClosedCaptionTrackInfo> closedCaptionTrackInfos)
        {
            foreach (var captionEncoded in encodedData.Split(","))
            {
                var captionInfoDic = UrlHelper.GetDictionaryFromUrlQuery(captionEncoded);

                var url = captionInfoDic.Get("u");

                var code = captionInfoDic.Get("lc");
                var name = captionInfoDic.Get("n");
                var language = new Language(code, name);

                var isAuto = captionInfoDic.Get("v").Contains("a.");

                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAuto);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }
        }

        /// <summary>
        /// Checks whether a video with the given ID exists
        /// </summary>
        public async Task<bool> CheckVideoExistsAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get video info
            var request = $"{YoutubeHost}/get_video_info?video_id={videoId}&el=info&ps=default&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoDic = UrlHelper.GetDictionaryFromUrlQuery(response);

            // Check error code
            if (videoDic.ContainsKey("errorcode"))
            {
                var errorCode = videoDic.Get("errorcode").ParseInt();
                return errorCode != 100 && errorCode != 150;
            }

            return true;
        }

        /// <summary>
        /// Gets video by ID
        /// </summary>
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get player context
            var context = await GetPlayerContextAsync(videoId).ConfigureAwait(false);

            // Get video info
            var request = $"{YoutubeHost}/get_video_info?video_id={videoId}&sts={context.Sts}&el=info&ps=default&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoDic = UrlHelper.GetDictionaryFromUrlQuery(response);

            // Check error code
            if (videoDic.ContainsKey("errorcode"))
            {
                var errorCode = videoDic.Get("errorcode").ParseInt();
                var errorReason = videoDic.Get("reason");
                throw new VideoNotAvailableException(videoId, errorCode, errorReason);
            }

            // Check if video requires purchase
            if (videoDic.GetOrDefault("requires_purchase") == "1")
            {
                var previewVideoId = videoDic.Get("ypc_vid");
                throw new VideoRequiresPurchaseException(videoId, previewVideoId);
            }

            // Parse metadata
            var title = videoDic.Get("title");
            var duration = TimeSpan.FromSeconds(videoDic.Get("length_seconds").ParseDouble());
            var viewCount = videoDic.Get("view_count").ParseLong();
            var keywords = videoDic.Get("keywords").Split(",");
            var isListed = videoDic.GetOrDefault("is_listed") == "1"; // unlisted videos don't have this
            var isRatingAllowed = videoDic.Get("allow_ratings") == "1";
            var isMuted = videoDic.Get("muted") == "1";
            var isEmbeddingAllowed = videoDic.Get("allow_embed") == "1";

            // Prepare stream info collections
            var muxedStreamInfos = new List<MuxedStreamInfo>();
            var audioStreamInfos = new List<AudioStreamInfo>();
            var videoStreamInfos = new List<VideoStreamInfo>();

            // Resolve muxed streams
            var muxedStreamInfosEncoded = videoDic.GetOrDefault("url_encoded_fmt_stream_map");
            if (muxedStreamInfosEncoded.IsNotBlank())
            {
                await ResolveMuxedStreamInfosAsync(context, muxedStreamInfosEncoded, muxedStreamInfos)
                    .ConfigureAwait(false);
            }

            // Resolve adaptive streams
            var adaptiveStreamInfosEncoded = videoDic.GetOrDefault("adaptive_fmts");
            if (adaptiveStreamInfosEncoded.IsNotBlank())
            {
                await ResolveAdaptiveStreamInfosAsync(context, adaptiveStreamInfosEncoded,
                        audioStreamInfos, videoStreamInfos)
                    .ConfigureAwait(false);
            }

            // Resolve dash streams
            var dashManifestUrl = videoDic.GetOrDefault("dashmpd");
            if (dashManifestUrl.IsNotBlank())
            {
                await ResolveDashStreamInfosAsync(context, dashManifestUrl,
                        audioStreamInfos, videoStreamInfos)
                    .ConfigureAwait(false);
            }

            // Finalize stream info collections
            muxedStreamInfos = muxedStreamInfos.Distinct(s => s.Itag).OrderByDescending(s => s.VideoQuality).ToList();
            audioStreamInfos = audioStreamInfos.Distinct(s => s.Itag).OrderByDescending(s => s.Bitrate).ToList();
            videoStreamInfos = videoStreamInfos.Distinct(s => s.Itag).OrderByDescending(s => s.VideoQuality).ToList();

            // Parse closed caption tracks
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            var closedCaptionTrackInfosEncoded = videoDic.GetOrDefault("caption_tracks");
            if (closedCaptionTrackInfosEncoded.IsNotBlank())
            {
                ParseClosedCaptionTrackInfos(closedCaptionTrackInfosEncoded, closedCaptionTrackInfos);
            }

            // Get metadata extension
            request = $"{YoutubeHost}/get_video_metadata?video_id={videoId}";
            response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoXml = XElement.Parse(response).StripNamespaces().ElementStrict("html_content");

            // Parse metadata extension
            var description = videoXml.ElementStrict("video_info").ElementStrict("description").Value;
            var likeCount = (long) videoXml.ElementStrict("video_info").ElementStrict("likes_count_unformatted");
            var dislikeCount = (long) videoXml.ElementStrict("video_info").ElementStrict("dislikes_count_unformatted");

            // Parse author info
            var authorId = videoXml.ElementStrict("user_info").ElementStrict("channel_external_id").Value;
            var authorName = videoXml.ElementStrict("user_info").ElementStrict("username").Value;
            var authorTitle = videoXml.ElementStrict("user_info").ElementStrict("channel_title").Value;
            var authorIsPaid = videoXml.ElementStrict("user_info").ElementStrict("channel_paid").Value == "1";
            var authorLogoUrl = videoXml.ElementStrict("user_info").ElementStrict("channel_logo_url").Value;
            var authorBannerUrl = videoXml.ElementStrict("user_info").ElementStrict("channel_banner_url").Value;

            // Concat metadata
            var author = new Channel(authorId, authorName, authorTitle, authorIsPaid, authorLogoUrl, authorBannerUrl);
            var thumbnails = new VideoThumbnails(videoId);
            var status = new VideoStatus(isListed, isRatingAllowed, isMuted, isEmbeddingAllowed);
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Video(videoId, author, title, description, thumbnails, duration, keywords, status, statistics,
                muxedStreamInfos, audioStreamInfos, videoStreamInfos, closedCaptionTrackInfos);
        }
    }
}