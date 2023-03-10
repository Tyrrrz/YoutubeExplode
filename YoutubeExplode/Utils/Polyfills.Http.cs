// ReSharper disable CheckNamespace

#if !NET5_0_OR_GREATER
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

internal static class HttpPolyfills
{
    public static async Task<Stream> ReadAsStreamAsync(
        this HttpContent httpContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await httpContent.ReadAsStreamAsync();
    }

    public static async Task<string> ReadAsStringAsync(
        this HttpContent httpContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await httpContent.ReadAsStringAsync();
    }

    public static async Task<string> GetStringAsync(
        this HttpClient httpClient,
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static async Task<Stream> GetStreamAsync(
        this HttpClient httpClient,
        string requestUri,
        CancellationToken cancellationToken)
    {
        // Must not be disposed for the stream to be usable
        var response = await httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
#endif