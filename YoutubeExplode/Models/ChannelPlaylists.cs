using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Wrapper for playlists on a specific channel with continuation loading
    /// </summary>
    public class ChannelPlaylists
    {
        private readonly List<ChannelPlaylist> _playlists;

        /// <summary>
        /// Channel Id
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Playlists on the channel
        /// </summary>
        public IReadOnlyList<ChannelPlaylist> Playlists => _playlists.AsReadOnly();

        /// <summary>
        /// For bigger channels multiple loads are required.
        /// </summary>
        public string ContinuationToken { get; }

        /// <summary>
        /// Initialize the channel playlists class
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken"></param>
        /// <param name="playlists"></param>
        public ChannelPlaylists(string channelId, string continuationToken, List<ChannelPlaylist> playlists)
        {
            _playlists = new List<ChannelPlaylist>();
            Id = channelId;
            ContinuationToken = continuationToken;
            _playlists = playlists;
        }

        /// <summary>
        /// Add a playlist to the playlists section
        /// </summary>
        /// <param name="playlist"></param>
        internal void AddPlaylist(ChannelPlaylist playlist)
        {
            _playlists.Add(playlist);
        }
    }
}
