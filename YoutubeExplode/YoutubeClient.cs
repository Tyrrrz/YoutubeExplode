﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    /// <summary>
    /// YoutubeClient
    /// </summary>
    public partial class YoutubeClient
    {
        private readonly IHttpService _httpService;
        private readonly Dictionary<string, PlayerSource> _playerSourceCache;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with custom services
        /// </summary>
        public YoutubeClient(IHttpService httpService)
        {
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _playerSourceCache = new Dictionary<string, PlayerSource>();
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with default services
        /// </summary>
        public YoutubeClient()
            : this(HttpService.Instance)
        {
        }

        private async Task<PlayerSource> GetPlayerSourceAsync(string version)
        {
            // Original code credit: Decipherer class of https://github.com/flagbug/YoutubeExtractor

            // Try to get from cache
            var playerSource = _playerSourceCache.GetOrDefault(version);
            if (playerSource != null) return playerSource;

            // Get player source code
            string request = $"https://www.youtube.com/yts/jsbin/player-{version}/base.js";
            string response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Get the name of the function that handles deciphering
            string funcName = Regex.Match(response, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
            if (funcName.IsBlank())
                throw new ParseException("Could not find the entry function for signature deciphering");

            // Get the body of the function
            string funcPattern = @"(?!h\.)" + Regex.Escape(funcName) + @"=function\(\w+\)\{(.*?)\}";
            string funcBody = Regex.Match(response, funcPattern, RegexOptions.Singleline).Groups[1].Value;
            if (funcBody.IsBlank())
                throw new ParseException("Could not get the signature decipherer function body");
            var funcLines = funcBody.Split(";").ToArray();

            // Identify cipher functions
            string reverseFuncName = null;
            string sliceFuncName = null;
            string charSwapFuncName = null;
            var operations = new List<ICipherOperation>();

            // Analyze the function body to determine the names of cipher functions
            foreach (string line in funcLines)
            {
                // Break when all functions are found
                if (reverseFuncName.IsNotBlank() && sliceFuncName.IsNotBlank() && charSwapFuncName.IsNotBlank())
                    break;

                // Get the function called on this line
                string calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFunctionName.IsBlank())
                    continue;

                // Find cipher function names
                if (Regex.IsMatch(response, $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\)"))
                    reverseFuncName = calledFunctionName;
                else if (Regex.IsMatch(response,
                    $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                    sliceFuncName = calledFunctionName;
                else if (Regex.IsMatch(response,
                    $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                    charSwapFuncName = calledFunctionName;
            }

            // Analyze the function body again to determine the operation set and order
            foreach (string line in funcLines)
            {
                // Get the function called on this line
                string calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFunctionName.IsBlank())
                    continue;

                // Swap operation
                if (calledFunctionName == charSwapFuncName)
                {
                    int index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }
                // Slice operation
                else if (calledFunctionName == sliceFuncName)
                {
                    int index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }
                // Reverse operation
                else if (calledFunctionName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return _playerSourceCache[version] = new PlayerSource(version, operations);
        }

        private async Task<long> GetContentLengthAsync(string url)
        {
            // Get the headers
            var headers = await _httpService.GetHeadersAsync(url).ConfigureAwait(false);

            // Extract content length
            return headers["Content-Length"].ParseLong();
        }

        /// <summary>
        /// Checks whether a video with the given ID exists
        /// </summary>
        public async Task<bool> CheckVideoExistsAsync(string videoId)
        {
            if (videoId == null)
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get
            string request = $"https://www.youtube.com/get_video_info?video_id={videoId}&el=info&ps=default";
            string response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Parse
            var videoInfoDic = UrlHelper.DictionaryFromUrlEncoded(response);

            // Check error code
            if (videoInfoDic.ContainsKey("errorcode"))
            {
                int errorCode = videoInfoDic["errorcode"].ParseInt();
                return !errorCode.IsEither(100, 150);
            }

            return true;
        }

        /// <summary>
        /// Gets video info by video ID
        /// </summary>
        public async Task<VideoInfo> GetVideoInfoAsync(string videoId)
        {
            if (videoId == null)
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get video context
            string request = $"https://www.youtube.com/embed/{videoId}";
            string response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Parse video context
            string playerVersion =
                Regex.Match(response, @"<script.*?\ssrc=""/yts/jsbin/player-(.*?)/base.js").Groups[1].Value;
            string sts =
                Regex.Match(response, @"""sts""\s*:\s*(\d+)").Groups[1].Value;
            if (playerVersion.IsBlank() || sts.IsBlank())
            {
                throw new ParseException("Could not parse video context");
            }

            // Get video info
            request = $"https://www.youtube.com/get_video_info?video_id={videoId}&sts={sts}&el=info&ps=default";
            response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoInfoDic = UrlHelper.DictionaryFromUrlEncoded(response);

            // Check for error
            if (videoInfoDic.ContainsKey("errorcode"))
            {
                int errorCode = videoInfoDic["errorcode"].ParseInt();
                string errorReason = videoInfoDic.GetOrDefault("reason") ?? "<no reason>";
                throw new FrontendException(errorCode, errorReason);
            }

            // Parse metadata
            string title = videoInfoDic["title"];
            var duration = TimeSpan.FromSeconds(videoInfoDic["length_seconds"].ParseDouble());
            long viewCount = videoInfoDic["view_count"].ParseLong();
            var keywords = videoInfoDic["keywords"].Split(",");
            var watermarks = videoInfoDic["watermark"].Split(",");
            bool isListed = videoInfoDic["is_listed"] == "1";
            bool isRatingAllowed = videoInfoDic["allow_ratings"] == "1";
            bool isMuted = videoInfoDic["muted"] == "1";
            bool isEmbeddingAllowed = videoInfoDic["allow_embed"] == "1";

            // Parse mixed streams
            var mixedStreams = new List<MixedStreamInfo>();
            string mixedStreamsEncoded = videoInfoDic.GetOrDefault("url_encoded_fmt_stream_map");
            if (mixedStreamsEncoded.IsNotBlank())
            {
                foreach (string streamEncoded in mixedStreamsEncoded.Split(","))
                {
                    var streamDic = UrlHelper.DictionaryFromUrlEncoded(streamEncoded);

                    // Parse data
                    int itag = streamDic["itag"].ParseInt();
                    string url = streamDic["url"];
                    string sig = streamDic.GetOrDefault("s");

                    // Decipher signature
                    if (sig.IsNotBlank())
                    {
                        var playerSource = await GetPlayerSourceAsync(playerVersion).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlHelper.SetUrlQueryParameter(url, "signature", sig);
                    }

                    // Get content length
                    long contentLength = await GetContentLengthAsync(url).ConfigureAwait(false);

                    var stream = new MixedStreamInfo(itag, url, contentLength);
                    mixedStreams.Add(stream);
                }
            }

            // Parse adaptive streams
            var audioStreams = new List<AudioStreamInfo>();
            var videoStreams = new List<VideoStreamInfo>();
            string adaptiveStreamsEncoded = videoInfoDic.GetOrDefault("adaptive_fmts");
            if (adaptiveStreamsEncoded.IsNotBlank())
            {
                foreach (string streamEncoded in adaptiveStreamsEncoded.Split(","))
                {
                    var streamDic = UrlHelper.DictionaryFromUrlEncoded(streamEncoded);

                    // Parse data
                    int itag = streamDic["itag"].ParseInt();
                    string url = streamDic["url"];
                    string sig = streamDic.GetOrDefault("s");
                    long contentLength = streamDic["clen"].ParseLong();
                    long bitrate = streamDic["bitrate"].ParseLong();
                    string type = streamDic["type"];

                    // Decipher signature
                    if (sig.IsNotBlank())
                    {
                        var playerSource = await GetPlayerSourceAsync(playerVersion).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlHelper.SetUrlQueryParameter(url, "signature", sig);
                    }

                    // If audio stream
                    if (type.ContainsInvariant("audio/"))
                    {
                        var stream = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreams.Add(stream);
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        string size = streamDic["size"];
                        int width = size.SubstringUntil("x").ParseInt();
                        int height = size.SubstringAfter("x").ParseInt();
                        var resolution = new VideoResolution(width, height);
                        double framerate = streamDic["fps"].ParseDouble();

                        var stream = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreams.Add(stream);
                    }
                }
            }

            // Parse adaptive streams from dash
            string dashManifestUrl = videoInfoDic.GetOrDefault("dashmpd");
            if (dashManifestUrl.IsNotBlank())
            {
                // Parse signature
                string sig = Regex.Match(dashManifestUrl, @"/s/(.*?)(?:/|$)").Groups[1].Value;

                // Decipher signature
                if (sig.IsNotBlank())
                {
                    var playerSource = await GetPlayerSourceAsync(playerVersion).ConfigureAwait(false);
                    sig = playerSource.Decipher(sig);
                    dashManifestUrl = UrlHelper.SetUrlPathParameter(dashManifestUrl, "signature", sig);
                }

                // Get the manifest
                response = await _httpService.GetStringAsync(dashManifestUrl).ConfigureAwait(false);

                var dashManifestXml = XElement.Parse(response).StripNamespaces();
                var streamsXml = dashManifestXml.Descendants("Representation");

                // Filter out partial streams
                streamsXml = streamsXml
                    .Where(x => !(x.Descendant("Initialization")
                                      ?.Attribute("sourceURL")
                                      ?.Value.ContainsInvariant("sq/") ?? false));

                // Parse streams
                foreach (var streamXml in streamsXml)
                {
                    // Parse data
                    int itag = (int) streamXml.Attribute("id");
                    string url = (string) streamXml.Element("BaseURL");
                    long bitrate = (long) streamXml.Attribute("bandwidth");

                    // Extract content length
                    long contentLength = Regex.Match(url, @"clen[/=](\d+)").Groups[1].Value.ParseLong();

                    var audioConfigurationXml = streamXml.Element("AudioChannelConfiguration");

                    // If audio stream
                    if (audioConfigurationXml != null)
                    {
                        var stream = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreams.Add(stream);
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        int width = (int) streamXml.Attribute("width");
                        int height = (int) streamXml.Attribute("height");
                        var resolution = new VideoResolution(width, height);
                        double framerate = (double) streamXml.Attribute("frameRate");

                        var stream = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreams.Add(stream);
                    }
                }
            }

            // Finalize stream lists
            mixedStreams = mixedStreams.Distinct(s => s.Itag).OrderByDescending(s => s.VideoQuality).ToList();
            audioStreams = audioStreams.Distinct(s => s.Itag).OrderByDescending(s => s.Bitrate).ToList();
            videoStreams = videoStreams.Distinct(s => s.Itag).OrderByDescending(s => s.VideoQuality).ToList();

            // Parse closed caption tracks
            var captions = new List<ClosedCaptionTrackInfo>();
            string captionsEncoded = videoInfoDic.GetOrDefault("caption_tracks");
            if (captionsEncoded.IsNotBlank())
            {
                foreach (string captionEncoded in captionsEncoded.Split(","))
                {
                    var captionDic = UrlHelper.DictionaryFromUrlEncoded(captionEncoded);

                    // Parse data
                    string url = captionDic["u"];
                    bool isAuto = captionDic["v"].ContainsInvariant("a.");
                    string lang = captionDic["lc"];

                    // Fix for Hebrew
                    if (lang.EqualsInvariant("iw")) lang = "he";

                    var culture = new CultureInfo(lang);
                    var caption = new ClosedCaptionTrackInfo(url, culture, isAuto);
                    captions.Add(caption);
                }
            }

            // Get metadata extension
            request = $"https://www.youtube.com/get_video_metadata?video_id={videoId}";
            response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoInfoExtXml = XElement.Parse(response).StripNamespaces().Element("html_content");

            // Parse extension metadata
            string description = (string) videoInfoExtXml?.Element("video_info")?.Element("description");
            long likeCount = (long) videoInfoExtXml?.Element("video_info")?.Element("likes_count_unformatted");
            long dislikeCount = (long) videoInfoExtXml?.Element("video_info")?.Element("dislikes_count_unformatted");

            // Parse author info
            string authorId = (string) videoInfoExtXml?.Element("user_info")?.Element("channel_external_id");
            string authorName = (string) videoInfoExtXml?.Element("user_info")?.Element("username");
            string authorDisplayName = (string) videoInfoExtXml?.Element("user_info")?.Element("public_name");
            string authorChannelTitle = (string) videoInfoExtXml?.Element("user_info")?.Element("channel_title");
            bool authorIsPaid = (string) videoInfoExtXml?.Element("user_info")?.Element("channel_title") == "1";
            string authorLogoUrl = (string) videoInfoExtXml?.Element("user_info")?.Element("channel_logo_url");
            string authorBannerUrl = (string) videoInfoExtXml?.Element("user_info")?.Element("channel_banner_url");
            var author = new UserInfo(
                authorId, authorName, authorDisplayName, authorChannelTitle,
                authorIsPaid, authorLogoUrl, authorBannerUrl);

            return new VideoInfo(
                videoId, title, author,
                duration, description, keywords, watermarks,
                viewCount, likeCount, dislikeCount,
                isListed, isRatingAllowed, isMuted, isEmbeddingAllowed,
                mixedStreams, audioStreams, videoStreams, captions);
        }

        /// <summary>
        /// Gets playlist metadata by playlist ID, optionally truncating video list at given number of pages
        /// </summary>
        public async Task<PlaylistInfo> GetPlaylistInfoAsync(string playlistId, int maxPages = int.MaxValue)
        {
            if (playlistId == null)
                throw new ArgumentNullException(nameof(playlistId));
            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException("Invalid Youtube playlist ID", nameof(playlistId));
            if (maxPages <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPages), "Needs to be a positive number");

            // Get playlist info
            string request = $"https://www.youtube.com/list_ajax?style=xml&action_get_list=1&list={playlistId}";
            string response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var playlistInfoXml = XElement.Parse(response).StripNamespaces();

            // Parse metadata
            string title = (string) playlistInfoXml.Element("title");
            string author = (string) playlistInfoXml.Element("author") ?? "";
            string description = (string) playlistInfoXml.Element("description");
            long viewCount = (long) playlistInfoXml.Element("views");

            // Parse video IDs
            var videoIds = playlistInfoXml.Descendants("encrypted_id").Select(e => (string) e).ToArray();

            // Continue with next pages
            int pagesDone = 1;
            bool canContinue = videoIds.Length > 0;
            int offset = videoIds.Length;
            while (pagesDone < maxPages && canContinue)
            {
                // Get next page
                string nextRequest = request + $"&index={offset}";
                response = await _httpService.GetStringAsync(nextRequest).ConfigureAwait(false);

                // Parse video IDs
                var nextPlaylistInfoXml = XElement.Parse(response).StripNamespaces();
                var nextVideoIds = nextPlaylistInfoXml.Descendants("encrypted_id").Select(e => (string) e);
                int delta = videoIds.Length;
                videoIds = videoIds.Union(nextVideoIds).ToArray();
                delta = videoIds.Length - delta;

                // Check if there's more pages
                pagesDone++;
                canContinue = delta > 0;
                offset += videoIds.Length;
            }

            return new PlaylistInfo(
                playlistId, title, author, description, viewCount,
                videoIds);
        }

        /// <summary>
        /// Gets actual media stream by its metadata
        /// </summary>
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo mediaStreamInfo)
        {
            if (mediaStreamInfo == null)
                throw new ArgumentNullException(nameof(mediaStreamInfo));

            // Get the stream
            var stream = await _httpService.GetStreamAsync(mediaStreamInfo.Url).ConfigureAwait(false);

            return new MediaStream(stream, mediaStreamInfo);
        }

        /// <summary>
        /// Gets actual closed caption track by its metadata
        /// </summary>
        public async Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo closedCaptionTrackInfo)
        {
            if (closedCaptionTrackInfo == null)
                throw new ArgumentNullException(nameof(closedCaptionTrackInfo));

            // Get closed caption track manifest
            string response = await _httpService.GetStringAsync(closedCaptionTrackInfo.Url).ConfigureAwait(false);
            var captionTrackXml = XElement.Parse(response).StripNamespaces();

            // Parse content
            var captions = new List<ClosedCaption>();
            foreach (var captionXml in captionTrackXml.Descendants("text"))
            {
                string text = (string) captionXml;
                var offset = TimeSpan.FromSeconds((double) captionXml.Attribute("start"));
                var duration = TimeSpan.FromSeconds((double) captionXml.Attribute("dur"));

                var caption = new ClosedCaption(text, offset, duration);
                captions.Add(caption);
            }

            return new ClosedCaptionTrack(closedCaptionTrackInfo, captions);
        }
    }

    public partial class YoutubeClient
    {
        /// <summary>
        /// Verifies that the given string is syntactically a valid Youtube video ID
        /// </summary>
        public static bool ValidateVideoId(string videoId)
        {
            if (videoId.IsBlank())
                return false;

            if (videoId.Length != 11)
                return false;

            return !Regex.IsMatch(videoId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse video ID from a youtube video URL
        /// </summary>
        public static bool TryParseVideoId(string videoUrl, out string videoId)
        {
            videoId = default(string);

            if (videoUrl.IsBlank())
                return false;

            // https://www.youtube.com/watch?v=yIVRs6YSbOM
            string regularMatch =
                Regex.Match(videoUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidateVideoId(regularMatch))
            {
                videoId = regularMatch;
                return true;
            }

            // https://youtu.be/yIVRs6YSbOM
            string shortMatch =
                Regex.Match(videoUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (shortMatch.IsNotBlank() && ValidateVideoId(shortMatch))
            {
                videoId = shortMatch;
                return true;
            }

            // https://www.youtube.com/embed/yIVRs6YSbOM
            string embedMatch =
                Regex.Match(videoUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (embedMatch.IsNotBlank() && ValidateVideoId(embedMatch))
            {
                videoId = embedMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses video ID from a Youtube video URL
        /// </summary>
        public static string ParseVideoId(string videoUrl)
        {
            if (videoUrl == null)
                throw new ArgumentNullException(nameof(videoUrl));

            bool success = TryParseVideoId(videoUrl, out string result);
            if (success)
                return result;

            throw new FormatException($"Could not parse video ID from given string [{videoUrl}]");
        }

        /// <summary>
        /// Verifies that the given string is syntactically a valid Youtube playlist ID
        /// </summary>
        public static bool ValidatePlaylistId(string playlistId)
        {
            if (playlistId.IsBlank())
                return false;

            if (!playlistId.Length.IsEither(2, 13, 18, 24, 34))
                return false;

            return !Regex.IsMatch(playlistId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse playlist ID from a Youtube playlist URL
        /// </summary>
        public static bool TryParsePlaylistId(string playlistUrl, out string playlistId)
        {
            playlistId = default(string);

            if (playlistUrl.IsBlank())
                return false;

            // https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
            string regularMatch =
                Regex.Match(playlistUrl, @"youtube\..+?/playlist.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidatePlaylistId(regularMatch))
            {
                playlistId = regularMatch;
                return true;
            }

            // https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            string compositeMatch =
                Regex.Match(playlistUrl, @"youtube\..+?/watch.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (compositeMatch.IsNotBlank() && ValidatePlaylistId(compositeMatch))
            {
                playlistId = compositeMatch;
                return true;
            }

            // https://youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            string shortCompositeMatch =
                Regex.Match(playlistUrl, @"youtu\.be/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (shortCompositeMatch.IsNotBlank() && ValidatePlaylistId(shortCompositeMatch))
            {
                playlistId = shortCompositeMatch;
                return true;
            }

            // https://www.youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            string embedCompositeMatch =
                Regex.Match(playlistUrl, @"youtube\..+?/embed/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (embedCompositeMatch.IsNotBlank() && ValidatePlaylistId(embedCompositeMatch))
            {
                playlistId = embedCompositeMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses playlist ID from a Youtube playlist URL
        /// </summary>
        public static string ParsePlaylistId(string playlistUrl)
        {
            if (playlistUrl == null)
                throw new ArgumentNullException(nameof(playlistUrl));

            bool success = TryParsePlaylistId(playlistUrl, out string result);
            if (success)
                return result;

            throw new FormatException($"Could not parse playlist ID from given string [{playlistUrl}]");
        }
    }
}