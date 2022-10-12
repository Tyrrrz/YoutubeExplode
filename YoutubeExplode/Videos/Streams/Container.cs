using System;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Stream container.
/// </summary>
public readonly partial struct Container
{
    /// <summary>
    /// Container name (e.g. mp4, webm, etc).
    /// Can be used as file extension.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether this container is a known audio-only container.
    /// </summary>
    /// <remarks>
    /// This property only refers to the container's capabilities and not its actual contents.
    /// If the container IS audio-only, it DOES NOT contain any video streams.
    /// If the container IS NOT audio-only, it MAY contain video streams, but is not required to.
    /// </remarks>
    public bool IsAudioOnly =>
        string.Equals(Name, "mp3", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Name, "m4a", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Name, "wav", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Name, "wma", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Name, "ogg", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Name, "aac", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Name, "opus", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes an instance of <see cref="Container" />.
    /// </summary>
    public Container(string name) => Name = name;

    /// <inheritdoc />
    public override string ToString() => Name;
}

public partial struct Container
{
    /// <summary>
    /// MPEG-2 Audio Layer III (mp3).
    /// </summary>
    public static Container Mp3 { get; } = new("mp3");

    /// <summary>
    /// MPEG-4 Part 14 (mp4).
    /// </summary>
    public static Container Mp4 { get; } = new("mp4");

    /// <summary>
    /// Web Media (webm).
    /// </summary>
    public static Container WebM { get; } = new("webm");

    /// <summary>
    /// 3rd Generation Partnership Project (3gpp).
    /// </summary>
    public static Container Tgpp { get; } = new("3gpp");
}

public partial struct Container : IEquatable<Container>
{
    /// <inheritdoc />
    public bool Equals(Container other) => StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Container other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(Container left, Container right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(Container left, Container right) => !(left == right);
}