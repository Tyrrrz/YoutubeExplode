using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.Ids;

namespace YoutubeExplode.Tests
{
    public class PlaylistSpecs
    {
        private readonly ITestOutputHelper _testOutput;

        public PlaylistSpecs(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public async Task User_can_get_metadata_of_a_playlist()
        {
            // Arrange
            const string playlistUrl = "https://www.youtube.com/playlist?list=PLr-IftNTIujSF-8tlGbZBQyGIT6TCF6Yd";
            var youtube = new YoutubeClient();

            // Act
            var playlist = await youtube.Playlists.GetAsync(playlistUrl);

            // Assert
            playlist.Id.Value.Should().Be("PLr-IftNTIujSF-8tlGbZBQyGIT6TCF6Yd");
            playlist.Url.Should().Be(playlistUrl);
            playlist.Title.Should().Be("osu! Highlights");
            playlist.Author.Should().Be("Tyrrrz");
            playlist.Description.Should().Be("My best osu! plays");
            playlist.ViewCount.Should().BeGreaterOrEqualTo(133);
            playlist.Thumbnails.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_cannot_get_metadata_of_a_private_playlist()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            var ex = await Assert.ThrowsAsync<PlaylistUnavailableException>(async () =>
                await youtube.Playlists.GetAsync(PlaylistIds.Private)
            );

            _testOutput.WriteLine(ex.Message);
        }

        [Fact]
        public async Task User_cannot_get_metadata_of_a_non_existing_playlist()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act & assert
            var ex = await Assert.ThrowsAsync<PlaylistUnavailableException>(async () =>
                await youtube.Playlists.GetAsync(PlaylistIds.NonExisting)
            );

            _testOutput.WriteLine(ex.Message);
        }

        [Theory]
        [InlineData(PlaylistIds.Normal)]
        [InlineData(PlaylistIds.MusicMix)]
        [InlineData(PlaylistIds.MusicAlbum)]
        [InlineData(PlaylistIds.ContainsLongVideos)]
        [InlineData(PlaylistIds.Weird)]
        public async Task User_can_get_metadata_of_any_available_playlist(string playlistId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var playlist = await youtube.Playlists.GetAsync(playlistId);

            // Assert
            playlist.Id.Value.Should().Be(playlistId);
        }

        [Fact]
        public async Task User_can_get_videos_included_in_a_playlist()
        {
            // Arrange
            const string playlistUrl = "https://www.youtube.com/playlist?list=PLr-IftNTIujSF-8tlGbZBQyGIT6TCF6Yd";
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Playlists.GetVideosAsync(playlistUrl);

            // Assert
            videos.Should().HaveCountGreaterOrEqualTo(19);
            videos.Select(v => v.Id.Value).Should().Contain(new[]
            {
                "B6N8-_rBTh8",
                "F1bvjgTckMc",
                "kMBzljXOb9g",
                "LsNPjFXIPT8",
                "fXYPMPglYTs",
                "AI7ULzgf8RU",
                "Qzu-fTdjeFY"
            });
        }

        [Theory]
        [InlineData(PlaylistIds.Normal)]
        [InlineData(PlaylistIds.Large)]
        [InlineData(PlaylistIds.MusicMix)]
        [InlineData(PlaylistIds.MusicAlbum)]
        [InlineData(PlaylistIds.UserUploads)]
        [InlineData(PlaylistIds.ContainsLongVideos)]
        [InlineData(PlaylistIds.Weird)]
        public async Task User_can_get_videos_included_in_any_available_playlist(string playlistId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Playlists.GetVideosAsync(playlistId);

            // Assert
            videos.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_can_get_a_subset_of_videos_included_in_a_playlist()
        {
            // Arrange
            const string playlistUrl = "https://www.youtube.com/playlist?list=PLr-IftNTIujSF-8tlGbZBQyGIT6TCF6Yd";
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Playlists.GetVideosAsync(playlistUrl).BufferAsync(10);

            // Assert
            videos.Should().HaveCount(10);
        }
    }
}