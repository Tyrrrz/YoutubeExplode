using System;
using System.Threading.Tasks;
using Polly;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Internal
{
    internal static class Retry
    {
        private static AsyncPolicy ExecutionPolicy { get; } = Policy.WrapAsync(
            Policy.Handle<TransientFailureException>().WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 0.5)),
            Policy.Handle<RequestLimitExceededException>().WaitAndRetryAsync(2, i => TimeSpan.FromSeconds(i * 0.5))
        );

        public static Task<T> WrapAsync<T>(Func<Task<T>> executeAsync) => ExecutionPolicy.ExecuteAsync(executeAsync);
    }
}