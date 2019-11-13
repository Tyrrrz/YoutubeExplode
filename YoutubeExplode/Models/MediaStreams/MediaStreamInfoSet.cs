using System;
using System.Collections.Generic;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Set of all available media stream infos.
    /// </summary>
    public class MediaStreamInfoSet
    {
        /// <summary>
        /// Muxed streams.
        /// </summary>
        public IReadOnlyList<MuxedStreamInfo> Muxed { get; }

        /// <summary>
        /// Audio-only streams.
        /// </summary>
        public IReadOnlyList<AudioStreamInfo> Audio { get; }

        /// <summary>
        /// Video-only streams.
        /// </summary>
        public IReadOnlyList<VideoStreamInfo> Video { get; }

        /// <summary>
        /// Raw HTTP Live Streaming (HLS) URL to the m3u8 playlist.
        /// Null if not a live stream.
        /// </summary>
        public string? HlsLiveStreamUrl { get; }

        /// <summary>
        /// Expiry date for this information.
        /// </summary>
        public DateTimeOffset ValidUntil { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MediaStreamInfoSet"/>.
        /// </summary>
        public MediaStreamInfoSet(IReadOnlyList<MuxedStreamInfo> muxed,
            IReadOnlyList<AudioStreamInfo> audio,
            IReadOnlyList<VideoStreamInfo> video,
            string? hlsLiveStreamUrl, 
            DateTimeOffset validUntil)
        {
            Muxed = muxed;
            Audio = audio;
            Video = video;
            HlsLiveStreamUrl = hlsLiveStreamUrl;
            ValidUntil = validUntil;
        }
    }
}