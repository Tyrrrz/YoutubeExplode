using YoutubeExplode.ReverseEngineering;

namespace YoutubeExplode.Videos.Streams
{
    public enum Container
    {
        /// <summary>
        /// MPEG-4 Part 14 (.mp4).
        /// </summary>
        Mp4,

        /// <summary>
        /// Web Media (.webm).
        /// </summary>
        WebM,

        /// <summary>
        /// 3rd Generation Partnership Project (.3gpp).
        /// </summary>
        Tgpp
    }

    public static class ContainerExtensions
    {
        /// <summary>
        /// Gets file extension based on container type.
        /// </summary>
        public static string GetFileExtension(this Container container) => Heuristics.ContainerToFileExtension(container);
    }
}