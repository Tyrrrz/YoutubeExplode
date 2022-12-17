using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Common;

namespace YoutubeExplode.Tests;

public class SearchSpecs
{
    [Fact]
    public async Task I_can_get_results_from_a_search_query()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var results = await youtube.Search.GetResultsAsync("undead corporation");

        // Assert
        results.Should().HaveCountGreaterOrEqualTo(50);
    }

    [Fact]
    public async Task I_can_get_video_results_from_a_search_query()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Search.GetVideosAsync("undead corporation");

        // Assert
        videos.Should().HaveCountGreaterOrEqualTo(50);
    }

    [Fact]
    public async Task I_can_get_playlist_results_from_a_search_query()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var playlists = await youtube.Search.GetPlaylistsAsync("undead corporation");

        // Assert
        playlists.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_channel_results_from_a_search_query()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var channels = await youtube.Search.GetChannelsAsync("undead corporation");

        // Assert
        channels.Should().NotBeEmpty();
    }
}