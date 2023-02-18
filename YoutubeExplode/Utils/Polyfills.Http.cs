// ReSharper disable CheckNamespace

#if !NET5_0_OR_GREATER
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

internal static class HttpPolyfills
{
    public static async Task<string> GetStringAsync(
        this HttpClient httpClient,
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
#endif