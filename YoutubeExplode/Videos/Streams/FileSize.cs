using System;

namespace YoutubeExplode.Videos.Streams
{
    // Loosely based on https://github.com/omar/ByteSize (MIT license)

    /// <summary>
    /// Stream file size.
    /// </summary>
    public readonly partial struct FileSize
    {
        /// <summary>
        /// Total bytes.
        /// </summary>
        public long TotalBytes { get; }

        /// <summary>
        /// Total kilobytes.
        /// </summary>
        public double TotalKiloBytes => TotalBytes / 1024.0;

        /// <summary>
        /// Total megabytes.
        /// </summary>
        public double TotalMegaBytes => TotalKiloBytes / 1024.0;

        /// <summary>
        /// Total gigabytes.
        /// </summary>
        public double TotalGigaBytes => TotalMegaBytes / 1024.0;

        /// <summary>
        /// Initializes an instance of <see cref="FileSize"/>.
        /// </summary>
        public FileSize(long totalBytes) => TotalBytes = totalBytes;

        private string GetLargestWholeNumberSymbol()
        {
            if (Math.Abs(TotalGigaBytes) >= 1)
                return "GB";

            if (Math.Abs(TotalMegaBytes) >= 1)
                return "MB";

            if (Math.Abs(TotalKiloBytes) >= 1)
                return "KB";

            return "B";
        }

        private double GetLargestWholeNumberValue()
        {
            if (Math.Abs(TotalGigaBytes) >= 1)
                return TotalGigaBytes;

            if (Math.Abs(TotalMegaBytes) >= 1)
                return TotalMegaBytes;

            if (Math.Abs(TotalKiloBytes) >= 1)
                return TotalKiloBytes;

            return TotalBytes;
        }

        /// <inheritdoc />
        public override string ToString() => $"{GetLargestWholeNumberValue():0.##} {GetLargestWholeNumberSymbol()}";
    }

    public partial struct FileSize : IComparable<FileSize>, IEquatable<FileSize>
    {
        /// <inheritdoc />
        public int CompareTo(FileSize other) => TotalBytes.CompareTo(other.TotalBytes);

        /// <inheritdoc />
        public bool Equals(FileSize other) => CompareTo(other) == 0;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is FileSize other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(TotalBytes);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(FileSize left, FileSize right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(FileSize left, FileSize right) => !(left == right);

        /// <summary>
        /// Comparison.
        /// </summary>
        public static bool operator >(FileSize left, FileSize right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Comparison.
        /// </summary>
        public static bool operator <(FileSize left, FileSize right) => left.CompareTo(right) < 0;
    }
}