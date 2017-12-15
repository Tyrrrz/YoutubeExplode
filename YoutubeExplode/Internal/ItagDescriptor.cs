using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal
{
    internal class ItagDescriptor
    {
        public Container Container { get; }

        public AudioEncoding? AudioEncoding { get; }

        public VideoEncoding? VideoEncoding { get; }

        public VideoQuality? VideoQuality { get; }

        public ItagDescriptor(Container container,
            AudioEncoding? audioEncoding,
            VideoEncoding? videoEncoding,
            VideoQuality? videoQuality)
        {
            Container = container;
            AudioEncoding = audioEncoding;
            VideoEncoding = videoEncoding;
            VideoQuality = videoQuality;
        }
    }
}