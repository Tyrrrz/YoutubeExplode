﻿using System;
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
        private async Task<string> GetPlaylistInfoRawAsync(string playlistId, int index = 0)
        {
            var url = $"https://www.youtube.com/list_ajax?style=xml&action_get_list=1&list={playlistId}&index={index}";
            return await _httpService.GetStringAsync(url).ConfigureAwait(false);
        }

        private async Task<XElement> GetPlaylistInfoAsync(string playlistId, int index = 0)
        {
            var raw = await GetPlaylistInfoRawAsync(playlistId, index).ConfigureAwait(false);
            return XElement.Parse(raw).StripNamespaces();
        }

        /// <summary>
        /// Gets playlist information by ID.
        /// The video list is truncated at given number of pages (1 page ≤ 200 videos).
        /// </summary>
        public async Task<Playlist> GetPlaylistAsync(string playlistId, int maxPages)
        {
            playlistId.GuardNotNull(nameof(playlistId));
            maxPages.GuardPositive(nameof(maxPages));
            if (!ValidatePlaylistId(playlistId))
                throw new ArgumentException($"Invalid YouTube playlist ID [{playlistId}].", nameof(playlistId));

            // Get all videos across pages
            var pagesDone = 0;
            var offset = 0;
            XElement playlistXml;
            var videoIds = new HashSet<string>();
            var videos = new List<Video>();
            do
            {
                // Get playlist info
                playlistXml = await GetPlaylistInfoAsync(playlistId, offset).ConfigureAwait(false);

                // Parse videos
                var total = 0;
                var delta = 0;
                foreach (var videoXml in playlistXml.Elements("video"))
                {
                    // Basic info
                    var videoId = (string) videoXml.Element("encrypted_id");
                    var videoAuthor = (string) videoXml.Element("author");
                    var videoTitle = (string) videoXml.Element("title");
                    var videoDuration = TimeSpan.FromSeconds((double) videoXml.Element("length_seconds"));
                    var videoDescription = (string) videoXml.Element("description");

                    // Keywords
                    var videoKeywordsJoined = (string) videoXml.Element("keywords");
                    var videoKeywords = Regex
                        .Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .Where(s => s.IsNotBlank())
                        .ToArray();

                    // Statistics
                    var videoViewCount = ((string) videoXml.Element("views")).StripNonDigit().ParseLong();
                    var videoLikeCount = ((string) videoXml.Element("likes")).StripNonDigit().ParseLong();
                    var videoDislikeCount = ((string) videoXml.Element("dislikes")).StripNonDigit().ParseLong();
                    var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);

                    // Video
                    var videoThumbnails = new ThumbnailSet(videoId);
                    var video = new Video(videoId, videoAuthor, videoTitle, videoDescription, videoThumbnails,
                        videoDuration, videoKeywords, videoStatistics);

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

            // Extract playlist info
            var title = (string) playlistXml.Element("title");
            var author = (string) playlistXml.Element("author") ?? ""; // system playlists don't have an author
            var description = (string) playlistXml.Element("description");

            // Statistics
            var viewCount = (long?) playlistXml.Element("views") ?? 0; // watchlater does not have views
            var likeCount = (long?) playlistXml.Element("likes") ?? 0; // system playlists don't have likes
            var dislikeCount = (long?) playlistXml.Element("dislikes") ?? 0; // system playlists don't have dislikes
            var statistics = new Statistics(viewCount, likeCount, dislikeCount);

            return new Playlist(playlistId, author, title, description, statistics, videos);
        }

        /// <summary>
        /// Gets playlist information by ID.
        /// </summary>
        public Task<Playlist> GetPlaylistAsync(string playlistId)
            => GetPlaylistAsync(playlistId, int.MaxValue);
    }
}