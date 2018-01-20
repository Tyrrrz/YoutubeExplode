using System.Linq;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Extensions for <see cref="Models"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the regular URL of a video.
        /// </summary>
        public static string GetUrl(this Video video)
        {
            video.GuardNotNull(nameof(video));
            return $"https://www.youtube.com/watch?v={video.Id}";
        }

        /// <summary>
        /// Gets the short URL of a video.
        /// </summary>
        public static string GetShortUrl(this Video video)
        {
            video.GuardNotNull(nameof(video));
            return $"https://youtu.be/{video.Id}";
        }

        /// <summary>
        /// Gets the embed URL of a video.
        /// </summary>
        public static string GetEmbedUrl(this Video video)
        {
            video.GuardNotNull(nameof(video));
            return $"https://www.youtube.com/embed/{video.Id}";
        }

        /// <summary>
        /// Gets the regular URL of a playlist.
        /// </summary>
        public static string GetUrl(this Playlist playlist)
        {
            playlist.GuardNotNull(nameof(playlist));
            return $"https://www.youtube.com/playlist?list={playlist.Id}";
        }

        /// <summary>
        /// Gets the watch URL of a playlist set to play the first video.
        /// </summary>
        public static string GetWatchUrl(this Playlist playlist)
        {
            playlist.GuardNotNull(nameof(playlist));
            var firstVideo = playlist.Videos.First();
            return $"https://www.youtube.com/watch?v={firstVideo.Id}&list={playlist.Id}";
        }

        /// <summary>
        /// Gets the short URL of a playlist set to play the first video.
        /// </summary>
        public static string GetShortUrl(this Playlist playlist)
        {
            playlist.GuardNotNull(nameof(playlist));
            var firstVideo = playlist.Videos.First();
            return $"https://www.youtu.be/{firstVideo.Id}/?list={playlist.Id}";
        }

        /// <summary>
        /// Gets the embed URL of a playlist set to play the first video.
        /// </summary>
        public static string GetEmbedUrl(this Playlist playlist)
        {
            playlist.GuardNotNull(nameof(playlist));
            var firstVideo = playlist.Videos.First();
            return $"https://www.youtube.com/embed/{firstVideo.Id}/?list={playlist.Id}";
        }

        /// <summary>
        /// Gets the URL of a channel.
        /// </summary>
        public static string GetUrl(this Channel channel)
        {
            channel.GuardNotNull(nameof(channel));
            return $"https://www.youtube.com/channel/{channel.Id}";
        }
    }
}