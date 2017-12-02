using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Model extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Generates the regular url of the YouTube watch page for this video
        /// </summary>
        public static string GetRegularUrl(this Video video)
        {
            video.GuardNotNull(nameof(video));
            return $"https://www.youtube.com/watch?v={video.Id}";
        }

        /// <summary>
        /// Generates the short url of the YouTube watch page for this video
        /// </summary>
        public static string GetShortUrl(this Video video)
        {
            video.GuardNotNull(nameof(video));
            return $"https://youtu.be/{video.Id}";
        }

        /// <summary>
        /// Generates the url of the embedded YouTube watch page for this video
        /// </summary>
        public static string GetEmbedUrl(this Video video)
        {
            video.GuardNotNull(nameof(video));
            return $"https://www.youtube.com/embed/{video.Id}";
        }

        /// <summary>
        /// Generates the regular url of the YouTube watch page for this video
        /// </summary>
        public static string GetRegularUrl(this PlaylistVideo playlistVideo)
        {
            playlistVideo.GuardNotNull(nameof(playlistVideo));
            return $"https://www.youtube.com/watch?v={playlistVideo.Id}";
        }

        /// <summary>
        /// Generates the short url of the YouTube watch page for this video
        /// </summary>
        public static string GetShortUrl(this PlaylistVideo playlistVideo)
        {
            playlistVideo.GuardNotNull(nameof(playlistVideo));
            return $"https://youtu.be/{playlistVideo.Id}";
        }

        /// <summary>
        /// Generates the url of the embedded YouTube watch page for this video
        /// </summary>
        public static string GetEmbedUrl(this PlaylistVideo playlistVideo)
        {
            playlistVideo.GuardNotNull(nameof(playlistVideo));
            return $"https://www.youtube.com/embed/{playlistVideo.Id}";
        }
    }
}