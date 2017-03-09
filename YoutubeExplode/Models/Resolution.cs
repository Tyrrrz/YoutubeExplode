namespace YoutubeExplode.Models
{
    /// <summary>
    /// Width and height
    /// </summary>
    public struct Resolution
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
        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}