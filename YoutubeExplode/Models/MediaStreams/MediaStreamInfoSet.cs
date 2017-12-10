using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Set of all available <see cref="MediaStreamInfo"/>s.
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

        /// <summary />
        public MediaStreamInfoSet(IReadOnlyList<MuxedStreamInfo> muxed,
            IReadOnlyList<AudioStreamInfo> audio,
            IReadOnlyList<VideoStreamInfo> video)
        {
            Muxed = muxed.GuardNotNull(nameof(muxed));
            Audio = audio.GuardNotNull(nameof(audio));
            Video = video.GuardNotNull(nameof(video));
        }
    }
}