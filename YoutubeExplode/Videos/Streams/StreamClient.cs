using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Operations related to media streams of YouTube videos.
/// </summary>
public class StreamClient
{
    private readonly HttpClient _http;
    private readonly StreamController _controller;

    /// <summary>
    /// Initializes an instance of <see cref="StreamClient" />.
    /// </summary>
    public StreamClient(HttpClient http)
    {
        _http = http;
        _controller = new StreamController(http);
    }

    private async IAsyncEnumerable<IStreamInfo> GetStreamInfosAsync(
        IEnumerable<IStreamInfoExtractor> streamInfoExtractors,
        CipherManifest cipherManifest,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var streamInfoExtractor in streamInfoExtractors)
        {
            var itag =
                streamInfoExtractor.TryGetItag() ??
                throw new YoutubeExplodeException("Could not extract stream itag.");

            var url =
                streamInfoExtractor.TryGetUrl() ??
                throw new YoutubeExplodeException("Could not extract stream URL.");

            var signature = streamInfoExtractor.TryGetSignature();
            if (!string.IsNullOrWhiteSpace(signature))
            {
                url = Url.SetQueryParameter(
                    url,
                    streamInfoExtractor.TryGetSignatureParameter() ?? "sig",
                    cipherManifest.Decipher(signature)
                );
            }

            // Get content length
            var contentLength =
                streamInfoExtractor.TryGetContentLength() ??
                await _http.TryGetContentLengthAsync(url, true, cancellationToken) ??
                0;

            if (contentLength <= 0)
                throw new YoutubeExplodeException("Could not extract stream content length.");

            var fileSize = new FileSize(contentLength);

            var container =
                streamInfoExtractor.TryGetContainer()?.Pipe(s => new Container(s)) ??
                throw new YoutubeExplodeException("Could not extract stream container.");

            var bitrate =
                streamInfoExtractor.TryGetBitrate()?.Pipe(s => new Bitrate(s)) ??
                throw new YoutubeExplodeException("Could not extract stream bitrate.");

            var audioCodec = streamInfoExtractor.TryGetAudioCodec();
            var videoCodec = streamInfoExtractor.TryGetVideoCodec();

            // Muxed or video-only stream
            if (!string.IsNullOrWhiteSpace(videoCodec))
            {
                var framerate = streamInfoExtractor.TryGetFramerate() ?? 24;
                var videoQualityLabel = streamInfoExtractor.TryGetVideoQualityLabel();

                var videoQuality = !string.IsNullOrWhiteSpace(videoQualityLabel)
                    ? VideoQuality.FromLabel(videoQualityLabel, framerate)
                    : VideoQuality.FromItag(itag, framerate);

                var videoWidth = streamInfoExtractor.TryGetVideoWidth();
                var videoHeight = streamInfoExtractor.TryGetVideoHeight();

                var videoResolution = videoWidth is not null && videoHeight is not null
                    ? new Resolution(videoWidth.Value, videoHeight.Value)
                    : videoQuality.GetDefaultVideoResolution();

                // Muxed
                if (!string.IsNullOrWhiteSpace(audioCodec))
                {
                    var streamInfo = new MuxedStreamInfo(
                        url,
                        container,
                        fileSize,
                        bitrate,
                        audioCodec,
                        videoCodec,
                        videoQuality,
                        videoResolution
                    );

                    yield return streamInfo;
                }
                // Video-only
                else
                {
                    var streamInfo = new VideoOnlyStreamInfo(
                        url,
                        container,
                        fileSize,
                        bitrate,
                        videoCodec,
                        videoQuality,
                        videoResolution
                    );

                    yield return streamInfo;
                }
            }
            // Audio-only
            else if (!string.IsNullOrWhiteSpace(audioCodec))
            {
                var streamInfo = new AudioOnlyStreamInfo(
                    url,
                    container,
                    fileSize,
                    bitrate,
                    audioCodec
                );

                yield return streamInfo;
            }
            else
            {
                throw new YoutubeExplodeException("Could not extract stream codec.");
            }
        }
    }

    private async IAsyncEnumerable<IStreamInfo> GetStreamInfosAsync(
        VideoId videoId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var cipherManifest = CipherManifest.Null;
        var playerResponse = await _controller.GetPlayerResponseAsync(videoId, cancellationToken);

        // Check if the video is pay-to-play
        var previewVideoId = playerResponse.TryGetPreviewVideoId();
        if (!string.IsNullOrWhiteSpace(previewVideoId))
        {
            throw new VideoRequiresPurchaseException(
                $"Video '{videoId}' requires purchase and cannot be played.",
                previewVideoId
            );
        }

        // If the video is unplayable, try one more time by fetching the player response
        // with signature deciphering. This is required for age-restricted videos.
        if (!playerResponse.IsVideoPlayable())
        {
            var playerSource =
                await _controller.TryGetPlayerSourceAsync(cancellationToken) ??
                throw new YoutubeExplodeException("Could not get player source.");

            var signatureTimestamp =
                playerSource.TryGetSignatureTimestamp() ??
                throw new YoutubeExplodeException("Could not get signature timestamp.");

            cipherManifest =
                playerSource.TryGetCipherManifest() ??
                throw new YoutubeExplodeException("Could not get cipher manifest.");

            playerResponse = await _controller.GetPlayerResponseAsync(videoId, signatureTimestamp, cancellationToken);
        }

        // If the video is still unplayable, error out
        if (!playerResponse.IsVideoPlayable())
        {
            var reason = playerResponse.TryGetVideoPlayabilityError();
            throw new VideoUnplayableException(
                $"Video '{videoId}' does not contain any playable streams. Reason: '{reason}'."
            );
        }

        // Extract streams from player response
        await foreach (var streamInfo in
                       GetStreamInfosAsync(playerResponse.GetStreams(), cipherManifest, cancellationToken))
        {
            yield return streamInfo;
        }

        // Extract streams from DASH manifest
        var dashManifestUrl = playerResponse.TryGetDashManifestUrl();
        if (!string.IsNullOrWhiteSpace(dashManifestUrl))
        {
            var dashManifest = await _controller.GetDashManifestAsync(dashManifestUrl, cancellationToken);

            await foreach (var streamInfo in
                           GetStreamInfosAsync(dashManifest.GetStreams(), cipherManifest, cancellationToken))
            {
                yield return streamInfo;
            }
        }
    }

    /// <summary>
    /// Gets the manifest containing information about available streams on the specified video.
    /// </summary>
    public async ValueTask<StreamManifest> GetManifestAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default) =>
        new(await GetStreamInfosAsync(videoId, cancellationToken));

    /// <summary>
    /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it is a livestream).
    /// </summary>
    public async ValueTask<string> GetHttpLiveStreamUrlAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var playerResponse = await _controller.GetPlayerResponseAsync(videoId, cancellationToken);
        if (!playerResponse.IsVideoPlayable())
        {
            var reason = playerResponse.TryGetVideoPlayabilityError();
            throw new VideoUnplayableException($"Video '{videoId}' is unplayable. Reason: '{reason}'.");
        }

        var hlsUrl = playerResponse.TryGetHlsManifestUrl();
        if (string.IsNullOrWhiteSpace(hlsUrl))
        {
            throw new YoutubeExplodeException(
                "Could not extract HTTP Live Stream manifest URL. " +
                $"Video '{videoId}' is likely not a live stream."
            );
        }

        return hlsUrl;
    }

    /// <summary>
    /// Gets the stream identified by the specified metadata.
    /// </summary>
    public async ValueTask<Stream> GetAsync(
        IStreamInfo streamInfo,
        CancellationToken cancellationToken = default)
    {
        // For most streams, YouTube limits transfer speed to match the video playback rate.
        // This helps them avoid unnecessary bandwidth, but for us it's a hindrance because
        // we want to download the stream as fast as possible.
        // To solve this, we divide the logical stream up into multiple segments and download
        // them all separately.

        var isThrottled = !Regex.IsMatch(streamInfo.Url, "ratebypass[=/]yes");

        var segmentSize = isThrottled
            ? 9_898_989 // breakpoint after which the throttling kicks in
            : (long?)null; // no segmentation for non-throttled streams

        var stream = new SegmentedHttpStream(_http, streamInfo.Url, streamInfo.Size.Bytes, segmentSize);

        // Pre-resolve inner stream
        await stream.InitializeAsync(cancellationToken);

        return stream;
    }

    /// <summary>
    /// Copies the stream identified by the specified metadata to the specified stream.
    /// </summary>
    public async ValueTask CopyToAsync(
        IStreamInfo streamInfo,
        Stream destination,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var input = await GetAsync(streamInfo, cancellationToken);
        await input.CopyToAsync(destination, progress, cancellationToken);
    }

    /// <summary>
    /// Downloads the stream identified by the specified metadata to the specified file.
    /// </summary>
    public async ValueTask DownloadAsync(
        IStreamInfo streamInfo,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var destination = File.Create(filePath);
        await CopyToAsync(streamInfo, destination, progress, cancellationToken);
    }
}