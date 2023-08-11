using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a media stream of a YouTube video.
/// </summary>
public interface IStreamInfo
{
    /// <summary>
    /// Stream URL.
    /// </summary>
    /// <remarks>
    /// While this URL can be used to access the underlying stream, you need a series of carefully crafted
    /// HTTP requests to properly resolve it. It's recommended to use <see cref="StreamClient.GetAsync" />
    /// or <see cref="StreamClient.DownloadAsync"/> instead, as they do all the heavy lifting for you.
    /// </remarks>
    string Url { get; }

    /// <summary>
    /// Stream container.
    /// </summary>
    Container Container { get; }

    /// <summary>
    /// Stream size.
    /// </summary>
    FileSize Size { get; }

    /// <summary>
    /// Stream bitrate.
    /// </summary>
    Bitrate Bitrate { get; }
}

/// <summary>
/// Extensions for <see cref="IStreamInfo" />.
/// </summary>
public static class StreamInfoExtensions
{
    internal static bool IsThrottled(this IStreamInfo streamInfo) => !string.Equals(
        UrlEx.TryGetQueryParameterValue(streamInfo.Url, "ratebypass"),
        "yes",
        StringComparison.OrdinalIgnoreCase
    );

    /// <summary>
    /// Gets the stream with the highest bitrate.
    /// Returns null if the sequence is empty.
    /// </summary>
    public static IStreamInfo? TryGetWithHighestBitrate(this IEnumerable<IStreamInfo> streamInfos) =>
        streamInfos.MaxBy(s => s.Bitrate);

    /// <summary>
    /// Gets the stream with the highest bitrate.
    /// </summary>
    public static IStreamInfo GetWithHighestBitrate(this IEnumerable<IStreamInfo> streamInfos) =>
        streamInfos.TryGetWithHighestBitrate() ??
        throw new InvalidOperationException("Input stream collection is empty.");
}