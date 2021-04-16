using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Common;

namespace YoutubeExplode.Tests
{
    public class SearchSpecs
    {
        [Fact]
        public async Task User_can_execute_a_search_query()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var results = await youtube.Search.GetResultsAsync("undead corporation");

            // Assert
            results.Should().HaveCountGreaterOrEqualTo(100);
        }

        [Fact]
        public async Task User_can_execute_a_search_query_and_filter_only_videos()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Search.GetVideosAsync("undead corporation");

            // Assert
            videos.Should().HaveCountGreaterOrEqualTo(100);

            foreach (var video in videos)
            {
                video.Id.Value.Should().NotBeNullOrWhiteSpace();
                video.Title.Should().NotBeNullOrWhiteSpace();
                video.Url.Should().NotBeNullOrWhiteSpace();
                video.Author.ChannelId.Value.Should().NotBeNullOrWhiteSpace();
                video.Author.Title.Should().NotBeNullOrWhiteSpace();
                video.Thumbnails.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task User_can_execute_a_search_query_and_filter_only_playlists()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var playlists = await youtube.Search.GetPlaylistsAsync("undead corporation");

            // Assert
            playlists.Should().HaveCountGreaterOrEqualTo(10);

            foreach (var playlist in playlists)
            {
                playlist.Id.Value.Should().NotBeNullOrWhiteSpace();
                playlist.Title.Should().NotBeNullOrWhiteSpace();
                playlist.Url.Should().NotBeNullOrWhiteSpace();
                playlist.Author?.ChannelId.Value.Should().NotBeNullOrWhiteSpace();
                playlist.Author?.Title.Should().NotBeNullOrWhiteSpace();
                playlist.Thumbnails.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task User_can_execute_a_search_query_and_filter_only_channels()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var channels = await youtube.Search.GetChannelsAsync("undead corporation");

            // Assert
            channels.Should().HaveCountGreaterOrEqualTo(5);

            foreach (var channel in channels)
            {
                channel.Id.Value.Should().NotBeNullOrWhiteSpace();
                channel.Title.Should().NotBeNullOrWhiteSpace();
                channel.Url.Should().NotBeNullOrWhiteSpace();
                channel.Thumbnails.Should().NotBeEmpty();
            }
        }
    }
}