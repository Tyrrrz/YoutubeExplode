using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates bitrate.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly struct Bitrate : IComparable<Bitrate>
    {
        /// <summary>
        /// Bits per second.
        /// </summary>
        public long BitsPerSecond { get; }

        /// <summary>
        /// Kilobits per second.
        /// </summary>
        public double KiloBitsPerSecond => BitsPerSecond / 1024.0;

        /// <summary>
        /// Megabits per second.
        /// </summary>
        public double MegaBitsPerSecond => KiloBitsPerSecond / 1024.0;

        /// <summary>
        /// Gigabits per second
        /// </summary>
        public double GigaBitsPerSecond => MegaBitsPerSecond / 1024.0;

        /// <summary>
        /// Initializes an instance of <see cref="Bitrate"/>.
        /// </summary>
        /// <param name="bitsPerSecond"></param>
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
        public int CompareTo(Bitrate other) => BitsPerSecond.CompareTo(other.BitsPerSecond);

        /// <inheritdoc />
        public override string ToString() => $"{GetLargestWholeNumberValue():0.##} {GetLargestWholeNumberSymbol()}";
    }
}