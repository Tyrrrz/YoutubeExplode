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
        /// Video quality label, as seen on YouTube.
        /// </summary>
        string VideoQualityLabel { get; }

        /// <summary>
        /// Video quality.
        /// </summary>
        VideoQuality VideoQuality { get; }

        /// <summary>
        /// Video resolution.
        /// </summary>
        VideoResolution Resolution { get; }

        /// <summary>
        /// Video framerate.
        /// </summary>
        Framerate Framerate { get; }
    }

    /// <summary>
    /// Extensions for <see cref="IVideoStreamInfo"/>.
    /// </summary>
    public static class VideoStreamInfoExtensions
    {
        /// <summary>
        /// Gets all video qualities available in a collection of video streams.
        /// </summary>
        public static IEnumerable<VideoQuality> GetAllVideoQualities(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.Select(s => s.VideoQuality).ToHashSet();

        /// <summary>
        /// Gets video quality labels of all streams available in a collection of video streams.
        /// </summary>
        public static IEnumerable<string> GetAllVideoQualityLabels(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.Select(s => s.VideoQualityLabel).ToHashSet();

        /// <summary>
        /// Gets the video stream with highest video quality.
        /// Returns null if sequence is empty.
        /// </summary>
        public static IVideoStreamInfo? WithHighestVideoQuality(this IEnumerable<IVideoStreamInfo> streamInfos) =>
            streamInfos.OrderByDescending(s => s.VideoQuality).ThenByDescending(s => s.Framerate.FramesPerSecond).FirstOrDefault();
    }
}