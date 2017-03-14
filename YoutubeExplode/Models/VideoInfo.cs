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
        /// ID of this video
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Title of this video
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// This video's author's name
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// Length of this video
        /// </summary>
        public TimeSpan Length { get; internal set; }

        /// <summary>
        /// View count of this video
        /// </summary>
        public long ViewCount { get; internal set; }

        /// <summary>
        /// Average user rating of this video.
        /// Ranges from 0 stars to 5 stars.
        /// </summary>
        public double AverageRating { get; internal set; }

        /// <summary>
        /// Normalized average user rating of this video.
        /// Ranges from 0 to 1.
        /// Also represents the "thumbs up" ratio in the new rating system.
        /// </summary>
        public double NormalizedAverageRating => AverageRating/5;

        /// <summary>
        /// This video's search keywords
        /// </summary>
        public IReadOnlyList<string> Keywords { get; internal set; }

        /// <summary>
        /// URL for the thumbnail image for this video
        /// </summary>
        public string ImageThumbnail => $"https://img.youtube.com/vi/{Id}/maxresdefault.jpg";

        /// <summary>
        /// URL for the default resolution image (not always available) for this video
        /// </summary>
        public string ImageStandardRes => $"https://img.youtube.com/vi/{Id}/sddefault.jpg";

        /// <summary>
        /// URL for the highest resolution image (not always available) for this video
        /// </summary>
        public string ImageMaxRes => $"https://img.youtube.com/vi/{Id}/maxresdefault.jpg";

        /// <summary>
        /// URL for the high resolution image for this video
        /// </summary>
        public string ImageHighRes => $"https://img.youtube.com/vi/{Id}/hqdefault.jpg";

        /// <summary>
        /// URL for the medium resolution image for this video
        /// </summary>
        public string ImageMediumRes => $"https://img.youtube.com/vi/{Id}/mqdefault.jpg";

        /// <summary>
        /// Collection of watermark URLs for this video
        /// </summary>
        public IReadOnlyList<string> Watermarks { get; internal set; }

        /// <summary>
        /// Whether this video is listed publicly
        /// </summary>
        public bool IsListed { get; internal set; }

        /// <summary>
        /// Whether it is allowed to leave user rating on this video
        /// </summary>
        public bool IsRatingAllowed { get; internal set; }

        /// <summary>
        /// Whether the audio has been muted on this video
        /// </summary>
        public bool IsMuted { get; internal set; }

        /// <summary>
        /// Whether it is allowed to embed this video on 3rd party sites
        /// </summary>
        public bool IsEmbeddingAllowed { get; internal set; }

        /// <summary>
        /// Whether this video has closed captions
        /// </summary>
        public bool HasClosedCaptions => ClosedCaptionTracks != null && ClosedCaptionTracks.Any();

        /// <summary>
        /// Metadata for this video's media streams
        /// </summary>
        public IReadOnlyList<MediaStreamInfo> Streams { get; internal set; }

        /// <summary>
        /// Metadata for this video's closed caption tracks
        /// </summary>
        public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTracks { get; internal set; }

        /// <summary>
        /// Dash manifest metadata for this video
        /// </summary>
        internal DashManifestInfo DashManifest { get; set; }

        /// <summary>
        /// Whether this video uses an encrypted signature for its streams that needs to be deciphered before the streams can be accessed
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
            return $"{Id}";
        }
    }
}