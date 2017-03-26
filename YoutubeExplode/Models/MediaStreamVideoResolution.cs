using System;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Width and height of a video stream
    /// </summary>
    public struct MediaStreamVideoResolution : IEquatable<MediaStreamVideoResolution>
    {
        /// <summary>
        /// Empty resolution
        /// </summary>
        public static MediaStreamVideoResolution Empty { get; } = new MediaStreamVideoResolution();

        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; }

        internal MediaStreamVideoResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is MediaStreamVideoResolution)
            {
                var other = (MediaStreamVideoResolution) obj;
                return Equals(other);
            }
            return false;
        }

        /// <inheritdoc />
        public bool Equals(MediaStreamVideoResolution other)
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
        public static bool operator ==(MediaStreamVideoResolution r1, MediaStreamVideoResolution r2) => r1.Equals(r2);

        /// <inheritdoc />
        public static bool operator !=(MediaStreamVideoResolution r1, MediaStreamVideoResolution r2) => !(r1 == r2);
    }
}