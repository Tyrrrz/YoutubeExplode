using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Metadata associated with a YouTube playlist.
/// </summary>
public class Playlist(
    PlaylistId id,
    string title,
    Author? author,
    string description,
    IReadOnlyList<Thumbnail> thumbnails
) : IPlaylist
{
    /// <inheritdoc />
    public PlaylistId Id { get; } = id;

    /// <inheritdoc />
    public string Url => $"https://www.youtube.com/playlist?list={Id}";

    /// <inheritdoc />
    public string Title { get; } = title;

    /// <inheritdoc />
    public Author? Author { get; } = author;

    /// <summary>
    /// Playlist description.
    /// </summary>
    public string Description { get; } = description;

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; } = thumbnails;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Playlist ({Title})";
}
