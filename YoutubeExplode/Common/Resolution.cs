using System;
using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Common;

/// <summary>
/// Resolution of an image or a video.
/// </summary>
public readonly partial struct Resolution
{
    /// <summary>
    /// Viewport width, measured in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Viewport height, measured in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Viewport area (i.e. width multiplied by height).
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// Initializes an instance of <see cref="Resolution" />.
    /// </summary>
    public Resolution(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Width}x{Height}";
}

public partial struct Resolution : IEquatable<Resolution>
{
    /// <inheritdoc />
    public bool Equals(Resolution other) => Width == other.Width && Height == other.Height;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Resolution other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Width, Height);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(Resolution left, Resolution right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(Resolution left, Resolution right) => !(left == right);
}