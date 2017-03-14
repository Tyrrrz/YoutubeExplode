using System;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Width and height
    /// </summary>
    public struct Resolution : IEquatable<Resolution>
    {
        /// <summary>
        /// Empty resolution
        /// </summary>
        public static Resolution Empty { get; } = new Resolution();

        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; }

        internal Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Resolution)
            {
                var other = (Resolution) obj;
                return Equals(other);
            }
            return false;
        }

        /// <inheritdoc />
        public bool Equals(Resolution other)
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
        public static bool operator ==(Resolution r1, Resolution r2) => r1.Equals(r2);

        /// <inheritdoc />
        public static bool operator !=(Resolution r1, Resolution r2) => !(r1 == r2);
    }
}