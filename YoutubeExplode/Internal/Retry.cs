using System;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Internal
{
    internal static class Retry
    {
        public static async Task<T> WrapAsync<T>(Func<Task<T>> executeAsync)
        {
            const int maxRetryCount = 5;
            var retryDelay = TimeSpan.FromSeconds(0.5);

            var currentRetry = 0;

            while (true)
            {
                try
                {
                    return await executeAsync();
                }
                catch (Exception ex) when (ex is TransientFailureException || ex is RequestLimitExceededException)
                {
                    if (++currentRetry > maxRetryCount)
                        throw;

                    await Task.Delay(retryDelay);
                }
            }
        }
    }
}