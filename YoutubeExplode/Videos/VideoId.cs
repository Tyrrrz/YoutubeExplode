using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos;

/// <summary>
/// Represents a syntactically valid YouTube video ID.
/// </summary>
public readonly partial struct VideoId
{
    /// <summary>
    /// Raw ID value.
    /// </summary>
    public string Value { get; }

    private VideoId(string value) => Value = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct VideoId
{
    private static bool IsValid(string videoId) =>
        videoId.Length == 11 &&
        videoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    private static string? TryNormalize(string? videoIdOrUrl)
    {
        if (string.IsNullOrWhiteSpace(videoIdOrUrl))
            return null;

        // Id
        // yIVRs6YSbOM
        if (IsValid(videoIdOrUrl))
            return videoIdOrUrl;

        // Regular URL
        // https://www.youtube.com/watch?v=yIVRs6YSbOM
        var regularMatch = Regex
            .Match(videoIdOrUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)")
            .Groups[1]
            .Value
            .Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(regularMatch) && IsValid(regularMatch))
            return regularMatch;

        // Short URL
        // https://youtu.be/yIVRs6YSbOM
        var shortMatch = Regex
            .Match(videoIdOrUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value
            .Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(shortMatch) && IsValid(shortMatch))
            return shortMatch;

        // Embed URL
        // https://www.youtube.com/embed/yIVRs6YSbOM
        var embedMatch = Regex
            .Match(videoIdOrUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value
            .Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(embedMatch) && IsValid(embedMatch))
            return embedMatch;

        // Shorts URL
        // https://www.youtube.com/shorts/sKL1vjP0tIo
        var shortsMatch = Regex
            .Match(videoIdOrUrl, @"youtube\..+?/shorts/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value
            .Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(shortsMatch) && IsValid(shortsMatch))
            return shortsMatch;

        // Invalid input
        return null;
    }

    /// <summary>
    /// Attempts to parse the specified string as a video ID or URL.
    /// Returns null in case of failure.
    /// </summary>
    public static VideoId? TryParse(string? videoIdOrUrl) =>
        TryNormalize(videoIdOrUrl)?.Pipe(id => new VideoId(id));

    /// <summary>
    /// Parses the specified string as a YouTube video ID or URL.
    /// Throws an exception in case of failure.
    /// </summary>
    public static VideoId Parse(string videoIdOrUrl) =>
        TryParse(videoIdOrUrl) ??
        throw new ArgumentException($"Invalid YouTube video ID or URL '{videoIdOrUrl}'.");

    /// <summary>
    /// Converts string to ID.
    /// </summary>
    public static implicit operator VideoId(string videoIdOrUrl) => Parse(videoIdOrUrl);

    /// <summary>
    /// Converts ID to string.
    /// </summary>
    public static implicit operator string(VideoId videoId) => videoId.ToString();
}

public partial struct VideoId : IEquatable<VideoId>
{
    /// <inheritdoc />
    public bool Equals(VideoId other) => StringComparer.Ordinal.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VideoId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(VideoId left, VideoId right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(VideoId left, VideoId right) => !(left == right);
}