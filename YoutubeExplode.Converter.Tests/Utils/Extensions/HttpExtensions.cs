using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Converter.Tests.Utils.Extensions;

internal static class HttpExtensions
{
    extension(HttpClient http)
    {
        public async Task DownloadAsync(
            string url,
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            using var response = await http.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = File.Create(filePath);

            await source.CopyToAsync(destination, cancellationToken);
        }
    }
}
