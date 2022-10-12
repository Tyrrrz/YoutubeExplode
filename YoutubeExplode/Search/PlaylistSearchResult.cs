using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Search;

/// <summary>
/// Metadata associated with a YouTube playlist returned by a search query.
/// </summary>
public class PlaylistSearchResult : ISearchResult, IPlaylist
{
    /// <inheritdoc />
    public PlaylistId Id { get; }

    /// <inheritdoc cref="IPlaylist.Url" />
    public string Url => $"https://www.youtube.com/playlist?list={Id}";

    /// <inheritdoc cref="IPlaylist.Title" />
    public string Title { get; }

    /// <inheritdoc />
    public Author? Author { get; }

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; }

    /// <summary>
    /// Initializes an instance of <see cref="PlaylistSearchResult" />.
    /// </summary>
    public PlaylistSearchResult(
        PlaylistId id,
        string title,
        Author? author,
        IReadOnlyList<Thumbnail> thumbnails)
    {
        Id = id;
        Title = title;
        Author = author;
        Thumbnails = thumbnails;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Playlist ({Title})";
}