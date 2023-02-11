using System;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils;

internal static class Retry
{
    public static async ValueTask<T?> WhileNullAsync<T>(
        Func<CancellationToken, Task<T?>> getResultAsync,
        int maxRetries = 5,
        CancellationToken cancellationToken = default)
    {
        var remainingRetries = maxRetries;

        while (true)
        {
            var result = await getResultAsync(cancellationToken);
            if (result is not null || remainingRetries-- <= 0 || cancellationToken.IsCancellationRequested)
                return result;
        }
    }
}