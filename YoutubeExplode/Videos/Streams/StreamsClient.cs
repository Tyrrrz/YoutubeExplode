using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Cipher;
using YoutubeExplode.ReverseEngineering.Responses;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Queries related to media streams of YouTube videos.
    /// </summary>
    public partial class StreamsClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="StreamsClient"/>.
        /// </summary>
        public StreamsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<StreamInfoSource> GetStreamInfoSourceFromVideoInfoAsync(VideoId videoId)
        {
            var embedPage = await EmbedPage.GetAsync(_httpClient, videoId);
            var playerConfig = embedPage.TryGetPlayerConfig() ??
                               throw VideoUnavailableException.Unavailable(videoId);

            var playerSource = await PlayerSource.GetAsync(_httpClient, playerConfig.GetPlayerSourceUrl());
            var cipherOperations = playerSource.GetCipherOperations().ToArray();

            var videoInfoResponse = await VideoInfoResponse.GetAsync(_httpClient, videoId, playerSource.GetSts());
            var playerResponse = videoInfoResponse.GetPlayerResponse();

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnavailableException.Unplayable(videoId);

            if (playerResponse.IsLive())
                throw VideoUnavailableException.Livestream(videoId);

            return new StreamInfoSource(
                videoInfoResponse.GetMuxedStreams().Concat(videoInfoResponse.GetAdaptiveStreams()).ToArray(),
                playerResponse.GetMuxedStreams().Concat(playerResponse.GetAdaptiveStreams()).ToArray(),
                cipherOperations
            );
        }

        private async Task<StreamInfoSource> GetStreamInfoSourceFromWatchPageAsync(VideoId videoId)
        {
            var watchPage = await WatchPage.GetAsync(_httpClient, videoId);
            var playerConfig = watchPage.TryGetPlayerConfig() ??
                               throw VideoUnavailableException.Unavailable(videoId);

            var playerResponse = playerConfig.GetPlayerResponse();

            var playerSource = await PlayerSource.GetAsync(_httpClient, playerConfig.GetPlayerSourceUrl());
            var cipherOperations = playerSource.GetCipherOperations().ToArray();

            if (!playerResponse.IsVideoPlayable())
                throw VideoUnavailableException.Unplayable(videoId);

            if (playerResponse.IsLive())
                throw VideoUnavailableException.Livestream(videoId);

            return new StreamInfoSource(
                playerConfig.GetMuxedStreams().Concat(playerConfig.GetAdaptiveStreams()).ToArray(),
                playerResponse.GetMuxedStreams().Concat(playerResponse.GetAdaptiveStreams()).ToArray(),
                cipherOperations
            );
        }

        private async Task<StreamManifest> GetManifestAsync(StreamInfoSource streamInfoSource)
        {
            var streams = new Dictionary<int, IStreamInfo>();

            foreach (var streamInfo in streamInfoSource.FromVideoInfo)
            {
                var tag = streamInfo.GetTag();
                var url = streamInfo.GetUrl();

                // Signature
                var signature = streamInfo.TryGetSignature();
                var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    signature = streamInfoSource.CipherOperations.Decipher(signature);
                    url = Url.SetQueryParameter(url, signatureParameter, signature);
                }

                // Content length
                var contentLength = streamInfo.TryGetContentLength() ??
                                    await _httpClient.TryGetContentLengthAsync(url, false) ??
                                    0;

                if (contentLength <= 0)
                    continue;

                if (streamInfo.IsMuxed())
                {
                    streams[tag] = new MuxedStreamInfo(
                        tag,
                        url,
                        streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                        contentLength.Pipe(FileSize.FromBytes),
                        streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                        streamInfo.GetAudioCodec(),
                        streamInfo.GetVideoCodec(),
                        tag.Pipe(Heuristics.VideoQualityFromTag).Pipe(Heuristics.VideoQualityToLabel),
                        tag.Pipe(Heuristics.VideoQualityFromTag),
                        new VideoResolution(
                            streamInfo.GetVideoWidth(),
                            streamInfo.GetVideoHeight()
                        ),
                        streamInfo.GetFramerate().Pipe(Framerate.FromFramesPerSecond)
                    );
                }
                else if (streamInfo.IsAudioOnly())
                {
                    streams[tag] = new AudioOnlyStreamInfo(
                        tag,
                        url,
                        streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                        contentLength.Pipe(FileSize.FromBytes),
                        streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                        streamInfo.GetAudioCodec()
                    );
                }
                else
                {
                    streams[tag] = new VideoOnlyStreamInfo(
                        tag,
                        url,
                        streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                        contentLength.Pipe(FileSize.FromBytes),
                        streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                        streamInfo.GetVideoCodec(),
                        streamInfo.GetVideoQualityLabel(),
                        streamInfo.GetVideoQualityLabel().Pipe(Heuristics.VideoQualityFromLabel),
                        new VideoResolution(
                            streamInfo.GetVideoWidth(),
                            streamInfo.GetVideoHeight()
                        ),
                        streamInfo.GetFramerate().Pipe(Framerate.FromFramesPerSecond)
                    );
                }
            }

            foreach (var streamInfo in streamInfoSource.FromPlayerResponse)
            {
                var tag = streamInfo.GetTag();
                var url = streamInfo.GetUrl();

                // Signature
                var signature = streamInfo.TryGetSignature();
                var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    signature = streamInfoSource.CipherOperations.Decipher(signature);
                    url = Url.SetQueryParameter(url, signatureParameter, signature);
                }

                // Content length
                var contentLength = streamInfo.TryGetContentLength() ??
                                    await _httpClient.TryGetContentLengthAsync(url, false) ??
                                    0;

                if (contentLength <= 0)
                    continue;

                if (streamInfo.IsMuxed())
                {
                    streams[tag] = new MuxedStreamInfo(
                        tag,
                        url,
                        streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                        contentLength.Pipe(FileSize.FromBytes),
                        streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                        streamInfo.GetAudioCodec(),
                        streamInfo.GetVideoCodec(),
                        tag.Pipe(Heuristics.VideoQualityFromTag).Pipe(Heuristics.VideoQualityToLabel),
                        tag.Pipe(Heuristics.VideoQualityFromTag),
                        new VideoResolution(
                            streamInfo.GetVideoWidth(),
                            streamInfo.GetVideoHeight()
                        ),
                        streamInfo.GetFramerate().Pipe(Framerate.FromFramesPerSecond)
                    );
                }
                else if (streamInfo.IsAudioOnly())
                {
                    streams[tag] = new AudioOnlyStreamInfo(
                        tag,
                        url,
                        streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                        contentLength.Pipe(FileSize.FromBytes),
                        streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                        streamInfo.GetAudioCodec()
                    );
                }
                else
                {
                    streams[tag] = new VideoOnlyStreamInfo(
                        tag,
                        url,
                        streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                        contentLength.Pipe(FileSize.FromBytes),
                        streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                        streamInfo.GetVideoCodec(),
                        streamInfo.GetVideoQualityLabel(),
                        streamInfo.GetVideoQualityLabel().Pipe(Heuristics.VideoQualityFromLabel),
                        new VideoResolution(
                            streamInfo.GetVideoWidth(),
                            streamInfo.GetVideoHeight()
                        ),
                        streamInfo.GetFramerate().Pipe(Framerate.FromFramesPerSecond)
                    );
                }
            }

            return new StreamManifest(streams.Values.ToArray());
        }

        /// <summary>
        /// Gets the manifest that contains information about available streams in the specified video.
        /// </summary>
        public async Task<StreamManifest> GetManifestAsync(VideoId videoId)
        {
            try
            {
                var streamInfoSource = await GetStreamInfoSourceFromVideoInfoAsync(videoId);
                return await GetManifestAsync(streamInfoSource);
            }
            catch (YoutubeExplodeException)
            {
                var streamInfoSource = await GetStreamInfoSourceFromWatchPageAsync(videoId);
                return await GetManifestAsync(streamInfoSource);
            }
        }

        /// <summary>
        /// Gets the actual stream which is identified by the specified metadata.
        /// </summary>
        public Task<Stream> GetAsync(IStreamInfo streamInfo)
        {
            // YouTube streams are often rate-limited -- they return data at about the same rate
            // as the actual video is going. This helps them avoid unnecessary bandwidth by not loading
            // all data eagerly. On the other hand, we want to download the streams as fast as possible,
            // so we'll be splitting the stream into small segments and retrieving them separately, to
            // work around rate limiting.

            var segmentSize = !Regex.IsMatch(streamInfo.Url, "ratebypass[=/]yes")
                ? 9_898_989 // this number was carefully devised through research
                : long.MaxValue; // don't use segmentation for non-rate-limited streams

            var stream = _httpClient.CreateSegmentedStream(streamInfo.Url, streamInfo.Size.TotalBytes, segmentSize);

            return Task.FromResult<Stream>(stream);
        }

        /// <summary>
        /// Copies the actual stream which is identified by the specified metadata to the specified stream.
        /// </summary>
        public async Task CopyToAsync(IStreamInfo streamInfo, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var input = await GetAsync(streamInfo);
            await input.CopyToAsync(destination, progress, cancellationToken);
        }

        /// <summary>
        /// Download the actual stream which is identified by the specified metadata to the specified file.
        /// </summary>
        public async Task DownloadAsync(IStreamInfo streamInfo, string filePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var destination = File.Create(filePath);
            await CopyToAsync(streamInfo, destination, progress, cancellationToken);
        }
    }

    public partial class StreamsClient
    {
        private class StreamInfoSource
        {
            public IReadOnlyList<VideoInfoResponse.StreamInfo> FromVideoInfo { get; }

            public IReadOnlyList<PlayerResponse.StreamInfo> FromPlayerResponse { get; }

            public IReadOnlyList<ICipherOperation> CipherOperations { get; }

            public StreamInfoSource(
                IReadOnlyList<VideoInfoResponse.StreamInfo> fromVideoInfo,
                IReadOnlyList<PlayerResponse.StreamInfo> fromPlayerResponse,
                IReadOnlyList<ICipherOperation> cipherOperations)
            {
                FromVideoInfo = fromVideoInfo;
                FromPlayerResponse = fromPlayerResponse;
                CipherOperations = cipherOperations;
            }
        }
    }
}