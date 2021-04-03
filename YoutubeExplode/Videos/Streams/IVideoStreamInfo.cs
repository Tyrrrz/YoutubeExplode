using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// YouTube media stream that contains video.
    /// </summary>
    public interface IVideoStreamInfo : IStreamInfo
    {
        /// <summary>
        /// Video codec.
        /// </summary>
        string VideoCodec { get; }

        /// <summary>
        /// Video quality.
        /// </summary>
        VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resolution.
        /// </summary>
        VideoResolution VideoResolution { get; }
    }

    /// <summary>
    /// Extensions for <see cref="IVideoStreamInfo"/>.
    /// </summary>
    public static class VideoStreamInfoExtensions
    {
        /// <summary>
        /// Gets the video stream with the highest video quality.
        /// Returns null if the sequence is empty.
        /// </summary>
        public static IVideoStreamInfo? WithHighestVideoQuality(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.OrderByDescending(s => s.VideoQuality).FirstOrDefault();
    }
}