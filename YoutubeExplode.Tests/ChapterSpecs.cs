using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace YoutubeExplode.Tests
{
    public class ChapterSpecs
    {
        [Fact]
        public async Task I_can_get_chapters_of_a_YouTube_video()
        {
            // Arrange
            const string videoUrl = "https://www.youtube.com/watch?v=QBEItnHiqKM";
            var youtube = new YoutubeClient();

            // Act
            var video = await youtube.Videos.GetAsync(videoUrl);

            // Assert
            video.Chapters.Count().Should().Be(11);
            video.Chapters![0].Title.Should().Be("Us vs Them mentality");
            video.Chapters![0].TimeRangeStart.Should().Be(0);    
            
            video.Chapters![5].Title.Should().Be("QUICK BITS");
            video.Chapters![5].TimeRangeStart.Should().Be(237000);

            video.Chapters![10].Title.Should().Be("Run homebrew DVD-R discs on PS2");
            video.Chapters![10].TimeRangeStart.Should().Be(342000);
        }
    }
}