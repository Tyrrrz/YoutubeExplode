using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Partial video info
    /// </summary>
    public class VideoInfoSnippet
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
        /// Average user rating in stars (1* to 5*)
        /// </summary>
        public double AverageRating => 1 + 4.0 * LikeCount / (LikeCount + DislikeCount);

        /// <inheritdoc />
        public VideoInfoSnippet(string id, string title, string description, IEnumerable<string> keywords,
            long viewCount, long likeCount, long dislikeCount)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Keywords = keywords?.ToArray() ?? throw new ArgumentNullException(nameof(keywords));
            ViewCount = viewCount >= 0 ? viewCount : throw new ArgumentOutOfRangeException(nameof(viewCount));
            LikeCount = likeCount >= 0 ? likeCount : throw new ArgumentOutOfRangeException(nameof(likeCount));
            DislikeCount = dislikeCount >= 0 ? dislikeCount : throw new ArgumentOutOfRangeException(nameof(dislikeCount));
        }
    }
}