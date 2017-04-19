using System;

namespace YoutubeExplode.Models.Streams
{
    /// <summary>
    /// Width and height of a video stream
    /// </summary>
    public struct VideoResolution : IEquatable<VideoResolution>
    {
        /// <summary>
        /// Empty resolution
        /// </summary>
        public static VideoResolution Empty { get; } = new VideoResolution();

        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; }

        internal VideoResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is VideoResolution)
            {
                var other = (VideoResolution) obj;
                return Equals(other);
            }
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
        {
            return $"{Width}x{Height}";
        }

        /// <inheritdoc />
        public static bool operator ==(VideoResolution r1, VideoResolution r2) => r1.Equals(r2);

        /// <inheritdoc />
        public static bool operator !=(VideoResolution r1, VideoResolution r2) => !(r1 == r2);
    }
}