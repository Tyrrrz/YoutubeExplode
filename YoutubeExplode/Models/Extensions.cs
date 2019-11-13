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
        public static string GetUrl(this Video video) => $"https://www.youtube.com/watch?v={video.Id}";

        /// <summary>
        /// Gets the short URL of a video.
        /// </summary>
        public static string GetShortUrl(this Video video) => $"https://youtu.be/{video.Id}";

        /// <summary>
        /// Gets the embed URL of a video.
        /// </summary>
        public static string GetEmbedUrl(this Video video) => $"https://www.youtube.com/embed/{video.Id}";

        /// <summary>
        /// Gets the regular URL of a playlist.
        /// </summary>
        public static string GetUrl(this Playlist playlist) => $"https://www.youtube.com/playlist?list={playlist.Id}";

        /// <summary>
        /// Gets the watch URL of a playlist set to play the first video.
        /// </summary>
        public static string GetWatchUrl(this Playlist playlist)
        {
            var firstVideo = playlist.Videos.First();
            return $"https://www.youtube.com/watch?v={firstVideo.Id}&list={playlist.Id}";
        }

        /// <summary>
        /// Gets the short URL of a playlist set to play the first video.
        /// </summary>
        public static string GetShortUrl(this Playlist playlist)
        {
            var firstVideo = playlist.Videos.First();
            return $"https://www.youtu.be/{firstVideo.Id}/?list={playlist.Id}";
        }

        /// <summary>
        /// Gets the embed URL of a playlist set to play the first video.
        /// </summary>
        public static string GetEmbedUrl(this Playlist playlist)
        {
            var firstVideo = playlist.Videos.First();
            return $"https://www.youtube.com/embed/{firstVideo.Id}/?list={playlist.Id}";
        }

        /// <summary>
        /// Gets the URL of a channel.
        /// </summary>
        public static string GetUrl(this Channel channel) => $"https://www.youtube.com/channel/{channel.Id}";

        /// <summary>
        /// Gets ID of a playlist that consists of similar videos.
        /// </summary>
        public static string GetVideoMixPlaylistId(this Video video) => "RD" + video.Id;

        /// <summary>
        /// Gets ID of a playlist that consists of similar videos from the same channel.
        /// </summary>
        public static string GetChannelVideoMixPlaylistId(this Video video) => "UL" + video.Id;

        /// <summary>
        /// Gets ID of a playlist that consists of this channel's uploads.
        /// </summary>
        public static string GetChannelVideosPlaylistId(this Channel channel) => "UU" + channel.Id.SubstringAfter("UC");

        /// <summary>
        /// Gets ID of a playlist that consists of this channel's popular uploads.
        /// </summary>
        public static string GetPopularChannelVideosPlaylistId(this Channel channel) => "PU" + channel.Id.SubstringAfter("UC");

        /// <summary>
        /// Gets ID of a playlist that consists of this channel's liked videos.
        /// </summary>
        public static string GetLikedVideosPlaylistId(this Channel channel) => "LL" + channel.Id.SubstringAfter("UC");

        /// <summary>
        /// Gets ID of a playlist that consists of this channel's favorite videos.
        /// </summary>
        public static string GetFavoritesPlaylistId(this Channel channel) => "FL" + channel.Id.SubstringAfter("UC");
    }
}