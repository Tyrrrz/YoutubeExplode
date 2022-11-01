using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Channels;

internal class ChannelController : YoutubeControllerBase
{
    public ChannelController(HttpClient http)
        : base(http)
    {
    }

    private async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
        string channelRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://www.youtube.com/{channelRoute}?hl=en";

        for (var retry = 0; retry <= 5; retry++)
        {
            var raw = await SendHttpRequestAsync(url, cancellationToken);

            var channelPage = ChannelPageExtractor.TryCreate(raw);
            if (channelPage is not null)
                return channelPage;
        }

        throw new YoutubeExplodeException(
            "Channel page is broken. " +
            "Please try again in a few minutes."
        );
    }

    public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
        ChannelId channelId,
        CancellationToken cancellationToken = default) =>
        await GetChannelPageAsync("channel/" + channelId, cancellationToken);

    public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
        UserName userName,
        CancellationToken cancellationToken = default) =>
        await GetChannelPageAsync("user/" + userName, cancellationToken);

    public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
        ChannelSlug channelSlug,
        CancellationToken cancellationToken = default) =>
        await GetChannelPageAsync("c/" + channelSlug, cancellationToken);

    public async ValueTask<ChannelPageExtractor> GetChannelPageAsync(
        ChannelHandle channelHandle,
        CancellationToken cancellationToken = default) =>
        await GetChannelPageAsync("@" + channelHandle, cancellationToken);
}