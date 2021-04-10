using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Tests.Fixtures;

namespace YoutubeExplode.Tests
{
    public class ClosedCaptionSpecs : IClassFixture<TempOutputFixture>
    {
        private readonly TempOutputFixture _tempOutputFixture;

        public ClosedCaptionSpecs(TempOutputFixture tempOutputFixture) =>
            _tempOutputFixture = tempOutputFixture;

        [Theory]
        [InlineData("WOxr2dmLHLo")]
        [InlineData("YltHGKX80Y8")]
        public async Task User_can_get_the_list_of_available_closed_caption_tracks_on_a_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            // Assert
            manifest.Tracks.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("WOxr2dmLHLo")]
        [InlineData("YltHGKX80Y8")]
        public async Task User_can_get_a_specific_closed_caption_track_from_a_video(string videoId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);
            var trackInfo = manifest.Tracks.First();

            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

            // Assert
            track.Captions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_can_get_an_individual_closed_caption_that_appears_at_a_specific_time_on_a_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=YltHGKX80Y8";
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
            var trackInfo = manifest.GetByLanguage("en");

            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

            var caption = track.GetByTime(TimeSpan.FromSeconds(641));

            // Assert
            caption.Text.Should().Be("know I worked really hard on not doing");
        }

        [Fact]
        public async Task User_can_get_an_individual_closed_caption_part_that_appears_at_a_specific_time_on_a_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=YltHGKX80Y8";
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
            var trackInfo = manifest.GetByLanguage("en");

            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

            var captionPart = track
                .GetByTime(TimeSpan.FromSeconds(641))
                .GetPartByTime(TimeSpan.FromSeconds(0.65));

            // Assert
            captionPart.Text.Should().Be(" hard");
        }

        [Theory]
        [InlineData("WOxr2dmLHLo")]
        [InlineData("YltHGKX80Y8")]
        public async Task User_can_download_a_specific_closed_caption_track_from_a_video(string videoId)
        {
            // Arrange
            var filePath = _tempOutputFixture.GetTempFilePath();
            var youtube = new YoutubeClient();

            // Act
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);
            var trackInfo = manifest.Tracks.First();

            await youtube.Videos.ClosedCaptions.DownloadAsync(trackInfo, filePath);

            var fileInfo = new FileInfo(filePath);

            // Assert
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);
        }
    }
}