using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// User activity statistics.
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// View count.
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// Like count.
        /// </summary>
        public long LikeCount { get; }

        /// <summary>
        /// Dislike count.
        /// </summary>
        public long DislikeCount { get; }

        /// <summary>
        /// Average user rating in stars (1 star to 5 stars).
        /// </summary>
        public double AverageRating
        {
            get
            {
                if (LikeCount + DislikeCount == 0) return 0;
                return 1 + 4.0 * LikeCount / (LikeCount + DislikeCount);
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="Statistics"/>.
        /// </summary>
        public Statistics(long viewCount, long likeCount, long dislikeCount)
        {
            ViewCount = viewCount.GuardNotNegative(nameof(viewCount));
            LikeCount = likeCount.GuardNotNegative(nameof(likeCount));
            DislikeCount = dislikeCount.GuardNotNegative(nameof(dislikeCount));
        }
    }
}