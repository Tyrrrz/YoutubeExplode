using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Search;

/// <summary>
/// Metadata associated with a YouTube video returned by a search query.
/// </summary>
public class VideoSearchResult(
    VideoId id,
    string title,
    Author author,
    TimeSpan? duration,
    long? viewCount,
    string? simpleUploadDate,
    IReadOnlyList<Thumbnail> thumbnails
) : ISearchResult, IVideo
{
    /// <inheritdoc />
    public VideoId Id { get; } = id;

    /// <inheritdoc cref="IVideo.Url" />
    public string Url => $"https://www.youtube.com/watch?v={Id}";

    /// <inheritdoc cref="IVideo.Title" />
    public string Title { get; } = title;

    /// <inheritdoc />
    public Author Author { get; } = author;

    /// <inheritdoc />
    public TimeSpan? Duration { get; } = duration;

    /// <summary>
    /// Video view count.
    /// </summary>
    /// <remarks>
    /// May be little bit inaccurate due to YouTube's view count caching.
    /// </remarks>
    public long ViewCount { get; } = viewCount;

    /// <summary>
    /// Video simplyfied upload date.
    /// </summary>
    /// <remarks>
    /// May be null if the video is planned premiere or live stream.
    /// </remarks>
    public string? SimpleUploadDate { get; } = simpleUploadDate;

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; } = thumbnails;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video ({Title})";
}
