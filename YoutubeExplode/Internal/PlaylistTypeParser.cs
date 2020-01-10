using System;
using YoutubeExplode.Models;

namespace YoutubeExplode.Internal
{
    internal class PlaylistTypeParser
    {
        /// <summary>
        /// Get playlist type by ID.
        /// </summary>
        internal static PlaylistType GetPlaylistType(string id)
        {
            if (id.StartsWith("PL", StringComparison.Ordinal))
                return PlaylistType.Normal;

            if (id.StartsWith("RD", StringComparison.Ordinal))
                return PlaylistType.VideoMix;

            if (id.StartsWith("UL", StringComparison.Ordinal))
                return PlaylistType.ChannelVideoMix;

            if (id.StartsWith("UU", StringComparison.Ordinal))
                return PlaylistType.ChannelVideos;

            if (id.StartsWith("PU", StringComparison.Ordinal))
                return PlaylistType.PopularChannelVideos;

            if (id.StartsWith("OL", StringComparison.Ordinal))
                return PlaylistType.MusicAlbum;

            if (id.StartsWith("LL", StringComparison.Ordinal))
                return PlaylistType.LikedVideos;

            if (id.StartsWith("FL", StringComparison.Ordinal))
                return PlaylistType.Favorites;

            if (id.StartsWith("WL", StringComparison.Ordinal))
                return PlaylistType.WatchLater;

            throw new ArgumentOutOfRangeException(nameof(id), $"Unexpected playlist ID [{id}].");
        }
    }
}