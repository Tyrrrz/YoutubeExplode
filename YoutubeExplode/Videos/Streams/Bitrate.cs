using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates bitrate.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct Bitrate : IComparable<Bitrate>
    {
        /// <summary>
        /// Bitrate as bytes per second.
        /// </summary>
        public double BytesPerSecond { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Bitrate"/>.
        /// </summary>
        /// <param name="bytesPerSecond"></param>
        public Bitrate(double bytesPerSecond) => BytesPerSecond = bytesPerSecond;

        /// <inheritdoc />
        public int CompareTo(Bitrate other) => BytesPerSecond.CompareTo(other.BytesPerSecond);
    }

    public partial struct Bitrate
    {
        /// <summary>
        /// Creates bitrate as bytes per second.
        /// </summary>
        public static Bitrate FromBytesPerSecond(double bytesPerSecond) => new Bitrate(bytesPerSecond);
    }
}