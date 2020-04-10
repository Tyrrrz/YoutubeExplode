using System;

namespace YoutubeExplode.Videos.Streams
{
    // Loosely based on https://github.com/omar/ByteSize (MIT license)

    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct FileSize : IComparable<FileSize>
    {
        public long TotalBytes { get; }

        public double TotalKiloBytes => TotalBytes / 1024.0;

        public double TotalMegaBytes => TotalKiloBytes / 1024.0;

        public double TotalGigaBytes => TotalMegaBytes / 1024.0;

        public FileSize(long totalBytes)
        {
            TotalBytes = totalBytes;
        }

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

        public int CompareTo(FileSize other) => TotalBytes.CompareTo(other.TotalBytes);
    }

    public partial struct FileSize
    {
        public static FileSize FromBytes(long bytes) => new FileSize(bytes);
    }
}