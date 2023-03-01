using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Channels;

/// <summary>
/// Represents a syntactically valid YouTube user name.
/// </summary>
public readonly partial struct UserName
{
    /// <summary>
    /// Raw user name value.
    /// </summary>
    public string Value { get; }

    private UserName(string value) => Value = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct UserName
{
    private static bool IsValid(string userName) =>
        userName.Length <= 20 &&
        userName.All(char.IsLetterOrDigit);

    private static string? TryNormalize(string? userNameOrUrl)
    {
        if (string.IsNullOrWhiteSpace(userNameOrUrl))
            return null;

        // Name
        // TheTyrrr
        if (IsValid(userNameOrUrl))
            return userNameOrUrl;

        // URL
        // https://www.youtube.com/user/TheTyrrr
        var regularMatch = Regex
            .Match(userNameOrUrl, @"youtube\..+?/user/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value
            .Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(regularMatch) && IsValid(regularMatch))
            return regularMatch;

        // Invalid input
        return null;
    }

    /// <summary>
    /// Attempts to parse the specified string as a YouTube user name or profile URL.
    /// Returns null in case of failure.
    /// </summary>
    public static UserName? TryParse(string? userNameOrUrl) =>
        TryNormalize(userNameOrUrl)?.Pipe(name => new UserName(name));

    /// <summary>
    /// Parses the specified string as a YouTube user name or profile URL.
    /// </summary>
    public static UserName Parse(string userNameOrUrl) =>
        TryParse(userNameOrUrl) ??
        throw new ArgumentException($"Invalid YouTube user name or profile URL '{userNameOrUrl}'.");

    /// <summary>
    /// Converts string to user name.
    /// </summary>
    public static implicit operator UserName(string userNameOrUrl) => Parse(userNameOrUrl);

    /// <summary>
    /// Converts user name to string.
    /// </summary>
    public static implicit operator string(UserName userName) => userName.ToString();
}

public partial struct UserName : IEquatable<UserName>
{
    /// <inheritdoc />
    public bool Equals(UserName other) => StringComparer.Ordinal.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UserName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(UserName left, UserName right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(UserName left, UserName right) => !(left == right);
}