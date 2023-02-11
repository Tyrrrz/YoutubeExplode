using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos;

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
    /// <remarks>
    /// YouTube no longer shows dislikes, so this value is always 0.
    /// </remarks>
    public long DislikeCount { get; }

    /// <summary>
    /// Average rating.
    /// </summary>
    /// <remarks>
    /// YouTube no longer shows dislikes, so this value is always 5.
    /// </remarks>
    public double AverageRating => LikeCount + DislikeCount != 0
        ? 1 + 4.0 * LikeCount / (LikeCount + DislikeCount)
        : 0; // avoid division by 0

    /// <summary>
    /// Initializes an instance of <see cref="Engagement" />.
    /// </summary>
    public Engagement(long viewCount, long likeCount, long dislikeCount)
    {
        ViewCount = viewCount;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Rating: {AverageRating:N1}";
}