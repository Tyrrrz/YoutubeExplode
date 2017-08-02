using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video info
    /// </summary>
    public class VideoInfo : VideoInfoSnippet
    {
        /// <summary>
        /// Author
        /// </summary>
        public ChannelInfo Author { get; }

        /// <summary>
        /// Collection of watermark URLs
        /// </summary>
        public IReadOnlyList<string> Watermarks { get; }

        /// <summary>
        /// Whether this video is publicly listed
        /// </summary>
        public bool IsListed { get; }

        /// <summary>
        /// Whether liking/disliking this video is allowed
        /// </summary>
        public bool IsRatingAllowed { get; }

        /// <summary>
        /// Whether the audio track has been muted
        /// </summary>
        public bool IsMuted { get; }

        /// <summary>
        /// Whether embedding this video on other websites is allowed
        /// </summary>
        public bool IsEmbeddingAllowed { get; }

        /// <summary>
        /// Mixed streams available for this video
        /// </summary>
        public IReadOnlyList<MixedStreamInfo> MixedStreams { get; }

        /// <summary>
        /// Audio-only streams available for this video
        /// </summary>
        public IReadOnlyList<AudioStreamInfo> AudioStreams { get; }

        /// <summary>
        /// Video-only streams available for this video
        /// </summary>
        public IReadOnlyList<VideoStreamInfo> VideoStreams { get; }

        /// <summary>
        /// Closed caption tracks available for this video
        /// </summary>
        public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTracks { get; }

        /// <inheritdoc />
        public VideoInfo(string id, string title, ChannelInfo author, TimeSpan duration, string description,
            IEnumerable<string> keywords, IEnumerable<string> watermarks, long viewCount, long likeCount,
            long dislikeCount, bool isListed, bool isRatingAllowed, bool isMuted, bool isEmbeddingAllowed,
            IEnumerable<MixedStreamInfo> mixedStreams, IEnumerable<AudioStreamInfo> audioStreams,
            IEnumerable<VideoStreamInfo> videoStreams, IEnumerable<ClosedCaptionTrackInfo> closedCaptionTracks)
            : base(id, title, description, keywords, viewCount, likeCount, dislikeCount, duration)
        {
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Watermarks = watermarks?.ToArray() ?? throw new ArgumentNullException(nameof(watermarks));
            IsListed = isListed;
            IsRatingAllowed = isRatingAllowed;
            IsMuted = isMuted;
            IsEmbeddingAllowed = isEmbeddingAllowed;
            MixedStreams = mixedStreams?.ToArray() ?? throw new ArgumentNullException(nameof(mixedStreams));
            AudioStreams = audioStreams?.ToArray() ?? throw new ArgumentNullException(nameof(audioStreams));
            VideoStreams = videoStreams?.ToArray() ?? throw new ArgumentNullException(nameof(videoStreams));
            ClosedCaptionTracks = closedCaptionTracks?.ToArray() ?? throw new ArgumentNullException(nameof(closedCaptionTracks));
        }
    }
}