using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Tests.Internal;

namespace YoutubeExplode.Tests
{
    public class ClosedCaptionSpecs : IDisposable
    {
        private string TempDirPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(ClosedCaptionSpecs)}_{Guid.NewGuid()}");

        public ClosedCaptionSpecs() => DirectoryEx.Reset(TempDirPath);

        public void Dispose() => DirectoryEx.DeleteIfExists(TempDirPath);

        [Theory]
        [InlineData("_QdPW8JrYzQ")]
        public async Task I_can_get_available_closed_caption_tracks_of_any_available_YouTube_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            // Assert
            manifest.Tracks.Should().NotBeEmpty();
        }

        [Fact]
        public async Task I_can_get_a_specific_closed_caption_track()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=_QdPW8JrYzQ";
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
            var trackInfo = manifest.TryGetByLanguage("en")!;
            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

            // Assert
            track.Captions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task I_can_get_a_specific_closed_caption_track_and_extract_a_closed_caption_that_appears_at_a_specific_time()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=_QdPW8JrYzQ";
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
            var trackInfo = manifest.TryGetByLanguage("en")!;
            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
            var caption = track.TryGetByTime(TimeSpan.FromSeconds(61))!;

            // Assert
            caption.Should().NotBeNull();
            caption.Text.Should().Be("And the game was afoot.");
        }

        [Fact]
        public async Task I_can_download_a_specific_closed_caption_track()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=_QdPW8JrYzQ";
            var filePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}.srt");
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
            var trackInfo = manifest.TryGetByLanguage("en")!;
            await youtube.Videos.ClosedCaptions.DownloadAsync(trackInfo, filePath);

            // Assert
            File.Exists(filePath).Should().BeTrue();
        }
    }
}