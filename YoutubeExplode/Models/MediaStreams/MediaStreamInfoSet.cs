using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    public class MediaStreamInfoSet
    {
        /// <summary>
        /// Muxed streams
        /// </summary>
        public IReadOnlyList<MuxedStreamInfo> Muxed { get; }

        /// <summary>
        /// Audio-only streams
        /// </summary>
        public IReadOnlyList<AudioStreamInfo> Audio { get; }

        /// <summary>
        /// Video-only streams
        /// </summary>
        public IReadOnlyList<VideoStreamInfo> Video { get; }

        public MediaStreamInfoSet(IReadOnlyList<MuxedStreamInfo> muxed,
            IReadOnlyList<AudioStreamInfo> audio,
            IReadOnlyList<VideoStreamInfo> video)
        {
            Muxed = muxed.GuardNotNull(nameof(muxed));
            Audio = audio.GuardNotNull(nameof(audio));
            Video = video.GuardNotNull(nameof(video));
        }

        public IEnumerable<MediaStreamInfo> EnumerateAll()
        {
            foreach (var streamInfo in Muxed)
                yield return streamInfo;
            foreach (var streamInfo in Audio)
                yield return streamInfo;
            foreach (var streamInfo in Video)
                yield return streamInfo;
        }
    }
}