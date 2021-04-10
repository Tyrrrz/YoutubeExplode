using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace YoutubeExplode.Tests
{
    public class ChannelSpecs
    {
        [Fact]
        public async Task User_can_get_metadata_of_a_channel()
        {
            // Arrange
            const string channelUrl = "https://www.youtube.com/channel/UCEnBXANsKmyj2r9xVyKoDiQ";
            var youtube = new YoutubeClient();

            // Act
            var channel = await youtube.Channels.GetAsync(channelUrl);

            // Assert
            channel.Id.Value.Should().Be("UCEnBXANsKmyj2r9xVyKoDiQ");
            channel.Url.Should().Be(channelUrl);
            channel.Title.Should().Be("Tyrrrz");
            channel.LogoUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task User_can_get_metadata_of_a_channel_by_user_name()
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var channel = await youtube.Channels.GetByUserAsync("TheTyrrr");

            // Assert
            channel.Id.Value.Should().Be("UCEnBXANsKmyj2r9xVyKoDiQ");
        }

        [Fact]
        public async Task User_can_get_videos_uploaded_by_a_channel()
        {
            // Arrange
            const string channelUrl = "https://www.youtube.com/channel/UCEnBXANsKmyj2r9xVyKoDiQ";
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Channels.GetUploadsAsync(channelUrl);

            // Assert
            videos.Should().HaveCountGreaterOrEqualTo(80);
            videos.Select(v => v.ChannelId).Should().OnlyContain(i => i == "UCEnBXANsKmyj2r9xVyKoDiQ");
        }
    }
}