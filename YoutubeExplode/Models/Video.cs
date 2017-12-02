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

        /// <summary>
        /// Regular url of the YouTube watch page for this video
        /// </summary>
        public string RegularUrl {
            get {
                return Id != null ?
                    $"https://www.youtube.com/watch?v={Id}" :
                    null;
            }
        }

        /// <summary>
        /// Short url of the YouTube watch page for this video
        /// </summary>
        public string ShortUrl {
            get {
                return Id != null ?
                    $"https://youtu.be/{Id}" :
                    null;
            }
        }

        /// <summary>
        /// Url of the embedded YouTube watch page for this video
        /// </summary>
        public string EmbedUrl {
            get {
                return Id != null ?
                    $"https://www.youtube.com/embed/{Id}" :
                    null;
            }
        }

        /// <summary />
        public Video(string id, Channel author, string title, string description, VideoThumbnails thumbnails,
            TimeSpan duration, IReadOnlyList<string> keywords, VideoStatus status, Statistics statistics,
            IReadOnlyList<MuxedStreamInfo> muxedStreamInfos, IReadOnlyList<AudioStreamInfo> audioStreamInfos,
            IReadOnlyList<VideoStreamInfo> videoStreamInfos, IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos)
        {
            Id = id.GuardNotNull(nameof(id));
            Author = author.GuardNotNull(nameof(author));
            Title = title.GuardNotNull(nameof(title));
            Description = description.GuardNotNull(nameof(description));
            Thumbnails = thumbnails.GuardNotNull(nameof(thumbnails));
            Duration = duration.GuardNotNegative(nameof(duration));
            Keywords = keywords.GuardNotNull(nameof(keywords));
            Status = status.GuardNotNull(nameof(status));
            Statistics = statistics.GuardNotNull(nameof(statistics));
            MuxedStreamInfos = muxedStreamInfos.GuardNotNull(nameof(muxedStreamInfos));
            AudioStreamInfos = audioStreamInfos.GuardNotNull(nameof(audioStreamInfos));
            VideoStreamInfos = videoStreamInfos.GuardNotNull(nameof(videoStreamInfos));
            ClosedCaptionTrackInfos = closedCaptionTrackInfos.GuardNotNull(nameof(closedCaptionTrackInfos));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}