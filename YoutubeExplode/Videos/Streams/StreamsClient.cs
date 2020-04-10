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
    public class StreamsClient
    {
        private readonly HttpClient _httpClient;

        public StreamsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<IStreamInfo?> TryGetMuxedStreamInfoAsync(
            VideoInfoResponse.StreamInfo streamInfo,
            IReadOnlyList<ICipherOperation> cipherOperations)
        {
            var tag = streamInfo.GetTag();
            var url = streamInfo.GetUrl();

            // Signature
            var signature = streamInfo.TryGetSignature();
            var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

            if (!string.IsNullOrWhiteSpace(signature))
            {
                signature = cipherOperations.Decipher(signature);
                url = Url.SetQueryParameter(url, signatureParameter, signature);
            }

            // Content length
            var contentLength = streamInfo.TryGetContentLength() ??
                                await _httpClient.TryGetContentLengthAsync(url, false) ??
                                0;

            if (contentLength <= 0)
                return null;

            return new MuxedStreamInfo(
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

        private async Task<IStreamInfo?> TryGetMuxedStreamInfoAsync(
            PlayerResponse.StreamInfo streamInfo,
            IReadOnlyList<ICipherOperation> cipherOperations)
        {
            var tag = streamInfo.GetTag();
            var url = streamInfo.GetUrl();

            // Signature
            var signature = streamInfo.TryGetSignature();
            var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

            if (!string.IsNullOrWhiteSpace(signature))
            {
                signature = cipherOperations.Decipher(signature);
                url = Url.SetQueryParameter(url, signatureParameter, signature);
            }

            // Content length
            var contentLength = streamInfo.TryGetContentLength() ??
                                await _httpClient.TryGetContentLengthAsync(url, false) ??
                                0;

            if (contentLength <= 0)
                return null;

            return new MuxedStreamInfo(
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

        private async Task<IStreamInfo?> TryGetAdaptiveStreamInfoAsync(
            VideoInfoResponse.StreamInfo streamInfo,
            IReadOnlyList<ICipherOperation> cipherOperations)
        {
            var tag = streamInfo.GetTag();
            var url = streamInfo.GetUrl();

            // Signature
            var signature = streamInfo.TryGetSignature();
            var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

            if (!string.IsNullOrWhiteSpace(signature))
            {
                signature = cipherOperations.Decipher(signature);
                url = Url.SetQueryParameter(url, signatureParameter, signature);
            }

            // Content length
            var contentLength = streamInfo.TryGetContentLength() ??
                                await _httpClient.TryGetContentLengthAsync(url, false) ??
                                0;

            if (contentLength <= 0)
                return null;

            // Audio-only
            if (streamInfo.IsAudioOnly())
            {
                return new AudioOnlyStreamInfo(
                    tag,
                    url,
                    streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                    contentLength.Pipe(FileSize.FromBytes),
                    streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                    streamInfo.GetAudioCodec()
                );
            }
            // Video-only
            else
            {
                return new VideoOnlyStreamInfo(
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

        private async Task<IStreamInfo?> TryGetAdaptiveStreamInfoAsync(
            PlayerResponse.StreamInfo streamInfo,
            IReadOnlyList<ICipherOperation> cipherOperations)
        {
            var tag = streamInfo.GetTag();
            var url = streamInfo.GetUrl();

            // Signature
            var signature = streamInfo.TryGetSignature();
            var signatureParameter = streamInfo.TryGetSignatureParameter() ?? "signature";

            if (!string.IsNullOrWhiteSpace(signature))
            {
                signature = cipherOperations.Decipher(signature);
                url = Url.SetQueryParameter(url, signatureParameter, signature);
            }

            // Content length
            var contentLength = streamInfo.TryGetContentLength() ??
                                await _httpClient.TryGetContentLengthAsync(url, false) ??
                                0;

            if (contentLength <= 0)
                return null;

            // Audio-only
            if (streamInfo.IsAudioOnly())
            {
                return new AudioOnlyStreamInfo(
                    tag,
                    url,
                    streamInfo.GetContainer().Pipe(Heuristics.ContainerFromMimeType),
                    contentLength.Pipe(FileSize.FromBytes),
                    streamInfo.GetBitrate().Pipe(Bitrate.FromBytesPerSecond),
                    streamInfo.GetAudioCodec()
                );
            }
            // Video-only
            else
            {
                return new VideoOnlyStreamInfo(
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

        private async Task<StreamManifest> GetManifestFromVideoInfoAsync(VideoId videoId)
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

            var streamInfos = new Dictionary<int, IStreamInfo>();

            foreach (var streamInfoProvider in videoInfoResponse.GetMuxedStreams())
            {
                var streamInfo = await TryGetMuxedStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            foreach (var streamInfoProvider in playerResponse.GetMuxedStreams())
            {
                var streamInfo = await TryGetMuxedStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            foreach (var streamInfoProvider in videoInfoResponse.GetAdaptiveStreams())
            {
                var streamInfo = await TryGetAdaptiveStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            foreach (var streamInfoProvider in playerResponse.GetAdaptiveStreams())
            {
                var streamInfo = await TryGetAdaptiveStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            return new StreamManifest(streamInfos.Values.ToArray());
        }

        private async Task<StreamManifest> GetManifestFromWatchPageAsync(VideoId videoId)
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

            var streamInfos = new Dictionary<int, IStreamInfo>();

            foreach (var streamInfoProvider in playerConfig.GetMuxedStreams())
            {
                var streamInfo = await TryGetMuxedStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            foreach (var streamInfoProvider in playerResponse.GetMuxedStreams())
            {
                var streamInfo = await TryGetMuxedStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            foreach (var streamInfoProvider in playerConfig.GetAdaptiveStreams())
            {
                var streamInfo = await TryGetAdaptiveStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            foreach (var streamInfoProvider in playerResponse.GetAdaptiveStreams())
            {
                var streamInfo = await TryGetAdaptiveStreamInfoAsync(streamInfoProvider, cipherOperations);

                if (streamInfo != null)
                {
                    streamInfos[streamInfo.Tag] = streamInfo;
                }
            }

            return new StreamManifest(streamInfos.Values.ToArray());
        }

        /// <summary>
        /// Gets the available streams for this video.
        /// </summary>
        public async Task<StreamManifest> GetManifestAsync(VideoId videoId)
        {
            try
            {
                return await GetManifestFromVideoInfoAsync(videoId);
            }
            catch (YoutubeExplodeException)
            {
                return await GetManifestFromWatchPageAsync(videoId);
            }
        }

        public Task<Stream> GetStreamAsync(IStreamInfo streamInfo)
        {
            // Get segment size depending on whether the stream is rate limited or not
            var segmentSize = !Regex.IsMatch(streamInfo.Url, "ratebypass[=/]yes")
                ? 9_898_989 // this number was carefully devised through research
                : long.MaxValue; // don't use segmentation for non-rate-limited streams

            var stream = _httpClient.CreateSegmentedStream(streamInfo.Url, streamInfo.Size.TotalBytes, segmentSize);

            return Task.FromResult<Stream>(stream);
        }

        public async Task CopyToAsync(IStreamInfo streamInfo, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var input = await GetStreamAsync(streamInfo);
            await input.CopyToAsync(destination, progress, cancellationToken);
        }

        public async Task DownloadAsync(IStreamInfo streamInfo, string filePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var destination = File.Create(filePath);
            await CopyToAsync(streamInfo, destination, progress, cancellationToken);
        }
    }
}