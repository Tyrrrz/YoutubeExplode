using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

    // Because we determine the player version ourselves, it's safe to cache the cipher manifest
    // for the entire lifetime of the client.
    private CipherManifest? _cipherManifest;

    /// <summary>
    /// Initializes an instance of <see cref="StreamClient" />.
    /// </summary>
    public StreamClient(HttpClient http)
    {
        _http = http;
        _controller = new StreamController(http);
    }

    private async ValueTask<CipherManifest> ResolveCipherManifestAsync(
        CancellationToken cancellationToken
    )
    {
        if (_cipherManifest is not null)
            return _cipherManifest;

        var playerSource = await _controller.GetPlayerSourceAsync(cancellationToken);

        return _cipherManifest =
            playerSource.CipherManifest
            ?? throw new YoutubeExplodeException("Failed to extract the cipher manifest.");
    }

    private async IAsyncEnumerable<IStreamInfo> GetStreamInfosAsync(
        IEnumerable<IStreamData> streamDatas,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var streamData in streamDatas)
        {
            var itag =
                streamData.Itag
                ?? throw new YoutubeExplodeException("Failed to extract the stream itag.");

            var url =
                streamData.Url
                ?? throw new YoutubeExplodeException("Failed to extract the stream URL.");

            // Handle cipher-protected streams
            if (!string.IsNullOrWhiteSpace(streamData.Signature))
            {
                var cipherManifest = await ResolveCipherManifestAsync(cancellationToken);

                url = UrlEx.SetQueryParameter(
                    url,
                    streamData.SignatureParameter ?? "sig",
                    cipherManifest.Decipher(streamData.Signature)
                );
            }

            var contentLength =
                streamData.ContentLength
                ?? await _http.TryGetContentLengthAsync(url, false, cancellationToken)
                ?? 0;

            // Stream is empty or cannot be accessed
            if (contentLength <= 0)
                continue;

            // Streams may have mismatched content length, so ensure that the obtained value is correct
            // https://github.com/Tyrrrz/YoutubeExplode/issues/759
            using (
                var response = await _http.GetAsync(
                    // Try to access the last byte of the stream
                    MediaStream.GetSegmentUrl(url, contentLength - 2, contentLength - 1),
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                )
            )
            {
                // Only check for the 404 status here, as other errors (e.g. 401/403) may indicate
                // at issues with the extraction process, not with the stream itself.
                if (response.StatusCode == HttpStatusCode.NotFound)
                    continue;
            }

            var container =
                streamData.Container?.Pipe(s => new Container(s))
                ?? throw new YoutubeExplodeException("Failed to extract the stream container.");

            var bitrate =
                streamData.Bitrate?.Pipe(s => new Bitrate(s))
                ?? throw new YoutubeExplodeException("Failed to extract the stream bitrate.");

            // Muxed or video-only stream
            if (!string.IsNullOrWhiteSpace(streamData.VideoCodec))
            {
                var framerate = streamData.VideoFramerate ?? 24;

                var videoQuality = !string.IsNullOrWhiteSpace(streamData.VideoQualityLabel)
                    ? VideoQuality.FromLabel(streamData.VideoQualityLabel, framerate)
                    : VideoQuality.FromItag(itag, framerate);

                var videoResolution =
                    streamData.VideoWidth is not null && streamData.VideoHeight is not null
                        ? new Resolution(streamData.VideoWidth.Value, streamData.VideoHeight.Value)
                        : videoQuality.GetDefaultVideoResolution();

                // Muxed
                if (!string.IsNullOrWhiteSpace(streamData.AudioCodec))
                {
                    var streamInfo = new MuxedStreamInfo(
                        url,
                        container,
                        new FileSize(contentLength),
                        bitrate,
                        streamData.AudioCodec,
                        streamData.VideoCodec,
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
                        new FileSize(contentLength),
                        bitrate,
                        streamData.VideoCodec,
                        videoQuality,
                        videoResolution
                    );

                    yield return streamInfo;
                }
            }
            // Audio-only
            else if (!string.IsNullOrWhiteSpace(streamData.AudioCodec))
            {
                var streamInfo = new AudioOnlyStreamInfo(
                    url,
                    container,
                    new FileSize(contentLength),
                    bitrate,
                    streamData.AudioCodec
                );

                yield return streamInfo;
            }
            else
            {
                throw new YoutubeExplodeException("Failed to extract the stream codec.");
            }
        }
    }

    private async ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosAsync(
        VideoId videoId,
        PlayerResponse playerResponse,
        CancellationToken cancellationToken = default
    )
    {
        // Video is pay-to-play
        if (!string.IsNullOrWhiteSpace(playerResponse.PreviewVideoId))
        {
            throw new VideoRequiresPurchaseException(
                $"Video '{videoId}' requires purchase and cannot be played.",
                playerResponse.PreviewVideoId
            );
        }

        // Video is unplayable
        if (!playerResponse.IsPlayable)
        {
            throw new VideoUnplayableException(
                $"Video '{videoId}' is unplayable. Reason: '{playerResponse.PlayabilityError}'."
            );
        }

        var streamInfos = new List<IStreamInfo>();

        // Extract streams from the player response
        await foreach (
            var streamInfo in GetStreamInfosAsync(playerResponse.Streams, cancellationToken)
        )
        {
            streamInfos.Add(streamInfo);
        }

        // Extract streams from the DASH manifest
        if (!string.IsNullOrWhiteSpace(playerResponse.DashManifestUrl))
        {
            try
            {
                var dashManifest = await _controller.GetDashManifestAsync(
                    playerResponse.DashManifestUrl,
                    cancellationToken
                );

                await foreach (
                    var streamInfo in GetStreamInfosAsync(dashManifest.Streams, cancellationToken)
                )
                {
                    streamInfos.Add(streamInfo);
                }
            }
            // Some DASH manifest URLs return 404 for whatever reason
            // https://github.com/Tyrrrz/YoutubeExplode/issues/728
            catch (HttpRequestException) { }
        }

        // Error if no streams were found
        if (!streamInfos.Any())
        {
            throw new VideoUnplayableException(
                $"Video '{videoId}' does not contain any playable streams."
            );
        }

        return streamInfos;
    }

    private async ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        // Try to get player response from a cipher-less client
        try
        {
            var playerResponse = await _controller.GetPlayerResponseAsync(
                videoId,
                cancellationToken
            );

            return await GetStreamInfosAsync(videoId, playerResponse, cancellationToken);
        }
        catch (VideoUnplayableException) { }

        // Try to get player response from a client with cipher
        {
            var cipherManifest = await ResolveCipherManifestAsync(cancellationToken);

            var playerResponse = await _controller.GetPlayerResponseAsync(
                videoId,
                cipherManifest.SignatureTimestamp,
                cancellationToken
            );

            return await GetStreamInfosAsync(videoId, playerResponse, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the manifest that lists available streams for the specified video.
    /// </summary>
    public async ValueTask<StreamManifest> GetManifestAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            var streamInfos = await GetStreamInfosAsync(videoId, cancellationToken);

            // YouTube sometimes returns stream URLs that produce 403 Forbidden errors when accessed.
            // This happens for both protected and non-protected streams, so the cause is unclear.
            // As a workaround, we can access one of the stream URLs and retry if it fails.
            using var response = await _http.HeadAsync(streamInfos.First().Url, cancellationToken);
            if ((int)response.StatusCode == 403 && retriesRemaining > 0)
                continue;

            response.EnsureSuccessStatusCode();

            return new StreamManifest(streamInfos);
        }
    }

    /// <summary>
    /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it is a livestream).
    /// </summary>
    public async ValueTask<string> GetHttpLiveStreamUrlAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        var playerResponse = await _controller.GetPlayerResponseAsync(videoId, cancellationToken);
        if (!playerResponse.IsPlayable)
        {
            throw new VideoUnplayableException(
                $"Video '{videoId}' is unplayable. "
                    + $"Reason: '{playerResponse.PlayabilityError}'."
            );
        }

        if (string.IsNullOrWhiteSpace(playerResponse.HlsManifestUrl))
        {
            throw new YoutubeExplodeException(
                "Failed to extract the HTTP Live Stream manifest URL. "
                    + $"Video '{videoId}' is likely not a live stream."
            );
        }

        return playerResponse.HlsManifestUrl;
    }

    /// <summary>
    /// Gets the stream identified by the specified metadata.
    /// </summary>
    public async ValueTask<Stream> GetAsync(
        IStreamInfo streamInfo,
        CancellationToken cancellationToken = default
    )
    {
        var stream = new MediaStream(_http, streamInfo);
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
        CancellationToken cancellationToken = default
    )
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
        CancellationToken cancellationToken = default
    )
    {
        using var destination = File.Create(filePath);
        await CopyToAsync(streamInfo, destination, progress, cancellationToken);
    }
}
