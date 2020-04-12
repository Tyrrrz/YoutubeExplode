using System;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Internal
{
    internal static class Retry
    {
        private static int GetRetryCost(this Exception ex) => ex switch
        {
            TransientFailureException _ => 1,
            RequestLimitExceededException _ => 2,
            FatalFailureException _ => 3,
            _ => 100
        };

        public static async Task<T> WrapAsync<T>(Func<Task<T>> executeAsync)
        {
            // Most exceptions are retried, but some are retried more than others
            var retryResourceRemaining = 5;

            while (true)
            {
                try
                {
                    return await executeAsync();
                }
                catch (Exception ex)
                {
                    if ((retryResourceRemaining -= ex.GetRetryCost()) < 0)
                        throw;

                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                }
            }
        }
    }
}