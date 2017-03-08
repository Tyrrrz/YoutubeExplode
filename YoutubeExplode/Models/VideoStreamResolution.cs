namespace YoutubeExplode.Models
{
    /// <summary>
    /// Width and Height of a video stream
    /// </summary>
    public struct VideoStreamResolution
    {
        /// <summary>
        /// Empty resolution
        /// </summary>
        public static VideoStreamResolution Empty { get; } = new VideoStreamResolution();

        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; }

        internal VideoStreamResolution(int width, int height)
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