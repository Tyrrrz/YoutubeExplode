using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Describes media streams available for a YouTube video.
/// </summary>
public class StreamManifest
{
    /// <summary>
    /// Available streams.
    /// </summary>
    public IReadOnlyList<IStreamInfo> Streams { get; }

    /// <summary>
    /// Initializes an instance of <see cref="StreamManifest" />.
    /// </summary>
    public StreamManifest(IReadOnlyList<IStreamInfo> streams)
    {
        Streams = streams;
    }

    /// <summary>
    /// Gets streams that contain audio (i.e. muxed and audio-only streams).
    /// </summary>
    public IEnumerable<IAudioStreamInfo> GetAudioStreams() =>
        Streams.OfType<IAudioStreamInfo>();

    /// <summary>
    /// Gets streams that contain video (i.e. muxed and video-only streams).
    /// </summary>
    public IEnumerable<IVideoStreamInfo> GetVideoStreams() =>
        Streams.OfType<IVideoStreamInfo>();

    /// <summary>
    /// Gets muxed streams (i.e. streams containing both audio and video).
    /// </summary>
    public IEnumerable<MuxedStreamInfo> GetMuxedStreams() =>
        Streams.OfType<MuxedStreamInfo>();

    /// <summary>
    /// Gets audio-only streams.
    /// </summary>
    public IEnumerable<AudioOnlyStreamInfo> GetAudioOnlyStreams() =>
        GetAudioStreams().OfType<AudioOnlyStreamInfo>();

    /// <summary>
    /// Gets video-only streams.
    /// </summary>
    public IEnumerable<VideoOnlyStreamInfo> GetVideoOnlyStreams() =>
        GetVideoStreams().OfType<VideoOnlyStreamInfo>();
}