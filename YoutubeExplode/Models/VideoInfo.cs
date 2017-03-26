using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video metadata
    /// </summary>
    public class VideoInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Author metadata
        /// </summary>
        public UserInfo Author { get; internal set; }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Search keywords
        /// </summary>
        public IReadOnlyList<string> Keywords { get; internal set; }

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
        public IReadOnlyList<string> Watermarks { get; internal set; }

        /// <summary>
        /// View count
        /// </summary>
        public long ViewCount { get; internal set; }

        /// <summary>
        /// Like count
        /// </summary>
        public long LikeCount { get; internal set; }

        /// <summary>
        /// Dislike count
        /// </summary>
        public long DislikeCount { get; internal set; }

        /// <summary>
        /// Average user rating in stars (0* to 5*)
        /// </summary>
        public double AverageRating => 5.0*LikeCount/(LikeCount + DislikeCount);

        /// <summary>
        /// Whether this video is publicly listed
        /// </summary>
        public bool IsListed { get; internal set; }

        /// <summary>
        /// Whether liking/disliking this video is allowed
        /// </summary>
        public bool IsRatingAllowed { get; internal set; }

        /// <summary>
        /// Whether the audio track has been muted
        /// </summary>
        public bool IsMuted { get; internal set; }

        /// <summary>
        /// Whether embedding this video on other websites is allowed
        /// </summary>
        public bool IsEmbeddingAllowed { get; internal set; }

        /// <summary>
        /// Collection of metadata for this video's media streams
        /// </summary>
        public IReadOnlyList<MediaStreamInfo> Streams { get; internal set; }

        /// <summary>
        /// Collection of metadata for this video's closed caption tracks
        /// </summary>
        public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTracks { get; internal set; }

        /// <summary>
        /// Dash manifest metadata for this video
        /// </summary>
        internal DashManifestInfo DashManifest { get; set; }

        /// <summary>
        /// Whether the signature needs to be deciphered
        /// </summary>
        internal bool NeedsDeciphering
        {
            get
            {
                return
                    (Streams != null && Streams.Any(s => s.NeedsDeciphering)) ||
                    (DashManifest != null && DashManifest.NeedsDeciphering);
            }
        }

        internal VideoInfo() { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Title}";
        }
    }
}