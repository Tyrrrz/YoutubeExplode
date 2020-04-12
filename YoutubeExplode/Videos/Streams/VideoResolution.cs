namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates video resolution.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly struct VideoResolution
    {
        /// <summary>
        /// Viewport width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Viewport height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoResolution"/>.
        /// </summary>
        public VideoResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Width}x{Height}";
    }
}