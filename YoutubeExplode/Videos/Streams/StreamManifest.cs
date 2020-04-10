using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams
{
    public class StreamManifest
    {
        public IReadOnlyList<IStreamInfo> Streams { get; }

        public StreamManifest(IReadOnlyList<IStreamInfo> streams)
        {
            Streams = streams;
        }

        public IEnumerable<IAudioStreamInfo> GetAudio() => Streams.OfType<IAudioStreamInfo>();

        public IEnumerable<IVideoStreamInfo> GetVideo() => Streams.OfType<IVideoStreamInfo>();

        public IEnumerable<MuxedStreamInfo> GetMuxed() => Streams.OfType<MuxedStreamInfo>();

        public IEnumerable<AudioOnlyStreamInfo> GetAudioOnly() => GetAudio().OfType<AudioOnlyStreamInfo>();

        public IEnumerable<VideoOnlyStreamInfo> GetVideoOnly() => GetVideo().OfType<VideoOnlyStreamInfo>();
    }
}