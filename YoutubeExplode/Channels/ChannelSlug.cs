using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Channels;

/// <summary>
/// Represents a syntactically valid YouTube channel slug.
/// </summary>
public readonly partial struct ChannelSlug
{
    /// <summary>
    /// Raw slug value.
    /// </summary>
    public string Value { get; }

    private ChannelSlug(string value) => Value = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public readonly partial struct ChannelSlug
{
    private static bool IsValid(string channelSlug) =>
        channelSlug.All(char.IsLetterOrDigit);

    private static string? TryNormalize(string? channelSlugOrUrl)
    {
        if (string.IsNullOrWhiteSpace(channelSlugOrUrl))
            return null;

        // Slug
        // Tyrrrz
        if (IsValid(channelSlugOrUrl))
            return channelSlugOrUrl;

        // URL
        // https://www.youtube.com/c/Tyrrrz
        var regularMatch = Regex
            .Match(channelSlugOrUrl, @"youtube\..+?/c/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value
            .Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(regularMatch) && IsValid(regularMatch))
            return regularMatch;

        // Invalid input
        return null;
    }

    /// <summary>
    /// Attempts to parse the specified string as a YouTube channel slug or legacy custom URL.
    /// Returns null in case of failure.
    /// </summary>
    public static ChannelSlug? TryParse(string? channelSlugOrUrl) =>
        TryNormalize(channelSlugOrUrl)?.Pipe(slug => new ChannelSlug(slug));

    /// <summary>
    /// Parses the specified string as a YouTube channel slug or legacy custom url.
    /// </summary>
    public static ChannelSlug Parse(string channelSlugOrUrl) =>
        TryParse(channelSlugOrUrl) ??
        throw new ArgumentException($"Invalid YouTube channel slug or legacy custom URL '{channelSlugOrUrl}'.");

    /// <summary>
    /// Converts string to channel slug.
    /// </summary>
    public static implicit operator ChannelSlug(string channelSlugOrUrl) => Parse(channelSlugOrUrl);

    /// <summary>
    /// Converts channel slug to string.
    /// </summary>
    public static implicit operator string(ChannelSlug channelSlug) => channelSlug.ToString();
}