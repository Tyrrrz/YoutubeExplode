namespace YoutubeExplode.Videos.Chapters
{
    /// <summary>
    /// YouTube video chapter.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// Chapter Title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Start of the chapter in milliseconds.
        /// </summary>
        public ulong TimeRangeStart { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Chapter"/>.
        /// </summary>
        public Chapter(
            string title,
            ulong timeRangeStart)
        {
            Title = title;
            TimeRangeStart = timeRangeStart;
        }
    }
}