using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Search;

namespace YoutubeExplode.Tests
{
    public class SearchSpecs
    {
        [Fact]
        public async Task User_can_search_for_content()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var results = await youtube.Search.GetResultsAsync("billie eilish");

            // Assert
            results.Should().HaveCountGreaterThan(300);
        }

        [Fact]
        public async Task User_can_search_for_content_using_a_query_that_contains_special_characters()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var results = await youtube.Search.GetResultsAsync("Kill la Kill Gomen ne, Iiko ja Irarenai.");

            // Assert
            results.Should().HaveCountGreaterThan(300);
        }
    }
}