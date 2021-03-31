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

        [Theory]
        [InlineData("UC46807r_RiRjH8IU-h_DrDQ")]
        [InlineData("UCJ6td3C9QlPO9O_J5dF4ZzA")]
        [InlineData("UCiGm_E4ZwYSHV3bcW1pnSeQ")]
        public async Task User_can_get_metadata_of_any_available_channel(string channelId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var channel = await youtube.Channels.GetAsync(channelId);

            // Assert
            channel.Id.Value.Should().Be(channelId);
        }

        [Theory]
        [InlineData("TheTyrrr", "UCEnBXANsKmyj2r9xVyKoDiQ")]
        public async Task User_can_get_metadata_of_a_channel_by_user_name(string userName, string expectedChannelId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var channel = await youtube.Channels.GetByUserAsync(userName);

            // Assert
            channel.Id.Value.Should().Be(expectedChannelId);
        }

        [Theory]
        [InlineData("5NmxuoNyDss", "UCEnBXANsKmyj2r9xVyKoDiQ")]
        public async Task User_can_get_metadata_of_any_available_channel_by_video_ID(string videoId, string expectedChannelId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var channel = await youtube.Channels.GetByVideoAsync(videoId);

            // Assert
            channel.Id.Value.Should().Be(expectedChannelId);
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

        [Theory]
        [InlineData("UC46807r_RiRjH8IU-h_DrDQ")]
        [InlineData("UCJ6td3C9QlPO9O_J5dF4ZzA")]
        [InlineData("UCiGm_E4ZwYSHV3bcW1pnSeQ")]
        public async Task User_can_get_videos_uploaded_by_any_available_channel(string channelId)
        {
            // Arrange
            var youtube = new YoutubeClient();

            // Act
            var videos = await youtube.Channels.GetUploadsAsync(channelId);

            // Assert
            videos.Should().NotBeEmpty();
            videos.Select(v => v.ChannelId).Should().OnlyContain(i => i == channelId);
        }
    }
}