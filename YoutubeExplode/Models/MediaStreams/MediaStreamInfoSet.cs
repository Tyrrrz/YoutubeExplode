using System.Collections.Generic;
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
       /// Raw HTTP Live Streaming (HLS) url to the m3u8 playlist.
       /// Will be null if not a live stream.
       /// </summary>
       public string HlsLiveStreamUrl { get; }

        /// <summary />
        public MediaStreamInfoSet(IReadOnlyList<MuxedStreamInfo> muxed,
            IReadOnlyList<AudioStreamInfo> audio,
            IReadOnlyList<VideoStreamInfo> video, 
            string hlsLiveStreamUrl)
        {
            Muxed = muxed.GuardNotNull(nameof(muxed));
            Audio = audio.GuardNotNull(nameof(audio));
            Video = video.GuardNotNull(nameof(video));
            HlsLiveStreamUrl = hlsLiveStreamUrl;
        }
    }
}