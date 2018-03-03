using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0
using System;
using System.Threading;
#endif

namespace YoutubeExplode
{
    /// <summary>
    /// Interface for <see cref="YoutubeClient"/>.
    /// </summary>
    public interface IYoutubeClient
    {
        #region Video

        /// <summary>
        /// Gets video information by ID.
        /// </summary>
        Task<Video> GetVideoAsync(string videoId);

        /// <summary>
        /// Gets author channel information for given video.
        /// </summary>
        Task<Channel> GetVideoAuthorChannelAsync(string videoId);

        /// <summary>
        /// Gets a set of all available media stream infos for given video.
        /// </summary>
        Task<MediaStreamInfoSet> GetVideoMediaStreamInfosAsync(string videoId);

        /// <summary>
        /// Gets all available closed caption track infos for given video.
        /// </summary>
        Task<IReadOnlyList<ClosedCaptionTrackInfo>> GetVideoClosedCaptionTrackInfosAsync(string videoId);

        #endregion

        #region Playlist

        /// <summary>
        /// Gets playlist information by ID.
        /// The video list is truncated at given number of pages (1 page ≤ 200 videos).
        /// </summary>
        Task<Playlist> GetPlaylistAsync(string playlistId, int maxPages);

        /// <summary>
        /// Gets playlist information by ID.
        /// </summary>
        Task<Playlist> GetPlaylistAsync(string playlistId);

        #endregion

        #region Search

        /// <summary>
        /// Searches videos using given query.
        /// The video list is truncated at given number of pages (1 page ≤ 20 videos).
        /// </summary>
        Task<IReadOnlyList<Video>> SearchVideosAsync(string query, int maxPages);

        /// <summary>
        /// Searches videos using given query.
        /// </summary>
        Task<IReadOnlyList<Video>> SearchVideosAsync(string query);

        #endregion

        #region Channel

        /// <summary>
        /// Gets channel information by ID.
        /// </summary>
        Task<Channel> GetChannelAsync(string channelId);

        /// <summary>
        /// Gets videos uploaded by channel with given ID.
        /// The video list is truncated at given number of pages (1 page ≤ 200 videos).
        /// </summary>
        Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId, int maxPages);

        /// <summary>
        /// Gets videos uploaded by channel with given ID.
        /// </summary>
        Task<IReadOnlyList<Video>> GetChannelUploadsAsync(string channelId);

        #endregion

        #region MediaStream

        /// <summary>
        /// Gets the actual media stream associated with given metadata.
        /// </summary>
        Task<MediaStream> GetMediaStreamAsync(MediaStreamInfo info);

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0

        /// <summary>
        /// Gets the actual media stream associated with given metadata
        /// and downloads it to a file.
        /// </summary>
        Task DownloadMediaStreamAsync(MediaStreamInfo info, string filePath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken));

#endif

        #endregion

        #region ClosedCaptionTrack

        /// <summary>
        /// Gets the actual closed caption track associated with given metadata.
        /// </summary>
        Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo info);

#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0

        /// <summary>
        /// Gets the actual closed caption track associated with given metadata
        /// and downloads it as SRT file.
        /// </summary>
        Task DownloadClosedCaptionTrackAsync(ClosedCaptionTrackInfo info, string filePath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken));

#endif

        #endregion
    }
}