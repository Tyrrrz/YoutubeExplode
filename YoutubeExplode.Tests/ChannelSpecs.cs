using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Common;
using YoutubeExplode.Tests.TestData;

namespace YoutubeExplode.Tests;

public class ChannelSpecs
{
    [Fact]
    public async Task I_can_get_metadata_of_a_channel()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var channel = await youtube.Channels.GetAsync(ChannelIds.Normal);

        // Assert
        channel.Id.Value.Should().Be(ChannelIds.Normal);
        channel.Url.Should().NotBeNullOrWhiteSpace();
        channel.Title.Should().Be("MrBeast");
        channel.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_metadata_of_a_channel_by_user_name()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var channel = await youtube.Channels.GetByUserAsync(UserNames.Normal);

        // Assert
        channel.Id.Value.Should().Be("UCX6OQ3DkcsbYNE6H8uQQuVA");
        channel.Url.Should().NotBeNullOrWhiteSpace();
        channel.Title.Should().Be("MrBeast");
        channel.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_metadata_of_a_channel_by_slug()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var channel = await youtube.Channels.GetBySlugAsync(ChannelSlugs.Normal);

        // Assert
        channel.Id.Value.Should().Be("UCSli-_XJrdRwRoPw8DXRiyw");
        channel.Url.Should().NotBeNullOrWhiteSpace();
        channel.Title.Should().Be("Меланія Подоляк");
        channel.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_metadata_of_a_channel_by_handle()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var channel = await youtube.Channels.GetByHandleAsync(ChannelHandles.Normal);

        // Assert
        channel.Id.Value.Should().Be("UCX6OQ3DkcsbYNE6H8uQQuVA");
        channel.Url.Should().NotBeNullOrWhiteSpace();
        channel.Title.Should().Be("MrBeast");
        channel.Thumbnails.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_videos_uploaded_by_a_channel()
    {
        // Arrange
        var youtube = new YoutubeClient();

        // Act
        var videos = await youtube.Channels.GetUploadsAsync(ChannelIds.Normal);

        // Assert
        videos.Should().HaveCountGreaterOrEqualTo(730);
        videos.Select(v => v.Author.ChannelId).Should().OnlyContain(i => i == ChannelIds.Normal);
    }
}