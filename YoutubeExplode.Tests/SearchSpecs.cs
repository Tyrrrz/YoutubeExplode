using System;
using System.Linq;
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
        using var youtube = new YoutubeClient();

        // Act
        var results = await youtube.Search.GetResultsAsync("undead corporation");

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(50);
        results
            .Should()
            .Contain(r =>
                r.Title.Contains("undead corporation", StringComparison.OrdinalIgnoreCase)
            );
    }

    [Fact]
    public async Task I_can_get_results_from_a_search_query_that_contains_special_characters()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        // Act
        var results = await youtube.Search.GetResultsAsync("\"dune 2\" ending");

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(50);
        results.Should().Contain(r => r.Title.Contains("dune", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task I_can_get_results_from_a_search_query_that_contains_non_ascii_characters()
    {
        // https://github.com/Tyrrrz/YoutubeExplode/issues/787

        // Arrange
        using var youtube = new YoutubeClient();

        // Act
        var results = await youtube.Search.GetResultsAsync("נועה קירל");

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(50);
        results
            .Should()
            .Contain(r => r.Title.Contains("נועה קירל", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task I_can_get_results_from_a_search_query_that_contains_non_ascii_characters_and_special_characters()
    {
        // https://github.com/Tyrrrz/YoutubeExplode/issues/787

        // Arrange
        using var youtube = new YoutubeClient();

        // Act
        var results = await youtube.Search.GetResultsAsync("\"נועה קירל\"");

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(50);
        results
            .Should()
            .Contain(r => r.Title.Contains("נועה קירל", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task I_can_get_video_results_from_a_search_query()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Search.GetVideosAsync("undead corporation");

        // Assert
        videos.Should().HaveCountGreaterThanOrEqualTo(50);
    }

    [Fact]
    public async Task I_can_get_playlist_results_from_a_search_query()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        // Act
        var playlists = await youtube.Search.GetPlaylistsAsync("undead corporation");

        // Assert
        playlists.Should().NotBeEmpty();

        var last = playlists.Last();

        last.Title.Should().NotBeNullOrWhiteSpace();
        last.Author.Should().NotBeNull();
        last.Thumbnails.Should().NotBeEmpty();

        var lastThumb = last.Thumbnails.Last();
        lastThumb.Url.Should().NotBeNullOrWhiteSpace();
        lastThumb.Resolution.Should().NotBeSameAs(default(Resolution));
    }

    [Fact]
    public async Task I_can_get_channel_results_from_a_search_query()
    {
        // Arrange
        using var youtube = new YoutubeClient();

        // Act
        var channels = await youtube.Search.GetChannelsAsync("undead corporation");

        // Assert
        channels.Should().NotBeEmpty();
    }
}
