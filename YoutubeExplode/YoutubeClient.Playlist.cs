using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoutubeExplode.Internal;
using YoutubeExplode.Models;
using YoutubeExplode.Services;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <summary>
        /// Gets playlist by ID, truncating resulting video list at given number of pages (1 page ≤ 200 videos)
        /// </summary>
        public async Task<Playlist> GetPlaylistAsync(string playlistId, int maxPages)
        {
            playlistId.GuardNotNull(nameof(playlistId));
            maxPages.GuardPositive(nameof(maxPages));
            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException("Invalid Youtube playlist ID", nameof(playlistId));

            // Get all videos across pages
            var pagesDone = 0;
            var offset = 0;
            XElement playlistXml;
            var videoIds = new HashSet<string>();
            var videos = new List<PlaylistVideo>();
            do
            {
                // Get manifest
                var request = $"{YoutubeHost}/list_ajax?style=xml&action_get_list=1&list={playlistId}&index={offset}";
                var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
                playlistXml = XElement.Parse(response).StripNamespaces();

                // Parse videos
                var total = 0;
                var delta = 0;
                foreach (var videoXml in playlistXml.Elements("video"))
                {
                    // Basic info
                    var videoId = videoXml.ElementStrict("encrypted_id").Value;
                    var videoTitle = videoXml.ElementStrict("title").Value;
                    var videoThumbnails = new VideoThumbnails(videoId);
                    var videoDuration = TimeSpan.FromSeconds((double) videoXml.ElementStrict("length_seconds"));
                    var videoDescription = videoXml.ElementStrict("description").Value;

                    // Keywords
                    var videoKeywordsJoined = videoXml.ElementStrict("keywords").Value;
                    var videoKeywords = Regex
                        .Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .Where(s => s.IsNotBlank())
                        .ToArray();

                    // Statistics
                    // The inner text is already formatted so we have to parse it manually
                    var videoViewCount =
                        Regex.Replace(videoXml.ElementStrict("views").Value, @"\D", "").ParseLong();
                    var videoLikeCount =
                        Regex.Replace(videoXml.ElementStrict("likes").Value, @"\D", "").ParseLong();
                    var videoDislikeCount =
                        Regex.Replace(videoXml.ElementStrict("dislikes").Value, @"\D", "").ParseLong();
                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);

                    // Video
                    var video = new PlaylistVideo(videoId, videoTitle, videoDescription, videoThumbnails, videoDuration,
                        videoKeywords, videoStatistics);

                    // Add to list if not already there
                    if (videoIds.Add(video.Id))
                    {
                        videos.Add(video);
                        delta++;
                    }
                    total++;
                }

                // Break if the videos started repeating
                if (delta <= 0)
                    break;

                // Prepare for next page
                pagesDone++;
                offset += total;
            } while (pagesDone < maxPages);

            // Basic info
            var title = playlistXml.ElementStrict("title").Value;
            var author = playlistXml.Element("author")?.Value ?? ""; // system playlists don't have an author
            var description = playlistXml.ElementStrict("description").Value;

            // Statistics
            var viewCount = (long?) playlistXml.Element("views") ?? 0; // watchlater does not have views
            var likeCount = (long?) playlistXml.Element("likes") ?? 0; // system playlists don't have likes
            var dislikeCount = (long?) playlistXml.Element("dislikes") ?? 0; // system playlists don't have dislikes
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, title, author, description, statistics, videos);
        }

        /// <summary>
        /// Gets playlist by ID
        /// </summary>
        public Task<Playlist> GetPlaylistAsync(string playlistId)
            => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}