using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams
{
    public interface IVideoStreamInfo : IStreamInfo
    {
        string VideoCodec { get; }

        string VideoQualityLabel { get; }

        VideoQuality VideoQuality { get; }

        VideoResolution Resolution { get; }

        Framerate Framerate { get; }
    }

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