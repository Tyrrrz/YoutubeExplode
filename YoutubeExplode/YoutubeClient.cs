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

#if NET45 || NETCOREAPP1_0
using System.IO;
using System.Text;
using System.Threading;
#endif

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

        private async Task<PlayerContext> GetPlayerContextAsync(string videoId)
        {
            // Get the embed video page
            var request = YoutubeHost + $"/embed/{videoId}";
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
            var funcName = Regex.Match(response, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
            if (funcName.IsBlank())
                throw new ParseException("Could not find the entry function for signature deciphering");

            // Find the body of the function
            var funcPattern = @"(?!h\.)" + Regex.Escape(funcName) + @"=function\(\w+\)\{(.*?)\}";
            var funcBody = Regex.Match(response, funcPattern, RegexOptions.Singleline).Groups[1].Value;
            if (funcBody.IsBlank())
                throw new ParseException("Could not find the signature decipherer function body");
            var funcLines = funcBody.Split(";").ToArray();

            // Identify cipher functions
            string reverseFuncName = null;
            string sliceFuncName = null;
            string charSwapFuncName = null;
            var operations = new List<ICipherOperation>();

            // Analyze the function body to determine the names of cipher functions
            foreach (var line in funcLines)
            {
                // Break when all functions are found
                if (reverseFuncName.IsNotBlank() && sliceFuncName.IsNotBlank() && charSwapFuncName.IsNotBlank())
                    break;

                // Get the function called on this line
                var calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFunctionName.IsBlank())
                    continue;

                // Find cipher function names
                if (Regex.IsMatch(response, $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFunctionName;
                }
                else if (Regex.IsMatch(response,
                    $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFunctionName;
                }
                else if (Regex.IsMatch(response,
                    $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    charSwapFuncName = calledFunctionName;
                }
            }

            // Analyze the function body again to determine the operation set and order
            foreach (var line in funcLines)
            {
                // Get the function called on this line
                var calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFunctionName.IsBlank())
                    continue;

                // Swap operation
                if (calledFunctionName == charSwapFuncName)
                {
                    var index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }
                // Slice operation
                else if (calledFunctionName == sliceFuncName)
                {
                    var index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }
                // Reverse operation
                else if (calledFunctionName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return _playerSourceCache[sourceUrl] = new PlayerSource(operations);
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
            var request = YoutubeHost + $"/get_video_info?video_id={videoId}&el=info&ps=default&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);

            // Parse
            var videoInfoDic = UrlHelper.GetDictionaryFromUrlQuery(response);

            // Check error code
            if (videoInfoDic.ContainsKey("errorcode"))
            {
                var errorCode = videoInfoDic.Get("errorcode").ParseInt();
                return errorCode != 100 && errorCode != 150;
            }

            return true;
        }

        /// <summary>
        /// Gets video info by ID
        /// </summary>
        public async Task<VideoInfo> GetVideoInfoAsync(string videoId)
        {
            if (videoId == null)
                throw new ArgumentNullException(nameof(videoId));
            if (!ValidateVideoId(videoId))
                throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

            // Get player context
            var context = await GetPlayerContextAsync(videoId).ConfigureAwait(false);

            // Get video info
            var request = YoutubeHost + $"/get_video_info?video_id={videoId}&sts={context.Sts}&el=info&ps=default&hl=en";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoInfoDic = UrlHelper.GetDictionaryFromUrlQuery(response);

            // Check for error
            if (videoInfoDic.ContainsKey("errorcode"))
            {
                var errorCode = videoInfoDic.Get("errorcode").ParseInt();
                var errorReason = videoInfoDic.Get("reason");
                throw new VideoNotAvailableException(errorCode, errorReason);
            }

            // Check for paid content
            if (videoInfoDic.GetOrDefault("requires_purchase") == "1")
            {
                var previewVideoId = videoInfoDic.Get("ypc_vid");
                throw new VideoRequiresPurchaseException(previewVideoId);
            }

            // Parse metadata
            var title = videoInfoDic.Get("title");
            var duration = TimeSpan.FromSeconds(videoInfoDic.Get("length_seconds").ParseDouble());
            var viewCount = videoInfoDic.Get("view_count").ParseLong();
            var keywords = videoInfoDic.Get("keywords").Split(",");
            var watermarks = videoInfoDic.Get("watermark").Split(",");
            var isListed = videoInfoDic.GetOrDefault("is_listed") == "1";
            var isRatingAllowed = videoInfoDic.Get("allow_ratings") == "1";
            var isMuted = videoInfoDic.Get("muted") == "1";
            var isEmbeddingAllowed = videoInfoDic.Get("allow_embed") == "1";

            // Parse mixed streams
            var mixedStreams = new List<MixedStreamInfo>();
            var mixedStreamsEncoded = videoInfoDic.GetOrDefault("url_encoded_fmt_stream_map");
            if (mixedStreamsEncoded.IsNotBlank())
            {
                foreach (var streamEncoded in mixedStreamsEncoded.Split(","))
                {
                    var streamInfoDic = UrlHelper.GetDictionaryFromUrlQuery(streamEncoded);

                    var itag = streamInfoDic.Get("itag").ParseInt();

#if RELEASE
                    // Skip unknown itags on RELEASE
                    if (!MediaStreamInfo.IsKnown(itag)) continue;
#endif

                    var url = streamInfoDic.Get("url");
                    var sig = streamInfoDic.GetOrDefault("s");

                    // Decipher signature if needed
                    if (sig.IsNotBlank())
                    {
                        var playerSource = await GetPlayerSourceAsync(context.SourceUrl).ConfigureAwait(false);
                        sig = playerSource.Decipher(sig);
                        url = UrlHelper.SetUrlQueryParameter(url, "signature", sig);
                    }

                    // Get content length
                    long contentLength;
                    using (var reqMsg = new HttpRequestMessage(HttpMethod.Head, url))
                    using (var resMsg = await _httpService.PerformRequestAsync(reqMsg).ConfigureAwait(false))
                    {
                        // Check status code (https://github.com/Tyrrrz/YoutubeExplode/issues/36)
                        if (resMsg.StatusCode == HttpStatusCode.NotFound ||
                            resMsg.StatusCode == HttpStatusCode.Gone)
                            continue;

                        // Ensure success
                        resMsg.EnsureSuccessStatusCode();

                        // Extract content length
                        contentLength = resMsg.Content.Headers.ContentLength ?? -1;
                        if (contentLength < 0)
                            throw new ParseException("Could not extract content length");
                    }

                    // Set rate bypass
                    url = UrlHelper.SetUrlQueryParameter(url, "ratebypass", "yes");

                    var stream = new MixedStreamInfo(itag, url, contentLength);
                    mixedStreams.Add(stream);
                }
            }

            // Parse adaptive streams
            var audioStreams = new List<AudioStreamInfo>();
            var videoStreams = new List<VideoStreamInfo>();
            var adaptiveStreamsEncoded = videoInfoDic.GetOrDefault("adaptive_fmts");
            if (adaptiveStreamsEncoded.IsNotBlank())
            {
                foreach (var streamEncoded in adaptiveStreamsEncoded.Split(","))
                {
                    var streamInfoDic = UrlHelper.GetDictionaryFromUrlQuery(streamEncoded);

                    var itag = streamInfoDic.Get("itag").ParseInt();

#if RELEASE
                    // Skip unknown itags on RELEASE
                    if (!MediaStreamInfo.IsKnown(itag)) continue;
#endif

                    var url = streamInfoDic.Get("url");
                    var sig = streamInfoDic.GetOrDefault("s");
                    var contentLength = streamInfoDic.Get("clen").ParseLong();
                    var bitrate = streamInfoDic.Get("bitrate").ParseLong();

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
                        var stream = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreams.Add(stream);
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var size = streamInfoDic.Get("size");
                        var width = size.SubstringUntil("x").ParseInt();
                        var height = size.SubstringAfter("x").ParseInt();
                        var resolution = new VideoResolution(width, height);
                        var framerate = streamInfoDic.Get("fps").ParseDouble();

                        var stream = new VideoStreamInfo(itag, url, contentLength, bitrate, resolution, framerate);
                        videoStreams.Add(stream);
                    }
                }
            }

            // Parse adaptive streams from dash
            var dashManifestUrl = videoInfoDic.GetOrDefault("dashmpd");
            if (dashManifestUrl.IsNotBlank())
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
                response = await _httpService.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
                var dashManifestXml = XElement.Parse(response).StripNamespaces();
                var streamsXml = dashManifestXml.Descendants("Representation");

                // Filter out partial streams
                streamsXml = streamsXml
                    .Where(x => !(x.Descendant("Initialization")
                                      ?.Attribute("sourceURL")
                                      ?.Value.Contains("sq/") ?? false));

                // Parse streams
                foreach (var streamXml in streamsXml)
                {
                    var itag = (int) streamXml.AttributeStrict("id");

#if RELEASE
                    // Skip unknown itags on RELEASE
                    if (!MediaStreamInfo.IsKnown(itag)) continue;
#endif

                    var url = streamXml.ElementStrict("BaseURL").Value;
                    var bitrate = (long) streamXml.AttributeStrict("bandwidth");

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
                        var stream = new AudioStreamInfo(itag, url, contentLength, bitrate);
                        audioStreams.Add(stream);
                    }
                    // If video stream
                    else
                    {
                        // Parse additional data
                        var width = (int) streamXml.AttributeStrict("width");
                        var height = (int) streamXml.AttributeStrict("height");
                        var resolution = new VideoResolution(width, height);
                        var framerate = (double) streamXml.AttributeStrict("frameRate");

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
            var captionTracks = new List<ClosedCaptionTrackInfo>();
            var captionTracksEncoded = videoInfoDic.GetOrDefault("caption_tracks");
            if (captionTracksEncoded.IsNotBlank())
            {
                foreach (var captionEncoded in captionTracksEncoded.Split(","))
                {
                    var captionInfoDic = UrlHelper.GetDictionaryFromUrlQuery(captionEncoded);

                    var url = captionInfoDic.Get("u");

                    var code = captionInfoDic.Get("lc");
                    var name = captionInfoDic.Get("n");
                    var language = new Language(code, name);

                    var isAuto = captionInfoDic.Get("v").Contains("a.");

                    var captionTrack = new ClosedCaptionTrackInfo(url, language, isAuto);
                    captionTracks.Add(captionTrack);
                }
            }

            // Get metadata extension
            request = YoutubeHost + $"/get_video_metadata?video_id={videoId}";
            response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var videoInfoExtXml = XElement.Parse(response).StripNamespaces().ElementStrict("html_content");

            // Parse
            var description = videoInfoExtXml.ElementStrict("video_info").ElementStrict("description").Value;
            var likeCount = (long) videoInfoExtXml.ElementStrict("video_info").ElementStrict("likes_count_unformatted");
            var dislikeCount = (long) videoInfoExtXml.ElementStrict("video_info").ElementStrict("dislikes_count_unformatted");

            // Parse author info
            var authorId = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_external_id").Value;
            var authorName = videoInfoExtXml.ElementStrict("user_info").ElementStrict("username").Value;
            var authorTitle = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_title").Value;
            var authorIsPaid = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_paid").Value == "1";
            var authorLogoUrl = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_logo_url").Value;
            var authorBannerUrl = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_banner_url").Value;
            var author = new ChannelInfo(
                authorId, authorName, authorTitle,
                authorIsPaid, authorLogoUrl, authorBannerUrl);

            return new VideoInfo(
                videoId, title, author,
                duration, description, keywords, watermarks,
                viewCount, likeCount, dislikeCount,
                isListed, isRatingAllowed, isMuted, isEmbeddingAllowed,
                mixedStreams, audioStreams, videoStreams, captionTracks);
        }

        /// <summary>
        /// Gets playlist info by ID, truncating resulting video list at given number of pages (1 page ≤ 200 videos)
        /// </summary>
        public async Task<PlaylistInfo> GetPlaylistInfoAsync(string playlistId, int maxPages)
        {
            if (playlistId == null)
                throw new ArgumentNullException(nameof(playlistId));
            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException("Invalid Youtube playlist ID", nameof(playlistId));
            if (maxPages <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPages), "Needs to be a positive number");

            // Get all videos across pages
            var pagesDone = 0;
            var offset = 0;
            XElement playlistInfoXml;
            var videos = new List<VideoInfoSnippet>();
            var videoIds = new HashSet<string>();
            do
            {
                // Get
                var request = YoutubeHost + $"/list_ajax?style=xml&action_get_list=1&list={playlistId}&index={offset}";
                var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
                playlistInfoXml = XElement.Parse(response).StripNamespaces();

                // Parse videos
                var total = 0;
                var delta = 0;
                foreach (var videoInfoXml in playlistInfoXml.Elements("video"))
                {
                    // Basic info
                    var videoId = videoInfoXml.ElementStrict("encrypted_id").Value;
                    var videoTitle = videoInfoXml.ElementStrict("title").Value;
                    var videoDuration =
                        TimeSpan.FromSeconds(videoInfoXml.ElementStrict("length_seconds").Value.ParseDouble());
                    var videoDescription = videoInfoXml.ElementStrict("description").Value;
                    var videoViewCount =
                        Regex.Replace(videoInfoXml.ElementStrict("views").Value, @"\D", "").ParseLong();
                    var videoLikeCount =
                        Regex.Replace(videoInfoXml.ElementStrict("likes").Value, @"\D", "").ParseLong();
                    var videoDislikeCount =
                        Regex.Replace(videoInfoXml.ElementStrict("dislikes").Value, @"\D", "").ParseLong();

                    // Keywords
                    var videoKeywordsJoined = videoInfoXml.ElementStrict("keywords").Value;
                    var videoKeywords = Regex
                        .Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<quote>""?))([^""]|(""""))*?(?=\<quote>(?=\s|$))")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .Where(s => s.IsNotBlank());

                    var video = new VideoInfoSnippet(videoId, videoTitle, videoDuration, videoDescription,
                        videoKeywords, videoViewCount, videoLikeCount, videoDislikeCount);

                    // Add to list if not already there
                    if (videoIds.Add(video.Id))
                    {
                        videos.Add(video);
                        delta++;
                    }
                    total++;
                }

                // Break if the videos started repeating
                if (delta <= 0) break;

                // Prepare for next page
                pagesDone++;
                offset += total;
            } while (pagesDone < maxPages);

            // Parse metadata
            var title = playlistInfoXml.ElementStrict("title").Value;
            var author = playlistInfoXml.Element("author")?.Value ?? "";
            var description = playlistInfoXml.ElementStrict("description").Value;
            var viewCount = (long?) playlistInfoXml.Element("views") ?? 0;

            return new PlaylistInfo(playlistId, title, author, description, viewCount, videos);
        }

        /// <summary>
        /// Gets playlist info by ID
        /// </summary>
        public Task<PlaylistInfo> GetPlaylistInfoAsync(string playlistId)
            => GetPlaylistInfoAsync(playlistId, int.MaxValue);

        /// <summary>
        /// Gets videos uploaded to a channel with given ID, truncating resulting video list at given number of pages (1 page ≤ 200 videos)
        /// </summary>
        public async Task<IEnumerable<VideoInfoSnippet>> GetChannelUploadsAsync(string channelId, int maxPages)
        {
            if (channelId == null)
                throw new ArgumentNullException(nameof(channelId));
            if (!ValidateChannelId(channelId))
                throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));
            if (maxPages <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPages), "Needs to be a positive number");

            // Compose a playlist ID
            var playlistId = "UU" + channelId.SubstringAfter("UC");

            // Get playlist info
            var playlistInfo = await GetPlaylistInfoAsync(playlistId, maxPages).ConfigureAwait(false);

            return playlistInfo.Videos;
        }

        /// <summary>
        /// Gets videos uploaded to a channel with given ID
        /// </summary>
        public Task<IEnumerable<VideoInfoSnippet>> GetChannelUploadsAsync(string channelId)
            => GetChannelUploadsAsync(channelId, int.MaxValue);

        /// <summary>
        /// Gets channel info by ID
        /// </summary>
        public async Task<ChannelInfo> GetChannelInfoAsync(string channelId)
        {
            if (channelId == null)
                throw new ArgumentNullException(nameof(channelId));
            if (!ValidateChannelId(channelId))
                throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));

            // Get channel uploads
            var uploads = await GetChannelUploadsAsync(channelId, 1).ConfigureAwait(false);
            var videoInfoSnippet = uploads.FirstOrDefault();
            if (videoInfoSnippet == null)
                throw new ParseException("Cannot get channel info because it doesn't have any uploaded videos");

            // Get video info of the first video
            var videoInfo = await GetVideoInfoAsync(videoInfoSnippet.Id).ConfigureAwait(false);

            return videoInfo.Author;
        }

        /// <summary>
        /// Gets the actual media stream represented by given metadata
        /// </summary>
        public async Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // Get
            var stream = await _httpService.GetStreamAsync(info.Url).ConfigureAwait(false);

            return new MediaStream(info, stream);
        }

        /// <summary>
        /// Gets the actual closed caption track represented by given metadata
        /// </summary>
        public async Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // Get
            var response = await _httpService.GetStringAsync(info.Url).ConfigureAwait(false);
            var captionTrackXml = XElement.Parse(response).StripNamespaces();

            // Parse
            var captions = new List<ClosedCaption>();
            foreach (var captionXml in captionTrackXml.Descendants("text"))
            {
                var text = captionXml.Value;
                var offset = TimeSpan.FromSeconds((double) captionXml.AttributeStrict("start"));
                var duration = TimeSpan.FromSeconds((double) captionXml.AttributeStrict("dur"));

                var caption = new ClosedCaption(text, offset, duration);
                captions.Add(caption);
            }

            return new ClosedCaptionTrack(info, captions);
        }
    }

    // Platform-specific extensions
    public partial class YoutubeClient
    {
#if NET45 || NETCOREAPP1_0

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public async Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken, int bufferSize)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            // Save to file
            using (var input = await GetMediaStreamAsync(info).ConfigureAwait(false))
            using (var output = File.Create(filePath, bufferSize))
            {
                var buffer = new byte[bufferSize];
                int bytesRead;
                long totalBytesRead = 0;
                do
                {
                    // Read
                    totalBytesRead += bytesRead =
                        await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                    // Write
                    await output.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                    // Report progress
                    progress?.Report(1.0 * totalBytesRead / input.Length);
                } while (bytesRead > 0);
            }
        }

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath, IProgress<double> progress,
            CancellationToken cancellationToken)
            => DownloadMediaStreamAsync(info, filePath, progress, cancellationToken, 4096);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath, IProgress<double> progress)
            => DownloadMediaStreamAsync(info, filePath, progress, CancellationToken.None);

        /// <summary>
        /// Downloads a media stream to file
        /// </summary>
        public Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath)
            => DownloadMediaStreamAsync(info, filePath, null);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public async Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info,
            string filePath, IProgress<double> progress, CancellationToken cancellationToken, int bufferSize)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            // Get the track
            var track = await GetClosedCaptionTrackAsync(info).ConfigureAwait(false);

            // Save to file as SRT
            using (var output = File.Create(filePath, bufferSize))
            using (var sw = new StreamWriter(output, Encoding.Unicode, bufferSize))
            {
                for (int i = 0; i < track.Captions.Count; i++)
                {
                    // Make sure cancellation was not requested
                    cancellationToken.ThrowIfCancellationRequested();

                    var caption = track.Captions[i];
                    var buffer = new StringBuilder();

                    // Line number
                    buffer.AppendLine((i + 1).ToString());

                    // Time start --> time end
                    buffer.Append(caption.Offset.ToString(@"hh\:mm\:ss\,fff"));
                    buffer.Append(" --> ");
                    buffer.Append((caption.Offset + caption.Duration).ToString(@"hh\:mm\:ss\,fff"));
                    buffer.AppendLine();

                    // Actual text
                    buffer.AppendLine(caption.Text);

                    // Write to stream
                    await sw.WriteLineAsync(buffer.ToString()).ConfigureAwait(false);

                    // Report progress
                    progress?.Report((i + 1.0) / track.Captions.Count);
                }
            }
        }

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
            => DownloadClosedCaptionTrackAsync(info, filePath, progress, cancellationToken, 4096);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info, string filePath,
            IProgress<double> progress)
            => DownloadClosedCaptionTrackAsync(info, filePath, progress, CancellationToken.None);

        /// <summary>
        /// Downloads a closed caption track to file
        /// </summary>
        public Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info, string filePath)
            => DownloadClosedCaptionTrackAsync(info, filePath, null);

#endif
    }

    // Methods for validation and parsing of IDs
    public partial class YoutubeClient
    {
        private const string YoutubeHost = "https://www.youtube.com";

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
        /// Tries to parse video ID from a Youtube video URL
        /// </summary>
        public static bool TryParseVideoId(string videoUrl, out string videoId)
        {
            videoId = default(string);

            if (videoUrl.IsBlank())
                return false;

            // https://www.youtube.com/watch?v=yIVRs6YSbOM
            var regularMatch =
                Regex.Match(videoUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidateVideoId(regularMatch))
            {
                videoId = regularMatch;
                return true;
            }

            // https://youtu.be/yIVRs6YSbOM
            var shortMatch =
                Regex.Match(videoUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (shortMatch.IsNotBlank() && ValidateVideoId(shortMatch))
            {
                videoId = shortMatch;
                return true;
            }

            // https://www.youtube.com/embed/yIVRs6YSbOM
            var embedMatch =
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

            var success = TryParseVideoId(videoUrl, out string result);
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

            if (playlistId.Length != 2 &&
                playlistId.Length != 13 &&
                playlistId.Length != 18 &&
                playlistId.Length != 24 &&
                playlistId.Length != 26 &&
                playlistId.Length != 34)
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
            var regularMatch =
                Regex.Match(playlistUrl, @"youtube\..+?/playlist.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidatePlaylistId(regularMatch))
            {
                playlistId = regularMatch;
                return true;
            }

            // https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var compositeMatch =
                Regex.Match(playlistUrl, @"youtube\..+?/watch.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (compositeMatch.IsNotBlank() && ValidatePlaylistId(compositeMatch))
            {
                playlistId = compositeMatch;
                return true;
            }

            // https://youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var shortCompositeMatch =
                Regex.Match(playlistUrl, @"youtu\.be/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (shortCompositeMatch.IsNotBlank() && ValidatePlaylistId(shortCompositeMatch))
            {
                playlistId = shortCompositeMatch;
                return true;
            }

            // https://www.youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var embedCompositeMatch =
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

            var success = TryParsePlaylistId(playlistUrl, out string result);
            if (success)
                return result;

            throw new FormatException($"Could not parse playlist ID from given string [{playlistUrl}]");
        }

        /// <summary>
        /// Verifies that the given string is syntactically a valid Youtube channel ID
        /// </summary>
        public static bool ValidateChannelId(string channelId)
        {
            if (channelId.IsBlank())
                return false;

            if (channelId.Length != 24)
                return false;

            if (!channelId.StartsWith("UC", StringComparison.OrdinalIgnoreCase))
                return false;

            return !Regex.IsMatch(channelId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse channel ID from a Youtube channel URL
        /// </summary>
        public static bool TryParseChannelId(string channelUrl, out string channelId)
        {
            channelId = default(string);

            if (channelUrl.IsBlank())
                return false;

            // https://www.youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ
            var regularMatch =
                Regex.Match(channelUrl, @"youtube\..+?/channel/(.*?)(?:&|/|$)").Groups[1].Value;
            if (regularMatch.IsNotBlank() && ValidateChannelId(regularMatch))
            {
                channelId = regularMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses channel ID from a Youtube channel URL
        /// </summary>
        public static string ParseChannelId(string channelUrl)
        {
            if (channelUrl == null)
                throw new ArgumentNullException(nameof(channelUrl));

            var success = TryParseChannelId(channelUrl, out string result);
            if (success)
                return result;

            throw new FormatException($"Could not parse channel ID from given string [{channelUrl}]");
        }
    }
}
