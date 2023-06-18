using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace YoutubeExplode.Converter.Tests.Utils.Extensions;

internal static class HttpExtensions
{
    public static async Task DownloadAsync(this HttpClient httpClient, string url, string filePath)
    {
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(filePath);

        await source.CopyToAsync(destination);
    }
}