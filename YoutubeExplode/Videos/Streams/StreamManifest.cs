using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Manifest that contains information about available media streams in a specific video.
    /// </summary>
    public class StreamManifest
    {
        /// <summary>
        /// Available streams.
        /// </summary>
        public IReadOnlyList<IStreamInfo> Streams { get; }

        /// <summary>
        /// Initializes an instance of <see cref="StreamManifest"/>.
        /// </summary>
        public StreamManifest(IReadOnlyList<IStreamInfo> streams)
        {
            Streams = streams;
        }

        /// <summary>
        /// Gets streams that contain audio (which includes muxed and audio-only streams).
        /// </summary>
        public IEnumerable<IAudioStreamInfo> GetAudio() => Streams.OfType<IAudioStreamInfo>();

        /// <summary>
        /// Gets streams that contain video (which includes muxed and video-only streams).
        /// </summary>
        public IEnumerable<IVideoStreamInfo> GetVideo() => Streams.OfType<IVideoStreamInfo>();

        /// <summary>
        /// Gets muxed streams (contain both audio and video).
        /// Note that muxed streams are limited in quality and don't go beyond 720p30.
        /// </summary>
        public IEnumerable<MuxedStreamInfo> GetMuxed() => Streams.OfType<MuxedStreamInfo>();

        /// <summary>
        /// Gets audio-only streams (no video).
        /// </summary>
        public IEnumerable<AudioOnlyStreamInfo> GetAudioOnly() => GetAudio().OfType<AudioOnlyStreamInfo>();

        /// <summary>
        /// Gets video-only streams (no audio).
        /// These streams have the widest range of qualities available.
        /// </summary>
        public IEnumerable<VideoOnlyStreamInfo> GetVideoOnly() => GetVideo().OfType<VideoOnlyStreamInfo>();
    }
}