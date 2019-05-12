using System.Collections.Generic;

namespace YoutubeExplode.Internal.CipherOperations
{
    internal static class Extensions
    {
        public static string Decipher(this IEnumerable<ICipherOperation> operations, string signature)
        {
            foreach (var operation in operations)
                signature = operation.Decipher(signature);

            return signature;
        }
    }
}