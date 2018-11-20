﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Parsers;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        private async Task<VideoEmbedPageParser> GetVideoEmbedPageParserAsync(string videoId)
        {
            var url = $"https://www.youtube.com/embed/{videoId}?disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return VideoEmbedPageParser.Initialize(raw);
        }

        private async Task<VideoWatchPageParser> GetVideoWatchPageParserAsync(string videoId)
        {
            var url = $"https://www.youtube.com/watch?v={videoId}&disable_polymer=true&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            return VideoWatchPageParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, string el)
        {
            // This parameter does magic and a lot of videos don't work without it
            var eurl = $"https://youtube.googleapis.com/v/{videoId}".UrlEncode();

            var url = $"https://www.youtube.com/get_video_info?video_id={videoId}&el={el}&eurl={eurl}&hl=en";
            var raw = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            
            return VideoInfoParser.Initialize(raw);
        }

        private async Task<VideoInfoParser> GetVideoInfoParserAsync(string videoId, bool ensurePlayability = false)
        {
            // Get parser with 'el=embedded'
            var parser = await GetVideoInfoParserAsync(videoId, "embedded").ConfigureAwait(false);

            // If the video is not available - throw exception
            if (!parser.ParseIsAvailable())
            {
                var errorReason = parser.ParseErrorReason();
                throw new VideoUnavailableException(videoId, $"Video [{videoId}] is unavailable. {errorReason}");
            }

            // If requested to ensure playability but the video is not playable - try again
            if (ensurePlayability && !parser.ParseIsPlayable())
            {
                // Retry with "el=detailpage"
                parser = await GetVideoInfoParserAsync(videoId, "detailpage").ConfigureAwait(false);

                // If the video is still not playable - throw exception
                if (!parser.ParseIsPlayable())
                {
                    var errorReason = parser.ParseErrorReason();
                    throw new VideoUnplayableException(videoId, $"Video [{videoId}] is unplayable. {errorReason}");
                }
            }

            return parser;
        }

        private async Task<DashManifestParser> GetDashManifestParserAsync(string dashManifestUrl)
        {
            var raw = await _httpClient.GetStringAsync(dashManifestUrl).ConfigureAwait(false);
            return DashManifestParser.Initialize(raw);
        }

        /// <inheritdoc />
        public async Task<Video> GetVideoAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info parser
            var videoInfoParser = await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var author = videoInfoParser.ParseAuthor();
            var title = videoInfoParser.ParseTitle();
            var duration = videoInfoParser.ParseDuration();
            var keywords = videoInfoParser.ParseKeywords();
            var viewCount = videoInfoParser.ParseViewCount();

            // Get video watch page parser
            var videoWatchPageParser = await GetVideoWatchPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var uploadDate = videoWatchPageParser.ParseUploadDate();
            var description = videoWatchPageParser.ParseDescription();
            var likeCount = videoWatchPageParser.ParseLikeCount();
            var dislikeCount = videoWatchPageParser.ParseDislikeCount();

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

            // Get video info parser just to assert that the video exists
            await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Get parser
            var parser = await GetVideoEmbedPageParserAsync(videoId).ConfigureAwait(false);

            // Extract info
            var id = parser.ParseChannelId();
            var title = parser.ParseChannelTitle();
            var logoUrl = parser.ParseChannelLogoUrl();

            return new Channel(id, title, logoUrl);
        }

        /// <inheritdoc />
        public async Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get parser
            var requestedAt = DateTimeOffset.Now;
            var parser = await GetVideoInfoParserAsync(videoId, true).ConfigureAwait(false);

            // Prepare stream info maps
            var muxedStreamInfoMap = new Dictionary<int, MuxedStreamInfo>();
            var audioStreamInfoMap = new Dictionary<int, AudioStreamInfo>();
            var videoStreamInfoMap = new Dictionary<int, VideoStreamInfo>();

            // Parse muxed stream infos
            foreach (var streamInfoParser in parser.GetMuxedStreamInfos())
            {
                // Extract info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();
                var contentLength = streamInfoParser.ParseContentLength();
                var bitrate = streamInfoParser.ParseBitrate();
                var format = streamInfoParser.ParseFormat();
                var audioEncoding = streamInfoParser.ParseAudioEncoding();
                var videoEncoding = streamInfoParser.ParseVideoEncoding();
                var videoQualityLabel = streamInfoParser.ParseQualityLabel();
                var videoQuality = VideoQualityConverter.FromVideoQualityLabel(videoQualityLabel);
                var width = streamInfoParser.ParseWidth();
                var height = streamInfoParser.ParseHeight();
                var resolution = new VideoResolution(width, height);
                var framerate = streamInfoParser.ParseFramerate();

                // TODO: refactor this
                // If content length is not set - get it ourselves
                if (contentLength <= 0)
                {
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    if (contentLength <= 0)
                        continue;
                }

                // If bitrate is not set - calculate it ourselves
                if (bitrate <= 0)
                {
                    var duration = streamInfoParser.ParseDuration();
                    bitrate = (long) (0.001 * contentLength / (duration.TotalMinutes * 0.0075));
                }

                muxedStreamInfoMap[itag] = new MuxedStreamInfo(url, contentLength, bitrate, format, audioEncoding,
                    videoEncoding, videoQualityLabel, videoQuality, resolution, framerate);
            }

            // Parse adaptive stream infos
            foreach (var streamInfoParser in parser.GetAdaptiveStreamInfos())
            {
                // Extract info
                var itag = streamInfoParser.ParseItag();
                var url = streamInfoParser.ParseUrl();
                var contentLength = streamInfoParser.ParseContentLength();
                var bitrate = streamInfoParser.ParseBitrate();
                var format = streamInfoParser.ParseFormat();

                // TODO: refactor this
                // If content length is not set - get it ourselves
                if (contentLength <= 0)
                {
                    contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? -1;

                    if (contentLength <= 0)
                        continue;
                }

                // If bitrate is not set - calculate it ourselves
                if (bitrate <= 0)
                {
                    var duration = streamInfoParser.ParseDuration();
                    bitrate = (long) (0.001 * contentLength / (duration.TotalMinutes * 0.0075));
                }

                // If audio-only
                if (streamInfoParser.ParseIsAudioOnly())
                {
                    // Extract audio-specific info
                    var audioEncoding = streamInfoParser.ParseAudioEncoding();

                    audioStreamInfoMap[itag] = new AudioStreamInfo(url, contentLength, bitrate, format, audioEncoding);
                }
                // If video-only
                else
                {
                    // Extract video-specific info
                    var videoEncoding = streamInfoParser.ParseVideoEncoding();
                    var videoQualityLabel = streamInfoParser.ParseQualityLabel();
                    var videoQuality = VideoQualityConverter.FromVideoQualityLabel(videoQualityLabel);
                    var width = streamInfoParser.ParseWidth();
                    var height = streamInfoParser.ParseHeight();
                    var resolution = new VideoResolution(width, height);
                    var framerate = streamInfoParser.ParseFramerate();

                    videoStreamInfoMap[itag] = new VideoStreamInfo(url, contentLength, bitrate, format, videoEncoding,
                        videoQualityLabel, videoQuality, resolution, framerate);
                }
            }

            // Parse dash manifest
            var dashManifestUrl = parser.ParseDashManifestUrl();
            if (dashManifestUrl.IsNotBlank())
            {
                // TODO: this is broken

                // Get the dash manifest parser
                var dashManifestParser = await GetDashManifestParserAsync(dashManifestUrl).ConfigureAwait(false);

                // Parse dash stream infos
                foreach (var dashStreamInfoParser in dashManifestParser.GetStreamInfos())
                {
                    // Extract info
                    var itag = dashStreamInfoParser.ParseItag();
                    var url = dashStreamInfoParser.ParseUrl();
                    var contentLength = dashStreamInfoParser.ParseContentLength();
                    var bitrate = dashStreamInfoParser.ParseBitrate();
                    var format = dashStreamInfoParser.ParseFormat();

                    // If audio-only
                    if (dashStreamInfoParser.ParseIsAudioOnly())
                    {
                        // Extract audio-specific info
                        var audioEncoding = dashStreamInfoParser.ParseAudioEncoding();

                        audioStreamInfoMap[itag] =
                            new AudioStreamInfo(url, contentLength, bitrate, format, audioEncoding);
                    }
                    // If video-only
                    else
                    {
                        // Extract video-specific info
                        var videoEncoding = dashStreamInfoParser.ParseVideoEncoding();
                        var width = dashStreamInfoParser.ParseWidth();
                        var height = dashStreamInfoParser.ParseHeight();
                        var resolution = new VideoResolution(width, height);
                        var framerate = dashStreamInfoParser.ParseFramerate();

                        videoStreamInfoMap[itag] = new VideoStreamInfo(url, contentLength, bitrate, format,
                            videoEncoding, "TODO", VideoQuality.High1080, resolution, framerate);
                    }
                }
            }

            // Finalize stream info collections
            var muxedStreamInfos = muxedStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();
            var audioStreamInfos = audioStreamInfoMap.Values.OrderByDescending(s => s.Bitrate).ToArray();
            var videoStreamInfos = videoStreamInfoMap.Values.OrderByDescending(s => s.VideoQuality).ToArray();

            // Get the raw HLS stream playlist (*.m3u8)
            var hlsPlaylistUrl = parser.ParseHlsPlaylistUrl();

            // Get date until which the streams are valid
            var lifeSpan = parser.ParseStreamInfoSetLifeSpan();
            var validUntil = requestedAt.Add(lifeSpan);

            return new MediaStreamInfoSet(muxedStreamInfos, audioStreamInfos, videoStreamInfos, hlsPlaylistUrl,
                validUntil);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId)
        {
            videoId.GuardNotNull(nameof(videoId));

            if (!ValidateVideoId(videoId))
                throw new ArgumentException($"Invalid YouTube video ID [{videoId}].", nameof(videoId));

            // Get video info parser
            var parser = await GetVideoInfoParserAsync(videoId).ConfigureAwait(false);

            // Parse closed caption track infos
            var closedCaptionTrackInfos = new List<ClosedCaptionTrackInfo>();
            foreach (var closedCaptionTrackInfoParser in parser.GetClosedCaptionTrackInfos())
            {
                // Extract info
                var url = closedCaptionTrackInfoParser.ParseUrl();
                var code = closedCaptionTrackInfoParser.ParseLanguageCode();
                var name = closedCaptionTrackInfoParser.ParseLanguageName();
                var isAutoGenerated = closedCaptionTrackInfoParser.ParseIsAutoGenerated();

                // Enforce format to the format we know how to parse
                url = UrlEx.SetQueryParameter(url, "format", "3");

                var language = new Language(code, name);
                var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url, language, isAutoGenerated);
                closedCaptionTrackInfos.Add(closedCaptionTrackInfo);
            }

            return closedCaptionTrackInfos;
        }
    }
}