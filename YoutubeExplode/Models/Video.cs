using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video
    /// </summary>
    public class Video
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Author channel
        /// </summary>
        public Channel Author { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Thumbnails
        /// </summary>
        public VideoThumbnails Thumbnails { get; }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Search keywords
        /// </summary>
        public IReadOnlyList<string> Keywords { get; }

        /// <summary>
        /// Status
        /// </summary>
        public VideoStatus Status { get; }

        /// <summary>
        /// Statistics
        /// </summary>
        public Statistics Statistics { get; }

        /// <summary>
        /// Muxed streams available for this video
        /// </summary>
        public IReadOnlyList<MuxedStreamInfo> MuxedStreamInfos { get; }

        /// <summary>
        /// Audio-only streams available for this video
        /// </summary>
        public IReadOnlyList<AudioStreamInfo> AudioStreamInfos { get; }

        /// <summary>
        /// Video-only streams available for this video
        /// </summary>
        public IReadOnlyList<VideoStreamInfo> VideoStreamInfos { get; }

        /// <summary>
        /// Closed caption tracks available for this video
        /// </summary>
        public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTrackInfos { get; }

        /// <summary />
        public Video(string id, Channel author, string title, string description, VideoThumbnails thumbnails,
            TimeSpan duration, IReadOnlyList<string> keywords, VideoStatus status, Statistics statistics,
            IReadOnlyList<MuxedStreamInfo> muxedStreamInfos, IReadOnlyList<AudioStreamInfo> audioStreamInfos,
            IReadOnlyList<VideoStreamInfo> videoStreamInfos, IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos)
        {
            Id = id.EnsureNotNull(nameof(id));
            Author = author.EnsureNotNull(nameof(author));
            Title = title.EnsureNotNull(nameof(title));
            Description = description.EnsureNotNull(nameof(description));
            Thumbnails = thumbnails.EnsureNotNull(nameof(thumbnails));
            Duration = duration.EnsureNotNegative(nameof(duration));
            Keywords = keywords.EnsureNotNull(nameof(keywords));
            Status = status.EnsureNotNull(nameof(status));
            Statistics = statistics.EnsureNotNull(nameof(statistics));
            MuxedStreamInfos = muxedStreamInfos.EnsureNotNull(nameof(muxedStreamInfos));
            AudioStreamInfos = audioStreamInfos.EnsureNotNull(nameof(audioStreamInfos));
            VideoStreamInfos = videoStreamInfos.EnsureNotNull(nameof(videoStreamInfos));
            ClosedCaptionTrackInfos = closedCaptionTrackInfos.EnsureNotNull(nameof(closedCaptionTrackInfos));
        }
    }
}