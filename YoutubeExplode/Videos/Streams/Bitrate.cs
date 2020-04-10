using System;

namespace YoutubeExplode.Videos.Streams
{
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct Bitrate : IComparable<Bitrate>
    {
        public double BytesPerSecond { get; }

        public Bitrate(double bytesPerSecond)
        {
            BytesPerSecond = bytesPerSecond;
        }

        public int CompareTo(Bitrate other) => BytesPerSecond.CompareTo(other.BytesPerSecond);
    }

    public partial struct Bitrate
    {
        public static Bitrate FromBytesPerSecond(double bytesPerSecond) => new Bitrate(bytesPerSecond);
    }
}