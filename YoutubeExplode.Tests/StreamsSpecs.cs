using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.Fixtures;

namespace YoutubeExplode.Tests
{
    public class StreamsSpecs : IClassFixture<TempOutputFixture>
    {
        private readonly TempOutputFixture _tempOutputFixture;

        public StreamsSpecs(TempOutputFixture tempOutputFixture) =>
            _tempOutputFixture = tempOutputFixture;

        [Theory]
        [InlineData("9bZkp7q19f0")] // very popular
        //[InlineData("SkRSXFQerZs")] // age restricted (embed allowed)
        [InlineData("hySoCSoH-g8")] // age restricted (embed not allowed)
        [InlineData("_kmeFXjjGfk")] // embed not allowed (type 1)
        [InlineData("MeJVWBSsPAY")] // embed not allowed (type 2)
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("rsAAeyAr-9Y")] // recording of a live stream
        [InlineData("AI7ULzgf8RU")] // has DASH manifest
        public async Task I_can_get_available_streams_of_any_playable_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            // Assert
            manifest.Streams.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("5qap5aO4i9A")] // live stream
        public async Task I_cannot_get_available_streams_of_an_unplayable_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnplayableException>(() => youtube.Videos.Streams.GetManifestAsync(videoId));
        }

        [Theory]
        [InlineData("p3dDcKOFXQg")] // requires purchase
        public async Task I_cannot_get_available_streams_of_a_YouTube_video_that_requires_purchase(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoRequiresPurchaseException>(() => youtube.Videos.Streams.GetManifestAsync(videoId));
        }

        [Theory]
        [InlineData("qld9w0b-1ao")] // doesn't exist
        [InlineData("pb_hHv3fByo")] // private
        public async Task I_cannot_get_available_streams_of_an_unavailable_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(() => youtube.Videos.Streams.GetManifestAsync(videoId));
        }

        [Theory]
        [InlineData("9bZkp7q19f0")] // very popular
        //[InlineData("SkRSXFQerZs")] // age restricted (embed allowed)
        [InlineData("hySoCSoH-g8")] // age restricted (embed not allowed)
        [InlineData("_kmeFXjjGfk")] // embed not allowed (type 1)
        [InlineData("MeJVWBSsPAY")] // embed not allowed (type 2)
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("rsAAeyAr-9Y")] // recording of a live stream
        [InlineData("AI7ULzgf8RU")] // has DASH manifest
        [InlineData("-xNN-bJQ4vI")] // 360° video
        public async Task I_can_get_a_specific_stream_of_any_playable_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            foreach (var streamInfo in manifest.Streams)
            {
                var buffer = new byte[1024];

                await using var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                await stream.ReadAsync(buffer);

                var isBufferEmpty = buffer.Distinct().Count() <= 1;

                // Assert
                isBufferEmpty.Should().BeFalse();
            }

            // Assert
            manifest.Streams.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("9bZkp7q19f0")] // very popular
        //[InlineData("SkRSXFQerZs")] // age restricted (embed allowed)
        [InlineData("hySoCSoH-g8")] // age restricted (embed not allowed)
        [InlineData("_kmeFXjjGfk")] // embed not allowed (type 1)
        [InlineData("MeJVWBSsPAY")] // embed not allowed (type 2)
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("rsAAeyAr-9Y")] // recording of a live stream
        [InlineData("AI7ULzgf8RU")] // has DASH manifest
        [InlineData("-xNN-bJQ4vI")] // 360° video
        public async Task I_can_download_a_specific_stream_of_any_playable_YouTube_video(string videoId)
        {
            // Arrange
            var filePath = _tempOutputFixture.GetTempFilePath();
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = manifest.Streams.OrderBy(s => s.Size).First();

            await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);

            // Assert
            File.Exists(filePath).Should().BeTrue();
        }

        [Theory]
        [InlineData("5qap5aO4i9A")]
        public async Task I_can_get_http_live_stream_url_of_any_ongoing_YouTube_live_stream_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var url = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(videoId);

            // Assert
            url.Should().NotBeNullOrWhiteSpace();
        }
    }
}