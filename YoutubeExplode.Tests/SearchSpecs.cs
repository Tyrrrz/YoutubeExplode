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
            var videos = await youtube.Search.GetVideosAsync("undead corporation megalomania");

            // Assert
            videos.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_can_search_for_videos_and_retrieve_a_subset_of_results()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Search.GetVideosAsync("billie eilish", 2, 1);

            // Assert
            videos.Should().NotBeEmpty();
            videos.Should().HaveCountLessOrEqualTo(30);
        }

        [Fact]
        public async Task User_can_search_for_YouTube_videos_using_a_query_that_contains_special_characters()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Search.GetVideosAsync("Kill la Kill Gomen ne, Iiko ja Irarenai.");

            // Assert
            videos.Should().NotBeEmpty();
        }
    }
}