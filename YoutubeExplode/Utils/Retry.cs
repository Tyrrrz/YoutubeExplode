using System;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils;

internal static class Retry
{
    public static async ValueTask<T?> WhileNullAsync<T>(Func<Task<T?>> getResultAsync, int maxRetries = 5)
    {
        var remainingRetries = maxRetries;
        while (true)
        {
            var result = await getResultAsync();
            if (result is not null || remainingRetries-- <= 0)
                return result;
        }
    }
}