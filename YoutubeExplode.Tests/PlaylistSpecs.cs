using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Tests
{
    public class PlaylistSpecs
    {
        [Fact]
        public async Task I_can_get_metadata_of_a_YouTube_playlist()
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
            playlist.Thumbnails?.LowResUrl.Should().NotBeNullOrWhiteSpace();
            playlist.Thumbnails?.MediumResUrl.Should().NotBeNullOrWhiteSpace();
            playlist.Thumbnails?.HighResUrl.Should().NotBeNullOrWhiteSpace();
            playlist.Thumbnails?.StandardResUrl.Should().NotBeNullOrWhiteSpace();
            playlist.Thumbnails?.MaxResUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task I_cannot_get_metadata_of_a_private_YouTube_playlist()
        {
            // Arrange
            const string playlistUrl = "https://www.youtube.com/playlist?list=PLYjTMWc3sa4ZKheRwyA1q56xxQrfQEUBr";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<PlaylistUnavailableException>(() => youtube.Playlists.GetAsync(playlistUrl));
        }

        [Fact]
        public async Task I_cannot_get_metadata_of_a_nonexistent_YouTube_playlist()
        {
            // Arrange
            const string playlistUrl = "https://www.youtube.com/playlist?list=PLYjTMWc3sa4ZKheRwyA1q56xxQrfQEUBx";
            var youtube = new YoutubeClient();

            // Act & assert
            await Assert.ThrowsAsync<PlaylistUnavailableException>(() => youtube.Playlists.GetAsync(playlistUrl));
        }

        [Theory]
        [InlineData("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e")] // normal
        [InlineData("RDCLAK5uy_lf8okgl2ygD075nhnJVjlfhwp8NsUgEbs")] // music mix
        [InlineData("OLAK5uy_lLeonUugocG5J0EUAEDmbskX4emejKwcM")] // music album
        [InlineData("PL601B2E69B03FAB9D")]
        public async Task I_can_get_metadata_of_any_available_YouTube_playlist(string playlistId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var playlist = await youtube.Playlists.GetAsync(playlistId);

            // Assert
            playlist.Id.Value.Should().Be(playlistId);
        }

        [Fact]
        public async Task I_can_get_videos_included_in_a_YouTube_playlist()
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
        [InlineData("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e")] // normal
        [InlineData("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk")] // large
        [InlineData("OLAK5uy_mtOdjCW76nDvf5yOzgcAVMYpJ5gcW5uKU")] // large 2
        [InlineData("RDCLAK5uy_lf8okgl2ygD075nhnJVjlfhwp8NsUgEbs")] // music mix
        [InlineData("UUTMt7iMWa7jy0fNXIktwyLA")] // user uploads
        [InlineData("OLAK5uy_lLeonUugocG5J0EUAEDmbskX4emejKwcM")] // music album
        [InlineData("PL601B2E69B03FAB9D")]
        public async Task I_can_get_videos_included_in_any_available_YouTube_playlist(string playlistId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Playlists.GetVideosAsync(playlistId);

            // Assert
            videos.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e")] // normal
        [InlineData("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk")] // large
        [InlineData("OLAK5uy_mtOdjCW76nDvf5yOzgcAVMYpJ5gcW5uKU")] // large 2
        public async Task I_can_get_a_subset_of_videos_included_in_any_available_YouTube_playlist(string playlistId)
        {
            // Arrange
            const int maxVideoCount = 50;
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Playlists.GetVideosAsync(playlistId).BufferAsync(maxVideoCount);

            // Assert
            videos.Should().NotBeEmpty();
            videos.Should().HaveCountLessOrEqualTo(maxVideoCount);
        }
    }
}