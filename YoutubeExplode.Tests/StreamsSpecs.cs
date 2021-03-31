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
        [InlineData("SkRSXFQerZs")] // age restricted (embed allowed)
        [InlineData("hySoCSoH-g8")] // age restricted (embed not allowed)
        [InlineData("_kmeFXjjGfk")] // embed not allowed (type 1)
        [InlineData("MeJVWBSsPAY")] // embed not allowed (type 2)
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("rsAAeyAr-9Y")] // recording of a live stream
        [InlineData("AI7ULzgf8RU")] // has DASH manifest
        [InlineData("-xNN-bJQ4vI")] // 360° video
        public async Task User_can_get_available_streams_of_a_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            // Assert
            manifest.Streams.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_cannot_get_available_streams_of_an_unplayable_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=5qap5aO4i9A"; // live stream
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnplayableException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(videoUrl)
            );
        }

        [Fact]
        public async Task User_cannot_get_available_streams_of_a_paid_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=p3dDcKOFXQg";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoRequiresPurchaseException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(videoUrl)
            );
        }

        [Fact]
        public async Task User_cannot_get_available_streams_of_a_private_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=pb_hHv3fByo";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(videoUrl)
            );
        }

        [Fact]
        public async Task User_cannot_get_available_streams_of_a_non_existing_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=qld9w0b-1ao";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(videoUrl)
            );
        }

        [Theory]
        [InlineData("9bZkp7q19f0")] // very popular
        [InlineData("SkRSXFQerZs")] // age restricted (embed allowed)
        [InlineData("hySoCSoH-g8")] // age restricted (embed not allowed)
        [InlineData("_kmeFXjjGfk")] // embed not allowed (type 1)
        [InlineData("MeJVWBSsPAY")] // embed not allowed (type 2)
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("rsAAeyAr-9Y")] // recording of a live stream
        [InlineData("AI7ULzgf8RU")] // has DASH manifest
        [InlineData("-xNN-bJQ4vI")] // 360° video
        public async Task User_can_get_a_specific_stream_of_a_video(string videoId)
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
        [InlineData("SkRSXFQerZs")] // age restricted (embed allowed)
        [InlineData("hySoCSoH-g8")] // age restricted (embed not allowed)
        [InlineData("_kmeFXjjGfk")] // embed not allowed (type 1)
        [InlineData("MeJVWBSsPAY")] // embed not allowed (type 2)
        [InlineData("5VGm0dczmHc")] // rating not allowed
        [InlineData("ZGdLIwrGHG8")] // unlisted
        [InlineData("rsAAeyAr-9Y")] // recording of a live stream
        [InlineData("AI7ULzgf8RU")] // has DASH manifest
        [InlineData("-xNN-bJQ4vI")] // 360° video
        public async Task User_can_download_a_specific_stream_of_a_video(string videoId)
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

        [Fact]
        public async Task User_can_get_HTTP_live_stream_URL_of_a_live_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=5qap5aO4i9A";
            var youtube = new YoutubeClient();

            // Act
            var url = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(videoUrl);

            // Assert
            url.Should().NotBeNullOrWhiteSpace();
        }
    }
}