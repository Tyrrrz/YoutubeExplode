using System;

namespace YoutubeExplode.Utils.Extensions;

internal static class UriExtensions
{
    extension(Uri uri)
    {
        public string Domain => uri.Scheme + Uri.SchemeDelimiter + uri.Host;
    }
}
