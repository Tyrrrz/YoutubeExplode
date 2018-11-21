using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using YoutubeExplode.Internal;

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
        [NotNull, ItemNotNull]
        public IReadOnlyList<MuxedStreamInfo> Muxed { get; }

        /// <summary>
        /// Audio-only streams.
        /// </summary>
        [NotNull, ItemNotNull]
        public IReadOnlyList<AudioStreamInfo> Audio { get; }

        /// <summary>
        /// Video-only streams.
        /// </summary>
        [NotNull, ItemNotNull]
        public IReadOnlyList<VideoStreamInfo> Video { get; }

        /// <summary>
        /// Raw HTTP Live Streaming (HLS) URL to the m3u8 playlist.
        /// Null if not a live stream.
        /// </summary>
        [CanBeNull]
        public string HlsLiveStreamUrl { get; }

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
            string hlsLiveStreamUrl, 
            DateTimeOffset validUntil)
        {
            Muxed = muxed.GuardNotNull(nameof(muxed));
            Audio = audio.GuardNotNull(nameof(audio));
            Video = video.GuardNotNull(nameof(video));
            HlsLiveStreamUrl = hlsLiveStreamUrl;
            ValidUntil = validUntil;
        }
    }
}