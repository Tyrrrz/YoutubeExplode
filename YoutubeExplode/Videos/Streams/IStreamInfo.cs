using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams
{
    public interface IStreamInfo
    {
        int Tag { get; }

        string Url { get; }

        Container Container { get; }

        FileSize Size { get; }

        Bitrate Bitrate { get; }
    }

    public static class StreamInfoExtensions
    {
        /// <summary>
        /// Gets the stream with highest bitrate.
        /// Returns null if sequence is empty.
        /// </summary>
        public static IStreamInfo WithHighestBitrate<T>(this IEnumerable<IStreamInfo> streamInfos) =>
            streamInfos.OrderByDescending(s => s.Bitrate.BytesPerSecond).FirstOrDefault();
    }
}