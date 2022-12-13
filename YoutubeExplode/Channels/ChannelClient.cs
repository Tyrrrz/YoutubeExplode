using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Playlists;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Channels;

/// <summary>
/// Operations related to YouTube channels.
/// </summary>
public class ChannelClient
{
    private readonly HttpClient _http;
    private readonly ChannelController _controller;

    /// <summary>
    /// Initializes an instance of <see cref="ChannelClient" />.
    /// </summary>
    public ChannelClient(HttpClient http)
    {
        _http = http;
        _controller = new ChannelController(http);
    }

    private Channel Extract(ChannelPageExtractor channelPage)
    {
        var channelId =
            channelPage.TryGetChannelId() ??
            throw new YoutubeExplodeException("Could not extract channel ID.");

        var title =
            channelPage.TryGetChannelTitle() ??
            throw new YoutubeExplodeException("Could not extract channel title.");

        var logoUrl =
            channelPage.TryGetChannelLogoUrl() ??
            throw new YoutubeExplodeException("Could not extract channel logo URL.");

        var logoSize = Regex
            .Matches(logoUrl, @"\bs(\d+)\b")
            .ToArray()
            .LastOrDefault()?
            .Groups[1]
            .Value
            .NullIfWhiteSpace()?
            .ParseIntOrNull() ?? 100;

        var thumbnails = new[] { new Thumbnail(logoUrl, new Resolution(logoSize, logoSize)) };

        return new Channel(channelId, title, thumbnails);
    }

    /// <summary>
    /// Gets the metadata associated with the specified channel.
    /// </summary>
    public async ValueTask<Channel> GetAsync(
        ChannelId channelId,
        CancellationToken cancellationToken = default) =>
        Extract(await _controller.GetChannelPageAsync(channelId, cancellationToken));

    /// <summary>
    /// Gets the metadata associated with the channel of the specified user.
    /// </summary>
    public async ValueTask<Channel> GetByUserAsync(
        UserName userName,
        CancellationToken cancellationToken = default) =>
        Extract(await _controller.GetChannelPageAsync(userName, cancellationToken));

    /// <summary>
    /// Gets the metadata associated with the channel identified by the specified slug or custom URL.
    /// </summary>
    public async ValueTask<Channel> GetBySlugAsync(
        ChannelSlug channelSlug,
        CancellationToken cancellationToken = default) =>
        Extract(await _controller.GetChannelPageAsync(channelSlug, cancellationToken));

    /// <summary>
    /// Gets the metadata associated with the channel identified by the specified handle or handle URL.
    /// </summary>
    public async ValueTask<Channel> GetByHandleAsync(
        ChannelHandle channelHandle,
        CancellationToken cancellationToken = default) =>
        Extract(await _controller.GetChannelPageAsync(channelHandle, cancellationToken));

    /// <summary>
    /// Enumerates videos uploaded by the specified channel.
    /// </summary>
    // TODO: should return <IVideo> sequence instead (breaking change)
    public IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(
        ChannelId channelId,
        CancellationToken cancellationToken = default)
    {
        // Replace 'UC' in channel ID with 'UU'
        var playlistId = "UU" + channelId.Value[2..];
        return new PlaylistClient(_http).GetVideosAsync(playlistId, cancellationToken);
    }
}