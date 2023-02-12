using System;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils;

internal static class Retry
{
    public static async ValueTask<T> WhileAsync<T>(
        Func<CancellationToken, Task<T>> getResultAsync,
        Func<T, bool> shouldRetry,
        int maxRetries = 5,
        CancellationToken cancellationToken = default)
    {
        var remainingRetries = maxRetries;

        while (true)
        {
            var result = await getResultAsync(cancellationToken);

            if (shouldRetry(result) &&
                remainingRetries-- > 0 &&
                !cancellationToken.IsCancellationRequested)
            {
                continue;
            }

            return result;
        }
    }

    public static async ValueTask<T?> WhileNullAsync<T>(
        Func<CancellationToken, Task<T?>> getResultAsync,
        int maxRetries = 5,
        CancellationToken cancellationToken = default) =>
        await WhileAsync(
            getResultAsync,
            r => r is null,
            maxRetries,
            cancellationToken
        );

    public static async ValueTask<T> WhileExceptionAsync<T>(
        Func<CancellationToken, Task<T>> getResultAsync,
        Func<Exception, bool> shouldRetry,
        int maxRetries = 5,
        CancellationToken cancellationToken = default)
    {
        var remainingRetries = maxRetries;

        while (true)
        {
            try
            {
                return await getResultAsync(cancellationToken);
            }
            catch (Exception ex) when (
                shouldRetry(ex) &&
                remainingRetries-- > 0 &&
                !cancellationToken.IsCancellationRequested)
            {
            }
        }
    }
}