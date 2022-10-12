using System;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Bitrate.
/// </summary>
public readonly partial struct Bitrate
{
    /// <summary>
    /// Bitrate in bits per second.
    /// </summary>
    public long BitsPerSecond { get; }

    /// <summary>
    /// Bitrate in kilobits per second.
    /// </summary>
    public double KiloBitsPerSecond => BitsPerSecond / 1024.0;

    /// <summary>
    /// Bitrate in megabits per second.
    /// </summary>
    public double MegaBitsPerSecond => KiloBitsPerSecond / 1024.0;

    /// <summary>
    /// Bitrate in gigabits per second
    /// </summary>
    public double GigaBitsPerSecond => MegaBitsPerSecond / 1024.0;

    /// <summary>
    /// Initializes an instance of <see cref="Bitrate" />.
    /// </summary>
    public Bitrate(long bitsPerSecond) => BitsPerSecond = bitsPerSecond;

    private string GetLargestWholeNumberSymbol()
    {
        if (Math.Abs(GigaBitsPerSecond) >= 1)
            return "Gbit/s";

        if (Math.Abs(MegaBitsPerSecond) >= 1)
            return "Mbit/s";

        if (Math.Abs(KiloBitsPerSecond) >= 1)
            return "Kbit/s";

        return "Bit/s";
    }

    private double GetLargestWholeNumberValue()
    {
        if (Math.Abs(GigaBitsPerSecond) >= 1)
            return GigaBitsPerSecond;

        if (Math.Abs(MegaBitsPerSecond) >= 1)
            return MegaBitsPerSecond;

        if (Math.Abs(KiloBitsPerSecond) >= 1)
            return KiloBitsPerSecond;

        return BitsPerSecond;
    }

    /// <inheritdoc />
    public override string ToString() => $"{GetLargestWholeNumberValue():0.##} {GetLargestWholeNumberSymbol()}";
}

public partial struct Bitrate : IComparable<Bitrate>, IEquatable<Bitrate>
{
    /// <inheritdoc />
    public int CompareTo(Bitrate other) => BitsPerSecond.CompareTo(other.BitsPerSecond);

    /// <inheritdoc />
    public bool Equals(Bitrate other) => CompareTo(other) == 0;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Bitrate other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(BitsPerSecond);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(Bitrate left, Bitrate right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(Bitrate left, Bitrate right) => !(left == right);

    /// <summary>
    /// Comparison.
    /// </summary>
    public static bool operator >(Bitrate left, Bitrate right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Comparison.
    /// </summary>
    public static bool operator <(Bitrate left, Bitrate right) => left.CompareTo(right) < 0;
}