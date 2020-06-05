namespace YoutubeExplode.Common
{
    /// <summary>
    /// Engagement statistics.
    /// </summary>
    public class Engagement
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
        /// Average rating.
        /// </summary>
        public double AverageRating => LikeCount + DislikeCount != 0
            ? 1 + 4.0 * LikeCount / (LikeCount + DislikeCount)
            : 0; // avoid division by 0

        /// <summary>
        /// Initializes an instance of <see cref="Engagement"/>.
        /// </summary>
        public Engagement(long viewCount, long likeCount, long dislikeCount)
        {
            ViewCount = viewCount;
            LikeCount = likeCount;
            DislikeCount = dislikeCount;
        }

        /// <inheritdoc />
        public override string ToString() => $"{ViewCount:N0} views | {LikeCount:N0} likes | {DislikeCount:N0} dislikes";
    }
}