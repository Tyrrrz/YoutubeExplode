using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace YoutubeExplode.Tests
{
    public class SearchSpecs
    {
        [Fact]
        public async Task User_can_search_for_videos()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Search.GetVideosAsync("billie eilish");

            // Assert
            videos.Should().HaveCountGreaterThan(200);
        }

        [Fact]
        public async Task User_can_search_for_YouTube_videos_using_a_query_that_contains_special_characters()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Search.GetVideosAsync("Kill la Kill Gomen ne, Iiko ja Irarenai.");

            // Assert
            videos.Should().HaveCountGreaterThan(300);
        }
    }
}