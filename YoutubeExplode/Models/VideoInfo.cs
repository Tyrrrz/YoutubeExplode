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
    public class VideoInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Author metadata
        /// </summary>
        public UserInfo Author { get; }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Search keywords
        /// </summary>
        public IReadOnlyList<string> Keywords { get; }

        /// <summary>
        /// Thumbnail image URL
        /// </summary>
        public string ImageThumbnailUrl => $"https://img.youtube.com/vi/{Id}/default.jpg";

        /// <summary>
        /// Medium resolution image URL
        /// </summary>
        public string ImageMediumResUrl => $"https://img.youtube.com/vi/{Id}/mqdefault.jpg";

        /// <summary>
        /// High resolution image URL
        /// </summary>
        public string ImageHighResUrl => $"https://img.youtube.com/vi/{Id}/hqdefault.jpg";

        /// <summary>
        /// Standard resolution image URL.
        /// Not always available.
        /// </summary>
        public string ImageStandardResUrl => $"https://img.youtube.com/vi/{Id}/sddefault.jpg";

        /// <summary>
        /// Highest resolution image URL.
        /// Not always available.
        /// </summary>
        public string ImageMaxResUrl => $"https://img.youtube.com/vi/{Id}/maxresdefault.jpg";

        /// <summary>
        /// Collection of watermark URLs
        /// </summary>
        public IReadOnlyList<string> Watermarks { get; }

        /// <summary>
        /// View count
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// Like count
        /// </summary>
        public long LikeCount { get; }

        /// <summary>
        /// Dislike count
        /// </summary>
        public long DislikeCount { get; }

        /// <summary>
        /// Average user rating in stars (0* to 5*)
        /// </summary>
        public double AverageRating => 5.0*LikeCount/(LikeCount + DislikeCount);

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
        public VideoInfo(string id, string title, UserInfo author, TimeSpan duration, string description,
            IEnumerable<string> keywords, IEnumerable<string> watermarks, long viewCount, long likeCount,
            long dislikeCount, bool isListed, bool isRatingAllowed, bool isMuted, bool isEmbeddingAllowed,
            IEnumerable<MixedStreamInfo> mixedStreams, IEnumerable<AudioStreamInfo> audioStreams,
            IEnumerable<VideoStreamInfo> videoStreams, IEnumerable<ClosedCaptionTrackInfo> closedCaptionTracks)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Duration = duration >= TimeSpan.Zero ? duration : throw new ArgumentOutOfRangeException(nameof(duration));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Keywords = keywords?.ToArray() ?? throw new ArgumentNullException(nameof(keywords));
            Watermarks = watermarks?.ToArray() ?? throw new ArgumentNullException(nameof(watermarks));
            ViewCount = viewCount >= 0 ? viewCount : throw new ArgumentOutOfRangeException(nameof(viewCount));
            LikeCount = likeCount >= 0 ? likeCount : throw new ArgumentOutOfRangeException(nameof(likeCount));
            DislikeCount = dislikeCount >= 0 ? dislikeCount : throw new ArgumentOutOfRangeException(nameof(dislikeCount));
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