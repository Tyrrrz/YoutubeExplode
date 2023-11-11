using System;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Tests;

public class PlaylistIdSpecs
{
    [Theory]
    [InlineData("WL")]
    [InlineData("LL")]
    [InlineData("RDMM")]
    [InlineData("PL601B2E69B03FAB9D")]
    [InlineData("PLI5YfMzCfRtZ8eV576YoY3vIYrHjyVm_e")]
    [InlineData("PLWwAypAcFRgKFlxtLbn_u14zddtDJj3mk")]
    [InlineData("OLAK5uy_mtOdjCW76nDvf5yOzgcAVMYpJ5gcW5uKU")]
    [InlineData("RD1hu8-y6fKg0")]
    [InlineData("RDMMU-ty-2B02VY")]
    [InlineData("RDCLAK5uy_lf8okgl2ygD075nhnJVjlfhwp8NsUgEbs")]
    [InlineData("ULl6WWX-BgIiE")]
    [InlineData("UUTMt7iMWa7jy0fNXIktwyLA")]
    [InlineData("OLAK5uy_lLeonUugocG5J0EUAEDmbskX4emejKwcM")]
    [InlineData("FLEnBXANsKmyj2r9xVyKoDiQ")]
    public void I_can_parse_a_playlist_ID_from_an_ID_string(string playlistId)
    {
        // Act
        var parsed = PlaylistId.Parse(playlistId);

        // Assert
        parsed.Value.Should().Be(playlistId);
    }

    [Theory]
    [InlineData(
        "youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H",
        "PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H"
    )]
    [InlineData(
        "youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr",
        "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
    )]
    [InlineData(
        "youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr",
        "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
    )]
    [InlineData(
        "youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr",
        "PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr"
    )]
    [InlineData(
        "youtube.com/watch?v=x2ZRoWQ0grU&list=RDEMNJhLy4rECJ_fG8NL-joqsg",
        "RDEMNJhLy4rECJ_fG8NL-joqsg"
    )]
    public void I_can_parse_a_playlist_ID_from_a_URL_string(
        string playlistUrl,
        string expectedPlaylistId
    )
    {
        // Act
        var parsed = PlaylistId.Parse(playlistUrl);

        // Assert
        parsed.Value.Should().Be(expectedPlaylistId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("PLm_3vnTS-pvmZFuF L1Pyhqf8kTTYVKjW")]
    [InlineData("PLm_3vnTS-pvmZFuF3L=Pyhqf8kTTYVKjW")]
    [InlineData("youtube.com/playlist?lisp=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H")]
    [InlineData("youtube.com/")]
    public void I_can_try_to_parse_a_playlist_ID_and_get_an_error_if_the_input_string_is_invalid(
        string playlistIdOrUrl
    )
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => PlaylistId.Parse(playlistIdOrUrl));
    }
}
