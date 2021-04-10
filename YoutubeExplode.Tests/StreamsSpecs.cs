using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.Fixtures;
using YoutubeExplode.Tests.Ids;

namespace YoutubeExplode.Tests
{
    public class StreamsSpecs : IClassFixture<TempOutputFixture>
    {
        private readonly TempOutputFixture _tempOutputFixture;

        public StreamsSpecs(TempOutputFixture tempOutputFixture) =>
            _tempOutputFixture = tempOutputFixture;

        [Theory]
        [InlineData(VideoIds.Normal)]
        [InlineData(VideoIds.Unlisted)]
        [InlineData(VideoIds.LiveStreamRecording)]
        [InlineData(VideoIds.ContainsDashManifest)]
        [InlineData(VideoIds.Omnidirectional)]
        [InlineData(VideoIds.EmbedRestrictedByAuthor)]
        [InlineData(VideoIds.EmbedRestrictedByYouTube)]
        [InlineData(VideoIds.AgeRestricted)]
        [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
        public async Task User_can_get_the_list_of_available_streams_on_a_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            // Assert
            manifest.Streams.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_cannot_get_the_list_of_available_streams_on_an_unplayable_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnplayableException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(VideoIds.LiveStream)
            );
        }

        [Fact]
        public async Task User_cannot_get_the_list_of_available_streams_on_a_paid_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            var ex = await Assert.ThrowsAsync<VideoRequiresPurchaseException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(VideoIds.RequiresPurchase)
            );

            ex.PreviewVideoId.Value.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task User_cannot_get_the_list_of_available_streams_on_a_private_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(VideoIds.Private)
            );
        }

        [Fact]
        public async Task User_cannot_get_the_list_of_available_streams_on_a_non_existing_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnavailableException>(async () =>
                await youtube.Videos.Streams.GetManifestAsync(VideoIds.NonExisting)
            );
        }

        [Theory]
        [InlineData(VideoIds.Normal)]
        [InlineData(VideoIds.AgeRestricted)]
        [InlineData(VideoIds.LiveStreamRecording)]
        [InlineData(VideoIds.ContainsDashManifest)]
        [InlineData(VideoIds.Omnidirectional)]
        public async Task User_can_get_a_specific_stream_from_a_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            foreach (var streamInfo in manifest.Streams)
            {
                var buffer = new byte[1024];

                await using var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                var bytesRead = await stream.ReadAsync(buffer);

                // Assert
                bytesRead.Should().BeGreaterThan(0);
            }
        }

        [Theory]
        [InlineData(VideoIds.Normal)]
        [InlineData(VideoIds.Unlisted)]
        [InlineData(VideoIds.LiveStreamRecording)]
        [InlineData(VideoIds.ContainsDashManifest)]
        [InlineData(VideoIds.Omnidirectional)]
        [InlineData(VideoIds.EmbedRestrictedByAuthor)]
        [InlineData(VideoIds.EmbedRestrictedByYouTube)]
        [InlineData(VideoIds.AgeRestricted)]
        [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
        public async Task User_can_download_a_specific_stream_from_a_video(string videoId)
        {
            // Arrange
            var filePath = _tempOutputFixture.GetTempFilePath();
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = manifest.Streams.OrderBy(s => s.Size).First();

            await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);

            var fileInfo = new FileInfo(filePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task User_can_get_HTTP_live_stream_URL_from_a_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var url = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(VideoIds.LiveStream);

            // Assert
            url.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task User_cannot_get_HTTP_live_stream_URL_from_an_unplayable_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<VideoUnplayableException>(async () =>
                await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(VideoIds.RequiresPurchase)
            );
        }

        [Fact]
        public async Task User_cannot_get_HTTP_live_stream_URL_from_a_non_live_video()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<YoutubeExplodeException>(async () =>
                await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(VideoIds.Normal)
            );
        }
    }
}