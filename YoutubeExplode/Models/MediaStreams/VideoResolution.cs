using System;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Width and height of a video stream
    /// </summary>
    public partial struct VideoResolution : IEquatable<VideoResolution>
    {
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; }

        /// <inheritdoc />
        public VideoResolution(int width, int height)
        {
            Width = width >= 0 ? width : throw new ArgumentOutOfRangeException(nameof(width));
            Height = height >= 0 ? height : throw new ArgumentOutOfRangeException(nameof(height));
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is VideoResolution other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public bool Equals(VideoResolution other)
        {
            return Width == other.Width && Height == other.Height;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Width*397) ^ Height;
            }
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Width}x{Height}";
    }

    public partial struct VideoResolution
    {
        /// <inheritdoc />
        public static bool operator ==(VideoResolution r1, VideoResolution r2) => r1.Equals(r2);

        /// <inheritdoc />
        public static bool operator !=(VideoResolution r1, VideoResolution r2) => !(r1 == r2);
    }
}