using System;

namespace YoutubeExplode.Videos.Streams
{
    // Loosely based on https://github.com/omar/ByteSize (MIT license)

    /// <summary>
    /// Encapsulates file size.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct FileSize : IComparable<FileSize>
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

        /// <inheritdoc />
        public int CompareTo(FileSize other) => TotalBytes.CompareTo(other.TotalBytes);
    }

    public partial struct FileSize
    {
        /// <summary>
        /// Creates a file size from byte size.
        /// </summary>
        public static FileSize FromBytes(long bytes) => new FileSize(bytes);
    }
}