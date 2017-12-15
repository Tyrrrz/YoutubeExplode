using System.Collections.Generic;
using YoutubeExplode.Internal;
using YoutubeExplode.Services;

namespace YoutubeExplode
{
    /// <summary>
    /// The entry point for <see cref="YoutubeExplode"/>.
    /// </summary>
    public partial class YoutubeClient
    {
        private readonly IHttpService _httpService;
        private readonly Dictionary<string, PlayerSource> _playerSourceCache;

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with custom services.
        /// </summary>
        public YoutubeClient(IHttpService httpService)
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
            _playerSourceCache = new Dictionary<string, PlayerSource>();
        }

        /// <summary>
        /// Creates an instance of <see cref="YoutubeClient"/> with default services.
        /// </summary>
        public YoutubeClient()
            : this(HttpService.Instance)
        {
        }
    }
}