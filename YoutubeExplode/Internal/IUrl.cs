using System.Collections.Generic;

namespace YoutubeExplode.Internal
{
    public interface IUrl
    {
        string SetQueryParameter(string url, string key, string value);
        string SetRouteParameter(string url, string key, string value);
        Dictionary<string, string> SplitQuery(string query);
    }
}