using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams.Resolving.Cipher
{
    internal interface ICipherOperation
    {
        string Decipher(string input);
    }

    internal static class CipherOperationExtensions
    {
        public static string Decipher(this IEnumerable<ICipherOperation> operations, string input) =>
            operations.Aggregate(input, (acc, op) => op.Decipher(acc));
    }
}