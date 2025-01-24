using System;
using System.IO;
using System.Linq;
using System.Text;

namespace YoutubeExplode.Utils
{
    internal static class YoutubeParsingHelper
    {
        private static string CONTENT_PLAYBACK_NONCE_ALPHABET =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        public static string GetRandomVisitorData()
        {
            Random r = new Random();
            ProtoBuilder pbE2 = new ProtoBuilder();
            pbE2.String(2, "");
            pbE2.Varint(4, r.Next(255) + 1);

            ProtoBuilder pbE = new ProtoBuilder();
            pbE.String(1, "US");
            pbE.Bytes(2, pbE2.ToBytes());

            ProtoBuilder pb = new ProtoBuilder();
            pb.String(1, GenerateRandomStringFromAlphabet(CONTENT_PLAYBACK_NONCE_ALPHABET, 11, r));
            pb.Varint(5, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - r.Next(600000));
            pb.Bytes(6, pbE.ToBytes());
            return pb.ToUrlencodedBase64();
        }

        private static string GenerateRandomStringFromAlphabet(
            string alphabet,
            int length,
            Random random
        )
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(alphabet.ElementAt(random.Next(alphabet.Length)));
            }
            return sb.ToString();
        }
    }
}
