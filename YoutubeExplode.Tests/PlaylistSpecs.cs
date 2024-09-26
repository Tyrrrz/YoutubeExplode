using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Tests.TestData;

namespace YoutubeExplode.Tests;

public class PlaylistSpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task I_can_get_the_metadata_of_a_playlist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var playlist = await youtube.Playlists.GetAsync(PlaylistIds.Normal);

        // Assert
        playlist.Id.Value.Should().Be(PlaylistIds.Normal);
        playlist.Url.Should().NotBeNullOrWhiteSpace();
        playlist.Title.Should().Be("Analytics Academy - Digital Analytics Fundamentals");
        playlist.Author.Should().NotBeNull();
        playlist.Author?.ChannelId.Value.Should().Be("UCJ5UyIAa5nEGksjcdp43Ixw");
        playlist.Author?.ChannelUrl.Should().NotBeNullOrWhiteSpace();
        playlist.Author?.ChannelTitle.Should().Be("Google Analytics");
        playlist
            .Description.Should()
            .Contain("Digital Analytics Fundamentals course on Analytics Academy");
        playlist.Count.Should().Be(22);
        playlist.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_try_to_get_the_metadata_of_a_playlist_and_get_an_error_if_it_is_private()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<PlaylistUnavailableException>(
            async () => await youtube.Playlists.GetAsync(PlaylistIds.Private)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Fact]
    public async Task I_can_try_to_get_the_metadata_of_a_playlist_and_get_an_error_if_it_does_not_exist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act & assert
        var ex = await Assert.ThrowsAsync<PlaylistUnavailableException>(
            async () => await youtube.Playlists.GetAsync(PlaylistIds.NonExisting)
        );

        testOutput.WriteLine(ex.ToString());
    }

    [Theory]
    [InlineData(PlaylistIds.Normal)]
    [InlineData(PlaylistIds.MusicMix)]
    [InlineData(PlaylistIds.VideoMix)]
    [InlineData(PlaylistIds.MusicAlbum)]
    [InlineData(PlaylistIds.ContainsLongVideos)]
    [InlineData(PlaylistIds.Weird)]
    public async Task I_can_get_the_metadata_of_any_available_playlist(string playlistId)
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var playlist = await youtube.Playlists.GetAsync(playlistId);

        // Assert
        playlist.Id.Value.Should().Be(playlistId);
        playlist.Url.Should().NotBeNullOrWhiteSpace();
        playlist.Title.Should().NotBeNullOrWhiteSpace();
        playlist.Description.Should().NotBeNull();
        playlist.Count.Should().NotBe(0);
        playlist.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_videos_included_in_a_playlist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Playlists.GetVideosAsync(PlaylistIds.Normal);

        // Assert
        videos.Should().HaveCountGreaterOrEqualTo(21);
        videos
            .Select(v => v.Id.Value)
            .Should()
            .Contain(
                [
                    "uPZSSdkGQhM",
                    "fi0w57kr_jY",
                    "xLJt5A-NeQI",
                    "EpDA3XaELqs",
                    "eyltEFyZ678",
                    "TW3gx4t4944",
                    "w9H_P2wAwSE",
                    "OyixJ7A9phg",
                    "dzwRzUEc_tA",
                    "vEpq3nYeZBc",
                    "4gYioQkIqKk",
                    "xyh8iG5mRIs",
                    "ORrYEEH_KPc",
                    "ii0T5JUO2BY",
                    "hgycbw6Beuc",
                    "Dz-zgq6OqTI",
                    "I1b4GT-GuEs",
                    "dN3gkBBffhs",
                    "8Kg-8ZjgLAQ",
                    "E9zfpKsw6f8",
                    "eBCw9sC5D40",
                ]
            );
    }

    [Fact]
    public async Task I_can_get_videos_included_in_a_large_playlist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Playlists.GetVideosAsync(PlaylistIds.Large);

        // Assert
        videos.Should().HaveCountGreaterOrEqualTo(1900);
        videos
            .Select(v => v.Id.Value)
            .Should()
            .Contain(
                [
                    "RBumgq5yVrA",
                    "kN0iD0pI3o0",
                    "YqB8Dm65X18",
                    "jlvY1o6XKwA",
                    "-0kcet4aPpQ",
                    "RnGJ3KJri1g",
                    "x-IR7PtA7RA",
                    "N-8E9mHxDy0",
                    "5ly88Ju1N6A",
                ]
            );
    }

    [Theory]
    [InlineData(PlaylistIds.Normal)]
    [InlineData(PlaylistIds.MusicMix)]
    [InlineData(PlaylistIds.VideoMix)]
    [InlineData(PlaylistIds.MusicAlbum)]
    [InlineData(PlaylistIds.UserUploads)]
    [InlineData(PlaylistIds.ContainsLongVideos)]
    [InlineData(PlaylistIds.Weird)]
    public async Task I_can_get_videos_included_in_any_available_playlist(string playlistId)
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Playlists.GetVideosAsync(playlistId);

        // Assert
        videos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_a_subset_of_videos_included_in_a_playlist()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Playlists.GetVideosAsync(PlaylistIds.Large).CollectAsync(10);

        // Assert
        videos.Should().HaveCount(10);
    }
}
